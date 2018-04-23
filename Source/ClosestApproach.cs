using System;
using System.Collections.Generic;
using UnityEngine;

public static class ClosestApproach
{
	public static void DrawClosestApproachPlanet(LineRenderer closestApproachLine, List<Orbit> orbits, CelestialBodyData targetPlanet, ref bool drawn)
	{
		for (int i = 0; i < orbits.Count; i++)
		{
			bool flag = orbits[i].orbitType != Orbit.Type.Encounter;
			if (flag)
			{
				bool flag2 = orbits[i].planet == targetPlanet;
				if (flag2)
				{
					ClosestApproach.SetLine(closestApproachLine, Kepler.GetPosition(orbits[i].periapsis, 0.0, orbits[i].argumentOfPeriapsis), Double3.zero, Ref.map.mapRefs[targetPlanet].holder);
					drawn = true;
					break;
				}
				bool flag3 = orbits[i].planet == targetPlanet.parentBody;
				if (flag3)
				{
					ClosestApproach.CalculateClosestApproach(closestApproachLine, orbits[i], targetPlanet, ref drawn);
					bool flag4 = drawn;
					if (flag4)
					{
						break;
					}
				}
			}
		}
	}

	public static void DrawClosestApproachVessel(LineRenderer closestApproachLine, List<Orbit> orbits, List<Orbit> targetOrbits, ref bool drawn)
	{
		for (int i = 0; i < orbits.Count; i++)
		{
			for (int j = 0; j < targetOrbits.Count; j++)
			{
				bool flag = !(orbits[i].planet != targetOrbits[j].planet);
				if (flag)
				{
					ClosestApproach.CalculateClosestApproach(closestApproachLine, orbits[i], targetOrbits[j], Ref.controller.globalTime, ref drawn);
					bool flag2 = drawn;
					if (flag2)
					{
						return;
					}
				}
			}
		}
	}

	private static void CalculateClosestApproach(LineRenderer closestApproachLine, Orbit orbit, CelestialBodyData targetPlanet, ref bool drawn)
	{
		List<double> list = new List<double>();
		bool flag = orbit.periapsis < targetPlanet.orbitData.orbitHeightM && orbit.apoapsis > targetPlanet.orbitData.orbitHeightM;
		if (flag)
		{
			double trueAnomalyAtRadius = Kepler.GetTrueAnomalyAtRadius(targetPlanet.orbitData.orbitHeightM, orbit.semiLatusRectum, orbit.eccentricity);
			list.Add(trueAnomalyAtRadius);
			list.Add(-trueAnomalyAtRadius);
		}
		else
		{
			bool flag2 = !orbit.CanPasSOI(targetPlanet.orbitData.orbitHeightM, targetPlanet.mapData.showClosestApproachDistance);
			if (flag2)
			{
				return;
			}
			bool flag3 = orbit.apoapsis < targetPlanet.orbitData.orbitHeightM;
			if (flag3)
			{
				list.Add(3.1415927410125732);
			}
			else
			{
				list.Add(0.0);
			}
		}
		double num = double.PositiveInfinity;
		Double3 posA = Double3.zero;
		Double3 posB = Double3.zero;
		for (int i = 0; i < list.Count; i++)
		{
			double nextTrueAnomalyPassageTime = orbit.GetNextTrueAnomalyPassageTime(Ref.controller.globalTime, list[i]);
			bool flag4 = nextTrueAnomalyPassageTime >= Ref.controller.globalTime;
			if (flag4)
			{
				Double3 position = Kepler.GetPosition(Kepler.GetRadius(orbit.semiLatusRectum, orbit.eccentricity, list[i]), list[i], orbit.argumentOfPeriapsis);
				Double3 posOut = targetPlanet.GetPosOut(nextTrueAnomalyPassageTime);
				double sqrMagnitude2d = (position - posOut).sqrMagnitude2d;
				bool flag5 = sqrMagnitude2d <= num;
				if (flag5)
				{
					num = sqrMagnitude2d;
					posA = position;
					posB = posOut;
				}
			}
		}
		bool flag6 = list.Count > 0;
		if (flag6)
		{
			ClosestApproach.SetLine(closestApproachLine, posA, posB, Ref.map.mapRefs[targetPlanet.parentBody].holder);
			drawn = true;
		}
	}

