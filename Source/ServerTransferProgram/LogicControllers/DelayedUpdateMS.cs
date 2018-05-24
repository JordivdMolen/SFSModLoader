using System;

namespace ServerTransferProgram.LogicControllers
{
	public class DelayedUpdateMS : Delayer
	{
		public DelayedUpdateMS(int holdPerLoop)
		{
			this.holdMS = holdPerLoop;
		}
	}
}
