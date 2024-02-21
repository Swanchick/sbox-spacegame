using Sandbox;
using Sandbox.Citizen;

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
		Vector3 finalVelocity = BuildDirection();

		if ( playerController.IsOnGround )
		{
			playerController.ApplyFriction( groundFriction );
		}
		else
		{
			finalVelocity *= airFriction;

			playerController.Velocity += gravityVelocity * Time.Delta ;
		}

		wishDir = finalVelocity;

		playerController.Accelerate( finalVelocity );
		playerController.Move();
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
		animationHelper.IsGrounded = playerController.IsOnGround;
	}

	private void RotateBody()
	{
		playerBody.Transform.LocalRotation = headAngles.ToRotation();
	}
}
