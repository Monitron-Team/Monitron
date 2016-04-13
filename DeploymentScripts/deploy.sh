#!/bin/env sh
pushd ..
echo Building monitron packages...
mdtool build Monitron.Packages/Monitron.mdproj
popd
echo Deploying Monitron...
ansible-playbook \
	--inventory-file ansible/monitron_inventory \
	--user root \
	--vault-password-file ansible/vault_password \
	--extra-vars @production_extra_vars.yml \
	ansible/site.yml

