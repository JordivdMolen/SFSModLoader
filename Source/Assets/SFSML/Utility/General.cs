using System;

namespace Assets.SFSML.Utility
{
	public class General
	{
		public static long getMillis()
		{
			return DateTime.Now.Ticks / 10000L;
		}

		public General()
		{
		}
	}
}
