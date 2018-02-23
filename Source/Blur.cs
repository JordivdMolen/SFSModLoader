using Sirenix.OdinInspector;
using System;
using System.IO;
using UnityEngine;

public class Blur : MonoBehaviour
{
	public Texture2D shade;

	public Texture2D blur;

	public float blurStreght;

	[Button("Generate Shade Texture", ButtonSizes.Small)]
	public void Generate()
	{
		Texture2D texture2D = new Texture2D(this.shade.width, this.shade.height, TextureFormat.Alpha8, false);
		MonoBehaviour.print(texture2D.width + "   " + texture2D.height);
		texture2D.filterMode = FilterMode.Point;
		for (int i = 0; i < this.shade.width; i++)
		{
			for (int j = 0; j < this.shade.height; j++)
			{
				if (this.blur.GetPixel(i, j).a > 0f)
				{
					int num = (int)(this.blur.GetPixel(i, j).a * 12f);
					float num2 = 0f;
					for (int k = 0; k < num * 2 + 1; k++)
					{
						num2 += this.shade.GetPixel(Mathf.Clamp(i - num + k, 0, this.shade.width), j).a;
					}
					num2 = Mathf.Lerp(this.shade.GetPixel(i, j).a, num2 / (float)(num * 2 + 1), this.blurStreght);
					texture2D.SetPixel(i, j, new Color(1f, 1f, 1f, num2));
				}
				else
				{
					texture2D.SetPixel(i, j, this.shade.GetPixel(i, j));
				}
			}
		}
		texture2D.Apply();
		File.WriteAllBytes(Application.dataPath + "/Textures/Combined.png", texture2D.EncodeToPNG());
	}
}
