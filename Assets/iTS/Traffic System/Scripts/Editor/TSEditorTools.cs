using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using ITS.Utils;
using Object = UnityEngine.Object;

namespace ITS.Editor
{
    public class TSEditorTools
    {
        private static TSEditorTools _instance;
        public static TSEditorTools Instance => _instance ?? (_instance = new TSEditorTools());


        private struct DrawingData : IEquatable<DrawingData>
        {
            public int laneIndex;
            public Vector3[] points;
            public List<Vector3[]> connectorsPoints;

            public bool Equals(DrawingData other)
            {
                return laneIndex == other.laneIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is DrawingData other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = laneIndex;
                    return hashCode;
                }
            }
        }

        HashSet<DrawingData> tempLanes;
        private TSMainManagerData mainData;
        private TSMainManager manager;
        private string[] toolbar1Contents = new string[] {"Lanes", "Lane Connectors", "Settings"};
        private string[] toolbar2Contents = new string[] {"Lane", "Lane Linking", "Edit Points", "Batch Settings"};
        private string[] toolbar3Contents = new string[] {"Connector", "Edit Points", "Batch Settings"};
        private bool addConnector = false;
        private int currentLaneSelected = -1;
        private int currentConnectorSelected = -1;
        private bool addLane1 = false;
        private bool removeLane = false;
        private TSLaneConnector newConnector;
        private bool addConnector1 = false;
        private bool removeConnector = false;
        private bool removeConnectorPoint = false;
        private bool removeLanePoint = false;
        private bool addLaneLink = false;
        private bool removeLaneLink = false;
        private int linkLane1 = -1;
        private int linkLane2 = -1;
        private bool linkLane1Set = false;
        private bool linkLane2Set = false;
        private bool linkLane1Right = false;
        private bool linkLane2Right = false;
        private Vector3 laneLinkPos = Vector3.zero;
        private Texture2D iTSLogo;
        private int laneToMerge;

        private HashSet<TSLaneInfo> selectedLanes = new HashSet<TSLaneInfo>();
        private HashSet<TSLaneConnector> selectedConnectors = new HashSet<TSLaneConnector>();


        public void OnEnable(Object target, SerializedObject serializedObject)
        {
            this.serializedObject = serializedObject;
            tempLanes = new HashSet<DrawingData>();
            GetMainManagerData();
            manager = (TSMainManager) target;
            if (manager.GetComponent<TSTrafficLightCheck>() == null)
                manager.gameObject.AddComponent<TSTrafficLightCheck>();
            if (manager.lanes == null || manager.lanes.Length == 0)
            {
                ResetLanes();
            }

            vehicleTypesNames = System.Enum.GetNames(typeof(TSLaneInfo.VehicleType));
            vehicleTypesSelected = new bool[vehicleTypesNames.Length];
            iTSLogo = AssetDatabase.LoadAssetAtPath("Assets/iTS/Traffic System/Required/iTSLogo/iTSLogo.png",
                typeof(Texture2D)) as Texture2D;

            MultipleConnectorsSelection();
            MultipleLanesSelection();
            EditorApplication.update += CheckLanes;
        }

        public void OnDisable()
        {
            EditorApplication.update -= CheckLanes;
        }

        private void GetMainManagerData()
        {
            string path = GetiTSDirectory();
            mainData = (TSMainManagerData) AssetDatabase.LoadAssetAtPath(path + "iTSMainData.asset",
                typeof(TSMainManagerData));

            if (!mainData)
            {
                mainData = ScriptableObject.CreateInstance<TSMainManagerData>();

                AssetDatabase.CreateAsset(mainData, path + "iTSMainData.asset");
            }
        }


        private void ResetLanes()
        {
            manager.lanes = new TSLaneInfo[0];
        }


