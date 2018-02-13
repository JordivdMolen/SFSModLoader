using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ValuesModule : Module
{
	public delegate void UpdateFloatValueDelegate(float oldValue);

	[Serializable]
	public class Holder
	{
		[Serializable]
		public class MethodRefHolder
		{
			public Component component;

			public string methodName;

			public MethodRefHolder(Component component, string methodName)
			{
				this.component = component;
				this.methodName = methodName;
			}
		}

		public string use;

		public float floatValue;

		public Color label = Color.white;

		public bool hasValidDelegate;

		public ValuesModule.UpdateFloatValueDelegate updateDelegate;

		public ValuesModule.Holder.MethodRefHolder[] methodsHolder = new ValuesModule.Holder.MethodRefHolder[0];
	}

	public ValuesModule.Holder[] values = new ValuesModule.Holder[0];

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
					ValuesModule.Holder expr_6A = this.values[i];
					expr_6A.updateDelegate = (ValuesModule.UpdateFloatValueDelegate)Delegate.Combine(expr_6A.updateDelegate, updateFloatValueDelegate);
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
}
