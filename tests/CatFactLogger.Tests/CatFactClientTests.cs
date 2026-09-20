using System.Net;
using CatFactLogger.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CatFactLogger.Tests;

public class CatFactClientTests
{
    [Fact]
    public async Task GetFactAsync_ReturnsFact_WhenResponseIsSuccessful()
    {
        const string json = """{"fact":"Cats sleep a lot.","length":18}""";
        using var httpClient = CreateHttpClient(HttpStatusCode.OK, json);
        var sut = new CatFactClient(httpClient, NullLogger<CatFactClient>.Instance);

        var result = await sut.GetFactAsync();

        Assert.NotNull(result);
        Assert.Equal("Cats sleep a lot.", result!.Fact);
        Assert.Equal(18, result.Length);
    }

    [Fact]
    public async Task GetFactAsync_ReturnsNull_WhenApiReturnsErrorStatus()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.InternalServerError, string.Empty);
        var sut = new CatFactClient(httpClient, NullLogger<CatFactClient>.Instance);

        var result = await sut.GetFactAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task GetFactAsync_ReturnsNull_WhenResponseIsNotValidJson()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.OK, "not json");
        var sut = new CatFactClient(httpClient, NullLogger<CatFactClient>.Instance);

        var result = await sut.GetFactAsync();

        Assert.Null(result);
    }

    private static HttpClient CreateHttpClient(HttpStatusCode statusCode, string content)
    {
        var handler = new FakeHttpMessageHandler(statusCode, content);
        return new HttpClient(handler) { BaseAddress = new Uri("https://catfact.ninja/fact") };
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        public FakeHttpMessageHandler(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            };
            return Task.FromResult(response);
        }
    }
}
