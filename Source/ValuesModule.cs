using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ValuesModule : Module
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
				bool flag = this.values[i].updateDelegate != null;
				if (flag)
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
			bool hasValidDelegate = this.values[k].hasValidDelegate;
			if (hasValidDelegate)
			{
				this.values[k].updateDelegate(this.values[k].floatValue);
			}
		}
	}

	public ValuesModule()
	{
	}

	public ValuesModule.Holder[] values = new ValuesModule.Holder[0];

	public delegate void UpdateFloatValueDelegate(float oldValue);

	[Serializable]
	public class Holder
	{
		public Holder()
		{
		}

		public string use;

		public float floatValue;

		public Color label = Color.white;

		public bool hasValidDelegate;

		public ValuesModule.UpdateFloatValueDelegate updateDelegate;

		public ValuesModule.Holder.MethodRefHolder[] methodsHolder = new ValuesModule.Holder.MethodRefHolder[0];

		[Serializable]
		public class MethodRefHolder
		{
			public MethodRefHolder(Component component, string methodName)
			{
				this.component = component;
				this.methodName = methodName;
			}

			public Component component;

			public string methodName;
		}
	}
}
