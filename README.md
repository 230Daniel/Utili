# Utili

Utili is a Discord bot with some useful utility features and nothing else.

A public, official instance can be found at [utili.xyz](https://utili.xyz). The publication of source code for this project means that anyone can run their own instance, but take care when selecting an instance as you are effectively granting admin privileges on your Discord server to the owner of that instance, and you are also placing trust in the security of their systems.

## Licensing

This project is licensed to you under the Apache 2.0 license, with the additional condition that you may not sell the software or derivative software. You should read the [LICENSE.txt](/LICENSE.txt) document to fully understand the licensing of the project.

Among other conditions, this means that it is not permitted to offer the premium subscription when self-hosting this software. Instead, the software will remain configured to enable premium features for all users.

## Contributing

Contributions are welcome, however I ask that you discuss any potential changes with me to avoid disappointment if I choose to reject the change. This is especially important when contributing entire features to the bot.

Read the full conditions surrounding contributing in the [LICENSE.txt](/LICENSE.txt) document before sending your contribution to me.

## The Disqord Library

This project makes heavy use of the [Disqord library](https://github.com/Quahu/Disqord), a Discord API wrapper for Dotnet.

Please note that it is licensed under the [GNU Lesser General Public License](https://github.com/Quahu/Disqord/blob/master/LICENSE), as stated in [DISQORD-LICENSE.txt](/DISQORD-LICENSE.txt).

I had a lot of fun working with this library, and its superior reliability saved Utili as the bot grew more popular. I highly recommend checking it out if you're looking to write a Discord bot in Dotnet.

## Running with Docker

The easiest way to host your own instance of Utili is using Docker.

You will need a Linux machine which runs 24/7 (eg. a VPS), a domain or subdomain, and an SSL certificate for that domain or subdomain. For SSL certificates, I recommend [Certbot](https://certbot.eff.org/) - it's free and convenient!

1. [Install Docker on a Linux machine](https://docs.docker.com/engine/install/#server).
2. Clone the repository: `git clone https://github.com/230Daniel/Utili`
3. Change directory into the repository root: `cd Utili`
4. Copy the example configuration files: `cp -r config-example config`
5. Modify the files in the `config` folder as per the instructions in the Configuration section below.
6. Start the containers: `docker compose up -d`
7. Monitor the container logs to check for any errors: `docker compose logs (bot|backend|postgres|nginx)`

### Configuration

#### bot.json

You must set the following values in the `bot.json` file:

 - `Discord:Token` - Your Discord bot's token
 - `Discord:OwnerId` - Your Discord account's user ID (not the bot)
 - `Services:WebsiteDomain` - The domain that your Utili website will be on (eg. `example.com` or `utili.example.com`)

#### backend.json

You must set the following values in the `backend.json` file:

 - `Frontend:Origin` - The address that the frontend will be on (eg. `https://example.com` or `https://utili.example.com`)
 - `Discord:ClientId` - Your Discord bot's client ID
 - `Discord:ClientSecret` - Your Discord bot's client secret
 - `Discord:Token` - Your Discord bot's token

#### frontend.js

You must set the following values in the `frontend.js` file:

 - `backend` - The address that the backend will be on (eg. `https://example.com/api`)
 - `clientId` - Your Discord bot's client ID

#### nginx.conf

 - Replace both `server_name` fields with the domain that your Utili website will be on (eg. `example.com` or `utili.example.com`)
 - Change the `ssl_certificate` and `ssl_certificate_key` fields to point to your SSL certificate. By default, your host machine's `/etc/letsencrypt` folder is accessible to the container. If you're not using Certbot, you'll need to modify `docker-compose.yaml` to make another folder accessible instead. You'll need to edit `services:nginx:volumes`. [More about Docker volumes](https://docs.docker.com/storage/volumes/).
