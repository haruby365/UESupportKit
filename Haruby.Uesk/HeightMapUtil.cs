// © 2022 Jong-il Hong

using ImageMagick;

namespace Haruby.Uesk;

/// <remarks>
/// https://github.com/EpicGames/UnrealEngine/blob/46544fa5e0aa9e6740c19b44b0628b72e7bbd5ce/Engine/Source/Runtime/Landscape/Public/LandscapeDataAccess.h#L22
/// https://docs.unrealengine.com/5.0/en-US/landscape-technical-guide-in-unreal-engine/#calculatingheightmapzscale
/// </remarks>
public static class HeightMapUtil
{
    public static void Modify(string sourceFilePath, string outputFilePath, double sourceHeightMeters, double offsetMeters, bool heightBaseAsZero)
    {
        using MagickImage image = new(sourceFilePath);
        using (IPixelCollection<ushort> pixels = image.GetPixels())
        {
            Parallel.For(0, image.Height, y =>
            {
                for (int x = 0; x < image.Width; x++)
                {
                    IPixel<ushort> pixel = pixels.GetPixel(x, y);
                    ushort v = pixel.GetChannel(0);
                    v = ModifyHeightValue(v, sourceHeightMeters, offsetMeters, heightBaseAsZero);
                    pixel.SetChannel(0, v);
                }
            });
        }
        image.Write(outputFilePath);
    }

    public static ushort ModifyHeightValue(ushort heightValue, double sourceMaxMeters, double offsetMeters, bool heightBaseAsZero)
    {
        if (heightBaseAsZero)
        {
            double linear = heightValue / (double)MaxHeightValue;
            double meters = linear * sourceMaxMeters;
            meters += LandscapeLowerMeters;
            meters += offsetMeters;
            return HeightValueToUInt16(meters * MetersToValueScale);
        }
        else
        {
            double scale = sourceMaxMeters / LandscapeUpperMeters;
            double source = (heightValue - MidHeightValue) * scale + MidHeightValue;
            double offsetValue = offsetMeters * MetersToValueScale;
            return HeightValueToUInt16(source + offsetValue);
        }
    }

    public static ushort HeightValueToUInt16(double heightValue)
    {
        return (ushort)Math.Round(Math.Clamp(heightValue, 0d, MaxHeightValue));
    }

    public const double LandscapeLowerMeters = 256d;
    public const double LandscapeUpperMeters = ValueToMetersScale * (ushort.MaxValue - MidHeightValue);

    public const ushort MaxHeightValue = ushort.MaxValue;
    public const ushort MidHeightValue = ushort.MaxValue / 2 + 1;

    public const double ValueToMetersScale = LandscapeLowerMeters / MidHeightValue;
    public const double MetersToValueScale = 1d / ValueToMetersScale;
}
