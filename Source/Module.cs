using System;
using System.Collections.Generic;
using UnityEngine;

public class Module : MonoBehaviour
{
	[Serializable]
	public class Save
	{
		public string moduleName;

		public string[] valuesList = new string[0];

		public Save(string moduleName, List<object> variablesToSave)
		{
			this.moduleName = moduleName;
			List<string> list = new List<string>();
			for (int i = 0; i < variablesToSave.Count; i++)
			{
				if (variablesToSave[i] is FloatValueHolder)
				{
					list.Add((variablesToSave[i] as FloatValueHolder).floatValue.ToString());
				}
				else if (variablesToSave[i] is FloatValueHolder[])
				{
					FloatValueHolder[] array = variablesToSave[i] as FloatValueHolder[];
					for (int j = 0; j < array.Length; j++)
					{
						list.Add(array[j].floatValue.ToString());
					}
				}
				else if (variablesToSave[i] is BoolValueHolder)
				{
					list.Add((variablesToSave[i] as BoolValueHolder).boolValue.ToString());
				}
			}
			this.valuesList = list.ToArray();
		}
	}

	[HideInInspector]
	public Part part;

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

	private void Awake()
	{
		this.part = base.GetComponent<Part>();
	}

	private void OnValidate()
	{
		this.part = base.GetComponent<Part>();
	}

	public virtual void OnPartUsed()
	{
	}

	public void Load(Module.Save loadedSave)
	{
		List<object> saveVariables = this.SaveVariables;
		int num = 0;
		for (int i = 0; i < saveVariables.Count; i++)
		{
			if (saveVariables[i] is FloatValueHolder)
			{
				(saveVariables[i] as FloatValueHolder).floatValue = float.Parse(loadedSave.valuesList[num]);
				num++;
			}
			else if (saveVariables[i] is FloatValueHolder[])
			{
				FloatValueHolder[] array = saveVariables[i] as FloatValueHolder[];
				for (int j = 0; j < array.Length; j++)
				{
					array[j].floatValue = float.Parse(loadedSave.valuesList[num]);
					num++;
				}
			}
			else if (saveVariables[i] is BoolValueHolder)
			{
				(saveVariables[i] as BoolValueHolder).boolValue = (loadedSave.valuesList[num] == "True");
				num++;
			}
		}
	}
}
