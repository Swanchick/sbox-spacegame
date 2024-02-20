using Sandbox;
using System;

public sealed class SUtils
{
	public static GameObject FindGameObjectByNameInChildren(GameObject parent, string name)
	{
		List<GameObject> children = parent.Children;

		foreach (GameObject child in children)
		{
			if (child.Name == name)
				return child;
		}

		return null;
	}
}
