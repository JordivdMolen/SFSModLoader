using System;

namespace JMtech.Translations
{
	public class SFST : Translation
	{
		public override void Init()
		{
			SFST.T = this;
		}

		public string Title = "Spaceflight Simulator";

		public string Sharing_No_Connection = "No connection";

		public string Sharing_No_Server = "Connection to server lost";

		public string Sharing_Upload_Success = "Rocket uploaded";

		public string Sharing_Upload_Failed = "Upload failed";

		public string Sharing_Vote_Placed = string.Empty;

		public string Sharing_Vote_Revoked = string.Empty;

		public string Sharing_Vote_Failed = string.Empty;

		public string Sharing_Nothing_To_See = string.Empty;

		public string Sharing_Wait_Request = string.Empty;

		public string Sharing_Old_Version = "This version is oudated, please update";

		public string Sharing_New_Version = "Somehow you are using a newer version? Black magic?!";

		public string Sharing_Old_Protocol = "This version is oudated, please update";

		public string Sharing_New_Protocol = "Somehow you are using a newer version? Black magic?!";

		public string STP_Request_Timeout = "Could not connect to server";

		public string STP_Old_Credentials = string.Empty;

		public string STP_Reconnected = "Reconnected";

		public static SFST T;
	}
}
