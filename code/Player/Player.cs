using Sandbox;
using Sandbox.Citizen;
using System.Numerics;

public sealed class Player : Component
{
	[Property] public float playerSpeed => 500f;
	[Property] public float groundFriction => 9f;
	[Property] public float cameraSensetivity => 0.1f;

	[Property] public CameraComponent playerCamera;
	[Property] public GameObject playerHead;
	[Property] public GameObject playerBody;

	private SpaceBody spaceBody;

	private CharacterController playerController { get; set; }
	private CitizenAnimationHelper animationHelper { get; set; }

	private float gravityAcceleration;
	private Vector3 gravityVelocity = Vector3.Zero;

	private Vector3 currentNormal = Vector3.Zero;


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

		FindSpaceBody();
	}

	private void FindSpaceBody()
	{
		spaceBody = Scene.GetAllComponents<SpaceBody>().FirstOrDefault();

		Log.Info( spaceBody );
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
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy ) return;
		
		Move();
		InteractWithSpaceBody();
		Fall();

		playerController.Move();
	}

	private Vector3 BuildDirection()
	{
		Rotation playerRotation = playerHead.Transform.Rotation;

		float horizontal = SInput.GetStength( "Right" ) - SInput.GetStength( "Left" );
		float vertical = SInput.GetStength( "Forward" ) - SInput.GetStength( "Backward" );

		Vector3 dir = horizontal * playerRotation.Right + vertical * playerRotation.Forward;

		return dir.Normal * playerSpeed;
	}

	private void Move()
	{
		wishDir = BuildDirection();

		playerController.Accelerate( wishDir );
		playerController.ApplyFriction( groundFriction );
	}

	private void InteractWithSpaceBody()
	{
		if ( spaceBody == null ) return;

		SceneTraceResult trace = Scene.Trace
			.Ray(Transform.Position, spaceBody.Transform.Position)
			.Run();

		Vector3 dir = trace.Normal;
		Angles angles = Rotation.LookAt( dir ).Angles();
		angles.pitch -= 90f;
		Rotation playerRotation = Transform.Rotation;

		Transform.Rotation = Rotation.Lerp( playerRotation, angles.ToRotation(), Time.Delta * 10f); // ToDo: Make speed for rotation
	}

	private bool IsOnSpaceBodyGround()
	{
		SceneTraceResult trace = Scene.Trace
			.Ray( Transform.Position + Transform.Rotation.Up * 10f, Transform.Position + Transform.Rotation.Down * 30f )
			.Run();

		return trace.Hit;
	}

	private void Fall()
	{
		if ( spaceBody == null ) return;

		Vector3 dirToFall = SVector3.FindDirectionOfVectors( Transform.Position, spaceBody.Transform.Position );

		Gizmo.Draw.Arrow( Transform.Position + Transform.Rotation.Up * 10f, Transform.Position + Transform.Rotation.Down * 40f );

		Log.Info( playerController.IsOnGround || IsOnSpaceBodyGround() );

		if ( playerController.IsOnGround )
		{
			dirToFall = Vector3.Zero;
		}
		else
		{
			dirToFall *= Time.Delta * 5000f;
		}

		

		playerController.Velocity += dirToFall;
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
		animationHelper.IsGrounded = IsOnSpaceBodyGround();
	}

	private void RotateBody()
	{
		playerBody.Transform.LocalRotation = headAngles.ToRotation();
	}
}
