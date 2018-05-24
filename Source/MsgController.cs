using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MsgController : MonoBehaviour
{
	private void Start()
	{
		MsgController.main = this;
	}

	public static void ShowMsg(string msgText)
	{
		MsgController.ShowMsg(msgText, 3.5f, true);
	}

	public static void ShowMsg(string msgText, bool showMsg = true)
	{
		MsgController.ShowMsg(msgText, 3.5f, showMsg);
	}

	public static void ShowMsg(string msgText, float displayTime = 3.5f, bool showMsg = true)
	{
		if (!showMsg)
		{
			return;
		}
		if (MsgController.main == null)
		{
			return;
		}
		if (MsgController.main.msgCoroutine != null)
		{
			MsgController.main.StopCoroutine(MsgController.main.msgCoroutine);
		}
		MsgController.main.msgText.gameObject.SetActive(true);
		MsgController.main.msgCoroutine = MsgController.main.MsgCoroutine(msgText, displayTime);
		MsgController.main.StartCoroutine(MsgController.main.msgCoroutine);
	}

	private IEnumerator MsgCoroutine(string msg, float displayTime)
	{
		this.msgText.text = msg;
		while (displayTime > 0f)
		{
			displayTime -= Time.deltaTime;
			Color newColor = new Color(1f, 1f, 1f, displayTime);
			if (newColor != this.msgText.color)
			{
				this.msgText.color = newColor;
			}
			yield return new WaitForEndOfFrame();
		}
		this.msgText.gameObject.SetActive(false);
		yield break;
	}

	public static MsgController main;

	public Text msgText;

	private IEnumerator msgCoroutine;
}
