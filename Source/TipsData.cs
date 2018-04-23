using System;
using UnityEngine;

[CreateAssetMenu]
public class TipsData : ScriptableObject
{
	public TipsData()
	{
	}

	[TextArea]
	public string[] tips;
}
