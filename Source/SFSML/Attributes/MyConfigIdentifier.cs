using System;

namespace SFSML.Attributes
{
	public class MyConfigIdentifier : Attribute
	{
		public MyConfigIdentifier(Type configType)
		{
			this.cfgType = configType;
		}

		public object instanciateConfig()
		{
			return Activator.CreateInstance(this.cfgType);
		}

		public readonly Type cfgType;
	}
}
