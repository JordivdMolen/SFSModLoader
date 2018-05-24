using System;

namespace ServerControlSoftware.Externals.Shared.RequestPackets
{
	public class VotePacket : AuthedPacket
	{
		public string voteOn;

		public bool vote;
	}
}
