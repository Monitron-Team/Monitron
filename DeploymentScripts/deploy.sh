#!/bin/env sh
echo Gather Api Server Dependencies...
pushd ../Boty.Api/
npm install
popd
echo Building Frontend
pushd ../Boty.Frontend/
ember build --environment=production
popd
echo Build .Net bits
pushd ../
mdtool build Monitron.sln
popd
echo Deploying Monitron...
ansible-playbook \
	--inventory-file ansible/production \
	--user root \
	--vault-password-file ansible/vault_password \
	--extra-vars @production_extra_vars.yml \
	ansible/site.yml
