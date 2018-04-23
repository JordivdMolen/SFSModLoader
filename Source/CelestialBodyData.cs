using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;

public class CelestialBodyData : MonoBehaviour
{
	[Button("Validate", 0)]
	public void OnValidate()
	{
		base.transform.name = this.bodyName;
		this.radius = this.diameterKm * 500.0;
		this.mass = Math.Pow(this.radius, 2.0) * this.surfaceGravity;
		this.cameraSwitchHeightM = this.cameraSwitchHeightKm * 1000.0;
		bool flag = this.atmosphereData.shadowIntensity == 0f;
		if (flag)
		{
			this.atmosphereData.shadowIntensity = 1.65f;
		}
		this.atmosphereData.atmosphereHeightM = this.atmosphereData.atmosphereHeightKm * 1000.0;
		bool flag2 = this.terrainData.terrainMaterial != null;
		if (flag2)
		{
			this.terrainData.terrainMaterial.color = this.terrainData.terrainColor;
		}
		this.terrainData.maxTerrainHeight = this.GetMaxTerrainHeight();
		this.terrainData.unitToAngle = 360.0 / ((this.radius + this.terrainData.maxTerrainHeight) * 2.0 * 3.1415926535897931);
		for (int i = 0; i < this.terrainData.detailLevels.Length; i++)
		{
			this.terrainData.detailLevels[i].chunckSize = (double)this.terrainData.baseChunckSize / Math.Pow(2.0, (double)i);
			this.terrainData.detailLevels[i].angularSize = (float)this.terrainData.detailLevels[i].chunckSize / (float)this.terrainData.heightMaps[0].heightMap.HeightDataArray.Length * 360f;
		}
		this.terrainData.detailLevels[0].loadDistance = double.PositiveInfinity;
		bool flag3 = this.type == CelestialBodyData.Type.Star;
		if (flag3)
		{
			this.parentBody = null;
			this.orbitData.SOIMultiplier = double.PositiveInfinity;
		}
		bool flag4 = this.parentBody != null && (this.type == this.parentBody.type || (this.type == CelestialBodyData.Type.Planet && this.parentBody.type == CelestialBodyData.Type.Moon));
		if (flag4)
		{
			this.parentBody = null;
		}
		bool flag5 = this.parentBody != null;
		if (flag5)
		{
			this.orbitData._period = Kepler.GetPeriod(0.0, this.orbitData.orbitHeightM, this.parentBody.mass);
			this.orbitData.periodString = Ref.GetTimeString(this.orbitData._period);
			this.orbitData._meanMotion = -6.2831853071795862 / this.orbitData._period;
			this.orbitData.orbitalVelocity = this.orbitData.orbitHeightM * this.orbitData._meanMotion;
			this.orbitData.SOI = this.orbitData.orbitHeightM * Math.Pow(this.mass / this.parentBody.mass, 0.4) * this.orbitData.SOIMultiplier;
			bool flag6 = !this.ParentHasThisBodyAsSatellite();
			if (flag6)
			{
				List<CelestialBodyData> list = new List<CelestialBodyData>(this.parentBody.satellites);
				list.Add(this);
				this.parentBody.satellites = list.ToArray();
				this.parentBody.ValidateSatellites();
			}
		}
		else
		{
			this.orbitData._period = double.NaN;
			this.orbitData.periodString = "NaN";
			this.orbitData._meanMotion = double.NaN;
			this.orbitData.orbitalVelocity = double.NaN;
			this.orbitData.SOI = ((this.type != CelestialBodyData.Type.Star) ? double.NaN : double.PositiveInfinity);
		}
		this.ValidateSatellites();
	}

