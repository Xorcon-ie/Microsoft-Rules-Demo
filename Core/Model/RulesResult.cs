namespace Core;

// ----------------------------------------------------------------------------------------------------
// A DTO for returning individual results form a rules evaluation
public record RulesResult
{
    public required string RecordId { get; init; }
    public required string RecordDescription { get; init; }
    public required string RuleName { get; init; }
    public required string RulesScript { get; init; }
    public bool Success { get; init; }
    public string? SuccessResult {get;set;}
    public string? LastError { get; set; }
    public object? RuleOutput { get; set; }
    public float ApproxExecutionMilliseconds { get; set; }
    public RulesResult[]? SubResults {get;set;}
}
