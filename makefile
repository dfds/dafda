PROJECT_FILE=${PWD}/src/Dafda/
CONFIGURATION=Debug
OUTPUT_DIR=${PWD}/.output

init: restore build

restore:
	cd src && dotnet restore

build:
	cd src && dotnet build

package:
	cd src && dotnet pack --no-build -c $(CONFIGURATION) -o $(OUTPUT_DIR) $(PROJECT_FILE)