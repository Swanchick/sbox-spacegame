using Sandbox;
using Sandbox.Internal;
using System;
using System.Runtime.CompilerServices;

public class SpaceController : Component
{
	[Range( 0f, 200f, 0.01f, true, true )]
	[Property]
	[DefaultValue( 16f )]
	public float Radius { get; set; } = 16f;


	[Range( 0f, 200f, 0.01f, true, true )]
	[Property]
	[DefaultValue( 64f )]
	public float Height { get; set; } = 64f;


	[Property]
	public TagSet IgnoreLayers { get; set; } = new TagSet();

	private SceneTrace BuildTrace( Vector3 from, Vector3 to )
	{
		return BuildTrace( base.Scene.Trace.Ray( in from, in to ) );
	}

	public BBox BoundingBox => new BBox( new Vector3( 0f - Radius, 0f - Radius, 0f ), new Vector3( Radius, Radius, Height ) );

	private SceneTrace BuildTrace( SceneTrace source )
	{
		BBox hull = BoundingBox;
		return source.Size( in hull ).WithoutTags( IgnoreLayers ).IgnoreGameObjectHierarchy( base.GameObject );
	}

	private SceneTrace BuildTraceFromTo( Vector3 from, Vector3 to )
	{
		return BuildTrace( base.Scene.Trace.Ray( in from, in to ) );
	}

	public SceneTraceResult TraceDirection( Vector3 direction )
	{
		return BuildTrace( base.GameObject.Transform.Position, base.GameObject.Transform.Position + direction ).Run();
	}

	protected override void DrawGizmos()
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		Gizmo.GizmoDraw draw = Gizmo.Draw;
		BBox box = BoundingBox;
		draw.LineBBox( in box );
	}

	public void MoveTo( Vector3 targetPosition )
	{
		Vector3 position = Transform.Position;
		Vector3 velocity = targetPosition - position;
		CharacterControllerHelper characterControllerHelper = new CharacterControllerHelper( BuildTrace( position, position ), position, velocity );

		characterControllerHelper.TryMove( 1f );

		Transform.Position = characterControllerHelper.Position;
	}
}
