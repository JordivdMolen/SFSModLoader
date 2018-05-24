using System;

namespace JMtech.JDIS.Web.Request
{
	public class JDISVote : JDISRequestExtension
	{
		public JDISVote(string tgt, string vote)
		{
			this.targetID = tgt;
			this.VOTE = vote;
		}

		public override object Initiate(BaseRequest tgt)
		{
			tgt.SEC = "ROCKET_SHARE";
			return this;
		}

		public string ACTION = "VOTE";

		public string targetID;

		public string VOTE;
	}
}
