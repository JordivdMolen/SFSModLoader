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
		Double3 b = Double3.Cross2d(posIn, velIn);
		Double3 @double = Double3.Cross(velIn, b) / mass - posIn.normalized2d;
		this.eccentricity = @double.magnitude2d;
		this.argumentOfPeriapsis = Math.Atan2(@double.y, @double.x);
		this.semiMajorAxis = -mass / (2.0 * (Math.Pow(velIn.magnitude2d, 2.0) / 2.0 - mass / posIn.magnitude2d));
		this.periapsis = this.semiMajorAxis * (1.0 - this.eccentricity);
		this.apoapsis = ((this.eccentricity >= 1.0) ? double.PositiveInfinity : (this.semiMajorAxis * (1.0 + this.eccentricity)));
		this.semiLatusRectum = Kepler.GetSemiLatusRectum(this.periapsis, this.eccentricity);
		this._period = Kepler.GetPeriod(this.eccentricity, this.semiMajorAxis, mass);
		this.meanMotion = Kepler.GetMeanMotion(this._period, this.eccentricity, mass, this.semiMajorAxis) * (double)Math.Sign(b.z);
		double trueAnomalyAtRadius = Kepler.GetTrueAnomalyAtRadius(posIn.magnitude2d, this.semiLatusRectum, this.eccentricity);
		double num = Kepler.GetMeanAnomaly(this.eccentricity, trueAnomalyAtRadius, posIn, this.argumentOfPeriapsis) / this.meanMotion;
		if (this.apoapsis > planet.orbitData.SOI || this.eccentricity >= 1.0)
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
		Double3 b = Double3.Cross(posIn, velIn);
		Double3 @double = Double3.Cross(velIn, b) / mass - posIn.normalized2d;
		this.eccentricity = @double.magnitude2d;
		this.argumentOfPeriapsis = Math.Atan2(@double.y, @double.x);
		this.semiMajorAxis = -mass / (2.0 * (Math.Pow(velIn.magnitude2d, 2.0) / 2.0 - mass / posIn.magnitude2d));
		this.periapsis = this.semiMajorAxis * (1.0 - this.eccentricity);
		this.apoapsis = ((this.eccentricity >= 1.0) ? double.PositiveInfinity : (this.semiMajorAxis * (1.0 + this.eccentricity)));
		if (this.apoapsis > planet.orbitData.SOI || this.eccentricity >= 1.0)
		{
			this.orbitType = Orbit.Type.Escape;
		}
		else
		{
			this.orbitType = Orbit.Type.Eternal;
		}
		this.meanMotion = b.z;
		if (double.IsNaN(this.meanMotion))
		{
			Debug.Log("mean motion is nan 1");
		}
	}

	public void GetOrbitType(double timeIn, CelestialBodyData lastPlanet)
	{
		this.orbitType = Orbit.Type.Eternal;
		this.orbitEndTime = double.PositiveInfinity;
		this.calculatePassesTime = double.PositiveInfinity;
		if (this.apoapsis > this.planet.orbitData.SOI || this.eccentricity > 1.0)
		{
			this.orbitEndTime = this.periapsisPassageTime + this.GetPassAnomaly(this.planet.orbitData.SOI);
			if (!double.IsNaN(this.meanMotion))
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
			if (this.CanPasSOI(celestialBodyData.orbitData.orbitHeightM, celestialBodyData.orbitData.SOI))
			{
				double cutStartTime = timeIn + ((!(celestialBodyData != lastPlanet)) ? ((this.orbitType != Orbit.Type.Escape) ? (this._period * 0.05) : double.PositiveInfinity) : 0.0);
				list.AddRange(this.CreatePasses(this.orbitType == Orbit.Type.Escape, cutStartTime, cutEndTime, celestialBodyData));
				if (this.orbitType == Orbit.Type.Eternal)
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
			if (this.CanPasSOI(celestialBodyData.orbitData.orbitHeightM, celestialBodyData.orbitData.SOI))
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
		if (this.periapsis < num2 && this.apoapsis > num)
		{
			double passAnomaly = this.GetPassAnomaly(num);
			double passAnomaly2 = this.GetPassAnomaly(num2);
			List<Orbit.Pass> list = this.CreatePass(num3 - passAnomaly + this._period, num3 - passAnomaly2 + this._period, cutStartTime, cutEndTime, isEscape, satellite);
			list.AddRange(this.CreatePass(num3 + passAnomaly2, num3 + passAnomaly, cutStartTime, cutEndTime, isEscape, satellite));
			return list;
		}
		if (this.periapsis <= num2 || this.periapsis >= num || this.apoapsis <= num)
		{
			if (!isEscape)
			{
				if (this.apoapsis > num2 && this.apoapsis < num && this.periapsis < num2)
				{
					double passAnomaly3 = this.GetPassAnomaly(num2);
					return this.CreatePass(num3 + passAnomaly3, num3 + this._period - passAnomaly3, cutStartTime, cutEndTime, false, satellite);
				}
				if (this.apoapsis < num && this.periapsis > num2)
				{
					return new List<Orbit.Pass>
					{
						new Orbit.Pass(cutStartTime, cutEndTime, satellite)
					};
				}
			}
			return new List<Orbit.Pass>();
		}
		double passAnomaly4 = this.GetPassAnomaly(num);
		if (num3 + this._period * 0.5 > cutStartTime)
		{
			return this.CreatePass(num3 - passAnomaly4, num3 + passAnomaly4, cutStartTime, cutEndTime, isEscape, satellite);
		}
		return this.CreatePass(num3 - passAnomaly4 + this._period, num3 + passAnomaly4 + this._period, cutStartTime, cutEndTime, isEscape, satellite);
	}

	private List<Orbit.Pass> CreatePass(double passStartTime, double passEndTime, double cutStartTime, double cutEndTime, bool isEscape, CelestialBodyData satellite)
	{
		List<Orbit.Pass> list = new List<Orbit.Pass>();
		if (cutStartTime < passEndTime && cutEndTime > passStartTime)
		{
			list.Add(new Orbit.Pass((cutStartTime >= passStartTime) ? cutStartTime : passStartTime, (cutEndTime <= passEndTime) ? cutEndTime : passEndTime, satellite));
		}
		if (!isEscape && cutStartTime < passEndTime + this._period && cutEndTime > passStartTime + this._period)
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
			if (num > 0.0)
			{
				this.orbitEndTime = num;
				this.endTrueAnomaly = this.GetTrueAnomalyOut(num);
				this.orbitType = Orbit.Type.Encounter;
				this.nextPlanet = passes[i].passPlanet;
				this.calculatePassesTime = double.PositiveInfinity;
				return true;
			}
			if (num < 0.0 && this.calculatePassesTime != double.PositiveInfinity)
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
			if (num5 < 5.0)
			{
				return num4;
			}
			Double3 double2 = pass.passPlanet.GetVelOut(num4) - this.GetVelOut(num4);
			double num6 = Math.Atan2(@double.x, @double.y) + 1.5707963267948966;
			double num7 = double2.x * Math.Cos(num6) - double2.y * Math.Sin(num6);
			double num8 = num7 * num7 / (num2 * 2.0);
			double num9 = Math.Sqrt(2.0 * (num5 + num8) / num2);
			double num10 = num7 / num2;
			num4 += num9 - num10;
			num3++;
			if (num3 > 50)
			{
				Debug.Log("Could not calculate till the end");
				return -num4;
			}
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
				if (pass2.startTime > num)
				{
					if (pass != null)
					{
						if (pass2.startTime < pass.startTime)
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
		if (this.periapsis + 10.0 < num)
		{
			double trueAnomalyAtRadius = Kepler.GetTrueAnomalyAtRadius(num, this.semiLatusRectum, this.eccentricity);
			double num2 = this.GetNextTrueAnomalyPassageTime(this.timeIn, trueAnomalyAtRadius);
			double num3 = this.GetNextTrueAnomalyPassageTime(this.timeIn, -trueAnomalyAtRadius);
			if (num2 < this.timeIn)
			{
				num2 += this._period;
			}
			if (num3 < this.timeIn)
			{
				num3 += this._period;
			}
			if (num2 < this.timeIn)
			{
				num2 = double.PositiveInfinity;
			}
			if (num3 < this.timeIn)
			{
				num3 = double.PositiveInfinity;
			}
			return Math.Min(num2, num3);
		}
		return double.PositiveInfinity;
	}

	public static List<Orbit> CalculateOrbits(Double3 posIn, Double3 velIn, CelestialBodyData initialPlanet)
	{
		List<Orbit> list = new List<Orbit>();
		if (double.IsNaN(posIn.x * velIn.y - posIn.y * velIn.x))
		{
			return list;
		}
		list.Add(new Orbit(posIn, velIn, Ref.controller.globalTime, initialPlanet, null));
		return Orbit.CalculateOrbits(list);
	}

	public static List<Orbit> CalculateOrbits(List<Orbit> orbitsIn)
	{
		while (orbitsIn.Count < 3)
		{
			Orbit orbit = orbitsIn[orbitsIn.Count - 1];
			if (orbit.orbitType == Orbit.Type.Escape)
			{
				if (orbitsIn.Count >= 2 && orbitsIn[orbitsIn.Count - 2].orbitType == Orbit.Type.Encounter)
				{
					return orbitsIn;
				}
				Double3 position = Kepler.GetPosition(orbit.planet.orbitData.SOI, orbit.endTrueAnomaly, orbit.argumentOfPeriapsis);
				Double3 velocity = Kepler.GetVelocity(orbit.semiMajorAxis, orbit.planet.orbitData.SOI, orbit.meanMotion, Kepler.GetEccentricAnomalyFromTrueAnomaly(orbit.endTrueAnomaly, orbit.eccentricity), orbit.endTrueAnomaly, orbit.eccentricity, orbit.argumentOfPeriapsis);
				Double3 posIn = position + orbit.planet.GetPosOut(orbit.orbitEndTime);
				Double3 velIn = velocity + orbit.planet.GetVelOut(orbit.orbitEndTime);
				if (double.IsNaN(posIn.x * velIn.y - posIn.y * velIn.x))
				{
					return orbitsIn;
				}
				orbitsIn.Add(new Orbit(posIn, velIn, orbit.orbitEndTime, orbit.nextPlanet, orbit.planet));
			}
			else
			{
				if (orbit.orbitType != Orbit.Type.Encounter)
				{
					return orbitsIn;
				}
				if (orbitsIn.Count >= 2 && orbitsIn[orbitsIn.Count - 2].planet == orbit.nextPlanet)
				{
					return orbitsIn;
				}
				double eccentricAnomalyFromTrueAnomaly = Kepler.GetEccentricAnomalyFromTrueAnomaly(orbit.endTrueAnomaly, orbit.eccentricity);
				double radius = Kepler.GetRadius(orbit.semiLatusRectum, orbit.eccentricity, orbit.endTrueAnomaly);
				Double3 position2 = Kepler.GetPosition(radius, orbit.endTrueAnomaly, orbit.argumentOfPeriapsis);
				Double3 velocity2 = Kepler.GetVelocity(orbit.semiMajorAxis, radius, orbit.meanMotion, eccentricAnomalyFromTrueAnomaly, orbit.endTrueAnomaly, orbit.eccentricity, orbit.argumentOfPeriapsis);
				Double3 posIn2 = position2 - orbit.nextPlanet.GetPosOut(orbit.orbitEndTime);
				Double3 velIn2 = velocity2 - orbit.nextPlanet.GetVelOut(orbit.orbitEndTime);
				if (double.IsNaN(posIn2.x * velIn2.y - posIn2.y * velIn2.x))
				{
					return orbitsIn;
				}
				orbitsIn.Add(new Orbit(posIn2, velIn2, orbit.orbitEndTime, orbit.nextPlanet, null));
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
		if (this.lastCalculationTime != newTime)
		{
			this.lastCalculationTime = newTime;
			double eccentricAnomaly = Kepler.GetEccentricAnomaly((newTime - this.periapsisPassageTime) * this.meanMotion, this.eccentricity);
			if (double.IsNaN(eccentricAnomaly))
			{
				return;
			}
			this.eccentricAnomnalyOut = eccentricAnomaly;
			this.trueAnomalyOut = Kepler.GetTrueAnomaly(this.eccentricAnomnalyOut, this.eccentricity);
			double radius = Kepler.GetRadius(this.semiLatusRectum, this.eccentricity, this.trueAnomalyOut);
			this.posOut = Kepler.GetPosition(radius, this.trueAnomalyOut, this.argumentOfPeriapsis);
			this.velOut = Kepler.GetVelocity(this.semiMajorAxis, radius, this.meanMotion, this.eccentricAnomnalyOut, this.trueAnomalyOut, this.eccentricity, this.argumentOfPeriapsis);
		}
	}

	public double GetNextTrueAnomalyPassageTime(double referenceTime, double trueAnomaly)
	{
		return this.GetLastTrueAnomalyPassageTime(referenceTime, trueAnomaly) + this._period;
	}

	public double GetLastTrueAnomalyPassageTime(double referenceTime, double trueAnomaly)
	{
		double num = this.periapsisPassageTime + Kepler.GetMeanAnomaly(this.eccentricity, trueAnomaly) / this.meanMotion;
		if (this.orbitType != Orbit.Type.Escape)
		{
			return num + (double)((int)((referenceTime - num) / this._period)) * this._period;
		}
		return num;
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
