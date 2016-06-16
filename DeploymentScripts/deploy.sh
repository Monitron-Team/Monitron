#!/bin/env sh
echo Building Frontend
pushd ../Boty.Frontend/
ember build --environment=production
popd
echo Deploying Monitron...
ansible-playbook \
	--inventory-file ansible/production \
	--user root \
	--vault-password-file ansible/vault_password \
	--extra-vars @production_extra_vars.yml \
	ansible/site.yml

