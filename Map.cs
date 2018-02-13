using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
	[Serializable]
	public class TransferWindow
	{
		public MeshFilter markerMesh;

		public MeshFilter localMarkerMesh;

		public Map.TransferType transferType;

		public double phaseAngle;

		public CelestialBodyData departure;

		public CelestialBodyData target;

		public CelestialBodyData firstNeighbour;

		public CelestialBodyData secondNeighbour;

		public LineRenderer closestApproachMarker;
	}

	public enum TransferType
	{
		None,
		ToSatellite,
		ToNeighbour,
		ToParent
	}

	[Serializable]
	public class Refs
	{
		public Transform holder;

		public LineRenderer orbitLine;

		public TextMesh nameIconText;
	}

	public Double3 mapPosition;

	[Space]
	public CelestialBodyData following;

	[Space]
	private Vector3[] ellipsePoints;

	public LineRenderer[] orbitLines;

	[Space]
	public Map.TransferWindow transferWindow;

	[Space]
	public float orbitLineWidth;

	public const float maxZoom = 2E+07f;

	public const float minZoom = 0.15f;

	public const int orbitsCount = 3;

	public const double mapScale = 10000.0;

	public Sprite spriteSOI;

	[BoxGroup("Map Elements Materials", true, false, 0)]
	public Material sunLinesMaterial;

	[BoxGroup("Map Elements Materials", true, false, 0)]
	public Material planetLinesMaterial;

	[BoxGroup("Map Elements Materials", true, false, 0)]
	public Material moonLinesMaterial;

	[BoxGroup("Map Elements Materials", true, false, 0)]
	public Material transferWindowMaterial;

	[BoxGroup("Map Elements Materials", true, false, 0)]
	public Material localTransferWindowMaterial;

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

	[BoxGroup, TableList]
	public Map.Refs[] planetsMapData;

	public Transform leftArrow;

	public Transform rightArrow;

	public void InitializeMap()
	{
		this.ellipsePoints = new Vector3[this.vesselOrbitLinePrefab.GetChild(0).GetComponent<LineRenderer>().positionCount];
		for (int i = 0; i < this.ellipsePoints.Length; i++)
		{
			this.ellipsePoints[i] = this.vesselOrbitLinePrefab.GetChild(0).GetComponent<LineRenderer>().GetPosition(i);
			Vector3[] expr_5C_cp_0 = this.ellipsePoints;
			int expr_5C_cp_1 = i;
			expr_5C_cp_0[expr_5C_cp_1].y = expr_5C_cp_0[expr_5C_cp_1].y * 0.5f;
			this.ellipsePoints[i] = this.ellipsePoints[i].normalized;
		}
		this.orbitLines = this.CreateVesselOrbitLines(3, "Selected vessel orbit line");
		int num = 0;
		Ref.solarSystemRoot.mapAdressId = num;
		num++;
		Ref.solarSystemRoot.mapRefs.holder = UnityEngine.Object.Instantiate<Transform>(this.planetHolderPrefab);
		Ref.solarSystemRoot.mapRefs.holder.gameObject.layer = base.gameObject.layer;
		Ref.solarSystemRoot.mapRefs.holder.parent = base.transform;
		Ref.solarSystemRoot.mapRefs.holder.localPosition = Vector3.zero;
		Ref.solarSystemRoot.mapRefs.holder.name = "Sun holder ( Map)";
		Ref.solarSystemRoot.mapRefs.nameIconText = this.CreateNameIcon(Ref.solarSystemRoot.bodyName, Ref.solarSystemRoot.mapRefs.holder).GetComponent<TextMesh>();
		base.transform.GetChild(0).position = Ref.solarSystemRoot.mapRefs.holder.position;
		base.transform.GetChild(0).parent = Ref.solarSystemRoot.mapRefs.holder;
		this.CreateScaledTerrain(Ref.solarSystemRoot, Ref.solarSystemRoot.mapRefs.holder);
		CelestialBodyData[] satellites = Ref.solarSystemRoot.satellites;
		for (int j = 0; j < satellites.Length; j++)
		{
			CelestialBodyData celestialBodyData = satellites[j];
			celestialBodyData.mapAdressId = num;
			num++;
			this.CreateMapCelestialBody(celestialBodyData, base.transform, Ref.solarSystemRoot.mapRefs.holder, 200);
			CelestialBodyData[] satellites2 = celestialBodyData.satellites;
			for (int k = 0; k < satellites2.Length; k++)
			{
				CelestialBodyData celestialBodyData2 = satellites2[k];
				celestialBodyData2.mapAdressId = num;
				num++;
				this.CreateMapCelestialBody(celestialBodyData2, celestialBodyData.mapRefs.holder, celestialBodyData.mapRefs.holder, 100);
			}
		}
		this.following = Ref.GetPlanetByName(Ref.controller.startAdress);
		this.UpdateMapZoom(-this.mapPosition.z);
		this.GetTransferType();
		this.GenerateTransferWindowMarker(this.transferWindow.localMarkerMesh, 0.05f, -0.005f, 1f, null);
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
			foreach (Vessel current in Ref.controller.vessels)
			{
				current.mapIcon.transform.parent = current.GetVesselPlanet.mapRefs.holder;
			}
			if (Ref.selectedVessel == null)
			{
				this.SelectVessel(Ref.mainVessel);
			}
			else
			{
				this.SelectVessel(Ref.selectedVessel);
			}
		}
		else
		{
			Ref.controller.UpdateVesselButtons();
			Ref.cam.transform.position = new Vector3(Ref.controller.cameraPositionGame.x, Ref.controller.cameraPositionGame.y, -Ref.controller.cameraDistanceGame);
		}
		Ref.inputController.instructionsMap.SetActive(false);
	}

	public void OnClickEmpty(Vector2 clickPosWorld)
	{
		CelestialBodyData celestialBodyData = this.PointCastMapPlanets(clickPosWorld);
		if (celestialBodyData != null)
		{
			Ref.map.SelectCelestilaBodyTarget(celestialBodyData);
		}
		else
		{
			Vessel vessel = Ref.map.PointCastVessels(clickPosWorld);
			if (vessel != null)
			{
				Ref.map.SelectVessel(vessel);
			}
		}
	}

	private CelestialBodyData PointCastMapPlanets(Vector2 clickPos)
	{
		float num = Mathf.Pow(0.035f * (float)(-(float)Ref.map.mapPosition.z), 2f);
		CelestialBodyData result = null;
		float sqrMagnitude = (Ref.solarSystemRoot.mapRefs.nameIconText.transform.position - (Vector3)clickPos).sqrMagnitude;
		if (sqrMagnitude < num && Ref.solarSystemRoot.mapRefs.nameIconText.color.a > 0.05f)
		{
			num = sqrMagnitude;
			result = Ref.solarSystemRoot;
		}
		CelestialBodyData[] satellites = Ref.solarSystemRoot.satellites;
		for (int i = 0; i < satellites.Length; i++)
		{
			CelestialBodyData celestialBodyData = satellites[i];
			float sqrMagnitude2 = (celestialBodyData.mapRefs.nameIconText.transform.position - (Vector3)clickPos).sqrMagnitude;
			if (sqrMagnitude2 < num && celestialBodyData.mapRefs.nameIconText.color.a > 0.05f)
			{
				num = sqrMagnitude2;
				result = celestialBodyData;
			}
			CelestialBodyData[] satellites2 = celestialBodyData.satellites;
			for (int j = 0; j < satellites2.Length; j++)
			{
				CelestialBodyData celestialBodyData2 = satellites2[j];
				float sqrMagnitude3 = (celestialBodyData2.mapRefs.nameIconText.transform.position - (Vector3)clickPos).sqrMagnitude;
				if (sqrMagnitude3 < num && celestialBodyData2.mapRefs.nameIconText.color.a > 0.05f)
				{
					num = sqrMagnitude3;
					result = celestialBodyData2;
				}
			}
		}
		return result;
	}

	public void SelectCelestilaBodyTarget(CelestialBodyData newTarget)
	{
		CelestialBodyData target = this.transferWindow.target;
		if (newTarget != target)
		{
			this.transferWindow.target = newTarget;
			newTarget.mapRefs.nameIconText.text = "►" + newTarget.bodyName + "◄";
			newTarget.mapRefs.nameIconText.fontStyle = FontStyle.BoldAndItalic;
			if (target != null)
			{
				target.mapRefs.nameIconText.text = target.bodyName;
				target.mapRefs.nameIconText.fontStyle = FontStyle.Bold;
			}
			Ref.controller.ShowMsg(newTarget.bodyName + " set as target");
		}
		else if (target != null)
		{
			target.mapRefs.nameIconText.text = target.bodyName;
			target.mapRefs.nameIconText.fontStyle = FontStyle.Bold;
			this.transferWindow.target = null;
		}
		Ref.inputController.PlayClickSound(0.2f);
		this.GetTransferType();
	}

	public void SelectVessel(Vessel newSelectedVessel)
	{
		if (newSelectedVessel == null)
		{
			return;
		}
		if (Ref.selectedVessel != newSelectedVessel)
		{
			Ref.inputController.PlayClickSound(0.2f);
		}
		Ref.selectedVessel = newSelectedVessel;
		this.UpdateVesselsMapIcons();
		if (Ref.selectedVessel.state == Vessel.State.RealTime)
		{
			Ref.map.UpdateRealtimeVesselOrbitLines(Ref.selectedVessel);
		}
		else
		{
			Ref.map.UpdateVesselOrbitLines(Ref.selectedVessel.orbits, Ref.timeWarping);
		}
		this.UpdateMapZoom(this.mapPosition.z);
		this.GetTransferType();
		Ref.controller.UpdateVesselButtons();
	}

	public Vessel PointCastVessels(Vector2 clickPos)
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
		if (Ref.mapView)
		{
			Double3 b = Double3.zero;
			if (this.following.type == CelestialBodyData.Type.Star)
			{
				b = this.mapPosition.roundTo1000;
				b.z = this.mapPosition.z;
				Ref.cam.transform.position = (this.mapPosition - b).toVector3;
			}
			else
			{
				Ref.cam.transform.position = this.mapPosition.toVector3;
				if (this.following.type == CelestialBodyData.Type.Planet)
				{
					b = this.following.GetPosOut(Ref.controller.globalTime) / 10000.0;
				}
				else
				{
					b = (this.following.GetPosOut(Ref.controller.globalTime) + this.following.parentBody.GetPosOut(Ref.controller.globalTime)) / 10000.0;
				}
			}
			Ref.solarSystemRoot.mapRefs.holder.position = -b.toVector3;
			CelestialBodyData[] satellites = Ref.solarSystemRoot.satellites;
			for (int i = 0; i < satellites.Length; i++)
			{
				CelestialBodyData celestialBodyData = satellites[i];
				celestialBodyData.mapRefs.holder.position = (celestialBodyData.GetPosOut(Ref.controller.globalTime) / 10000.0 - b).toVector3;
				celestialBodyData.mapRefs.orbitLine.transform.localEulerAngles = new Vector3(0f, 0f, (float)(Ref.controller.globalTime * celestialBodyData.orbitData._meanMotion) * 57.29578f);
				CelestialBodyData[] satellites2 = celestialBodyData.satellites;
				for (int j = 0; j < satellites2.Length; j++)
				{
					CelestialBodyData celestialBodyData2 = satellites2[j];
					celestialBodyData2.mapRefs.holder.localPosition = (celestialBodyData2.GetPosOut(Ref.controller.globalTime) / 10000.0).toVector3;
					celestialBodyData2.mapRefs.orbitLine.transform.localEulerAngles = new Vector3(0f, 0f, (float)(Ref.controller.globalTime * celestialBodyData2.orbitData._meanMotion) * 57.29578f);
				}
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			if (Ref.selectedVessel != null)
			{
				List<Orbit> list = new List<Orbit>();
				if (Ref.selectedVessel.state == Vessel.State.RealTime)
				{
					list = this.UpdateRealtimeVesselOrbitLines(Ref.selectedVessel);
				}
				else
				{
					list = Ref.selectedVessel.orbits;
					if (list.Count > 0 && list[0].orbitType == Orbit.Type.Encounter)
					{
						this.orbitLines[0].SetPositions(list[0].GenerateOrbitLinePoints(list[0].GetTrueAnomalyOut(Ref.controller.globalTime), list[0].endTrueAnomaly + (double)(12.566371f * (float)Math.Sign(list[0].meanMotion)), 200));
					}
				}
				flag4 = (list.Count > 0);
				flag = (this.transferWindow.target != null && this.GetClosestApproach(list, this.transferWindow.target));
				if (this.transferWindow.transferType != Map.TransferType.None)
				{
					if (this.transferWindow.transferType == Map.TransferType.ToNeighbour)
					{
						double num = this.transferWindow.phaseAngle + Ref.controller.globalTime * this.transferWindow.secondNeighbour.orbitData._meanMotion;
						this.transferWindow.markerMesh.transform.localEulerAngles = new Vector3(0f, 0f, (float)num * 57.29578f);
						flag2 = true;
					}
					flag3 = (list.Count > 0 && list[0].orbitType != Orbit.Type.Encounter);
					if (flag3)
					{
						flag3 = this.UpdateLocalTransferWindowMarker(Math.Min(list[0].periapsis * 1.2, list[0].apoapsis), list[0]);
						if (flag3)
						{
							this.UpdateTranserWindowMarkerAlpha(this.localTransferWindowMaterial, list[0].periapsis);
						}
					}
				}
			}
			if (this.transferWindow.closestApproachMarker.gameObject.activeSelf != flag)
			{
				this.transferWindow.closestApproachMarker.gameObject.SetActive(flag);
			}
			if (this.transferWindow.markerMesh.gameObject.activeSelf != flag2)
			{
				this.transferWindow.markerMesh.gameObject.SetActive(flag2);
			}
			if (this.transferWindow.localMarkerMesh.gameObject.activeSelf != flag3)
			{
				this.transferWindow.localMarkerMesh.gameObject.SetActive(flag3);
			}
			if (!flag4 && this.orbitLines[0].gameObject.activeSelf)
			{
				LineRenderer[] array = this.orbitLines;
				for (int k = 0; k < array.Length; k++)
				{
					LineRenderer lineRenderer = array[k];
					lineRenderer.gameObject.SetActive(false);
				}
			}
		}
	}

	private List<Orbit> UpdateRealtimeVesselOrbitLines(Vessel vessel)
	{
		if (vessel == null || vessel.partsManager.rb2d == null)
		{
			return new List<Orbit>();
		}
		Double3 velIn = Ref.velocityOffset + vessel.partsManager.rb2d.velocity;
		if (velIn.sqrMagnitude2d > 2.0)
		{
			List<Orbit> list = Orbit.CalculateOrbits(vessel.GetGlobalPosition, velIn, Ref.controller.loadedPlanet);
			this.UpdateVesselOrbitLines(list, false);
			return list;
		}
		return new List<Orbit>();
	}

	public void UpdateVesselOrbitLines(List<Orbit> orbits, bool forTimewarp)
	{
		for (int i = 0; i < 3; i++)
		{
			if (i < orbits.Count)
			{
				this.orbitLines[i].transform.parent.parent = orbits[i].planet.mapRefs.holder;
				this.orbitLines[i].gameObject.SetActive(true);
				this.orbitLines[i].material = ((orbits[i].planet.type != CelestialBodyData.Type.Moon) ? ((orbits[i].planet.type != CelestialBodyData.Type.Planet) ? this.sunLinesMaterial : this.planetLinesMaterial) : this.moonLinesMaterial);
				if ((!forTimewarp && this.orbitLines[i].material.color.a == 0f) || (orbits[i].eccentricity <= 1.0 && orbits[i].eccentricity > 0.9999999))
				{
					this.orbitLines[i].transform.gameObject.SetActive(false);
				}
				else
				{
					this.orbitLines[i].endColor = new Color(1f, 1f, 1f, 0.2f);
					if (orbits[i].orbitType == Orbit.Type.Escape)
					{
						this.orbitLines[i].SetPositions(orbits[i].GenerateOrbitLinePoints(-orbits[i].endTrueAnomaly, orbits[i].endTrueAnomaly, 200));
						this.orbitLines[i].startColor = new Color(1f, 1f, 1f, 0.05f);
						this.orbitLines[i].transform.parent.localPosition = Vector3.zero;
						this.orbitLines[i].transform.parent.localEulerAngles = Vector3.zero;
						this.orbitLines[i].transform.localEulerAngles = Vector3.zero;
						this.orbitLines[i].transform.parent.localScale = Vector3.one / 10000f;
					}
					else if (orbits[i].orbitType == Orbit.Type.Encounter)
					{
						double newTime = Math.Max(Ref.controller.globalTime, (i <= 0) ? 0.0 : orbits[i - 1].orbitEndTime);
						this.orbitLines[i].SetPositions(orbits[i].GenerateOrbitLinePoints(orbits[i].GetTrueAnomalyOut(newTime), orbits[i].endTrueAnomaly + (double)(12.566371f * (float)Math.Sign(orbits[i].meanMotion)), 200));
						this.orbitLines[i].startColor = new Color(1f, 1f, 1f, (i <= 0) ? 0.05f : 0.2f);
						this.orbitLines[i].transform.parent.localPosition = Vector3.zero;
						this.orbitLines[i].transform.parent.localEulerAngles = Vector3.zero;
						this.orbitLines[i].transform.localEulerAngles = Vector3.zero;
						this.orbitLines[i].transform.parent.localScale = Vector3.one / 10000f;
					}
					else if (orbits[i].orbitType == Orbit.Type.Eternal)
					{
						this.orbitLines[i].SetPositions(this.ellipsePoints);
						this.orbitLines[i].startColor = new Color(1f, 1f, 1f, 0.2f);
						this.orbitLines[i].transform.parent.localPosition = new Vector3(Mathf.Cos((float)orbits[i].argumentOfPeriapsis), Mathf.Sin((float)orbits[i].argumentOfPeriapsis), 0f) * (float)((orbits[i].periapsis - orbits[i].apoapsis) / 10000.0 * 0.5);
						this.orbitLines[i].transform.parent.localEulerAngles = new Vector3(0f, 0f, (float)orbits[i].argumentOfPeriapsis * 57.29578f);
						double num = Math.Sqrt(1.0 - orbits[i].eccentricity * orbits[i].eccentricity);
						double num2 = (num <= 0.0) ? 0.0 : (orbits[i].semiMajorAxis * num);
						this.orbitLines[i].transform.parent.localScale = new Vector3((float)(orbits[i].semiMajorAxis / 10000.0), (float)(num2 / 10000.0 * (double)(-(double)Math.Sign(orbits[i].meanMotion))), 1f);
						if (i > 0 && orbits[i - 1].orbitType == Orbit.Type.Escape)
						{
							this.orbitLines[i].startColor = new Color(1f, 1f, 1f, 0.05f);
							this.orbitLines[i].transform.localEulerAngles = new Vector3(0f, 0f, (float)orbits[i].GetEccentricAnomalyOut(orbits[i - 1].orbitEndTime) * 57.29578f * (float)(-(float)Math.Sign(orbits[i].meanMotion)));
						}
					}
				}
			}
			else if (this.orbitLines[i].transform.gameObject.activeSelf)
			{
				this.orbitLines[i].transform.gameObject.SetActive(false);
			}
		}
		this.GetTransferType();
	}

	private void GetTransferType()
	{
		this.transferWindow.transferType = Map.TransferType.None;
		this.transferWindow.phaseAngle = 0.0;
		this.transferWindow.departure = null;
		this.transferWindow.firstNeighbour = null;
		this.transferWindow.secondNeighbour = null;
		if (this.transferWindow.target == null || Ref.selectedVessel == null)
		{
			return;
		}
		this.transferWindow.departure = Ref.selectedVessel.GetVesselPlanet;
		this.transferWindow.localMarkerMesh.transform.parent = this.transferWindow.departure.mapRefs.holder;
		this.transferWindow.localMarkerMesh.transform.localPosition = Vector3.zero;
		if (this.transferWindow.target == null || this.transferWindow.departure.type == CelestialBodyData.Type.Star || this.transferWindow.target.type == CelestialBodyData.Type.Star)
		{
			return;
		}
		if (this.transferWindow.departure == this.transferWindow.target)
		{
			return;
		}
		if (this.transferWindow.departure == this.transferWindow.target.parentBody)
		{
			this.transferWindow.transferType = Map.TransferType.ToSatellite;
		}
		else if (this.transferWindow.departure.parentBody == this.transferWindow.target)
		{
			this.transferWindow.transferType = Map.TransferType.ToParent;
		}
		else if (this.transferWindow.departure.parentBody == this.transferWindow.target.parentBody)
		{
			this.transferWindow.firstNeighbour = this.transferWindow.departure;
			this.transferWindow.secondNeighbour = this.transferWindow.target;
			this.transferWindow.transferType = Map.TransferType.ToNeighbour;
		}
		else if (this.transferWindow.departure.parentBody.parentBody == this.transferWindow.target.parentBody && this.transferWindow.departure.parentBody != this.transferWindow.target)
		{
			this.transferWindow.firstNeighbour = this.transferWindow.departure.parentBody;
			this.transferWindow.secondNeighbour = this.transferWindow.target;
			this.transferWindow.transferType = Map.TransferType.ToNeighbour;
		}
		else if (this.transferWindow.departure.parentBody == this.transferWindow.target.parentBody.parentBody && this.transferWindow.departure != this.transferWindow.target.parentBody)
		{
			this.transferWindow.firstNeighbour = this.transferWindow.departure;
			this.transferWindow.secondNeighbour = this.transferWindow.target.parentBody;
			this.transferWindow.transferType = Map.TransferType.ToNeighbour;
		}
		else if (this.transferWindow.departure.parentBody.parentBody == this.transferWindow.target.parentBody.parentBody)
		{
			this.transferWindow.firstNeighbour = this.transferWindow.departure.parentBody;
			this.transferWindow.secondNeighbour = this.transferWindow.target.parentBody;
			this.transferWindow.transferType = Map.TransferType.ToNeighbour;
		}
		if (this.transferWindow.transferType == Map.TransferType.ToNeighbour)
		{
			this.UpdatePhaseAngle();
		}
	}

	private void UpdatePhaseAngle()
	{
		this.transferWindow.phaseAngle = Kepler.GetPhaseAngle(this.transferWindow.firstNeighbour, this.transferWindow.secondNeighbour);
		this.GenerateTransferWindowMarker(this.transferWindow.markerMesh, 0.05f, -0.005f, (float)(this.transferWindow.firstNeighbour.orbitData.orbitHeightM / 10000.0), this.transferWindow.firstNeighbour.parentBody.mapRefs.holder);
		this.UpdateTranserWindowMarkerAlpha(this.transferWindowMaterial, this.transferWindow.firstNeighbour.orbitData.orbitHeightM * 0.5);
	}

	private void GenerateTransferWindowMarker(MeshFilter meshFilter, float widthPercent, float angularSize, float size, Transform parent)
	{
		Vector3[] array = new Vector3[42];
		Color[] array2 = new Color[42];
		for (int i = 0; i < 21; i++)
		{
			array[i * 2] = new Vector3(Mathf.Cos((float)i * angularSize), Mathf.Sin((float)i * angularSize), 0f);
			array[i * 2 + 1] = array[i * 2] * (1f - widthPercent);
			array2[i * 2] = Color.white;
			array2[i * 2 + 1] = new Color(1f, 1f, 1f, 0f);
		}
		List<int> list = new List<int>();
		for (int j = 0; j < 20; j++)
		{
			list.Add(j * 2);
			list.Add(j * 2 + 2);
			list.Add(j * 2 + 1);
			list.Add(j * 2 + 1);
			list.Add(j * 2 + 2);
			list.Add(j * 2 + 3);
		}
		if (this.transferWindow.markerMesh.mesh == null)
		{
			meshFilter.mesh = new Mesh();
		}
		meshFilter.mesh.vertices = array;
		meshFilter.mesh.triangles = list.ToArray();
		meshFilter.mesh.colors = array2;
		meshFilter.mesh.RecalculateNormals();
		meshFilter.mesh.RecalculateBounds();
		meshFilter.transform.localScale = Vector3.one * size;
		meshFilter.transform.parent = parent;
		meshFilter.transform.localPosition = Vector3.zero;
	}

	private bool UpdateLocalTransferWindowMarker(double ejectionOrbitHeight, Orbit orbit)
	{
		if (this.transferWindow.transferType == Map.TransferType.ToNeighbour && this.transferWindow.departure != this.transferWindow.firstNeighbour)
		{
			return false;
		}
		if (this.transferWindow.transferType == Map.TransferType.ToNeighbour || this.transferWindow.transferType == Map.TransferType.ToParent)
		{
			CelestialBodyData celestialBodyData = (this.transferWindow.transferType != Map.TransferType.ToNeighbour) ? this.transferWindow.departure : this.transferWindow.firstNeighbour;
			double orbitHeightM = celestialBodyData.orbitData.orbitHeightM;
			double num = (this.transferWindow.transferType != Map.TransferType.ToNeighbour) ? (this.transferWindow.target.radius + this.transferWindow.target.atmosphereData.atmosphereHeightM) : this.transferWindow.secondNeighbour.orbitData.orbitHeightM;
			double num2 = Math.Min(num, orbitHeightM);
			double num3 = (orbitHeightM + num) * 0.5;
			double e = 1.0 - num2 / num3;
			double mass = celestialBodyData.parentBody.mass;
			double meanMotion = Kepler.GetMeanMotion(Kepler.GetPeriod(e, num3, mass), e, mass, num3);
			bool flag = num > orbitHeightM;
			double magnitude2d = Kepler.GetVelocity(num3, orbitHeightM, meanMotion, (!flag) ? 3.1415926535897931 : 0.0, (!flag) ? 3.1415926535897931 : 0.0, e, 0.0).magnitude2d;
			double escapeVelocity = magnitude2d - -celestialBodyData.orbitData.orbitalVelocity;
			float num4 = (float)(Kepler.GetEjectionAngle(escapeVelocity, ejectionOrbitHeight, celestialBodyData.mass, celestialBodyData.orbitData.SOI) * (double)Math.Sign(-orbit.meanMotion) + ((!flag) ? 3.1415926535897931 : 0.0));
			float ejectionAngle = num4 + (float)(Ref.controller.globalTime * celestialBodyData.orbitData._meanMotion) - 1.57079637f;
			return this.GenerateLocalTransferWindowMarker(ejectionAngle, orbit);
		}
		if (this.transferWindow.transferType != Map.TransferType.ToSatellite)
		{
			return false;
		}
		if (ejectionOrbitHeight > this.transferWindow.target.orbitData.orbitHeightM * 0.6)
		{
			return false;
		}
		float num5 = (float)Kepler.GetPhaseAngle(ejectionOrbitHeight, this.transferWindow.target.orbitData.orbitHeightM);
		float ejectionAngle2 = num5 + (float)(Ref.controller.globalTime * this.transferWindow.target.orbitData._meanMotion);
		return this.GenerateLocalTransferWindowMarker(ejectionAngle2, orbit);
	}

	private bool GenerateLocalTransferWindowMarker(float ejectionAngle, Orbit orbit)
	{
		for (int i = 0; i < 2; i++)
		{
			double num = Math.Abs(Kepler.GetRadius(Kepler.GetSemiLatusRectum(orbit.periapsis, orbit.eccentricity), orbit.eccentricity, (double)(ejectionAngle + (float)i * 0.2f * (float)Math.Sign(orbit.meanMotion)) - orbit.argumentOfPeriapsis));
			if (num > orbit.planet.orbitData.SOI || num > orbit.periapsis * 1.2)
			{
				return false;
			}
		}
		Vector3[] array = new Vector3[42];
		for (int j = 0; j < 21; j++)
		{
			float num2 = ejectionAngle + (float)j * 0.01f * (float)Math.Sign(orbit.meanMotion);
			array[j * 2] = new Vector3(Mathf.Cos(num2), Mathf.Sin(num2), 0f) * (float)(Kepler.GetRadius(Kepler.GetSemiLatusRectum(orbit.periapsis, orbit.eccentricity), orbit.eccentricity, (double)num2 - orbit.argumentOfPeriapsis) / 10000.0);
			array[j * 2 + 1] = array[j * 2] * 0.915f;
		}
		this.transferWindow.localMarkerMesh.mesh.vertices = array;
		this.transferWindow.localMarkerMesh.mesh.RecalculateNormals();
		this.transferWindow.localMarkerMesh.mesh.RecalculateBounds();
		return true;
	}

	private void UpdateTranserWindowMarkerAlpha(Material material, double markerOrbitHeight)
	{
		if (this.transferWindow.firstNeighbour != null)
		{
			material.color = new Color(1f, 1f, 1f, 0.01f + Mathf.Clamp01((float)(-(float)this.mapPosition.z * 10000.0 / markerOrbitHeight)) * 0.2f);
		}
	}

	private bool GetClosestApproach(List<Orbit> orbits, CelestialBodyData targetingPlanet)
	{
		for (int i = 0; i < orbits.Count; i++)
		{
			if (orbits[i].orbitType != Orbit.Type.Encounter)
			{
				if (orbits[i].planet == targetingPlanet)
				{
					return this.CalculateClosestApproach(orbits[i], targetingPlanet);
				}
				if (orbits[i].planet == targetingPlanet.parentBody)
				{
					return this.CalculateClosestApproach(orbits[i], targetingPlanet);
				}
			}
		}
		return false;
	}

	private bool CalculateClosestApproach(Orbit orbit, CelestialBodyData targetingPlanet)
	{
		double num = double.PositiveInfinity;
		Double3 @double = Double3.zero;
		Double3 a = Double3.zero;
		bool flag = false;
		if (orbit.planet != targetingPlanet)
		{
			List<double> list = new List<double>();
			if (orbit.periapsis < targetingPlanet.orbitData.orbitHeightM && orbit.apoapsis > targetingPlanet.orbitData.orbitHeightM)
			{
				double trueAnomalyAtRadius = Kepler.GetTrueAnomalyAtRadius(targetingPlanet.orbitData.orbitHeightM, orbit.semiLatusRectum, orbit.eccentricity);
				list.Add(trueAnomalyAtRadius);
				list.Add(-trueAnomalyAtRadius);
			}
			else
			{
				if (!orbit.CanPasSOI(targetingPlanet.orbitData.orbitHeightM, targetingPlanet.mapData.showClosestApproachDistance))
				{
					return false;
				}
				if (orbit.apoapsis < targetingPlanet.orbitData.orbitHeightM)
				{
					list.Add(3.1415927410125732);
				}
				else
				{
					list.Add(0.0);
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				double nextTrueAnomalyPassageTime = orbit.GetNextTrueAnomalyPassageTime(Ref.controller.globalTime, list[i]);
				if (nextTrueAnomalyPassageTime >= Ref.controller.globalTime)
				{
					Double3 position = Kepler.GetPosition(Kepler.GetRadius(orbit.semiLatusRectum, orbit.eccentricity, list[i]), list[i], orbit.argumentOfPeriapsis);
					Double3 double2 = (!(orbit.planet != targetingPlanet)) ? Double3.zero : targetingPlanet.GetPosOut(nextTrueAnomalyPassageTime);
					double sqrMagnitude2d = (position - double2).sqrMagnitude2d;
					if (sqrMagnitude2d < num)
					{
						num = sqrMagnitude2d;
						@double = position;
						a = double2;
						flag = true;
					}
				}
			}
		}
		else
		{
			@double = Kepler.GetPosition(orbit.periapsis, 0.0, orbit.argumentOfPeriapsis);
			flag = true;
		}
		if (!flag)
		{
			return false;
		}
		Transform transform = (!(orbit.planet != targetingPlanet)) ? targetingPlanet.mapRefs.holder : targetingPlanet.parentBody.mapRefs.holder;
		if (this.transferWindow.closestApproachMarker.transform.parent != transform)
		{
			this.transferWindow.closestApproachMarker.transform.parent = transform;
		}
		this.transferWindow.closestApproachMarker.transform.localPosition = (@double / 10000.0).toVector3;
		Double3 double3 = (a - @double) / 10000.0;
		this.transferWindow.closestApproachMarker.SetPosition(1, double3.toVector3);
		this.transferWindow.closestApproachMarker.sharedMaterial.mainTextureScale = new Vector2(Mathf.Max(1.6f, (float)(double3.magnitude2d / -(float)this.mapPosition.z * 80.0) + 0.6f), 1f);
		return true;
	}

	public void UpdateMapPosition(Double3 newMapPosition)
	{
		this.mapPosition = new Double3(newMapPosition.x, newMapPosition.y, this.mapPosition.z);
		if (this.mapPosition.magnitude2d > this.following.orbitData.SOI * 3.0 / 10000.0)
		{
			this.SwitchFollowingBody(this.following.parentBody);
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
		if (this.following.orbitData.SOI * 10.0 / 10000.0 < -this.mapPosition.z)
		{
			this.SwitchFollowingBody(this.following.parentBody);
		}
		this.TryFollowSattelites();
		this.UpdateOrbitLinesVisibility();
		if (this.transferWindow.phaseAngle != 0.0)
		{
			this.UpdateTranserWindowMarkerAlpha(this.transferWindowMaterial, this.transferWindow.firstNeighbour.orbitData.orbitHeightM * 0.5);
		}
	}

	private void UpdateOrbitLinesVisibility()
	{
		this.orbitLineWidth = 0.004f * (float)(-(float)Ref.map.mapPosition.z);
		if (this.following.type == CelestialBodyData.Type.Moon)
		{
			this.sunLinesMaterial.color = new Color(1f, 1f, 1f, this.GetOrbitLineAlpha(this.following.parentBody.orbitData.SOI / 10000.0));
			this.planetLinesMaterial.color = new Color(1f, 1f, 1f, this.GetOrbitLineAlpha(this.following.orbitData.SOI / 10000.0));
		}
		else if (this.following.type == CelestialBodyData.Type.Planet)
		{
			this.sunLinesMaterial.color = new Color(1f, 1f, 1f, this.GetOrbitLineAlpha(this.following.orbitData.SOI / 10000.0));
			this.planetLinesMaterial.color = new Color(1f, 1f, 1f, 1f);
		}
		else
		{
			this.sunLinesMaterial.color = new Color(1f, 1f, 1f, 1f);
			this.planetLinesMaterial.color = new Color(1f, 1f, 1f, 1f);
		}
		this.UpdateCelesialBodysOrbitLinesVisibility(Ref.solarSystemRoot, Vector3.one * (0.004f * (float)(-(float)Ref.map.mapPosition.z)));
		for (int i = 0; i < this.orbitLines.Length; i++)
		{
			this.orbitLines[i].widthMultiplier = this.orbitLineWidth;
		}
		this.transferWindow.closestApproachMarker.widthMultiplier = this.orbitLineWidth * 0.85f;
	}

	private void UpdateCelesialBodysOrbitLinesVisibility(CelestialBodyData parentBody, Vector3 nameIconScale)
	{
		parentBody.mapRefs.nameIconText.color = new Color(1f, 1f, 1f, (float)Math.Min(parentBody.orbitData.orbitHeightM * 40.0 / (-this.mapPosition.z * 10000.0) - 2.0, -this.mapPosition.z * 10000.0 / (parentBody.radius * parentBody.mapData.hideNameMultiplier) - 3.0));
		parentBody.mapRefs.nameIconText.transform.localScale = nameIconScale;
		CelestialBodyData[] satellites = parentBody.satellites;
		for (int i = 0; i < satellites.Length; i++)
		{
			CelestialBodyData celestialBodyData = satellites[i];
			celestialBodyData.mapRefs.orbitLine.widthMultiplier = this.orbitLineWidth;
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
		CelestialBodyData[] satellites = this.following.satellites;
		for (int i = 0; i < satellites.Length; i++)
		{
			CelestialBodyData celestialBodyData = satellites[i];
			if (celestialBodyData.orbitData.SOI * 10.0 / 10000.0 > -this.mapPosition.z && (celestialBodyData.GetPosOut(Ref.controller.globalTime) / 10000.0 - this.mapPosition).magnitude2d < celestialBodyData.orbitData.SOI * 3.0 / 10000.0)
			{
				this.SwitchFollowingBody(celestialBodyData);
			}
		}
	}

	private void SwitchFollowingBody(CelestialBodyData newFollow)
	{
		Double3 a = Double3.zero;
		if (newFollow.parentBody == this.following)
		{
			a = -newFollow.GetPosOut(Ref.controller.globalTime);
		}
		if (this.following.parentBody == newFollow)
		{
			a = this.following.GetPosOut(Ref.controller.globalTime);
		}
		this.mapPosition += a / 10000.0;
		this.following = newFollow;
	}

	private void CreateMapCelestialBody(CelestialBodyData celestialBody, Transform holderParent, Transform orbitLineParent, int orbitLineResolution)
	{
		celestialBody.mapRefs.holder = UnityEngine.Object.Instantiate<Transform>(this.planetHolderPrefab, holderParent);
		celestialBody.mapRefs.holder.gameObject.layer = base.gameObject.layer;
		celestialBody.mapRefs.holder.name = celestialBody.bodyName + " holder ( Map)";
		this.CreateScaledTerrain(celestialBody, celestialBody.mapRefs.holder);
		if (celestialBody.atmosphereData.hasAtmosphere)
		{
			this.CreateCircle(celestialBody.mapRefs.holder, (float)((celestialBody.radius + celestialBody.atmosphereData.atmosphereHeightM) / 10000.0), new Color(1f, 1f, 1f, 0.06f), 1, "Atmosphere");
		}
		this.CreateCircle(celestialBody.mapRefs.holder, (float)(celestialBody.orbitData.SOI / 10000.0), new Color(1f, 1f, 1f, 0.025f), 0, "SOI");
		celestialBody.mapRefs.orbitLine = this.CreatePlanetOrbitLine(orbitLineParent, celestialBody.orbitData.orbitHeightM, celestialBody.bodyName, orbitLineResolution, celestialBody.type);
		celestialBody.mapRefs.nameIconText = this.CreateNameIcon(celestialBody.bodyName, celestialBody.mapRefs.holder).GetComponent<TextMesh>();
	}

	private Transform CreateNameIcon(string name, Transform holder)
	{
		Transform transform = UnityEngine.Object.Instantiate<Transform>(this.nameIconPrefab, holder);
		transform.name = name + " name icon";
		transform.GetComponent<TextMesh>().text = name;
		transform.GetComponent<MeshRenderer>().sortingLayerName = "Map";
		transform.GetComponent<MeshRenderer>().sortingOrder = 101;
		return transform;
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
		transform.GetComponent<LineRenderer>().material = ((bodyType != CelestialBodyData.Type.Planet) ? this.planetLinesMaterial : this.sunLinesMaterial);
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

	public LineRenderer[] CreateVesselOrbitLines(int orbitCount, string name)
	{
		LineRenderer[] array = new LineRenderer[orbitCount];
		for (int i = 0; i < orbitCount; i++)
		{
			array[i] = UnityEngine.Object.Instantiate<Transform>(this.vesselOrbitLinePrefab, Vector3.zero, Quaternion.identity, base.transform).GetChild(0).GetComponent<LineRenderer>();
			array[i].widthMultiplier = 0.003f * (float)(-(float)this.mapPosition.z);
			array[i].sortingOrder = 2;
			array[i].sortingLayerName = "Map";
			array[i].gameObject.SetActive(false);
			array[i].transform.parent.name = name + " " + (i + 1);
			array[i].endColor = new Color(1f, 1f, 1f, 0.2f);
		}
		return array;
	}

	public void UpdateCameraRotation(Vector3 camRotation)
	{
		if (Ref.solarSystemRoot.mapRefs.nameIconText.transform.localEulerAngles == camRotation)
		{
			return;
		}
		Ref.solarSystemRoot.mapRefs.nameIconText.transform.localEulerAngles = camRotation;
		CelestialBodyData[] satellites = Ref.solarSystemRoot.satellites;
		for (int i = 0; i < satellites.Length; i++)
		{
			CelestialBodyData celestialBodyData = satellites[i];
			celestialBodyData.mapRefs.nameIconText.transform.localEulerAngles = camRotation;
			CelestialBodyData[] satellites2 = celestialBodyData.satellites;
			for (int j = 0; j < satellites2.Length; j++)
			{
				CelestialBodyData celestialBodyData2 = satellites2[j];
				celestialBodyData2.mapRefs.nameIconText.transform.localEulerAngles = camRotation;
			}
		}
	}
}
