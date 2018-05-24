using System;
using System.Collections.Generic;
using UnityEngine;

public static class OrbitLines
{
	public static void CalculateOrbitLines(OrbitLines.OrbitLinesPack orbitLines, List<Orbit> orbits)
	{
		if (orbits.Count == 0)
		{
			orbitLines.HideAll();
			return;
		}
		OrbitLines.DrawKeplerOrbits(orbitLines, orbits);
	}

	public static void DrawKeplerOrbits(OrbitLines.OrbitLinesPack orbitLines, List<Orbit> orbits)
	{
		orbitLines.hidden = false;
		bool ellipticalActive = false;
		bool[] array = new bool[3];
		int num = 0;
		while (num < orbits.Count && num < 3)
		{
			if (orbits[num].eccentricity > 1.0 || orbits[num].eccentricity <= 0.9999999)
			{
				Orbit.Type orbitType = orbits[num].orbitType;
				if (orbitType != Orbit.Type.Eternal)
				{
					if (orbitType != Orbit.Type.Escape)
					{
						if (orbitType == Orbit.Type.Encounter)
						{
							orbitLines.procedual[num].material = Ref.map.orbitLineMaterials[orbits[num].planet.type];
							double newTime = Math.Max(Ref.controller.globalTime, orbits[num].timeIn);
							orbitLines.procedual[num].SetPositions(orbits[num].GenerateOrbitLinePoints(orbits[num].GetTrueAnomalyOut(newTime), orbits[num].endTrueAnomaly + (double)(12.566371f * (float)Math.Sign(orbits[num].meanMotion)), 200));
							orbitLines.procedual[num].startColor = new Color(1f, 1f, 1f, (num <= 0) ? 0.05f : 0.2f);
							OrbitLines.SetParentAndPosition(orbitLines.procedual[num].transform.parent, Ref.map.mapRefs[orbits[num].planet].holder, Vector3.zero);
							array[num] = true;
						}
					}
					else
					{
						orbitLines.procedual[num].material = Ref.map.orbitLineMaterials[orbits[num].planet.type];
						orbitLines.procedual[num].SetPositions(orbits[num].GenerateOrbitLinePoints(-orbits[num].endTrueAnomaly, orbits[num].endTrueAnomaly, 200));
						orbitLines.procedual[num].startColor = new Color(1f, 1f, 1f, 0.05f);
						OrbitLines.SetParentAndPosition(orbitLines.procedual[num].transform.parent, Ref.map.mapRefs[orbits[num].planet].holder, Vector3.zero);
						array[num] = true;
					}
				}
				else
				{
					orbitLines.elliptical.material = Ref.map.orbitLineMaterials[orbits[num].planet.type];
					orbitLines.elliptical.startColor = new Color(1f, 1f, 1f, 0.2f);
					orbitLines.elliptical.transform.parent.localEulerAngles = new Vector3(0f, 0f, (float)orbits[num].argumentOfPeriapsis * 57.29578f);
					double num2 = Math.Sqrt(1.0 - orbits[num].eccentricity * orbits[num].eccentricity);
					double num3 = (num2 <= 0.0) ? 0.0 : (orbits[num].semiMajorAxis * num2);
					orbitLines.elliptical.transform.parent.localScale = new Vector3((float)(orbits[num].semiMajorAxis / 10000.0), (float)(num3 / 10000.0 * (double)(-(double)Math.Sign(orbits[num].meanMotion))), 1f);
					if (num > 0 && orbits[num - 1].orbitType == Orbit.Type.Escape)
					{
						orbitLines.elliptical.startColor = new Color(1f, 1f, 1f, 0.05f);
						orbitLines.elliptical.transform.localEulerAngles = new Vector3(0f, 0f, (float)orbits[num].GetEccentricAnomalyOut(orbits[num - 1].orbitEndTime) * 57.29578f * (float)(-(float)Math.Sign(orbits[num].meanMotion)));
					}
					Vector3 localPosition = new Vector3(Mathf.Cos((float)orbits[num].argumentOfPeriapsis), Mathf.Sin((float)orbits[num].argumentOfPeriapsis), 0f) * (float)((orbits[num].periapsis - orbits[num].apoapsis) / 20000.0);
					OrbitLines.SetParentAndPosition(orbitLines.elliptical.transform.parent, Ref.map.mapRefs[orbits[num].planet].holder, localPosition);
					ellipticalActive = true;
				}
			}
			num++;
		}
		orbitLines.SetActive(ellipticalActive, array);
	}

