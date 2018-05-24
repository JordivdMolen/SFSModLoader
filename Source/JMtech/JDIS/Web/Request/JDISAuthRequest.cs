using System;

namespace JMtech.JDIS.Web.Request
{
	public class JDISAuthRequest : JDISRequestExtension
	{
		public override object Initiate(BaseRequest tgt)
		{
			tgt.AUTH = "GENERATE";
			return this;
		}
	}
}
