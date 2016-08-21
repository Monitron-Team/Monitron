.PHONY: getdeps build

all: build

build: getdeps
	ember build --environment=production

getdeps:
	npm install

serve:
	ember serve
