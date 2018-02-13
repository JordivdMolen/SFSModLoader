using System;

public static class Kepler
{
	public const double Tau = 6.2831853071795862;

	private static int maxIterations = 50;

	private static double tolerance = 1E-05;

	public static double GetSemiLatusRectum(double p, double e)
	{
		if (e == 0.0)
		{
			return p;
		}
		if (e == 1.0)
		{
			return p * 2.0;
		}
		if (e < 1.0)
		{
			return p * (1.0 - e * e) / (1.0 - e);
		}
		return p * (e * e - 1.0) / (e - 1.0);
	}

	public static double GetPeriod(double e, double sma, double mass)
	{
		return 6.2831853071795862 * Math.Sqrt(Math.Pow(sma, 3.0) / mass);
	}

	public static double GetMeanMotion(double period, double e, double planetMass, double sma)
	{
		if (e < 1.0)
		{
			return 6.2831853071795862 / period;
		}
		return Math.Sqrt(planetMass / Math.Pow(-sma, 3.0));
	}

	public static double GetTrueAnomaly(double E, double e)
	{
		if (e < 1.0)
		{
			return 2.0 * Math.Atan2(Math.Sqrt(1.0 + e) * Math.Sin(E / 2.0), Math.Sqrt(1.0 - e) * Math.Cos(E / 2.0));
		}
		if (e > 1.0)
		{
			return 2.0 * Math.Atan2(Math.Sqrt(e + 1.0) * Math.Sinh(E / 2.0), Math.Sqrt(e - 1.0) * Math.Cosh(E / 2.0));
		}
		if (e == 1.0)
		{
			return 2.0 * Math.Atan(E);
		}
		return E;
	}

	public static double GetRadius(double l, double e, double v)
	{
		return l / (1.0 + e * Math.Cos(v));
	}

	public static double GetTrueAnomalyAtRadius(double r, double l, double e)
	{
		return Math.Acos((l / r - 1.0) / e);
	}

	public static Double3 GetVelocity(double a, double r, double n, double E, double v, double e, double arg)
	{
		Double3 a2;
		if (e < 1.0)
		{
			a2 = new Double3(-Math.Sin(E), Math.Cos(E) * Math.Sqrt(1.0 - e * e));
			a2 *= a * a * n / r;
		}
		else if (e > 1.0)
		{
			a2 = new Double3(-Math.Sinh(E), Math.Cosh(E) * Math.Sqrt(e * e - 1.0));
			a2 *= a * a * n / r;
		}
		else
		{
			a2 = new Double3(-Math.Sin(v) / (Math.Cos(v) + 1.0), 1.0);
			a2 = a2.normalized2d * Math.Sqrt(n * n * a * a * a * (2.0 / r));
		}
		double num = Math.Cos(arg);
		double num2 = Math.Sin(arg);
		return new Double3(a2.x * num - a2.y * num2, a2.x * num2 + a2.y * num);
	}

	public static double GetEccentricAnomaly(double M, double e)
	{
		if (e < 1.0)
		{
			return Kepler.NewtonElliptical(M, e, 1);
		}
		if (e > 1.0)
		{
			return Kepler.NewtonHyperbolic(M, e);
		}
		if (e == 1.0)
		{
			return Kepler.NewtonParabolic(M, e, 1);
		}
		return M;
	}

	public static double GetEccentricAnomalyFromTrueAnomaly(double trueAnomaly, double e)
	{
		if (e < 1.0)
		{
			return 2.0 * Math.Atan(Math.Tan(trueAnomaly / 2.0) / Math.Sqrt((1.0 + e) / (1.0 - e)));
		}
		if (e >= 1.0)
		{
			double num = (e + Math.Cos(trueAnomaly)) / (1.0 + e * Math.Cos(trueAnomaly));
			return Math.Log(num + Math.Sqrt(num * num - 1.0)) * (double)Math.Sign(trueAnomaly);
		}
		return 0.0;
	}

	public static double GetMeanAnomaly(double e, double trueAnomaly, Double3 posIn, double arg)
	{
		double num = 0.0;
		if (e < 1.0)
		{
			double num2 = 2.0 * Math.Atan(Math.Tan(trueAnomaly / 2.0) / Math.Sqrt((1.0 + e) / (1.0 - e)));
			num = num2 - e * Math.Sin(num2);
		}
		else if (e >= 1.0)
		{
			double num3 = (e + Math.Cos(trueAnomaly)) / (1.0 + e * Math.Cos(trueAnomaly));
			double num4 = Math.Log(num3 + Math.Sqrt(num3 * num3 - 1.0));
			num = e * Math.Sinh(num4) - num4;
		}
		double num5 = Math.Atan2(posIn.y, posIn.x) - arg;
		if (num5 > 3.1415926535897931)
		{
			num5 -= 6.2831853071795862;
		}
		if (num5 < -3.1415926535897931)
		{
			num5 += 6.2831853071795862;
		}
		if (num5 > 0.0)
		{
			num = -num;
		}
		return num;
	}

