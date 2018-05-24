using System;

namespace ServerControlSoftware.Externals.Shared.Objects
{
	public class AuthID
	{
		public int ID;

		public string KEY1;

		public string KEY2;

		private static bool inUse;

		private static int LastID = -1;
	}
}
