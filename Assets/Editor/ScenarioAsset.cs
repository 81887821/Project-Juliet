using UnityEditor;

public class ScenarioAsset
{
    [MenuItem("Assets/Create/ScenarioScript")]
    public static void CreateAsset()
    {
        ScriptableObjectUtility.CreateAsset<ScenarioScript>();
    }
}
