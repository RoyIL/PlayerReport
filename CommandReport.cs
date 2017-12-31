using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.IO;

namespace RG.PlayerReport
{
	public class CommandReport : IRocketCommand
    {
        public string Help
        {
            get { return "Report malicious players"; }
        }

        public string Name
        {
            get { return "report"; }
        }

        public string Syntax
        {
            get { return "<player> [reason]"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Both;
            }
        }

        public List<string> Permissions
        {
            get { return new List<string>() { "RocketReport.report" }; }
        }

		public void Execute(IRocketPlayer caller, params string[] command)
		{
			try
			{
				if (PlayerReport.Instance.MySQLON)
				{
					if (caller is ConsolePlayer)
					{
						Logger.Log(PlayerReport.Instance.Translate("command_from_console"));
						return;
					}
					if (command.Length < 2)
					{
						UnturnedChat.Say(caller, "Try /report " + Syntax);
						return;
					}
					// To admins test if this command is working
					if (PlayerReport.Instance.Configuration.Instance.TestCode && (command[0] == PlayerReport.Instance.Configuration.Instance.KeyTestCode))
					{
						UnturnedPlayer Reported1 = UnturnedPlayer.FromName(command[1]);
						UnturnedPlayer Reporter1 = (UnturnedPlayer)caller;
						if (Reported1 == null)
						{
							UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_player_not_found"));
							return;
						}
						string ReportText1 = "";
						int num1 = 0;
						foreach (var c in command)
						{
							num1++;
							if (num1 == 1) continue;
							ReportText1 = ReportText1 + " " + c;
						}
						if (PlayerReport.Instance.Configuration.Instance.LimCharacter <= 5)
						{
							PlayerReport.Instance.Configuration.Instance.DatabasePort = 50;
							PlayerReport.Instance.Configuration.Save();
						}
						if (PlayerReport.Instance.Configuration.Instance.MaxCharacter)
						{
							if (ReportText1.Length <= PlayerReport.Instance.Configuration.Instance.LimCharacter)
							{
								PlayerReport.Instance.Database.MySqlAddReport(caller, Reported1.CSteamID, Reporter1.CSteamID, ReportText1);
							}
							else
							{
								UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_report_maxchar"));
							}
						}
						else
						{
							PlayerReport.Instance.Database.MySqlAddReport(caller, Reported1.CSteamID, Reporter1.CSteamID, ReportText1);
						}
						return;
					}
					UnturnedPlayer Reported = UnturnedPlayer.FromName(command[0]);
					UnturnedPlayer Reporter = (UnturnedPlayer)caller;
					if (Reported == null)
					{
						UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_player_not_found"));
						return;
					}
					if (caller.Id == Reported.Id)
					{
						UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_report_yourself"));
						return;
					}
					string ReportText = "";
					int num = 0;
					foreach (var s in command)
					{
						num++;
						if (num == 1) continue;
						ReportText = ReportText + " " + s;
					}
					if (PlayerReport.Instance.Configuration.Instance.LimCharacter <= 5)
					{
						PlayerReport.Instance.Configuration.Instance.DatabasePort = 50;
						PlayerReport.Instance.Configuration.Save();
					}
					if (PlayerReport.Instance.Configuration.Instance.MaxCharacter)
					{
						if (ReportText.Length <= PlayerReport.Instance.Configuration.Instance.LimCharacter)
						{
							PlayerReport.Instance.Database.MySqlAddReport(caller, Reported.CSteamID, Reporter.CSteamID, ReportText);
							if (PlayerReport.Instance.Logg)
							{
								File.AppendAllText(PlayerReport.ReportLog, "[" + DateTime.Now + "] " + Reported.DisplayName + "(" + Reported.Id + ") " + "was reported by " + Reporter.DisplayName + "(" + Reporter.Id + ") because" + ReportText + Environment.NewLine);
							}
						}
						else
						{
							UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_report_maxchar"));
						}
					}
					else
					{
						PlayerReport.Instance.Database.MySqlAddReport(caller, Reported.CSteamID, Reporter.CSteamID, ReportText);
						if (PlayerReport.Instance.Logg)
						{
							File.AppendAllText(PlayerReport.ReportLog, "[" + DateTime.Now + "] " + Reported.DisplayName + "(" + Reported.Id + ") " + "was reported by " + Reporter.DisplayName + "(" + Reporter.Id + ") because" + ReportText + Environment.NewLine);
						}
					}
				}
				else
				{
					if (caller is ConsolePlayer)
					{
						Logger.Log(PlayerReport.Instance.Translate("command_erro_saving"));
						return;
					}
					else
					{
						UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_erro_saving"));
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
			}
        }
    }
}