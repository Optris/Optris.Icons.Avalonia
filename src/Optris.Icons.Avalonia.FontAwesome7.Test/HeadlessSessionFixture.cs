using System;
using Avalonia;
using Avalonia.Headless;
using Xunit;

namespace Optris.Icons.Avalonia.FontAwesome7.Test;

/// <summary>
/// Runs tests on a headless Avalonia UI thread that renders with Skia. Every dispatched test gets its own
/// application instance.
/// </summary>
public sealed class HeadlessSessionFixture : IDisposable
{
    public HeadlessUnitTestSession Session { get; } = HeadlessUnitTestSession.StartNew(typeof(HeadlessSessionFixture));

    /// <summary>
    /// Entry point <see cref="HeadlessUnitTestSession"/> builds each test's application from.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<Application>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });

    public void Dispose() => Session.Dispose();
}

/// <summary>
/// Groups every test class that uses Avalonia. They share one <see cref="HeadlessSessionFixture"/>, because
/// Avalonia's dispatcher and services are process-wide, and xunit runs them one after another.
/// </summary>
[CollectionDefinition(Name)]
public sealed class HeadlessCollection : ICollectionFixture<HeadlessSessionFixture>
{
    public const string Name = "Headless";
}
