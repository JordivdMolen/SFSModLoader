using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JMtech.JDIS.Web.Response
{
	public class JDISRocket
	{
		public Dictionary<string, int> getVotes()
		{
			Dictionary<string, int> result = new Dictionary<string, int>();
			try
			{
				if (this.voted.GetType().Name.Equals(typeof(JObject).Name))
				{
					result = ((JObject)this.voted).ToObject<Dictionary<string, int>>();
				}
			}
			catch (Exception ex)
			{
			}
			return result;
		}

		public int authorId;

		public string title;

		public List<string> tags;

		public string rocketJson;

		public List<JDISComment> comments;

		public int score;

		public int downloads;

		public string rocketId;

		public int dateCreated;

		public bool publicVisible;

		public object voted;
	}
}
