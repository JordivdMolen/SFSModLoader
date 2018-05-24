﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class Warning : MonoBehaviour
{
	public void ShowWarning(string warning, Vector2 warningWindowSize, string confirm, Warning.EmptyDelegate onConfirm, string cancel, Warning.EmptyDelegate onCancel, int buttonSize)
	{
		base.gameObject.SetActive(true);
		this.warningText.text = warning;
		this.warningTextShade.sizeDelta = warningWindowSize;
		this.confirmText.text = confirm;
		this.cancelText.text = cancel;
		this.onConfirm = onConfirm;
		this.onCancel = onCancel;
		RectTransform component = this.confirmText.transform.parent.GetComponent<RectTransform>();
		RectTransform component2 = this.cancelText.transform.parent.GetComponent<RectTransform>();
		Vector2 vector = new Vector2((float)buttonSize, component.rect.height);
		RectTransform rectTransform = component;
		Vector2 vector2 = vector;
		component2.sizeDelta = vector2;
		rectTransform.sizeDelta = vector2;
		RectTransform component3 = component.GetChild(0).GetComponent<RectTransform>();
		vector2 = vector;
		component2.GetChild(0).GetComponent<RectTransform>().sizeDelta = vector2;
		component3.sizeDelta = vector2;
		BoxCollider2D component4 = component.GetComponent<BoxCollider2D>();
		vector2 = new Vector2(vector.x, 74f);
		component2.GetComponent<BoxCollider2D>().size = vector2;
		component4.size = vector2;
		component.GetComponent<BoxCollider2D>().offset = new Vector2((float)(buttonSize / 2), component.GetComponent<BoxCollider2D>().offset.y);
		component2.GetComponent<BoxCollider2D>().offset = new Vector2((float)(buttonSize / -2), component2.GetComponent<BoxCollider2D>().offset.y);
	}

	public void Confirm()
	{
		base.Invoke("ConfirmAction", 0.1f);
	}

	private void ConfirmAction()
	{
		this.onConfirm();
		this.CloseWarning();
	}

	public void Cancel()
	{
		base.Invoke("CancelAction", 0.1f);
	}

	public void CancelAction()
	{
		this.CloseWarning();
		if (this.onCancel != null)
		{
			this.onCancel();
		}
	}

	public void CloseWarning()
	{
		base.gameObject.SetActive(false);
	}

	public Text warningText;

	public Text confirmText;

	public Text cancelText;

	[SerializeField]
	private Warning.EmptyDelegate onConfirm;

	[SerializeField]
	private Warning.EmptyDelegate onCancel;

	public RectTransform warningTextShade;

	public delegate void EmptyDelegate();
}
