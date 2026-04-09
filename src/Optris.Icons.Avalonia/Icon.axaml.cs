using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

#nullable enable

namespace Optris.Icons.Avalonia
{
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

        private static readonly SolidColorBrush _fallbackForeground = new(Colors.Black);

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

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == ValueProperty || change.Property == ForegroundProperty)
            {
                // Create new IconImage so that Image.Draw is invoked
                Image = new IconImage(Value, Foreground ?? _fallbackForeground);
            }
        }
    }
}
