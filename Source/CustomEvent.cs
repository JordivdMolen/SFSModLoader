using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class CustomEvent : MonoBehaviour
{
	[Button(ButtonSizes.Small)]
	public void InvokeEvenets()
	{
		this.customEvent.Invoke();
	}

	public void InvokeEventsDelayed(float delay)
	{
		base.Invoke("InvokeEvenets", delay);
	}

	[HideLabel]
	[Space(3f)]
	public string function;

	public UnityEvent customEvent;
}
