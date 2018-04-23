using System;
using System.Collections.Generic;

public class TorqueModule : Module
{
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
			List<string> result;
			if (this.showParametersDescription)
			{
				(result = new List<string>()).Add("Torque: " + this.torque.floatValue + " kN");
			}
			else
			{
				result = new List<string>();
			}
			return result;
		}
	}

	public void UpdateTorque(float newTorque)
	{
		base.transform.root.GetComponent<Vessel>().partsManager.UpdateTorque();
	}

	public TorqueModule()
	{
	}

	public FloatValueHolder torque;

	public bool showParametersDescription = true;
}
