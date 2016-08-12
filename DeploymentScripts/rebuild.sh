#!/bin/env sh
echo Gather Api Server Dependencies...
pushd ../Boty.Api/
npm install
popd
echo Building Frontend
pushd ../Boty.Frontend/
npm install
ember build --environment=production
popd
echo Build .Net bits
pushd ../
mdtool build Monitron.sln
popd
