using System;
using System.Collections.Generic;

namespace JMtech.JDIS.Web.Request
{
	public class JDISUploadRocket : JDISRequestExtension
	{
		public JDISUploadRocket(string title, List<string> tags, string json, bool premium)
		{
			this.rocketTitle = title;
			this.rocketTags = tags;
			this.rocketJson = json;
			this.premiumUser = premium;
		}

		public override object Initiate(BaseRequest tgt)
		{
			tgt.SEC = "ROCKET_SHARE";
			return this;
		}

		public string ACTION = "ADD";

		public string rocketTitle;

		public List<string> rocketTags;

		public string rocketJson;

		public bool premiumUser;
	}
}
