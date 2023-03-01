namespace ConsoleApp.DTOs;

internal class OutputOptions
{
    public OutputTypes Type { get; set; }
    public int ConsoleAmountOfEvents { get; set; }

    public enum OutputTypes
    {
        Console,
        Logging
    }
}
