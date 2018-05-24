using System;

namespace JMtech.JDIS.Web.Request
{
	public class JDISChangeName : JDISRequestExtension
	{
		public JDISChangeName(string newName)
		{
			this.NICK = newName;
		}

		public override object Initiate(BaseRequest tgt)
		{
			tgt.SEC = "USER_MAN";
			return this;
		}

		public string ACTION = "NICK";

		public string NICK;
	}
}
