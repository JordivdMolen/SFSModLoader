using System;

namespace ServerTransferProgram.LogicControllers
{
	public class DelayedUpdateTPS : Delayer
	{
		public DelayedUpdateTPS(int loopsPerSecond)
		{
			this.holdMS = 1000 / loopsPerSecond;
		}
	}
}