	private static void CalculateClosestApproach(LineRenderer closestApproachLine, Orbit orbitA, Orbit orbitB, double time, ref bool drawn)
	{
		bool flag = orbitA.orbitType != Orbit.Type.Eternal || orbitB.orbitType > Orbit.Type.Eternal;
		if (!flag)
		{
			List<double> list = new List<double>();
			bool flag2 = orbitA._period > orbitB._period;
			if (flag2)
			{
				list.Add(ClosestApproach.GetIntersectTrueAnomalyGlobal(orbitA, orbitB, time, 15) - orbitA.argumentOfPeriapsis);
			}
			else
			{
				list.Add(ClosestApproach.GetIntersectTrueAnomalyGlobal(orbitB, orbitA, time, 15) - orbitA.argumentOfPeriapsis);
			}
			double num = double.PositiveInfinity;
			Double3 posA = Double3.zero;
			Double3 posB = Double3.zero;
			for (int i = 0; i < list.Count; i++)
			{
				double nextTrueAnomalyPassageTime = orbitA.GetNextTrueAnomalyPassageTime(Ref.controller.globalTime, list[i]);
				bool flag3 = nextTrueAnomalyPassageTime >= Ref.controller.globalTime;
				if (flag3)
				{
					Double3 position = Kepler.GetPosition(Kepler.GetRadius(orbitA.semiLatusRectum, orbitA.eccentricity, list[i]), list[i], orbitA.argumentOfPeriapsis);
					Double3 posOut = orbitB.GetPosOut(nextTrueAnomalyPassageTime);
					double sqrMagnitude2d = (position - posOut).sqrMagnitude2d;
					bool flag4 = sqrMagnitude2d <= num;
					if (flag4)
					{
						num = sqrMagnitude2d;
						posA = position;
						posB = posOut;
					}
				}
			}
			bool flag5 = list.Count > 0;
			if (flag5)
			{
				ClosestApproach.SetLine(closestApproachLine, posA, posB, Ref.map.mapRefs[orbitA.planet].holder);
				drawn = true;
			}
		}
	}

	private static double GetIntersectTrueAnomalyGlobal(Orbit orbitA, Orbit orbitB, double time, int maxIterations)
	{
		double num = 0.0;
		double num2 = double.PositiveInfinity;
		double trueAnomalyOut = orbitA.GetTrueAnomalyOut(time);
		for (int i = 0; i < 8; i++)
		{
			double distanceSqrtAtHeight = ClosestApproach.GetDistanceSqrtAtHeight(orbitA, orbitB, time, trueAnomalyOut + (double)i * 3.1415926535897931 / 4.0 + 0.39269908169872414);
			bool flag = distanceSqrtAtHeight < num2;
			if (flag)
			{
				num = trueAnomalyOut + (double)i * 3.1415926535897931 / 4.0 + 0.39269908169872414;
				num2 = distanceSqrtAtHeight;
			}
		}
		double num3 = 0.78539816339744828;
		double num4 = double.PositiveInfinity;
		for (int j = 0; j < maxIterations; j++)
		{
			num3 /= 2.0;
			double num5 = num4;
			double distanceSqrtAtHeight2 = ClosestApproach.GetDistanceSqrtAtHeight(orbitA, orbitB, time, num + num3);
			double distanceSqrtAtHeight3 = ClosestApproach.GetDistanceSqrtAtHeight(orbitA, orbitB, time, num - num3);
			bool flag2 = distanceSqrtAtHeight2 < distanceSqrtAtHeight3;
			double num6;
			if (flag2)
			{
				num6 = num + num3;
				num4 = distanceSqrtAtHeight2;
			}
			else
			{
				num6 = num - num3;
				num4 = distanceSqrtAtHeight3;
			}
			Debug.Log(num5 - num4);
			num = num6;
		}
		return num;
	}

	private static double GetDistanceSqrtAtHeight(Orbit orbitA, Orbit orbitB, double time, double globalTrueAnomaly)
	{
		double nextTrueAnomalyPassageTime = orbitA.GetNextTrueAnomalyPassageTime(time, globalTrueAnomaly - orbitA.argumentOfPeriapsis);
		Double3 position = Kepler.GetPosition(Kepler.GetRadius(orbitA.semiLatusRectum, orbitA.eccentricity, globalTrueAnomaly - orbitA.argumentOfPeriapsis), globalTrueAnomaly, 0.0);
		Double3 posOut = orbitB.GetPosOut(nextTrueAnomalyPassageTime);
		Debug.DrawLine((position / 10000.0).toVector3, (posOut / 10000.0).toVector3, Color.green);
		return (position - posOut).sqrMagnitude2d;
	}

	private static void SetLine(LineRenderer closestApproachLine, Double3 posA, Double3 posB, Transform markerParent)
	{
		closestApproachLine.SetPositions(new Vector3[]
		{
			(posA / 10000.0).toVector3,
			(posB / 10000.0).toVector3
		});
		Double3 @double = (posA - posB) / 10000.0;
		closestApproachLine.sharedMaterial.mainTextureScale = new Vector2(Mathf.Max(1.6f, (float)(@double.magnitude2d / (double)(-(double)((float)Ref.map.mapPosition.z)) * 80.0) + 0.6f), 1f);
		bool flag = closestApproachLine.transform.parent != markerParent;
		if (flag)
		{
			closestApproachLine.transform.parent = markerParent;
			closestApproachLine.transform.localPosition = Vector3.zero;
		}
	}
}
