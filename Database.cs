using LiteDB;
using MySql.Data.MySqlClient;
using Logger = Rocket.Core.Logging.Logger;
using Rocket.API;
using Rocket.Unturned.Chat;
using Steamworks;
using System;
using System.IO;
using System.Collections.Generic;

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

		public void LiteDBAddReport(IRocketPlayer caller, string ReportedID, string ReporterID, string ReportText)
		{
			if (!Directory.Exists("Database"))
			{
				Directory.CreateDirectory("Database");
			}
			using (LiteDatabase LiteDBFile = new LiteDatabase(Path.Combine("Database", PlayerReport.Instance.Configuration.Instance.DatabaseName + ".db")))
			using (LiteTransaction Trans = LiteDBFile.BeginTrans())
			{
				LiteCollection<AddReportDB> ReportsCollection = LiteDBFile.GetCollection<AddReportDB>(TableName);
				var AddReportsDB = new AddReportDB
				{
					ReportedID = ReportedID,
					ReporterID = ReporterID,
					ReportDate = DateTime.UtcNow,
					ReportInfo = ReportText,
					Notified = 0
				};
				ReportsCollection.Insert(AddReportsDB);
				Trans.Commit();
			}
			UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_add_successful"));
		}

		public void LiteDBDelReport(IRocketPlayer caller, string ID)
        {
            if (Directory.Exists("Database"))
            {
				using (LiteDatabase LiteDBFile = new LiteDatabase(Path.Combine("Database", PlayerReport.Instance.Configuration.Instance.DatabaseName + ".db")))
				using (LiteTransaction Trans = LiteDBFile.BeginTrans())
				{
					LiteCollection<AddReportDB> ReportsCollection = LiteDBFile.GetCollection<AddReportDB>(TableName);
					if (ReportsCollection.Exists(Query.EQ("_id", ID)))
					{
						ReportsCollection.Delete(ID);
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
						UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_report_not_found"));
					}
					Trans.Commit();
				}
            }
            else
            {
                UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_data_not_found"));
            }
        }

		//working

		/* public void LiteDBNotif()
		{
			int CountVal = 0;
			try
			{
				MySqlConnection SQLconnection = CreateConnection();
				MySqlCommand SQLcommand = SQLconnection.CreateCommand();
				SQLcommand.CommandText = "SELECT `Notified`, COUNT(*) AS `HowMany` FROM `Unturned`.`Reports` WHERE Notified = 'false' GROUP BY `Notified`;";
				SQLconnection.Open();
				object Ok = SQLcommand.ExecuteScalar();
				if (Ok != null)
				{
					int.TryParse(Ok.ToString(), out CountVal);
				}
				SQLcommand.ExecuteNonQuery();
				SQLconnection.Close();
			}
			catch (Exception ex)
			{
				Logger.LogException(ex);
			}
			if (CountVal > 0)
			{
				PlayerReport.Instance.NotifyExist = true;
			}
		}

		public void LiteDBNotified()
		{
			if (Directory.Exists("Database"))
			{
				using (LiteDatabase LiteDBFile = new LiteDatabase(Path.Combine("Database", PlayerReport.Instance.Configuration.Instance.DatabaseName + ".db")))
				using (LiteTransaction Trans = LiteDBFile.BeginTrans())
				{
					LiteCollection<AddReportDB> ReportsCollection = LiteDBFile.GetCollection<AddReportDB>(TableName);
					if (ReportsCollection.Exists(Query.EQ("_id", ID)))
					{
						ReportsCollection.Delete(ID);
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
						UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_report_not_found"));
					}
					Trans.Commit();
				}
			}
			else
			{
				UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_data_not_found"));
			}
			try
			{
				MySqlConnection SQLconnection = CreateConnection();
				MySqlCommand SQLcommand = SQLconnection.CreateCommand();
				SQLcommand.CommandText = "update `Unturned`.`Reports` set Notified='true' where Notified='false';";
				SQLconnection.Open();
				SQLcommand.ExecuteNonQuery();
				SQLconnection.Close();
			}
			catch (Exception ex)
			{
				Logger.LogException(ex);
			}
		} */

		//working 

		public class AddReportDB
        {
            [BsonId]
            public int ID { get; set; }

			[BsonIndex(true)]
			public string ReportedID { get; set; }

			[BsonIndex(true)]
			public string ReporterID { get; set; }

            public DateTime ReportDate { get; set; }
            public string ReportInfo { get; set; }
			public int Notified { get; set; }

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

		public ReportData MySqlQuerySteam(CSteamID SteamId)
		{
			ReportData reportData = new ReportData();
			MySqlDataReader Reader = null;
			try
			{
				MySqlConnection SQLconnection = CreateConnection();
				MySqlCommand SQLcommand = SQLconnection.CreateCommand();
				SQLcommand.Parameters.AddWithValue("@steamid", SteamId);
				SQLcommand.CommandText = string.Concat("SELECT id, ReportedID, ReporterID, ReportDate, ReportInfo FROM `" + TableName + "` WHERE ReportedID = @steamid ORDER BY id ASC" );
				Reader = SQLcommand.ExecuteReader();
				if (Reader.Read())
				{
					reportData = BuildReportData(Reader);
					PlayerReport.Instance.DataFound = true;
				}
			}
			catch (MySqlException ex)
			{
				Logger.LogException(ex);
			}
			finally
			{
				if (Reader != null)
				{
					Reader.Close();
					Reader.Dispose();
				}
			}
			return reportData;
		}

		public ReportData MySqlQueryID(int RepId)
		{
			ReportData UnsetData = new ReportData();
			ReportData reportData = UnsetData;
			MySqlDataReader Reader = null;
			try
			{
				MySqlConnection SQLconnection = CreateConnection();
				MySqlCommand SQLcommand = SQLconnection.CreateCommand();
				SQLcommand.Parameters.AddWithValue("@id", RepId);
				SQLcommand.CommandText = string.Concat("SELECT id, ReportedID, ReporterID, ReportDate, ReportInfo FROM `" + TableName + "` WHERE id = @id");
				Reader = SQLcommand.ExecuteReader();
				if (Reader.Read())
				{
					reportData = BuildReportData(Reader);
				}
			}
			catch (MySqlException ex)
			{
				Logger.LogException(ex);
			}
			finally
			{
				if (Reader != null)
				{
					Reader.Close();
					Reader.Dispose();
				}
			}
			return reportData;
		}

		public bool MySqlExistSteam(CSteamID SteamId)
		{
			ReportData reportData = new ReportData();
			MySqlDataReader Reader = null;
			try
			{
				MySqlConnection SQLconnection = CreateConnection();
				MySqlCommand SQLcommand = SQLconnection.CreateCommand();
				SQLcommand.Parameters.AddWithValue("@steamid", SteamId);
				SQLcommand.CommandText = string.Concat("SELECT id FROM `" + TableName + "` WHERE ReportedID = @steamid ORDER BY id ASC");
				Reader = SQLcommand.ExecuteReader();
				if (Reader.Read())
				{
					reportData = BuildReportData(Reader);
					PlayerReport.Instance.DataFound = true;
				}
			}
			catch (MySqlException ex)
			{
				Logger.LogException(ex);
			}
			finally
			{
				if (Reader != null)
				{
					Reader.Close();
					Reader.Dispose();
				}
			}
			return false;
		}

		public bool MySqlExistID(int RepId)
		{
			ReportData UnsetData = new ReportData();
			ReportData reportData = UnsetData;
			MySqlDataReader Reader = null;
			try
			{
				MySqlConnection SQLconnection = CreateConnection();
				MySqlCommand SQLcommand = SQLconnection.CreateCommand();
				SQLcommand.Parameters.AddWithValue("@id", RepId);
				SQLcommand.CommandText = string.Concat("SELECT id, ReportedID, ReporterID, ReportDate, ReportInfo FROM `" + TableName + "` WHERE id = @id");
				Reader = SQLcommand.ExecuteReader();
				if (Reader.Read())
				{
					reportData = BuildReportData(Reader);
				}
			}
			catch (MySqlException ex)
			{
				Logger.LogException(ex);
			}
			finally
			{
				if (Reader != null)
				{
					Reader.Close();
					Reader.Dispose();
				}
			}
			return false;
		}

		private ReportData BuildReportData(MySqlDataReader Reader)
		{
			return new ReportData(Reader.GetInt32("id"), (CSteamID)Reader.GetUInt64("ReportedID"), (CSteamID)Reader.GetUInt64("ReporterID"), DateTime.FromBinary(Reader.GetInt64("ReportDate")), Reader.GetString("ReportInfo"));
		}
	}
}