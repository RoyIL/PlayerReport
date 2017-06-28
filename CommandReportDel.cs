using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using System.Collections.Generic;

namespace RG.PlayerReport
{
    public class CommandReportDel : IRocketCommand
    {
        public string Help
        {
            get { return "Delete reports"; }
        }

        public string Name
        {
            get { return "delreport"; }
        }

        public string Syntax
        {
            get { return "<ReportID>"; }
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
                return new List<string>() { "RocketReport.delreport" };
            }
        }

        public void Execute(IRocketPlayer caller, params string[] command)
        {
            if (command.Length != 1)
            {
                UnturnedChat.Say(caller, "Try /delreport" + Syntax);
                return;
            }
            if (PlayerReport.Instance.Configuration.Instance.UseMYSQL)
            {
                PlayerReport.Instance.Database.MySqlDelReport(command[0]);
                UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_del_successful"));
            }
            else if (!PlayerReport.Instance.Configuration.Instance.UseMYSQL)
            {
                PlayerReport.Instance.Database.LiteDBReport(command[0]);
                UnturnedChat.Say(caller, PlayerReport.Instance.Translate("command_del_successful"));
            }
        }
    }
}
