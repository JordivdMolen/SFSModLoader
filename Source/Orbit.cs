using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Orbit
{
	public Orbit(Double3 posIn, Double3 velIn, double timeIn, CelestialBodyData planet, CelestialBodyData lastPlanet)
	{
		this.planet = planet;
		this.timeIn = timeIn;
		double mass = planet.mass;
		Double3 @double = Double3.Cross2d(posIn, velIn);
		Double3 double2 = Double3.Cross(velIn, @double) / mass - posIn.normalized2d;
		this.eccentricity = double2.magnitude2d;
		this.argumentOfPeriapsis = Math.Atan2(double2.y, double2.x);
		this.semiMajorAxis = -mass / (2.0 * (Math.Pow(velIn.magnitude2d, 2.0) / 2.0 - mass / posIn.magnitude2d));
		this.periapsis = this.semiMajorAxis * (1.0 - this.eccentricity);
		this.apoapsis = ((this.eccentricity >= 1.0) ? double.PositiveInfinity : (this.semiMajorAxis * (1.0 + this.eccentricity)));
		this.semiLatusRectum = Kepler.GetSemiLatusRectum(this.periapsis, this.eccentricity);
		this._period = Kepler.GetPeriod(this.eccentricity, this.semiMajorAxis, mass);
		this.meanMotion = Kepler.GetMeanMotion(this._period, this.eccentricity, mass, this.semiMajorAxis) * (double)Math.Sign(@double.z);
		double trueAnomalyAtRadius = Kepler.GetTrueAnomalyAtRadius(posIn.magnitude2d, this.semiLatusRectum, this.eccentricity);
		double num = Kepler.GetMeanAnomaly(this.eccentricity, trueAnomalyAtRadius, posIn, this.argumentOfPeriapsis) / this.meanMotion;
		bool flag = this.apoapsis > planet.orbitData.SOI || this.eccentricity >= 1.0;
		if (flag)
		{
			this._period = 0.0;
		}
		this.periapsisPassageTime = timeIn + num - this._period * 10.0;
		this.GetOrbitType(timeIn, lastPlanet);
		this.stopTimeWarpTime = this.GetStopTimeWarpTime();
	}

	public Orbit(Double3 posIn, Double3 velIn, CelestialBodyData planet)
	{
		this.planet = planet;
		double mass = planet.mass;
		Double3 @double = Double3.Cross(posIn, velIn);
		Double3 double2 = Double3.Cross(velIn, @double) / mass - posIn.normalized2d;
		this.eccentricity = double2.magnitude2d;
		this.argumentOfPeriapsis = Math.Atan2(double2.y, double2.x);
		this.semiMajorAxis = -mass / (2.0 * (Math.Pow(velIn.magnitude2d, 2.0) / 2.0 - mass / posIn.magnitude2d));
		this.periapsis = this.semiMajorAxis * (1.0 - this.eccentricity);
		this.apoapsis = ((this.eccentricity >= 1.0) ? double.PositiveInfinity : (this.semiMajorAxis * (1.0 + this.eccentricity)));
		bool flag = this.apoapsis > planet.orbitData.SOI || this.eccentricity >= 1.0;
		if (flag)
		{
			this.orbitType = Orbit.Type.Escape;
		}
		else
		{
			this.orbitType = Orbit.Type.Eternal;
		}
		this.meanMotion = @double.z;
		bool flag2 = double.IsNaN(this.meanMotion);
		if (flag2)
		{
			Debug.Log("mean motion is nan 1");
		}
	}

	public void GetOrbitType(double timeIn, CelestialBodyData lastPlanet)
	{
		this.orbitType = Orbit.Type.Eternal;
		this.orbitEndTime = double.PositiveInfinity;
		this.calculatePassesTime = double.PositiveInfinity;
		bool flag = this.apoapsis > this.planet.orbitData.SOI || this.eccentricity > 1.0;
		if (flag)
		{
			this.orbitEndTime = this.periapsisPassageTime + this.GetPassAnomaly(this.planet.orbitData.SOI);
			bool flag2 = !double.IsNaN(this.meanMotion);
			if (flag2)
			{
				this.endTrueAnomaly = Kepler.GetTrueAnomalyAtRadius(this.planet.orbitData.SOI, this.semiLatusRectum, this.eccentricity) * (double)Math.Sign(this.meanMotion);
			}
			this.orbitType = Orbit.Type.Escape;
			this.nextPlanet = this.planet.parentBody;
		}
		double cutEndTime = (this.orbitType != Orbit.Type.Escape) ? (timeIn + this._period * 0.995) : double.PositiveInfinity;
		List<Orbit.Pass> list = new List<Orbit.Pass>();
		foreach (CelestialBodyData celestialBodyData in this.planet.satellites)
		{
			bool flag3 = this.CanPasSOI(celestialBodyData.orbitData.orbitHeightM, celestialBodyData.orbitData.SOI);
			if (flag3)
			{
				double cutStartTime = timeIn + ((!(celestialBodyData != lastPlanet)) ? ((this.orbitType != Orbit.Type.Escape) ? (this._period * 0.05) : double.PositiveInfinity) : 0.0);
				list.AddRange(this.CreatePasses(this.orbitType == Orbit.Type.Escape, cutStartTime, cutEndTime, celestialBodyData));
				bool flag4 = this.orbitType == Orbit.Type.Eternal;
				if (flag4)
				{
					this.calculatePassesTime = cutEndTime;
				}
			}
		}
		this.ProcessPasses(this.SortPasses(list));
	}

	public bool UpdatePass()
	{
		List<Orbit.Pass> list = new List<Orbit.Pass>();
		foreach (CelestialBodyData celestialBodyData in this.planet.satellites)
		{
			bool flag = this.CanPasSOI(celestialBodyData.orbitData.orbitHeightM, celestialBodyData.orbitData.SOI);
			if (flag)
			{
				list.AddRange(this.CreatePasses(false, this.calculatePassesTime, Ref.controller.globalTime + this._period * 0.995, celestialBodyData));
			}
		}
		this.calculatePassesTime = Ref.controller.globalTime + this._period * 0.995;
		return this.ProcessPasses(this.SortPasses(list));
	}

	private List<Orbit.Pass> CreatePasses(bool isEscape, double cutStartTime, double cutEndTime, CelestialBodyData satellite)
	{
		double num = satellite.orbitData.orbitHeightM + satellite.orbitData.SOI;
		double num2 = satellite.orbitData.orbitHeightM - satellite.orbitData.SOI;
		double num3 = (!isEscape) ? (this.periapsisPassageTime - this._period + (double)((int)((cutStartTime - (this.periapsisPassageTime - this._period)) / this._period)) * this._period) : this.periapsisPassageTime;
		bool flag = this.periapsis < num2 && this.apoapsis > num;
		List<Orbit.Pass> result;
		if (flag)
		{
			double passAnomaly = this.GetPassAnomaly(num);
			double passAnomaly2 = this.GetPassAnomaly(num2);
			List<Orbit.Pass> list = this.CreatePass(num3 - passAnomaly + this._period, num3 - passAnomaly2 + this._period, cutStartTime, cutEndTime, isEscape, satellite);
			list.AddRange(this.CreatePass(num3 + passAnomaly2, num3 + passAnomaly, cutStartTime, cutEndTime, isEscape, satellite));
			result = list;
		}
		else
		{
			bool flag2 = this.periapsis <= num2 || this.periapsis >= num || this.apoapsis <= num;
			if (flag2)
			{
				bool flag3 = !isEscape;
				if (flag3)
				{
					bool flag4 = this.apoapsis > num2 && this.apoapsis < num && this.periapsis < num2;
					if (flag4)
					{
						double passAnomaly3 = this.GetPassAnomaly(num2);
						return this.CreatePass(num3 + passAnomaly3, num3 + this._period - passAnomaly3, cutStartTime, cutEndTime, false, satellite);
					}
					bool flag5 = this.apoapsis < num && this.periapsis > num2;
					if (flag5)
					{
						return new List<Orbit.Pass>
						{
							new Orbit.Pass(cutStartTime, cutEndTime, satellite)
						};
					}
				}
				result = new List<Orbit.Pass>();
			}
			else
			{
				double passAnomaly4 = this.GetPassAnomaly(num);
				bool flag6 = num3 + this._period * 0.5 > cutStartTime;
				if (flag6)
				{
					result = this.CreatePass(num3 - passAnomaly4, num3 + passAnomaly4, cutStartTime, cutEndTime, isEscape, satellite);
				}
				else
				{
					result = this.CreatePass(num3 - passAnomaly4 + this._period, num3 + passAnomaly4 + this._period, cutStartTime, cutEndTime, isEscape, satellite);
				}
			}
		}
		return result;
	}

	private List<Orbit.Pass> CreatePass(double passStartTime, double passEndTime, double cutStartTime, double cutEndTime, bool isEscape, CelestialBodyData satellite)
	{
		List<Orbit.Pass> list = new List<Orbit.Pass>();
		bool flag = cutStartTime < passEndTime && cutEndTime > passStartTime;
		if (flag)
		{
			list.Add(new Orbit.Pass((cutStartTime >= passStartTime) ? cutStartTime : passStartTime, (cutEndTime <= passEndTime) ? cutEndTime : passEndTime, satellite));
		}
		bool flag2 = !isEscape && cutStartTime < passEndTime + this._period && cutEndTime > passStartTime + this._period;
		if (flag2)
		{
			list.Add(new Orbit.Pass((cutStartTime >= passStartTime + this._period) ? cutStartTime : (passStartTime + this._period), (cutEndTime <= passEndTime + this._period) ? cutEndTime : (passEndTime + this._period), satellite));
		}
		return list;
	}

	private bool ProcessPasses(List<Orbit.Pass> passes)
	{
		for (int i = 0; i < passes.Count; i++)
		{
			double num = this.ProcessPasss(passes[i]);
			bool flag = num > 0.0;
			if (flag)
			{
				this.orbitEndTime = num;
				this.endTrueAnomaly = this.GetTrueAnomalyOut(num);
				this.orbitType = Orbit.Type.Encounter;
				this.nextPlanet = passes[i].passPlanet;
				this.calculatePassesTime = double.PositiveInfinity;
				return true;
			}
			bool flag2 = num < 0.0 && this.calculatePassesTime != double.PositiveInfinity;
			if (flag2)
			{
				this.calculatePassesTime = -num;
			}
		}
		return false;
	}

	private double ProcessPasss(Orbit.Pass pass)
	{
		double num = pass.passPlanet.orbitData.orbitHeightM - pass.passPlanet.orbitData.SOI;
		double num2 = this.planet.mass / (num * num) * 2.0;
		int num3 = 0;
		double num4 = pass.startTime;
		while (num4 < pass.endTime)
		{
			Double3 @double = pass.passPlanet.GetPosOut(num4) - this.GetPosOut(num4);
			double num5 = @double.magnitude2d - pass.passPlanet.orbitData.SOI;
			Debug.DrawLine((this.GetPosOut(num4) / 10000.0).toVector3, (pass.passPlanet.GetPosOut(num4) / 10000.0).toVector3, (num5 >= 5.0) ? Color.green : Color.red);
			bool flag = num5 < 5.0;
			double result;
			if (flag)
			{
				result = num4;
			}
			else
			{
				Double3 double2 = pass.passPlanet.GetVelOut(num4) - this.GetVelOut(num4);
				double num6 = Math.Atan2(@double.x, @double.y) + 1.5707963267948966;
				double num7 = double2.x * Math.Cos(num6) - double2.y * Math.Sin(num6);
				double num8 = num7 * num7 / (num2 * 2.0);
				double num9 = Math.Sqrt(2.0 * (num5 + num8) / num2);
				double num10 = num7 / num2;
				num4 += num9 - num10;
				num3++;
				bool flag2 = num3 > 50;
				if (!flag2)
				{
					continue;
				}
				Debug.Log("Could not calculate till the end");
				result = -num4;
			}
			return result;
		}
		return 0.0;
	}

	private List<Orbit.Pass> SortPasses(List<Orbit.Pass> passes)
	{
		List<Orbit.Pass> list = new List<Orbit.Pass>();
		double num = 0.0;
		for (int i = 0; i < passes.Count; i++)
		{
			Orbit.Pass pass = null;
			foreach (Orbit.Pass pass2 in passes)
			{
				bool flag = pass2.startTime > num;
				if (flag)
				{
					bool flag2 = pass != null;
					if (flag2)
					{
						bool flag3 = pass2.startTime < pass.startTime;
						if (flag3)
						{
							pass = pass2;
						}
					}
					else
					{
						pass = pass2;
					}
				}
			}
			num = pass.startTime;
			list.Add(pass);
		}
		return list;
	}

	public double GetPassAnomaly(double height)
	{
		double trueAnomalyAtRadius = Kepler.GetTrueAnomalyAtRadius(height, this.semiLatusRectum, this.eccentricity);
		double meanAnomaly = Kepler.GetMeanAnomaly(this.eccentricity, trueAnomalyAtRadius);
		return Math.Abs(meanAnomaly / this.meanMotion);
	}

	public bool CanPasSOI(double orbitHeight, double SOI)
	{
		return this.periapsis < orbitHeight + SOI && this.apoapsis > orbitHeight - SOI;
	}

	public double GetStopTimeWarpTime()
	{
		double num = this.planet.radius + this.planet.minTimewarpHeightKm * 1000.0;
		bool flag = this.periapsis + 10.0 < num;
		double result;
		if (flag)
		{
			double trueAnomalyAtRadius = Kepler.GetTrueAnomalyAtRadius(num, this.semiLatusRectum, this.eccentricity);
			double num2 = this.GetNextTrueAnomalyPassageTime(this.timeIn, trueAnomalyAtRadius);
			double num3 = this.GetNextTrueAnomalyPassageTime(this.timeIn, -trueAnomalyAtRadius);
			bool flag2 = num2 < this.timeIn;
			if (flag2)
			{
				num2 += this._period;
			}
			bool flag3 = num3 < this.timeIn;
			if (flag3)
			{
				num3 += this._period;
			}
			bool flag4 = num2 < this.timeIn;
			if (flag4)
			{
				num2 = double.PositiveInfinity;
			}
			bool flag5 = num3 < this.timeIn;
			if (flag5)
			{
				num3 = double.PositiveInfinity;
			}
			result = Math.Min(num2, num3);
		}
		else
		{
			result = double.PositiveInfinity;
		}
		return result;
	}

	public static List<Orbit> CalculateOrbits(Double3 posIn, Double3 velIn, CelestialBodyData initialPlanet)
	{
		List<Orbit> list = new List<Orbit>();
		bool flag = double.IsNaN(posIn.x * velIn.y - posIn.y * velIn.x);
		List<Orbit> result;
		if (flag)
		{
			result = list;
		}
		else
		{
			list.Add(new Orbit(posIn, velIn, Ref.controller.globalTime, initialPlanet, null));
			result = Orbit.CalculateOrbits(list);
		}
		return result;
	}

	public static List<Orbit> CalculateOrbits(List<Orbit> orbitsIn)
	{
		while (orbitsIn.Count < 3)
		{
			Orbit orbit = orbitsIn[orbitsIn.Count - 1];
			bool flag = orbit.orbitType == Orbit.Type.Escape;
			if (flag)
			{
				bool flag2 = orbitsIn.Count >= 2 && orbitsIn[orbitsIn.Count - 2].orbitType == Orbit.Type.Encounter;
				if (flag2)
				{
					return orbitsIn;
				}
				Double3 position = Kepler.GetPosition(orbit.planet.orbitData.SOI, orbit.endTrueAnomaly, orbit.argumentOfPeriapsis);
				Double3 velocity = Kepler.GetVelocity(orbit.semiMajorAxis, orbit.planet.orbitData.SOI, orbit.meanMotion, Kepler.GetEccentricAnomalyFromTrueAnomaly(orbit.endTrueAnomaly, orbit.eccentricity), orbit.endTrueAnomaly, orbit.eccentricity, orbit.argumentOfPeriapsis);
				Double3 @double = position + orbit.planet.GetPosOut(orbit.orbitEndTime);
				Double3 double2 = velocity + orbit.planet.GetVelOut(orbit.orbitEndTime);
				bool flag3 = double.IsNaN(@double.x * double2.y - @double.y * double2.x);
				if (flag3)
				{
					return orbitsIn;
				}
				orbitsIn.Add(new Orbit(@double, double2, orbit.orbitEndTime, orbit.nextPlanet, orbit.planet));
			}
			else
			{
				bool flag4 = orbit.orbitType != Orbit.Type.Encounter;
				if (flag4)
				{
					return orbitsIn;
				}
				bool flag5 = orbitsIn.Count >= 2 && orbitsIn[orbitsIn.Count - 2].planet == orbit.nextPlanet;
				if (flag5)
				{
					return orbitsIn;
				}
				double eccentricAnomalyFromTrueAnomaly = Kepler.GetEccentricAnomalyFromTrueAnomaly(orbit.endTrueAnomaly, orbit.eccentricity);
				double radius = Kepler.GetRadius(orbit.semiLatusRectum, orbit.eccentricity, orbit.endTrueAnomaly);
				Double3 position2 = Kepler.GetPosition(radius, orbit.endTrueAnomaly, orbit.argumentOfPeriapsis);
				Double3 velocity2 = Kepler.GetVelocity(orbit.semiMajorAxis, radius, orbit.meanMotion, eccentricAnomalyFromTrueAnomaly, orbit.endTrueAnomaly, orbit.eccentricity, orbit.argumentOfPeriapsis);
				Double3 double3 = position2 - orbit.nextPlanet.GetPosOut(orbit.orbitEndTime);
				Double3 double4 = velocity2 - orbit.nextPlanet.GetVelOut(orbit.orbitEndTime);
				bool flag6 = double.IsNaN(double3.x * double4.y - double3.y * double4.x);
				if (flag6)
				{
					return orbitsIn;
				}
				orbitsIn.Add(new Orbit(double3, double4, orbit.orbitEndTime, orbit.nextPlanet, null));
			}
		}
		return orbitsIn;
	}

	public Double3 GetVelOut(double newTime)
	{
		this.UpdateDynamicVariables(newTime);
		return this.velOut;
	}

	public Double3 GetPosOut(double newTime)
	{
		this.UpdateDynamicVariables(newTime);
		return this.posOut;
	}

	public double GetTrueAnomalyOut(double newTime)
	{
		this.UpdateDynamicVariables(newTime);
		return this.trueAnomalyOut;
	}

	public double GetEccentricAnomalyOut(double newTime)
	{
		this.UpdateDynamicVariables(newTime);
		return this.eccentricAnomnalyOut;
	}

	private void UpdateDynamicVariables(double newTime)
	{
		bool flag = this.lastCalculationTime != newTime;
		if (flag)
		{
			this.lastCalculationTime = newTime;
			double eccentricAnomaly = Kepler.GetEccentricAnomaly((newTime - this.periapsisPassageTime) * this.meanMotion, this.eccentricity);
			bool flag2 = double.IsNaN(eccentricAnomaly);
			if (!flag2)
			{
				this.eccentricAnomnalyOut = eccentricAnomaly;
				this.trueAnomalyOut = Kepler.GetTrueAnomaly(this.eccentricAnomnalyOut, this.eccentricity);
				double radius = Kepler.GetRadius(this.semiLatusRectum, this.eccentricity, this.trueAnomalyOut);
				this.posOut = Kepler.GetPosition(radius, this.trueAnomalyOut, this.argumentOfPeriapsis);
				this.velOut = Kepler.GetVelocity(this.semiMajorAxis, radius, this.meanMotion, this.eccentricAnomnalyOut, this.trueAnomalyOut, this.eccentricity, this.argumentOfPeriapsis);
			}
		}
	}

	public double GetNextTrueAnomalyPassageTime(double referenceTime, double trueAnomaly)
	{
		return this.GetLastTrueAnomalyPassageTime(referenceTime, trueAnomaly) + this._period;
	}

	public double GetLastTrueAnomalyPassageTime(double referenceTime, double trueAnomaly)
	{
		double num = this.periapsisPassageTime + Kepler.GetMeanAnomaly(this.eccentricity, trueAnomaly) / this.meanMotion;
		bool flag = this.orbitType != Orbit.Type.Escape;
		double result;
		if (flag)
		{
			result = num + (double)((int)((referenceTime - num) / this._period)) * this._period;
		}
		else
		{
			result = num;
		}
		return result;
	}

	public Vector3[] GenerateOrbitLinePoints(double fromTrueAnomaly, double toTrueAnomaly, int resolution)
	{
		Vector3[] array = new Vector3[resolution + 1];
		double num = (toTrueAnomaly - fromTrueAnomaly) % 6.2831853071795862 / (double)resolution;
		for (int i = 0; i < resolution + 1; i++)
		{
			double v = fromTrueAnomaly + num * (double)i;
			double radius = Kepler.GetRadius(this.semiLatusRectum, this.eccentricity, v);
			array[i] = Kepler.GetPosition(radius, v, this.argumentOfPeriapsis).toVector3;
		}
		return array;
	}

	[Header(" Static properties:")]
	public CelestialBodyData planet;

	[Header(" Static properties:")]
	public CelestialBodyData nextPlanet;

	public Orbit.Type orbitType;

	public double timeIn;

	public double orbitEndTime;

	public double endTrueAnomaly;

	public double stopTimeWarpTime;

	public double calculatePassesTime;

	[HideInInspector]
	public double semiMajorAxis;

	[HideInInspector]
	public double semiLatusRectum;

	[HideInInspector]
	public double argumentOfPeriapsis;

	public double meanMotion;

	public double periapsis;

	public double apoapsis;

	public double eccentricity;

	public double _period;

	public double periapsisPassageTime;

	[Header(" Dynamic properties:")]
	private double trueAnomalyOut;

	[Header(" Dynamic properties:")]
	private double eccentricAnomnalyOut;

	private Double3 posOut;

	private Double3 velOut;

	private double lastCalculationTime = -1.0;

	[Serializable]
	public class Pass
	{
		public Pass(double startTime, double endTime, CelestialBodyData passPlanet)
		{
			this.startTime = startTime;
			this.endTime = endTime;
			this.passPlanet = passPlanet;
		}

		public double startTime;

		public double endTime;

		public CelestialBodyData passPlanet;
	}

	public enum Type
	{
		Eternal,
		Encounter,
		Escape
	}
}
