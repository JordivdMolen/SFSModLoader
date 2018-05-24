using System;

namespace ServerTransferProgram.LogicControllers.EventTypes
{
	public class CancellableEvent : Event
	{
		public void SetCancelled(bool state)
		{
			this.Cancel = state;
		}

		private bool Cancel;
	}
}
