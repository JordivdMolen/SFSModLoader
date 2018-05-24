using System;
using System.Collections.Generic;
using UnityEngine;

public static class TransferWindow
{
	public static void PositionPlanetMarker(ref bool show, ref MeshFilter markerPlanet, TransferWindow.Data transferWindow)
	{
		if (transferWindow.transferType != TransferWindow.TransferType.ToNeighbour)
		{
			return;
		}
		double num = transferWindow.phaseAngle + Ref.controller.globalTime * transferWindow.secondNeighbour.orbitData._meanMotion;
		markerPlanet.transform.localEulerAngles = new Vector3(0f, 0f, (float)num * 57.29578f);
		show = true;
	}

	public static void PositionOrbitMarker(ref bool show, ref MeshFilter meshFilterOrbit, List<Orbit> orbits, TransferWindow.Data transferWindow)
	{
		if (transferWindow.transferType == TransferWindow.TransferType.None)
		{
			return;
		}
		if (orbits.Count == 0)
		{
			return;
		}
		if (orbits[0].orbitType == Orbit.Type.Encounter)
		{
			return;
		}
		double ejectionOrbitHeight = Math.Min(orbits[0].periapsis * 1.35, orbits[0].apoapsis);
		TransferWindow.UpdateLocalTransferWindowMarker(ref show, ref meshFilterOrbit, ejectionOrbitHeight, orbits[0], transferWindow);
	}

	public static void SetParents(TransferWindow.Data transferWindowData, Transform markerPlanet, Transform markerOrbit, Material meshPlanetMaterial)
	{
		if (transferWindowData.transferType == TransferWindow.TransferType.None)
		{
			return;
		}
		markerOrbit.parent = Ref.map.mapRefs[transferWindowData.departure].holder;
		markerOrbit.localPosition = Vector3.zero;
		if (transferWindowData.transferType != TransferWindow.TransferType.ToNeighbour)
		{
			return;
		}
		bool flag = transferWindowData.firstNeighbour.orbitData.orbitHeightM > transferWindowData.secondNeighbour.orbitData.orbitHeightM;
		float num = (float)(transferWindowData.firstNeighbour.orbitData.orbitHeightM / 10000.0);
		markerPlanet.localScale = new Vector3(num, (!flag) ? (-num) : num, 1f);
		markerPlanet.parent = Ref.map.mapRefs[transferWindowData.firstNeighbour.parentBody].holder;
		markerPlanet.localPosition = Vector3.zero;
		TransferWindow.UpdateTransferWindowAlpha(meshPlanetMaterial, transferWindowData);
	}

