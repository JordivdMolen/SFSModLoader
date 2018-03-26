using System;

namespace SFSML.Attributes
{
	public class MyConfigIdentifier : Attribute
	{
		public readonly Type cfgType;
		public MyConfigIdentifier(Type configType)
		{
			this.cfgType = configType;
		}
		public object instanciateConfig()
		{
			return Activator.CreateInstance(this.cfgType);
		}
	}
}

