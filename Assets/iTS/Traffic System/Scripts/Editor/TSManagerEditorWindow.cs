using ITS.Editor;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class TSManagerEditorWindow : EditorWindow
{
    private TSMainManager manager;
    private SerializedObject _serializedObject;
    [MenuItem("Window/iTS/iTS Manager Window")]
    public static void Init()
    {
        TSMainManager m = FindObjectOfType<TSMainManager>();
        if (m != null)
        {
            Init(m);
        }
    }

    public static void Init(TSMainManager m)
    {
        TSManagerEditorWindow window = (TSManagerEditorWindow)EditorWindow.GetWindow(typeof(TSManagerEditorWindow));
        window.Show(true);
        window.Initialize(m);
    }

    private void OnEnable()
    {
        Initialize(null);
        EditorSceneManager.sceneLoaded += OnSceneManagerOnsceneLoaded;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }
    
    void OnDisable()
    {
        TSEditorTools.Instance.OnDisable();
        EditorSceneManager.sceneLoaded -= OnSceneManagerOnsceneLoaded;
        EditorSceneManager.sceneOpened -= OnSceneOpened;
    }

    private void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        Initialize(null);
    }

    private void OnSceneManagerOnsceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Initialize(null);
    }

    void OnFocus()
    {
        SceneView.duringSceneGui -= TSEditorTools.Instance.OnSceneGUI;
        SceneView.duringSceneGui += TSEditorTools.Instance.OnSceneGUI;
    }

    private void OnDestroy()
    {
        SceneView.duringSceneGui -= TSEditorTools.Instance.OnSceneGUI;
        EditorSceneManager.sceneLoaded -= OnSceneManagerOnsceneLoaded;
        EditorSceneManager.sceneOpened -= OnSceneOpened;
    }

    void Initialize(TSMainManager m )
    {
        if (m == null)
        {
            manager = FindObjectOfType<TSMainManager>();
        }
        if (manager == null){Close(); return;}

        _serializedObject = new SerializedObject(manager);
        TSEditorTools.Instance.OnEnable(manager, _serializedObject);
    }

    Vector2 scrollPos;

    private void OnGUI()
    {
        manager = EditorGUILayout.ObjectField("ITSManager", manager, typeof(TSMainManager),true)as TSMainManager;

        if (manager == null || _serializedObject == null)
        {
            Initialize(manager);
            return;
        }
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        TSEditorTools.Instance.OnInspectorGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(manager);
        }
        EditorGUILayout.EndScrollView();
    }
} //End of Main Class