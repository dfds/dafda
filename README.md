# Dafda

**TL;DR:** `Dafda` is a small Kafka client library for .NET.

## Documentation and Examples

See [dfds.github.io/dafda](https://dfds.github.io/dafda/) for more the documentation, or check the [Examples](https://github.com/dfds/dafda/tree/master/examples) folder in the repository.

## Building and Releasing

Dafda is built and released using [GitHub Actions](https://github.com/dfds/dafda/blob/master/.github/workflows/release.yml)

You will need the dotnet sdk for local development. Refer to the [Microsoft Documentation](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu) on how to install.

Dafda is available on [NuGet](https://www.nuget.org/packages/Dafda/).

### Releases

Releases are created from the **Dafda Release** workflow in GitHub Actions.

1. Open **Actions** in GitHub.
2. Run **Dafda Release**.
3. Enter the version to release (for example `1.2.3`).

The workflow will update `src/Dafda/Dafda.csproj`, create and push a tag, build/test/pack, publish to NuGet, and create a GitHub release.

> Note: The repository must define a `NUGET_API_KEY` secret for publishing to nuget.org.

### Documentation

Documentation is written in markdown, and compiled to a static site using [MkDocs](https://www.mkdocs.org/) and [Material for MkDocs](https://squidfunk.github.io/mkdocs-material/), and hosted on GitHub.

#### Development

```bash
cd docs
docker-compose up -d
```

Uses `docker-compose` to run `MkDocs` development server, which watches changes to `/docs` folder. The website is available on [`http://localhost:8000`](`http://localhost:8000`).

#### Release

```bash
cd docs
docker-compose run --rm mkdocs-deploy
```

Will build and deploy the static site to GitHub Pages.
