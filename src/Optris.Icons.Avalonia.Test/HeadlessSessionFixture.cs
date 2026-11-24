using System;
using System.Collections;
using System.Reflection;
using Avalonia;
using Avalonia.Headless;

namespace Optris.Icons.Avalonia.Test;

/// <summary>
/// Runs tests on a headless Avalonia UI thread. Every dispatched test gets its own application instance.
/// </summary>
public sealed class HeadlessSessionFixture : IDisposable
{
    public HeadlessUnitTestSession Session { get; } = HeadlessUnitTestSession.StartNew(typeof(Application));

    public void Dispose() => Session.Dispose();
}

/// <summary>
/// Observes Avalonia's global animation clock, the clock owned by the internal <c>MediaContext</c>.
/// While it has subscribers, <c>MediaContext</c> schedules a UI-thread render pass about every 16 ms,
/// even if every subscribed animation is paused. Avalonia has no public API for this, so the tests read it
/// through reflection; the lookup fails loudly if a future Avalonia version renames these internals.
/// </summary>
internal static class GlobalAnimationClock
{
    private const BindingFlags AnyMember =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public static int SubscriberCount
    {
        get
        {
            var mediaContextType = typeof(Visual).Assembly.GetType("Avalonia.Media.MediaContext", throwOnError: true);
            var mediaContext = mediaContextType.GetProperty("Instance", AnyMember)?.GetValue(null)
                ?? throw Missing("MediaContext.Instance");
            var clock = mediaContextType.GetField("_clock", AnyMember)?.GetValue(mediaContext)
                ?? throw Missing("MediaContext._clock");
            var observers = clock.GetType().GetField("_observers", AnyMember)?.GetValue(clock) as ICollection
                ?? throw Missing("MediaContextClock._observers");
            return observers.Count;
        }
    }

    private static InvalidOperationException Missing(string member) =>
        new($"Avalonia internal '{member}' was not found. Update {nameof(GlobalAnimationClock)} for this Avalonia version.");
}
