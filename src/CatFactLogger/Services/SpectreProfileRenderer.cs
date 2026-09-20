using CatFactLogger.Models;
using Spectre.Console;

namespace CatFactLogger.Services;

/// <summary>
/// Renders the profile/CV as panels, a table and a tree using Spectre.Console.
/// Separated from JsonProfileProvider so the data source and the presentation
/// can change independently (and so this is unit-testable if needed later).
/// </summary>
public sealed class SpectreProfileRenderer : IProfileRenderer
{
    public void Render(ProfileModel profile)
    {
        AnsiConsole.Write(new FigletText(profile.FullName).Color(Color.Cyan1));
        AnsiConsole.MarkupLine($"[bold]{Markup.Escape(profile.Headline)}[/]");
        AnsiConsole.WriteLine();

        AnsiConsole.Write(new Panel(Markup.Escape(profile.About))
        {
            Header = new PanelHeader("About"),
            Border = BoxBorder.Rounded
        });

        AnsiConsole.Write(new Panel(BuildContactText(profile.Contact))
        {
            Header = new PanelHeader("Contact"),
            Border = BoxBorder.Rounded
        });

        AnsiConsole.Write(BuildSkillsTable(profile.Skills));
        AnsiConsole.Write(BuildExperienceTree(profile.Experience));

        if (profile.Education.Count > 0)
        {
            AnsiConsole.Write(new Panel(string.Join(Environment.NewLine, profile.Education.Select(Markup.Escape)))
            {
                Header = new PanelHeader("Education"),
                Border = BoxBorder.Rounded
            });
        }
    }

    private static string BuildContactText(ContactInfo contact)
    {
        var lines = new List<string> { $"Email: {Markup.Escape(contact.Email)}" };

        if (!string.IsNullOrWhiteSpace(contact.GitHub))
        {
            lines.Add($"GitHub: {Markup.Escape(contact.GitHub)}");
        }

        if (!string.IsNullOrWhiteSpace(contact.LinkedIn))
        {
            lines.Add($"LinkedIn: {Markup.Escape(contact.LinkedIn)}");
        }

        if (!string.IsNullOrWhiteSpace(contact.Phone))
        {
            lines.Add($"Phone: {Markup.Escape(contact.Phone)}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static Table BuildSkillsTable(List<SkillEntry> skills)
    {
        var table = new Table().Border(TableBorder.Rounded).Title("Skills");
        table.AddColumn("Skill");
        table.AddColumn("Level");
        table.AddColumn("Experience");

        foreach (var skill in skills)
        {
            table.AddRow(
                Markup.Escape(skill.Name),
                Markup.Escape(skill.Level),
                Markup.Escape(skill.YearsUsed ?? "-"));
        }

        return table;
    }

    private static Tree BuildExperienceTree(List<ExperienceEntry> experience)
    {
        var tree = new Tree("Experience");

        foreach (var job in experience)
        {
            var node = tree.AddNode(
                $"[bold]{Markup.Escape(job.Role)}[/] @ {Markup.Escape(job.Company)} ({Markup.Escape(job.Period)})");

            foreach (var highlight in job.Highlights)
            {
                node.AddNode(Markup.Escape(highlight));
            }
        }

        return tree;
    }
}
