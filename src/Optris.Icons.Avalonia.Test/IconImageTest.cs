using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using FluentAssertions;
using FluentAssertions.Execution;
using Optris.Icons.Avalonia.Models;
using Xunit;

namespace Optris.Icons.Avalonia.Test;

[Collection(HeadlessCollection.Name)]
public class IconImageTest
{
    private const double Tolerance = 1e-6;

    // A destination rectangle that does not start at 0,0, with different X and Y.
    private static readonly Rect _offsetDestRect = new(100, 60, 64, 64);

    private readonly HeadlessUnitTestSession _session;

    static IconImageTest()
    {
        if (!IconProvider.Current.Providers.Any(p => p.Prefix == InlineIconProvider.InlinePrefix))
        {
            IconProvider.Current.Register(new InlineIconProvider());
        }
    }

    public IconImageTest(HeadlessSessionFixture fixture)
    {
        _session = fixture.Session;
    }

    // Rectangles inside their viewBox (all but the last fill it exactly) and rectangles that extend past it,
    // with square and non-square viewBoxes and viewBoxes that do not start at 0,0.
    public static TheoryData<string, string, bool> Icons => new()
    {
        { "0 0 512 512", "M0 0H512V512H0Z", false },
        { "0 0 640 512", "M0 0H640V512H0Z", false },
        { "0 0 320 512", "M0 0H320V512H0Z", false },
        { "100 50 512 512", "M100 50H612V562H100Z", false },
        { "-64 -32 640 512", "M-64 -32H576V480H-64Z", false },
        { "0 0 576 512", "M32 16H544V496H32Z", false },
        { "0 0 576 512", "M0 0H576V548H0Z", true },
        { "0 0 320 512", "M0 -64H320V576H0Z", true },
        { "0 0 448 512", "M-16 0H464V512H-16Z", true },
        { "0 0 512 512", "M0 -64H512V512H0Z", true },
        { "100 50 512 512", "M90 40H612V562H90Z", true },
        { "-64 -32 640 512", "M-64 -48H600V480H-64Z", true },
    };

    [Theory]
    [MemberData(nameof(Icons))]
    public Task ViewBox_Should_Be_The_Image_Box(string viewBox, string path, bool _) => Run(() =>
    {
        var expected = ViewBoxModel.Parse(viewBox);
        var image = new IconImage(InlineIconProvider.Key(viewBox, path), Brushes.Black);

        var draw = HeadlessRendering.RecordDraw(image, 64);

        // Size, scale and position come from the viewBox alone: it is drawn exactly onto the image.
        using var scope = new AssertionScope();
        image.Size.Should().Be(new Size(expected.Width, expected.Height));
        var viewBoxDrawn = new Rect(expected.X, expected.Y, expected.Width, expected.Height).TransformToAABB(draw.Transform);
        ShouldBeClose(viewBoxDrawn, draw.DestRect);
    });

    [Theory]
    [MemberData(nameof(Icons))]
    public Task Whole_Image_Draw_Should_Not_Clip_The_Geometry(string viewBox, string path, bool _) => Run(() =>
    {
        var image = new IconImage(InlineIconProvider.Key(viewBox, path), Brushes.Black);

        var draw = HeadlessRendering.RecordDraw(image, 64);

        using var scope = new AssertionScope();
        draw.ClipsContainDrawing().Should().BeTrue($"the clips {string.Join(", ", draw.Clips)} should contain the drawn geometry {draw.Drawn}");
        draw.Clips.Should().ContainSingle("the icon is drawn once; separate draws would leave a seam where the geometry crosses the box edge");
    });

    [Theory]
    [InlineData("0 0 512 512", "M0 0H512V512H0Z")]
    [InlineData("0 0 576 512", "M32 16H544V496H32Z")]
    [InlineData("-64 -32 640 512", "M-64 -32H576V480H-64Z")]
    public Task Icon_Inside_Its_ViewBox_Should_Be_Clipped_To_The_Image(string viewBox, string path) => Run(() =>
    {
        var image = new IconImage(InlineIconProvider.Key(viewBox, path), Brushes.Black);

        var draw = HeadlessRendering.RecordDraw(image, 64);

        draw.Clips.Should().ContainSingle();
        ShouldBeClose(draw.Clips[0], draw.DestRect);
    });

