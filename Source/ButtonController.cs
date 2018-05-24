using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
	private void Start()
	{
		this.buttonImage = base.GetComponent<Image>();
		this.buttonColor = this.buttonImage.color;
		this.orginAlpha = this.To255(this.buttonColor.a);
	}

	private int To255(float val)
	{
		return (int)Math.Round((double)(255f * val));
	}

	private float From255(int val)
	{
		return (float)val / 255f;
	}

	private void Update()
	{
	}

	public new bool enabled;

	private Image buttonImage;

	private Color buttonColor;

	[SerializeField]
	private int orginAlpha;
}
