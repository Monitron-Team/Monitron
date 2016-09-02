ifeq ("$(EMBER_ENV)", "")
EMBER_ENV=production
endif

.PHONY: getdeps build

all: build

build: getdeps
	ember build --environment=$(EMBER_ENV) --output-path=dist/$(EMBER_ENV)

getdeps:
	npm install

serve:
	ember serve
