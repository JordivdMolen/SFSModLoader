using System;

public static class EXT
{
	public static string FormatStr(this string str, object args)
	{
		return string.Format(str, args);
	}

	public static string FormatStr(this string str, object[] args)
	{
		return string.Format(str, args);
	}
}
