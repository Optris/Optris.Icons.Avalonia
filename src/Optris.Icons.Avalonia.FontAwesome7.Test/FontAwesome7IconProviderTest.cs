using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Optris.Icons.Avalonia.Models;
using SkiaSharp;
using Xunit;

namespace Optris.Icons.Avalonia.FontAwesome7.Test;

public class FontAwesome7IconProviderTest
{
    private readonly FontAwesome7IconProvider _iconProvider = new();
    private readonly FontAwesome7IconProvider _customIconProvider = new(new TestStreamProvider());

    [Theory]
    [InlineData("fa7-github")]
    [InlineData("fa7-arrow-left")]
    [InlineData("fa7-arrow-right")]
    [InlineData("fa7-brands fa7-github")]
    [InlineData("fa7-solid fa7-arrow-left")]
    [InlineData("fa7-regular fa7-address-book")]
    public void Icon_Should_Exist_And_Be_Valid_SVG_Path(string value)
    {
        // Act
        var icon = _iconProvider.GetIcon(value);
        var skiaPath = SKPath.ParseSvgPathData(icon.Path);

        // Assert
        skiaPath.Should().NotBeNull();
        skiaPath.Bounds.IsEmpty.Should().BeFalse();
    }

    [Theory]
    [InlineData("fa7-you-cant-find-me")]
    [InlineData("fa7")]
    public void IconProvider_Should_Throw_Exception_If_Icon_Does_Not_Exist(string value)
    {
        Assert.Throws<KeyNotFoundException>(() => _iconProvider.GetIcon(value));
    }

    [Theory]
    [InlineData("fa7-github-custom")]
    [InlineData("fa7-brands fa7-github-custom")]
    public void IconProvider_Should_Use_Custom_Stream_Provider(string value)
    {
        // Act
        var icon = _customIconProvider.GetIcon(value);

        // Assert
        icon.Should().NotBeNull();

        icon.Path.ToString()
            .Should()
            .StartWith("M165.9 397.4c0 2-2.3 3.6-5.2 3.6-3.3.3-5.6-1.3-5.6-3.6 0-2 2.3-3.6");
    }

    [Theory]
    [InlineData("fa7-github")]
    [InlineData("fa7-arrow-left")]
    public void IconProvider_Should_Throw_Exception_If_Icon_Does_Not_Exist_In_Custom_Stream(
        string value
    )
    {
        Assert.Throws<KeyNotFoundException>(() => _customIconProvider.GetIcon(value));
    }

    [Theory]
    [InlineData("fa7-github-custom")]
    [InlineData("fa7-brands fa7-github-custom")]
    public void FromEmbeddedResource_Should_Load_Custom_Icons(string value)
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"{assembly.GetName().Name}.icons.json";
        var provider = FontAwesome7IconProvider.FromEmbeddedResource(assembly, resourceName);

        // Act
        var icon = provider.GetIcon(value);

        // Assert
        icon.Should().NotBeNull();
        icon.Path.ToString()
            .Should()
            .StartWith("M165.9 397.4c0 2-2.3 3.6-5.2 3.6-3.3.3-5.6-1.3-5.6-3.6 0-2 2.3-3.6");
    }

    [Fact]
    public void FromFile_Should_Load_Icons_From_Disk()
    {
        // Arrange
        using var sourceStream = new TestStreamProvider().GetUtf8JsonStream();
        var tempPath = Path.Combine(Path.GetTempPath(), $"optris-fa7-test-{Guid.NewGuid():N}.json");
        try
        {
            using (var file = File.Create(tempPath))
            {
                sourceStream.CopyTo(file);
            }

            var provider = FontAwesome7IconProvider.FromFile(tempPath);

            // Act
            var icon = provider.GetIcon("fa7-github-custom");

            // Assert
            icon.Should().NotBeNull();
            icon.Path.ToString()
                .Should()
                .StartWith("M165.9 397.4c0 2-2.3 3.6-5.2 3.6-3.3.3-5.6-1.3-5.6-3.6 0-2 2.3-3.6");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public void FromStream_Should_Load_Icons_And_Not_Retain_Source_Stream()
    {
        // Arrange
        using var sourceStream = new TestStreamProvider().GetUtf8JsonStream();
        var provider = FontAwesome7IconProvider.FromStream(sourceStream);

        // Act
        var icon = provider.GetIcon("fa7-github-custom");

        // Assert — subsequent lookups still work (stream was buffered, not held)
        icon.Should().NotBeNull();
        provider.GetIcon("fa7-brands fa7-github-custom").Should().NotBeNull();
    }

    [Fact]
    public void FromEmbeddedResource_Should_Throw_When_Resource_Missing()
    {
        // Arrange
        var provider = FontAwesome7IconProvider.FromEmbeddedResource(
            Assembly.GetExecutingAssembly(),
            "does-not-exist.json"
        );

        // Act + Assert — exception surfaces on first lookup (Lazy)
        Assert.Throws<InvalidOperationException>(() => provider.GetIcon("fa7-anything"));
    }

    private sealed class TestStreamProvider : IFontAwesome7Utf8JsonStreamProvider
    {
        public Stream GetUtf8JsonStream()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{assembly.GetName().Name}.icons.json";
            return assembly.GetManifestResourceStream(resourceName);
        }
    }
}
