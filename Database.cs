using LiteDB;
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
        private string tableName = PlayerReport.Instance.Configuration.Instance.DatabaseTableName;

        internal string TableName { get => tableName; set => tableName = value; }

        public Database()
        {
			if (PlayerReport.Instance.Configuration.Instance.UseMYSQL)
			{
				new I18N.West.CP1250();
				CheckSchema();
			}
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
            }
            return SQLconnection;
        }

        public void CheckSchema()
        {
            try
            {
                MySqlConnection SQLconnection = CreateConnection();
                MySqlCommand SQLcommand = SQLconnection.CreateCommand();
                SQLcommand.CommandText = string.Concat("show tables like '" + PlayerReport.Instance.Configuration.Instance.DatabaseTableName + "'");
                SQLconnection.Open();
                if (SQLcommand.ExecuteScalar() == null)
                {
                    SQLcommand.CommandText = string.Concat("CREATE TABLE `" + PlayerReport.Instance.Configuration.Instance.DatabaseTableName + "` (`id` int(11) NOT NULL AUTO_INCREMENT,`ReportedID` varchar(32) NOT NULL,`ReporterID` varchar(32) NOT NULL,`ReportDate` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,`ReportInfo` varchar(512) DEFAULT NULL,PRIMARY KEY (`id`));");
                    SQLcommand.ExecuteNonQuery();
                }
                SQLconnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogException(exception);
            }
        }

        public void LiteDBAddReport(IRocketPlayer caller, CSteamID ReportedID, CSteamID ReporterID, string ReportText)
        {
            try
            {
                if (!Directory.Exists("Database"))
                {
                    Directory.CreateDirectory("Database");
                }
                using (var LiteDBFile = new LiteDatabase(Path.Combine("Database", PlayerReport.Instance.Configuration.Instance.DatabaseName + ".db")))
                using (var Trans = LiteDBFile.BeginTrans())
                {
                    var ReportsCollection = LiteDBFile.GetCollection<AddReportDB>(TableName);
                    var AddReportsDB = new AddReportDB
                    {
                        ReportedID = ReportedID,
                        ReporterID = ReporterID,
                        ReportDate = DateTime.UtcNow,
                        ReportText = ReportText,
                    };
                    ReportsCollection.Insert(AddReportsDB);
                    Trans.Commit();
                }
                UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_add_successful"));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public void LiteDBDelReport(IRocketPlayer caller, string ID)
        {
            if (Directory.Exists("Database"))
            {
                LiteDatabase LiteDBFile = new LiteDatabase(Path.Combine("Database", PlayerReport.Instance.Configuration.Instance.DatabaseName + ".db"));
                LiteCollection<AddReportDB> ReportsCollection = LiteDBFile.GetCollection<AddReportDB>(TableName);
                ReportsCollection.Delete(ID);
                ReportsCollection.Exists(Query.EQ("_id", ID));
                UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_del_successful"));
                UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_report_not_found")); 
            }
            else
            {
                UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_data_not_found"));
            }
        }

        public class AddReportDB
        {
            [BsonId]
            public int ID { get; set; }

            [BsonIndex(false)]
            public CSteamID ReportedID { get; set; }

            [BsonIndex(false)]
            public CSteamID ReporterID { get; set; }

            public DateTime ReportDate { get; set; }
            public string ReportText { get; set; }

        }

        public void MySqlAddReport(IRocketPlayer caller, CSteamID ReportedID, CSteamID ReporterID, string ReportText)
        {
            try
            {
                MySqlConnection SQLconnection = CreateConnection();
                MySqlCommand SQLcommand = SQLconnection.CreateCommand();
                SQLcommand.Parameters.AddWithValue("@ReportedID", ReportedID);
				SQLcommand.Parameters.AddWithValue("@ReporterID", ReporterID);
                SQLcommand.Parameters.AddWithValue("@ReportInfo", ReportText);
                SQLcommand.CommandText = "insert into `" + PlayerReport.Instance.Configuration.Instance.DatabaseTableName + "` (`ReportedID`,`ReporterID`,`ReportInfo`) values(@ReportedID,@ReporterID,@ReportInfo);";
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
                SQLcommand.CommandText = string.Concat(new string[]
                {
                    "delete from `",
                    PlayerReport.Instance.Configuration.Instance.DatabaseTableName,
                    "` where id='",
                    ID,
                    "';"
                });
                SQLconnection.Open();
                int OK = SQLcommand.ExecuteNonQuery();
                SQLconnection.Close();
                if (OK > 0)
                {
                    UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_del_successful"));
                }
                else
                {
                    UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_report_not_found"));
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, null);
                UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_erro_saving"));
            }
        }
    }
}