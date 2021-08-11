using Newtonsoft.Json.Linq;
using MineStatLib;
using System;
using System.Collections.Generic;
using System.Threading;
using Discord;
using Discord.Webhook;
using System.Drawing;
using System.IO;

namespace MinecraftServerNotifier
{
    class Program
    {
        public static Dictionary<string, ServerInfo> servers = new Dictionary<string, ServerInfo>();

        public static int pollInterval = 10; // default value incase not specified.
        public static string webhookUrl = "";

        static void Main(string[] args)
        {
            // start config stuff
            if (File.Exists("config.json"))
            {
                dynamic configJson = JObject.Parse(File.ReadAllText("config.json"));
                if (configJson.webhookUrl != null)
                {
                    webhookUrl = (string)configJson.webhookUrl;
                    if (configJson.pollInterval != null) { pollInterval = (int)configJson.pollInterval; }
                    if (configJson.servers != null)
                    {
                        int serverCount = 0; // just incase name not specified, counter babyyy
                        foreach (var server in configJson.servers)
                        {
                            serverCount++;
                            ServerInfo serverToAdd = new ServerInfo();
                            if (server.IP != null)
                            {
                                serverToAdd.IP = (string)server.IP;
                                if (server.Port != null) { serverToAdd.Port = (ushort)server.Port; }
                                if (server.Name != null) { servers.Add((string)server.Name, serverToAdd); } else { servers.Add("Minecraft server " + serverCount, serverToAdd); } // add to the dictionary of servers
                            }
                            else
                            {
                                Console.WriteLine("Server #" + serverCount + " didn't specify an IP, skipping...");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Please specify server(s) in your config.json!");
                    }
                }
                else
                {
                    Console.WriteLine("Please specify a webhookUrl in your config.json!");
                }
            }
            else
            {
                Console.WriteLine("config.json doesn't exist! Please create this.");
            }
            // end of config

            if (servers.Count == 0)
            {
                Console.WriteLine("You didn't specify any valid servers! Closing...");
            }
            else
            {
                while (true)
                {
                    try
                    {
                        foreach (var itm in servers)
                        {
                            Console.WriteLine("Checking: " + itm.Key);
                            MineStat ms = new MineStat(itm.Value.IP, itm.Value.Port, protocol: SlpProtocol.Json);
                            if (ms.ServerUp)
                            {
                                DiscordWebhook hook = new DiscordWebhook
                                {
                                    Url = webhookUrl
                                };
                                DiscordEmbed embed = new DiscordEmbed();
                                DiscordMessage message = new DiscordMessage();
                                embed.Title = itm.Key + " update!";
                                embed.Timestamp = DateTime.Now;
                                embed.Color = Color.Pink;
                                embed.Footer = new EmbedFooter() { Text = "Minecraft Server notifier" };
                                string totalMessage = "";

                                // check if server has come back online
                                if (itm.Value.IsOffline)
                                {
                                    totalMessage = totalMessage + itm.Key + " is now **online**!\n";
                                }


                                // check if there are more players in current query than we have cached (log ins)
                                foreach (string player in ms.CurrentPlayersList.Split(','))
                                {
                                    bool hasFound = false;
                                    foreach (string playerRn in itm.Value.CurrentPlayersList.Split(','))
                                    {
                                        if (playerRn == player)
                                        {
                                            hasFound = true;
                                            break;
                                        }
                                    }
                                    if (!hasFound)
                                    {
                                        totalMessage = totalMessage + player + " has logged **IN** to " + itm.Key + "!\n";
                                    }
                                }

                                // check if there are more players in our cache than in the current query (log outs)
                                foreach (string player in itm.Value.CurrentPlayersList.Split(','))
                                {
                                    bool hasFound = false;
                                    foreach (string playerRn in ms.CurrentPlayersList.Split(','))
                                    {
                                        if (playerRn == player)
                                        {
                                            hasFound = true;
                                            break;
                                        }
                                    }
                                    if (!hasFound)
                                    {
                                        totalMessage = totalMessage + player + " has logged **OUT** of " + itm.Key + "!\n";
                                    }
                                }

                                // if the player count is different or the server has just come back online, send the update message
                                if (ms.CurrentPlayersInt != itm.Value.CurrentPlayers || itm.Value.IsOffline)
                                {
                                    totalMessage = totalMessage + "There are now **" + ms.CurrentPlayersInt + "** players online on: " + itm.Key + "!\n";
                                    embed.Description = totalMessage;
                                    message.Embeds = new List<DiscordEmbed>
                                    {
                                        embed
                                    };
                                    hook.Send(message);
                                }

                                // set to current values now
                                itm.Value.CurrentPlayersList = ms.CurrentPlayersList;
                                itm.Value.CurrentPlayers = ms.CurrentPlayersInt;
                                itm.Value.IsOffline = false;
                            }
                            else
                            {
                                if (!itm.Value.IsOffline) // checking if the server isnt already cached as offline, so we dont spam notifications
                                {
                                    itm.Value.IsOffline = true;
                                    DiscordWebhook hook = new DiscordWebhook
                                    {
                                        Url = webhookUrl
                                    };
                                    DiscordEmbed embed = new DiscordEmbed();
                                    DiscordMessage message = new DiscordMessage();
                                    embed.Title = itm.Key + " update!";
                                    embed.Timestamp = DateTime.Now;
                                    embed.Color = Color.Pink;
                                    embed.Footer = new EmbedFooter() { Text = "Minecraft Server notifier" };
                                    string totalMessage = itm.Key + " has just gone **offline**!";
                                    embed.Description = totalMessage;
                                    message.Embeds = new List<DiscordEmbed>
                                    {
                                        embed
                                    };
                                    hook.Send(message);
                                }
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Something went wrong, is your config.json file correct?");
                    }
                    Thread.Sleep(pollInterval * 1000);
                }
            }
        }
    }

    public class ServerInfo
    {
        public string IP { get; set; }
        public ushort Port { get; set; } = 25565;
        public int CurrentPlayers { get; set; } = 0;
        public string CurrentPlayersList { get; set; } = "";
        public bool IsOffline { get; set; } = false;
    }
}
