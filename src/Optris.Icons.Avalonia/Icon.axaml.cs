using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

#nullable enable

namespace Optris.Icons.Avalonia;

public partial class Icon : TemplatedControl
{
    public static readonly StyledProperty<string> ValueProperty = AvaloniaProperty.Register<
        Icon,
        string
    >(nameof(Value), string.Empty);

    public static readonly StyledProperty<IconAnimation> AnimationProperty =
        AvaloniaProperty.Register<Icon, IconAnimation>(nameof(Animation));

    internal static readonly StyledProperty<IconImage> ImageProperty =
        AvaloniaProperty.Register<Icon, IconImage>(nameof(Image));

    /// <summary>
    /// True while the icon has an <see cref="Animation"/> and is effectively visible in a visual tree.
    /// The Spin and Pulse styles only match while it is true, so hiding the icon disposes its animation.
    /// Avalonia would only pause it, and a paused animation stays subscribed to the global animation
    /// clock, which keeps the UI thread rendering a frame about every 16 ms.
    /// </summary>
    internal static readonly StyledProperty<bool> IsAnimationActiveProperty =
        AvaloniaProperty.Register<Icon, bool>(nameof(IsAnimationActive));

    private static readonly SolidColorBrush _fallbackForeground = new(Colors.Black);

    // Animated icons attached to a visual tree. Avalonia has no public notification for a change of an
    // ancestor's IsVisible, so these icons check their effective visibility on every IsVisible change.
    private static readonly HashSet<Icon> _animatedIcons = new();

    private bool _isAttachedToVisualTree;

    static Icon()
    {
        // TemplatedControl clips to its bounds by default. Icons may draw parts outside their box on
        // purpose (see IconImage), so the icon itself does not clip them.
        ClipToBoundsProperty.OverrideDefaultValue<Icon>(false);
        IsVisibleProperty.Changed.Subscribe(_ => UpdateAnimatedIcons());
    }

    public Icon()
    {
        Image = new();
        AvaloniaXamlLoader.Load(this);
    }

    public string Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public IconAnimation Animation
    {
        get => GetValue(AnimationProperty);
        set => SetValue(AnimationProperty, value);
    }

    internal IconImage Image
    {
        get => GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
    }

    internal bool IsAnimationActive
    {
        get => GetValue(IsAnimationActiveProperty);
        set => SetValue(IsAnimationActiveProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttachedToVisualTree = true;
        UpdateIsAnimationActive();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _isAttachedToVisualTree = false;
        UpdateIsAnimationActive();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty || change.Property == ForegroundProperty)
        {
            // Create new IconImage so that Image.Draw is invoked
            Image = new IconImage(Value, Foreground ?? _fallbackForeground);
        }
        else if (change.Property == AnimationProperty)
        {
            UpdateIsAnimationActive();
        }
    }

    private static void UpdateAnimatedIcons()
    {
        // Collect first: updating an icon raises property changes, and their handlers may attach or detach icons.
        List<Icon>? changed = null;
        foreach (var icon in _animatedIcons)
        {
            if (icon.IsAnimationActive != icon.IsEffectivelyVisible)
            {
                (changed ??= new()).Add(icon);
            }
        }

        changed?.ForEach(icon => icon.UpdateIsAnimationActive());
    }

    private void UpdateIsAnimationActive()
    {
        var isAnimated = _isAttachedToVisualTree && Animation != IconAnimation.None;
        if (isAnimated)
        {
            _animatedIcons.Add(this);
        }
        else
        {
            _animatedIcons.Remove(this);
        }

        var isAnimationActive = isAnimated && IsEffectivelyVisible;
        if (IsAnimationActive != isAnimationActive)
        {
            IsAnimationActive = isAnimationActive;
        }
    }
}
