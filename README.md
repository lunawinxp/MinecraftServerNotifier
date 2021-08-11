# MinecraftServerNotifier
A simple program that updates you on the status of a Minecraft server through Discord, including when the server goes offline/online, and player log ins/outs, including their usernames.
# Pictures
![Image of the notifications](https://i.imgur.com/TjwIWw7.png)
# How does it work?
You simply enter the servers you would like to monitor, and a Discord webhook for the notifications to go to.\
Example configuration file (config.json):
```
{
	"pollInterval": 10, // How often in seconds it should check the minecraft server's status
	"webhookUrl": "https://discord.com/api/webhooks/", // Your discord webhook URL goes here, this is where your server reports will go
	
	"servers": [
		{ "Name": "My SMP Minecraft Server", "IP": "1.1.1.3", "Port": 25565 }, // The only thing needed is "IP", but you can also manually specify a server name ("Name") and/or a specific port ("Port")
		{ "Name": "My Friends Minecraft Server", "IP": "1.1.1.4", "Port": 25565 }, // Can have multiple servers, however many you want.
	]
}
```
# Thanks
This program uses the following open-source projects:\
[N4T4NM's CSharpDiscordWebhook](https://github.com/N4T4NM/CSharpDiscordWebhook) - For posting to the Discord webhook, this could be removed and sent with just pure HTTP requests but, convenience.\
[FragLand's MineStat](https://github.com/FragLand/minestat) - For getting basic information from the Minecraft servers, although it has been (badly) slightly modified to allow the return of the player list.
[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
