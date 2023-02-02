#!/bin/bash

# Interactive script to replace placeholders in config-example.

main()
{
	echo "Enter your Discord application's client ID (eg. 1070762880791761116)"
	read -p "> " CLIENT_ID

	echo "Enter your Discord application's client secret (eg. 9kct-VSO8dtOTRSnY2l1Ahwzi8-Fq5Zi)"
	read -p "> " CLIENT_SECRET

	echo "Enter your Discord bot's token (eg. MTA3MDc2Mjg4MDc5MTc2MTExNg.GZY5IY.PGcey53122fg8UrXsH6I5QonI2q-BzJzRsZeug)"
	read -p "> " TOKEN

	echo "Enter your Discord user ID (eg. 218613903653863427)"
	read -p "> " OWNER_ID

	echo "Enter your domain name (eg. utili.example.com)"
	read -p "> " DOMAIN

	echo "Enter the default Discord command prefix (eg. !)"
	read -p "> " DEFAULT_PREFIX

	echo -e "\n\nPlease check these details carefully.\n"

	echo "Client ID:       ${CLIENT_ID}"
	echo "Client secret:   ${CLIENT_SECRET}"
	echo "Token:           ${TOKEN}"
	echo "Owner ID:        ${OWNER_ID}"
	echo "Domain:          ${DOMAIN}"
	echo -e "Default prefix:  ${DEFAULT_PREFIX}\n"

	read -p "Are these details correct? (Y/N) " CONFIRMATION

	if [ $CONFIRMATION != "Y" ] && [ $CONFIRMATION != "y" ]; then
		echo -e "\n\nAborting."
		exit 1
	fi

	echo -e "\n\nWriting changes..."

	rm -rf config > /dev/null
	cp -r config-example config

	replace_config "{CLIENT_ID}" ${CLIENT_ID}
	replace_config "{CLIENT_SECRET}" ${CLIENT_SECRET}
	replace_config "{TOKEN}" ${TOKEN}
	replace_config "{OWNER_ID}" ${OWNER_ID}
	replace_config "{DEFAULT_PREFIX}" ${DEFAULT_PREFIX}
	replace_config "{DOMAIN}" ${DOMAIN}

	echo "Your changes have been saved."
}


replace_config()
{
	find config -type f -exec sed -i "s/${1}/${2}/g" {} \;
}


main
