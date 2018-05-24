using System;

namespace JMtech.JDIS.Web.Request
{
	public class TrendCategory
	{
		public string fromEnum(TrendEnum e)
		{
			if (e == TrendEnum.HOT)
			{
				return TrendCategory.HOT;
			}
			if (e == TrendEnum.TOP)
			{
				return TrendCategory.TOP;
			}
			if (e == TrendEnum.NEW)
			{
				return TrendCategory.NEW;
			}
			if (e == TrendEnum.OWN)
			{
				return TrendCategory.OWN;
			}
			return TrendCategory.NEW;
		}

		public static readonly string TOP = "TOP";

		public static readonly string HOT = "HOT";

		public static readonly string NEW = "NEW";

		public static readonly string OWN = "OWN";
	}
}
