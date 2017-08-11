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
		public bool LogFile;
		public bool MaxCharacter;
        public int LimCharacter;

        public void LoadDefaults()
        {
            DatabaseAddress = "localhost";
            DatabaseUsername = "root";
            DatabasePassword = "password";
            DatabaseName = "Unturned";
            DatabaseTableName = "Reports";
            DatabasePort = 3306;
            UseMYSQL = true;
			LogFile = false;
			MaxCharacter = true;
            LimCharacter = 150;
        }
    }
}
