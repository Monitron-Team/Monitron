#!/bin/env sh
echo Gather Api Server Dependencies...
pushd ../Boty.Api/
make
popd
echo Building Frontend
pushd ../Boty.Frontend/
make
popd
echo Build .Net bits
pushd ../
mdtool build Monitron.sln
popd
