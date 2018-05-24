using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class TorqueModule : Module
{
	public override bool IsSorted()
	{
		return true;
	}

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

	public void Torque(float newTorque)
	{
		base.transform.root.GetComponent<Vessel>().partsManager.UpdateTorque();
	}

	[InlineProperty]
	public FloatValueHolder torque;

	public bool showParametersDescription = true;
}
