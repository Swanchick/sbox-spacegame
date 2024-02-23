using Sandbox;

public sealed class TestPlayer : Component
{
	private SpaceController controller;

	protected override void OnStart()
	{
		controller = Components.Get<SpaceController>();
	}

	protected override void OnFixedUpdate()
	{
		
	}
}
