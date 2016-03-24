#!/bin/env sh
pushd ..
echo Building monitron packages...
mdtool build Monitron.Packages/Monitron.mdproj
popd
echo Deploying Monitron...
ansible-playbook \
	--inventory-file monitron_inventory \
	--user root \
	--vault-password-file vault_password \
	--extra-vars @monitron_release_extra_vars.yml \
	master_playbook.yml