	public static void GenerateMeshPlanetTW(ref MeshFilter meshFilterPlanet, float widthPercent, float angularSize, float size, Transform parent)
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
		if (meshFilterPlanet.mesh == null)
		{
			meshFilterPlanet.mesh = new Mesh();
		}
		meshFilterPlanet.mesh.vertices = array;
		meshFilterPlanet.mesh.triangles = list.ToArray();
		meshFilterPlanet.mesh.colors = array2;
		meshFilterPlanet.mesh.RecalculateNormals();
		meshFilterPlanet.mesh.RecalculateBounds();
		meshFilterPlanet.transform.localScale = Vector3.one * size;
		meshFilterPlanet.transform.parent = parent;
		meshFilterPlanet.transform.localPosition = Vector3.zero;
	}

	private static void GenerateMeshOrbitTW(ref MeshFilter meshFilterOrbit, ref bool show, float ejectionAngle, Orbit orbit)
	{
		for (int i = 0; i < 2; i++)
		{
			double num = Math.Abs(Kepler.GetRadius(Kepler.GetSemiLatusRectum(orbit.periapsis, orbit.eccentricity), orbit.eccentricity, (double)(ejectionAngle + (float)i * 0.2f * (float)Math.Sign(orbit.meanMotion)) - orbit.argumentOfPeriapsis));
			if (num > orbit.planet.orbitData.SOI || num > orbit.periapsis * 1.35)
			{
				return;
			}
		}
		Vector3[] array = new Vector3[42];
		for (int j = 0; j < 21; j++)
		{
			float num2 = ejectionAngle + (float)j * 0.01f * (float)Math.Sign(orbit.meanMotion);
			array[j * 2] = new Vector3(Mathf.Cos(num2), Mathf.Sin(num2), 0f) * (float)(Kepler.GetRadius(Kepler.GetSemiLatusRectum(orbit.periapsis, orbit.eccentricity), orbit.eccentricity, (double)num2 - orbit.argumentOfPeriapsis) / 10000.0);
			array[j * 2 + 1] = array[j * 2] * 0.915f;
		}
		meshFilterOrbit.mesh.vertices = array;
		meshFilterOrbit.mesh.RecalculateNormals();
		meshFilterOrbit.mesh.RecalculateBounds();
		show = true;
	}

	private static void UpdateLocalTransferWindowMarker(ref bool show, ref MeshFilter meshFilterOrbit, double ejectionOrbitHeight, Orbit orbit, TransferWindow.Data transferWindow)
	{
		if (transferWindow.departure.type == CelestialBodyData.Type.Star || (transferWindow.transferType == TransferWindow.TransferType.ToNeighbour && transferWindow.departure != transferWindow.firstNeighbour))
		{
			return;
		}
		if (transferWindow.transferType == TransferWindow.TransferType.ToNeighbour || transferWindow.transferType == TransferWindow.TransferType.ToParent)
		{
			CelestialBodyData celestialBodyData = (transferWindow.transferType != TransferWindow.TransferType.ToNeighbour) ? transferWindow.departure : transferWindow.firstNeighbour;
			double orbitHeightM = celestialBodyData.orbitData.orbitHeightM;
			double num = (transferWindow.transferType != TransferWindow.TransferType.ToNeighbour) ? (transferWindow.target.radius + transferWindow.target.atmosphereData.atmosphereHeightM) : transferWindow.secondNeighbour.orbitData.orbitHeightM;
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
			TransferWindow.GenerateMeshOrbitTW(ref meshFilterOrbit, ref show, ejectionAngle, orbit);
			return;
		}
		if (transferWindow.transferType != TransferWindow.TransferType.ToSatellite)
		{
			return;
		}
		if (ejectionOrbitHeight > transferWindow.target.orbitData.orbitHeightM * 0.75)
		{
			return;
		}
		float num5 = (float)Kepler.GetPhaseAngle(ejectionOrbitHeight, transferWindow.target.orbitData.orbitHeightM);
		float ejectionAngle2 = num5 + (float)(Ref.controller.globalTime * transferWindow.target.orbitData._meanMotion);
		TransferWindow.GenerateMeshOrbitTW(ref meshFilterOrbit, ref show, ejectionAngle2, orbit);
	}

	public static void UpdateTransferWindowAlpha(Material material, TransferWindow.Data data)
	{
		material.color = new Color(1f, 1f, 1f, 0.05f + Mathf.Clamp01((float)(-(float)(Ref.map.mapPosition.z * 10000.0) / (data.firstNeighbour.orbitData.orbitHeightM * 0.5))) * 0.2f);
	}

	[Serializable]
	public class Data
	{
		private Data(TransferWindow.TransferType transferType, CelestialBodyData departure, CelestialBodyData target, CelestialBodyData firstNeighbour, CelestialBodyData secondNeighbour)
		{
			this.transferType = transferType;
			this.departure = departure;
			this.target = target;
			this.firstNeighbour = firstNeighbour;
			this.secondNeighbour = secondNeighbour;
			this.phaseAngle = double.NaN;
			this.phaseAngle = ((transferType != TransferWindow.TransferType.ToNeighbour) ? double.NaN : Kepler.GetPhaseAngle(firstNeighbour, secondNeighbour));
		}

		private static TransferWindow.Data Empty()
		{
			return new TransferWindow.Data(TransferWindow.TransferType.None, null, null, null, null);
		}

		public static TransferWindow.Data GetTransferType(Vessel vessel, CelestialBodyData target)
		{
			if (target == null || vessel == null)
			{
				return TransferWindow.Data.Empty();
			}
			CelestialBodyData getVesselPlanet = vessel.GetVesselPlanet;
			if (target == null || getVesselPlanet.type == CelestialBodyData.Type.Star || target.type == CelestialBodyData.Type.Star)
			{
				return TransferWindow.Data.Empty();
			}
			if (getVesselPlanet == target)
			{
				return TransferWindow.Data.Empty();
			}
			if (getVesselPlanet == target.parentBody)
			{
				return new TransferWindow.Data(TransferWindow.TransferType.ToSatellite, getVesselPlanet, target, null, null);
			}
			if (getVesselPlanet.parentBody == target)
			{
				return new TransferWindow.Data(TransferWindow.TransferType.ToParent, getVesselPlanet, target, null, null);
			}
			if (getVesselPlanet.parentBody == target.parentBody)
			{
				return new TransferWindow.Data(TransferWindow.TransferType.ToNeighbour, getVesselPlanet, target, getVesselPlanet, target);
			}
			if (getVesselPlanet.parentBody.parentBody == target.parentBody && getVesselPlanet.parentBody != target)
			{
				return new TransferWindow.Data(TransferWindow.TransferType.ToNeighbour, getVesselPlanet, target, getVesselPlanet.parentBody, target);
			}
			if (getVesselPlanet.parentBody == target.parentBody.parentBody && getVesselPlanet != target.parentBody)
			{
				return new TransferWindow.Data(TransferWindow.TransferType.ToNeighbour, getVesselPlanet, target, getVesselPlanet, target.parentBody);
			}
			if (getVesselPlanet.parentBody.parentBody == target.parentBody.parentBody)
			{
				return new TransferWindow.Data(TransferWindow.TransferType.ToNeighbour, getVesselPlanet, target, getVesselPlanet.parentBody, target.parentBody);
			}
			return TransferWindow.Data.Empty();
		}

		public TransferWindow.TransferType transferType;

		public CelestialBodyData departure;

		public CelestialBodyData target;

		public CelestialBodyData firstNeighbour;

		public CelestialBodyData secondNeighbour;

		public double phaseAngle;
	}

	public enum TransferType
	{
		None,
		ToSatellite,
		ToNeighbour,
		ToParent
	}
}
