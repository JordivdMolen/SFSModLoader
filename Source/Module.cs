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

    public virtual bool IsSorted()
    {
        return false;
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
            if (saveVariables[i] is FloatValueHolder)
            {
                if (saveVariables.Count > i && loadedSave.valuesList.Length > num2 && float.TryParse(loadedSave.valuesList[num2], out num))
                {
                    (saveVariables[i] as FloatValueHolder).floatValue = float.Parse(loadedSave.valuesList[num2]);
                }
                num2++;
            }
            else if (saveVariables[i] is FloatValueHolder[])
            {
                if (saveVariables.Count > i)
                {
                    FloatValueHolder[] array = saveVariables[i] as FloatValueHolder[];
                    for (int j = 0; j < array.Length; j++)
                    {
                        if (loadedSave.valuesList.Length > num2 && float.TryParse(loadedSave.valuesList[num2], out num))
                        {
                            array[j].floatValue = float.Parse(loadedSave.valuesList[num2]);
                        }
                        num2++;
                    }
                }
            }
            else if (saveVariables[i] is BoolValueHolder)
            {
                if (saveVariables.Count > i && loadedSave.valuesList.Length > num2)
                {
                    (saveVariables[i] as BoolValueHolder).boolValue = (loadedSave.valuesList[num2] == "True");
                }
                num2++;
            }
        }
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

        public string moduleName;

        public string[] valuesList = new string[0];
    }
}
