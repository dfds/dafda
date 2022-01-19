# Dafda

**TL;DR:** `Dafda` is a small Kafka client library for .NET.

## Documentation and Examples

See [dfds.github.io/dafda](https://dfds.github.io/dafda/) for more the documentation, or check the [Examples](https://github.com/dfds/dafda/tree/master/examples) folder in the repository.

## Building and Releasing

Dafda is build and released using a combination of `make` and [GitHub Actions](https://github.com/dfds/dafda/blob/master/.github/workflows/release.yml)

You will need the dotnet sdk. Refer to the [Microsoft Documentation](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu) on how to install

Dafda is available on [NuGet](https://www.nuget.org/packages/Dafda/).

### Versioning

Run:

```bash
make version
```

And input the new version of Dafda. This will update the `Dafda.csproj` with the new version.

### NuGet Packages

Run:

```bash
make release
git push --follow-tags
```

Will git tag with the current version (see [Versioning](#versioning), and GitHub Actions will take care of building, and pushing to NuGet.

### Documentation

Documentation is written in markdown, and compiled to a static site using [MkDocs](https://www.mkdocs.org/) and [Material for MkDocs](https://squidfunk.github.io/mkdocs-material/), and hosted on GitHub.

#### Development

```bash
make docs-dev
```

Uses `docker-compose` to run `MkDocs` development server, which watches changes to `/docs` folder. The website is available on [`http://localhost:8000`](`http://localhost:8000`).

#### Release

```bash
make docs-deploy
```

Will build and deploy the static site to GitHub Pages.
