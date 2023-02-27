using ConsoleApp.DTOs;
using Deribit.ServiceClient.DTOs.Book;
using Deribit.ServiceClient.DTOs.Ticker;
using Microsoft.Extensions.Options;
using Deribit.ServiceClient;
using Deribit.ServiceClient.Abstractions;
using Spectre.Console;
using System.Collections.ObjectModel;

namespace ConsoleApp.Pipelines;

internal class SpectreOutputPipeline : Pipeline
{
    private readonly OutputOptions outputOptions;
    private readonly ObservableCollection<(TickerData?, BookData?)> lastEvents = new();
    private readonly Table table;
    private readonly LiveDisplay liveTable;

    public SpectreOutputPipeline(IDeribitApiClient client, IOptions<OutputOptions> options, Table table, LiveDisplay liveTable) : base(client)
    {
        outputOptions = options.Value;
        this.table = table;
        this.liveTable = liveTable;
    }

    public override Task RunAsync(CancellationToken cancellationToken)
    {
        return liveTable.StartAsync((ctx) =>
        {
            AddTableHeaders();
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
        
        var converter = new Func<List<dynamic>, string>((x) => $"[bold]Change:[/] {x[0]}, [bold red]Price:[/] {x[1]}, [bold]Amount:[/] {x[2]}");
        for (var i = 0; i < lastEvents.Count; i++)
        {
            var (t, b) = lastEvents[i];
            
            var asksAsString = string.Join("; ", b?.Asks?.Select(converter) ?? new List<string>());
            var bidsAsString = string.Join("; ", b?.Bids?.Select(converter) ?? new List<string>());
            if (t is null)
            {
                table.AddEmptyRow();
            }
            else
            {
                table.AddRow(
                    new Markup($"[bold green]{t.InstrumentName}[/]"),
                    new Markup($"[bold red]{t.State}[/]"),
                    new Markup($"{t.MinPrice}"),
                    new Markup($"{t.MaxPrice}"),
                    new Markup($"{t.BestBidPrice}"),
                    new Markup($"{t.BestAskPrice}"),
                    new Markup(asksAsString),
                    new Markup(bidsAsString)
                    );
            }
        }
    }

    protected override void Client_OnTickerReceived(object? sender, TickerReceivedEventArgs e)
    {
        if (lastEvents.Count >= outputOptions.ConsoleAmountOfEvents)
        {
            lastEvents.RemoveAt(0);
        }

        lastEvents.Add((e.Ticker, e.Book));
    }

    protected override void WritePipelineStep(string stepInfo)
    {
        AnsiConsole.MarkupLine($"LOG: [bold red]{stepInfo}[/]");
    }
}
