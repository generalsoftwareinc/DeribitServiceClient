using ConsoleApp.DTOs;
using Microsoft.Extensions.Options;
using ServiceClient;
using ServiceClient.Abstractions;
using ServiceClient.DTOs;
using ServiceClient.Implements;
using Spectre.Console;
using System.Collections.ObjectModel;

namespace ConsoleApp.Pipelines;

internal class SpectreOutputPipeline : Pipeline
{
    private readonly OutputOptions outputOptions;
    private readonly ObservableCollection<Ticker> lastEvents = new();
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
        return liveTable.StartAsync((ctx) =>
        {
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
        table.AddColumn("Instrument Name");
        table.AddColumn("State");
        table.AddColumn("Min Price");
        table.AddColumn("Max Price");
        table.AddColumn("Best Bid Price");
        table.AddColumn("Best Ask Price");
        table.AddColumn("Book Asks Changed");
        table.AddColumn("Book Bids Changed");
    }

    private void UpdateTableItems()
    {
        table.Rows.Clear();

        var converter = new Func<BidAskParameter, string>((x) => $"[bold]Change:[/] {x.BidAskAction}, [bold red]Price:[/] {x.Price}, [bold]Amount:[/] {x.Amount}");
        for (var i = 0; i < lastEvents.Count; i++)
        {
            var ev = lastEvents[i];
            var asksAsString = string.Join("; ", ev.LastBook.Asks.Select(converter));
            var bidsAsString = string.Join("; ", ev.LastBook.Bids.Select(converter));

            table.AddRow(
                new Markup($"[bold green]{ev.InstrumentName}[/]"),
                new Markup($"[bold red]{ev.State}[/]"),
                new Markup($"{ev.MinPrice}"),
                new Markup($"{ev.MaxPrice}"),
                new Markup($"{ev.BestBidPrice}"),
                new Markup($"{ev.BestAskPrice}"),
                new Markup(asksAsString),
                new Markup(bidsAsString)
                );
        }
    }

    protected override void Client_OnTickerReceived(object? sender, TickerReceivedEventArgs e)
    {
        if (lastEvents.Count >= outputOptions.ConsoleAmountOfEvents)
        {
            lastEvents.RemoveAt(0);
        }
        lastEvents.Add(e.Ticker);
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
