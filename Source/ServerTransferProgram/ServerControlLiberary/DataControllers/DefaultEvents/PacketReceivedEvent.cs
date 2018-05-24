using System;
using ServerTransferProgram.LogicControllers.EventTypes;
using ServerTransferProgram.ServerControlLiberary.DataControllers.PacketSystem;

namespace ServerTransferProgram.ServerControlLiberary.DataControllers.DefaultEvents
{
	public class PacketReceivedEvent : Event
	{
		public PacketReceivedEvent(DefaultPacket pack)
		{
			this.receivedPacket = pack;
		}

		public DefaultPacket receivedPacket;
	}
}
