using Sandbox;
using Sandbox.Citizen;
using System;

public sealed class Player : Component
{
	[Property] public float playerSpeed => 500f;
	[Property] public float groundFriction => 9f;
	[Property] public float cameraSensetivity => 0.1f;

	[Property] public float airFriction => 0.1f;

	[Property] public CameraComponent playerCamera;
	[Property] public GameObject playerHead;
	[Property] public GameObject playerBody;
	
	private SpaceBody spaceBody;

	private Vector3 gravityVelocity;

	private CharacterController playerController { get; set; }
	private CitizenAnimationHelper animationHelper { get; set; }


	[Sync] public Vector3 wishDir { get; set; }
	[Sync] public Angles headAngles { get; set; }	

	protected override void OnStart()
	{
		playerController = Components.Get<CharacterController>();
		animationHelper = Components.Get<CitizenAnimationHelper>();

		playerBody.Enabled = IsProxy;

		if ( IsProxy )
		{
			playerCamera.GameObject.Destroy();
			return;
		}

		playerCamera.GameObject.SetParent( playerHead, false );

		gravityVelocity = Scene.PhysicsWorld.Gravity;

		spaceBody = Scene.GetAllComponents<SpaceBody>().FirstOrDefault();
	}

	protected override void OnUpdate()
	{
		if ( playerBody.Enabled )
		{
			Animation();
			RotateBody();
		}

		if ( IsProxy ) return;

		CameraRotation();
		RotateToSpaceBody();
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;
		
		Move();

		playerController.Move();
	}

	private Vector3 BuildDirection()
	{
		Rotation playerRotation = playerHead.Transform.Rotation;

		float horizontal = SInput.GetStength( "Right" ) - SInput.GetStength( "Left" );
		float vertical = SInput.GetStength( "Forward" ) - SInput.GetStength( "Backward" );

		Vector3 dir = horizontal * playerRotation.Right + vertical * playerRotation.Forward;

		return dir.Normal;
	}

	private void Move()
	{
		Vector3 finalVelocity = BuildDirection() * playerSpeed;

		Vector3 fallDirection = -Transform.Rotation.Up;
		Vector3 fallVelocity;

		Log.Info( fallDirection );

		if ( IsPlayerOnSpaceBody() || playerController.IsOnGround )
		{
			fallVelocity = Vector3.Zero;
			playerController.ApplyFriction( groundFriction );
		}
		else
		{
			fallVelocity = fallDirection * 1000f * Time.Delta;
			finalVelocity *= 0.1f;
		}

		wishDir = finalVelocity;

		playerController.Velocity += fallVelocity;

		playerController.Accelerate( finalVelocity );
		
	}

	private bool IsPlayerOnSpaceBody()
	{
		SceneTraceResult trace = Scene.Trace
			.Ray( Transform.Position, spaceBody.Transform.Position )
			.Run();

		return trace.Distance < 20f;
	}

	private void RotateToSpaceBody()
	{
		if ( spaceBody == null ) return;

		Vector3 dir = SVector3.FindDirectionOfVectors( Transform.Position, spaceBody.Transform.Position );

		Vector3 lookUp = Transform.Rotation.Up;

		Vector3 backward = -Transform.Rotation.Up;
		Vector3 forward = dir;

		float dot = Vector3.Dot( backward, forward );

		Vector3 cross = Vector3.Cross( backward, forward );
		float angle = (float)Math.Atan2( cross.Length, dot );

		Vector3 axis;
		if ( cross.Length > 0 )
		{
			axis = cross.Normal;
		}
		else
		{
			axis = Vector3.Cross( backward, lookUp ).Normal;
		}

		float w = (float)Math.Cos( angle / 2f );
		float x = axis.x * (float)Math.Sin( angle / 2f );
		float y = axis.y * (float)Math.Sin( angle / 2f );
		float z = axis.z * (float)Math.Sin( angle / 2f );

		Rotation rotation = new Rotation( x, y, z, w ) * Transform.Rotation;

		Transform.Rotation = Rotation.Slerp( Transform.Rotation, rotation, Time.Delta * 50f );
	}

	private void CameraRotation()
	{
		Angles _headAngles = playerHead.Transform.LocalRotation.Angles();
		Angles cameraAngles = playerCamera.Transform.LocalRotation.Angles();
		Vector2 mouseDealta = Input.MouseDelta;

		_headAngles.yaw -= mouseDealta.x * cameraSensetivity;
		playerHead.Transform.LocalRotation = _headAngles.ToRotation();

		headAngles = _headAngles;

		cameraAngles.pitch += mouseDealta.y * cameraSensetivity;
		cameraAngles.pitch = cameraAngles.pitch.Clamp( -89f, 89f );

		playerCamera.Transform.LocalRotation = cameraAngles.ToRotation();
	}

	private void Animation()
	{
		animationHelper.WithWishVelocity( wishDir );
		animationHelper.WithVelocity( playerController.Velocity );
		animationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
		animationHelper.IsGrounded = playerController.IsOnGround || IsPlayerOnSpaceBody();
	}

	private void RotateBody()
	{
		playerBody.Transform.LocalRotation = headAngles.ToRotation();
	}
}
