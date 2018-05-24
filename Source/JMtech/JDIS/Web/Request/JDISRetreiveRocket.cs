using System;

namespace JMtech.JDIS.Web.Request
{
	public class JDISRetreiveRocket : JDISRequestExtension
	{
		public JDISRetreiveRocket(string id)
		{
			this.targetID = id;
		}

		public JDISRetreiveRocket(int authorID, int rocketID)
		{
			this.targetID = authorID + "#" + rocketID;
		}

		public override object Initiate(BaseRequest tgt)
		{
			tgt.SEC = "ROCKET_SHARE";
			return this;
		}

		public string ACTION = "DL";

		public string targetID;
	}
}
