using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TSTrafficSpawner))]
public class TSSpawnerEditor : Editor{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        TSTrafficSpawner spawner = (TSTrafficSpawner)target;
        if (spawner.cars != null && spawner.cars.Length >0)
        {
            if (spawner.cars[0].cars == null){return;}
            if (PrefabUtility.GetPrefabAssetType(spawner.cars[0].cars) != PrefabAssetType.NotAPrefab)
            {
                spawner.carsArePrefabs = true;
                EditorUtility.SetDirty(target);
            }
            else
            {
                spawner.carsArePrefabs = false;
                EditorUtility.SetDirty(target);
            }
        }
    }
}