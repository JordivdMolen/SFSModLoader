using System;
using System.Collections.Generic;
using UnityEngine;

public class Module : MonoBehaviour
{
	private void Awake()
	{
		this.part = base.GetComponent<Part>();
	}

	private void OnValidate()
	{
		this.part = base.GetComponent<Part>();
	}

	public virtual List<object> SaveVariables
	{
		get
		{
			return new List<object>();
		}
	}

	public virtual List<string> DescriptionVariables
	{
		get
		{
			return new List<string>();
		}
	}

	public virtual void OnPartUsed()
	{
	}

	public virtual void OnPartLoaded()
	{
	}

	public void Load(Module.Save loadedSave)
	{
		float num = 0f;
		List<object> saveVariables = this.SaveVariables;
		int num2 = 0;
		for (int i = 0; i < saveVariables.Count; i++)
		{
			bool flag = saveVariables[i] is FloatValueHolder;
			if (flag)
			{
				bool flag2 = saveVariables.Count > i && loadedSave.valuesList.Length > num2 && float.TryParse(loadedSave.valuesList[num2], out num);
				if (flag2)
				{
					(saveVariables[i] as FloatValueHolder).floatValue = float.Parse(loadedSave.valuesList[num2]);
				}
				num2++;
			}
			else
			{
				bool flag3 = saveVariables[i] is FloatValueHolder[];
				if (flag3)
				{
					bool flag4 = saveVariables.Count > i;
					if (flag4)
					{
						FloatValueHolder[] array = saveVariables[i] as FloatValueHolder[];
						for (int j = 0; j < array.Length; j++)
						{
							bool flag5 = loadedSave.valuesList.Length > num2 && float.TryParse(loadedSave.valuesList[num2], out num);
							if (flag5)
							{
								array[j].floatValue = float.Parse(loadedSave.valuesList[num2]);
							}
							num2++;
						}
					}
				}
				else
				{
					bool flag6 = saveVariables[i] is BoolValueHolder;
					if (flag6)
					{
						bool flag7 = saveVariables.Count > i && loadedSave.valuesList.Length > num2;
						if (flag7)
						{
							(saveVariables[i] as BoolValueHolder).boolValue = (loadedSave.valuesList[num2] == "True");
						}
						num2++;
					}
				}
			}
		}
	}

	public Module()
	{
	}

	[HideInInspector]
	public Part part;

	[Serializable]
	public class Save
	{
		public Save(string moduleName, List<object> variablesToSave)
		{
			this.moduleName = moduleName;
			List<string> list = new List<string>();
			for (int i = 0; i < variablesToSave.Count; i++)
			{
				bool flag = variablesToSave[i] is FloatValueHolder;
				if (flag)
				{
					list.Add((variablesToSave[i] as FloatValueHolder).floatValue.ToString());
				}
				else
				{
					bool flag2 = variablesToSave[i] is FloatValueHolder[];
					if (flag2)
					{
						FloatValueHolder[] array = variablesToSave[i] as FloatValueHolder[];
						for (int j = 0; j < array.Length; j++)
						{
							list.Add(array[j].floatValue.ToString());
						}
					}
					else
					{
						bool flag3 = variablesToSave[i] is BoolValueHolder;
						if (flag3)
						{
							list.Add((variablesToSave[i] as BoolValueHolder).boolValue.ToString());
						}
					}
				}
			}
			this.valuesList = list.ToArray();
		}

		public string moduleName;

		public string[] valuesList = new string[0];
	}
}
