using Rocket.API;
using Rocket.API.Collections;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.IO;

namespace RG.PlayerReport
{
    public class PlayerReport : RocketPlugin<PlayerReportConfiguration>
    {
        public static PlayerReport Instance;

        public bool MySQLON = true;
        public bool NotifyExist = false;
        public bool DataFound = false;
        public bool Logg;

        public static string ReportLog = System.IO.Directory.GetCurrentDirectory() + @"/Reports.log";
        private int Notif = 0;

        public Database Database;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    { "command_player_not_found", "Player not found." },
                    { "command_report_yourself", "You can not report yourself." },
                    { "command_from_console", "You are the console, the console does not report people, it gives them ban." },
                    { "command_add_successful", "You reported the player successfully." },
                    { "command_del_successful", "You deleted the report successfully." },
                    { "command_data_not_found", "Database not found." },
                    { "command_erro_saving", "An error occurred with database." },
                    { "command_report_not_found", "Report not found." },
                    { "command_report_maxchar", "The reason for the report has exceeded the character limit." },
                    { "new_reports_to_see", "There are new reports for you to review, {0} reports." },
                    { "invalid_num", "Please enter a valid number."},
                    { "command_report_or_steam_not_found", "The id of the report or steam id was not found."}
                };
            }
        }

        protected override void Load()
        {
            Instance = this;
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            Logg = Instance.Configuration.Instance.LogFile;
            if (Instance.Configuration.Instance.Notifications <= 0)
            {
                Instance.Configuration.Instance.Notifications = 3;
                Instance.Configuration.Save();
            }
            Logger.Log("Connecting the database ...", ConsoleColor.DarkGreen);
            Database = new Database();
            if (!Instance.MySQLON)
            {
                Logger.Log("To connect to the database, please check the settings!", ConsoleColor.DarkGreen);
                Logger.Log("Report Plugin has been loaded without MySQL!", ConsoleColor.DarkGreen);
                if (Logg)
                {
                    File.AppendAllText(ReportLog, "[" + DateTime.Now + "] Report Plugin has been loaded without MySQL!" + System.Environment.NewLine);
                }

            }
            else
            {
                Logger.Log("Successful connection!", ConsoleColor.DarkGreen);
                Logger.Log("Report Plugin has been loaded with MySQL!", ConsoleColor.DarkGreen);
                if (Logg)
                {
                    File.AppendAllText(ReportLog, "[" + DateTime.Now + "] Report Plugin has been loaded with MySQL!" + System.Environment.NewLine);
                }
            }
        }

        protected override void Unload()
        {
            Instance = null;
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            Logger.Log("Report Plugin has been unloaded!", ConsoleColor.DarkGreen);
            if (Logg)
            {
                File.AppendAllText(ReportLog, "[" + DateTime.Now + "] Report Plugin has been unloaded!" + System.Environment.NewLine);
            }
        }

        private void Events_OnPlayerConnected(IRocketPlayer ConnectedPlayer)
        {
            Logger.LogWarning(ConnectedPlayer.DisplayName + " connected with IP " + ((UnturnedPlayer)ConnectedPlayer).IP);
            if (Instance.MySQLON && Instance.Database.MySqlNotif() > 0)
            {
                if (ConnectedPlayer.HasPermission("RocketReport.notify") || ConnectedPlayer.IsAdmin)
                {
                    UnturnedChat.Say(ConnectedPlayer, Instance.Translate("new_reports_to_see", Instance.Database.MySqlNotif()));
                    Notif++;
                    if (Notif == Instance.Configuration.Instance.Notifications)
                    {
                        Instance.Database.MySqlNotified();
                    }
                }
            }
        }
    }
}