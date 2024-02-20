using Sandbox;
using System;

public sealed class SpaceBody : Component
{
	[Property] public GameObject testArrow;

	private void GetDirectionAngle( Vector3 direction )
	{
		Angles angle = new Angles();

		double radToAngle = 180 / Math.PI;

		angle.pitch = (float)(Math.Atan2( direction.x, direction.z ) * radToAngle) + 180;
		angle.roll = (float)(Math.Atan2( direction.z, direction.y ) * radToAngle) + 90;

		testArrow.Transform.Rotation = angle.ToRotation();
	}

	protected override void OnUpdate()
	{
		Vector3 direction = SVector3.FindDirectionOfVectors( testArrow.Transform.Position, Transform.Position );

		GetDirectionAngle( direction );
	}
}
