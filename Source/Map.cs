using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Map : MonoBehaviour
{
	public void InitializeMap()
	{
		this.ellipsePoints = new Vector3[this.vesselOrbitLinePrefab.GetChild(0).GetComponent<LineRenderer>().positionCount];
		for (int i = 0; i < this.ellipsePoints.Length; i++)
		{
			this.ellipsePoints[i] = this.vesselOrbitLinePrefab.GetChild(0).GetComponent<LineRenderer>().GetPosition(i);
			Vector3[] array = this.ellipsePoints;
			int num = i;
			array[num].y = array[num].y * 0.5f;
			this.ellipsePoints[i] = this.ellipsePoints[i].normalized;
		}
		this.mainLines = new OrbitLines.OrbitLinesPack(this.vesselOrbitLinePrefab);
		this.selectedLines = new OrbitLines.OrbitLinesPack(this.vesselOrbitLinePrefab);
		this.orbitLineMaterials.Add(CelestialBodyData.Type.Star, this.sunMaterial);
		this.orbitLineMaterials.Add(CelestialBodyData.Type.Planet, this.planetMaterial);
		this.orbitLineMaterials.Add(CelestialBodyData.Type.Moon, this.moonMaterial);
		this.mapRefs.Add(Ref.solarSystemRoot, this.CreateMapCelestialBody(Ref.solarSystemRoot, base.transform, 0, true, false, false));
		this.sunGlow.parent = this.mapRefs[Ref.solarSystemRoot].holder;
		this.sunGlow.localPosition = Vector3.zero;
		foreach (CelestialBodyData celestialBodyData in Ref.solarSystemRoot.satellites)
		{
			this.mapRefs.Add(celestialBodyData, this.CreateMapCelestialBody(celestialBodyData, base.transform, 200, true, true, celestialBodyData.atmosphereData.hasAtmosphere));
			foreach (CelestialBodyData celestialBodyData2 in celestialBodyData.satellites)
			{
				this.mapRefs.Add(celestialBodyData2, this.CreateMapCelestialBody(celestialBodyData2, this.mapRefs[celestialBodyData].holder, 100, true, true, celestialBodyData2.atmosphereData.hasAtmosphere));
			}
		}
		this.following = new OrbitLines.Target(Ref.GetPlanetByName(Ref.controller.startAdress));
		this.UpdateMapZoom(-this.mapPosition.z);
		TransferWindow.GenerateMeshPlanetTW(ref this.transferWindowPlanet, 0.05f, 0.006f, 1f, null);
		TransferWindow.GenerateMeshPlanetTW(ref this.transferWindowOrbit, 0.05f, 0f, 1f, null);
		this.CreateNewTransferWindow();
	}

	public void ToggleMap()
	{
		Ref.mapView = !Ref.mapView;
		if (Ref.mapView)
		{
			Ref.cam.cullingMask = Ref.controller.mapView;
		}
		base.gameObject.SetActive(Ref.mapView);
		if (Ref.mapView)
		{
			foreach (Vessel vessel in Ref.controller.vessels)
			{
				vessel.mapIcon.transform.parent = this.mapRefs[vessel.GetVesselPlanet].holder;
			}
			if (Ref.selectedVessel == null)
			{
				this.SelectVessel(Ref.mainVessel, false);
			}
			else
			{
				this.SelectVessel(Ref.selectedVessel, false);
			}
		}
		else
		{
			Ref.controller.UpdateVesselButtons();
			Ref.cam.transform.position = new Vector3(Ref.controller.cameraPositionGame.x, Ref.controller.cameraPositionGame.y, -Ref.controller.cameraDistanceGame);
		}
		Ref.inputController.instructionsMap.SetActive(false);
	}

	public void SelectCelestilaBodyAsTarget(CelestialBodyData newTarget)
	{
		CelestialBodyData celestialBodyData = this.targetPlanet;
		if (newTarget != celestialBodyData)
		{
			this.targetPlanet = newTarget;
			this.mapRefs[newTarget].nameIconText.text = "►" + newTarget.bodyName + "◄";
			this.mapRefs[newTarget].nameIconText.fontStyle = FontStyle.BoldAndItalic;
			if (celestialBodyData != null)
			{
				this.mapRefs[celestialBodyData].nameIconText.text = celestialBodyData.bodyName;
				this.mapRefs[celestialBodyData].nameIconText.fontStyle = FontStyle.Bold;
			}
			MsgController.ShowMsg(newTarget.bodyName + " set as target");
		}
		else if (celestialBodyData != null)
		{
			this.mapRefs[celestialBodyData].nameIconText.text = celestialBodyData.bodyName;
			this.mapRefs[celestialBodyData].nameIconText.fontStyle = FontStyle.Bold;
			this.targetPlanet = null;
		}
		Ref.inputController.PlayClickSound(0.2f);
		this.CreateNewTransferWindow();
	}

	public void SelectVessel(Vessel newSelectedVessel, bool playSound)
	{
		if (newSelectedVessel == null)
		{
			return;
		}
		if (playSound && Ref.selectedVessel != newSelectedVessel)
		{
			Ref.inputController.PlayClickSound(0.2f);
		}
		Ref.selectedVessel = newSelectedVessel;
		this.UpdateVesselsMapIcons();
		Ref.controller.UpdateVesselButtons();
		this.DrawOrbitLines();
		this.UpdateMapZoom(this.mapPosition.z);
		this.CreateNewTransferWindow();
	}

	private void CreateNewTransferWindow()
	{
		this.transferWindowData = TransferWindow.Data.GetTransferType((!(Ref.selectedVessel != null)) ? Ref.mainVessel : Ref.selectedVessel, this.targetPlanet);
		TransferWindow.SetParents(this.transferWindowData, this.transferWindowPlanet.transform, this.transferWindowOrbit.transform, this.transferWindowPlanet.GetComponent<MeshRenderer>().sharedMaterial);
	}

	private void PositionTransferWindowMarkers(List<Orbit> orbits)
	{
		bool flag = false;
		TransferWindow.PositionPlanetMarker(ref flag, ref this.transferWindowPlanet, this.transferWindowData);
		if (this.transferWindowPlanet.gameObject.activeSelf != flag)
		{
			this.transferWindowPlanet.gameObject.SetActive(flag);
		}
		bool flag2 = false;
		TransferWindow.PositionOrbitMarker(ref flag2, ref this.transferWindowOrbit, orbits, this.transferWindowData);
		if (this.transferWindowOrbit.gameObject.activeSelf != flag2)
		{
			this.transferWindowOrbit.gameObject.SetActive(flag2);
		}
	}

	public void UpdateVesselsMapIcons()
	{
		for (int i = 0; i < Ref.controller.vessels.Count; i++)
		{
			Ref.controller.vessels[i].mapIcon.localScale = new Vector3(0.01f, 0.01f);
			Ref.controller.vessels[i].mapIcon.GetComponent<SpriteRenderer>().color = new Color(0.72f, 0.72f, 0.72f, 1f);
			Ref.controller.vessels[i].mapIcon.GetComponent<SpriteRenderer>().sortingOrder = 13;
		}
		if (Ref.mainVessel != null)
		{
			Ref.mainVessel.mapIcon.localScale = new Vector3(0.015f, 0.015f);
			Ref.mainVessel.mapIcon.GetComponent<SpriteRenderer>().sortingOrder = 14;
		}
		if (Ref.selectedVessel != null)
		{
			Ref.selectedVessel.mapIcon.GetComponent<SpriteRenderer>().color = Color.white;
			Ref.selectedVessel.mapIcon.GetComponent<SpriteRenderer>().sortingOrder = 15;
		}
	}

	private void LateUpdate()
	{
		if (!Ref.mapView)
		{
			return;
		}
		Double3 offset = this.GetOffset();
		Ref.cam.transform.position = ((this.following.GetFollowingBody().type != CelestialBodyData.Type.Star) ? this.mapPosition.toVector3 : (this.mapPosition - offset).toVector3);
		this.PositionPlanets(offset);
		List<Orbit> vesselOrbits = this.GetVesselOrbits(Ref.mainVessel);
		List<Orbit> list = (!(Ref.selectedVessel == Ref.mainVessel)) ? this.GetVesselOrbits(Ref.selectedVessel) : new List<Orbit>();
		this.DrawOrbitLines(vesselOrbits, list);
		bool flag = false;
		if (this.targetPlanet != null)
		{
			ClosestApproach.DrawClosestApproachPlanet(this.closestApproachLinePlanet, this.GetVesselOrbits(Ref.mainVessel), this.targetPlanet, ref flag);
		}
		if (this.closestApproachLinePlanet.gameObject.activeSelf != flag)
		{
			this.closestApproachLinePlanet.gameObject.SetActive(flag);
		}
		bool flag2 = false;
		if (Ref.selectedVessel != null && Ref.selectedVessel != Ref.mainVessel)
		{
			ClosestApproach.DrawClosestApproachVessel(this.closestApproachLineVessel, this.GetVesselOrbits(Ref.mainVessel), this.GetVesselOrbits(Ref.selectedVessel), ref flag2);
		}
		if (this.closestApproachLineVessel.gameObject.activeSelf != flag2)
		{
			this.closestApproachLineVessel.gameObject.SetActive(flag2);
		}
		this.PositionTransferWindowMarkers((vesselOrbits.Count <= 0) ? list : vesselOrbits);
	}

	public void DrawOrbitLines()
	{
		this.DrawOrbitLines(this.GetVesselOrbits(Ref.mainVessel), (!(Ref.selectedVessel == Ref.mainVessel)) ? this.GetVesselOrbits(Ref.selectedVessel) : new List<Orbit>());
	}

	public void DrawOrbitLines(List<Orbit> mainVesselOrbits, List<Orbit> selectedVesselOrbits)
	{
		OrbitLines.CalculateOrbitLines(this.mainLines, mainVesselOrbits);
		OrbitLines.CalculateOrbitLines(this.selectedLines, selectedVesselOrbits);
	}

	private void PositionPlanets(Double3 offset)
	{
		this.mapRefs[Ref.solarSystemRoot].holder.position = -offset.toVector3;
		foreach (CelestialBodyData celestialBodyData in Ref.solarSystemRoot.satellites)
		{
			this.mapRefs[celestialBodyData].holder.position = (celestialBodyData.GetPosOut(Ref.controller.globalTime) / 10000.0 - offset).toVector3;
			this.mapRefs[celestialBodyData].orbitLine.transform.localEulerAngles = new Vector3(0f, 0f, (float)(Ref.controller.globalTime * celestialBodyData.orbitData._meanMotion) * 57.29578f);
			foreach (CelestialBodyData celestialBodyData2 in celestialBodyData.satellites)
			{
				this.mapRefs[celestialBodyData2].holder.localPosition = (celestialBodyData2.GetPosOut(Ref.controller.globalTime) / 10000.0).toVector3;
				this.mapRefs[celestialBodyData2].orbitLine.transform.localEulerAngles = new Vector3(0f, 0f, (float)(Ref.controller.globalTime * celestialBodyData2.orbitData._meanMotion) * 57.29578f);
			}
		}
	}

	private List<Orbit> GetVesselOrbits(Vessel vessel)
	{
		if (vessel == null)
		{
			return new List<Orbit>();
		}
		if (vessel.state == Vessel.State.RealTime)
		{
			Double3 velIn = Ref.velocityOffset + vessel.partsManager.rb2d.velocity;
			if (velIn.sqrMagnitude2d > 4.0)
			{
				return Orbit.CalculateOrbits(vessel.GetGlobalPosition, velIn, Ref.controller.loadedPlanet);
			}
		}
		return vessel.orbits;
	}

	private Double3 GetOffset()
	{
		CelestialBodyData.Type type = this.following.GetFollowingBody().type;
		if (type == CelestialBodyData.Type.Star)
		{
			return new Double3(this.mapPosition.roundTo1000.x, this.mapPosition.roundTo1000.y, this.mapPosition.z);
		}
		if (type == CelestialBodyData.Type.Planet)
		{
			return this.following.GetFollowingBody().GetPosOut(Ref.controller.globalTime) / 10000.0;
		}
		if (type != CelestialBodyData.Type.Moon)
		{
			return Double3.zero;
		}
		return (this.following.GetFollowingBody().GetPosOut(Ref.controller.globalTime) + this.following.GetFollowingBody().parentBody.GetPosOut(Ref.controller.globalTime)) / 10000.0;
	}

	public void OnClickEmpty(Vector2 clickPosWorld)
	{
		CelestialBodyData celestialBodyData = this.PointCastMapPlanets(clickPosWorld);
		if (celestialBodyData != null)
		{
			this.SelectCelestilaBodyAsTarget(celestialBodyData);
			return;
		}
		Vessel vessel = this.PointCastVessels(clickPosWorld);
		if (vessel != null)
		{
			Ref.map.SelectVessel(vessel, true);
			return;
		}
	}

	private CelestialBodyData PointCastMapPlanets(Vector2 clickPos)
	{
		float num = Mathf.Pow(0.035f * (float)(-(float)Ref.map.mapPosition.z), 2f);
		CelestialBodyData result = null;
		float sqrMagnitude = (this.mapRefs[Ref.solarSystemRoot].nameIconText.transform.position - (Vector3)clickPos).sqrMagnitude;
		if (sqrMagnitude < num && this.mapRefs[Ref.solarSystemRoot].nameIconText.color.a > 0.05f)
		{
			num = sqrMagnitude;
			result = Ref.solarSystemRoot;
		}
		foreach (CelestialBodyData celestialBodyData in Ref.solarSystemRoot.satellites)
		{
			float sqrMagnitude2 = (this.mapRefs[celestialBodyData].nameIconText.transform.position - (Vector3)clickPos).sqrMagnitude;
			if (sqrMagnitude2 < num && this.mapRefs[celestialBodyData].nameIconText.color.a > 0.05f)
			{
				num = sqrMagnitude2;
				result = celestialBodyData;
			}
			foreach (CelestialBodyData celestialBodyData2 in celestialBodyData.satellites)
			{
				float sqrMagnitude3 = (this.mapRefs[celestialBodyData2].nameIconText.transform.position - (Vector3)clickPos).sqrMagnitude;
				if (sqrMagnitude3 < num && this.mapRefs[celestialBodyData2].nameIconText.color.a > 0.05f)
				{
					num = sqrMagnitude3;
					result = celestialBodyData2;
				}
			}
		}
		return result;
	}

	private Vessel PointCastVessels(Vector2 clickPos)
	{
		float num = Mathf.Pow(0.03f * (float)(-(float)Ref.map.mapPosition.z), 2f);
		Vessel result = null;
		for (int i = 0; i < Ref.controller.vessels.Count; i++)
		{
			float sqrMagnitude = (Ref.controller.vessels[i].mapIcon.position - (Vector3)clickPos).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = Ref.controller.vessels[i];
			}
		}
		return result;
	}

	public void UpdateMapPosition(Double3 newMapPosition)
	{
		this.mapPosition = new Double3(newMapPosition.x, newMapPosition.y, this.mapPosition.z);
		if (this.mapPosition.magnitude2d > this.following.GetFollowingBody().orbitData.SOI * 3.0 / 10000.0)
		{
			this.SwitchFollowingBody(this.following.GetFollowingBody().parentBody);
		}
		this.UpdateOrbitLinesVisibility();
	}

	public void UpdateMapZoom(double newZoom)
	{
		if (newZoom < 0.0)
		{
			newZoom = -newZoom;
		}
		if (newZoom > 20000000.0)
		{
			newZoom = 20000000.0;
		}
		if (newZoom < 0.15000000596046448)
		{
			newZoom = 0.15000000596046448;
		}
		this.mapPosition.z = -newZoom;
		if (this.following.GetFollowingBody().orbitData.SOI * 10.0 / 10000.0 < -this.mapPosition.z)
		{
			this.SwitchFollowingBody(this.following.GetFollowingBody().parentBody);
		}
		this.TryFollowSattelites();
		this.UpdateOrbitLinesVisibility();
		if (this.transferWindowData.transferType == TransferWindow.TransferType.ToNeighbour)
		{
			TransferWindow.UpdateTransferWindowAlpha(this.transferWindowPlanet.GetComponent<MeshRenderer>().sharedMaterial, this.transferWindowData);
		}
	}

	private void UpdateOrbitLinesVisibility()
	{
		this.orbitLineWidth = 0.004f * (float)(-(float)Ref.map.mapPosition.z);
		this.orbitLineMaterials[CelestialBodyData.Type.Planet].color = ((this.following.GetFollowingBody().type != CelestialBodyData.Type.Moon) ? Color.white : new Color(1f, 1f, 1f, this.GetOrbitLineAlpha(this.following.GetFollowingBody().orbitData.SOI / 10000.0)));
		this.orbitLineMaterials[CelestialBodyData.Type.Star].color = ((this.following.GetFollowingBody().type != CelestialBodyData.Type.Star) ? new Color(1f, 1f, 1f, this.GetOrbitLineAlpha(((this.following.GetFollowingBody().type != CelestialBodyData.Type.Planet) ? this.following.GetFollowingBody().parentBody : this.following.GetFollowingBody()).orbitData.SOI / 10000.0)) : Color.white);
		this.UpdateCelesialBodysOrbitLinesVisibility(Ref.solarSystemRoot, Vector3.one * (0.004f * (float)(-(float)Ref.map.mapPosition.z)));
		this.mainLines.UpdateLinesWidth(this.orbitLineWidth);
		this.selectedLines.UpdateLinesWidth(this.orbitLineWidth);
		this.closestApproachLinePlanet.widthMultiplier = this.orbitLineWidth * 0.85f;
		this.closestApproachLineVessel.widthMultiplier = this.orbitLineWidth * 0.85f;
	}

	private void UpdateCelesialBodysOrbitLinesVisibility(CelestialBodyData parentBody, Vector3 nameIconScale)
	{
		this.mapRefs[parentBody].nameIconText.color = new Color(1f, 1f, 1f, (float)Math.Min(parentBody.orbitData.orbitHeightM * 40.0 / (-this.mapPosition.z * 10000.0) - 2.0, -this.mapPosition.z * 10000.0 / (parentBody.radius * parentBody.mapData.hideNameMultiplier) - 3.0));
		this.mapRefs[parentBody].nameIconText.transform.localScale = nameIconScale;
		foreach (CelestialBodyData celestialBodyData in parentBody.satellites)
		{
			this.mapRefs[celestialBodyData].orbitLine.widthMultiplier = this.orbitLineWidth;
			this.UpdateCelesialBodysOrbitLinesVisibility(celestialBodyData, nameIconScale);
		}
	}

	private float GetOrbitLineAlpha(double followingSOI)
	{
		float num = Mathf.Clamp01((float)(this.mapPosition.magnitude2d / (followingSOI * 3.0)) * 4f - 3f);
		float num2 = Mathf.Clamp01((float)(-(float)this.mapPosition.z / (followingSOI * 3.5) - 1.0) * 1.5f);
		return Mathf.Pow((num <= num2) ? num2 : num, 2f);
	}

	private void TryFollowSattelites()
	{
		foreach (CelestialBodyData celestialBodyData in this.following.GetFollowingBody().satellites)
		{
			if (celestialBodyData.orbitData.SOI * 10.0 / 10000.0 > -this.mapPosition.z && (celestialBodyData.GetPosOut(Ref.controller.globalTime) / 10000.0 - this.mapPosition).magnitude2d < celestialBodyData.orbitData.SOI * 3.0 / 10000.0)
			{
				this.SwitchFollowingBody(celestialBodyData);
			}
		}
	}

	private void SwitchFollowingBody(CelestialBodyData newFollow)
	{
		Double3 @double = Double3.zero;
		if (newFollow.parentBody == this.following.targetPlanet)
		{
			@double = -newFollow.GetPosOut(Ref.controller.globalTime);
		}
		if (this.following.targetPlanet.parentBody == newFollow)
		{
			@double = this.following.targetPlanet.GetPosOut(Ref.controller.globalTime);
		}
		MonoBehaviour.print(@double);
		this.mapPosition += @double / 10000.0;
		this.following = new OrbitLines.Target(newFollow);
	}

	private Map.Refs CreateMapCelestialBody(CelestialBodyData celestialBody, Transform holderParent, int orbitLineResolution, bool createTerrain, bool createSOI, bool createAtmosphere)
	{
		Transform transform = UnityEngine.Object.Instantiate<Transform>(this.planetHolderPrefab, holderParent);
		transform.gameObject.layer = base.gameObject.layer;
		transform.name = celestialBody.bodyName + " holder ( Map)";
		if (createTerrain)
		{
			this.CreateScaledTerrain(celestialBody, transform);
		}
		if (createSOI)
		{
			this.CreateCircle(transform, (float)(celestialBody.orbitData.SOI / 10000.0), new Color(1f, 1f, 1f, 0.025f), 0, "SOI");
		}
		if (createAtmosphere)
		{
			this.CreateCircle(transform, (float)((celestialBody.radius + celestialBody.atmosphereData.atmosphereHeightM) / 10000.0), new Color(1f, 1f, 1f, 0.06f), 1, "Atmosphere");
		}
		return new Map.Refs(transform, (orbitLineResolution <= 0) ? null : this.CreatePlanetOrbitLine(this.mapRefs[celestialBody.parentBody].holder, celestialBody.orbitData.orbitHeightM, celestialBody.bodyName, orbitLineResolution, celestialBody.type), this.CreateNameIcon(celestialBody.bodyName, transform).GetComponent<TextMesh>());
	}

	private TextMesh CreateNameIcon(string name, Transform holder)
	{
		Transform transform = UnityEngine.Object.Instantiate<Transform>(this.nameIconPrefab, holder);
		transform.name = name + " name icon";
		transform.GetComponent<TextMesh>().text = name;
		transform.GetComponent<MeshRenderer>().sortingLayerName = "Map";
		transform.GetComponent<MeshRenderer>().sortingOrder = 101;
		return transform.GetComponent<TextMesh>();
	}

	private Transform CreateScaledTerrain(CelestialBodyData planet, Transform parent)
	{
		Transform chunckTransform = new PlanetManager.Chunck(16.0, (double)planet.terrainData.heightMaps[0].heightMap.HeightDataArray.Length, planet.terrainData.mapDetailLevelId, planet, planet.terrainData.detailLevels, base.transform, true, false).chunckTransform;
		chunckTransform.gameObject.layer = base.gameObject.layer;
		chunckTransform.localScale = Vector3.one * 1.0001f / 10000f;
		chunckTransform.parent = parent;
		chunckTransform.localPosition = Vector3.zero;
		MeshRenderer component = chunckTransform.GetComponent<MeshRenderer>();
		component.sortingOrder = 10;
		component.sortingLayerName = "Map";
		chunckTransform.name = "Mesh ( " + planet.bodyName + ")";
		return chunckTransform;
	}

	private LineRenderer CreatePlanetOrbitLine(Transform parent, double radius, string name, int orbitLineResolution, CelestialBodyData.Type bodyType)
	{
		Transform transform = UnityEngine.Object.Instantiate<Transform>(this.planetOrbitLinePrefab, Vector3.zero, Quaternion.identity, parent);
		transform.GetComponent<Ellipse>().resolution = orbitLineResolution;
		transform.name = name + " Orbit line";
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.one * (float)(radius / 10000.0);
		transform.GetComponent<LineRenderer>().sortingOrder = 0;
		transform.GetComponent<LineRenderer>().sortingLayerName = "Map";
		transform.GetComponent<LineRenderer>().startColor = new Color(1f, 1f, 1f, 0.12f);
		transform.GetComponent<LineRenderer>().endColor = new Color(1f, 1f, 1f, 0.025f);
		transform.GetComponent<LineRenderer>().material = ((bodyType != CelestialBodyData.Type.Planet) ? this.orbitLineMaterials[CelestialBodyData.Type.Planet] : this.orbitLineMaterials[CelestialBodyData.Type.Star]);
		return transform.GetComponent<LineRenderer>();
	}

	private void CreateCircle(Transform parent, float circleSize, Color color, int sortingOrder, string circleName)
	{
		Transform transform = UnityEngine.Object.Instantiate<Transform>(this.SOIPrefab, parent);
		transform.name = circleName;
		transform.localScale = Vector3.one * circleSize;
		transform.localPosition = Vector3.zero;
		SpriteRenderer component = transform.GetComponent<SpriteRenderer>();
		component.color = color;
		component.sortingLayerName = "Map";
		component.sortingOrder = sortingOrder;
		if (circleName == "SOI")
		{
			component.sprite = this.spriteSOI;
		}
	}

	public void UpdateCameraRotation(Vector3 camRotation)
	{
		if (this.mapRefs[Ref.solarSystemRoot].nameIconText.transform.localEulerAngles == camRotation)
		{
			return;
		}
		this.mapRefs[Ref.solarSystemRoot].nameIconText.transform.localEulerAngles = camRotation;
		foreach (CelestialBodyData celestialBodyData in Ref.solarSystemRoot.satellites)
		{
			this.mapRefs[celestialBodyData].nameIconText.transform.localEulerAngles = camRotation;
			foreach (CelestialBodyData key in celestialBodyData.satellites)
			{
				this.mapRefs[key].nameIconText.transform.localEulerAngles = camRotation;
			}
		}
	}

	public const double mapScale = 10000.0;

	public const double maxZoom = 20000000.0;

	public const double minZoom = 0.15000000596046448;

	public const int maxPatchConics = 3;

	[BoxGroup("a", false, false, 0)]
	public Double3 mapPosition;

	[BoxGroup("a", false, false, 0)]
	public OrbitLines.Target following;

	[BoxGroup("a", false, false, 0)]
	public CelestialBodyData targetPlanet;

	[Space]
	public float orbitLineWidth;

	[BoxGroup("b", false, false, 0)]
	public OrbitLines.OrbitLinesPack mainLines;

	[BoxGroup("c", false, false, 0)]
	public OrbitLines.OrbitLinesPack selectedLines;

	[BoxGroup("d", false, false, 0)]
	public LineRenderer closestApproachLinePlanet;

	[BoxGroup("d", false, false, 0)]
	public LineRenderer closestApproachLineVessel;

	[BoxGroup("Transfer Windows", true, false, 0)]
	public MeshFilter transferWindowPlanet;

	[BoxGroup("Transfer Windows", true, false, 0)]
	public MeshFilter transferWindowOrbit;

	[BoxGroup("Transfer Windows", true, false, 0)]
	public TransferWindow.Data transferWindowData;

	[BoxGroup("Dictionarys", true, false, 0)]
	[ShowInInspector]
	public Dictionary<CelestialBodyData.Type, Material> orbitLineMaterials = new Dictionary<CelestialBodyData.Type, Material>();

	[BoxGroup("Dictionarys", true, false, 0)]
	[ShowInInspector]
	public Dictionary<CelestialBodyData, Map.Refs> mapRefs = new Dictionary<CelestialBodyData, Map.Refs>();

	[BoxGroup("Orbit Lines Materials", true, false, 0)]
	public Material sunMaterial;

	[BoxGroup("Orbit Lines Materials", true, false, 0)]
	public Material planetMaterial;

	[BoxGroup("Orbit Lines Materials", true, false, 0)]
	public Material moonMaterial;

	[BoxGroup("Prefab References", true, false, 0)]
	public Transform mapVesselIconPrefab;

	[BoxGroup("Prefab References", true, false, 0)]
	public Transform planetHolderPrefab;

	[BoxGroup("Prefab References", true, false, 0)]
	public Transform SOIPrefab;

	[BoxGroup("Prefab References", true, false, 0)]
	public Transform planetOrbitLinePrefab;

	[BoxGroup("Prefab References", true, false, 0)]
	public Transform vesselOrbitLinePrefab;

	[BoxGroup("Prefab References", true, false, 0)]
	public Transform nameIconPrefab;

	public Transform sunGlow;

	[HideInInspector]
	public Vector3[] ellipsePoints;

	[DrawWithUnity]
	public Sprite spriteSOI;

	[Serializable]
	public class Refs
	{
		public Refs(Transform holder, LineRenderer orbitLine, TextMesh nameIconText)
		{
			this.holder = holder;
			this.orbitLine = orbitLine;
			this.nameIconText = nameIconText;
		}

		public Transform holder;

		public LineRenderer orbitLine;

		public TextMesh nameIconText;
	}
}
