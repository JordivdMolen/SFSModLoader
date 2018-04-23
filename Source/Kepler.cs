using System;

public static class Kepler
{
	public static double GetSemiLatusRectum(double p, double e)
	{
		bool flag = e == 0.0;
		double result;
		if (flag)
		{
			result = p;
		}
		else
		{
			bool flag2 = e == 1.0;
			if (flag2)
			{
				result = p * 2.0;
			}
			else
			{
				bool flag3 = e < 1.0;
				if (flag3)
				{
					result = p * (1.0 - e * e) / (1.0 - e);
				}
				else
				{
					result = p * (e * e - 1.0) / (e - 1.0);
				}
			}
		}
		return result;
	}

	public static double GetPeriod(double e, double sma, double mass)
	{
		return 6.2831853071795862 * Math.Sqrt(Math.Pow(sma, 3.0) / mass);
	}

	public static double GetMeanMotion(double period, double e, double planetMass, double sma)
	{
		bool flag = e < 1.0;
		double result;
		if (flag)
		{
			result = 6.2831853071795862 / period;
		}
		else
		{
			result = Math.Sqrt(planetMass / Math.Pow(-sma, 3.0));
		}
		return result;
	}

	public static double GetTrueAnomaly(double E, double e)
	{
		bool flag = e < 1.0;
		double result;
		if (flag)
		{
			result = 2.0 * Math.Atan2(Math.Sqrt(1.0 + e) * Math.Sin(E / 2.0), Math.Sqrt(1.0 - e) * Math.Cos(E / 2.0));
		}
		else
		{
			bool flag2 = e > 1.0;
			if (flag2)
			{
				result = 2.0 * Math.Atan2(Math.Sqrt(e + 1.0) * Math.Sinh(E / 2.0), Math.Sqrt(e - 1.0) * Math.Cosh(E / 2.0));
			}
			else
			{
				bool flag3 = e == 1.0;
				if (flag3)
				{
					result = 2.0 * Math.Atan(E);
				}
				else
				{
					result = E;
				}
			}
		}
		return result;
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
		bool flag = e < 1.0;
		Double3 @double;
		if (flag)
		{
			@double = new Double3(-Math.Sin(E), Math.Cos(E) * Math.Sqrt(1.0 - e * e));
			@double *= a * a * n / r;
		}
		else
		{
			bool flag2 = e > 1.0;
			if (flag2)
			{
				@double = new Double3(-Math.Sinh(E), Math.Cosh(E) * Math.Sqrt(e * e - 1.0));
				@double *= a * a * n / r;
			}
			else
			{
				@double = new Double3(-Math.Sin(v) / (Math.Cos(v) + 1.0), 1.0);
				@double = @double.normalized2d * Math.Sqrt(n * n * a * a * a * (2.0 / r));
			}
		}
		double num = Math.Cos(arg);
		double num2 = Math.Sin(arg);
		return new Double3(@double.x * num - @double.y * num2, @double.x * num2 + @double.y * num);
	}

	public static double GetEccentricAnomaly(double M, double e)
	{
		bool flag = e < 1.0;
		double result;
		if (flag)
		{
			result = Kepler.NewtonElliptical(M, e, 1);
		}
		else
		{
			bool flag2 = e > 1.0;
			if (flag2)
			{
				result = Kepler.NewtonHyperbolic(M, e);
			}
			else
			{
				bool flag3 = e == 1.0;
				if (flag3)
				{
					result = Kepler.NewtonParabolic(M, e, 1);
				}
				else
				{
					result = M;
				}
			}
		}
		return result;
	}

	public static double GetEccentricAnomalyFromTrueAnomaly(double trueAnomaly, double e)
	{
		bool flag = e < 1.0;
		double result;
		if (flag)
		{
			result = 2.0 * Math.Atan(Math.Tan(trueAnomaly / 2.0) / Math.Sqrt((1.0 + e) / (1.0 - e)));
		}
		else
		{
			bool flag2 = e >= 1.0;
			if (flag2)
			{
				double num = (e + Math.Cos(trueAnomaly)) / (1.0 + e * Math.Cos(trueAnomaly));
				result = Math.Log(num + Math.Sqrt(num * num - 1.0)) * (double)Math.Sign(trueAnomaly);
			}
			else
			{
				result = 0.0;
			}
		}
		return result;
	}

