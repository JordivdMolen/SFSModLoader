using System;

namespace JMtech.JDIS.Web.Request
{
	public abstract class JDISRequestExtension
	{
		public abstract object Initiate(BaseRequest tgt);
	}
}
