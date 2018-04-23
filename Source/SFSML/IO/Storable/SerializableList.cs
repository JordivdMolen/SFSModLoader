using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SFSML.IO.Storable
{
	public class SerializableList<T> : IEnumerator<T>, IDisposable, IEnumerator
	{
		public void Add(T toAdd)
		{
			List<SerializableObject<T>> container = this.getContainer();
			container.Add(new SerializableObject<T>(toAdd));
			this.SetContent(container);
		}

		public string getSerialized()
		{
			return this.holder;
		}

		private List<SerializableObject<T>> getContainer()
		{
			List<string> list = new List<string>();
			List<SerializableObject<T>> list2 = new List<SerializableObject<T>>();
			string text = "";
			bool flag = false;
			foreach (char c in this.holder.ToCharArray())
			{
				string text2 = c.ToString();
				bool flag2 = flag;
				if (flag2)
				{
					text += text2;
					flag = false;
				}
				else
				{
					bool flag3 = text2 == "$" && !flag;
					if (flag3)
					{
						list.Add(text);
						text = "";
					}
					else
					{
						bool flag4 = text2 == "\\";
						if (flag4)
						{
							flag = true;
						}
						else
						{
							text += text2;
						}
					}
				}
			}
			bool flag5 = text != "";
			if (flag5)
			{
				list.Add(text);
			}
			foreach (string text3 in list)
			{
				ModLoader.mainConsole.log(text3);
				list2.Add(JsonUtility.FromJson<SerializableObject<T>>(text3));
			}
			return list2;
		}

		private void SetContent(List<SerializableObject<T>> content)
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			foreach (SerializableObject<T> obj in content)
			{
				list.Add(JsonUtility.ToJson(obj));
			}
			foreach (string text in list)
			{
				string text2 = "";
				bool flag = false;
				for (int i = 0; i < text.Length; i++)
				{
					string text3 = text[i].ToString();
					bool flag2 = flag;
					if (flag2)
					{
						text2 += text3;
						flag = false;
					}
					else
					{
						bool flag3 = text3 == "$" && !flag;
						if (flag3)
						{
							text2 += "\\$";
						}
						else
						{
							bool flag4 = text3 == "\\";
							if (flag4)
							{
								flag = true;
							}
							else
							{
								text2 += text3;
							}
						}
					}
				}
				list2.Add(text2);
			}
			string text4 = string.Join("$", list2.ToArray());
			this.holder = text4;
		}

		private T objectOn(int place)
		{
			return this.getContainer()[place].content;
		}

		public T Current
		{
			get
			{
				return this.objectOn(this.position);
			}
		}

		object IEnumerator.Current
		{
			get
			{
				return this.objectOn(this.position);
			}
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			this.position++;
			return this.position < this.count;
		}

		public void Reset()
		{
			this.position = 0;
		}

		public SerializableList()
		{
		}

		private string holder = "";

		private int position = 0;

		private int count = 0;
	}
}
