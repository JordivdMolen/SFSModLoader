using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class FloatValueHolder
{
	public float floatValue
	{
		get
		{
			return (!this.useExternalReference) ? this.localFloat : this.valuesHolder.values[this.index].floatValue;
		}
		set
		{
			if (((!this.useExternalReference) ? this.localFloat : this.valuesHolder.values[this.index].floatValue) == value)
			{
				return;
			}
			if (this.useExternalReference)
			{
				this.valuesHolder.values[this.index].floatValue = value;
				if (this.valuesHolder.values[this.index].hasValidDelegate)
				{
					this.valuesHolder.values[this.index].updateDelegate(value);
				}
			}
			else
			{
				this.localFloat = value;
			}
		}
	}

	[HideInInspector]
	public bool useExternalReference;

	[HideInInspector]
	public float localFloat;

	[HorizontalGroup(0f, 0, 0, 0)]
	[CustomValueDrawer("DrawColor")]
	public int index = -1;

	[HorizontalGroup(0f, 0, 0, 0)]
	[HideLabel]
	[ShowIf("ShowValueModule", true)]
	public ValuesModule valuesHolder;
}
