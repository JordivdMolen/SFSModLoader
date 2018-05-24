using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class TouchKeyboard : MonoBehaviour
{
	private void Awake()
	{
		TouchKeyboard.main = this;
	}

	public void OpenKeyboard(GameObject newTextField)
	{
		this.textField = newTextField.GetComponent<Text>();
		if (this.textField == null)
		{
			this.CloseKeyboard();
			return;
		}
		if (this.caps)
		{
			this.ToggleCaps();
		}
		base.gameObject.SetActive(true);
	}

	public void CloseKeyboard()
	{
		base.gameObject.SetActive(false);
		this.textField = null;
	}

	public string GetText(string noneReplacement)
	{
		if (this.textField == null)
		{
			return noneReplacement;
		}
		string text = this.textField.text.Replace("|", string.Empty);
		if (text == string.Empty)
		{
			return noneReplacement;
		}
		return text;
	}

	public void ToggleCaps()
	{
		this.caps = !this.caps;
		List<BoxCollider2D> allKeys = this.GetAllKeys();
		for (int i = 0; i < allKeys.Count; i++)
		{
			Text componentInChildren = allKeys[i].GetComponentInChildren<Text>();
			if (componentInChildren != null && componentInChildren.text.Length == 1)
			{
				componentInChildren.text = ((!this.caps) ? componentInChildren.text.ToLower() : componentInChildren.text.ToUpper());
			}
		}
	}

	public void OnKey(string key)
	{
		this.textField.text = this.GetClean() + key + this.GetTypingMarker();
	}

	public void OnBackspace()
	{
		string clean = this.GetClean();
		this.textField.text = clean.Substring(0, Mathf.Max(clean.Length - 1, 0)) + this.GetTypingMarker();
	}

	public void OnKeyboardClick(Vector2 clickPosPixel)
	{
		GameObject gameObject = Ref.inputController.PointCastUI(clickPosPixel, this.GetAllKeys().ToArray());
		if (gameObject == null)
		{
			return;
		}
		this.OnKey(gameObject.GetComponentInChildren<Text>().text);
		Ref.inputController.PlayClickSound(0.4f);
		base.StartCoroutine(Ref.inputController.ClickGlow(gameObject.transform.GetChild(0).gameObject));
	}

	private void Update()
	{
		this.textField.text = this.GetClean() + this.GetTypingMarker();
		if (Input.anyKeyDown)
		{
			this.OnKey(Input.inputString);
		}
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			this.OnBackspace();
		}
	}

	private string GetTypingMarker()
	{
		return (Time.unscaledTime % 1f <= 0.5f) ? string.Empty : "|";
	}

	private string GetClean()
	{
		return (!(this.textField != null)) ? string.Empty : this.textField.text.Replace("|", string.Empty);
	}

	private List<BoxCollider2D> GetAllKeys()
	{
		List<BoxCollider2D> list = new List<BoxCollider2D>();
		list.AddRange(this.numbers);
		list.AddRange(this.line1);
		list.AddRange(this.line2);
		list.AddRange(this.line3);
		list.AddRange(this.other);
		return list;
	}

	public static TouchKeyboard main;

	public Text textField;

	public bool caps;

	[BoxGroup]
	public List<BoxCollider2D> numbers;

	[BoxGroup]
	public List<BoxCollider2D> line1;

	[BoxGroup]
	public List<BoxCollider2D> line2;

	[BoxGroup]
	public List<BoxCollider2D> line3;

	[BoxGroup]
	public List<BoxCollider2D> other;
}
