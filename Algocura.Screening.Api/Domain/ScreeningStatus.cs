namespace Algocura.Screening.Api.Domain;

public enum ScreeningStatus
{
    Draft = 1,
    Queued = 2,
    Running = 3,
    Completed = 4,
    Failed = 5,
    Cancelled = 6
}