	private double GetMaxTerrainHeight()
	{
		double num = 0.0;
		foreach (CelestialBodyData.TerrainData.HeightMapData heightMapData in this.terrainData.heightMaps)
		{
			bool apply = heightMapData.apply;
			if (apply)
			{
				num += heightMapData.height;
			}
		}
		for (int j = 0; j < this.terrainData.perlinLayers.Length; j++)
		{
			bool apply2 = this.terrainData.perlinLayers[j].apply;
			if (apply2)
			{
				bool needLocalValue = this.terrainData.perlinLayers[j].NeedLocalValue;
				if (needLocalValue)
				{
					double num2 = 0.0;
					for (int k = 0; k < this.terrainData.perlinLayers[j].layerComponents.Length; k++)
					{
						CelestialBodyData.TerrainData.PerlinaLayer.Layer layer = this.terrainData.perlinLayers[j].layerComponents[k];
						switch (layer.applyType)
						{
						case CelestialBodyData.TerrainData.PerlinaLayer.Layer.ApplyType.AddToGlobal:
							num += num2 * (double)layer.height;
							break;
						case CelestialBodyData.TerrainData.PerlinaLayer.Layer.ApplyType.AddToLocal:
							num2 += (double)layer.height;
							break;
						case CelestialBodyData.TerrainData.PerlinaLayer.Layer.ApplyType.MultiplyLocal:
							num2 *= (double)layer.height;
							break;
						case CelestialBodyData.TerrainData.PerlinaLayer.Layer.ApplyType.MultiplyByLocalAddToGlobal:
							num += num2 * (double)layer.height;
							break;
						}
					}
				}
				else
				{
					for (int l = 0; l < this.terrainData.perlinLayers[j].layerComponents.Length; l++)
					{
						CelestialBodyData.TerrainData.PerlinaLayer.Layer layer2 = this.terrainData.perlinLayers[j].layerComponents[l];
						num += (double)layer2.height;
					}
				}
			}
		}
		return num;
	}

	public void ValidateSatellites()
	{
		List<CelestialBodyData> list = new List<CelestialBodyData>(this.satellites);
		for (int i = 0; i < list.Count; i++)
		{
			bool flag = list[i] == null || list[i].parentBody != this;
			if (flag)
			{
				list.RemoveAt(i);
				i--;
			}
			else
			{
				list[i].OnValidate();
			}
		}
		this.satellites = list.ToArray();
	}

