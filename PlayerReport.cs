using Rocket.API.Collections;
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

        public override TranslationList DefaultTranslations
        {
            get
            {
                return new TranslationList()
                {
                    { "command_player_not_found", "Player not found" },
                    { "command_report_self", "You can not report yourself" },
                    { "command_from_console", "You are the console, the console does not report people, it gives them ban" },
                    { "command_successful", "You reported the player successfully" },
                    { "command_del_successful", "You deleted the report successfully" },
					{ "command_erro_saving", "An error occurred in the report writing process." }
                };
            }
        }

        protected override void Load()
        {
            Instance = this;
            this.Database = new Database();
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
			if (Instance.Configuration.Instance.UseMYSQL)
			{
				Rocket.Core.Logging.Logger.Log("Report Plugin has been loaded with MySQL!", ConsoleColor.DarkGreen);
			}
			else if (!Instance.Configuration.Instance.UseMYSQL)
			{
				Rocket.Core.Logging.Logger.Log("Report Plugin has been loaded without MySQL!", ConsoleColor.DarkGreen);
			}
			else
			{
                Instance.Configuration.Instance.UseMYSQL = false;
			}
        }

        protected override void Unload()
        {
            Instance = null;
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
            Rocket.Core.Logging.Logger.Log("Report Plugin has been unloaded!", ConsoleColor.DarkGreen);
        }

        private void Events_OnPlayerConnected(UnturnedPlayer ConnectedPlayer)
        {
            Rocket.Core.Logging.Logger.LogWarning(ConnectedPlayer.DisplayName + " connected with IP " + ConnectedPlayer.IP);
        }
    }
}
