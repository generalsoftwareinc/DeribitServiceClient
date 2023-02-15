using ConsoleApp.DTOs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ServiceClient.Abstractions;
using Spectre.Console;
using System.Collections.ObjectModel;

namespace ConsoleApp.Pipelines;

internal class SpectreOutputPipeline : Pipeline
{
    private readonly OutputOptions outputOptions;
    private readonly ObservableCollection<string> lastEvents = new();
    private readonly Table table;
    private readonly LiveDisplay liveTable;

    public SpectreOutputPipeline(IServiceClient client, IOptions<OutputOptions> options, Table table, LiveDisplay liveTable) : base(client)
    {
        outputOptions = options.Value;
        this.table = table;
        this.liveTable = liveTable;
    }

    public override Task RunAsync(CancellationToken cancellationToken)
    {
        return liveTable.StartAsync( (ctx) => {
            lastEvents.CollectionChanged += (s, e) =>
            {
                UpdateTableItems();
                ctx.Refresh();
            };
            return base.RunAsync(cancellationToken);
        });
    }

    private void AddTableHeaders()
    {
        table.AddColumn("Number");
        table.AddColumn("Name");
        table.AddColumn("Last Name");
    }

    private void UpdateTableItems()
    {
        table.Rows.Clear();

        for(var i = 0; i < lastEvents.Count; i++)
        {
            table.AddRow(
                new Markup($"[bold]{i}[/]"),
                new Markup($"[green]{lastEvents[i]}[/]")
                );
        }
    }

    protected override void Client_OnTickerReceived(object? sender, EventArgs e)
    {
        if (lastEvents.Count >= outputOptions.ConsoleAmountOfEvents)
        {
            lastEvents.RemoveAt(0);
        }
        lastEvents.Add($"New [bold red]event[/] {DateTime.Now.ToLocalTime()}");
    }

    protected override void WritePipelineStep(string stepInfo)
    {
        AnsiConsole.MarkupLine($"LOG: [bold red]{stepInfo}[/]");
    }

    protected override void PreInitializeHook()
    {
        base.PreInitializeHook();
        AddTableHeaders();
    }
}
