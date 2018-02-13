using System;
using System.Collections.Generic;

public class TorqueModule : Module
{
	public FloatValueHolder torque;

	public bool showParametersDescription = true;

	public override List<object> SaveVariables
	{
		get
		{
			return new List<object>
			{
				this.torque
			};
		}
	}

	public override List<string> DescriptionVariables
	{
		get
		{
			return (!this.showParametersDescription) ? new List<string>() : new List<string>
			{
				"Torque: " + this.torque.floatValue + " kN"
			};
		}
	}

	public void UpdateTorque(float newTorque)
	{
		base.transform.root.GetComponent<Vessel>().partsManager.UpdateTorque();
	}
}
