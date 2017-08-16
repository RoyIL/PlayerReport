using Rocket.API;

namespace RG.PlayerReport
{
	public class PlayerReportConfiguration : IRocketPluginConfiguration
    {
        public string DatabaseAddress;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string DatabaseName;
        public string DatabaseTableName;
        public int DatabasePort;
        public bool UseMYSQL;
		public int Notifications;
		public bool MaxCharacter;
        public int LimCharacter;
		public bool TestCode;
		public string KeyTestCode;
		public bool LogFile;

		public void LoadDefaults()
		{
			DatabaseAddress = "localhost";
			DatabaseUsername = "root";
			DatabasePassword = "password";
			DatabaseName = "Unturned";
			DatabaseTableName = "Reports";
			DatabasePort = 3306;
			UseMYSQL = true;
			Notifications = 3;
			MaxCharacter = true;
			LimCharacter = 150;
			TestCode = false;
			KeyTestCode = "admintests1567";
			LogFile = false;
		}
    }
}
