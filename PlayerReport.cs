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
        internal Database Database;

        internal bool MySQLON = true;
        internal ushort Notif = 0;

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    { "command_from_console", "You are the console, the console does not report people, it gives them ban." },
                    { "command_erro_saving", "An error occurred with database." },
                    { "command_player_not_found", "Player not found." },
                    { "command_report_yourself", "You can not report yourself." },
                    { "command_report_maxchar", "The reason for the report has exceeded the character limit." },
                    { "command_add_successful", "You reported the player successfully." },
                    { "new_reports_to_see", "There are new reports for you to review, {0} reports." }
                };
            }
        }

        protected override void Load()
        {
            Instance = this;
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;

            if (Instance.Configuration.Instance.Notifications < 0)
            {
                Instance.Configuration.Instance.Notifications = 3;
                Instance.Configuration.Save();
            }

            Logger.Log("Connecting the database ...", ConsoleColor.DarkGreen);
            Database = new Database();

            if (!Instance.MySQLON)
            {
                Logger.Log("ERRO. To connect to the database, please check the settings!", ConsoleColor.DarkGreen);
                Logger.Log("Report Plugin has been loaded without MySQL!", ConsoleColor.DarkGreen);
                if (Instance.Configuration.Instance.LogFile)
                {
                    File.AppendAllText(System.IO.Directory.GetCurrentDirectory(), "[" + DateTime.Now + "] Report Plugin has been loaded without MySQL!" + System.Environment.NewLine);
                }
                UnloadPlugin();
            }
            else
            {
                Logger.Log("Successful connection!", ConsoleColor.DarkGreen);
                Logger.Log("Report Plugin has been loaded with MySQL!", ConsoleColor.DarkGreen);
                if (Instance.Configuration.Instance.LogFile)
                {
                    File.AppendAllText(System.IO.Directory.GetCurrentDirectory(), "[" + DateTime.Now + "] Report Plugin has been loaded with MySQL!" + System.Environment.NewLine);
                }
            }
        }

        protected override void Unload()
        {
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            Logger.Log("Report Plugin has been unloaded!", ConsoleColor.DarkGreen);
            if (Instance.Configuration.Instance.LogFile)
            {
                File.AppendAllText(System.IO.Directory.GetCurrentDirectory(), "[" + DateTime.Now + "] Report Plugin has been unloaded!" + System.Environment.NewLine);
            }
            Instance = null;
        }

        internal void Events_OnPlayerConnected(IRocketPlayer ConnectedPlayer)
        {
            if (Instance.MySQLON && Instance.Database.MySqlNotif() > 0)
            {
                if (ConnectedPlayer.HasPermission("RGPReport.*") || ConnectedPlayer.HasPermission("RGPReport.Notify") || ConnectedPlayer.IsAdmin)
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