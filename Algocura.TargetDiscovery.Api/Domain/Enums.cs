namespace Algocura.TargetDiscovery.Api.Domain;

public enum TargetHypothesisProjectionStatus
{
    Proposed = 1,
    UnderReview = 2,
    Accepted = 3,
    Rejected = 4,
    Published = 5
}

public enum TargetAssessmentStatus
{
    Draft = 1,
    InProgress = 2,
    Completed = 3,
    Rejected = 4,
    Selected = 5
}

public enum TargetConfidenceLevel
{
    Low = 1,
    Moderate = 2,
    High = 3,
    VeryHigh = 4
}
