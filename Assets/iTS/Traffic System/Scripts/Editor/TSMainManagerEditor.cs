using ITS.Editor;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TSMainManager))]
public class TSMainManagerEditor : Editor
{
    private TSMainManager manager;

    public void OnEnable()
    {
        manager = (TSMainManager) target;
        TSEditorTools.Instance.OnEnable(manager, serializedObject);
    }

    void OnDisable()
    {
        TSEditorTools.Instance.OnDisable();
    }

    public override void OnInspectorGUI()
    {
        TSEditorTools.Instance.OnInspectorGUI();
        
        if (GUI.changed)
        {
            EditorUtility.SetDirty(manager);
        }
    }

   
    private void OnSceneGUI()
    {
        TSEditorTools.Instance.OnSceneGUI();
    }
} //End of Main Class