using System;

namespace SFSML.IO.Storable
{
	internal class SerializableObject<T>
	{
		public SerializableObject(T con)
		{
			this.content = con;
		}

		public T content;
	}
}
