using System;

namespace ServerControlSoftware.Externals.Shared.Objects.Identification
{
	public class IDGenerator
	{
		public long GenerateID()
		{
			long lastID = this.LastID;
			this.LastID += 1L;
			return lastID;
		}

		public long LastID;
	}
}
