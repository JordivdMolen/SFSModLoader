using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class GameEvent : ScriptableObject
{
	[Button("Raise", 0)]
	public void Raise()
	{
		for (int i = this.eventListeners.Count - 1; i >= 0; i--)
		{
			this.eventListeners[i].OnEventRaised();
		}
	}

	public void RegisterListener(GameEventListener listener)
	{
		bool flag = !this.eventListeners.Contains(listener);
		if (flag)
		{
			this.eventListeners.Add(listener);
		}
	}

	public void UnregisterListener(GameEventListener listener)
	{
		bool flag = this.eventListeners.Contains(listener);
		if (flag)
		{
			this.eventListeners.Remove(listener);
		}
	}

	public GameEvent()
	{
	}

	[ReadOnly]
	public List<GameEventListener> eventListeners = new List<GameEventListener>();
}
