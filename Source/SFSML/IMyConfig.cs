using System;

namespace SFSML
{
	public abstract class IMyConfig
	{
		public abstract void SetupDefaults();

		public void save()
		{
			this.parent.save();
		}

		public void setParent(MyConfig par)
		{
			bool flag = this.parent == null;
			if (flag)
			{
				this.parent = par;
			}
		}

		protected IMyConfig()
		{
		}

		[NonSerialized]
		private MyConfig parent = null;
	}
}
