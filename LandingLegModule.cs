using System;

public class LandingLegModule : Module
{
	public MoveModule moveModule;

	public override void OnPartUsed()
	{
		if (Ref.timeWarping)
		{
			Ref.controller.ShowMsg("Cannot " + ((this.moveModule.targetTime.floatValue != 1f) ? "extend" : "retract") + " landing legs while time warping");
		}
		else
		{
			this.moveModule.SetTargetTime((float)((this.moveModule.targetTime.floatValue != 1f) ? 1 : 0));
		}
	}
}
