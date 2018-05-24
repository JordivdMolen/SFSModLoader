using System;

namespace JMtech.JDIS.Web.Request
{
	public class JDISRequestPage : JDISRequestExtension
	{
		public override object Initiate(BaseRequest tgt)
		{
			tgt.SEC = "ROCKET_SHARE";
			return this;
		}

		public string ACTION = "GET_PAGE";

		public int page;

		public string category = "top";
	}
}
