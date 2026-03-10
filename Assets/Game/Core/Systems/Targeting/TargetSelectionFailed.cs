using System;

/// <summary>
/// Исключение, выбрасываемое когда не удается выбрать необходимое количество целей
/// </summary>
public class TargetSelectionFailed : Exception
{
    public Geid TargetingSpecId { get; }
    public int RequiredTargets { get; }
    public int FoundTargets { get; }

    public TargetSelectionFailed(Geid targetingSpecId, int requiredTargets, int foundTargets)
        : base($"Failed to select targets for spec {targetingSpecId}. Required: {requiredTargets}, Found: {foundTargets}")
    {
        TargetingSpecId = targetingSpecId;
        RequiredTargets = requiredTargets;
        FoundTargets = foundTargets;
    }

    public TargetSelectionFailed(string message) : base(message)
    {
    }

    public TargetSelectionFailed(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}