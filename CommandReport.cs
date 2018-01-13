using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;

namespace RG.PlayerReport
{
	public class CommandReport : IRocketCommand
    {
        public string Help => "Report malicious players";

        public string Name => "report";

        public string Syntax => "<player> [reason]";

        public List<string> Aliases => new List<string>();
	
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public List<string> Permissions => new List<string>() { "RGPReport.Report" , "RGPReport.*" };
        
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

                    UnturnedPlayer Reported;
                    UnturnedPlayer Reporter = (UnturnedPlayer)caller;

                    if (PlayerReport.Instance.Configuration.Instance.TestCode && (command[0] == PlayerReport.Instance.Configuration.Instance.KeyTestCode))
                        Reported = UnturnedPlayer.FromName(command[1]);
                    else
                        Reported = UnturnedPlayer.FromName(command[0]);

                    if (Reported == null)
                    {
                        UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_player_not_found"));
                        return;
                    }
                    else if (Reporter.Id == Reported.Id && !PlayerReport.Instance.Configuration.Instance.TestCode && (command[0] != PlayerReport.Instance.Configuration.Instance.KeyTestCode))
                    {
                        UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_report_yourself"));
                        return;
                    }

                    string ReportText = "";
                    int num = 0;
                    foreach (var cmd in command)
                    {
                        num++;
                        if (num == 1) continue;
                        ReportText = ReportText + " " + cmd;
                    }

                    if (PlayerReport.Instance.Configuration.Instance.LimCharacter <= 50)
                    {
                        PlayerReport.Instance.Configuration.Instance.LimCharacter = 50;
                        PlayerReport.Instance.Configuration.Save();
                    }

                    if (PlayerReport.Instance.Configuration.Instance.MaxCharacter)
                    {
                        if (ReportText.Length <= PlayerReport.Instance.Configuration.Instance.LimCharacter)
                        {
                            PlayerReport.Instance.Database.MySqlAddReport(caller, Reported.CSteamID, Reporter.CSteamID, ReportText);
                            if (PlayerReport.Instance.Configuration.Instance.LogFile)
                                File.AppendAllText(Directory.GetCurrentDirectory(), "[" + DateTime.Now + "] " + Reported.DisplayName + "(" + Reported.Id + ") " + "was reported by " + Reporter.DisplayName + "(" + Reporter.Id + ") because" + ReportText + System.Environment.NewLine);
                        }
                        else
                        {
                            UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_report_maxchar"));
                            return;
                        }
                    }
                    else
                    {
                        PlayerReport.Instance.Database.MySqlAddReport(caller, Reported.CSteamID, Reporter.CSteamID, ReportText);
                        if (PlayerReport.Instance.Configuration.Instance.LogFile)
                            File.AppendAllText(Directory.GetCurrentDirectory() + @"/Reports.log", "[" + DateTime.Now + "] " + Reported.DisplayName + "(" + Reported.Id + ") " + "was reported by " + Reporter.DisplayName + "(" + Reporter.Id + ") because" + ReportText + System.Environment.NewLine);
                    }
                    foreach (var pla in Provider.clients)
                    {
                        if (((IRocketPlayer)pla).HasPermission("RGPReport.Notify") || ((IRocketPlayer)pla).IsAdmin || ((IRocketPlayer)pla).HasPermission("RGPReport.*"))
                        {
                            UnturnedChat.Say((IRocketPlayer)pla, PlayerReport.Instance.Translate("new_reports_to_see", PlayerReport.Instance.Database.MySqlNotif()));
                            PlayerReport.Instance.Notif++;
                            if (PlayerReport.Instance.Notif == PlayerReport.Instance.Configuration.Instance.Notifications)
                            {
                                PlayerReport.Instance.Database.MySqlNotified();
                            }
                        }
                    }
                }
                else
                {
                    if (caller is ConsolePlayer)
                    {
                        Logger.Log(PlayerReport.Instance.Translate("command_erro_saving"));
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