        public void OnInspectorGUI()
        {
            EditorUtility.ClearProgressBar();
            if (GUILayout.Button("Open Lane Editor"))
            {
                TSManagerEditorWindow.Init(manager);
                Selection.activeGameObject = null;
            }

            GUILayout.Space(10);
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.UpperCenter;
            style.imagePosition = ImagePosition.ImageAbove;
            Rect logoRect = GUILayoutUtility.GetRect(0, 100);
            EditorGUI.LabelField(logoRect, new GUIContent(iTSLogo), style);
            GUILayout.Label("Version 2.1.0", style);
            GUILayout.Space(10);
            GUILayout.BeginVertical("Road Data", GUI.skin.box);
            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save", GUILayout.Width(60)))
            {
                string path = EditorUtility.SaveFilePanel("Save Road Data as", Application.dataPath, "RoadData", "json");
                if (path.Length != 0)
                {
                    Save(path);
                    return;
                }
            }

            if (GUILayout.Button("Load", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFilePanel("Load Road Data", Application.dataPath, "json");
                if (path.Length != 0)
                {
                    Load(path);
                    ProcessJunctions();
                    return;
                }
            }

            if (GUILayout.Button("Clear", GUILayout.Width(60)))
            {
                ClearDataChecked();
                return;
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.EndVertical();

            Color defaultGuiColor = GUI.backgroundColor;
            if (!manager.junctionsProcessed)
            {
                GUI.backgroundColor = Color.red;
            }
            else GUI.backgroundColor = Color.green;


            if (GUILayout.Button("Process junctions", GUILayout.Height(25)))
            {
                ProcessJunctions();
            }
            GUI.backgroundColor = defaultGuiColor;
            
            AutoconnectLanesGUI();

            manager.menuSelection = GUILayout.Toolbar(manager.menuSelection, toolbar1Contents);

            switch (manager.menuSelection)
            {
                case 0:
                    addConnector1 = false;
                    manager.connectionsMenuSelection = 0;
                    removeConnector = false;
                    removeConnectorPoint = false;

                    LanesGUI();
                    break;
                case 1:
                    addLane1 = false;
                    removeLane = false;
                    removeLanePoint = false;
                    manager.laneMenuSelection = 0;
                    ConnectionsGUI();
                    break;
                case 2:
                    Settings();
                    break;
            }

            GUILayout.Label("Total lanes: " + manager.lanes.Length.ToString());


            /*************************************************************************************************
             ************************************DEBUG CODE***************************************************
             *************************************************************************************************/
            /*if (currentConnectorSelected >= 0 && currentLaneSelected >= 0 &&
                currentConnectorSelected < manager.lanes[currentLaneSelected].connectors.Length &&
                currentLaneSelected < manager.lanes.Length)
            {
                serializedObject.Update();
                SerializedProperty property = this.serializedObject.FindProperty("lanes");

                SerializedProperty connector =
                    property.GetArrayElementAtIndex(currentLaneSelected).FindPropertyRelative("connectors");
                EditorGUILayout.PropertyField(connector.GetArrayElementAtIndex(currentConnectorSelected), true);
                serializedObject.ApplyModifiedProperties();
            }*/

            //else 
            /*if (currentLaneSelected < manager.lanes.Length && currentConnectorSelected >= 0 && currentLaneSelected >= 0)
            {
                SerializedProperty property = this.serializedObject.FindProperty("lanes");

                EditorGUILayout.PropertyField(property.GetArrayElementAtIndex(currentLaneSelected), true);
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();*/
            /*************************************************************************************************
             ************************************DEBUG CODE***************************************************
             *************************************************************************************************/
            ModuleCheckAndInstaller.ShowModulesOnGUI(manager);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(manager);
            }
        }

        private void AutoconnectLanesGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.HelpBox("Auto connect lanes(Preview)", MessageType.Info);
            EditorGUILayout.Space(5);
            mainData.autoLaneConnectionSettings.maxDistance = EditorGUILayout.FloatField(new GUIContent("Max Distance", "The max distance to search for near lanes to connect"), mainData.autoLaneConnectionSettings.maxDistance);
            mainData.autoLaneConnectionSettings.maxAngle = EditorGUILayout.FloatField(new GUIContent("Max Angle", "The max angle of the direction between 2 lanes to connect, this is to avoid connecting lanes that goes in opposites direction"), mainData.autoLaneConnectionSettings.maxAngle);
            if (GUILayout.Button("Auto connect lanes", GUILayout.Height(25)))
            {
                Undo.RecordObject(manager, "Auto Connect lanes");
                manager.AutoConnectNearLanes(mainData.autoLaneConnectionSettings.maxDistance,
                    mainData.autoLaneConnectionSettings.maxAngle);
                EditorUtility.SetDirty(manager);
                serializedObject.Update();
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
        }

        void ClearDataChecked()
        {
            int decision = EditorUtility.DisplayDialogComplex("Warning!",
                "You are about to delete all the road data of this scene, if you have not saved the road data and don't want to loose your work press NO and save your data first!,\n Do you want to continue to clear all road data?",
                "Yes", "No", "Cancel");
            if (mainData.enableUndoForCleaingRoadData)
            {
                if (manager.lanes.Length > 25)
                {
                    int result = EditorUtility.DisplayDialogComplex("Undo Warning!",
                        "The undo process would take several seconds to minutes, would you like to continue with Undo?",
                        "Yes", "No", "Cancel");
                    switch (result)
                    {
                        case 0:
                            Undo.RecordObject(manager, "Clear road data");
                            break;
                        case 1:
                            break;
                        case 2:
                            return;
                    }
                }
                else
                {
                    Undo.RecordObject(manager, "Clear road data");
                }
            }

            if (decision == 0)
            {
                manager.Clear();
            }
        }


        bool[] vehicleTypesSelected;
        string[] vehicleTypesNames;

        private void LanesGUI()
        {
            GUILayout.Space(25);

            GUILayout.BeginHorizontal();
            GUILayout.Space(Screen.width / 2 - 210);
            manager.laneMenuSelection =
                GUILayout.Toolbar(manager.laneMenuSelection, toolbar2Contents, GUILayout.Width(400));
            GUILayout.EndHorizontal();

            switch (manager.laneMenuSelection)
            {
                case 0:
                    removeLanePoint = false;
                    addLane1 = GUILayout.Toggle(addLane1, "Create", EditorStyles.miniButton);
                    if (addLane1) removeLane = false;

                    removeLane = GUILayout.Toggle(removeLane, "Remove", EditorStyles.miniButton);
                    if (removeLane)
                        addLane1 = false;
                    break;
                case 1:
                    removeLanePoint = false;
                    addLane1 = false;
                    removeLane = false;
                    addLaneLink = GUILayout.Toggle(addLaneLink, "Create", EditorStyles.miniButton);
                    if (addLaneLink) removeLaneLink = false;

                    removeLaneLink = GUILayout.Toggle(removeLaneLink, "Remove", EditorStyles.miniButton);
                    if (removeLaneLink)
                        addLaneLink = false;
                    break;
                case 2:
                    EditorGUILayout.HelpBox("Hold Shift to add points at the end of the lane", MessageType.Info);
                    removeLanePoint = GUILayout.Toggle(removeLanePoint, "Remove", EditorStyles.miniButton);
                    addLane1 = false;
                    removeLane = false;
                    break;
                case 3:
                    EditorGUILayout.HelpBox(
                        "Hold Shift + left mouse button (drag also) to position the selection bounding box/sphere",
                        MessageType.Info);
                    if (GUI.changed)
                        MultipleLanesSelection();
                    EditSettingsMultipleLanes();
                    break;
            }

            //Lane selection
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Current Selected Lane: ");
            currentLaneSelected = EditorGUILayout.IntField(currentLaneSelected);
            if (currentLaneSelected >= manager.lanes.Length) currentLaneSelected = manager.lanes.Length - 1;
            if (GUILayout.Button("Goto selected lane") && currentLaneSelected != -1)
            {
                if (SceneView.sceneViews.Count > 0)
                {
                    SceneView currenScene = (SceneView) SceneView.sceneViews[0];
                    int middlePoint = manager.lanes[currentLaneSelected].middlePoints.Count / 2;
                    currenScene.pivot = new Vector3(manager.lanes[currentLaneSelected].middlePoints[middlePoint].x,
                        currenScene.pivot.y, manager.lanes[currentLaneSelected].middlePoints[middlePoint].z);
                }
            }

            if (GUILayout.Button("Delete") && currentLaneSelected != -1)
            {
                DeleteLanesCheck();
            }

            if (GUILayout.Button("Swap") && currentLaneSelected != -1)
            {
                if (manager.lanes[currentLaneSelected].connectors.Length > 0)
                {
                    if (EditorUtility.DisplayDialog("Warning!",
                        "If you swap this lane direction, all connectors of this lane would be deleted, do you want to continue?",
                        "Yes", "No"))
                    {
                        Undo.RecordObject(manager, "Swap Lane Direction");
                        while (manager.lanes[currentLaneSelected].connectors.Length > 0)
                            manager.RemoveConnector(currentLaneSelected, 0);
                        manager.SwapLaneDirection(currentLaneSelected);
                    }
                }
                else
                {
                    Undo.RecordObject(manager, "Swap Lane Direction");
                    manager.SwapLaneDirection(currentLaneSelected);
                }
            }

            GUILayout.Label("Merge Lane: ");
            laneToMerge = EditorGUILayout.IntField(laneToMerge);
            if (GUILayout.Button("Merge") && currentLaneSelected != -1)
            {
                Undo.RecordObject(manager, "Lane Merge");
                manager.MergeLanes(currentLaneSelected, laneToMerge);
                manager.RemoveLane(laneToMerge, OnLaneDeleted);
                RefreshPoints(manager.lanes[currentLaneSelected].conectorA,
                    manager.lanes[currentLaneSelected].conectorB,
                    manager.lanes[currentLaneSelected].middlePoints, ref manager.lanes[currentLaneSelected].points,
                    ref manager.lanes[currentLaneSelected].totalDistance, false);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (manager.laneMenuSelection != 3 && currentLaneSelected != -1 &&
                currentLaneSelected < manager.lanes.Length)
            {
                manager.lanes[currentLaneSelected].laneWidth = EditorGUILayout.Slider("Width",
                    manager.lanes[currentLaneSelected].laneWidth, 0f, 10f);
                manager.lanes[currentLaneSelected].maxSpeed =
                    EditorGUILayout.FloatField("Max Speed", manager.lanes[currentLaneSelected].maxSpeed);
                manager.lanes[currentLaneSelected].trafficDensity = EditorGUILayout.Slider("Max Density",
                    manager.lanes[currentLaneSelected].trafficDensity, 0f, 1f);
                manager.lanes[currentLaneSelected].maxTotalOccupation = EditorGUILayout.Slider(
                    new GUIContent("Max Total Ocupation (%)",
                        "This values is in percentage, so to have full max Ocupation should be 100 (100%)"),
                    manager.lanes[currentLaneSelected].maxTotalOccupation, 50f, 1000f);
                VehicleTypeGUI(ref manager.lanes[currentLaneSelected].vehicleType);
                GUILayout.Label("Total lane distance: " + manager.lanes[currentLaneSelected].totalDistance);
            }


            if (GUI.changed)
            {
                SceneView.RepaintAll();
                EditorUtility.SetDirty(manager);
            }
        }

        private void DeleteLanesCheck()
        {
            if (mainData.enableUndoForLanesDeletion)
            {
                if (manager.lanes.Length - currentLaneSelected > 5)
                {
                    var result = EditorUtility.DisplayDialogComplex("Undo Warning!",
                        "The undo process would take several seconds to minutes, would you like to continue with Undo?",
                        "Yes", "No", "Cancel");

                    if (result >= 2)
                    {
                        return;
                    }

                    if (result == 1)
                    {
                        Undo.RecordObject(manager, "Delete Lane");
                    }
                }
                else
                {
                    Undo.RecordObject(manager, "Delete Lane");
                }
            }

            manager.RemoveLane(currentLaneSelected, OnLaneDeleted);
        }


        private void EditSettingsMultipleLanes()
        {
            SphereBoxSelection();
            GUILayout.BeginHorizontal();
            mainData.batchLCSettings.setLaneWidth =
                EditorGUILayout.Toggle(mainData.batchLCSettings.setLaneWidth, GUILayout.Width(15));
            mainData.batchLCSettings.defaultLaneWidth =
                EditorGUILayout.Slider("Half Width", mainData.batchLCSettings.defaultLaneWidth, 0f, 10f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            mainData.batchLCSettings.setMaxSpeed =
                EditorGUILayout.Toggle(mainData.batchLCSettings.setMaxSpeed, GUILayout.Width(15));
            mainData.batchLCSettings.defaultMaxSpeed =
                EditorGUILayout.FloatField("Max Speed", mainData.batchLCSettings.defaultMaxSpeed);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            mainData.batchLCSettings.setMaxDensity =
                EditorGUILayout.Toggle(mainData.batchLCSettings.setMaxDensity, GUILayout.Width(15));
            mainData.batchLCSettings.defaultMaxDensity =
                EditorGUILayout.Slider("Max Density", mainData.batchLCSettings.defaultMaxDensity, 0f, 1f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            mainData.batchLCSettings.setVehicleTypeLane = EditorGUILayout.Toggle(mainData.batchLCSettings.setVehicleTypeLane, GUILayout.Width(15));
            VehicleTypeGUI(ref mainData.batchLCSettings.defaultVehicleType);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply to all", GUILayout.Width(100)))
            {
                ApplyBatchSettingsToAllLanes();
            }

            if (GUILayout.Button("Apply to selection", GUILayout.Width(140)))
            {
                ApplyBatchSettingsToSelectedLanes();
            }

            GUILayout.EndHorizontal();
        }

        private void ApplyBatchSettingsToAllLanes()
        {
            Undo.RecordObject(manager, "Apply Lanes Batch Settings");
            for (int i = 0; i < manager.lanes.Length; i++)
            {
                if (mainData.batchLCSettings.setLaneWidth)
                {
                    manager.lanes[i].laneWidth = mainData.batchLCSettings.defaultLaneWidth;
                }

                if (mainData.batchLCSettings.setMaxSpeed)
                {
                    manager.lanes[i].maxSpeed = mainData.batchLCSettings.defaultMaxSpeed;
                }

                if (mainData.batchLCSettings.setMaxDensity)
                {
                    manager.lanes[i].trafficDensity = mainData.batchLCSettings.defaultMaxDensity;
                }

                if (mainData.batchLCSettings.setVehicleTypeLane)
                {
                    manager.lanes[i].vehicleType = mainData.batchLCSettings.defaultVehicleType;
                }
            }
        }

        private void ApplyBatchSettingsToSelectedLanes()
        {
            Undo.RecordObject(manager, "Apply Lanes Batch Settings");
            foreach (TSLaneInfo lane in selectedLanes)
            {
                if (mainData.batchLCSettings.setLaneWidth)
                {
                    lane.laneWidth = mainData.batchLCSettings.defaultLaneWidth;
                }

                if (mainData.batchLCSettings.setMaxSpeed)
                {
                    lane.maxSpeed = mainData.batchLCSettings.defaultMaxSpeed;
                }

                if (mainData.batchLCSettings.setMaxDensity)
                {
                    lane.trafficDensity = mainData.batchLCSettings.defaultMaxDensity;
                }

                if (mainData.batchLCSettings.setVehicleTypeLane)
                {
                    lane.vehicleType = mainData.batchLCSettings.defaultVehicleType;
                }
            }
        }

        private void VehicleTypeGUI(ref TSLaneInfo.VehicleType vehicleType)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Vehicle Type:");
            GUILayout.Space(15);
            GetEnumBools(vehicleType);

            for (int i = 0; i < vehicleTypesNames.Length; i++)
            {
                vehicleTypesSelected[i] = EditorGUILayout.Toggle(vehicleTypesNames[i], vehicleTypesSelected[i]);
            }

            SetEnumBools(ref vehicleType);

            if (manager.vehicleTypePresets.Count > 0)
            {
                GUILayout.BeginVertical("Presets", GUI.skin.box);
                GUILayout.Space(15);
                for (int i = 0; i < manager.vehicleTypePresets.Count; i++)
                {
                    if (GUILayout.Button(manager.vehicleTypePresets[i].name))
                    {
                        vehicleType = manager.vehicleTypePresets[i].vehicleType;
                        GUILayout.EndVertical();
                        GUILayout.EndVertical();
                        EditorUtility.SetDirty(manager);
                        return;
                    }
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();
        }


        private void GetEnumBools(TSLaneInfo.VehicleType myEnum)
        {
            if (myEnum.Has(TSLaneInfo.VehicleType.Taxi))
            {
                vehicleTypesSelected[0] = true;
            }
            else vehicleTypesSelected[0] = false;

            if (myEnum.Has(TSLaneInfo.VehicleType.Bus))
            {
                vehicleTypesSelected[1] = true;
            }
            else vehicleTypesSelected[1] = false;

            if (myEnum.Has(TSLaneInfo.VehicleType.Light))
            {
                vehicleTypesSelected[2] = true;
            }
            else vehicleTypesSelected[2] = false;

            if (myEnum.Has(TSLaneInfo.VehicleType.Medium))
            {
                vehicleTypesSelected[3] = true;
            }
            else vehicleTypesSelected[3] = false;

            if (myEnum.Has(TSLaneInfo.VehicleType.Heavy))
            {
                vehicleTypesSelected[4] = true;
            }
            else vehicleTypesSelected[4] = false;

            if (myEnum.Has(TSLaneInfo.VehicleType.Train))
            {
                vehicleTypesSelected[5] = true;
            }
            else vehicleTypesSelected[5] = false;

            if (myEnum.Has(TSLaneInfo.VehicleType.Heavy_Machinery))
            {
                vehicleTypesSelected[6] = true;
            }
            else vehicleTypesSelected[6] = false;

            if (myEnum.Has(TSLaneInfo.VehicleType.Pedestrians))
            {
                vehicleTypesSelected[7] = true;
            }
            else vehicleTypesSelected[7] = false;

            if (myEnum.Has(TSLaneInfo.VehicleType.Racer))
            {
                vehicleTypesSelected[8] = true;
            }
            else vehicleTypesSelected[8] = false;
        }

        private void SetEnumBools(ref TSLaneInfo.VehicleType myEnum)
        {
            if (vehicleTypesSelected[0])
            {
                myEnum = myEnum.Add(TSLaneInfo.VehicleType.Taxi);
            }
            else myEnum = myEnum.Remove(TSLaneInfo.VehicleType.Taxi);

            if (vehicleTypesSelected[1])
            {
                myEnum = myEnum.Add(TSLaneInfo.VehicleType.Bus);
            }
            else myEnum = myEnum.Remove(TSLaneInfo.VehicleType.Bus);

            if (vehicleTypesSelected[2])
            {
                myEnum = myEnum.Add(TSLaneInfo.VehicleType.Light);
            }
            else myEnum = myEnum.Remove(TSLaneInfo.VehicleType.Light);

            if (vehicleTypesSelected[3])
            {
                myEnum = myEnum.Add(TSLaneInfo.VehicleType.Medium);
            }
            else myEnum = myEnum.Remove(TSLaneInfo.VehicleType.Medium);

            if (vehicleTypesSelected[4])
            {
                myEnum = myEnum.Add(TSLaneInfo.VehicleType.Heavy);
            }
            else myEnum = myEnum.Remove(TSLaneInfo.VehicleType.Heavy);

            if (vehicleTypesSelected[5])
            {
                myEnum = myEnum.Add(TSLaneInfo.VehicleType.Train);
            }
            else myEnum = myEnum.Remove(TSLaneInfo.VehicleType.Train);

            if (vehicleTypesSelected[6])
            {
                myEnum = myEnum.Add(TSLaneInfo.VehicleType.Heavy_Machinery);
            }
            else myEnum = myEnum.Remove(TSLaneInfo.VehicleType.Heavy_Machinery);

            if (vehicleTypesSelected[7])
            {
                myEnum = myEnum.Add(TSLaneInfo.VehicleType.Pedestrians);
            }
            else myEnum = myEnum.Remove(TSLaneInfo.VehicleType.Pedestrians);

            if (vehicleTypesSelected[8])
            {
                myEnum = myEnum.Add(TSLaneInfo.VehicleType.Racer);
            }
            else myEnum = myEnum.Remove(TSLaneInfo.VehicleType.Racer);
        }

        private void ConnectionsGUI()
        {
            GUILayout.Space(25);
            GUILayout.BeginHorizontal();
            GUILayout.Space(Screen.width / 2 - 175);
            manager.connectionsMenuSelection =
                GUILayout.Toolbar(manager.connectionsMenuSelection, toolbar3Contents, GUILayout.Width(350));
            GUILayout.EndHorizontal();

            switch (manager.connectionsMenuSelection)
            {
                case 0:
                    bool lastAddConnector = addConnector;
                    addConnector1 = GUILayout.Toggle(addConnector1, "Create", EditorStyles.miniButton);
                    if (lastAddConnector != addConnector1) SceneView.RepaintAll();
                    if (addConnector1)
                    {
                        if (newConnector == null)
                            newConnector = new TSLaneConnector();
                        removeConnector = false;
                    }

                    removeConnector = GUILayout.Toggle(removeConnector, "Remove", EditorStyles.miniButton);
                    if (removeConnector)
                        addConnector1 = false;
                    break;
                case 1:
                    EditorGUILayout.HelpBox("Hold Shift to add points at the end of the connector", MessageType.Info);
                    removeConnectorPoint = GUILayout.Toggle(removeConnectorPoint, "Remove", EditorStyles.miniButton);
                    break;
                case 2:
                    EditorGUILayout.HelpBox(
                        "Hold Shift + left mouse button (drag also) to position the selection bounding box/sphere",
                        MessageType.Info);
                    if (GUI.changed)
                        MultipleConnectorsSelection();
                    EditSettingsMultipleConnectors();
                    break;
            }


            GUILayout.BeginVertical(GUI.skin.box);
            //Lane Selection
            GUILayout.BeginHorizontal();
            GUILayout.Label("Current Selected Lane: ");
            currentLaneSelected = EditorGUILayout.IntField(currentLaneSelected);
            if (currentLaneSelected >= manager.lanes.Length) currentLaneSelected = manager.lanes.Length - 1;
            if (GUILayout.Button("Goto selected lane") && currentLaneSelected != -1)
            {
                if (SceneView.sceneViews.Count > 0)
                {
                    SceneView currenScene = (SceneView) SceneView.sceneViews[0];
                    currenScene.pivot = new Vector3(manager.lanes[currentLaneSelected].conectorB.x, currenScene.pivot.y,
                        manager.lanes[currentLaneSelected].conectorB.z);
                }
            }

            if (GUILayout.Button("Delete") && currentLaneSelected != -1)
            {
                DeleteLanesCheck();
            }

            GUILayout.EndHorizontal();

            //Connector Selection
            GUILayout.BeginHorizontal();
            GUILayout.Label("Current Selected Connector: ");
            currentConnectorSelected = EditorGUILayout.IntField(currentConnectorSelected);
            if (manager.lanes.Length > 0 && currentLaneSelected != -1 &&
                currentConnectorSelected >= manager.lanes[currentLaneSelected].connectors.Length)
            {
                if (currentLaneSelected > manager.lanes.Length) currentLaneSelected = manager.lanes.Length - 1;
                currentConnectorSelected = manager.lanes[currentLaneSelected].connectors.Length - 1;
            }

            if (GUILayout.Button("Goto selected connector") && currentLaneSelected != -1 &&
                currentConnectorSelected != -1)
            {
                if (SceneView.sceneViews.Count > 0)
                {
                    SceneView currenScene = (SceneView) SceneView.sceneViews[0];
                    int middlePoint = manager.lanes[currentLaneSelected].connectors[currentConnectorSelected]
                        .middlePoints
                        .Count / 2;
                    currenScene.pivot = new Vector3(
                        manager.lanes[currentLaneSelected].connectors[currentConnectorSelected]
                            .middlePoints[middlePoint].x,
                        currenScene.pivot.y,
                        manager.lanes[currentLaneSelected].connectors[currentConnectorSelected]
                            .middlePoints[middlePoint]
                            .z);
                }
            }

            if (GUILayout.Button("Delete") && currentLaneSelected != -1 && currentConnectorSelected != -1)
            {
                Undo.RecordObject(manager, "Delete Connector");
                manager.RemoveConnector(currentLaneSelected, currentConnectorSelected);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (manager.connectionsMenuSelection != 2 && currentLaneSelected != -1 &&
                currentLaneSelected < manager.lanes.Length && currentConnectorSelected != -1 &&
                currentConnectorSelected < manager.lanes[currentLaneSelected].connectors.Length)
            {
                manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].direction =
                    (TSLaneConnector.Direction) EditorGUILayout.EnumPopup("Direction",
                        manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].direction);
                manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].forcedStop =
                    EditorGUILayout.Toggle(
                        new GUIContent("Forced stop",
                            "If this is enabled, all cars would always stop at the start of this connector, and then would continue, simulating an stop sign"),
                        manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].forcedStop);
                manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].priority =
                    EditorGUILayout.IntSlider(
                        new GUIContent("Pass priority",
                            "The priority the cars would have if they are waiting on this connector"),
                        manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].priority, 0, 100);
                manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].maxSpeed =
                    EditorGUILayout.FloatField("Max Speed", manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].maxSpeed);
                VehicleTypeGUI(ref manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].vehicleType);
            }


            if (GUI.changed)
            {
                EditorUtility.SetDirty(manager);
                SceneView.RepaintAll();
            }
        }

        private void SphereBoxSelection()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label("Selection type");
            mainData.useSphereSelection = GUILayout.Toggle(mainData.useSphereSelection, "Sphere",
                EditorStyles.miniButtonLeft, GUILayout.Width(70));
            mainData.useSphereSelection = !GUILayout.Toggle(!mainData.useSphereSelection, "Box",
                EditorStyles.miniButtonRight, GUILayout.Width(70));
            GUILayout.EndVertical();
        }

        private void EditSettingsMultipleConnectors()
        {
            SphereBoxSelection();

            GUILayout.BeginHorizontal();
            mainData.batchLCSettings.setDirection =
                EditorGUILayout.Toggle(mainData.batchLCSettings.setDirection, GUILayout.Width(15));
            mainData.batchLCSettings.defaultDirection =
                (TSLaneConnector.Direction) EditorGUILayout.EnumPopup("Direction",
                    mainData.batchLCSettings.defaultDirection);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            mainData.batchLCSettings.setForcedStop =
                EditorGUILayout.Toggle(mainData.batchLCSettings.setForcedStop, GUILayout.Width(15));
            mainData.batchLCSettings.defaultForcedStop =
                EditorGUILayout.Toggle("Forced Stop", mainData.batchLCSettings.defaultForcedStop);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            mainData.batchLCSettings.setMaxSpeed =
                EditorGUILayout.Toggle(mainData.batchLCSettings.setMaxSpeed, GUILayout.Width(15));
            mainData.batchLCSettings.defaultMaxSpeed =
                EditorGUILayout.FloatField("Max Speed", mainData.batchLCSettings.defaultMaxSpeed);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            mainData.batchLCSettings.setPassPriority =
                EditorGUILayout.Toggle(mainData.batchLCSettings.setPassPriority, GUILayout.Width(15));
            mainData.batchLCSettings.defaultPassPriority = EditorGUILayout.IntSlider("Pass Priority",
                mainData.batchLCSettings.defaultPassPriority, 0, 100);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            mainData.batchLCSettings.setVehicleTypeConnector =
                EditorGUILayout.Toggle(mainData.batchLCSettings.setVehicleTypeConnector, GUILayout.Width(15));
            VehicleTypeGUI(ref mainData.batchLCSettings.defaultVehicleType);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply to all", GUILayout.Width(100)))
            {
                ApplyBatchSettingsToAllConnectors();
            }

            if (GUILayout.Button("Apply to Selection", GUILayout.Width(130)))
            {
                ApplyBatchSettingsToSelectedConnectors();
            }

            GUILayout.EndHorizontal();
        }


        private void ApplyBatchSettingsToAllConnectors()
        {
            Undo.RecordObject(manager, "Apply Connectors Batch Settings");
            for (int i = 0; i < manager.lanes.Length; i++)
            {
                for (int y = 0; y < manager.lanes[i].connectors.Length; y++)
                {
                    var connector = manager.lanes[i].connectors[y];
                    ApplyConnectorSettings(connector);
                }
            }
        }

        private void ApplyConnectorSettings(TSLaneConnector connector)
        {
            if (mainData.batchLCSettings.setDirection)
            {
                connector.direction = mainData.batchLCSettings.defaultDirection;
            }

            if (mainData.batchLCSettings.setForcedStop)
            {
                connector.forcedStop = mainData.batchLCSettings.defaultForcedStop;
            }

            if (mainData.batchLCSettings.setPassPriority)
            {
                connector.priority = mainData.batchLCSettings.defaultPassPriority;
            }

            if (mainData.batchLCSettings.setVehicleTypeConnector)
            {
                connector.vehicleType = mainData.batchLCSettings.defaultVehicleType;
            }

            if (mainData.batchLCSettings.setMaxSpeed)
            {
                connector.maxSpeed = mainData.batchLCSettings.defaultMaxSpeed;
            }
        }

        private void ApplyBatchSettingsToSelectedConnectors()
        {
            Undo.RecordObject(manager, "Apply Connectors Batch Settings");
            foreach (TSLaneConnector connector in selectedConnectors)
            {
               ApplyConnectorSettings(connector);
            }
        }


        private void Settings()
        {
            GUI.changed = false;
            manager.visualLinesWidth = EditorGUILayout.Slider("Lines width", manager.visualLinesWidth, 0.1f, 15f);
            manager.resolution = EditorGUILayout.Slider("Resolution lane", manager.resolution, 0.1f, 15f);
            manager.laneCurveSpeedMultiplier =
                EditorGUILayout.Slider("Lane curve speed multiplier", manager.laneCurveSpeedMultiplier, 0.01f, 5f);
            manager.resolutionConnectors =
                EditorGUILayout.Slider("Resolution connector", manager.resolutionConnectors, 0.1f, 4f);
            manager.connectorsCurveSpeedMultiplier = EditorGUILayout.Slider("Connector curve speed multiplier",
                manager.connectorsCurveSpeedMultiplier, 0.01f, 5f);
            bool guiChange = GUI.changed;
            manager.scaleFactor =
                EditorGUILayout.Slider(
                    new GUIContent("Scale factor", "This is the scale factor for all the visual on scene editor toold"),
                    manager.scaleFactor, 0.01f, 5f);
            GUI.changed = guiChange;
            if (GUILayout.Button("Reprocess lane points"))
            {
                RefreshLanes(true);
                ProcessJunctions();
                manager.junctionsProcessed = true;
                EditorUtility.SetDirty(manager);
                SceneView.RepaintAll();
                GUI.changed = false;
            }

            if (GUILayout.Button("Reprocess connectors points"))
            {
                RefreshConnectors(true);
                ProcessJunctions();
                manager.junctionsProcessed = true;
                EditorUtility.SetDirty(manager);
                SceneView.RepaintAll();
                GUI.changed = false;
            }

            if (GUI.changed)
            {
                manager.junctionsProcessed = false;
                EditorUtility.SetDirty(manager);
                SceneView.RepaintAll();
            }

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Default vehicle type");
            GUILayout.Space(15);

            GetEnumBools(manager.defaultVehicleType);

            for (int i = 0; i < vehicleTypesNames.Length; i++)
            {
                vehicleTypesSelected[i] = EditorGUILayout.Toggle(vehicleTypesNames[i], vehicleTypesSelected[i]);
            }

            SetEnumBools(ref manager.defaultVehicleType);

            if (GUILayout.Button("Add Selected as preset"))
            {
                manager.vehicleTypePresets.Add(new TSMainManager.VehicleTypePresets());
                manager.vehicleTypePresets[manager.vehicleTypePresets.Count - 1].vehicleType =
                    manager.defaultVehicleType;
                GetVehicleTypePresetNames(manager.vehicleTypePresets.Count - 1);
            }

            for (int i = 0; i < manager.vehicleTypePresets.Count; i++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    manager.vehicleTypePresets.RemoveAt(i);
                    break;
                }

                manager.vehicleTypePresets[i].vehicleType = (TSLaneInfo.VehicleType) EditorGUILayout.MaskField(
                    "Vehicle preset#" + i, (int) manager.vehicleTypePresets[i].vehicleType,
                    System.Enum.GetNames(typeof(TSLaneInfo.VehicleType)));
                if (GUI.changed)
                    GetVehicleTypePresetNames(i);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();


            EditorGUI.BeginChangeCheck();
            mainData.enableUndoForCleaingRoadData = EditorGUILayout.Toggle("Enable undo for clearing Road Data?",
                mainData.enableUndoForCleaingRoadData);
            mainData.enableUndoForLanesDeletion =
                EditorGUILayout.Toggle("Enable undo for deleting lanes?", mainData.enableUndoForLanesDeletion);
            mainData.allowDeadEndLanes = EditorGUILayout.Toggle("Allow Dead End Lanes?", mainData.allowDeadEndLanes);
            mainData.calculateNerbyPointsForPlayerFinding = EditorGUILayout.Toggle(
                "Calculate Nerby Points For Player Finding?", mainData.calculateNerbyPointsForPlayerFinding);
            mainData.nearbyPointsRadius =
                EditorGUILayout.FloatField("Nearby points radius", mainData.nearbyPointsRadius);
            mainData.maxLaneCachedPerFrame =
                EditorGUILayout.FloatField("Max lanes cached per frame", mainData.maxLaneCachedPerFrame);
            if (GUI.changed || EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(mainData);
        }

        private void GetVehicleTypePresetNames(int y)
        {
            GetEnumBools(manager.vehicleTypePresets[y].vehicleType);
            manager.vehicleTypePresets[y].name = "";
            if (manager.vehicleTypePresets[y].vehicleType == (TSLaneInfo.VehicleType) (-1))
                manager.vehicleTypePresets[y].name = "Everything";
            else if ((int) (manager.vehicleTypePresets[y].vehicleType) == 0)
                manager.vehicleTypePresets[y].name = "Nothing";
            else
            {
                for (int i = 0; i < vehicleTypesSelected.Length; i++)
                {
                    if (vehicleTypesSelected[i])
                        manager.vehicleTypePresets[y].name += vehicleTypesNames[i] + " ";
                }
            }
        }


        bool dontDoAnything = false;


        private void MultipleLanesSelection()
        {
            selectedLanes.Clear();
            if (mainData.useSphereSelection)
            {
                for (int i = 0; i < manager.lanes.Length; i++)
                {
                    if (Vector3.Distance(mainData.MultipleSelectionOrigin, manager.lanes[i].conectorA) <
                        Mathf.Abs(mainData.MultipleSelectionRadius) &&
                        Vector3.Distance(mainData.MultipleSelectionOrigin, manager.lanes[i].conectorB) <
                        Mathf.Abs(mainData.MultipleSelectionRadius) &&
                        Vector3.Distance(mainData.MultipleSelectionOrigin,
                            manager.lanes[i].points[manager.lanes[i].points.Length / 2].point) <
                        Mathf.Abs(mainData.MultipleSelectionRadius))
                    {
                        selectedLanes.Add(manager.lanes[i]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < manager.lanes.Length; i++)
                {
                    if (mainData.MultipleSelectionBounds.Contains(manager.lanes[i].conectorA) &&
                        mainData.MultipleSelectionBounds.Contains(manager.lanes[i].conectorB) &&
                        mainData.MultipleSelectionBounds.Contains(manager.lanes[i]
                            .points[manager.lanes[i].points.Length / 2].point))
                    {
                        selectedLanes.Add(manager.lanes[i]);
                    }
                }
            }
        }

        private void MultipleConnectorsSelection()
        {
            selectedConnectors.Clear();
            if (mainData.useSphereSelection)
            {
                for (int i = 0; i < manager.lanes.Length; i++)
                {
                    for (int y = 0; y < manager.lanes[i].connectors.Length; y++)
                    {
                        if (Vector3.Distance(mainData.MultipleSelectionOrigin,
                                manager.lanes[i].connectors[y].conectorA) <
                            Mathf.Abs(mainData.MultipleSelectionRadius) &&
                            Vector3.Distance(mainData.MultipleSelectionOrigin,
                                manager.lanes[i].connectors[y].conectorB) <
                            Mathf.Abs(mainData.MultipleSelectionRadius) &&
                            Vector3.Distance(mainData.MultipleSelectionOrigin,
                                manager.lanes[i].connectors[y].points[manager.lanes[i].connectors[y].points.Length / 2]
                                    .point) < Mathf.Abs(mainData.MultipleSelectionRadius))
                        {
                            selectedConnectors.Add(manager.lanes[i].connectors[y]);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < manager.lanes.Length; i++)
                {
                    for (int y = 0; y < manager.lanes[i].connectors.Length; y++)
                    {
                        if (mainData.MultipleSelectionBounds.Contains(manager.lanes[i].connectors[y].conectorA) &&
                            mainData.MultipleSelectionBounds.Contains(manager.lanes[i].connectors[y].conectorB) &&
                            mainData.MultipleSelectionBounds.Contains(manager.lanes[i].connectors[y]
                                .points[manager.lanes[i].connectors[y].points.Length / 2].point))
                        {
                            selectedConnectors.Add(manager.lanes[i].connectors[y]);
                        }
                    }
                }
            }
        }


        private void UpdateMultiLaneConnectorSelection(int controlID)
        {
            bool selectConnector = manager.connectionsMenuSelection == 2 && manager.menuSelection == 1;
            bool selectLanes = manager.laneMenuSelection == 3 && manager.menuSelection == 0;

            if (!selectLanes && !selectConnector)
            {
                return;
            }

            DrawSelectionRectangle(selectConnector, selectLanes);

            if (Event.current.button != 0)
            {
                return;
            }

            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    if (Event.current.shift)
                    {
                        GUIUtility.hotControl = controlID;
                        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        RaycastHit hit = new RaycastHit();
                        bool rayHit = Physics.Raycast(ray, out hit);

                        if (rayHit)
                        {
                            mainData.MultipleSelectionBounds.center = mainData.MultipleSelectionOrigin = hit.point;
                        }
                        else
                        {
                            mainData.MultipleSelectionBounds.center =
                                mainData.MultipleSelectionOrigin = ray.origin + ray.direction * 10f;
                        }

                        //SceneView.RepaintAll();
                    }

                    break;
                case EventType.MouseUp:
                    GUIUtility.hotControl = controlID;
                    if (selectConnector)
                    {
                        MultipleConnectorsSelection();
                    }

                    if (selectLanes)
                    {
                        MultipleLanesSelection();
                    }

                    //SceneView.RepaintAll();
                    GUIUtility.hotControl = 0;
                    break;
            }
        }


        private readonly Color _cubeColor = new Color(1, 0, 0, 0.15f);
        private readonly Color _outlineColor = new Color(1, 1, 1, 1);

        private void DrawSelectionRectangle(bool selectConnector, bool selectLanes)
        {
            if (!mainData.useSphereSelection)
            {
                Handles.DrawSolidRectangleWithOutline(new Vector3[]
                {
                    new Vector3(mainData.MultipleSelectionBounds.min.x, mainData.MultipleSelectionBounds.min.y,
                        mainData.MultipleSelectionBounds.min.z),
                    new Vector3(mainData.MultipleSelectionBounds.min.x, mainData.MultipleSelectionBounds.max.y,
                        mainData.MultipleSelectionBounds.min.z),
                    new Vector3(mainData.MultipleSelectionBounds.max.x, mainData.MultipleSelectionBounds.max.y,
                        mainData.MultipleSelectionBounds.min.z),
                    new Vector3(mainData.MultipleSelectionBounds.max.x, mainData.MultipleSelectionBounds.min.y,
                        mainData.MultipleSelectionBounds.min.z),
                }, _cubeColor, _outlineColor);

                Handles.DrawSolidRectangleWithOutline(new Vector3[]
                {
                    new Vector3(mainData.MultipleSelectionBounds.min.x, mainData.MultipleSelectionBounds.min.y,
                        mainData.MultipleSelectionBounds.min.z),
                    new Vector3(mainData.MultipleSelectionBounds.min.x, mainData.MultipleSelectionBounds.min.y,
                        mainData.MultipleSelectionBounds.max.z),
                    new Vector3(mainData.MultipleSelectionBounds.min.x, mainData.MultipleSelectionBounds.max.y,
                        mainData.MultipleSelectionBounds.max.z),
                    new Vector3(mainData.MultipleSelectionBounds.min.x, mainData.MultipleSelectionBounds.max.y,
                        mainData.MultipleSelectionBounds.min.z),
                }, _cubeColor, _outlineColor);

                Handles.DrawSolidRectangleWithOutline(new Vector3[]
                {
                    new Vector3(mainData.MultipleSelectionBounds.min.x, mainData.MultipleSelectionBounds.min.y,
                        mainData.MultipleSelectionBounds.min.z),
                    new Vector3(mainData.MultipleSelectionBounds.max.x, mainData.MultipleSelectionBounds.min.y,
                        mainData.MultipleSelectionBounds.min.z),
                    new Vector3(mainData.MultipleSelectionBounds.max.x, mainData.MultipleSelectionBounds.min.y,
                        mainData.MultipleSelectionBounds.max.z),
                    new Vector3(mainData.MultipleSelectionBounds.min.x, mainData.MultipleSelectionBounds.min.y,
                        mainData.MultipleSelectionBounds.max.z),
                }, _cubeColor, _outlineColor);


                Handles.DrawSolidRectangleWithOutline(new Vector3[]
                {
                    new Vector3(mainData.MultipleSelectionBounds.max.x, mainData.MultipleSelectionBounds.max.y,
                        mainData.MultipleSelectionBounds.max.z),
                    new Vector3(mainData.MultipleSelectionBounds.max.x, mainData.MultipleSelectionBounds.max.y,
                        mainData.MultipleSelectionBounds.min.z),
                    new Vector3(mainData.MultipleSelectionBounds.min.x, mainData.MultipleSelectionBounds.max.y,
                        mainData.MultipleSelectionBounds.min.z),
                    new Vector3(mainData.MultipleSelectionBounds.min.x, mainData.MultipleSelectionBounds.max.y,
                        mainData.MultipleSelectionBounds.max.z),
                }, _cubeColor, _outlineColor);


                Handles.DrawSolidRectangleWithOutline(new Vector3[]
                {
                    new Vector3(mainData.MultipleSelectionBounds.max.x, mainData.MultipleSelectionBounds.max.y,
                        mainData.MultipleSelectionBounds.max.z),
                    new Vector3(mainData.MultipleSelectionBounds.max.x, mainData.MultipleSelectionBounds.min.y,
                        mainData.MultipleSelectionBounds.max.z),
                    new Vector3(mainData.MultipleSelectionBounds.min.x, mainData.MultipleSelectionBounds.min.y,
                        mainData.MultipleSelectionBounds.max.z),
                    new Vector3(mainData.MultipleSelectionBounds.min.x, mainData.MultipleSelectionBounds.max.y,
                        mainData.MultipleSelectionBounds.max.z),
                }, _cubeColor, _outlineColor);

                Handles.DrawSolidRectangleWithOutline(new Vector3[]
                {
                    new Vector3(mainData.MultipleSelectionBounds.max.x, mainData.MultipleSelectionBounds.max.y,
                        mainData.MultipleSelectionBounds.max.z),
                    new Vector3(mainData.MultipleSelectionBounds.max.x, mainData.MultipleSelectionBounds.min.y,
                        mainData.MultipleSelectionBounds.max.z),
                    new Vector3(mainData.MultipleSelectionBounds.max.x, mainData.MultipleSelectionBounds.min.y,
                        mainData.MultipleSelectionBounds.min.z),
                    new Vector3(mainData.MultipleSelectionBounds.max.x, mainData.MultipleSelectionBounds.max.y,
                        mainData.MultipleSelectionBounds.min.z)
                }, _cubeColor, _outlineColor);

                Vector3 size = mainData.MultipleSelectionBounds.min;
                boxBoundsHandle.center = mainData.MultipleSelectionBounds.center;
                boxBoundsHandle.size = mainData.MultipleSelectionBounds.size;
                EditorGUI.BeginChangeCheck();
                boxBoundsHandle.DrawHandle();
                Vector3 size1 = mainData.MultipleSelectionBounds.max;
                if (EditorGUI.EndChangeCheck())
                {
                    var bounds = new Bounds(boxBoundsHandle.center, boxBoundsHandle.size);
                    mainData.MultipleSelectionBounds = bounds;
                }

                if (size == mainData.MultipleSelectionBounds.min && size1 == mainData.MultipleSelectionBounds.max)
                {
                    return;
                }

                if (selectConnector)
                {
                    MultipleConnectorsSelection();
                }

                if (selectLanes)
                {
                    MultipleLanesSelection();
                }
            }
            else
            {
                var handleColor = Handles.color;
                Handles.color = _cubeColor;
                Handles.SphereHandleCap(0, mainData.MultipleSelectionOrigin, Quaternion.identity,
                    mainData.MultipleSelectionRadius * 2f, EventType.Repaint);
                Handles.color = handleColor;
                var radius = mainData.MultipleSelectionRadius;
                mainData.MultipleSelectionRadius = Handles.RadiusHandle(Quaternion.identity,
                    mainData.MultipleSelectionOrigin, mainData.MultipleSelectionRadius);

                if (radius == mainData.MultipleSelectionRadius)
                {
                    return;
                }

                if (selectConnector)
                {
                    MultipleConnectorsSelection();
                }

                if (selectLanes)
                {
                    MultipleLanesSelection();
                }
            }
        }

        BoxBoundsHandle boxBoundsHandle = new BoxBoundsHandle();
        int lanesCounter = 0;
        int currentLanesCounter = 0;
        int lastLaneCount = 0;

        private void CheckLanes()
        {
            int i = 0;
            if (lanesCounter >= manager.lanes.Length)
            {
                lanesCounter = 0;
            }

            for (; lanesCounter < manager.lanes.Length; lanesCounter++)
            {
                i = lanesCounter;
                if (currentLanesCounter > mainData.maxLaneCachedPerFrame)
                {
                    currentLanesCounter = 0;
                    break;
                }

                Bounds bounds = new Bounds(manager.lanes[i].conectorA, Vector3.one);
                bounds.Encapsulate(manager.lanes[i].conectorB);
                int midPointIndex = manager.lanes[i].points.Length / 2;

                if (manager.lanes[i].points.Length != 0)
                {
                    bounds.Encapsulate(manager.lanes[i].points[midPointIndex].point);
                }

                for (int ii = 0; ii < manager.lanes[i].connectors.Length; ii++)
                {
                    bounds.Encapsulate(manager.lanes[i].connectors[ii].conectorB);
                }

                Plane[] planes = GeometryUtility.CalculateFrustumPlanes(SceneView.GetAllSceneCameras()[0]);

                List<Vector3[]> connectorsToAdd = new List<Vector3[]>();
                foreach (var connector in manager.lanes[i].connectors)
                {
                    connectorsToAdd.Add(connector.points.Select(point => point.point).ToArray());
                }

                var points = manager.lanes[i].points.Select(point => point.point).ToList();
                if (currentLaneSelected != i)
                {
                    LineUtility.Simplify(points, 0.1f, points);
                }

                var drawingData = new DrawingData
                {
                    laneIndex = i,
                    points = points.ToArray(),
                    connectorsPoints = connectorsToAdd
                };

                tempLanes.Remove(drawingData);
                if (GeometryUtility.TestPlanesAABB(planes, bounds))
                {
                    tempLanes.Add(drawingData);
                }

                currentLanesCounter++;
            }

            if (lastLaneCount == tempLanes.Count)
            {
                return;
            }

            lastLaneCount = tempLanes.Count;
            //SceneView.RepaintAll();
        }

        private bool PointsSelection(int i, int index)
        {
            if (i == currentLaneSelected)
            {
                return true;
            }

            var min = Mathf.Min(manager.lanes[i].points.Length / 4, 10);
            return index % min == 0 || index == manager.lanes[i].points.Length - 1;
        }


        //OnSceneGUI start
        public void OnSceneGUI(SceneView sceneView)
        {
            OnSceneGUI();
        }

        public void OnSceneGUI()
        {
            dontDoAnything = Tools.current == Tool.View || Tools.viewTool == ViewTool.Orbit ||
                             Tools.viewTool == ViewTool.FPS;
            var controlID = GUIUtility.GetControlID(FocusType.Passive);

            UpdateMultiLaneConnectorSelection(controlID);

            var ray = HandleUtility.GUIPointToWorldRay((Event.current.mousePosition));

            var hit = new RaycastHit();
            var rayHit = Physics.Raycast(ray, out hit);

            if (manager.menuSelection == 0 && manager.laneMenuSelection == 2 && Event.current.button == 0 &&
                !dontDoAnything)
            {
                EditPoints(true, controlID, rayHit, hit);
            }

            Handles.color = Color.green;
            var connectorA = false;
            var connectorB = false;
            var selectedLaneForConnector = -1;

            if (addLane1 && dragging)
            {
                Handles.DrawLine(startPoint, hit.point);
            }

            if (manager.lanes == null)
            {
                return;
            }

            foreach (var lane in tempLanes)
            {
                var drawingData = lane;
                if (drawingData.laneIndex >= manager.lanes.Length) continue;


                var laneInfo = manager.lanes[drawingData.laneIndex];

                //Lane Drawing and selection

                if ((drawingData.laneIndex == currentLaneSelected ||
                     (manager.laneMenuSelection == 3 && selectedLanes.Contains(laneInfo))) &&
                    manager.menuSelection == 0)
                {
                    Handles.color = Color.yellow;
                }

                if (laneInfo.middlePoints.Count > 0)
                {
                    if (rayHit && manager.menuSelection == 0 && manager.laneMenuSelection == 0 ||
                        manager.menuSelection == 1 && manager.connectionsMenuSelection == 0)
                    {
                        Action OnFocused = () =>
                        {
                            if (addConnector1)
                            {
                                selectedLaneForConnector = drawingData.laneIndex;
                            }

                            if (!removeLanePoint && !addConnector1)
                            {
                                Handles.color = Color.blue;
                            }
                        };
                        Action<int> OnPointSelected = w =>
                        {
                            currentLaneSelected = drawingData.laneIndex;
                            GUIUtility.hotControl = 0;


                            if (addConnector1)
                            {
                                selectedLaneForConnector = drawingData.laneIndex;
                            }

                            if (w == 0)
                            {
                                connectorA = true;
                            }

                            if (w > 0)
                            {
                                connectorB = true;
                            }

                            GUIUtility.hotControl = controlID;
                        };


                        if (CheckPointsSelection(laneInfo, hit, OnPointSelected, OnFocused))
                        {
                            if (removeLane)
                            {
                                DeleteLanesCheck();
                                tempLanes.Clear();
                                GUIUtility.hotControl = 0;
                                return;
                            }
                        }
                    }

                    Handles.DrawPolyLine(drawingData.points);

                    if (laneInfo.points.Length > 3)
                    {
                        var p = new Vector3[4];
                        var tsLaneInfo = manager.lanes[drawingData.laneIndex];
                        var index = tsLaneInfo.points.Length / 2;
                        var tempDir = Quaternion.LookRotation(
                            (tsLaneInfo.points[index + 1 < laneInfo.points.Length ? index + 1 : index].point -
                             tsLaneInfo.points[index].point));
                        p[0] = tsLaneInfo.points[index].point + tempDir * Vector3.right * laneInfo.laneWidth;
                        p[1] = tsLaneInfo.points[index].point + tempDir * Vector3.forward * 5 * manager.scaleFactor;
                        p[2] = tsLaneInfo.points[index].point + tempDir * -Vector3.right * laneInfo.laneWidth;
                        p[3] = tsLaneInfo.points[index].point;
                        if (laneInfo.totalDistance != 0)
                        {
                            Handles.DrawSolidRectangleWithOutline(p, Handles.color, Handles.color);
                            Handles.Label(p[1], "Lane " + drawingData.laneIndex, EditorStyles.whiteLargeLabel);
                            if (manager.menuSelection == 0 && manager.laneMenuSelection == 1)
                            {
                                Handles.color = Color.blue;
                                //Right
                                var isRight = false;
                                var isLeft = false;
                                if (rayHit && (p[0] - hit.point).magnitude <= 1f * manager.scaleFactor)
                                {
                                    Handles.color = Color.yellow;
                                    isRight = true;
                                    isLeft = false;
                                }

                                Handles.DrawSolidDisc(p[0], Vector3.up, 0.5f * (manager.scaleFactor / 2f));
                                Handles.color = Color.blue;
                                //Left
                                if (rayHit && (p[2] - hit.point).magnitude <= 1f * manager.scaleFactor)
                                {
                                    Handles.color = Color.red;
                                    isLeft = true;
                                    isRight = false;
                                }

                                Handles.DrawSolidDisc(p[2], Vector3.up, 0.5f * (manager.scaleFactor / 2f));
                                Handles.color = Color.green;

                                if (Event.current.type == EventType.MouseDown && (isRight || isLeft) &&
                                    Event.current.button == 0 && (addLaneLink || removeLaneLink) && !dontDoAnything)
                                {
                                    GUIUtility.hotControl = controlID;
                                    if (addLaneLink && !linkLane1Set)
                                    {
                                        linkLane1Set = true;
                                        linkLane1 = drawingData.laneIndex;
                                        linkLane1Right = isRight;
                                        if (isRight)
                                            laneLinkPos = p[0];
                                        else laneLinkPos = p[2];
                                    }

                                    if (removeLaneLink)
                                    {
                                        Undo.RecordObject(manager, "Remove Lane Link");
                                        if (isRight)
                                        {
                                            if (laneInfo.laneLinkRight != -1)
                                            {
                                                if (manager.lanes[laneInfo.laneLinkRight].laneLinkLeft ==
                                                    drawingData.laneIndex)
                                                    manager.lanes[laneInfo.laneLinkRight].laneLinkLeft = -1;
                                                else if (manager.lanes[laneInfo.laneLinkRight].laneLinkRight ==
                                                         drawingData.laneIndex)
                                                    manager.lanes[laneInfo.laneLinkRight].laneLinkRight = -1;
                                            }

                                            laneInfo.laneLinkRight = -1;
                                        }

                                        if (isLeft)
                                        {
                                            if (laneInfo.laneLinkLeft != -1)
                                            {
                                                if (manager.lanes[laneInfo.laneLinkLeft].laneLinkLeft ==
                                                    drawingData.laneIndex)
                                                    manager.lanes[laneInfo.laneLinkLeft].laneLinkLeft = -1;
                                                else if (manager.lanes[laneInfo.laneLinkLeft].laneLinkRight ==
                                                         drawingData.laneIndex)
                                                    manager.lanes[laneInfo.laneLinkLeft].laneLinkRight = -1;
                                            }

                                            laneInfo.laneLinkLeft = -1;
                                        }
                                    }

                                    GUIUtility.hotControl = 0;
                                    Event.current.Use();
                                }

                                if ((Event.current.type == EventType.MouseUp ||
                                     Event.current.type == EventType.MouseUp) &&
                                    (isRight || isLeft) && Event.current.button == 0 && addLaneLink && !dontDoAnything)
                                {
                                    Undo.RecordObject(manager, "Add Lane Link");
                                    GUIUtility.hotControl = controlID;
                                    if (addLaneLink && !linkLane2Set)
                                    {
                                        linkLane2Set = true;
                                        linkLane2 = drawingData.laneIndex;
                                        linkLane2Right = isRight;
                                    }

                                    if (linkLane1Right && !linkLane2Right && linkLane1Set && linkLane2Set)
                                    {
                                        if (manager.lanes[linkLane1] != manager.lanes[linkLane2])
                                        {
                                            manager.lanes[linkLane1].laneLinkRight = linkLane2;
                                            manager.lanes[linkLane2].laneLinkLeft = linkLane1;
                                        }

                                        linkLane1 = -1;
                                        linkLane2 = -1;
                                        linkLane1Set = false;
                                        linkLane2Set = false;
                                    }
                                    else if (!linkLane1Right && linkLane2Right && linkLane1Set && linkLane2Set)
                                    {
                                        if (manager.lanes[linkLane1] != manager.lanes[linkLane2])
                                        {
                                            manager.lanes[linkLane1].laneLinkLeft = linkLane2;
                                            manager.lanes[linkLane2].laneLinkRight = linkLane1;
                                        }

                                        linkLane1 = -1;
                                        linkLane2 = -1;
                                        linkLane1Set = false;
                                        linkLane2Set = false;
                                    }
                                    else if (!linkLane1Right && !linkLane2Right && linkLane1Set && linkLane2Set)
                                    {
                                        if (manager.lanes[linkLane1] != manager.lanes[linkLane2])
                                        {
                                            manager.lanes[linkLane1].laneLinkLeft = linkLane2;
                                            manager.lanes[linkLane2].laneLinkLeft = linkLane1;
                                        }

                                        linkLane1 = -1;
                                        linkLane2 = -1;
                                        linkLane1Set = false;
                                        linkLane2Set = false;
                                    }
                                    else if (linkLane1Right && linkLane2Right && linkLane1Set && linkLane2Set)
                                    {
                                        if (manager.lanes[linkLane1] != manager.lanes[linkLane2])
                                        {
                                            manager.lanes[linkLane1].laneLinkRight = linkLane2;
                                            manager.lanes[linkLane2].laneLinkRight = linkLane1;
                                        }

                                        linkLane1 = -1;
                                        linkLane2 = -1;
                                        linkLane1Set = false;
                                        linkLane2Set = false;
                                    }

                                    GUIUtility.hotControl = 0;
                                    Event.current.Use();
                                }

                                if (linkLane1Set && !linkLane2Set)
                                {
                                    Handles.DrawLine(laneLinkPos, hit.point);
                                }

                                if (laneInfo.laneLinkLeft != -1)
                                {
                                    var p1 = Vector3.zero;
                                    var index1 = (manager.lanes[laneInfo.laneLinkLeft].points.Length) / 2;
                                    var tempDir1 = Quaternion.LookRotation(
                                        (manager.lanes[laneInfo.laneLinkLeft]
                                             .points[
                                                 index1 + 1 < manager.lanes[laneInfo.laneLinkLeft].points.Length
                                                     ? index1 + 1
                                                     : index1].point -
                                         manager.lanes[laneInfo.laneLinkLeft].points[index1].point));

                                    if (manager.lanes[laneInfo.laneLinkLeft].laneLinkRight == drawingData.laneIndex)
                                        p1 = manager.lanes[laneInfo.laneLinkLeft].points[index1].point +
                                             tempDir1 * Vector3.right *
                                             manager.lanes[laneInfo.laneLinkLeft].laneWidth;
                                    else if (manager.lanes[laneInfo.laneLinkLeft].laneLinkLeft == drawingData.laneIndex)
                                        p1 = manager.lanes[laneInfo.laneLinkLeft].points[index1].point +
                                             tempDir1 * -Vector3.right *
                                             manager.lanes[laneInfo.laneLinkLeft].laneWidth;
                                    Handles.DrawLine(p[2], p1);
                                }

                                if (laneInfo.laneLinkRight != -1)
                                {
                                    var p1 = Vector3.zero;
                                    var index1 = (manager.lanes[laneInfo.laneLinkRight].points.Length) / 2;
                                    var tempDir1 = Quaternion.LookRotation(
                                        (manager.lanes[laneInfo.laneLinkRight]
                                             .points[
                                                 index1 + 1 < manager.lanes[laneInfo.laneLinkRight].points.Length
                                                     ? index1 + 1
                                                     : index1].point -
                                         manager.lanes[laneInfo.laneLinkRight].points[index1].point));

                                    if (manager.lanes[laneInfo.laneLinkRight].laneLinkLeft == drawingData.laneIndex)
                                        p1 = manager.lanes[laneInfo.laneLinkRight].points[index1].point +
                                             tempDir1 * -Vector3.right *
                                             manager.lanes[laneInfo.laneLinkRight].laneWidth;
                                    else if (manager.lanes[laneInfo.laneLinkRight].laneLinkRight ==
                                             drawingData.laneIndex)
                                        p1 = manager.lanes[laneInfo.laneLinkRight].points[index1].point +
                                             tempDir1 * Vector3.right *
                                             manager.lanes[laneInfo.laneLinkRight].laneWidth;
                                    Handles.DrawLine(p[0], p1);
                                }

                                //SceneView.RepaintAll();
                            }
                        }
                    }
                }

                //Connectors drawing and selection

                for (var y = 0; y < laneInfo.connectors.Length; y++)
                {
                    if (laneInfo.connectors[y].middlePoints.Count == 0)
                    {
                        continue;
                    }

                    var laneFromConnectorB = laneInfo.conectorB;
                    if (laneInfo.connectors[y].conectorA != laneFromConnectorB)
                    {
                        laneInfo.connectors[y].conectorA = laneFromConnectorB;
                        EditPoints(true, controlID, rayHit, hit,
                            ref laneInfo.connectors[y].middlePoints, ref laneInfo.connectors[y].conectorA,
                            ref laneInfo.connectors[y].conectorB, ref laneInfo.connectors[y].points,
                            ref laneInfo.connectors[y].totalDistance, true, true);
                        SceneView.RepaintAll();
                    }

                    var laneToConnectorA = manager.lanes[laneInfo.connectors[y].nextLane].conectorA;
                    if (laneInfo.connectors[y].conectorB != laneToConnectorA)
                    {
                        laneInfo.connectors[y].conectorB = laneToConnectorA;
                        EditPoints(true, controlID, rayHit, hit,
                            ref laneInfo.connectors[y].middlePoints, ref laneInfo.connectors[y].conectorA,
                            ref laneInfo.connectors[y].conectorB, ref laneInfo.connectors[y].points,
                            ref laneInfo.connectors[y].totalDistance, true, true);
                        SceneView.RepaintAll();
                    }

                    Handles.color = Color.magenta;

                    if (manager.menuSelection == 1 && !addConnector1 && manager.connectionsMenuSelection == 0)
                    {
                        if (CheckPointsSelection(laneInfo.connectors[y], hit, w =>
                        {
                            GUIUtility.hotControl = controlID;
                            currentConnectorSelected = y;
                            selectedLaneForConnector = drawingData.laneIndex;
                            currentLaneSelected = drawingData.laneIndex;
                            GUIUtility.hotControl = 0;
                            Event.current.Use();
                        }, () => { Handles.color = Color.blue; }))
                        {
                            if (removeConnector)
                            {
                                Undo.RecordObject(manager, "Delete Connector");
                                manager.RemoveConnector(currentLaneSelected, currentConnectorSelected);
                                Event.current.Use();
                                GUIUtility.hotControl = 0;
                                return;
                            }
                        }
                    }

                    if (currentConnectorSelected == y && currentLaneSelected == drawingData.laneIndex ||
                        manager.connectionsMenuSelection == 2 && selectedConnectors.Contains(laneInfo.connectors[y]))
                    {
                        Handles.color = Color.yellow;
                    }

                    if (drawingData.connectorsPoints.Count > 0)
                    {
                        Handles.DrawPolyLine(drawingData.connectorsPoints[y]);
                    }

                    Handles.color = Color.green;
                }

                DrawNewConnector();
                DrawNewConnectorsLanesSpheres(laneInfo);

                Handles.color = Color.green;
            }

            if (manager.menuSelection == 1 && manager.connectionsMenuSelection == 1 && Event.current.button == 0)
            {
                if (currentLaneSelected >= 0 && currentLaneSelected < manager.lanes.Length && manager.lanes.Length > 0)
                {
                    if (currentConnectorSelected >= 0 &&
                        currentConnectorSelected < manager.lanes[currentLaneSelected].connectors.Length &&
                        manager.lanes[currentLaneSelected].connectors.Length > 0)
                    {
                        EditPoints(true, controlID, rayHit, hit,
                            ref manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].middlePoints,
                            ref manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].conectorA,
                            ref manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].conectorB,
                            ref manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].points,
                            ref manager.lanes[currentLaneSelected].connectors[currentConnectorSelected].totalDistance,
                            true,
                            false);
                    }
                }
            }

            AddLanesAndConnectors(rayHit, hit, addLane1, addConnector1, controlID, connectorA, connectorB, selectedLaneForConnector);
        }

        private bool CheckPointsSelection(TSBaseInfo baseInfo, RaycastHit hit, Action<int> OnPointSelected, Action OnFocused)
        {
            if (dragging)
            {
                //return false;
            }

            for (var w = 0; w < baseInfo.Points.Length; w++)
            {
                var points = baseInfo.Points;
                var dist = (points[w].point - hit.point).magnitude;
                /*var screenP1 = HandleUtility.WorldToGUIPoint(w == 0 ? baseInfo.conectorA : points[w - 1].point);
                var screenP2 = HandleUtility.WorldToGUIPoint(points[w].point);
                var p1 = (screenP1 - screenP2);
                var p2 = (screenP1 - Event.current.mousePosition);
                var dot = Vector2.Dot(p1.normalized, p2.normalized);
                var isLastGood = (w == points.Length - 1
                    ? (p2.sqrMagnitude < p1.sqrMagnitude)
                    : (p2.sqrMagnitude < p1.sqrMagnitude * 2));
                var shouldSelect = dot > 0.95f && isLastGood;*/
                if ( dist > 3 * manager.scaleFactor)//shouldSelect == false &&
                {
                    continue;
                }

                if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp) &&
                    Event.current.button == 0 && !dontDoAnything)
                {
                    OnPointSelected?.Invoke(w);
                    return true;
                }

                OnFocused?.Invoke();
                return false;
            }

            return false;
        }

        private void DrawNewConnectorsLanesSpheres(TSLaneInfo lane)
        {
            //If add connectors is active, draw spheres on the lanes connectors A and B
            if (!addConnector1) {return;}

            if (newConnector == null) {return;}
            
            Handles.color = Color.green;

            if (newConnector.conectorB == lane.conectorA || ((newConnector.conectorB - lane.conectorA).magnitude < 3 * manager.scaleFactor))
            {
                Handles.color = Color.white;
            }

            Handles.SphereHandleCap(0, lane.conectorA, Quaternion.identity, 2 * manager.scaleFactor, EventType.Repaint);
            Handles.color = Color.blue;
            
            if (newConnector.conectorA == lane.conectorB || ((newConnector.conectorA - lane.conectorB).magnitude < 3 * manager.scaleFactor))
            {
                Handles.color = Color.white;
            }

            Handles.SphereHandleCap(0, lane.conectorB, Quaternion.identity, 2 * manager.scaleFactor, EventType.Repaint);
            Handles.color = Color.green;
        }

        private void DrawNewConnector()
        {
            Handles.color = Color.green;
            if (newConnector != null && newConnector.points != null && newConnector.points.Length > 0)
            {
                var points = new Vector3[newConnector.points.Length];
                for (var w = 0; w < newConnector.points.Length; w++)
                {
                    points[w] = newConnector.points[w].point;
                }

                Handles.DrawPolyLine(points);
            }
        }
        //OnSceneGUI end

        private void OnLaneDeleted(int selectedLane)
        {
            List<Vector3[]> connectorsToAdd = new List<Vector3[]>();
            foreach (var connector in manager.lanes[selectedLane].connectors)
            {
                connectorsToAdd.Add(connector.points.Select(point => point.point).ToArray());
            }

            var drawingData = new DrawingData
            {
                laneIndex = selectedLane,
                points = manager.lanes[selectedLane].points.Select(point => point.point).ToArray(),
                connectorsPoints = connectorsToAdd
            };

            tempLanes.Remove(drawingData);
        }

        private void EditPoints(bool editPoints1, int controlID, bool rayHit, RaycastHit hit)
        {
            if (editPoints1 && currentLaneSelected != -1 && currentLaneSelected < manager.lanes.Length)
                EditPoints(editPoints1, controlID, rayHit, hit, ref manager.lanes[currentLaneSelected].middlePoints,
                    ref manager.lanes[currentLaneSelected].conectorA, ref manager.lanes[currentLaneSelected].conectorB,
                    ref manager.lanes[currentLaneSelected].points, ref manager.lanes[currentLaneSelected].totalDistance,
                    false, false);
        }

        private void EditPoints<T>(bool editPoints1, int controlID, bool rayHit, RaycastHit hit,
            ref List<Vector3> middlePoints,
            ref Vector3 conectorA, ref Vector3 conectorB, ref T[] points, ref float totalDistance, bool isConnector,
            bool forcedUpdate) where T : TSPoints, new()
        {
            bool changedPosition = false;
            if (forcedUpdate) changedPosition = true;
            if (editPoints1)
            {
                if (mainData.enableUndoForLanesDeletion)
                    Undo.RecordObject(manager, "Edit points");
                Vector3 lastPos = Vector3.zero;
                for (int i = 0; i < middlePoints.Count; i++)
                {
                    if ((manager.menuSelection == 0 && !removeLanePoint) ||
                        (manager.menuSelection == 1 && !removeConnectorPoint))
                    {
                        lastPos = middlePoints[i];
                        middlePoints[i] = Handles.PositionHandle(middlePoints[i], Quaternion.identity);
                        if (lastPos != middlePoints[i]) changedPosition = true;
                    }

                    Handles.color = Color.red;
                    Handles.SphereHandleCap(0, middlePoints[i], Quaternion.identity, 2 * manager.scaleFactor,
                        EventType.Repaint);
                }

                if (!isConnector)
                {
                    lastPos = conectorA;
                    conectorA = Handles.PositionHandle(conectorA, Quaternion.identity);
                    if (lastPos != conectorA) changedPosition = true;
                    lastPos = conectorB;
                    conectorB = Handles.PositionHandle(conectorB, Quaternion.identity);
                    if (lastPos != conectorB) changedPosition = true;
                    Handles.color = Color.green;
                    Handles.SphereHandleCap(0, conectorA, Quaternion.identity, 2 * manager.scaleFactor,
                        EventType.Repaint);
                    Handles.SphereHandleCap(0, conectorB, Quaternion.identity, 2 * manager.scaleFactor,
                        EventType.Repaint);
                }

                if (changedPosition)
                {
                    manager.junctionsProcessed = false;
                    RefreshPoints(conectorA, conectorB, middlePoints, ref points, ref totalDistance, isConnector);
                }

                if (Event.current.type == EventType.MouseDown && changedPosition == false && rayHit &&
                    ((manager.menuSelection == 0 && manager.laneMenuSelection == 2 && !removeLanePoint) ||
                     (manager.menuSelection == 1 && manager.connectionsMenuSelection == 1 && !removeConnectorPoint)) &&
                    !dontDoAnything)
                {
                    GUIUtility.hotControl = controlID;
                    int insertPosition = 0;
                    float hitDistance = (conectorA - hit.point).magnitude;
                    int nearPoint = 0;
                    float maxDist = float.MaxValue;
                    for (insertPosition = 0; insertPosition < middlePoints.Count; insertPosition++)
                    {
                        float currentDist = (hit.point - middlePoints[insertPosition]).magnitude;
                        if (currentDist < maxDist)
                        {
                            maxDist = currentDist;
                            nearPoint = insertPosition;
                        }
                    }

                    float hitConnectorBDist = (conectorB - hit.point).magnitude;
                    if (hitConnectorBDist < maxDist) nearPoint = middlePoints.Count - 1;
                    bool insertBeforeConnectorA = false;
                    bool insertAfterConnectorB = false;
                    if (nearPoint == 0)
                    {
                        //If this is near the first middle point, we  need to compare both points distances to
                        //connector A to see which one is near and insert the point acordingly
                        float currentDist = (conectorA - middlePoints[nearPoint]).magnitude;

                        insertBeforeConnectorA = CheckBehindPoint(conectorA, middlePoints[nearPoint], hit.point);

                        if (hitDistance < currentDist)
                        {
                            nearPoint = 0;
                        }
                        else nearPoint = 1;

                        if (insertBeforeConnectorA)
                        {
                            nearPoint = 0;
                        }

                        if (middlePoints.Count == 1)
                        {
                            insertAfterConnectorB = CheckBehindPoint(conectorB, middlePoints[0], hit.point);
                        }
                    }
                    else
                    {
                        float currentDist = (middlePoints[nearPoint - 1] - middlePoints[nearPoint]).magnitude;
                        hitDistance = (middlePoints[nearPoint - 1] - hit.point).magnitude;

                        if (nearPoint == middlePoints.Count - 1)
                        {
                            insertAfterConnectorB = CheckBehindPoint(conectorB, middlePoints[nearPoint], hit.point);
                            if (insertAfterConnectorB)
                            {
                                nearPoint = middlePoints.Count;
                                hitDistance = currentDist - 1;
                            }
                        }

                        if (hitDistance > currentDist)
                            nearPoint++;
                    }

                    if (Event.current.shift)
                    {
                        insertBeforeConnectorA = false;
                        insertAfterConnectorB = true;
                        nearPoint = middlePoints.Count;
                    }

                    if (insertBeforeConnectorA)
                    {
                        middlePoints.Insert(nearPoint, conectorA);
                        conectorA = hit.point;
                    }
                    else if (insertAfterConnectorB)
                    {
                        middlePoints.Insert(nearPoint, conectorB);
                        conectorB = hit.point;
                    }
                    else
                    {
                        middlePoints.Insert(nearPoint, hit.point);
                    }

                    Event.current.Use();
                    GUIUtility.hotControl = 0;

                    RefreshPoints(conectorA, conectorB, middlePoints, ref points, ref totalDistance, isConnector);
                    manager.junctionsProcessed = false;
                }

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !dontDoAnything)
                {
                    float minDist = float.MaxValue;
                    int pointIndex = -1;
                    for (int w = 0; w < middlePoints.Count; w++)
                    {
                        if ((manager.menuSelection == 0 && manager.laneMenuSelection == 2 && removeLanePoint) ||
                            (manager.menuSelection == 1 && manager.connectionsMenuSelection == 1 &&
                             removeConnectorPoint))
                        {
                            float dist = (middlePoints[w] - hit.point).magnitude;
                            if (rayHit && dist < minDist && dist <= 1 && middlePoints.Count > 1)
                            {
                                minDist = dist;
                                pointIndex = w;
                            }
                        }
                    }

                    if (pointIndex != -1)
                        middlePoints.Remove(middlePoints[pointIndex]);
                    Event.current.Use();
                    GUIUtility.hotControl = 0;

                    RefreshPoints(conectorA, conectorB, middlePoints, ref points, ref totalDistance, isConnector);
                    manager.junctionsProcessed = false;
                }
            }
        }


        private bool CheckBehindPoint(Vector3 point1, Vector3 point2, Vector3 hitPoint)
        {
            GameObject tempObject = new GameObject();
            tempObject.transform.position = point1;
            tempObject.transform.rotation = Quaternion.LookRotation(point1 - point2);
            float behind = tempObject.transform.InverseTransformPoint(hitPoint).z;
            Object.DestroyImmediate(tempObject);
            if (behind > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Refreshs the lanes.
        /// </summary>
        /// <param name="showProgressBar">If set to <c>true</c> show progress bar.</param>
        private void RefreshLanes(bool showProgressBar)
        {
            float progress = 0;
            if (showProgressBar)
                EditorUtility.DisplayProgressBar("Processing lanes points", "Starting....", progress);
            for (int i = 0; i < manager.lanes.Length; i++)
            {
                RefreshPoints(manager.lanes[i].conectorA,
                    manager.lanes[i].conectorB,
                    manager.lanes[i].middlePoints,
                    ref manager.lanes[i].points,
                    ref manager.lanes[i].totalDistance,
                    false);
                if (showProgressBar)
                {
                    progress = i / manager.lanes.Length;
                    EditorUtility.DisplayProgressBar("Processing lanes points", "Lane" + i + "/" + manager.lanes.Length,
                        progress);
                }
            }

            EditorUtility.ClearProgressBar();
            ProcessJunctions();
        }


        private void RefreshConnectors(bool showProgressBar)
        {
            float progress = 0;
            if (showProgressBar)
                EditorUtility.DisplayProgressBar("Processing connectors points", "Starting....", progress);
            for (int i = 0; i < manager.lanes.Length; i++)
            {
                for (int w = 0; w < manager.lanes[i].connectors.Length; w++)
                {
                    RefreshPoints(manager.lanes[i].connectors[w].conectorA,
                        manager.lanes[i].connectors[w].conectorB,
                        manager.lanes[i].connectors[w].middlePoints,
                        ref manager.lanes[i].connectors[w].points,
                        ref manager.lanes[i].connectors[w].totalDistance,
                        true);
                    if (showProgressBar)
                    {
                        progress = i / manager.lanes.Length;
                        EditorUtility.DisplayProgressBar("Processing connectors points",
                            "Lane:" + i + "/" + manager.lanes.Length + " Connector:" + w + "/" +
                            manager.lanes[i].connectors.Length, progress);
                    }
                }
            }

            ProcessJunctions();
        }

        private void ProcessJunctions()
        {
            EditorUtility.ClearProgressBar();
            manager.ProcessJunctions(mainData.calculateNerbyPointsForPlayerFinding, mainData.nearbyPointsRadius,
                (title, description, progress) => { EditorUtility.DisplayProgressBar(title, description, progress); },
                () =>
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.SetDirty(manager);
                });
        }

        private void RefreshPoints<T>(Vector3 conectorA, Vector3 conectorB, List<Vector3> middlePoints, ref T[] points,
            ref float totalDistance, bool isConnector) where T : TSPoints, new()
        {
            Vector3[] pts = new Vector3[4 + middlePoints.Count];
            pts[0] = conectorA;
            pts[1] = conectorA;
            int r = 2;
            for (r = 2; r < (2 + middlePoints.Count); r++)
                pts[r] = middlePoints[r - 2];

            pts[r] = conectorB;
            pts[r + 1] = conectorB;
            points = new T[0];

            TSUtils.CreatePoints(isConnector ? manager.resolutionConnectors : manager.resolution, pts, ref points,
                ref totalDistance);
        }

        private Vector3 startPoint;

        private void AddLanesAndConnectors(bool rayHit, RaycastHit hit, bool addLane1, bool addConnector1,
            int controlID,
            bool connectorA, bool connectorB, int lane)
        {
            if (Event.current.button == 0 && !dontDoAnything &&
                ((addLane1 && manager.menuSelection == 0 && manager.laneMenuSelection == 0) || (addConnector1 &&
                    manager.menuSelection == 1 && manager.connectionsMenuSelection == 0)))
            {
                switch (Event.current.GetTypeForControl(controlID))
                {
                    case EventType.MouseDown:

                        GUIUtility.hotControl = controlID;
                        if (rayHit)
                        {
                            startPoint = hit.point;

                            if (addConnector1 && manager.lanes.Length > 0)
                            {
                                addConnector = true;
                                AddConnector(lane, connectorA, connectorB, false, hit.point);
                            }
                        }

                        Event.current.Use();
                        break;

                    case EventType.MouseUp:
                        GUIUtility.hotControl = 0;
                        dragging = false;
                        if (rayHit)
                        {
                            if (addLane1)
                            {
                                AddLane(hit.point, true);
                                manager.junctionsProcessed = false;
                            }

                            if (addConnector && manager.lanes.Length > 0)
                            {
                                addConnector = false;
                                AddConnector(lane, connectorA, connectorB, false, hit.point);
                            }
                        }

                        Event.current.Use();
                        break;

                    case EventType.MouseDrag:
                        if (rayHit)
                        {
                            dragging = true;
                            if (addLane1)
                            {
                                SceneView.RepaintAll();
                            }

                            if (addConnector && manager.lanes.Length > 0)
                            {
                                addConnector = false;
                                AddConnector(lane, connectorA, connectorB, true, hit.point);
                                addConnector = true;
                                SceneView.RepaintAll();
                            }
                        }

                        //Event.current.Use();
                        break;
                }
            }
        }


        private void AddLane(Vector3 position, bool finished)
        {
            if (finished)
            {
                manager.AddLane<TSLaneInfo>(startPoint, position);
                currentLaneSelected = manager.lanes.Length - 1;
            }

            if (finished && manager.lanes[currentLaneSelected].points.Length < 3)
            {
                manager.RemoveLane(currentLaneSelected, OnLaneDeleted);
                currentLaneSelected = manager.lanes.Length - 1;
            }
        }

        private bool addedConnectorA = false;
        private bool addedConnectorB = false;
        private int laneFrom = -1;
        private int laneTo = -1;
        private bool dragging;
        private SerializedObject serializedObject;

        private void AddConnector(int lane, bool connectorA, bool connectorB, bool dragging, Vector3 position)
        {
            if (addConnector)
            {
                addedConnectorA = false;
                addedConnectorB = false;

                if (!connectorA && !connectorB) {return;}
                
                if (connectorA)
                {
                    addedConnectorA = true;
                    laneTo = lane;
                }

                if (connectorB)
                {
                    addedConnectorB = true;
                    laneFrom = lane;
                }

                newConnector.conectorA = newConnector.conectorB = connectorA ? manager.lanes[lane].conectorA : manager.lanes[lane].conectorB;
            }
            else
            {
                var finished = false;
                float sign = 0;
                float angle = 0;
                if (!dragging)
                {
                    if (connectorA || connectorB)
                    {
                        if (connectorA)
                        {
                            addedConnectorA = true;
                            laneTo = lane;
                            newConnector.conectorB = manager.lanes[lane].conectorA;
                        }

                        if (connectorB)
                        {
                            addedConnectorB = true;
                            laneFrom = lane;
                            newConnector.conectorA = manager.lanes[lane].conectorB;
                        }

                        newConnector.nextLane = laneTo;
                        newConnector.previousLane = laneFrom;
                    }

                    if (addedConnectorA && addedConnectorB)
                    {
                        angle = Vector3.Angle(manager.lanes[laneTo].conectorA - manager.lanes[laneTo].points[2].point,
                            manager.lanes[laneFrom].points[manager.lanes[laneFrom].points.Length - 3].point -
                            manager.lanes[laneFrom].conectorB);
                        var referenceRight = Vector3.Cross(Vector3.up,
                            manager.lanes[laneFrom].points[manager.lanes[laneFrom].points.Length - 3].point -
                            manager.lanes[laneFrom].conectorB);
                        sign = Mathf.Sign(Vector3.Dot(
                            manager.lanes[laneTo].conectorA - manager.lanes[laneTo].points[2].point,
                            referenceRight)); // >= 0.0f) ? 1.0f: -1.0f;
                        angle *= sign;
                        if (angle > 30) newConnector.direction = TSLaneConnector.Direction.Right;
                        else if (angle < -30) newConnector.direction = TSLaneConnector.Direction.Left;
                        else newConnector.direction = TSLaneConnector.Direction.Straight;
                        finished = true;
                    }
                }
                else
                {
                    if (!addedConnectorA)
                    {
                        newConnector.conectorB = position;
                    }

                    if (!addedConnectorB)
                    {
                        newConnector.conectorA = position;
                    }
                }

                newConnector.middlePoints = new List<Vector3>();
                newConnector.middlePoints.Add(((newConnector.conectorA + newConnector.conectorB) / 2f));

                if (finished)
                {
                    Undo.RecordObject(manager, "Add new Connector");
                    manager.junctionsProcessed = false;
                    if (Mathf.Abs(angle) > 5)
                    {
                        var multiplier = Mathf.Min(Mathf.Abs(angle) / 90f * 0.35f, 0.5f);
                        var tempDir = Quaternion.LookRotation(manager.lanes[laneTo].conectorA - manager.lanes[laneTo].points[2].point);
                        
                        var tempDir1 = Quaternion.LookRotation(manager.lanes[laneFrom].points[manager.lanes[laneFrom].points.Length - 3].point - manager.lanes[laneFrom].conectorB);

                        var tempConnectorA = new GameObject();
                        tempConnectorA.transform.position = newConnector.conectorA;
                        tempConnectorA.transform.rotation = tempDir1;
                        var tempDistance = tempConnectorA.transform.InverseTransformPoint(newConnector.conectorB);

                        UnityEditor.Editor.DestroyImmediate(tempConnectorA);
                        newConnector.middlePoints[0] +=
                            (tempDir * Vector3.forward * Mathf.Abs(tempDistance.x) * multiplier) +
                            (tempDir1 * -Vector3.forward * Mathf.Abs(tempDistance.z) * multiplier);
                    }

                    tempLanes.Clear();
                }

                var pts = new Vector3[5];
                pts[0] = newConnector.conectorA;
                pts[1] = newConnector.conectorA;
                pts[2] = newConnector.middlePoints[0];
                pts[3] = newConnector.conectorB;
                pts[4] = newConnector.conectorB;
                newConnector.points = new TSConnectorPoint[0];
                TSUtils.CreatePoints(manager.resolutionConnectors, pts, ref newConnector.points, ref newConnector.totalDistance);
                if (addedConnectorA && addedConnectorB)
                {
                    manager.lanes[laneFrom].connectors = manager.lanes[laneFrom].connectors.Add(newConnector);
                    manager.lanes[laneTo].connectorsReverse = manager.lanes[laneTo].connectorsReverse.Add(newConnector);
                }

                if (finished) newConnector = new TSLaneConnector();
            }
        }

        private void Save(string path)
        {
            manager.Save(path);
        }

        private void Load(string path)
        {
            manager.Load(path);
        }

        private string GetiTSDirectory()
        {
            if (Directory.Exists(Application.dataPath + Path.DirectorySeparatorChar + "iTS" +
                                 Path.DirectorySeparatorChar + "Traffic System" + Path.DirectorySeparatorChar +
                                 "Required"))
                return "Assets" + Path.DirectorySeparatorChar + "iTS" + Path.DirectorySeparatorChar + "Traffic System" +
                       Path.DirectorySeparatorChar + "Required" + Path.DirectorySeparatorChar;
            Stack<string> stack = new Stack<string>();
            // Add the root directory to the stack
            stack.Push(Application.dataPath);
            // While we have directories to process...
            //		Debug.Log ("Pre-Seacrhing all cars on the specified directories!");
            while (stack.Count > 0)
            {
                // Grab a directory off the stack
                string currentDir = stack.Pop();
                {
                    foreach (string dir in Directory.GetDirectories(currentDir))
                    {
                        if (dir.EndsWith("iTS"))
                        {
                            if (Directory.Exists(dir + Path.DirectorySeparatorChar + "Traffic System" +
                                                 Path.DirectorySeparatorChar + "Required" +
                                                 Path.DirectorySeparatorChar))
                            {
                                return dir.Replace(Application.dataPath, "Assets") + Path.DirectorySeparatorChar +
                                       "Traffic System" + Path.DirectorySeparatorChar + "Required" +
                                       Path.DirectorySeparatorChar;
                            }
                        }

                        // Add directories at the current level into the stack
                        stack.Push(dir);
                    }
                }
            }

            return "Not found!";
        }
    }
}

