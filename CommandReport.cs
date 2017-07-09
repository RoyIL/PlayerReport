﻿using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using System.Collections.Generic;
using System.Management.Instrumentation;

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
                UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_from_console"));
                return;
            }
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, "Try /report " + Syntax);
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
            CSteamID ReportedID = Reported.CSteamID;
            CSteamID ReporterID = Reporter.CSteamID;
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
			}
			PlayerReport.Instance.Configuration.Save();
			if (PlayerReport.Instance.Configuration.Instance.MaxCharacter)
			{
				if (ReportText.Length <= PlayerReport.Instance.Configuration.Instance.LimCharacter)
				{
					if (PlayerReport.Instance.Configuration.Instance.UseMYSQL)
					{
						PlayerReport.Instance.Database.MySqlAddReport(caller, ReportedID, ReporterID, ReportText);
					}
					else if (!PlayerReport.Instance.Configuration.Instance.UseMYSQL)
					{
						PlayerReport.Instance.Database.LiteDBAddReport(caller, ReportedID, ReporterID, ReportText);
					}
				}
				else
				{
					UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_report_maxchar"));
				}
			}
			else
			{
				if (PlayerReport.Instance.Configuration.Instance.UseMYSQL)
				{
					PlayerReport.Instance.Database.MySqlAddReport(caller, ReportedID, ReporterID, ReportText);
				}
				else if (!PlayerReport.Instance.Configuration.Instance.UseMYSQL)
				{
					PlayerReport.Instance.Database.LiteDBAddReport(caller, ReportedID, ReporterID, ReportText);
				}
			}
        }
    }
}