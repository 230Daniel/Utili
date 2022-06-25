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

## Self-Hosting

Full instructions on how to self-host this project are in the works, and I may eventually endeavour to create a solution to make this easier, such as a Docker image. (If someone is an expert with Docker images, this would be a great contribution)

For now, you need to:

 - Run a Postgresql server. I run version 14, but higher versions probably work. At runtime the database will be updated automatically to the correct schema. Avoid starting both the Backend and Bot at the same time if the database needs to update.
 - For both Utili.Backend and Utili.Bot, make an appsettings.json with the contents of appsettings.example.json, and configure the settings. Point both the backend and the bot to the same database.
 - Compile Utili.Backend and Utili.Bot (eg. `dotnet publish -r linux-x64 -c release`)
 - Run Utili.Backend and Utili.Bot 24/7, Systemd works well with the `Notify` type.
 - Use Nginx or similar to reverse-proxy traffic to the local addresses (defaults are `https://localhost:5001` and `http://localhost:5000`)
 - Build the frontend with `yarn install` then `yarn build`
 - TODO: Instruct on how to modify backend url
 - Serve the files in the `/build` directory with a basic file server like Nginx.

If you have any issues, don't hesitate to reach out to me. A Github issue would be good for this, as it would then stick around for future reference. For now though, please don't ask me to walk you through the entire process above - you should instead wait for the full instructions to be published.