    // Non-square viewBoxes, so that UniformToFill in a square has to crop them.
    [Theory]
    [InlineData("0 0 576 512", "M0 0H576V548H0Z")]
    [InlineData("0 0 320 512", "M0 -64H320V576H0Z")]
    [InlineData("0 0 448 512", "M-16 0H464V512H-16Z")]
    [InlineData("-64 -32 640 512", "M-64 -48H600V480H-64Z")]
    public Task Cropping_Draw_Should_Be_Clipped_To_The_Image(string viewBox, string path) => Run(() =>
    {
        var image = new IconImage(InlineIconProvider.Key(viewBox, path), Brushes.Black);

        // UniformToFill crops the image to fill the square, so Image asks for only part of it.
        var draw = HeadlessRendering.RecordDraw(image, 64, Stretch.UniformToFill);

        draw.Clips.Should().ContainSingle();
        ShouldBeClose(draw.Clips[0], draw.DestRect);
    });

    [Theory]
    [MemberData(nameof(Icons))]
    public Task Only_Geometry_Outside_The_ViewBox_Should_Change_The_Rendering(string viewBox, string path, bool outsideViewBox) => Run(() =>
    {
        const int size = 64;
        var key = InlineIconProvider.Key(viewBox, path);

        // "Before" is the same image clipped to its box, as IconImage drew every icon before.
        var (before, _) = HeadlessRendering.RenderImage(new ClippedToDestRect(new IconImage(key, Brushes.Black)), size);
        var (actual, _) = HeadlessRendering.RenderImage(new IconImage(key, Brushes.Black), size);

        var changed = Enumerable.Range(0, before.Length).Count(i => actual[i] != before[i]);
        var added = Enumerable.Range(0, before.Length).Count(i => before[i] == 0 && actual[i] != 0);

        if (outsideViewBox)
        {
            added.Should().BePositive("the geometry outside the viewBox must be drawn");
        }
        else
        {
            changed.Should().Be(0, "an icon inside its viewBox renders exactly as it did when clipped to its box");
        }
    });

    // Custom render code and ImageDrawing can ask an image to draw into a rectangle that does not start at 0,0 (issue #43).
    [Theory]
    [MemberData(nameof(Icons))]
    public Task ViewBox_Should_Be_Drawn_Onto_An_Offset_Dest_Rect(string viewBox, string path, bool _) => Run(() =>
    {
        var expected = ViewBoxModel.Parse(viewBox);
        var image = new IconImage(InlineIconProvider.Key(viewBox, path), Brushes.Black);

        var draw = HeadlessRendering.RecordDraw(image, new Rect(image.Size), _offsetDestRect);

        var viewBoxDrawn = new Rect(expected.X, expected.Y, expected.Width, expected.Height).TransformToAABB(draw.Transform);
        ShouldBeClose(viewBoxDrawn, _offsetDestRect);
    });

    [Theory]
    [MemberData(nameof(Icons))]
    public Task Whole_Image_Draw_Should_Land_In_An_Offset_Dest_Rect(string viewBox, string path, bool _) => Run(() =>
    {
        const int size = 256;
        var model = ViewBoxModel.Parse(viewBox);
        var geometry = StreamGeometry.Parse(path).Bounds;
        var image = new IconImage(InlineIconProvider.Key(viewBox, path), Brushes.Black);

        var pixels = HeadlessRendering.RenderDraw(image, new Rect(image.Size), _offsetDestRect, size);

        // The geometry, including any part outside the viewBox, lands where the viewBox-to-destRect mapping puts it.
        var scaleX = _offsetDestRect.Width / model.Width;
        var scaleY = _offsetDestRect.Height / model.Height;
        var expected = new Rect(
            _offsetDestRect.X + (geometry.X - model.X) * scaleX,
            _offsetDestRect.Y + (geometry.Y - model.Y) * scaleY,
            geometry.Width * scaleX,
            geometry.Height * scaleY);
        ShouldBeClose(HeadlessRendering.InkBounds(pixels, size), expected, tolerance: 1);
    });

