using Steamworks;
using System;

namespace RG.PlayerReport
{
	public class ReportData
	{
		public int Id { get; private set; }
		public CSteamID ReporterID { get; private set; }
		public CSteamID ReportedID { get; private set; }
		public DateTime ReportDate { get; internal set; }
		public string ReportInfo { get; internal set; }

		internal ReportData()
		{
			Id = 0;
			ReportedID = CSteamID.Nil;
		}

		internal ReportData(int id, CSteamID reporterID, CSteamID reportedID, DateTime reportDate, string reportInfo)
		{
			Id = id;
			ReporterID = reporterID;
			ReportedID = reportedID;
			ReportDate = reportDate;
			ReportInfo = reportInfo;
		}
	}
}