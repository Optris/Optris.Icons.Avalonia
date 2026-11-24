using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Immutable;

#nullable enable

namespace Optris.Icons.Avalonia;

public class IconImage : DrawingImage, IImage
{
    public static readonly StyledProperty<string> ValueProperty = AvaloniaProperty.Register<
        IconImage,
        string
    >(nameof(Value), string.Empty);

    public static readonly StyledProperty<IBrush> BrushProperty = AvaloniaProperty.Register<
        IconImage,
        IBrush
    >(nameof(Brush), new SolidColorBrush(Colors.Black));

    public static readonly StyledProperty<IPen> PenProperty = AvaloniaProperty.Register<
        IconImage,
        IPen
    >(nameof(Pen), new ImmutablePen(Colors.Black.ToUInt32(), 0));

    // The viewBox. It is the image's Size, so it alone decides how an icon is sized and positioned.
    private Rect _bounds;

    // The viewBox together with the geometry. Some icons extend past their viewBox on purpose: Font
    // Awesome renders its SVGs with "overflow: visible", and many Font Awesome 7 icons rely on that.
    private Rect _drawingBounds;

    public IconImage()
        : this(string.Empty, new SolidColorBrush(Colors.Black)) { }

    public IconImage(string value, IBrush brush)
    {
        Value = value;
        Brush = brush;
    }

    public string Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IBrush Brush
    {
        get => GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    public IPen Pen
    {
        get => GetValue(PenProperty);
        set => SetValue(PenProperty, value);
    }

    /// <inheritdoc>
    public new Size Size => _bounds.Size;

    /// <inheritdoc>
    Size IImage.Size => _bounds.Size;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty)
        {
            HandleValueChanged();
            RaiseInvalidated(EventArgs.Empty);
        }
        else if (change.Property == BrushProperty)
        {
            HandleBrushChanged();
            RaiseInvalidated(EventArgs.Empty);
        }
        else if (change.Property == PenProperty)
        {
            HandlePenChanged();
            RaiseInvalidated(EventArgs.Empty);
        }
    }

    private void HandleValueChanged()
    {
        var iconModel = IconProvider.Current.GetIcon(Value);
        var geometry = StreamGeometry.Parse(iconModel.Path);

        _bounds = new Rect(
            x: iconModel.ViewBox.X,
            y: iconModel.ViewBox.Y,
            width: iconModel.ViewBox.Width,
            height: iconModel.ViewBox.Height
        );
        _drawingBounds = _bounds.Union(geometry.Bounds);

        var drawing = GetGeometryDrawing();
        drawing.Geometry = geometry;
    }

    private void HandleBrushChanged()
    {
        var drawing = GetGeometryDrawing();
        drawing.Brush = Brush;
    }

    private void HandlePenChanged()
    {
        var drawing = GetGeometryDrawing();
        drawing.Pen = Pen;
    }

    private GeometryDrawing GetGeometryDrawing()
    {
        return (GeometryDrawing)(Drawing ??= new GeometryDrawing());
    }

    /// <inheritdoc/>
    void IImage.Draw(DrawingContext context, Rect sourceRect, Rect destRect)
    {
        var drawing = Drawing;
        if (drawing == null)
        {
            return;
        }

        var bounds = _bounds;
        var scale = Matrix.CreateScale(
            destRect.Width / sourceRect.Width,
            destRect.Height / sourceRect.Height
        );
        var translate = Matrix.CreateTranslation(
            -sourceRect.X + destRect.X - bounds.X,
            -sourceRect.Y + destRect.Y - bounds.Y
        );
        var transform = translate * scale;

        // When the whole image is drawn, the clip also covers the parts of the icon outside its viewBox,
        // up to where the transform puts them, so they are drawn outside destRect instead of being cut
        // off. A draw of only part of the image (a smaller source rect, as Stretch.UniformToFill
        // produces) stays cropped to destRect.
        var clip = destRect;
        if (_drawingBounds != bounds && CoversImage(sourceRect, bounds.Size))
        {
            clip = destRect.Union(_drawingBounds.TransformToAABB(transform));
        }

        using (context.PushClip(clip))
        using (context.PushTransform(transform))
        {
            drawing.Draw(context);
        }
    }

    private static bool CoversImage(Rect sourceRect, Size imageSize)
    {
        // Absorbs rounding in the source rect that Image.Render computes for the whole image.
        const double tolerance = 1e-6;
        return sourceRect.X <= tolerance
            && sourceRect.Y <= tolerance
            && sourceRect.Right >= imageSize.Width - tolerance
            && sourceRect.Bottom >= imageSize.Height - tolerance;
    }
}
