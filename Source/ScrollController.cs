using System;
using UnityEngine;

public class ScrollController : MonoBehaviour
{
	private void Start()
	{
		this.scrollObject = (this.scrollObject ?? base.gameObject);
		this.rectHolder = this.scrollHolder.GetComponent<RectTransform>();
		this.rectObject = this.scrollObject.GetComponent<RectTransform>();
		this.scrollDelta = Mathf.Abs(this.rectHolder.sizeDelta.y - this.rectObject.sizeDelta.y);
	}

	private void Update()
	{
	}

	public GameObject scrollHolder;

	public GameObject scrollObject;

	public float scrollDelta;

	private RectTransform rectHolder;

	private RectTransform rectObject;
}
