using Rocket.API;
using Rocket.API.Extensions;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;

namespace RG.PlayerReport
{
    public class CommandReports : IRocketCommand
    {
        public string Help
        {
            get { return "report options, delete, view, etc ..."; }
        }

        public string Name
        {
            get { return "reports"; }
        }

        public string Syntax
        {
            get { return " view <id>, /reports del <id>, /reports list <page>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Both; }
        }

        public List<string> Permissions
		{
			get { return new List<string>() { "RocketReports.view", "RocketReports.del", "RocketReports.list" }; }
		}

        public void Execute(IRocketPlayer caller, params string[] command)
        {
			if (command.Length >= 2 && command.Length <= 3)
			{
				uint? idval;
				switch (command[0])
				{
					case "del":
						idval = command.GetUInt32Parameter(1);
						if (idval == null || idval <= 0)
						{
							if (caller is ConsolePlayer)
							{
								Logger.Log(PlayerReport.Instance.Translate("invalid_num"));
							}
							else
							{
								UnturnedChat.Say(caller, PlayerReport.Instance.Translate("invalid_num"));
							}
							return;
						}
						if (PlayerReport.Instance.MySQLON)
						{
							PlayerReport.Instance.Database.MySqlDelReport(caller, command[1]);
						}
						else
						{
							PlayerReport.Instance.Database.LiteDBDelReport(caller, command[1]);
						}
						if (PlayerReport.Instance.Configuration.Instance.LogFile)
						{
							File.AppendAllText(PlayerReport.ReportLog, "[" + DateTime.Now + "] " + caller.DisplayName + "(" + ((UnturnedPlayer)caller).Id + ") " + "deleted report with id " + command[1] + Environment.NewLine);
						}
						break;
					case "list":
						idval = command.GetUInt32Parameter(1);
						if (idval == null || idval <= 0)
						{
							if (caller is ConsolePlayer)
							{
								Logger.Log(PlayerReport.Instance.Translate("invalid_num"));
							}
							else
							{
								UnturnedChat.Say(caller, PlayerReport.Instance.Translate("invalid_num"));
							}
							return;
						}
						if (caller is ConsolePlayer)
						{
							Logger.Log("Nothing, add soon");
						}
						else
						{
							UnturnedChat.Say(caller, "Nothing, add soon");
						}
						break;
					case "view":
						int repid;
						if (Int32.TryParse(command[1], out repid))
						{
							if (repid <= 0)
							{
								if (caller is ConsolePlayer)
								{
									Logger.Log(PlayerReport.Instance.Translate("invalid_num"));
								}
								else
								{
									UnturnedChat.Say(caller, PlayerReport.Instance.Translate("invalid_num"));
								}
								return;
							}
							if (command[1].Length <= 5)
							{
								ReportData rData = PlayerReport.Instance.Database.MySqlQueryID(repid);
								if (caller is ConsolePlayer)
								{
									Logger.Log("soon");
								}
								else
								{
									UnturnedChat.Say(caller, "soon");
								}
							}
							else
							{
								UnturnedPlayer WQuery = UnturnedPlayer.FromName(command[1].ToString());
								if (WQuery == null)
								{
									if (caller is ConsolePlayer)
									{
										Logger.Log(PlayerReport.Instance.Translate("command_report_or_steam_not_found"));
									}
									else
									{
										UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_report_or_steam_not_found"));
									}
									return;
								}
								if (PlayerReport.Instance.MySQLON)
								{
									ReportData rData = PlayerReport.Instance.Database.MySqlQuerySteam(WQuery.CSteamID);
									if (caller is ConsolePlayer)
									{
										Logger.Log("soon");
									}
									else
									{
										UnturnedChat.Say(caller, "soon");
									}
								}
							}
						}
						else
						{
							UnturnedPlayer WQuery = UnturnedPlayer.FromName(command[1].ToString());
							if (WQuery == null)
							{
								if (caller is ConsolePlayer)
								{
									Logger.Log(PlayerReport.Instance.Translate("command_player_not_found"));
								}
								else
								{
									UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_player_not_found"));
								}
								return;
							}
							if (PlayerReport.Instance.MySQLON)
							{
								ReportData rData = PlayerReport.Instance.Database.MySqlQuerySteam(WQuery.CSteamID);
								if (caller is ConsolePlayer)
								{
									Logger.Log("soon");
								}
								else
								{
									UnturnedChat.Say(caller, "soon");
								}
							}
							else
							{
								if (caller is ConsolePlayer)
								{
									Logger.Log("Nothing, add soon");
								}
								else
								{
									UnturnedChat.Say(caller, "Nothing, add soon");
								}
							}
						}
						break;
					default:
						if (caller is ConsolePlayer)
						{
							Logger.Log("Try /reports " + Syntax);
						}
						else
						{
							UnturnedChat.Say(caller, "Try /reports " + Syntax);
						}
						break;
				}
			}
			else
			{
				if (caller is ConsolePlayer)
				{
					Logger.Log("Try /reports " + Syntax);
				}
				else
				{
					UnturnedChat.Say(caller, "Try /reports " + Syntax);
				}
			}
        }
	}
}
