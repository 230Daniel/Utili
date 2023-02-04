# Utili

Utili is a Discord bot with some useful utility features and nothing else.

A public, official instance can be found at [utili.xyz](https://utili.xyz). The publication of source code for this project means that anyone can run their own instance, but take care when selecting an instance as you are effectively granting admin privileges on your Discord server to the owner of that instance, and you are also placing trust in the security of their systems.


## Self-Hosting

Self-hosting Utili has been made easier than ever with Docker! Follow the self-hosting guide [here](/SelfHosting.md) to get started. Don't be put off by the guide's length, it's very verbose because it assumes minimal technical knowledge!

You will need:

 - A virtual server which runs 24/7 (at least 1 CPU core, 2GB memory).
 - A domain or subdomain which points to your virtual server.
 - An hour or two of your time, depending on your technical knowledge of Linux.


## Licensing

This project is licensed to you under the Apache 2.0 license, with the additional condition that you may not sell the software or derivative software. You should read the [LICENSE.txt](/LICENSE.txt) document to fully understand the licensing of the project.

Among other conditions, this means that it is not permitted to offer the premium subscription when self-hosting this software. Instead, the software will be configured to enable premium features for all users.


## Contributing

Contributions are welcome, however I ask that you discuss any potential changes with me to avoid disappointment if I choose to reject the change. This is especially important when contributing entire features to the bot.

Read the full conditions surrounding contributing in the [LICENSE.txt](/LICENSE.txt) document before sending your contribution to me.


## The Disqord Library

This project makes heavy use of the [Disqord library](https://github.com/Quahu/Disqord), a Discord API wrapper for Dotnet.

Please note that it is licensed under the [GNU Lesser General Public License](https://github.com/Quahu/Disqord/blob/master/LICENSE), as stated in [DISQORD-LICENSE.txt](/DISQORD-LICENSE.txt).

I had a lot of fun working with this library, and its superior reliability saved Utili as the bot grew more popular. I highly recommend checking it out if you're looking to write a Discord bot in Dotnet.
