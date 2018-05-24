using System;

namespace ServerControlSoftware.Externals.Shared.RequestPackets
{
	public class SharingCategory
	{
		public SharingCategory(string name)
		{
			this.categoryName = name;
		}

		public static readonly SharingCategory TOP = new SharingCategory("TOP");

		public static readonly SharingCategory HOT = new SharingCategory("HOT");

		public static readonly SharingCategory NEW = new SharingCategory("NEW");

		public static readonly SharingCategory OWN = new SharingCategory("OWN");

		public string categoryName;
	}
}
