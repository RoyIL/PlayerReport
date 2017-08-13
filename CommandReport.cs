using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;

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
            get
            {
            return new List<string>() { "RocketReport.report" };
            }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
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
            UnturnedPlayer Reported = UnturnedPlayer.FromName(command[0]);
            UnturnedPlayer Reporter = (UnturnedPlayer)caller;
			List<SteamPlayer> Players = Provider.clients;
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
					
					foreach (IRocketPlayer PlayPerm in Players)
					{
						if (PlayPerm.IsAdmin || PlayPerm.HasPermission("RocketReport.notify"))
						{
							UnturnedChat.Say(PlayPerm, PlayerReport.Instance.Translate("new_reports_to_see"));
						}
					}
					if (PlayerReport.Instance.MySQLON)
					{
						PlayerReport.Instance.Database.MySqlAddReport(caller, Reported.CSteamID, Reporter.CSteamID, ReportText);
					}
					else
					{
						PlayerReport.Instance.Database.LiteDBAddReport(caller, Reported.Id, Reporter.Id, ReportText);
					}
				}
				else
				{
					UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_report_maxchar"));
					return;
				}
			}
			else
			{
				foreach (IRocketPlayer PlayPerm in Players)
				{
					if (PlayPerm.IsAdmin || PlayPerm.HasPermission("RocketReport.notify"))
					{
						UnturnedChat.Say(PlayPerm, PlayerReport.Instance.Translate("new_reports_to_see"));
					}
				}
				if (PlayerReport.Instance.MySQLON)
				{
					PlayerReport.Instance.Database.MySqlAddReport(caller, Reported.CSteamID, Reporter.CSteamID, ReportText);
				}
				else
				{
					PlayerReport.Instance.Database.LiteDBAddReport(caller, Reported.Id, Reporter.Id, ReportText);
				}
				
			}
        }
    }
}