using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace Optris.Icons.Avalonia.FontAwesome7.Test;

// Many Font Awesome 7 icons draw parts outside their viewBox on purpose (issue #30). These tests cover the
// whole Font Awesome 7 Free set.
[Collection(HeadlessCollection.Name)]
public class FontAwesome7IconRenderingTest
{
    private const double Tolerance = 1e-6;

    private static readonly FontAwesome7IconProvider _iconProvider = new();

    private readonly HeadlessUnitTestSession _session;

    static FontAwesome7IconRenderingTest()
    {
        if (!IconProvider.Current.Providers.Any(p => p.Prefix == _iconProvider.Prefix))
        {
            IconProvider.Current.Register(_iconProvider);
        }
    }

    public FontAwesome7IconRenderingTest(HeadlessSessionFixture fixture)
    {
        _session = fixture.Session;
    }

    [Fact]
    public Task No_Icon_Should_Lose_Geometry_To_The_Clip() => Run(() =>
    {
        var keys = _iconProvider.Keys;

        var draws = keys.Select(key => (key, draw: HeadlessRendering.RecordDraw(new IconImage(key, Brushes.Black), 32))).ToList();
        var clipped = draws.Where(d => !d.draw.ClipsContainDrawing()).Select(d => d.key).ToList();
        var drawnInParts = draws.Where(d => d.draw.Clips.Count != 1).Select(d => d.key).ToList();

        using var scope = new AssertionScope();
        clipped.Should().BeEmpty("{0} of {1} icons lose geometry to the clip", clipped.Count, keys.Count);
        drawnInParts.Should().BeEmpty("every icon is drawn once; separate draws would leave a seam at the box edge");
    });

    [Fact]
    public Task Every_Icon_Should_Keep_Its_ViewBox_As_Image_Box() => Run(() =>
    {
        var changed = new List<string>();

        foreach (var key in _iconProvider.Keys)
        {
            var model = _iconProvider.GetIcon(key);
            var viewBox = new Rect(model.ViewBox.X, model.ViewBox.Y, model.ViewBox.Width, model.ViewBox.Height);
            var image = new IconImage(key, Brushes.Black);

            var draw = HeadlessRendering.RecordDraw(image, 32);

            // Size, scale and position come from the viewBox alone: it is drawn exactly onto the image.
            var drawnViewBox = viewBox.TransformToAABB(draw.Transform);
            if (image.Size != viewBox.Size || !AreClose(drawnViewBox, draw.DestRect))
            {
                changed.Add($"{key} (size {image.Size}, viewBox {viewBox} drawn at {drawnViewBox}, image {draw.DestRect})");
            }
        }

        changed.Should().BeEmpty();
    });

    [Fact]
    public Task Only_Geometry_Outside_The_ViewBox_Should_Change_The_Rendering() => Run(() =>
    {
        const int size = 32;
        var changedInsideViewBox = new List<string>();
        var outsideNotDrawn = new List<string>();

        foreach (var key in _iconProvider.Keys)
        {
            var model = _iconProvider.GetIcon(key);
            var viewBox = new Rect(model.ViewBox.X, model.ViewBox.Y, model.ViewBox.Width, model.ViewBox.Height);
            var image = new IconImage(key, Brushes.Black);
            var geometry = ((GeometryDrawing)image.Drawing).Geometry.Bounds;

            // "Before" is the same image clipped to its box, as IconImage drew every icon before.
            var reference = new ClippedToDestRect(new IconImage(key, Brushes.Black));
            var (before, _) = HeadlessRendering.RenderImage(reference, size);
            var (actual, _) = HeadlessRendering.RenderImage(image, size);

            if (viewBox.Union(geometry) == viewBox)
            {
                // An icon that stays inside its viewBox renders exactly as it did when clipped to its box.
                var changed = Enumerable.Range(0, before.Length).Count(i => actual[i] != before[i]);
                if (changed > 0)
                {
                    changedInsideViewBox.Add($"{key} ({changed} px)");
                }

                continue;
            }

            // Geometry that extends past the viewBox by a pixel or more must show up outside the box.
            var scale = reference.DestRect.Width / image.Size.Width;
            var outside = new[]
            {
                viewBox.Left - geometry.Left,
                viewBox.Top - geometry.Top,
                geometry.Right - viewBox.Right,
                geometry.Bottom - viewBox.Bottom,
            }.Max() * scale;
            var added = Enumerable.Range(0, before.Length).Count(i => before[i] == 0 && actual[i] != 0);
            if (outside >= 1 && added == 0)
            {
                outsideNotDrawn.Add($"{key} ({outside.ToString("0.##", CultureInfo.InvariantCulture)} px outside the viewBox)");
            }
        }

        using var scope = new AssertionScope();
        changedInsideViewBox.Should().BeEmpty("icons inside their viewBox must render as before");
        outsideNotDrawn.Should().BeEmpty("geometry outside the viewBox must be drawn");
    });

    [Theory]
    [InlineData("fa7-solid fa7-file-pdf")]
    [InlineData("fa7-regular fa7-file-pdf")]
    public Task Icon_Should_Draw_The_Whole_File_Pdf_Label(string value) => Run(() =>
    {
        const int size = 256;
        var geometry = StreamGeometry.Parse(_iconProvider.GetIcon(value).Path).Bounds;
        var icon = new Icon { Value = value, FontSize = size, Foreground = Brushes.Black };
        var canvas = new Canvas { Width = 2 * size, Height = 2 * size };
        Canvas.SetLeft(icon, size / 2.0);
        Canvas.SetTop(icon, size / 2.0);
        canvas.Children.Add(icon);

        var pixels = HeadlessRendering.Pixels(HeadlessRendering.Render(canvas, 2 * size));
        var inked = Enumerable.Range(0, pixels.Length).Where(i => pixels[i] != 0).ToList();
        var width = inked.Max(i => i % (2 * size)) - inked.Min(i => i % (2 * size)) + 1;
        var height = inked.Max(i => i / (2 * size)) - inked.Min(i => i / (2 * size)) + 1;

        // The "PDF" label reaches below the viewBox (to y = 548 in a 512 high viewBox). If the label is
        // cut off, the ink is too short for its width.
        var expectedHeight = geometry.Height * width / geometry.Width;
        ((double)height).Should().BeApproximately(expectedHeight, 2);
    });

    private Task Run(Action test) => _session.Dispatch(test, CancellationToken.None);

    private static bool AreClose(Rect a, Rect b) =>
        Math.Abs(a.Left - b.Left) <= Tolerance
        && Math.Abs(a.Top - b.Top) <= Tolerance
        && Math.Abs(a.Right - b.Right) <= Tolerance
        && Math.Abs(a.Bottom - b.Bottom) <= Tolerance;
}
