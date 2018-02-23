using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Events;

public class CustomEvent : MonoBehaviour
{
	[HideLabel, Space(3f)]
	public string function;

	public UnityEvent customEvent;

	[Button(ButtonSizes.Small)]
	public void InvokeEvenets()
	{
		this.customEvent.Invoke();
	}

	public void InvokeEventsDelayed(float delay)
	{
		base.Invoke("InvokeEvenets", delay);
	}
}
