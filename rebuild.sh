#!/bin/env bash
project_dir=$(dirname "${BASH_SOURCE[0]}")
cd "${project_dir}"
mdtool build -c:Release ./Monitron.sln
