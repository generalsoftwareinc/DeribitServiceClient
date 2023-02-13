using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Spectre.Console;

namespace ConsoleApp.Commands;

internal class AsyncSpectreCommand : AsyncCommand<AsyncSpectreCommand.Settings>
{
    private readonly Pipeline _pipeline;

    public AsyncSpectreCommand(Pipeline pipeline)
    {
        _pipeline= pipeline;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var table = new Table().Centered();
        await AnsiConsole.Live(table)
            .StartAsync(async ctx =>
            {
                var num = 1;
                table.AddColumn("Number");
                table.AddColumn("Name");
                table.AddColumn("Last Name");
                ctx.Refresh();
                await Task.Delay(1000);
                while (true)
                {
                    table.Rows.Clear();
                    for (var i = 0; i < settings.RowsCount; i++)
                    {
                        table.AddRow(new Markup($"[bold]{num++}[/]"), new Markup("[green]Test[/]"), new Markup("[red]Test[/]"));
                        ctx.Refresh();
                    }
                    await Task.Delay(1000);
                }
            });
        return 0;
    }

    public sealed class Settings : CommandSettings
    {
        [CommandOption ("-s|--show")]
        public int? RowsCount { get; set; } = 10;
    }
}