	public static double GetMeanAnomaly(double e, double trueAnomaly, Double3 posIn, double arg)
	{
		double num = 0.0;
		bool flag = e < 1.0;
		if (flag)
		{
			double num2 = 2.0 * Math.Atan(Math.Tan(trueAnomaly / 2.0) / Math.Sqrt((1.0 + e) / (1.0 - e)));
			num = num2 - e * Math.Sin(num2);
		}
		else
		{
			bool flag2 = e >= 1.0;
			if (flag2)
			{
				double num3 = (e + Math.Cos(trueAnomaly)) / (1.0 + e * Math.Cos(trueAnomaly));
				double num4 = Math.Log(num3 + Math.Sqrt(num3 * num3 - 1.0));
				num = e * Math.Sinh(num4) - num4;
			}
		}
		double num5 = Math.Atan2(posIn.y, posIn.x) - arg;
		bool flag3 = num5 > 3.1415926535897931;
		if (flag3)
		{
			num5 -= 6.2831853071795862;
		}
		bool flag4 = num5 < -3.1415926535897931;
		if (flag4)
		{
			num5 += 6.2831853071795862;
		}
		bool flag5 = num5 > 0.0;
		if (flag5)
		{
			num = -num;
		}
		return num;
	}

	public static double GetMeanAnomaly(double e, double trueAnomaly)
	{
		bool flag = e < 1.0;
		double result;
		if (flag)
		{
			double num = 2.0 * Math.Atan(Math.Tan(trueAnomaly / 2.0) / Math.Sqrt((1.0 + e) / (1.0 - e)));
			result = num - e * Math.Sin(num);
		}
		else
		{
			double num2 = (e + Math.Cos(trueAnomaly)) / (1.0 + e * Math.Cos(trueAnomaly));
			double num3 = Math.Log(num2 + Math.Sqrt(num2 * num2 - 1.0));
			result = (e * Math.Sinh(num3) - num3) * (double)Math.Sign(trueAnomaly);
		}
		return result;
	}

	public static double GetSemiMajorAxis(double p, double e)
	{
		bool flag = e > 1.0;
		double result;
		if (flag)
		{
			result = -p / (e - 1.0);
		}
		else
		{
			bool flag2 = e < 1.0;
			if (flag2)
			{
				result = p / (1.0 - e);
			}
			else
			{
				result = double.PositiveInfinity;
			}
		}
		return result;
	}

	private static double NewtonElliptical(double M, double e, int count)
	{
		double num = M;
		double num2 = 0.0;
		for (int i = 0; i < Kepler.maxIterations * count; i++)
		{
			num2 = num - (num - e * Math.Sin(num2) - M) / (1.0 - e * Math.Cos(num2));
			bool flag = Math.Abs(num2 - e * Math.Sin(num2) - M) < Kepler.tolerance;
			if (flag)
			{
				return num2;
			}
			num = num2;
		}
		bool flag2 = count <= 3;
		if (flag2)
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
			bool flag = Math.Abs(num2 + num2 * num2 * num2 / 3.0 - M) < Kepler.tolerance;
			if (flag)
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
			bool flag = Math.Abs(e * Math.Sinh(num2) - num2 - M) < Kepler.tolerance;
			if (flag)
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
			bool flag = i == 15;
			if (flag)
			{
				return num - Math.Atan2(double2.y, double2.x);
			}
			num -= num2 * (double)Math.Sign(-mass / (2.0 * (Math.Pow(@double.magnitude2d, 2.0) / 2.0 - mass / a.magnitude2d)) * (1.0 - double2.magnitude2d) - targetPeriapsis);
			num2 *= 0.5;
		}
		return 0.0;
	}

	static Kepler()
	{
		// Note: this type is marked as 'beforefieldinit'.
	}

	public const double Tau = 6.2831853071795862;

	private static int maxIterations = 50;

	private static double tolerance = 1E-05;
}
