using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public struct Double3
{
	public Double3(double X, double Y, double Z)
	{
		this.x = X;
		this.y = Y;
		this.z = Z;
	}

	public Double3(double X, double Y)
	{
		this.x = X;
		this.y = Y;
		this.z = 0.0;
	}

	public static Double3 zero
	{
		get
		{
			return new Double3(0.0, 0.0);
		}
	}

	public static Double3 Cross(Double3 a, Double3 b)
	{
		return new Double3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
	}

	public static Double3 Cross2d(Double3 a, Double3 b)
	{
		return new Double3(0.0, 0.0, a.x * b.y - a.y * b.x);
	}

	public static double Dot(Double3 a, Double3 b)
	{
		return a.x * b.x + a.y * b.y + a.z * b.z;
	}

	public double magnitude2d
	{
		get
		{
			return Math.Sqrt(this.x * this.x + this.y * this.y);
		}
	}

	public double sqrMagnitude
	{
		get
		{
			return this.x * this.x + this.y * this.y + this.z * this.z;
		}
	}

	public double sqrMagnitude2d
	{
		get
		{
			return this.x * this.x + this.y * this.y;
		}
	}

	public Double3 normalized2d
	{
		get
		{
			double magnitude2d = this.magnitude2d;
			if (magnitude2d > 9.99999974737875E-06)
			{
				return this / magnitude2d;
			}
			return Double3.zero;
		}
	}

	public static Double3 operator +(Double3 a, Double3 b)
	{
		return new Double3(a.x + b.x, a.y + b.y, a.z + b.z);
	}

	public static Double3 operator +(Double3 a, Vector3 b)
	{
		return new Double3(a.x + (double)b.x, a.y + (double)b.y, a.z + (double)b.z);
	}

	public static Double3 operator -(Double3 a, Double3 b)
	{
		return new Double3(a.x - b.x, a.y - b.y, a.z - b.z);
	}

	public static Double3 operator -(Double3 a, Vector3 b)
	{
		return new Double3(a.x - (double)b.x, a.y - (double)b.y, a.z - (double)b.z);
	}

	public static Double3 operator -(Double3 a)
	{
		return new Double3(-a.x, -a.y, -a.z);
	}

	public static Double3 operator *(Double3 a, double d)
	{
		return new Double3(a.x * d, a.y * d, a.z * d);
	}

	public static Double3 operator *(double d, Double3 a)
	{
		return new Double3(a.x * d, a.y * d, a.z * d);
	}

	public static Double3 operator /(Double3 a, double d)
	{
		return new Double3(a.x / d, a.y / d, a.z / d);
	}

	public Vector3 toVector3
	{
		get
		{
			return new Vector3((float)this.x, (float)this.y, (float)this.z);
		}
	}

	public Vector2 toVector2
	{
		get
		{
			return new Vector2((float)this.x, (float)this.y);
		}
	}

	public static Double3 ToDouble3(Vector3 vector3)
	{
		return new Double3((double)vector3.x, (double)vector3.y, (double)vector3.z);
	}

	public Double3 roundTo1000
	{
		get
		{
			return new Double3((double)((int)(this.x / 1000.0)) * 1000.0, (double)((int)(this.y / 1000.0)) * 1000.0);
		}
	}

	public Double3 RotateZ(double rotZ)
	{
		double num = Math.Cos(rotZ);
		double num2 = Math.Sin(rotZ);
		return new Double3(this.x * num - this.y * num2, this.x * num2 + this.y * num);
	}

	public static double GetClosestPointOnLine(Double3 p2, Double3 p3)
	{
		double num = p2.x;
		double num2 = p2.y;
		double num3 = num * num + num2 * num2;
		double num4 = (p3.x * num + p3.y * num2) / num3;
		if (num4 < 0.0)
		{
			return 0.0;
		}
		if (num4 > 1.0)
		{
			return 1.0;
		}
		return num4;
	}

	[HorizontalGroup(0f, 0, 0, 0)]
	[LabelWidth(15f)]
	public double x;

	[HorizontalGroup(0f, 0, 0, 0)]
	[LabelWidth(15f)]
	public double y;

	[HorizontalGroup(0f, 0, 0, 0)]
	[LabelWidth(15f)]
	public double z;
}
