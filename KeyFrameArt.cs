using System.IO;
using System.Reflection;
using UnityEngine;

namespace ReStoryBetterWorkbench;

internal static class KeyFrameArt
{
    private const string ResourceName = "ReStoryBetterWorkbench.Assets.KeyFrame.png";
    private const string ArtName = "ReStoryKeyFrame";

    private const float PixelsPerUnit = 800f;

    private static readonly Vector4 Border = new(50f, 50f, 50f, 30f);

    private static Sprite _frame;
    private static bool _hasTriedToLoad;

    internal static Sprite Frame()
    {
        if (_hasTriedToLoad)
            return _frame;

        _hasTriedToLoad = true;
        _frame = Load();

        return _frame;
    }

    private static Sprite Load()
    {
        byte[] png = ReadResource();

        if (png == null)
        {
            Log.Warning($"Key frame art {ResourceName} is missing; key names show without a frame.");
            return null;
        }

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false)
        {
            name = ArtName,
            hideFlags = HideFlags.HideAndDontSave,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        if (!texture.LoadImage(png))
        {
            Log.Warning("Key frame art could not be decoded; key names show without a frame.");
            Object.Destroy(texture);
            return null;
        }

        Sprite frame = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f), PixelsPerUnit, 0, SpriteMeshType.FullRect, Border);

        frame.name = ArtName;
        frame.hideFlags = HideFlags.HideAndDontSave;

        Log.Debug($"Key frame art loaded at {texture.width}x{texture.height}.");
        return frame;
    }

    private static byte[] ReadResource()
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName);

        if (stream == null)
            return null;

        using MemoryStream buffer = new MemoryStream();
        stream.CopyTo(buffer);

        return buffer.ToArray();
    }
}
