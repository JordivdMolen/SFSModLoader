using System;

public class LandingLegModule : Module
{
	public override void OnPartUsed()
	{
		if (Ref.timeWarping)
		{
			MsgController.ShowMsg((this.moveModule.targetTime.floatValue != this.closed) ? this.onTimeWarpWarningClose : this.onTimeWarpWarningOpen);
		}
		else
		{
			this.moveModule.SetTargetTime((this.moveModule.targetTime.floatValue != this.closed) ? this.closed : this.open);
		}
	}

	public MoveModule moveModule;

	public string onTimeWarpWarningOpen;

	public string onTimeWarpWarningClose;

	public float closed;

	public float open;
}
