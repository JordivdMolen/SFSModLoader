using System;

namespace Assets.Scripts.Addons.JMtech.ShareController
{
	public class PageContainer
	{
		public PageContainer(string title, string id, string json)
		{
			this.rocketTitle = title;
			this.rocketId = id;
			this.rocketJson = json;
		}

		public string rocketTitle;

		public string rocketId;

		public string rocketJson;
	}
}
