PROJECT_FILE=${PWD}/src/Dafda/
CONFIGURATION=Debug
OUTPUT_DIR=${PWD}/.output
PRERELEASE_TAG=ci-$(shell date +%s)

init: restore build

restore:
	cd src && dotnet restore

build:
	cd src && dotnet build

package:
	cd src && dotnet pack --version-suffix "$(PRERELEASE_TAG)" --no-build -c $(CONFIGURATION) -o $(OUTPUT_DIR) $(PROJECT_FILE)

local-release: restore build package

release: PRERELEASE_TAG=
release: restore build package