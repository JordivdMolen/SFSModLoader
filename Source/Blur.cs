using System;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

public class Blur : MonoBehaviour
{
	[Button("Generate Shade Texture", 0)]
	public void Generate()
	{
		Texture2D texture2D = new Texture2D(this.shade.width, this.shade.height, TextureFormat.Alpha8, false);
		texture2D.filterMode = FilterMode.Point;
		for (int i = 0; i < this.shade.width; i++)
		{
			for (int j = 0; j < this.shade.height; j++)
			{
				bool flag = this.blur.GetPixel(i, j).a > 0f;
				if (flag)
				{
					int num = (int)(this.blur.GetPixel(i, j).a * 12f);
					float num2 = 0f;
					for (int k = 0; k < num * 2 + 1; k++)
					{
						num2 += 1f - this.shade.GetPixel(Mathf.Clamp(i - num + k, 0, this.shade.width), j).r;
					}
					num2 = Mathf.Lerp(1f - this.shade.GetPixel(i, j).r, num2 / (float)(num * 2 + 1), this.blurStreght);
					texture2D.SetPixel(i, j, new Color(1f, 1f, 1f, num2));
				}
				else
				{
					texture2D.SetPixel(i, j, new Color(0f, 0f, 0f, 1f - this.shade.GetPixel(i, j).r));
				}
			}
		}
		texture2D.Apply();
		File.WriteAllBytes(Application.dataPath + "/Textures/Combined.png", texture2D.EncodeToPNG());
	}

	[Button("Generate Shade Texture", 0)]
	public void GenerateFlame()
	{
		Texture2D texture2D = new Texture2D(this.flameIn.width, this.flameIn.height, TextureFormat.RGBA32, false);
		texture2D.filterMode = FilterMode.Point;
		for (int i = 0; i < texture2D.width; i++)
		{
			for (int j = 0; j < texture2D.height; j++)
			{
				bool flag = this.flameIn.GetPixel(i, j).a == 0f;
				if (flag)
				{
					texture2D.SetPixel(i, j, Color.clear);
				}
				else
				{
					Color pixel = this.flameIn.GetPixel(i, j);
					float num = Mathf.Max(new float[]
					{
						pixel.r,
						pixel.g,
						pixel.b
					});
					texture2D.SetPixel(i, j, new Color(pixel.r / num, pixel.g / num, pixel.b / num, num));
				}
			}
		}
		texture2D.Apply();
		File.WriteAllBytes(Application.dataPath + "/Textures/" + this.flameOut.name + ".png", texture2D.EncodeToPNG());
	}

	public Blur()
	{
	}

	public Texture2D shade;

	public Texture2D blur;

	public float blurStreght;

	public Texture2D flameIn;

	public Texture2D flameOut;
}
