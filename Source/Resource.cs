using System;

public class Resource
{
    public static string GetResourceName(Resource.Type resourceType)
    {
        return (new string[]
        {
            "Fuel",
            "Solid Fuel",
            "Electric Power"
        })[(int)resourceType];
    }

    public static string GetResourceUnit(Resource.Type resourceType)
    {
        return (new string[]
        {
            "t",
            "t",
            string.Empty
        })[(int)resourceType];
    }

    public static bool GrupsGlobally(Resource.Type resourceType)
    {
        return (new bool[]
        {
            default(bool),
            default(bool),
            true
        })[(int)resourceType];
    }

    public enum Type
    {
        Fuel,
        SolidFuel,
        Power
    }
}
