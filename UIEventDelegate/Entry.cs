using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIEventDelegate
{
	[Serializable]
	public class Entry : IComparer<Entry>, IComparable<Entry>
	{
		public UnityEngine.Object target;

		public string name;

		private static int entryHash = "Entry".GetHashCode();

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
			if (obj == null)
			{
				return false;
			}
			if (obj is Entry)
			{
				Entry entry = obj as Entry;
				if (entry == null)
				{
					return false;
				}
				if (this.target == entry.target && this.name == entry.name)
				{
					return true;
				}
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Entry.entryHash;
		}
	}
}
