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
		bool flag = this.modifiers.Length != this.types.Length;
		if (flag)
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
			bool flag2 = this.modifiers[j].valuesHolder != null;
			if (flag2)
			{
				bool flag3 = false;
				int num = 0;
				while (num < this.modifiers[j].valuesHolder.values[this.modifiers[j].index].methodsHolder.Length && !flag3)
				{
					bool flag4 = this.modifiers[j].valuesHolder.values[this.modifiers[j].index].methodsHolder[num].component == this && this.modifiers[j].valuesHolder.values[this.modifiers[j].index].methodsHolder[num].methodName == "Calculate";
					if (flag4)
					{
						flag3 = true;
					}
					num++;
				}
				bool flag5 = !flag3;
				if (flag5)
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
			bool flag = this.types[i] == LinkModule.Type.Multiply;
			if (flag)
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

	public LinkModule()
	{
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
