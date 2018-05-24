using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class LinkModule : Module
{
	public override List<object> SaveVariables
	{
		get
		{
			return new List<object>
			{
				this.modifiers,
				this.Out
			};
		}
	}

	private void OnValidate()
	{
		if (this.modifiers.Length != this.types.Length)
		{
			LinkModule.Type[] array = new LinkModule.Type[this.modifiers.Length];
			for (int i = 0; i < Mathf.Min(this.modifiers.Length, this.types.Length); i++)
			{
				array[i] = this.types[i];
			}
			this.types = array;
		}
		for (int j = 0; j < this.modifiers.Length; j++)
		{
			if (this.modifiers[j].valuesHolder != null)
			{
				bool flag = false;
				int num = 0;
				while (num < this.modifiers[j].valuesHolder.values[this.modifiers[j].index].methodsHolder.Length && !flag)
				{
					if (this.modifiers[j].valuesHolder.values[this.modifiers[j].index].methodsHolder[num].component == this && this.modifiers[j].valuesHolder.values[this.modifiers[j].index].methodsHolder[num].methodName == "Calculate")
					{
						flag = true;
					}
					num++;
				}
				if (!flag)
				{
					List<ValuesModule.Holder.MethodRefHolder> list = new List<ValuesModule.Holder.MethodRefHolder>(this.modifiers[j].valuesHolder.values[this.modifiers[j].index].methodsHolder);
					list.Add(new ValuesModule.Holder.MethodRefHolder(this, "Calculate"));
					this.modifiers[j].valuesHolder.values[this.modifiers[j].index].methodsHolder = list.ToArray();
				}
			}
		}
	}

	public void Calculate(float emptyValue)
	{
		float num = this.In;
		for (int i = 0; i < this.modifiers.Length; i++)
		{
			if (this.types[i] == LinkModule.Type.Multiply)
			{
				num *= this.modifiers[i].floatValue;
			}
			else
			{
				num += this.modifiers[i].floatValue;
			}
		}
		this.Out.floatValue = num;
	}

	[BoxGroup("A", false, false, 0)]
	public float In;

	[Space]
	[BoxGroup("A", false, false, 0)]
	[HorizontalGroup("A/A", 0f, 0, 0, 0)]
	public FloatValueHolder[] modifiers;

	[Space]
	[BoxGroup("A", false, false, 0)]
	[HorizontalGroup("A/A", 0f, 0, 0, 0)]
	public LinkModule.Type[] types = new LinkModule.Type[0];

	[BoxGroup("A", false, false, 0)]
	public FloatValueHolder Out;

	public enum Type
	{
		Multiply,
		Add
	}
}