	private static void SetParentAndPosition(Transform orbitLineParent, Transform parent, Vector3 localPosition)
	{
		if (orbitLineParent.parent != parent)
		{
			orbitLineParent.parent = parent;
		}
		orbitLineParent.localPosition = localPosition;
	}

	[Serializable]
	public class Target
	{
		public Target(Vessel targetVessel)
		{
			this.targetType = OrbitLines.Target.Type.Vessel;
			this.targetVessel = targetVessel;
		}

		public Target(CelestialBodyData targetPlanet)
		{
			this.targetType = OrbitLines.Target.Type.CelestialBody;
			this.targetPlanet = targetPlanet;
		}

		public CelestialBodyData GetFollowingBody()
		{
			if (this.targetType == OrbitLines.Target.Type.CelestialBody)
			{
				return this.targetPlanet;
			}
			if (this.targetType == OrbitLines.Target.Type.Vessel)
			{
				return this.targetVessel.GetVesselPlanet;
			}
			return null;
		}

		public OrbitLines.Target.Type targetType;

		public Vessel targetVessel;

		public CelestialBodyData targetPlanet;

		public enum Type
		{
			CelestialBody,
			Vessel
		}
	}

	[Serializable]
	public class OrbitLinesPack
	{
		public OrbitLinesPack(Transform vesselOrbitLinePrefab)
		{
			this.elliptical = OrbitLines.OrbitLinesPack.CreateVesselOrbitLine("Elliptical", vesselOrbitLinePrefab);
			this.elliptical.SetPositions(Ref.map.ellipsePoints);
			this.elliptical.endColor = new Color(1f, 1f, 1f, 0.2f);
			this.elliptical.transform.localScale = Vector3.one;
			this.procedual = new LineRenderer[3];
			for (int i = 0; i < this.procedual.Length; i++)
			{
				this.procedual[i] = OrbitLines.OrbitLinesPack.CreateVesselOrbitLine("Orbit line procedual" + (i + 1), vesselOrbitLinePrefab);
			}
			for (int j = 0; j < this.procedual.Length; j++)
			{
				this.procedual[j].endColor = new Color(1f, 1f, 1f, 0.2f);
			}
		}

		private static LineRenderer CreateVesselOrbitLine(string name, Transform vesselOrbitLinePrefab)
		{
			LineRenderer component = UnityEngine.Object.Instantiate<Transform>(vesselOrbitLinePrefab, Vector3.zero, Quaternion.identity, Ref.map.transform).GetChild(0).GetComponent<LineRenderer>();
			component.widthMultiplier = 0.003f * (float)(-(float)Ref.map.mapPosition.z);
			component.sortingOrder = 2;
			component.sortingLayerName = "Map";
			component.transform.parent.gameObject.SetActive(false);
			component.transform.parent.name = name;
			component.transform.localScale = Vector3.one / 10000f;
			component.endColor = new Color(1f, 1f, 1f, 0.2f);
			return component;
		}

		public void UpdateLinesWidth(float newWidth)
		{
			this.elliptical.widthMultiplier = newWidth;
			for (int i = 0; i < this.procedual.Length; i++)
			{
				this.procedual[i].widthMultiplier = newWidth;
			}
		}

		public void HideAll()
		{
			if (this.hidden)
			{
				return;
			}
			this.hidden = true;
			if (this.elliptical.transform.parent.gameObject.activeSelf)
			{
				this.elliptical.transform.parent.gameObject.SetActive(false);
			}
			for (int i = 0; i < this.procedual.Length; i++)
			{
				if (this.procedual[i].transform.parent.gameObject.activeSelf)
				{
					this.procedual[i].transform.parent.gameObject.SetActive(false);
				}
			}
		}

		public void SetActive(bool ellipticalActive, bool[] procedualActive)
		{
			if (this.elliptical.transform.parent.gameObject.activeSelf != ellipticalActive)
			{
				this.elliptical.transform.parent.gameObject.SetActive(ellipticalActive);
			}
			for (int i = 0; i < this.procedual.Length; i++)
			{
				if (this.procedual[i].transform.parent.gameObject.activeSelf != procedualActive[i])
				{
					this.procedual[i].transform.parent.gameObject.SetActive(procedualActive[i]);
				}
			}
		}

		public bool hidden;

		public LineRenderer elliptical;

		public LineRenderer[] procedual;
	}
}
