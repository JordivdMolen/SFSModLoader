using System;

public static class Engineer
{
	public static double[] GetOrbitData()
	{
		if (Ref.mainVessel == null)
		{
			return new double[3];
		}
		double[] array = new double[3];
		Double3 posIn = Ref.mainVessel.GetGlobalPosition;
		if (Ref.mainVessel.state == Vessel.State.RealTime)
		{
			posIn = Ref.positionOffset + Ref.mainVessel.partsManager.rb2d.worldCenterOfMass;
			Orbit orbit = new Orbit(posIn, Ref.mainVessel.GetGlobalVelocity, Ref.mainVessel.GetVesselPlanet);
			array[0] = orbit.apoapsis - Ref.mainVessel.GetVesselPlanet.radius;
			array[1] = orbit.periapsis - Ref.mainVessel.GetVesselPlanet.radius;
			array[2] = orbit.eccentricity;
		}
		return array;
	}
}
