using System;
using System.Collections.Generic;
using UnityEngine;

public static class ClosestApproach
{
	public static void DrawClosestApproachPlanet(LineRenderer closestApproachLine, List<Orbit> orbits, CelestialBodyData targetPlanet, ref bool drawn)
	{
		for (int i = 0; i < orbits.Count; i++)
		{
			if (orbits[i].orbitType != Orbit.Type.Encounter)
			{
				if (orbits[i].planet == targetPlanet)
				{
					ClosestApproach.SetLine(closestApproachLine, Kepler.GetPosition(orbits[i].periapsis, 0.0, orbits[i].argumentOfPeriapsis), Double3.zero, Ref.map.mapRefs[targetPlanet].holder);
					drawn = true;
					return;
				}
				if (orbits[i].planet == targetPlanet.parentBody)
				{
					ClosestApproach.CalculateClosestApproach(closestApproachLine, orbits[i], targetPlanet, ref drawn);
					if (drawn)
					{
						return;
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
				if (!(orbits[i].planet != targetOrbits[j].planet))
				{
					ClosestApproach.CalculateClosestApproach(closestApproachLine, orbits[i], targetOrbits[j], Ref.controller.globalTime, ref drawn);
					if (drawn)
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
		if (orbit.periapsis < targetPlanet.orbitData.orbitHeightM && orbit.apoapsis > targetPlanet.orbitData.orbitHeightM)
		{
			double trueAnomalyAtRadius = Kepler.GetTrueAnomalyAtRadius(targetPlanet.orbitData.orbitHeightM, orbit.semiLatusRectum, orbit.eccentricity);
			list.Add(trueAnomalyAtRadius);
			list.Add(-trueAnomalyAtRadius);
		}
		else
		{
			if (!orbit.CanPasSOI(targetPlanet.orbitData.orbitHeightM, targetPlanet.mapData.showClosestApproachDistance))
			{
				return;
			}
			if (orbit.apoapsis < targetPlanet.orbitData.orbitHeightM)
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
			if (nextTrueAnomalyPassageTime >= Ref.controller.globalTime)
			{
				Double3 position = Kepler.GetPosition(Kepler.GetRadius(orbit.semiLatusRectum, orbit.eccentricity, list[i]), list[i], orbit.argumentOfPeriapsis);
				Double3 posOut = targetPlanet.GetPosOut(nextTrueAnomalyPassageTime);
				double sqrMagnitude2d = (position - posOut).sqrMagnitude2d;
				if (sqrMagnitude2d <= num)
				{
					num = sqrMagnitude2d;
					posA = position;
					posB = posOut;
				}
			}
		}
		if (list.Count > 0)
		{
			ClosestApproach.SetLine(closestApproachLine, posA, posB, Ref.map.mapRefs[targetPlanet.parentBody].holder);
			drawn = true;
		}
	}

	private static void CalculateClosestApproach(LineRenderer closestApproachLine, Orbit orbitA, Orbit orbitB, double time, ref bool drawn)
	{
		if (orbitA.orbitType != Orbit.Type.Eternal || orbitB.orbitType != Orbit.Type.Eternal)
		{
			return;
		}
		List<double> list = new List<double>();
		if (orbitA._period > orbitB._period)
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
			if (nextTrueAnomalyPassageTime >= Ref.controller.globalTime)
			{
				Double3 position = Kepler.GetPosition(Kepler.GetRadius(orbitA.semiLatusRectum, orbitA.eccentricity, list[i]), list[i], orbitA.argumentOfPeriapsis);
				Double3 posOut = orbitB.GetPosOut(nextTrueAnomalyPassageTime);
				double sqrMagnitude2d = (position - posOut).sqrMagnitude2d;
				if (sqrMagnitude2d <= num)
				{
					num = sqrMagnitude2d;
					posA = position;
					posB = posOut;
				}
			}
		}
		if (list.Count > 0)
		{
			ClosestApproach.SetLine(closestApproachLine, posA, posB, Ref.map.mapRefs[orbitA.planet].holder);
			drawn = true;
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
			if (distanceSqrtAtHeight < num2)
			{
				num = trueAnomalyOut + (double)i * 3.1415926535897931 / 4.0 + 0.39269908169872414;
				num2 = distanceSqrtAtHeight;
			}
		}
		double num3 = 0.78539816339744828;
		for (int j = 0; j < maxIterations; j++)
		{
			num3 /= 2.0;
			double distanceSqrtAtHeight2 = ClosestApproach.GetDistanceSqrtAtHeight(orbitA, orbitB, time, num + num3);
			double distanceSqrtAtHeight3 = ClosestApproach.GetDistanceSqrtAtHeight(orbitA, orbitB, time, num - num3);
			double num4;
			if (distanceSqrtAtHeight2 < distanceSqrtAtHeight3)
			{
				num4 = num + num3;
			}
			else
			{
				num4 = num - num3;
			}
			num = num4;
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
		closestApproachLine.sharedMaterial.mainTextureScale = new Vector2(Mathf.Max(1.6f, (float)(@double.magnitude2d / -(float)Ref.map.mapPosition.z * 80.0) + 0.6f), 1f);
		if (closestApproachLine.transform.parent != markerParent)
		{
			closestApproachLine.transform.parent = markerParent;
			closestApproachLine.transform.localPosition = Vector3.zero;
		}
	}
}
