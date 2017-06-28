using LiteDB;
using MySql.Data.MySqlClient;
using Logger = Rocket.Core.Logging.Logger;
using Steamworks;
using System;
using System.IO;

namespace RG.PlayerReport
{
    public class Database
    {
        internal string TableName = PlayerReport.Instance.Configuration.Instance.DatabaseTableName;

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
                if (PlayerReport.Instance.Configuration.Instance.DatabasePort == 0) PlayerReport.Instance.Configuration.Instance.DatabasePort = 3306;
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

        public enum OperationResult
        {

            SUCCESS,
            TRANSACTION_TO_SELF,
            PLAYER_NOT_FOUND,
            NON_POSITIVE_AMOUNT,
            NOT_ENOUGH_MONEY

        }

        public void LiteDBAddReport(CSteamID ReportedID, CSteamID ReporterID, string ReportText)
        {
            if(!Directory.Exists("Database"))
            {
                Directory.CreateDirectory("Database");
            }
            LiteDatabase LiteDBFile = new LiteDatabase(Path.Combine("Database", PlayerReport.Instance.Configuration.Instance.DatabaseName + ".db"));
            LiteTransaction Transaction = LiteDBFile.BeginTrans();
            LiteCollection<AddReportDB> ReportsCollection = LiteDBFile.GetCollection<AddReportDB>(TableName);
            var AddReportDB = new AddReportDB
            {
                ReportedID = ReportedID,
                ReporterID = ReporterID,
                ReportDate = DateTime.UtcNow,
                ReportText = ReportText,
            };
            ReportsCollection.Insert(AddReportDB);
            Transaction.Commit();
        }

        public class AddReportDB
        {

            [BsonId]
            public int Id { get; set; }

            [BsonIndex(false)]
            public CSteamID ReportedID { get; set; }

            [BsonIndex(false)]
            public CSteamID ReporterID { get; set; }

            public DateTime ReportDate { get; set; }

            public string ReportText { get; set; }

        }

        public void MySqlAddReport(CSteamID ReportedID, CSteamID ReporterID, string ReportText)
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
                SQLcommand.ExecuteNonQuery();
                SQLconnection.Close();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        public bool MySqlDelReport(string ID)
        {
            bool result;
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
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, null);
                result = false;
            }
            return result;
        }

    }
}