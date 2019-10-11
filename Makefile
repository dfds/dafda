PACKAGES		:= Dafda Dafda.AspNetCore
CONFIGURATION	:= Debug
VERSION			?= $(shell git describe --tags --always --dirty --match=*.*.* 2> /dev/null | sed -E 's/-(.+)-.+/-beta.\1/' || cat $(CURDIR)/.version 2> /dev/null || echo 0.0.1)
NUGET_API_KEY	?= $(shell git config --global nuget.token)
BIN				:= $(CURDIR)/.output
M				= $(shell printf "\033[34;1m▶\033[0m")

.PHONY: all
all: build

.PHONY: init
init: restore build ## restore, build

.PHONY: clean
clean: ; $(info $(M) Cleaning...) @ ## clean the build artifacts
	@rm -rf $(BIN)

.PHONY: restore
restore: ; $(info $(M) Restoring dependencies...) @ ## restore project dependencies
	@cd src && dotnet restore

.PHONY: build
build: ; $(info $(M) Building...) @ ## build the project
	@cd src && dotnet build --configuration $(CONFIGURATION)

.PHONY: package
package: $(addprefix package-,$(subst /,-,$(PACKAGES))) ## create nuget packages

package-%: ; $(info $(M) Packing $*...)
	@cd src && dotnet pack --no-build --configuration $(CONFIGURATION) -p:PackageVersion=$(VERSION) --output $(BIN) $(CURDIR)/src/$*/

.PHONY: local-release
local-release: clean restore build package ## create a nuget package for local development

.PHONY: release
release: CONFIGURATION=Release ## create a release nuget package
release: clean restore build package

.PHONY: push
push: $(addprefix push-,$(subst /,-,$(PACKAGES))) ## push nuget packages

push-%: ; $(info $(M) Pushing $*...)
	cd $(BIN) && dotnet nuget push $*.$(VERSION).nupkg --source https://api.nuget.org/v3/index.json --api-key $(NUGET_API_KEY)

.PHONY: version
version: ## prints the version (from either environment VERSION, git describe, or .version. default: 0.0.1)
	@echo $(VERSION)

.PHONY: help
help:
	@grep -E '^[ a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | \
		awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-15s\033[0m %s\n", $$1, $$2}'
