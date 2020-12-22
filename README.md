# Utili Version 2
Version 2 of the Utili discord bot

### Contributing
Please fork this repository, make your changes, and create a pull request which outlines your changes.

### Running
 - Add `https://www.myget.org/F/discord-net/api/v3/index.json` to your NuGet package sources.
 - Create a file called `DatabaseCredentials.json` in the running environment with this content:
```json
{
  "Server": "51.210.19.7",
  "Port": 28544,
  "Database": "Alpha",
  "Username": "alpha",
  "Password": "Wz8cS9RPKfkIV92q!D8wOO#DQQE%L3LO&Ing0*ow#qOsKzwXU5Cy$ALJXgB"
}
```

### Running Bot
 - Create a file called `Config.json` in the running environment with this content:
 ```json
 {
  "Token": "NzkwMjU0ODgwOTQ1NjAyNjAw.X998NQ.am727RNkGhlVmFjDSv1jdI6AoEE",
  "Production": false,
  "CacheDatabase": false,
  "LowerShardId": 0,
  "UpperShardId": 0,
  "Domain": "alpha.utili.xyz",
  "HasteServer": "https://p.utili.xyz",
  "SystemGuildId": 790255755524571157,
  "SystemChannelId": 791033505743765545,
  "StatusChannelId": 791033505743765545,
  "DefaultPrefix": "."
}
 ```
 
 ### Running Website
  - Create a file called `Config.json` in the running environment with this content:
 ```json
 {
  "DiscordClientId": "790254880945602600",
  "DiscordClientSecret": "crqBesin54gBRfk7GwXlNhkh3ke6k2h4",
  "DiscordToken": "NzkwMjU0ODgwOTQ1NjAyNjAw.X998NQ.am727RNkGhlVmFjDSv1jdI6AoEE",
  "StripePrivateKey": "",
  "StripePublicKey": "",
  "StripeWebhookSecret": ""
}
 ```
 > Note: The running environment for the website is in the same location as the `wwwroot` folder.
  - Change your debugging target from ISS Express to UtiliSite
