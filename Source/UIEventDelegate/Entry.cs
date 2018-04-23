using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIEventDelegate
{
	[Serializable]
	public class Entry : IComparer<Entry>, IComparable<Entry>
	{
		public Entry()
		{
		}

		public Entry(UnityEngine.Object target, string name)
		{
			this.target = target;
			this.name = name;
		}

		public int CompareTo(Entry other)
		{
			return this.name.CompareTo(other.name);
		}

		public int Compare(Entry x, Entry y)
		{
			return x.name.CompareTo(y.name);
		}

		public override bool Equals(object obj)
		{
			bool flag = obj == null;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = obj is Entry;
				if (flag2)
				{
					Entry entry = obj as Entry;
					bool flag3 = entry == null;
					if (flag3)
					{
						return false;
					}
					bool flag4 = this.target == entry.target && this.name == entry.name;
					if (flag4)
					{
						return true;
					}
				}
				result = false;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return Entry.entryHash;
		}

		static Entry()
		{
			// Note: this type is marked as 'beforefieldinit'.
		}

		public UnityEngine.Object target;

		public string name;

		private static int entryHash = "Entry".GetHashCode();
	}
}