	public static double GetMeanAnomaly(double e, double trueAnomaly)
	{
		if (e < 1.0)
		{
			double num = 2.0 * Math.Atan(Math.Tan(trueAnomaly / 2.0) / Math.Sqrt((1.0 + e) / (1.0 - e)));
			return num - e * Math.Sin(num);
		}
		double num2 = (e + Math.Cos(trueAnomaly)) / (1.0 + e * Math.Cos(trueAnomaly));
		double num3 = Math.Log(num2 + Math.Sqrt(num2 * num2 - 1.0));
		return (e * Math.Sinh(num3) - num3) * (double)Math.Sign(trueAnomaly);
	}

	public static double GetSemiMajorAxis(double p, double e)
	{
		if (e > 1.0)
		{
			return -p / (e - 1.0);
		}
		if (e < 1.0)
		{
			return p / (1.0 - e);
		}
		return double.PositiveInfinity;
	}

	private static double NewtonElliptical(double M, double e, int count)
	{
		double num = M;
		double num2 = 0.0;
		for (int i = 0; i < Kepler.maxIterations * count; i++)
		{
			num2 = num - (num - e * Math.Sin(num2) - M) / (1.0 - e * Math.Cos(num2));
			if (Math.Abs(num2 - e * Math.Sin(num2) - M) < Kepler.tolerance)
			{
				return num2;
			}
			num = num2;
		}
		if (count <= 3)
		{
			return Kepler.NewtonElliptical(M + 1E-05, e, count + 1);
		}
		return num2;
	}

	private static double NewtonParabolic(double M, double e, int count)
	{
		double num = M;
		double num2 = 0.0;
		for (int i = 0; i < Kepler.maxIterations * count; i++)
		{
			num2 = num - (num + num2 * num2 * num2 / 3.0 - M) / (1.0 + num * num);
			if (Math.Abs(num2 + num2 * num2 * num2 / 3.0 - M) < Kepler.tolerance)
			{
				return num2;
			}
			num = num2;
		}
		return num2;
	}

	private static double NewtonHyperbolic(double M, double e)
	{
		double num = Math.Log(2.0 * Math.Abs(M) / e + 1.8);
		double num2 = 0.0;
		for (int i = 0; i < Kepler.maxIterations * 20; i++)
		{
			num2 = num - (e * Math.Sinh(num) - num - M) / (e * Math.Cosh(num) - 1.0);
			if (Math.Abs(e * Math.Sinh(num2) - num2 - M) < Kepler.tolerance)
			{
				return num2;
			}
			num = num2;
		}
		return num2;
	}

	public static Double3 GetPosition(double r, double v, double arg)
	{
		return new Double3(Math.Cos(v + arg) * r, Math.Sin(v + arg) * r);
	}

	public static double GetPhaseAngle(double currentSma, double destinationSma)
	{
		double x = (currentSma + destinationSma) * 0.5;
		return 3.1415926535897931 - 3.1415926535897931 / Math.Sqrt(Math.Pow(destinationSma, 3.0) / Math.Pow(x, 3.0));
	}

	public static double GetPhaseAngle(CelestialBodyData departurePlanet, CelestialBodyData destinationBody)
	{
		double x = (departurePlanet.orbitData.orbitHeightM + destinationBody.orbitData.orbitHeightM) * 0.5;
		return 3.1415926535897931 - 3.1415926535897931 / Math.Sqrt(Math.Pow(destinationBody.orbitData.orbitHeightM, 3.0) / Math.Pow(x, 3.0));
	}

	public static double GetEjectionAngle(double escapeVelocity, double targetPeriapsis, double mass, double SOI)
	{
		Double3 a = new Double3(SOI, 0.0);
		double num = 0.78539816339744828;
		double num2 = 0.39269908169872414;
		for (int i = 0; i < 16; i++)
		{
			Double3 @double = new Double3(Math.Cos(num) * escapeVelocity, Math.Sin(num) * escapeVelocity);
			Double3 double2 = Double3.Cross(@double, Double3.Cross2d(a, @double)) / mass - a.normalized2d;
			if (i == 15)
			{
				return num - Math.Atan2(double2.y, double2.x);
			}
			num -= num2 * (double)Math.Sign(-mass / (2.0 * (Math.Pow(@double.magnitude2d, 2.0) / 2.0 - mass / a.magnitude2d)) * (1.0 - double2.magnitude2d) - targetPeriapsis);
			num2 *= 0.5;
		}
		return 0.0;
	}
}
