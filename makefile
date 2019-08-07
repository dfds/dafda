PROJECT_FILE=${PWD}/src/Dafda/
CONFIGURATION=Debug
OUTPUT_DIR=${PWD}/.output
PRERELEASE_TAG=ci-$(shell date +%s)

SUBDIRS := $(wildcard *.nupkg)

init: restore build

clean:
	rm -rf $(OUTPUT_DIR)

restore:
	cd src && dotnet restore

build:
	cd src && dotnet build

package:
	cd src && dotnet pack --version-suffix "$(PRERELEASE_TAG)" --no-build -c $(CONFIGURATION) -o $(OUTPUT_DIR) $(PROJECT_FILE)

local-release: clean restore build package

release: PRERELEASE_TAG=
release: clean restore build package

push: require
	cd $(OUTPUT_DIR) && dotnet nuget push *.nupkg --source https://api.nuget.org/v3/index.json --api-key $(NUGET_API_KEY)

require:
ifndef NUGET_API_KEY
	$(error NUGET_API_KEY is undefined)
endif

.PHONY: require push