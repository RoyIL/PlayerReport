using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using System;

namespace RG.PlayerReport
{
    public class PlayerReport : RocketPlugin<PlayerReportConfiguration>
    {
        public static PlayerReport Instance;

        public Database Database;

		public bool MySQLON = true;

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
					{ "command_report_maxchar", "The reason for the report has exceeded the character limit." }
				};
            }
        }

        protected override void Load()
        {
            Instance = this;
            Database = new Database();
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
			if (Instance.Configuration.Instance.UseMYSQL & PlayerReport.Instance.MySQLON)
			{
				Logger.Log("Report Plugin has been loaded with MySQL!", ConsoleColor.DarkGreen);
			}
			else if (!Instance.Configuration.Instance.UseMYSQL)
			{
				Logger.Log("Report Plugin has been loaded without MySQL!", ConsoleColor.DarkGreen);
			}
			else
			{
                Instance.Configuration.Instance.UseMYSQL = true;
				Instance.Configuration.Save();
				Logger.Log("Report Plugin has been loaded without MySQL!", ConsoleColor.DarkGreen);
			}
        }

        protected override void Unload()
        {
            Instance = null;
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            Logger.Log("Report Plugin has been unloaded!", ConsoleColor.DarkGreen);
        }

        private void Events_OnPlayerConnected(UnturnedPlayer ConnectedPlayer)
        {
            Logger.LogWarning(ConnectedPlayer.DisplayName + " connected with IP " + ConnectedPlayer.IP);
        }
    }
}
