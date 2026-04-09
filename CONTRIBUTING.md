# Contributing to Optris.Icons.Avalonia

Thank you for your interest in contributing! This guide covers how to set up the project for development, run tests, and submit changes.

## Prerequisites

- [.NET SDK 10+](https://dotnet.microsoft.com/download) (see [global.json](global.json) for the exact version; rollForward is set to `latestFeature`)
- The solution also targets .NET 8.0 and .NET 9.0

## Project structure

```
src/
  Optris.Icons.Avalonia/                  Core library
  Optris.Icons.Avalonia.FontAwesome/      Font Awesome 6 Free icon provider
  Optris.Icons.Avalonia.MaterialDesign/   Material Design icon provider
  Optris.Icons.Avalonia.Test/             Core tests
  Optris.Icons.Avalonia.FontAwesome.Test/ Font Awesome tests
  Optris.Icons.Avalonia.MaterialDesign.Test/ Material Design tests
  Demo/                                   Shared demo UI
  Demo.Desktop/                           Desktop demo app
  Demo.Browser/                           Browser/WASM demo app
tools/
  IconGenerator/                          Icon generation tooling
```

## Building

```bash
dotnet build src/
```

## Running tests

```bash
dotnet test src/
```

## Running the demo

```bash
dotnet run --project src/Demo.Desktop
```

The browser demo is deployed to [GitHub Pages](https://optris.github.io/Optris.Icons.Avalonia/) automatically. To test it locally:

```bash
dotnet run --project src/Demo.Browser
```

## Code style

The repository includes an [.editorconfig](.editorconfig) file. Please follow the conventions defined there.

## Submitting changes

1. Fork the repository and create a feature branch from `main`.
2. Make your changes with clear, descriptive commits.
3. Ensure all tests pass (`dotnet test src/`).
4. Open a pull request describing what you changed and why.

## For maintainers

### Releasing a new version

The major version tracks Avalonia (e.g. `12.x.y` for Avalonia 12). Minor and patch versions are for library changes.

1. Tag the release: `git tag -a v12.0.1 -m "v12.0.1"`
2. Push the tag: `git push origin v12.0.1`

The [release workflow](.github/workflows/release.yml) extracts the version from the tag, packs all three NuGet packages, and publishes them to nuget.org. No need to edit version numbers in source files.

### Icon sync

Font Awesome and Material Design icon sets are automatically kept up to date via scheduled GitHub Actions workflows:

- [sync-fontawesome.yml](.github/workflows/sync-fontawesome.yml) syncs from the Font Awesome 6.x repository
- [sync-materialdesign.yml](.github/workflows/sync-materialdesign.yml) syncs from the Material Design SVG repository

Both workflows create pull requests automatically when icon changes are detected.

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
