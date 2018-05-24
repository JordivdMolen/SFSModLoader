using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class HeightMap : MonoBehaviour
{
	[Button("Generate Height Map", ButtonSizes.Medium)]
	private void GenerateHightData()
	{
		this.HeightDataArray = new float[this.heightMapTexture.texture.width];
		for (int i = 0; i < this.HeightDataArray.Length; i++)
		{
			this.HeightDataArray[this.HeightDataArray.Length - i - 1] = this.GetHeightAtX(i);
		}
	}

	private float GetHeightAtX(int x)
	{
		for (int i = 0; i < this.heightMapTexture.texture.height; i++)
		{
			float a = this.heightMapTexture.texture.GetPixel(x, i).a;
			if (a < 1f)
			{
				return ((float)i + a) / (float)this.heightMapTexture.texture.height;
			}
		}
		return 1f;
	}

	public Sprite heightMapTexture;

	[Space]
	[TableMatrix]
	public float[] HeightDataArray;
}
