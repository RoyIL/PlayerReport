using MySql.Data.MySqlClient;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.API;
using Rocket.Unturned.Chat;
using Steamworks;
using System;
using System.IO;

namespace RG.PlayerReport
{
	public class Database
	{
		private static string TableName = PlayerReport.Instance.Configuration.Instance.DatabaseTableName;

		public Database()
		{
			new I18N.West.CP1250();
			CheckSchema();
		}

		private MySqlConnection CreateConnection()
		{
			MySqlConnection SQLconnection = null;
			try
			{
				if (PlayerReport.Instance.Configuration.Instance.DatabasePort <= 0)
				{
					PlayerReport.Instance.Configuration.Instance.DatabasePort = 3306;
					PlayerReport.Instance.Configuration.Save();
				}
				SQLconnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", new object[] {
					PlayerReport.Instance.Configuration.Instance.DatabaseAddress,
					PlayerReport.Instance.Configuration.Instance.DatabaseName,
					PlayerReport.Instance.Configuration.Instance.DatabaseUsername,
					PlayerReport.Instance.Configuration.Instance.DatabasePassword,
					PlayerReport.Instance.Configuration.Instance.DatabasePort}));
			}
			catch (Exception ex)
			{
				Logger.LogException(ex);
				PlayerReport.Instance.MySQLON = false;
			}
			return SQLconnection;
		}

		public void CheckSchema()
		{
			try
			{
				MySqlConnection SQLconnection = CreateConnection();
				MySqlCommand SQLcommand = SQLconnection.CreateCommand();
				SQLcommand.CommandText = string.Concat("show tables like '" + TableName + "'");
				SQLconnection.Open();
				if (SQLcommand.ExecuteScalar() == null)
				{
					SQLcommand.CommandText = string.Concat("CREATE TABLE `" + TableName + "` (`id` int(11) NOT NULL AUTO_INCREMENT,`ReportedID` varchar(32) NOT NULL,`ReporterID` varchar(32) NOT NULL,`ReportDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,`ReportInfo` varchar(512) DEFAULT NULL,`Notified` varchar(5) DEFAULT false,PRIMARY KEY (`id`)); ");
					SQLcommand.ExecuteNonQuery();
				}
				SQLconnection.Close();
			}
			catch (Exception exception)
			{
				Logger.LogException(exception);
				PlayerReport.Instance.MySQLON = false;
			}
		}

        public void MySqlAddReport(IRocketPlayer caller, CSteamID MReportedID, CSteamID MReporterID, string ReportText)
        {
            try
            {
                MySqlConnection SQLconnection = CreateConnection();
                MySqlCommand SQLcommand = SQLconnection.CreateCommand();
                SQLcommand.Parameters.AddWithValue("@ReportedID", MReportedID);
				SQLcommand.Parameters.AddWithValue("@ReporterID", MReporterID);
				SQLcommand.Parameters.AddWithValue("@ReportInfo", ReportText);
				SQLcommand.CommandText = string.Concat("insert into `" + TableName + "` (`ReportedID`,`ReporterID`,`ReportInfo`,`Notified`) values(@ReportedID,@ReporterID,@ReportInfo,false);");
                SQLconnection.Open();
                int OK = SQLcommand.ExecuteNonQuery();
                SQLconnection.Close();
                if (OK > 0)
                {
                    UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_add_successful"));
                }
                else
                {
                    UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_erro_saving"));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_erro_saving"));
            }
        }

		public void MySqlDelReport(IRocketPlayer caller, string ID)
		{
			try
			{
				MySqlConnection SQLconnection = CreateConnection();
				MySqlCommand SQLcommand = SQLconnection.CreateCommand();
				SQLcommand.CommandText = string.Concat("delete from `" + TableName + "` where id = `" + ID + "`" );
				SQLconnection.Open();
				int OK = SQLcommand.ExecuteNonQuery();
				SQLconnection.Close();
				if (OK > 0)
				{
					if (caller is ConsolePlayer)
					{
						Logger.Log(PlayerReport.Instance.Translate("command_del_successful"));
					}
					else
					{
						UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_del_successful"));
					}
				}
				else
				{
					if (caller is ConsolePlayer)
					{
						Logger.Log(PlayerReport.Instance.Translate("command_report_not_found"));
					}
					else
					{
						UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_report_not_found"));
					}
				}
			}
			catch (Exception ex)
			{
				Logger.LogException(ex, null);
				UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_erro_saving"));
			}
		}

		public bool MySqlNotif()
		{
			int CountVal = 0;
			try
			{
				MySqlConnection SQLconnection = CreateConnection();
				MySqlCommand SQLcommand = SQLconnection.CreateCommand();
				SQLcommand.CommandText = string.Concat("SELECT `Notified`, COUNT(*) AS `HowMany` FROM `Unturned`.`Reports` WHERE Notified = `false` GROUP BY `Notified`;");
				SQLconnection.Open();
				object Ok = SQLcommand.ExecuteScalar();
				if (Ok != null)
				{
					int.TryParse(Ok.ToString(), out CountVal);
				}
				SQLcommand.ExecuteNonQuery();
				SQLconnection.Close();
				if (CountVal > 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				Logger.LogException(ex);
			}
			return false;
		}

		public void MySqlNotified()
		{
			try
			{
				MySqlConnection SQLconnection = CreateConnection();
				MySqlCommand SQLcommand = SQLconnection.CreateCommand();
				SQLcommand.CommandText = string.Concat("update `Unturned`.`Reports` set Notified = `true` where Notified = `false`");
				SQLconnection.Open();
				SQLcommand.ExecuteNonQuery();
				SQLconnection.Close();
			}
			catch (Exception ex)
			{
				Logger.LogException(ex);
			}
		}

	}
}