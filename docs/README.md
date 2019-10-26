# API documentation

## Building

Documentation generated using [MkDocs](http://www.mkdocs.org/) with the [Material](https://squidfunk.github.io/mkdocs-material/) theme.

### Start development server on http://localhost:8000

```bash
# using docker-compose
docker-compose up

# using docker
docker run --rm -it -p 8000:8000 -v ${PWD}:/docs squidfunk/mkdocs-material
```

### Build documentation

```bash
docker run --rm -it -v ${PWD}:/docs squidfunk/mkdocs-material build
```

### Deploy documentation to GitHub Pages

```bash
docker run --rm -it -v ~/.ssh:/root/.ssh -v ${PWD}:/docs squidfunk/mkdocs-material gh-deploy
```
