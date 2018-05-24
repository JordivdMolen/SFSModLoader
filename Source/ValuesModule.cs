using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

[DisallowMultipleComponent]
public class ValuesModule : global::Module
{
	private void Start()
	{
		for (int i = 0; i < this.values.Length; i++)
		{
			for (int j = 0; j < this.values[i].methodsHolder.Length; j++)
			{
				Component component = this.values[i].methodsHolder[j].component;
				string methodName = this.values[i].methodsHolder[j].methodName;
				ValuesModule.UpdateFloatValueDelegate updateFloatValueDelegate = Delegate.CreateDelegate(typeof(ValuesModule.UpdateFloatValueDelegate), component, methodName) as ValuesModule.UpdateFloatValueDelegate;
				if (this.values[i].updateDelegate != null)
				{
					ValuesModule.Holder holder = this.values[i];
					holder.updateDelegate = (ValuesModule.UpdateFloatValueDelegate)Delegate.Combine(holder.updateDelegate, updateFloatValueDelegate);
				}
				else
				{
					this.values[i].updateDelegate = updateFloatValueDelegate;
				}
			}
		}
		for (int k = 0; k < this.values.Length; k++)
		{
			this.values[k].hasValidDelegate = (this.values[k].updateDelegate != null);
			if (this.values[k].hasValidDelegate)
			{
				this.values[k].updateDelegate(this.values[k].floatValue);
			}
		}
	}

	[Button(ButtonSizes.Small)]
	public void ScanForMethods()
	{
		Component[] components = base.gameObject.GetComponents<Component>();
		foreach (Component component in components)
		{
			MemberInfo[] members = component.GetType().GetMembers();
			for (int j = 0; j < members.Length; j++)
			{
				if (members[j].ToString().Contains("FloatValueHolder "))
				{
					MethodInfo[] methods = component.GetType().GetMethods();
					for (int k = 0; k < methods.Length; k++)
					{
						string name = members[j].Name;
						if (methods[k].Name.ToLower() == name.ToLower())
						{
							FloatValueHolder floatValueHolder = component.GetType().GetField(name).GetValue(component) as FloatValueHolder;
							if (floatValueHolder.useExternalReference)
							{
								List<ValuesModule.Holder.MethodRefHolder> list = new List<ValuesModule.Holder.MethodRefHolder>(this.values[floatValueHolder.index].methodsHolder);
								list.Add(new ValuesModule.Holder.MethodRefHolder(component, methods[k].Name));
								this.values[floatValueHolder.index].methodsHolder = list.ToArray();
							}
						}
					}
				}
			}
		}
		for (int l = 0; l < this.links.Length; l++)
		{
			this.AddToUpdateDelegates(this.links[l].a);
			this.AddToUpdateDelegates(this.links[l].b);
		}
	}

	private void AddToUpdateDelegates(FloatValueHolder a)
	{
		if (!a.useExternalReference)
		{
			return;
		}
		List<ValuesModule.Holder.MethodRefHolder> list = new List<ValuesModule.Holder.MethodRefHolder>(this.values[a.index].methodsHolder);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].methodName == "Recalculate")
			{
				return;
			}
		}
		list.Add(new ValuesModule.Holder.MethodRefHolder(this, "Recalculate"));
		this.values[a.index].methodsHolder = list.ToArray();
	}

	public void Recalculate(float empty)
	{
		for (int i = 0; i < this.links.Length; i++)
		{
			this.links[i].Recalculate();
		}
	}

	public ValuesModule.Holder[] values = new ValuesModule.Holder[0];

	public ValuesModule.Link[] links;

	public delegate void UpdateFloatValueDelegate(float oldValue);

	[Serializable]
	public class Holder
	{
		private string Draw(int index, GUIContent label)
		{
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			return this.name;
		}

		[HorizontalGroup(0f, 0, 0, 0)]
		[LabelWidth(50f)]
		[LabelText("Name:")]
		public string name;

		[HorizontalGroup(0f, 0, 0, 0)]
		[HideLabel]
		public float floatValue;

		[HorizontalGroup(0f, 0, 0, 0)]
		[HideLabel]
		public Color label = Color.white;

		[HideInInspector]
		public bool hasValidDelegate;

		public ValuesModule.UpdateFloatValueDelegate updateDelegate;

		[LabelText("On Value Change")]
		public ValuesModule.Holder.MethodRefHolder[] methodsHolder = new ValuesModule.Holder.MethodRefHolder[0];

		[Serializable]
		public class MethodRefHolder
		{
			public MethodRefHolder(Component component, string methodName)
			{
				this.component = component;
				this.methodName = methodName;
			}

			[HorizontalGroup(0f, 0, 0, 0)]
			public Component component;

			[HorizontalGroup(0f, 0, 0, 0)]
			[CustomValueDrawer("Draw")]
			public string methodName;
		}
	}

	[Serializable]
	public class Link
	{
		public void Recalculate()
		{
			this.output.floatValue = ((this.operation != ValuesModule.Link.Type.Multiply) ? (this.a.floatValue + this.b.floatValue) : (this.a.floatValue * this.b.floatValue));
		}

		[InlineProperty]
		[BoxGroup]
		public FloatValueHolder a;

		[InlineProperty]
		[BoxGroup]
		public FloatValueHolder b;

		[BoxGroup]
		public ValuesModule.Link.Type operation;

		[InlineProperty]
		[BoxGroup]
		public FloatValueHolder output;

		public enum Type
		{
			Add,
			Multiply
		}
	}
}
