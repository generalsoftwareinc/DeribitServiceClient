using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Deribit.ApiClient.Configuration;

public sealed record DeribitOptions : IValidateOptions<DeribitOptions>
{
    [Required]
    public string ClientId { get; set; } = string.Empty;
    [Required]
    public string ClientSecret { get; set; } = string.Empty;
    [Required]
    public string WebSocketUrl { get; set; } = string.Empty;
    [Required]
    public string InstrumentName { get; set; } = string.Empty;
    [Required]
    public string TickerInterval { get; set; } = string.Empty;
    [Required]
    public string BookInterval { get; set; } = string.Empty;
    [Required]
    [Range(1, 3000)]
    public int HeartBeatInterval { get; set; }

    #region Validation
    public ValidateOptionsResult Validate(string? name, DeribitOptions options)
    {
        if (options == null)
            return ValidateOptionsResult.Fail($"{nameof(DeribitOptions)} is null.");

        var errors = GetValidationErrorMessages(options);
        
        return errors.Any()
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }

    public static readonly string[] ValidSubscriptionIntervalValues = new string[] { "raw", "100ms" };

    private static IEnumerable<string> GetValidationErrorMessages(DeribitOptions options)
    {
        if (!ValidSubscriptionIntervalValues.Contains(options.BookInterval, StringComparer.InvariantCulture))
            yield return $"{nameof(options.BookInterval)} value '{options.BookInterval}' is not supported. (Valid values: '{string.Join("', '", ValidSubscriptionIntervalValues)}')";

        if (!ValidSubscriptionIntervalValues.Contains(options.TickerInterval, StringComparer.InvariantCulture))
            yield return $"{nameof(options.TickerInterval)} value '{options.TickerInterval}' is not supported. (Valid values: '{string.Join("', '", ValidSubscriptionIntervalValues)}')";
        
        if (!Uri.TryCreate(options.WebSocketUrl, UriKind.RelativeOrAbsolute, out var _))
            yield return $"{nameof(options.WebSocketUrl)} is not a valid Uri.";
    }
    #endregion Validation
}
