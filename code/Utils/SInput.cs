using Sandbox;

public sealed class SInput
{
	public static int GetStength(string keyName)
	{
		bool key = Input.Down(keyName);

		return key ? 1 : 0;
	}
}