    // Non-square viewBoxes cropped to the square in their middle, as UniformToFill crops them.
    [Theory]
    [InlineData("0 0 576 512", "M0 0H576V548H0Z")]
    [InlineData("0 0 320 512", "M0 -64H320V576H0Z")]
    [InlineData("0 0 448 512", "M-16 0H464V512H-16Z")]
    [InlineData("-64 -32 640 512", "M-64 -48H600V480H-64Z")]
    public Task Cropping_Draw_Should_Fill_An_Offset_Dest_Rect(string viewBox, string path) => Run(() =>
    {
        const int size = 256;
        var model = ViewBoxModel.Parse(viewBox);
        var image = new IconImage(InlineIconProvider.Key(viewBox, path), Brushes.Black);
        var side = Math.Min(image.Size.Width, image.Size.Height);
        var sourceRect = new Rect((image.Size.Width - side) / 2, (image.Size.Height - side) / 2, side, side);

        var draw = HeadlessRendering.RecordDraw(image, sourceRect, _offsetDestRect);
        var pixels = HeadlessRendering.RenderDraw(image, sourceRect, _offsetDestRect, size);

        // The source rect is drawn exactly onto destRect and stays clipped to it. Every path covers its viewBox,
        // so the crop fills destRect.
        using var scope = new AssertionScope();
        var sourceDrawn = sourceRect.Translate(new Vector(model.X, model.Y)).TransformToAABB(draw.Transform);
        ShouldBeClose(sourceDrawn, _offsetDestRect);
        draw.Clips.Should().ContainSingle();
        ShouldBeClose(draw.Clips[0], _offsetDestRect);
        HeadlessRendering.InkBounds(pixels, size).Should().Be(_offsetDestRect);
    });

    [Fact]
    public Task Icon_Should_Show_Geometry_Outside_Its_Box() => Run(() =>
    {
        const int size = 64;
        // A square icon whose geometry extends 64 units (8 px at this size) above its viewBox.
        var icon = new Icon { Value = InlineIconProvider.Key("0 0 512 512", "M0 -64H512V512H0Z"), FontSize = size, Foreground = Brushes.Black };
        var canvas = new Canvas { Width = 2 * size, Height = 2 * size };
        Canvas.SetLeft(icon, size / 2.0);
        Canvas.SetTop(icon, size / 2.0);
        canvas.Children.Add(icon);

        var pixels = HeadlessRendering.Pixels(HeadlessRendering.Render(canvas, 2 * size));

        var top = Enumerable.Range(0, pixels.Length).Where(i => pixels[i] != 0).Min(i => i / (2 * size));
        top.Should().Be(size / 2 - 8, "the part above the viewBox is drawn above the icon's box");
    });

    private Task Run(Action test) => _session.Dispatch(test, CancellationToken.None);

    private static void ShouldBeClose(Rect actual, Rect expected, double tolerance = Tolerance)
    {
        using var scope = new AssertionScope($"{actual} compared with {expected}");
        actual.Left.Should().BeApproximately(expected.Left, tolerance);
        actual.Top.Should().BeApproximately(expected.Top, tolerance);
        actual.Right.Should().BeApproximately(expected.Right, tolerance);
        actual.Bottom.Should().BeApproximately(expected.Bottom, tolerance);
    }

    /// <summary>
    /// Serves icons whose viewBox and path are part of the key: "inline-{viewBox}|{path}".
    /// </summary>
    private sealed class InlineIconProvider : IIconProvider
    {
        public const string InlinePrefix = "inline";

        public string Prefix => InlinePrefix;

        public static string Key(string viewBox, string path) => $"{InlinePrefix}-{viewBox}|{path}";

        public IconModel GetIcon(string value)
        {
            var parts = value.Substring(InlinePrefix.Length + 1).Split('|');
            return new IconModel(ViewBoxModel.Parse(parts[0]), new PathModel(parts[1]));
        }
    }
}
