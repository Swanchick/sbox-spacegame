using Sandbox;
using System;

public sealed class SVector3
{
	public static Angles GetDirectionAngle(Vector3 direction, Vector3 up)
	{
		Angles angles = new Angles();

		double radToAngle = 180 / Math.PI;

		angles.pitch = (float)(Math.Atan2(direction.x, direction.z) * radToAngle);
		angles.roll = (float)(Math.Atan2( direction.z, -direction.y ) * radToAngle) + 90;
		
		return angles;
	}

	public static Vector3 FindDirectionOfVectors( Vector3 a, Vector3 b )
	{
		Vector3 result = b - a;

		return result;
	}
}
