using System;

namespace ServerTransferProgram.ServerControlLiberary.DataControllers.PacketSystem.Packets
{
	public class AuthenticationHeaderPacket : DefaultPacket
	{
		public AuthenticationHeaderPacket(int id, string key1, string key2)
		{
			this.ID = id;
			this.KEY1 = key1;
			this.KEY2 = key2;
		}

		public int ID;

		public string KEY1;

		public string KEY2;
	}
}
