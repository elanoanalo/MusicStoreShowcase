using SkiaSharp;
using System;

namespace MusicStoreShowcase.Models
{
    public static class CoverGenerator
    {
        private const int Size = 400; // обложка 400x400 пикселей
        private enum BackgroundStyle
        {
            RadialBurst,
            GeometricMosaic,
            ConcentricRings
        }



        public static byte[] GenerateCoverPng(ulong userSeed, int trackIndex, string title, string artist)
        {
            unchecked
            {
                ulong combined = userSeed * 6364136223846793005UL + (ulong)trackIndex + 0x9E3779B9UL;
                int coverSeed = (int)(combined & 0x7FFFFFFF);
                var rand = new Random(coverSeed);

                using var surface = SKSurface.Create(new SKImageInfo(Size, Size));
                var canvas = surface.Canvas;

                var palette = GeneratePalette(rand);
                canvas.Clear(palette.Base);
                var style = (BackgroundStyle)rand.Next(Enum.GetValues<BackgroundStyle>().Length);
                switch (style)
                {
                    case BackgroundStyle.RadialBurst:
                        DrawRadialBurst(canvas, palette, rand);
                        break;
                    case BackgroundStyle.GeometricMosaic:
                        DrawGeometricMosaic(canvas, palette, rand);
                        break;
                    case BackgroundStyle.ConcentricRings:
                        DrawConcentricRings(canvas, palette, rand);
                        break;
                }

                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Png, 90);
                return data.ToArray();
            }
        }
        private record Palette(SKColor Base, SKColor Accent, SKColor TextColor);

        private static Palette GeneratePalette(Random rand)
        {
            float hue = rand.Next(0, 360);
            float saturation = 55f + (float)rand.NextDouble() * 35f; // 55–90%
            float lightness = 35f + (float)rand.NextDouble() * 20f;  // 35–55%

            var baseColor = SKColor.FromHsl(hue, saturation, lightness);

            float accentHue = (hue + 180f) % 360f;
            var accentColor = SKColor.FromHsl(accentHue, saturation, Math.Min(lightness + 15f, 70f));

            var textColor = lightness > 50 ? new SKColor(20, 20, 20) : new SKColor(245, 245, 245);

            return new Palette(baseColor, accentColor, textColor);
        }

        private static void DrawRadialBurst(SKCanvas canvas, Palette palette, Random rand)
        {
            float cx = Size / 2f, cy = Size / 2f; // центр обложки
            int rayCount = rand.Next(10, 18);      // сколько лучей
            float maxRadius = Size * 0.8f;          // длина лучей

            using var paint = new SKPaint { IsAntialias = true };

            for (int i = 0; i < rayCount; i++)
            {
                float angle = (float)(i * (2 * Math.PI / rayCount));
                float spread = (float)(Math.PI / rayCount) * 0.6f;

                using var builder = new SKPathBuilder();
                builder.MoveTo(cx, cy);
                builder.LineTo(cx + maxRadius * (float)Math.Cos(angle - spread),
                               cy + maxRadius * (float)Math.Sin(angle - spread));
                builder.LineTo(cx + maxRadius * (float)Math.Cos(angle + spread),
                               cy + maxRadius * (float)Math.Sin(angle + spread));
                builder.Close();
                using var path = builder.Detach();

                paint.Color = i % 2 == 0 ? palette.Accent.WithAlpha(190) : palette.Accent.WithAlpha(110);
                canvas.DrawPath(path, paint);
            }
        }

        private static void DrawGeometricMosaic(SKCanvas canvas, Palette palette, Random rand)
        {
            using var paint = new SKPaint { IsAntialias = true };
            int shapeCount = rand.Next(14, 24);

            for (int i = 0; i < shapeCount; i++)
            {
                float x = rand.Next(0, Size);
                float y = rand.Next(0, Size);
                float r = rand.Next(20, 90);
                paint.Color = palette.Accent.WithAlpha((byte)rand.Next(90, 200));

                if (rand.Next(2) == 0)
                {
                    canvas.DrawCircle(x, y, r, paint);
                }
                else
                {
                    canvas.DrawRect(x, y, r, r, paint);
                }
            }
        }

        private static void DrawConcentricRings(SKCanvas canvas, Palette palette, Random rand)
        {
            float cx = Size / 2f, cy = Size / 2f;
            int ringCount = rand.Next(6, 11);
            float maxRadius = Size * 0.7f;

            using var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            for (int i = ringCount; i > 0; i--)
            {
                float radius = maxRadius * i / ringCount;
                paint.StrokeWidth = maxRadius / ringCount * 0.7f;
                paint.Color = i % 2 == 0 ? palette.Accent.WithAlpha(200) : palette.Accent.WithAlpha(100);
                canvas.DrawCircle(cx, cy, radius, paint);
            }
        }
    }
}