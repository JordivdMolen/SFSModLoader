using System;
using System.Collections.Generic;

public class WheelModule : Module
{
	public override List<object> SaveVariables
	{
		get
		{
			return new List<object>
			{
				this.on
			};
		}
	}

	public override void OnPartUsed()
	{
		this.on.boolValue = !this.on.boolValue;
		Ref.controller.ShowMsg("Rover wheel " + ((!this.on.boolValue) ? "Off" : "On"));
	}

	public WheelModule()
	{
	}

	public Wheel wheel;

	public BoolValueHolder on;
}
