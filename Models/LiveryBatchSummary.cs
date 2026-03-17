namespace LiveryManager.Models;

public sealed class LiveryBatchSummary
{
    public LiveryBatchSummary(IReadOnlyList<LiveryInstallOutcome> outcomes)
    {
        Outcomes = outcomes;
    }

    public IReadOnlyList<LiveryInstallOutcome> Outcomes { get; }

    public int InstalledCount => Outcomes.Count(outcome => outcome.Status == LiveryInstallOutcomeStatus.Installed);

    public int SkippedCount => Outcomes.Count(outcome => outcome.Status == LiveryInstallOutcomeStatus.Skipped);

    public int FailedCount => Outcomes.Count(outcome => outcome.Status == LiveryInstallOutcomeStatus.Failed);

    public bool HasSuccessfulInstalls => InstalledCount > 0;

    public bool IsSingleItem => Outcomes.Count == 1;
}
