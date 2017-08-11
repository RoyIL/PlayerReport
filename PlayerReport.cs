using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using Rocket.Unturned.Plugins;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace RG.PlayerReport
{
    public class PlayerReport : RocketPlugin<PlayerReportConfiguration>
    {
        public static PlayerReport Instance;

		public bool MySQLON = true;
		public bool NotifyExist = false;
		public static string ReportLog = System.IO.Directory.GetCurrentDirectory() + @"/Reports.log";

		public Database Database;

		public List<IRocketPlayer> List = new List<IRocketPlayer>();

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
					{ "new_reports_to_see", "There are new reports for you to check." },
					{ "command_del_nonum", "" },
					{ "command_del_nofound", "" }
				};
            }
        }

		protected override void Load()
        {
            Instance = this;
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
			U.Events.OnPlayerDisconnected += Events_OnPlayerDisconnected;
			foreach (SteamPlayer StPl in Provider.clients)
			{
				IRocketPlayer Play = UnturnedPlayer.FromSteamPlayer(StPl);
				List.Add(Play);
			}
			if (Instance.Configuration.Instance.UseMYSQL)
			{
				Logger.Log("Connecting the database ...", ConsoleColor.DarkGreen);
				Database = new Database();
				if (!Instance.MySQLON)
				{
					Logger.Log("To connect to the database, please check the settings!", ConsoleColor.DarkGreen);
					Logger.Log("Report Plugin has been loaded without MySQL!", ConsoleColor.DarkGreen);
					File.AppendAllText(ReportLog, "[" + DateTime.Now + "] Report Plugin has been loaded without MySQL!" + System.Environment.NewLine);

				}
				else
				{
					Logger.Log("Successful connection!", ConsoleColor.DarkGreen);
					Logger.Log("Report Plugin has been loaded with MySQL!", ConsoleColor.DarkGreen);
					File.AppendAllText(ReportLog, "[" + DateTime.Now + "] Report Plugin has been loaded with MySQL!" + System.Environment.NewLine);
				}
			}
			else if (!Instance.Configuration.Instance.UseMYSQL)
			{
				Instance.MySQLON = false;
				Logger.Log("Report Plugin has been loaded without MySQL!", ConsoleColor.DarkGreen);
				File.AppendAllText(ReportLog, "[" + DateTime.Now + "] Report Plugin has been loaded without MySQL!" + System.Environment.NewLine);
			}
			else
			{
				Database = new Database();
				if (!Instance.MySQLON)
				{
					Instance.Configuration.Instance.UseMYSQL = false;
					Instance.Configuration.Save();
					Logger.Log("Report Plugin has been loaded without MySQL!", ConsoleColor.DarkGreen);
					File.AppendAllText(ReportLog, "[" + DateTime.Now + "] Report Plugin has been loaded without MySQL!" + System.Environment.NewLine);
				}
				else
				{
					Instance.Configuration.Instance.UseMYSQL = true;
					Instance.Configuration.Save();
					Logger.Log("Report Plugin has been loaded with MySQL!", ConsoleColor.DarkGreen);
					File.AppendAllText(ReportLog, "[" + DateTime.Now + "] Report Plugin has been loaded with MySQL!" + System.Environment.NewLine);
				}

			}
		}

        protected override void Unload()
        {
            Instance = null;
            U.Events.OnPlayerConnected -= Events_OnPlayerConnected;
			U.Events.OnPlayerDisconnected -= Events_OnPlayerDisconnected;
			Logger.Log("Report Plugin has been unloaded!", ConsoleColor.DarkGreen);
			File.AppendAllText(ReportLog, "[" + DateTime.Now + "] Report Plugin has been unloaded!" + System.Environment.NewLine);
		}

		private void Events_OnPlayerDisconnected(IRocketPlayer DisconnectedPlayer)
		{
			List.Remove(DisconnectedPlayer);
		}

		private void Events_OnPlayerConnected(IRocketPlayer ConnectedPlayer)
		{
			UnturnedPlayer ConPlayer = (UnturnedPlayer)ConnectedPlayer;
			Logger.LogWarning(ConnectedPlayer.DisplayName + " connected with IP " + ConPlayer.IP);
			List.Add(ConnectedPlayer);
			if (Instance.MySQLON)
			{
				Instance.Database.MySqlNotif();
			}
			else
			{
				//soon Instance.Database.LiteDBNotif();
			}
			if (NotifyExist)
			{
				UnturnedPlayerEvents.OnPlayerUpdatePosition += Events_OnPlayerUpdatePosition;
			}
		}

		private void Events_OnPlayerUpdatePosition(IRocketPlayer MovPlayer, Vector3 NoCare)
		{
			if (MovPlayer.HasPermission("RocketReport.notify") || MovPlayer.IsAdmin)
			{
				UnturnedChat.Say(MovPlayer, Instance.Translate("new_reports_to_see"));
				if (Instance.MySQLON)
				{
					Instance.Database.MySqlNotified();
				}
				else
				{
					//not yet Instance.Database.LiteDBNotified();
				}
				UnturnedPlayerEvents.OnPlayerUpdatePosition -= Events_OnPlayerUpdatePosition;
			}
		}
	}
}
