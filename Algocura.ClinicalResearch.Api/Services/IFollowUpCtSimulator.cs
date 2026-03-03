using Algocura.ClinicalResearch.Api.Contracts;

namespace Algocura.ClinicalResearch.Api.Services;

public interface IFollowUpCtSimulator
{
    Task<(byte[] Png, SimulatedFollowUpCtResponse Metadata)> SimulateAsync(Stream inputImage, double responseScore, int lesionCenterX, int lesionCenterY, int lesionRadiusPixels, CancellationToken cancellationToken);
}
