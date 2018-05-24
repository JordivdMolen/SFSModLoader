using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameEvent : ScriptableObject
{
	[Button("Raise", ButtonSizes.Small)]
	public void Raise()
	{
		for (int i = this.eventListeners.Count - 1; i >= 0; i--)
		{
			this.eventListeners[i].OnEventRaised();
		}
	}

	public void RegisterListener(GameEventListener listener)
	{
		if (!this.eventListeners.Contains(listener))
		{
			this.eventListeners.Add(listener);
		}
	}

	public void UnregisterListener(GameEventListener listener)
	{
		if (this.eventListeners.Contains(listener))
		{
			this.eventListeners.Remove(listener);
		}
	}

	[ReadOnly]
	public List<GameEventListener> eventListeners = new List<GameEventListener>();
}
