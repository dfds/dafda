PACKAGE			:= Dafda
PROJECT			:= $(CURDIR)/src/$(PACKAGE)/$(PACKAGE).csproj
CONFIGURATION	:= Debug
NUGET_API_KEY	?= $(shell git config --global nuget.token)
BIN				:= $(CURDIR)/.output

.PHONY: all
all: build

.PHONY: init
init: restore build ## restore, build

.PHONY: clean
clean: ; $(info > Cleaning...) @ ## clean the build artifacts
	@rm -rf $(BIN)

.PHONY: restore
restore: ; $(info > Restoring dependencies...) @ ## restore project dependencies
	@cd src && dotnet restore

.PHONY: build
build: ; $(info > Building...) @ ## build the project
	@cd src && dotnet build --configuration $(CONFIGURATION)

.PHONY: package
package: clean restore build ; $(info > Packing $(PACKAGE)...) @ ## create the nuget package
	@cd src && dotnet pack --no-build --configuration $(CONFIGURATION) \
		--output $(BIN) \
		$(PROJECT)

.PHONY: push
push: ; $(info > Pushing $(PACKAGE)...) @ ## push the nuget package to nuget.org
	dotnet nuget push $(BIN)/$(PACKAGE).*.nupkg --source https://api.nuget.org/v3/index.json --api-key $(NUGET_API_KEY)

.PHONY: version
version: ## set the package version base on user input
	@echo "----------------------------------------------"
	@echo "- This action will update the:"
	@echo "- $(PROJECT)"
	@echo "- and create a new commit"
	@echo "----------------------------------------------"
	@echo
	@echo "Set the version of $(PACKAGE)"
	@echo "  Current version:   $$(sed -n -E 's,[[:blank:]]+<Version>(.+)</Version>,\1,p' $(PROJECT))"
	@read -p "  Enter new version: " version \
		&& sed -E "s,<Version>(.+)</Version>,<Version>$${version}</Version>," $(PROJECT) > $(PROJECT).new \
		&& mv $(PROJECT).new $(PROJECT) \
		&& git add $(PROJECT) \
		&& git commit -m "update version to $${version}" 1>/dev/null
	@echo
	@echo "Version updated. Run e.g. 'make release' to tag according to latest version"

.PHONY: release
release: ## tag a release with latest version
	@case $$(uname) in \
		'Darwin'|'Linux') \
			git tag $$(sed -n -E 's,[[:blank:]]+<Version>(.+)</Version>,\1,p' $(PROJECT) | tr -d '\r') \
			;; \
		*) \
			git tag $$(sed -n -E 's,[[:blank:]]+<Version>(.+)</Version>,\1,p' $(PROJECT)) \
			;; \
	esac
	@echo
	@echo "----------------------------------------------"
	@echo "- !! CAUTION !!"
	@echo "-"
	@echo "- The lastest version was tagged in git"
	@echo "----------------------------------------------"
	@echo
	@echo "Run e.g. 'git push --follow-tags' to release"

docs-dev: ## edit documentation
	@cd docs && docker-compose up -d

docs-deploy: ## deploy documentation to github pages
	@docker run --rm -it \
		-v ~/.ssh:/root/.ssh \
		-v ${PWD}:/docs \
		squidfunk/mkdocs-material:4.1.2 \
		gh-deploy --clean --config-file docs/mkdocs.yml

.PHONY: help
help:
	@grep -E '^[ a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | \
		awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-15s\033[0m %s\n", $$1, $$2}'
