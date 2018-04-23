using System;
using System.Linq;
using System.Reflection;

namespace UIEventDelegate
{
	public static class AttributeExtension
	{
		public static T GetAttribute<T>(this MemberInfo memberInfo) where T : Attribute
		{
			return memberInfo.GetCustomAttributes(typeof(T), true).FirstOrDefault<object>() as T;
		}
	}
}
