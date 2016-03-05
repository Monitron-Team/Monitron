#!/bin/env sh
ansible-playbook -i monitron_inventory -u root --vault-password-file ./vault_password deploy.yml