	private bool ParentHasThisBodyAsSatellite()
	{
		for (int i = 0; i < this.parentBody.satellites.Length; i++)
		{
			bool flag = this == this.parentBody.satellites[i];
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private int[] GetAdress()
	{
		bool flag = this.type == CelestialBodyData.Type.Star;
		int[] result;
		if (flag)
		{
			result = new int[0];
		}
		else
		{
			bool flag2 = !(this.parentBody == null);
			if (flag2)
			{
				List<int> list = new List<int>();
				for (int i = 0; i < this.parentBody.satellites.Length; i++)
				{
					bool flag3 = this.parentBody.satellites[i] == this;
					if (flag3)
					{
						list.Add(i);
						break;
					}
				}
				bool flag4 = this.parentBody.type > CelestialBodyData.Type.Star;
				if (flag4)
				{
					bool flag5 = this.parentBody.parentBody != null;
					if (flag5)
					{
						for (int j = 0; j < this.parentBody.parentBody.satellites.Length; j++)
						{
							bool flag6 = this.parentBody.parentBody.satellites[j] == this.parentBody;
							if (flag6)
							{
								list.Add(j);
								break;
							}
						}
					}
					else
					{
						list.Add(-1);
					}
				}
				list.Reverse();
				result = list.ToArray();
			}
			else
			{
				bool flag7 = this.type == CelestialBodyData.Type.Planet;
				if (flag7)
				{
					result = new int[]
					{
						-1
					};
				}
				else
				{
					result = new int[]
					{
						-1,
						-1
					};
				}
			}
		}
		return result;
	}

	public Double3 GetPosOut(double time)
	{
		double num = time * this.orbitData._meanMotion;
		double orbitHeightM = this.orbitData.orbitHeightM;
		return new Double3(Math.Cos(num) * orbitHeightM, Math.Sin(num) * orbitHeightM);
	}

	public Double3 GetVelOut(double time)
	{
		double num = time * this.orbitData._meanMotion;
		return new Double3(-Math.Sin(num), Math.Cos(num)) * this.orbitData.orbitalVelocity;
	}

	public Double3 GetSolarSystemPosition()
	{
		bool flag = this.type == CelestialBodyData.Type.Moon;
		Double3 result;
		if (flag)
		{
			result = this.GetPosOut(Ref.controller.globalTime) + this.parentBody.GetPosOut(Ref.controller.globalTime);
		}
		else
		{
			bool flag2 = this.type == CelestialBodyData.Type.Planet;
			if (flag2)
			{
				result = this.GetPosOut(Ref.controller.globalTime);
			}
			else
			{
				result = new Double3(0.0, 0.0);
			}
		}
		return result;
	}

	public PlanetManager.TerrainPoints GetTerrainPoints(double from, double size, double LOD, bool offset)
	{
		PlanetManager.TerrainPoints terrainPoints = new PlanetManager.TerrainPoints((int)(size * LOD) + 2);
		PlanetManager.TerrainPoints result;
		if (offset)
		{
			double num = 6.28319 / (double)this.terrainData.heightMaps[0].heightMap.HeightDataArray.Length / LOD;
			double num2 = 6.28319 / (double)this.terrainData.heightMaps[0].heightMap.HeightDataArray.Length * from;
			Double3 @double = new Double3((double)((int)(Math.Cos(num2) * this.radius / 1000.0) * 1000), (double)((int)(Math.Sin(num2) * this.radius / 1000.0) * 1000));
			int num3 = (int)(size * LOD) + 1;
			double[] terrainSamples = this.GetTerrainSamples(from, LOD, num3 + 1, true);
			for (int i = 0; i < num3; i++)
			{
				double num4 = num2 + (double)i * num;
				terrainPoints.points[i + 1] = new Vector3((float)(Math.Cos(num4) * terrainSamples[i] - @double.x), (float)(Math.Sin(num4) * terrainSamples[i] - @double.y), 0f);
				float x = 0.5f + (float)((terrainSamples[i] - terrainSamples[i + 1]) * LOD * 0.001);
				terrainPoints.uvOthers[i + 1] = new Vector2(x, this.terrainData.texLayer.perlinCurve.Evaluate(Mathf.PerlinNoise((float)(from + (double)i / LOD) * this.terrainData.texLayer.repeatX, 0f)));
			}
			terrainPoints.points[0] = -@double.toVector3;
			terrainPoints.uvOthers[0] = Vector2.zero;
			result = terrainPoints;
		}
		else
		{
			double num5 = 6.28319 / (double)this.terrainData.heightMaps[0].heightMap.HeightDataArray.Length / LOD;
			double num6 = 6.28319 / (double)this.terrainData.heightMaps[0].heightMap.HeightDataArray.Length * from;
			int num7 = (int)(size * LOD) + 1;
			double[] terrainSamples2 = this.GetTerrainSamples(from, LOD, num7 + 1, true);
			for (int j = 0; j < num7; j++)
			{
				double num8 = (double)j * num5 + num6;
				terrainPoints.points[j + 1] = new Vector3((float)(Math.Cos(num8) * terrainSamples2[j]), (float)(Math.Sin(num8) * terrainSamples2[j]), 0f);
				float x2 = 0.5f + (float)((terrainSamples2[j] - terrainSamples2[j + 1]) * LOD * 0.001);
				terrainPoints.uvOthers[j + 1] = new Vector2(x2, this.terrainData.texLayer.perlinCurve.Evaluate(Mathf.PerlinNoise((float)(from + (double)j / LOD) * this.terrainData.texLayer.repeatX, 0f)));
			}
			terrainPoints.uvOthers[0] = Vector2.zero;
			result = terrainPoints;
		}
		return result;
	}

	private double[] GetTerrainSamples(double from, double LOD, int pointCount, bool addRadius)
	{
		double[] array = new double[pointCount];
		double[] array2 = new double[pointCount];
		for (int i = 0; i < pointCount; i++)
		{
			array2[i] = from + (double)i / LOD;
			if (addRadius)
			{
				array[i] = this.radius;
			}
		}
		foreach (CelestialBodyData.TerrainData.HeightMapData heightMapData in this.terrainData.heightMaps)
		{
			bool apply = heightMapData.apply;
			if (apply)
			{
				for (int k = 0; k < pointCount; k++)
				{
					float num = (float)(array2[k] * heightMapData.repeat % 1.0);
					int num2 = (int)(array2[k] * heightMapData.repeat);
					array[k] += heightMapData.height * (double)(heightMapData.heightMap.HeightDataArray[num2 % heightMapData.heightMap.HeightDataArray.Length] * (1f - num) + heightMapData.heightMap.HeightDataArray[(num2 + 1) % heightMapData.heightMap.HeightDataArray.Length] * num);
				}
			}
		}
		for (int l = 0; l < this.terrainData.perlinLayers.Length; l++)
		{
			bool apply2 = this.terrainData.perlinLayers[l].apply;
			if (apply2)
			{
				bool needLocalValue = this.terrainData.perlinLayers[l].NeedLocalValue;
				if (needLocalValue)
				{
					double[] array3 = new double[pointCount];
					for (int m = 0; m < this.terrainData.perlinLayers[l].layerComponents.Length; m++)
					{
						CelestialBodyData.TerrainData.PerlinaLayer.Layer layer = this.terrainData.perlinLayers[l].layerComponents[m];
						switch (layer.applyType)
						{
						case CelestialBodyData.TerrainData.PerlinaLayer.Layer.ApplyType.AddToGlobal:
							for (int n = 0; n < pointCount; n++)
							{
								array[n] += array3[n] + (double)(layer.perlinCurve.Evaluate(Mathf.PerlinNoise((float)array2[n] * layer.repeatX, 0f)) * layer.height);
							}
							break;
						case CelestialBodyData.TerrainData.PerlinaLayer.Layer.ApplyType.AddToLocal:
							for (int num3 = 0; num3 < pointCount; num3++)
							{
								array3[num3] += (double)(layer.perlinCurve.Evaluate(Mathf.PerlinNoise((float)array2[num3] * layer.repeatX, 0f)) * layer.height);
							}
							break;
						case CelestialBodyData.TerrainData.PerlinaLayer.Layer.ApplyType.MultiplyLocal:
							for (int num4 = 0; num4 < pointCount; num4++)
							{
								array3[num4] *= (double)(layer.perlinCurve.Evaluate(Mathf.PerlinNoise((float)array2[num4] * layer.repeatX, 0f)) * layer.height);
							}
							break;
						case CelestialBodyData.TerrainData.PerlinaLayer.Layer.ApplyType.MultiplyByLocalAddToGlobal:
							for (int num5 = 0; num5 < pointCount; num5++)
							{
								array[num5] += array3[num5] * (double)layer.perlinCurve.Evaluate(Mathf.PerlinNoise((float)array2[num5] * layer.repeatX, 0f)) * (double)layer.height;
							}
							break;
						}
					}
				}
				else
				{
					for (int num6 = 0; num6 < this.terrainData.perlinLayers[l].layerComponents.Length; num6++)
					{
						CelestialBodyData.TerrainData.PerlinaLayer.Layer layer2 = this.terrainData.perlinLayers[l].layerComponents[num6];
						for (int num7 = 0; num7 < pointCount; num7++)
						{
							array[num7] += (double)(layer2.perlinCurve.Evaluate(Mathf.PerlinNoise((float)array2[num7] * layer2.repeatX, 0f)) * layer2.height);
						}
					}
				}
			}
		}
		return array;
	}

	public double GetTerrainSampleAtAngle(double angle)
	{
		bool flag = angle < 0.0;
		if (flag)
		{
			angle += 360.0;
		}
		else
		{
			bool flag2 = angle > 360.0;
			if (flag2)
			{
				angle -= 360.0;
			}
		}
		return this.GetTerrainSamples(angle / 360.0 * (double)this.terrainData.heightMaps[0].heightMap.HeightDataArray.Length, 1.0, 1, false)[0];
	}

	public Texture2D GenerateAtmosphereTexture()
	{
		Texture2D texture2D = new Texture2D(30, 1);
		for (int i = 0; i < texture2D.width; i++)
		{
			texture2D.SetPixel(i, 0, this.atmosphereData.atmosphereGradient.Evaluate(1f - Mathf.Pow((float)i / (float)texture2D.width, 2f)));
		}
		texture2D.Apply();
		texture2D.wrapMode = TextureWrapMode.Clamp;
		return texture2D;
	}

	public Texture2D GenerateAtmosphereTexture(Gradient gradient)
	{
		Texture2D texture2D = new Texture2D(30, 1);
		for (int i = 0; i < texture2D.width; i++)
		{
			texture2D.SetPixel(i, 0, gradient.Evaluate(1f - Mathf.Pow((float)i / (float)texture2D.width, 2f)));
		}
		texture2D.Apply();
		texture2D.wrapMode = TextureWrapMode.Clamp;
		return texture2D;
	}

	public CelestialBodyData()
	{
	}

	[FoldoutGroup("Basic", 0)]
	public string bodyName;

	[FoldoutGroup("Basic", 0)]
	public CelestialBodyData.Type type;

	[FoldoutGroup("Basic", 0)]
	[Space]
	[SerializeField]
	private double diameterKm;

	[FoldoutGroup("Basic", 0)]
	public double surfaceGravity;

	[FoldoutGroup("Basic", 0)]
	[ReadOnly]
	public double mass;

	[FoldoutGroup("Basic", 0)]
	public double cameraSwitchHeightKm;

	[FoldoutGroup("Basic", 0)]
	public double minTimewarpHeightKm;

	[HideInInspector]
	public double cameraSwitchHeightM;

	[HideInInspector]
	[ReadOnly]
	public double radius;

	[BoxGroup("1", false, false, 0)]
	public CelestialBodyData.AtmosphereData atmosphereData;

	[BoxGroup("2", false, false, 0)]
	public CelestialBodyData.TerrainData terrainData;

	[FoldoutGroup("Solar System Positon", 0)]
	[BoxGroup("Solar System Positon/", true, false, 0)]
	public CelestialBodyData parentBody;

	[Space]
	[FoldoutGroup("Solar System Positon", 0)]
	public CelestialBodyData[] satellites;

	[BoxGroup("3", false, false, 0)]
	public CelestialBodyData.OrbitData orbitData;

	[BoxGroup("5", false, false, 0)]
	public CelestialBodyData.MapData mapData;

	[Serializable]
	public struct AtmosphereData
	{
		public bool hasAtmosphere;

		[ShowIf("hasAtmosphere", true)]
		[SuffixLabel(" km ", false)]
		public double atmosphereHeightKm;

		[ShowIf("hasAtmosphere", true)]
		[SuffixLabel(" m ", false)]
		[ReadOnly]
		public double atmosphereHeightM;

		[ShowIf("hasAtmosphere", true)]
		public double atmoDensity;

		[ShowIf("hasAtmosphere", true)]
		public double atmoCurvePow;

		[ShowIf("hasAtmosphere", true)]
		public float shadowIntensity;

		[ShowIf("hasAtmosphere", true)]
		public float maxAtmosphereFade;

		[ShowIf("hasAtmosphere", true)]
		public Gradient atmosphereGradient;

		[ShowIf("hasAtmosphere", true)]
		public double gradientHightMultiplier;

		[ShowIf("hasAtmosphere", true)]
		public Color closeColor;

		[ShowIf("hasAtmosphere", true)]
		public Color farColor;
	}

	[Serializable]
	public struct TerrainData
	{
		public Color terrainColor;

		public Material terrainMaterial;

		[Space(5f)]
		public Color mapColor;

		public Material mapMaterial;

		[Space(5f)]
		public int baseChunckSize;

		public int mapDetailLevelId;

		[ReadOnly]
		public double maxTerrainHeight;

		[ReadOnly]
		public double unitToAngle;

		[Space]
		[TableList]
		public CelestialBodyData.TerrainData.DetailLevel[] detailLevels;

		[Space]
		[TableList]
		public CelestialBodyData.TerrainData.HeightMapData[] heightMaps;

		[Space]
		[Indent(-1)]
		public CelestialBodyData.TerrainData.PerlinaLayer[] perlinLayers;

		[Indent(-1)]
		public CelestialBodyData.TerrainData.PerlinaLayer.Layer texLayer;

		[Space]
		[Indent(-1)]
		public CelestialBodyData.TexPipeline[] texPipelines;

		[Serializable]
		public class DetailLevel
		{
			public DetailLevel()
			{
			}

			public double loadDistance;

			public double LOD;

			[ReadOnly]
			public double chunckSize;

			[ReadOnly]
			public float angularSize;
		}

		[Serializable]
		public class HeightMapData
		{
			public HeightMapData()
			{
			}

			public string name;

			[TableColumnWidth(60)]
			public bool apply;

			[TableColumnWidth(80)]
			public double height;

			[TableColumnWidth(80)]
			public double repeat;

			public HeightMap heightMap;
		}

		[Serializable]
		public class PerlinaLayer
		{
			[ShowInInspector]
			public bool NeedLocalValue
			{
				get
				{
					for (int i = 0; i < this.layerComponents.Length; i++)
					{
						bool flag = this.layerComponents[i].applyType > CelestialBodyData.TerrainData.PerlinaLayer.Layer.ApplyType.AddToGlobal;
						if (flag)
						{
							return true;
						}
					}
					return false;
				}
			}

			public PerlinaLayer()
			{
			}

			[BoxGroup]
			[LabelWidth(80f)]
			[GUIColor(0.8f, 0.95f, 0.8f, 1f)]
			public string layerName;

			[BoxGroup]
			[LabelWidth(80f)]
			public bool apply;

			[BoxGroup]
			[TableList]
			[EnableIf("apply")]
			public CelestialBodyData.TerrainData.PerlinaLayer.Layer[] layerComponents;

			[Serializable]
			public class Layer
			{
				public Layer()
				{
					this.height = (this.repeatX = 1f);
				}

				[TableColumnWidth(75)]
				public float height;

				[TableColumnWidth(75)]
				public float repeatX;

				public AnimationCurve perlinCurve;

				public CelestialBodyData.TerrainData.PerlinaLayer.Layer.ApplyType applyType;

				public enum ApplyType
				{
					AddToGlobal,
					AddToLocal,
					MultiplyLocal,
					MultiplyByLocalAddToGlobal
				}
			}
		}
	}

	[Serializable]
	public class TexPipeline
	{
		[Button("Generate Texture", 22)]
		public void GenerateTexture()
		{
			Texture2D texture2D = new Texture2D(this.texIN.width, this.texIN.height, TextureFormat.RGB24, false);
			Color[] pixels = this.texIN.GetPixels(0, 0, texture2D.width, texture2D.height);
			for (int i = 0; i < pixels.Length; i++)
			{
				pixels[i] *= this.color;
			}
			texture2D.SetPixels(pixels);
			texture2D.Apply();
			File.WriteAllBytes(Application.dataPath + "/Textures/Terrain/" + this.name + ".png", texture2D.EncodeToPNG());
		}

		public TexPipeline()
		{
		}

		public Texture2D texIN;

		public Color color = Color.white;

		public string name;
	}

	[Serializable]
	public struct OrbitData
	{
		public double orbitHeightM
		{
			get
			{
				return this.orbitHeightMm * 1000000.0;
			}
		}

		[Space]
		[SerializeField]
		[SuffixLabel(".000 km ", false)]
		public double orbitHeightMm;

		public double SOIMultiplier;

		[Space]
		[ReadOnly]
		[SuffixLabel("m/s ", false)]
		public double orbitalVelocity;

		[ReadOnly]
		[SuffixLabel("m ", false)]
		public double SOI;

		[ReadOnly]
		[SuffixLabel("rad/s ", false)]
		public double _meanMotion;

		[ReadOnly]
		[SuffixLabel("seconds ", false)]
		public double _period;

		[ReadOnly]
		public string periodString;
	}

	[Serializable]
	public struct MapData
	{
		public double hideNameMultiplier;

		public double showClosestApproachDistance;
	}

	public enum Type
	{
		Star,
		Planet,
		Moon
	}
}
