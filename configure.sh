#!/bin/bash

# Interactive script to replace placeholders in config-example.

main()
{
	rm -r config
	cp -r config-example config

	echo "Enter your Discord application's client ID (eg. 1070762880791761116)"
	read -p "> " CLIENT_ID

	echo "Enter your Discord application's client secret (eg. 9kct-VSO8dtOTRSnY2l1Ahwzi8-Fq5Zi)"
	read -p "> " CLIENT_SECRET

	echo "Enter your Discord bot's token (eg. MTA3MDc2Mjg4MDc5MTc2MTExNg.GZY5IY.PGcey53122fg8UrXsH6I5QonI2q-BzJzRsZeug)"
	read -p "> " TOKEN

	echo "Enter the default Discord command prefix (eg. !)"
	read -p "> " DEFAULT_PREFIX

	echo "Enter your domain name (eg. utili.example.com)"
	read -p "> " DOMAIN

	echo "\n\nPlease check these details carefully.\n"

	echo "Client ID:       ${CLIENT_ID}"
	echo "Client secret:   ${CLIENT_SECRET}"
	echo "Token:           ${TOKEN}"
	echo "Default prefix:  ${DEFAULT_PREFIX}"
	echo "Domain:          ${DOMAIN}"

	read -p "Are these details correct? (Y/N) " CONFIRMATION

	if [ $CONFIRMATION != "Y" ] && [ $CONFIRMATION != "y" ]; then
		echo "Aborting."
		exit 1
	fi

	echo "Writing changes..."

	replace_config "{CLIENT_ID}" $CLIENT_ID
	replace_config "{CLIENT_SECRET}" $CLIENT_SECRET
	replace_config "{TOKEN}" $TOKEN
	replace_config "{DEFAULT_PREFIX}" $DEFAULT_PREFIX
	replace_config "{DOMAIN}" $DOMAIN

	echo "Your changes have been saved."
}


replace_config()
{
	find config -type f -exec sed -i 's/${1}/$2/g' {} \;
}


main
