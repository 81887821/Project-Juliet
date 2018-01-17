using UnityEngine;
using UnityEditor;

public class DialogueAsset{
	[MenuItem("Assets/Create/Dialogue")]
	public static void CreateAsset()
	{
		ScriptableObjectUtility.CreateAsset<Dialogue>();
	}
}