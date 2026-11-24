using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Optris.Icons.Avalonia.FontAwesome7.Test;

/// <summary>
/// Draws images the way <see cref="Image"/> does and inspects the result. Must run on the headless UI thread.
/// </summary>
internal static class HeadlessRendering
{
    /// <summary>
    /// Shows <paramref name="source"/> in a <paramref name="size"/> x <paramref name="size"/> <see cref="Image"/>
    /// and records what it draws.
    /// </summary>
    public static RecordedDraw RecordDraw(IImage source, double size, Stretch stretch = Stretch.Uniform)
    {
        var image = new Image { Source = source, Width = size, Height = size, Stretch = stretch };
        image.Measure(new Size(size, size));
        image.Arrange(new Rect(0, 0, size, size));

        var recording = new DrawingGroup();
        using (var context = recording.Open())
        {
            image.Render(context);
        }

        var clipped = Flatten(recording).Where(group => group.ClipGeometry is not null).ToList();
        var transformed = Flatten(clipped[0]).Single(group => group.Transform is not null);
        return new RecordedDraw(
            DestRect: new Rect(image.Bounds.Size),
            Clips: clipped.Select(group => group.ClipGeometry.Bounds).ToList(),
            Drawn: clipped[0].GetBounds(),
            Transform: transformed.Transform.Value);
    }

    /// <summary>
    /// Renders <paramref name="source"/> in a <paramref name="size"/> x <paramref name="size"/> <see cref="Image"/>
    /// in the middle of a canvas twice that size, so that anything drawn outside the image is captured too.
    /// Returns the canvas pixels and where the image ended up on the canvas.
    /// </summary>
    public static (uint[] Pixels, Point ImagePosition) RenderImage(IImage source, int size)
    {
        var image = new Image { Source = source, Width = size, Height = size };
        var canvas = new Canvas { Width = 2 * size, Height = 2 * size };
        Canvas.SetLeft(image, size / 2.0);
        Canvas.SetTop(image, size / 2.0);
        canvas.Children.Add(image);
        var pixels = Pixels(Render(canvas, 2 * size));
        return (pixels, image.Bounds.Position);
    }

    public static Bitmap Render(Control control, int size)
    {
        control.Measure(new Size(size, size));
        control.Arrange(new Rect(0, 0, size, size));
        var bitmap = new RenderTargetBitmap(new PixelSize(size, size));
        bitmap.Render(control);
        return bitmap;
    }

    /// <summary>
    /// Returns the pixels as 32-bit values, row by row.
    /// </summary>
    public static uint[] Pixels(Bitmap bitmap)
    {
        var size = bitmap.PixelSize;
        var pixels = new uint[size.Width * size.Height];
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            bitmap.CopyPixels(new PixelRect(size), handle.AddrOfPinnedObject(), pixels.Length * 4, size.Width * 4);
        }
        finally
        {
            handle.Free();
        }

        return pixels;
    }

    private static IEnumerable<DrawingGroup> Flatten(DrawingGroup group) =>
        group.Children.OfType<DrawingGroup>().SelectMany(Flatten).Prepend(group);
}

/// <param name="DestRect">The rectangle <see cref="Image"/> asks the source to draw into.</param>
/// <param name="Clips">The clips of the source's draws, in drawing order. An icon is drawn once, so there is one.</param>
/// <param name="Drawn">The bounds of the drawing, ignoring the clips.</param>
/// <param name="Transform">The transform the source draws with.</param>
internal sealed record RecordedDraw(Rect DestRect, IReadOnlyList<Rect> Clips, Rect Drawn, Matrix Transform)
{
    /// <summary>
    /// Whether the clips together cover the whole drawing, so that no part of it is cut off.
    /// </summary>
    public bool ClipsContainDrawing(double tolerance = 1e-6)
    {
        var clip = Clips.Aggregate((a, b) => a.Union(b));
        return Drawn.Left >= clip.Left - tolerance
            && Drawn.Top >= clip.Top - tolerance
            && Drawn.Right <= clip.Right + tolerance
            && Drawn.Bottom <= clip.Bottom + tolerance;
    }
}

/// <summary>
/// Draws <paramref name="image"/> clipped to the rectangle it is asked to draw into, as <see cref="IconImage"/>
/// did before it drew the parts of an icon that lie outside its viewBox.
/// </summary>
internal sealed class ClippedToDestRect(IImage image) : IImage
{
    public Rect DestRect { get; private set; }

    public Size Size => image.Size;

    public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
    {
        DestRect = destRect;
        using (context.PushClip(destRect))
        {
            image.Draw(context, sourceRect, destRect);
        }
    }
}
