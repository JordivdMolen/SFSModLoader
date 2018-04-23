using System;

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
			bool flag = ((!this.useExternalReference) ? this.localFloat : this.valuesHolder.values[this.index].floatValue) == value;
			if (!flag)
			{
				bool flag2 = this.useExternalReference;
				if (flag2)
				{
					this.valuesHolder.values[this.index].floatValue = value;
					bool hasValidDelegate = this.valuesHolder.values[this.index].hasValidDelegate;
					if (hasValidDelegate)
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

	public FloatValueHolder()
	{
	}

	public float localFloat;

	public bool useExternalReference;

	public ValuesModule valuesHolder;

	public int index = -1;
}
