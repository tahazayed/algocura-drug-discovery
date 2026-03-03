using Algocura.ClinicalResearch.Api.Contracts;
using SkiaSharp;

namespace Algocura.ClinicalResearch.Api.Services;

public sealed class PrototypeFollowUpCtSimulator
    : IFollowUpCtSimulator
{
    public async Task<(byte[] Png, SimulatedFollowUpCtResponse Metadata)> SimulateAsync(
        Stream inputImage,
        double responseScore,
        int lesionCenterX,
        int lesionCenterY,
        int lesionRadiusPixels,
        CancellationToken cancellationToken)
    {
        if (responseScore is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(responseScore));
        }

        if (lesionRadiusPixels < 4)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lesionRadiusPixels));
        }

        using var memoryStream =
            new MemoryStream();

        await inputImage.CopyToAsync(
            memoryStream,
            cancellationToken);

        memoryStream.Position = 0;

        using var inputBitmap =
            SKBitmap.Decode(memoryStream);

        if (inputBitmap is null)
        {
            throw new InvalidOperationException(
                "The input image could not be decoded.");
        }

        if (
            lesionCenterX < 0 ||
            lesionCenterX >= inputBitmap.Width ||
            lesionCenterY < 0 ||
            lesionCenterY >= inputBitmap.Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lesionCenterX),
                "Lesion center must be inside the input image.");
        }

        using var outputBitmap =
            inputBitmap.Copy();

        var shrinkage =
            Math.Clamp(
                responseScore * 0.55,
                0,
                0.55);

        var newRadius =
            Math.Max(
                2,
                (int)Math.Round(
                    lesionRadiusPixels *
                    (1 - shrinkage)));

        var background =
            EstimateBackground(
                outputBitmap,
                lesionCenterX,
                lesionCenterY,
                lesionRadiusPixels);

        var originalRadiusSquared =
            lesionRadiusPixels *
            lesionRadiusPixels;

        var newRadiusSquared =
            newRadius *
            newRadius;

        var minX =
            Math.Max(
                0,
                lesionCenterX -
                lesionRadiusPixels);

        var maxX =
            Math.Min(
                outputBitmap.Width - 1,
                lesionCenterX +
                lesionRadiusPixels);

        var minY =
            Math.Max(
                0,
                lesionCenterY -
                lesionRadiusPixels);

        var maxY =
            Math.Min(
                outputBitmap.Height - 1,
                lesionCenterY +
                lesionRadiusPixels);

        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var dx =
                    x - lesionCenterX;

                var dy =
                    y - lesionCenterY;

                var distanceSquared =
                    (dx * dx) +
                    (dy * dy);

                if (
                    distanceSquared <= originalRadiusSquared &&
                    distanceSquared > newRadiusSquared)
                {
                    var source =
                        outputBitmap.GetPixel(
                            x,
                            y);

                    var blended =
                        Blend(
                            source,
                            background,
                            0.72f);

                    outputBitmap.SetPixel(
                        x,
                        y,
                        blended);
                }
            }
        }

        using var outputImage =
            SKImage.FromBitmap(
                outputBitmap);

        using var encoded =
            outputImage.Encode(
                SKEncodedImageFormat.Png,
                100);

        if (encoded is null)
        {
            throw new InvalidOperationException(
                "The simulated image could not be encoded.");
        }

        var png =
            encoded.ToArray();

        var name =
            $"algocura-simulated-followup-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.png";

        var metadata =
            new SimulatedFollowUpCtResponse(
                name,
                "image/png",
                Math.Round(
                    responseScore,
                    4),
                Math.Round(
                    shrinkage,
                    4),
                lesionRadiusPixels,
                newRadius,
                "Synthetic research visualization only. " +
                "It does not predict an individual patient's future CT " +
                "appearance and must not be used for clinical decision-making.");

        return (
            png,
            metadata);
    }

    private static SKColor EstimateBackground(
        SKBitmap bitmap,
        int centerX,
        int centerY,
        int radius)
    {
        long red = 0;
        long green = 0;
        long blue = 0;
        long count = 0;

        var outerRadius =
            Math.Max(
                radius + 2,
                (int)Math.Round(
                    radius * 1.25));

        var innerSquared =
            radius *
            radius;

        var outerSquared =
            outerRadius *
            outerRadius;

        var minY =
            Math.Max(
                0,
                centerY -
                outerRadius);

        var maxY =
            Math.Min(
                bitmap.Height - 1,
                centerY +
                outerRadius);

        var minX =
            Math.Max(
                0,
                centerX -
                outerRadius);

        var maxX =
            Math.Min(
                bitmap.Width - 1,
                centerX +
                outerRadius);

        for (var y = minY; y <= maxY; y++)
        {
            for (var x = minX; x <= maxX; x++)
            {
                var dx =
                    x - centerX;

                var dy =
                    y - centerY;

                var distanceSquared =
                    (dx * dx) +
                    (dy * dy);

                if (
                    distanceSquared <= innerSquared ||
                    distanceSquared > outerSquared)
                {
                    continue;
                }

                var pixel =
                    bitmap.GetPixel(
                        x,
                        y);

                red += pixel.Red;
                green += pixel.Green;
                blue += pixel.Blue;
                count++;
            }
        }

        if (count == 0)
        {
            return new SKColor(
                96,
                96,
                96);
        }

        return new SKColor(
            (byte)(red / count),
            (byte)(green / count),
            (byte)(blue / count));
    }

    private static SKColor Blend(
        SKColor source,
        SKColor target,
        float amount)
    {
        static byte Lerp(
            byte from,
            byte to,
            float factor)
        {
            return
                (byte)Math.Clamp(
                    (int)Math.Round(
                        from +
                        ((to - from) * factor)),
                    0,
                    255);
        }

        return new SKColor(
            Lerp(
                source.Red,
                target.Red,
                amount),
            Lerp(
                source.Green,
                target.Green,
                amount),
            Lerp(
                source.Blue,
                target.Blue,
                amount),
            source.Alpha);
    }
}