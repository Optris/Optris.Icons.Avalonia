using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Media;
using FluentAssertions;
using Xunit;

namespace Optris.Icons.Avalonia.Test;

public enum HideMethod
{
    IconIsVisible,
    AncestorIsVisible,
    RemoveFromVisualTree,
}

[Collection(HeadlessCollection.Name)]
public class IconAnimationVisibilityTest
{
    // Generous, because a busy CI machine can delay render passes. Passing tests stop waiting as soon as they can.
    private static readonly TimeSpan AnimatingTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan StoppedTimeout = TimeSpan.FromSeconds(5);

    private readonly HeadlessUnitTestSession _session;

    public IconAnimationVisibilityTest(HeadlessSessionFixture fixture)
    {
        _session = fixture.Session;
    }

    [Theory]
    [InlineData(IconAnimation.Spin, HideMethod.IconIsVisible)]
    [InlineData(IconAnimation.Spin, HideMethod.AncestorIsVisible)]
    [InlineData(IconAnimation.Spin, HideMethod.RemoveFromVisualTree)]
    [InlineData(IconAnimation.Pulse, HideMethod.IconIsVisible)]
    [InlineData(IconAnimation.Pulse, HideMethod.AncestorIsVisible)]
    [InlineData(IconAnimation.Pulse, HideMethod.RemoveFromVisualTree)]
    public Task Hidden_Icon_Should_Stop_Animating_Until_Shown_Again(IconAnimation animation, HideMethod hideMethod)
    {
        return RunWithWindow(animation, iconIsVisible: true, async tree =>
        {
            await WaitUntilAnimating(tree.Icon);

            tree.SetShown(hideMethod, false);
            await WaitUntil(() => GlobalAnimationClock.SubscriberCount == 0, StoppedTimeout);
            GlobalAnimationClock.SubscriberCount.Should().Be(0, "a hidden icon must not keep the render loop running");
            var hiddenAngle = GetAngle(tree.Icon);
            await Task.Delay(300);
            GetAngle(tree.Icon).Should().Be(hiddenAngle, "a hidden icon must not be animated");

            tree.SetShown(hideMethod, true);
            await WaitUntilAnimating(tree.Icon);
        });
    }

    [Theory]
    [InlineData(IconAnimation.Spin)]
    [InlineData(IconAnimation.Pulse)]
    public Task Icon_Created_Hidden_Should_Not_Animate_Until_Shown(IconAnimation animation)
    {
        return RunWithWindow(animation, iconIsVisible: false, async tree =>
        {
            await Task.Delay(300);
            GlobalAnimationClock.SubscriberCount.Should().Be(0, "a hidden icon must not keep the render loop running");
            GetAngle(tree.Icon).Should().Be(0);

            tree.Icon.IsVisible = true;
            await WaitUntilAnimating(tree.Icon);
        });
    }

    [Theory]
    [InlineData(IconAnimation.Spin)]
    [InlineData(IconAnimation.Pulse)]
    public Task Animation_Set_On_Shown_Icon_Should_Run_Until_Cleared(IconAnimation animation)
    {
        return RunWithWindow(IconAnimation.None, iconIsVisible: true, async tree =>
        {
            await Task.Delay(100);
            GlobalAnimationClock.SubscriberCount.Should().Be(0, "an icon without animation must not use the animation clock");

            tree.Icon.Animation = animation;
            await WaitUntilAnimating(tree.Icon);

            tree.Icon.Animation = IconAnimation.None;
            await WaitUntil(() => GlobalAnimationClock.SubscriberCount == 0, StoppedTimeout);
            GlobalAnimationClock.SubscriberCount.Should().Be(0, "a cleared animation must not keep the render loop running");
            GetAngle(tree.Icon).Should().Be(0);
        });
    }

    [Fact]
    public Task Visible_Spin_Should_Rotate_Continuously()
    {
        return RunWithWindow(IconAnimation.Spin, iconIsVisible: true, async tree =>
        {
            var angles = await SampleDistinctAngles(tree.Icon, count: 5);

            angles.Should().HaveCount(5, "Spin keeps rotating");
            angles.Should().OnlyContain(angle => angle >= 0 && angle <= 360);
            angles.Should().Contain(angle => angle % 45 != 0, "Spin rotates smoothly, not in steps");
        });
    }

    [Fact]
    public Task Visible_Pulse_Should_Rotate_In_Eight_Steps()
    {
        return RunWithWindow(IconAnimation.Pulse, iconIsVisible: true, async tree =>
        {
            var angles = await SampleDistinctAngles(tree.Icon, count: 3);

            angles.Should().HaveCount(3, "Pulse keeps rotating");
            angles.Should().OnlyContain(angle => angle % 45 == 0, "Pulse rotates in steps of 360 / 8 degrees");
        });
    }

    private Task RunWithWindow(IconAnimation animation, bool iconIsVisible, Func<TestTree, Task> test)
    {
        return _session.Dispatch<int>(async () =>
        {
            var tree = TestTree.Show(animation, iconIsVisible);
            try
            {
                await test(tree);
            }
            finally
            {
                tree.Window.Close();
            }

            return 0;
        }, CancellationToken.None);
    }

    private static double GetAngle(Icon icon)
    {
        var transform = (TransformGroup)icon.RenderTransform;
        return transform.Children.OfType<RotateTransform>().Single().Angle;
    }

    private static async Task WaitUntilAnimating(Icon icon)
    {
        var startAngle = GetAngle(icon);
        var animating = await WaitUntil(
            () => GlobalAnimationClock.SubscriberCount > 0 && GetAngle(icon) != startAngle,
            AnimatingTimeout);
        animating.Should().BeTrue("a visible icon must be animated");
    }

    private static async Task<bool> WaitUntil(Func<bool> condition, TimeSpan timeout)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!condition())
        {
            if (stopwatch.Elapsed > timeout)
            {
                return false;
            }

            await Task.Delay(10);
        }

        return true;
    }

    private static async Task<HashSet<double>> SampleDistinctAngles(Icon icon, int count)
    {
        var angles = new HashSet<double>();
        await WaitUntil(
            () =>
            {
                angles.Add(GetAngle(icon));
                return angles.Count >= count;
            },
            AnimatingTimeout);
        return angles;
    }

    private sealed record TestTree(Window Window, Border Host, Border Parent, Icon Icon)
    {
        public static TestTree Show(IconAnimation animation, bool iconIsVisible)
        {
            var icon = new Icon { Animation = animation, IsVisible = iconIsVisible };
            var parent = new Border { Child = icon };
            var host = new Border { Child = parent };
            var window = new Window
            {
                Content = host,
                // No theme is loaded, so the window gets a minimal template that presents its content.
                Template = new FuncControlTemplate<Window>((owner, _) => new ContentPresenter
                {
                    Name = "PART_ContentPresenter",
                    [!ContentPresenter.ContentProperty] = owner[!ContentControl.ContentProperty],
                }),
            };
            window.Show();
            return new TestTree(window, host, parent, icon);
        }

        public void SetShown(HideMethod hideMethod, bool shown)
        {
            switch (hideMethod)
            {
                case HideMethod.IconIsVisible:
                    Icon.IsVisible = shown;
                    break;
                case HideMethod.AncestorIsVisible:
                    Parent.IsVisible = shown;
                    break;
                case HideMethod.RemoveFromVisualTree:
                    Host.Child = shown ? Parent : null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(hideMethod), hideMethod, null);
            }
        }
    }
}
