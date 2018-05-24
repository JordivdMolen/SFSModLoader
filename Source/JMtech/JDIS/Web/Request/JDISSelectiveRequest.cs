using System;
using System.Collections.Generic;

namespace JMtech.JDIS.Web.Request
{
	public class JDISSelectiveRequest : JDISRequestExtension
	{
		public JDISSelectiveRequest(string rocketID, List<string> callSign)
		{
			this.call = callSign;
			this.targetID = rocketID;
		}

		public override object Initiate(BaseRequest tgt)
		{
			tgt.SEC = "ROCKET_SHARE";
			return this;
		}

		public string ACTION = "SELECTIVE";

		public List<string> call;

		public string targetID;
	}
}
