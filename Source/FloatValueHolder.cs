using System;

[Serializable]
public class FloatValueHolder
{
	public float localFloat;

	public bool useExternalReference;

	public ValuesModule valuesHolder;

	public int index = -1;

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
}
