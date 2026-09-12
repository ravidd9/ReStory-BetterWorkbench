using UnityEngine;

namespace ReStoryBetterWorkbench;

internal static class PanelArt
{
    private const string ArtName = "ReStoryRoundedPanel";

    private const int CornerRadius = 48;

    private const float PixelsPerUnit = 300f;

    private const int EdgeSamples = 4;

    private const int StraightEdgePixels = 2;

    private static Sprite _panel;
    private static bool _hasBuilt;

    internal static Sprite RoundedPanel()
    {
        if (_hasBuilt)
            return _panel;

        _hasBuilt = true;
        _panel = Build();

        return _panel;
    }

    private static Sprite Build()
    {
        int size = CornerRadius * 2 + StraightEdgePixels;

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false)
        {
            name = ArtName,
            hideFlags = HideFlags.HideAndDontSave,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                pixels[y * size + x] = new Color(1f, 1f, 1f, Coverage(x, y, size));
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(updateMipmaps: false);

        Vector4 border = Vector4.one * CornerRadius;

        Sprite panel = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f),
            PixelsPerUnit, 0, SpriteMeshType.FullRect, border);

        panel.name = ArtName;
        panel.hideFlags = HideFlags.HideAndDontSave;

        Log.Debug($"Rounded panel art built at {size}x{size}.");
        return panel;
    }

    private static float Coverage(int x, int y, int size)
    {
        int insideSamples = 0;

        for (int sampleY = 0; sampleY < EdgeSamples; sampleY++)
        {
            for (int sampleX = 0; sampleX < EdgeSamples; sampleX++)
            {
                float sampleAcross = x + (sampleX + 0.5f) / EdgeSamples;
                float sampleUp = y + (sampleY + 0.5f) / EdgeSamples;

                if (IsInsideRoundedRect(sampleAcross, sampleUp, size))
                {
                    insideSamples++;
                }
            }
        }

        return (float)insideSamples / (EdgeSamples * EdgeSamples);
    }

    private static bool IsInsideRoundedRect(float x, float y, float size)
    {
        float pastCornerX = Mathf.Max(CornerRadius - x, x - (size - CornerRadius), 0f);
        float pastCornerY = Mathf.Max(CornerRadius - y, y - (size - CornerRadius), 0f);

        return pastCornerX * pastCornerX + pastCornerY * pastCornerY <= CornerRadius * CornerRadius;
    }
}
