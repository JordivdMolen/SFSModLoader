using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlanetManager : MonoBehaviour
{
	public void SwitchLocation(CelestialBodyData newPlanet, Double3 newFocusPosition, bool showSwitchMsg, bool fullyLoadTerrain, double unloadDistance)
	{
		CelestialBodyData loadedPlanet = Ref.controller.loadedPlanet;
		foreach (Vessel vessel in Ref.controller.vessels)
		{
			bool flag = loadedPlanet != newPlanet || (newFocusPosition - vessel.GetGlobalPosition).sqrMagnitude2d > unloadDistance * unloadDistance;
			if (flag)
			{
				vessel.SetVesselState(Vessel.ToState.ToUnloaded);
			}
		}
		this.ClearPlanet();
		Ref.controller.loadedPlanet = newPlanet;
		this.UpdateAtmosphereFade();
		Ref.controller.OffsetScenePosition((newFocusPosition.roundTo1000 - Ref.positionOffset).toVector2);
		this.LoadBaseChuncks(newPlanet);
		if (fullyLoadTerrain)
		{
			this.FullyLoadTerrain(newPlanet);
		}
		this.LoadPlanetAtmosphere(newPlanet);
		this.launchPad.gameObject.SetActive(newPlanet.bodyName == "Earth");
		Ref.partShader.SetFloat("_Intensity", 1.7f);
		if (showSwitchMsg)
		{
			this.ShowSwitchedPlanetMsg(newPlanet, loadedPlanet);
		}
		this.SetOtherTerrains();
	}

	private void SetOtherTerrains()
	{
		for (int i = 0; i < this.lowRezPlanets.planets.Count; i++)
		{
			bool flag = this.lowRezPlanets.planets[i].planet != Ref.controller.loadedPlanet && this.lowRezPlanets.planets[i].planet.atmosphereData.hasAtmosphere;
			if (flag)
			{
				this.lowRezPlanets.planets[i].meshHolder.GetComponent<MeshRenderer>().sharedMaterial.SetFloat(Shader.PropertyToID("_FadeStrength"), this.lowRezPlanets.planets[i].planet.atmosphereData.maxAtmosphereFade);
			}
		}
	}

	private void LoadBaseChuncks(CelestialBodyData planet)
	{
		bool flag = planet.type > CelestialBodyData.Type.Star;
		if (flag)
		{
			for (int i = 0; i < planet.terrainData.heightMaps[0].heightMap.HeightDataArray.Length / planet.terrainData.baseChunckSize; i++)
			{
				PlanetManager.Chunck chunck = new PlanetManager.Chunck((double)(i * planet.terrainData.baseChunckSize), (double)planet.terrainData.baseChunckSize, 0, planet, planet.terrainData.detailLevels, this.chuncksHolder, false, false);
				chunck.updateMerge = double.PositiveInfinity;
				this.activeChuncks.Add(chunck);
			}
		}
	}

	private void ClearPlanet()
	{
		while (this.chuncksHolder.childCount > 0)
		{
			UnityEngine.Object.DestroyImmediate(this.chuncksHolder.GetChild(0).gameObject);
		}
		foreach (PlanetManager.Chunck chunck in this.colliderChuncks)
		{
			bool flag = chunck.chunckTransform != null;
			if (flag)
			{
				UnityEngine.Object.DestroyImmediate(chunck.chunckTransform.gameObject);
			}
		}
		this.activeChuncks.Clear();
		this.colliderChuncks.Clear();
	}

	private void LoadPlanetAtmosphere(CelestialBodyData newPlanet)
	{
		this.atmosphereRenderer.gameObject.SetActive(newPlanet.atmosphereData.hasAtmosphere);
		this.atmosphereRenderer.transform.localScale = Vector3.one * ((!newPlanet.atmosphereData.hasAtmosphere) ? 1f : ((float)(newPlanet.atmosphereData.atmosphereHeightM * newPlanet.atmosphereData.gradientHightMultiplier + newPlanet.radius)));
		bool hasAtmosphere = newPlanet.atmosphereData.hasAtmosphere;
		if (hasAtmosphere)
		{
			Mesh mesh = this.atmosphereRenderer.GetComponent<MeshFilter>().mesh;
			Vector2[] uv = mesh.uv;
			uv[0] = new Vector2(1f + (float)(newPlanet.radius / (newPlanet.atmosphereData.atmosphereHeightM * newPlanet.atmosphereData.gradientHightMultiplier)), 0f);
			mesh.uv = uv;
			this.atmosphereRenderer.sortingLayerName = ((newPlanet.type != CelestialBodyData.Type.Star) ? "Back" : "Default");
			this.atmosphereRenderer.sortingOrder = ((newPlanet.type != CelestialBodyData.Type.Star) ? 0 : 500);
			this.atmosphereRenderer.sharedMaterial.SetTexture("_MainTex", newPlanet.GenerateAtmosphereTexture());
		}
	}

	public void FullyLoadTerrain(CelestialBodyData newPlanet)
	{
		this.UpdateChuncksLoading(newPlanet.terrainData.detailLevels);
	}

	private void ShowSwitchedPlanetMsg(CelestialBodyData newPlanet, CelestialBodyData oldPlanet)
	{
		bool flag = oldPlanet.parentBody == newPlanet && oldPlanet != null;
		if (flag)
		{
			Ref.controller.ShowMsg("Left " + oldPlanet.bodyName + " sphere of influence");
		}
		bool flag2 = newPlanet.parentBody == oldPlanet;
		if (flag2)
		{
			Ref.controller.ShowMsg("Entered " + newPlanet.bodyName + " sphere of influence");
		}
	}

	private void Start()
	{
		Color color = this.atmosphereRenderer.sharedMaterial.color;
		this.atmosphereRenderer.sharedMaterial.color = Color.white;
		this.lowRezPlanets = new PlanetManager.LowRezPlanets();
		this.lowRezPlanets.planets.Add(new PlanetManager.LowRezPlanets.LowRezPlanet(Ref.solarSystemRoot));
		foreach (CelestialBodyData celestialBodyData in Ref.solarSystemRoot.satellites)
		{
			this.lowRezPlanets.planets.Add(new PlanetManager.LowRezPlanets.LowRezPlanet(celestialBodyData));
			foreach (CelestialBodyData planet in celestialBodyData.satellites)
			{
				this.lowRezPlanets.planets.Add(new PlanetManager.LowRezPlanets.LowRezPlanet(planet));
			}
		}
		this.atmosphereRenderer.sharedMaterial.color = color;
	}

	private void Update()
	{
		this.UpdateChuncksLoading(Ref.controller.loadedPlanet.terrainData.detailLevels);
		bool flag = Ref.positionOffset.magnitude2d < 500000.0 && Ref.positionOffset.toVector3 != -base.transform.position;
		if (flag)
		{
			this.UpdatePositionOffset(-Ref.positionOffset);
			this.UpdatePositionOffset(-Ref.positionOffset);
		}
	}

	private void UpdateChuncksLoading(CelestialBodyData.TerrainData.DetailLevel[] detailLevels)
	{
		this.totalDistanceMoved += this.GetMaxVesselsVelocity() * (double)Time.deltaTime + 0.1;
		for (int i = 0; i < this.activeChuncks.Count; i++)
		{
			PlanetManager.Chunck chunck = this.activeChuncks[i];
			bool flag = this.totalDistanceMoved > chunck.updateSplit;
			if (flag)
			{
				bool flag2 = this.TrySplitChunck(chunck, detailLevels);
				bool flag3 = flag2;
				if (flag3)
				{
					i--;
				}
			}
			bool flag4 = this.totalDistanceMoved > chunck.updateMerge;
			if (flag4)
			{
				this.TryMergeChunck(chunck, detailLevels);
			}
		}
	}

	private bool TrySplitChunck(PlanetManager.Chunck chunck, CelestialBodyData.TerrainData.DetailLevel[] detailLevels)
	{
		double loadDistance = detailLevels[chunck.LODId + 1].loadDistance;
		double minDistanceToVessels = this.GetMinDistanceToVessels(chunck);
		bool flag = minDistanceToVessels < loadDistance + 10.0;
		bool result;
		if (flag)
		{
			this.SplitChunck(chunck, detailLevels);
			result = true;
		}
		else
		{
			bool flag2 = minDistanceToVessels != double.PositiveInfinity;
			if (flag2)
			{
				chunck.updateSplit = this.totalDistanceMoved + Math.Min(minDistanceToVessels - loadDistance, 100000.0);
			}
			result = false;
		}
		return result;
	}

	private void TryMergeChunck(PlanetManager.Chunck chunck, CelestialBodyData.TerrainData.DetailLevel[] detailLevels)
	{
		double loadDistance = detailLevels[chunck.LODId].loadDistance;
		double minDistanceToVessels = this.GetMinDistanceToVessels(chunck.parentChunck);
		bool flag = minDistanceToVessels > loadDistance + 10.0;
		if (flag)
		{
			this.MergeChunck(chunck);
		}
		bool flag2 = minDistanceToVessels != double.PositiveInfinity;
		if (flag2)
		{
			chunck.updateMerge = this.totalDistanceMoved + Math.Min(loadDistance - minDistanceToVessels, 100000.0) + 30.0;
		}
	}

	private void SplitChunck(PlanetManager.Chunck chunckToSplit, CelestialBodyData.TerrainData.DetailLevel[] detailLevels)
	{
		chunckToSplit.chunckTransform.gameObject.SetActive(false);
		this.activeChuncks.Remove(chunckToSplit);
		double chunckSize = detailLevels[chunckToSplit.LODId + 1].chunckSize;
		bool flag = chunckToSplit.LODId + 1 == detailLevels.Length - 1;
		PlanetManager.Chunck chunck = new PlanetManager.Chunck(chunckToSplit.from, chunckSize, chunckToSplit.LODId + 1, Ref.controller.loadedPlanet, detailLevels, this.chuncksHolder, false, flag);
		PlanetManager.Chunck chunck2 = new PlanetManager.Chunck(chunckToSplit.from + chunckSize, chunckSize, chunckToSplit.LODId + 1, Ref.controller.loadedPlanet, detailLevels, this.chuncksHolder, false, flag);
		this.activeChuncks.Add(chunck);
		this.activeChuncks.Add(chunck2);
		chunck.chunckTransform.localPosition = ((!flag) ? Vector3.zero : (-chunck.chunckPosOffset));
		chunck2.chunckTransform.localPosition = ((!flag) ? Vector3.zero : (-chunck2.chunckPosOffset));
		chunck.secondHalf = chunck2;
		chunck.parentChunck = chunckToSplit;
		chunck2.updateMerge = double.PositiveInfinity;
		chunck.chunckTransform.name = "Chunck - Lod: " + (chunckToSplit.LODId + 1).ToString();
		chunck2.chunckTransform.name = "Chunck - Lod: " + (chunckToSplit.LODId + 1).ToString();
		bool flag2 = chunckToSplit.LODId + 1 == detailLevels.Length - 1;
		if (flag2)
		{
			chunck.GenerateCollider();
			chunck2.GenerateCollider();
			this.colliderChuncks.Add(chunck);
			this.colliderChuncks.Add(chunck2);
			chunck.updateSplit = double.PositiveInfinity;
			chunck2.updateSplit = double.PositiveInfinity;
		}
		else
		{
			this.TrySplitChunck(chunck, detailLevels);
			this.TrySplitChunck(chunck2, detailLevels);
		}
	}

	private void MergeChunck(PlanetManager.Chunck firstHalf)
	{
		PlanetManager.Chunck secondHalf = firstHalf.secondHalf;
		bool flag = !secondHalf.chunckTransform.gameObject.activeSelf;
		if (!flag)
		{
			firstHalf.parentChunck.chunckTransform.gameObject.SetActive(true);
			this.activeChuncks.Add(firstHalf.parentChunck);
			this.activeChuncks.Remove(firstHalf);
			this.activeChuncks.Remove(secondHalf);
			this.colliderChuncks.Remove(firstHalf);
			this.colliderChuncks.Remove(secondHalf);
			UnityEngine.Object.Destroy(firstHalf.chunckTransform.gameObject);
			UnityEngine.Object.Destroy(firstHalf.secondHalf.chunckTransform.gameObject);
		}
	}

	private double GetMaxVesselsVelocity()
	{
		double num = 0.0;
		for (int i = 0; i < Ref.controller.vessels.Count; i++)
		{
			bool flag = Ref.controller.vessels[i].state == Vessel.State.RealTime;
			if (flag)
			{
				double magnitude2d = Ref.controller.vessels[i].GetGlobalVelocity.magnitude2d;
				bool flag2 = magnitude2d > num;
				if (flag2)
				{
					num = magnitude2d;
				}
			}
		}
		return num;
	}

	private double GetMinDistanceToVessels(PlanetManager.Chunck chunck)
	{
		double num = double.PositiveInfinity;
		for (int i = 0; i < Ref.controller.vessels.Count; i++)
		{
			bool flag = Ref.controller.vessels[i].state == Vessel.State.RealTime || Ref.controller.vessels[i].state == Vessel.State.Stationary;
			if (flag)
			{
				Double3 @double = (Ref.controller.vessels[i].state != Vessel.State.RealTime) ? Ref.controller.vessels[i].GetGlobalPosition : (Ref.positionOffset + Ref.controller.vessels[i].partsManager.rb2d.transform.position);
				double closestPointOnLine = Double3.GetClosestPointOnLine(chunck.topPosition, @double);
				Double3 a = chunck.topPosition * closestPointOnLine;
				double num2 = (a - @double).magnitude2d - chunck.topSizeHalf * closestPointOnLine;
				bool flag2 = num2 < num;
				if (flag2)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	public void UpdatePositionOffset(Double3 newPlanetOffset)
	{
		bool flag = newPlanetOffset.x == Ref.positionOffset.x && newPlanetOffset.y == Ref.positionOffset.y;
		if (!flag)
		{
			Ref.positionOffset = newPlanetOffset;
			Double3 a = new Double3(-450.0, Ref.controller.loadedPlanet.radius + 28.0);
			this.launchPad.transform.position = (a - Ref.positionOffset).toVector3;
			base.transform.position = -Ref.positionOffset.toVector3;
			for (int i = 0; i < this.colliderChuncks.Count; i++)
			{
				this.colliderChuncks[i].chunckTransform.position = -Ref.positionOffset.toVector3 - this.colliderChuncks[i].chunckPosOffset;
			}
		}
	}

	public void UpdateAtmosphereFade()
	{
		float num = Mathf.Pow(Mathf.Clamp01((Mathf.Max(Ref.controller.cameraDistanceGame, 500f) - 500f) / 79500f), 0.3f);
		float num2 = num * Ref.controller.loadedPlanet.atmosphereData.maxAtmosphereFade;
		Ref.controller.loadedPlanet.terrainData.terrainMaterial.SetFloat(Shader.PropertyToID("_FadeStrength"), num2);
		this.atmosphereRenderer.sharedMaterial.SetColor("_Color", Color.Lerp(Ref.controller.loadedPlanet.atmosphereData.closeColor, Ref.controller.loadedPlanet.atmosphereData.farColor, num2));
	}

	public void PositionLowRezPlanets()
	{
		Double3 solarSystemPosition = Ref.controller.loadedPlanet.GetSolarSystemPosition();
		for (int i = 0; i < this.lowRezPlanets.planets.Count; i++)
		{
			this.lowRezPlanets.planets[i].meshHolder.localPosition = ((this.lowRezPlanets.planets[i].planet.GetSolarSystemPosition() - solarSystemPosition) * 0.0001).toVector3;
		}
	}

	public PlanetManager()
	{
	}

	[TableList]
	public List<PlanetManager.Chunck> activeChuncks;

	[TableList]
	public List<PlanetManager.Chunck> colliderChuncks;

	public MeshRenderer atmosphereRenderer;

	public Transform chuncksHolder;

	public Transform launchPad;

	public Transform scaledHolder;

	public Transform chunckPrefab;

	public double totalDistanceMoved;

	public PlanetManager.LowRezPlanets lowRezPlanets;

	public Gradient flareGradient;

	[Serializable]
	public class LowRezPlanets
	{
		public LowRezPlanets()
		{
		}

		public List<PlanetManager.LowRezPlanets.LowRezPlanet> planets = new List<PlanetManager.LowRezPlanets.LowRezPlanet>();

		[Serializable]
		public class LowRezPlanet
		{
			public LowRezPlanet(CelestialBodyData planet)
			{
				PlanetManager.Chunck chunck = new PlanetManager.Chunck(0.0, (double)planet.terrainData.heightMaps[0].heightMap.HeightDataArray.Length, 0, planet, planet.terrainData.detailLevels, null, false, false);
				this.planet = planet;
				this.meshHolder = chunck.chunckTransform;
				this.meshHolder.localScale = Vector3.one / 10000f;
				this.meshHolder.gameObject.layer = LayerMask.NameToLayer("Scaled Space");
				this.meshHolder.name = planet.bodyName + " Scaled";
				this.meshHolder.parent = Ref.planetManager.scaledHolder;
				bool flag = !planet.atmosphereData.hasAtmosphere;
				if (!flag)
				{
					PlanetManager.LowRezPlanets.LowRezPlanet.CreateAtmosphere(this.meshHolder, planet, planet.atmosphereData.gradientHightMultiplier, "Scaled Space", (planet.type != CelestialBodyData.Type.Star) ? "Back" : "Default", (planet.type != CelestialBodyData.Type.Star) ? 0 : 500, planet.GenerateAtmosphereTexture());
				}
			}

			public static void CreateAtmosphere(Transform parent, CelestialBodyData planet, double multiplier, string layer, string sortingLayer, int sortingOrder, Texture2D tex)
			{
				Transform transform = UnityEngine.Object.Instantiate<Transform>(Ref.planetManager.atmosphereRenderer.transform, parent);
				transform.gameObject.SetActive(true);
				transform.gameObject.layer = LayerMask.NameToLayer(layer);
				transform.localPosition = Vector3.forward * 0.4f;
				transform.transform.localScale = Vector3.one * (float)(planet.atmosphereData.atmosphereHeightM * multiplier + planet.radius);
				Mesh mesh = transform.GetComponent<MeshFilter>().mesh;
				Vector2[] uv = mesh.uv;
				uv[0] = new Vector2(1f + (float)(planet.radius / (planet.atmosphereData.atmosphereHeightM * multiplier)), 0f);
				mesh.uv = uv;
				transform.GetComponent<MeshRenderer>().sortingLayerName = sortingLayer;
				transform.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
				transform.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", tex);
			}

			public CelestialBodyData planet;

			public Transform meshHolder;
		}
	}

	[Serializable]
	public class TerrainPoints
	{
		public TerrainPoints(int index)
		{
			this.points = new Vector3[index];
			this.uvOthers = new Vector2[index];
		}

		public Vector3[] points;

		public Vector2[] uvOthers;
	}

	[Serializable]
	public class Chunck
	{
		public Chunck(double from, double size, int LODId, CelestialBodyData planet, CelestialBodyData.TerrainData.DetailLevel[] detailLevels, Transform parent, bool forMap, bool offset)
		{
			this.from = from;
			this.LODId = LODId;
			this.topSizeHalf = (double)detailLevels[LODId].angularSize / planet.terrainData.unitToAngle * 0.5;
			float num = (float)from / (float)planet.terrainData.heightMaps[0].heightMap.HeightDataArray.Length * 360f + detailLevels[LODId].angularSize * 0.5f;
			this.topPosition = new Double3(Math.Cos((double)(num * 0.0174532924f)), Math.Sin((double)(num * 0.0174532924f))) * (planet.radius + planet.terrainData.maxTerrainHeight);
			this.chunckTransform = UnityEngine.Object.Instantiate<Transform>(Ref.planetManager.chunckPrefab).transform;
			this.chunckTransform.parent = parent;
			this.chunckTransform.localPosition = Vector3.zero;
			PlanetManager.TerrainPoints terrainPoints = planet.GetTerrainPoints(from, size, detailLevels[LODId].LOD, offset);
			this.GenerateMesh(this.chunckTransform, terrainPoints, this.GenerateIndices(terrainPoints.points.Length), from, 1f / (float)((double)planet.terrainData.heightMaps[0].heightMap.HeightDataArray.Length * detailLevels[LODId].LOD), 1f / (float)detailLevels[LODId].LOD, "Default", 10, planet, forMap);
			if (offset)
			{
				this.chunckPosOffset = terrainPoints.points[0];
			}
		}

		private void GenerateMesh(Transform chunckTrans, PlanetManager.TerrainPoints terrainPoints, int[] indices, double fromForUV, float traingleAngularSizeOf1, float texMultiplier, string sortingLayer, int orderInLayer, CelestialBodyData planet, bool map)
		{
			Mesh mesh = chunckTrans.GetComponent<MeshFilter>().mesh;
			mesh.Clear();
			mesh.vertices = terrainPoints.points;
			mesh.triangles = indices;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			Vector2[] array = new Vector2[terrainPoints.points.Length];
			for (int i = 0; i < terrainPoints.points.Length - 1; i++)
			{
				array[i + 1] = new Vector2((float)(fromForUV % 10.0) + (float)i * texMultiplier, 0f);
			}
			array[0] = Vector3.up;
			mesh.uv = array;
			Vector2[] array2 = new Vector2[terrainPoints.points.Length];
			double num = fromForUV / (double)((float)planet.terrainData.heightMaps[0].heightMap.HeightDataArray.Length);
			for (int j = 0; j < terrainPoints.points.Length - 1; j++)
			{
				array2[j + 1] = new Vector2((float)num + (float)j * traingleAngularSizeOf1, 1f);
			}
			array2[0] = Vector3.zero;
			mesh.uv2 = array2;
			mesh.uv3 = terrainPoints.uvOthers;
			MeshRenderer component = chunckTrans.GetComponent<MeshRenderer>();
			component.sortingLayerName = sortingLayer;
			component.sortingOrder = orderInLayer;
			if (map)
			{
				component.material = planet.terrainData.mapMaterial;
				component.material.color = planet.terrainData.mapColor;
			}
			else
			{
				component.sharedMaterial = planet.terrainData.terrainMaterial;
			}
		}

		private int[] GenerateIndices(int lenght)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < lenght - 2; i++)
			{
				list.Add(0);
				list.Add(i + 2);
				list.Add(i + 1);
			}
			return list.ToArray();
		}

		public void GenerateCollider()
		{
			PolygonCollider2D polygonCollider2D = this.chunckTransform.gameObject.AddComponent<PolygonCollider2D>();
			Vector3[] vertices = this.chunckTransform.GetComponent<MeshFilter>().mesh.vertices;
			Vector2[] array = new Vector2[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
			{
				array[i] = vertices[i];
			}
			polygonCollider2D.points = array;
			this.chunckTransform.parent = null;
		}

		public Transform chunckTransform;

		[TableColumnWidth(60)]
		public double from;

		[TableColumnWidth(50)]
		public int LODId;

		public double updateSplit;

		public double updateMerge;

		public double topSizeHalf;

		public Double3 topPosition;

		public Vector3 chunckPosOffset;

		public PlanetManager.Chunck secondHalf;

		public PlanetManager.Chunck parentChunck;
	}
}
