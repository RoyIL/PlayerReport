using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using System.Collections.Generic;

namespace RG.PlayerReport
{
    public class CommandReports : IRocketCommand
    {
        public string Help
        {
            get { return "eport options, delete, view, etc ..."; }
        }

        public string Name
        {
            get { return "reports"; }
        }

        public string Syntax
        {
            get { return "reports view/del/list/page <ReportID>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get
            {
                return AllowedCaller.Both;
            }
        }

        public List<string> Permissions
		{
			get
			{
				return new List<string>()
				{
					"RocketReports",
					"RocketReports.view",
					"RocketReports.del",
					"RocketReports.del"
				};
			}
		}

        public void Execute(IRocketPlayer caller, params string[] command)
        {
			if (command.Length == 2)
			{
				if (true)
				{
					int repid;
					bool isNumeric = int.TryParse(command[1], out repid);
					if (isNumeric)
					{
						if (PlayerReport.Instance.MySQLON)
						{

							PlayerReport.Instance.Database.MySqlDelReport(caller, command[1]);
						}
						else
						{
							PlayerReport.Instance.Database.LiteDBDelReport(caller, command[1]);
						}
					}
					else
					{
						//working pls ignore 
						//command_del_invalid
					}
				}
				
			}
			else
			{
				UnturnedChat.Say(caller, "Try /reports " + Syntax);
				return;
			}
        }
    }
}
