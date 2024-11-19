using System;
using System.Collections.Generic;
using System.Linq;
using ITS.Utils;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ModuleCheckAndInstaller : EditorWindow
{
    private class ModuleData
    {
        public BaseInstallableModule Module => module;
        private BaseInstallableModule module;
        public bool FoldOut { get; set; }
        public bool Detected { get; set; }
        
        public float Fade { get; private set; }

        public ModuleData(BaseInstallableModule newModule)
        {
            module = newModule;
            Detected = module.Detected;
        }

        public void UpdateFadeValue()
        {
            Fade += FoldOut ? Time.deltaTime : -Time.deltaTime;
            Fade = Mathf.Clamp01(Fade);
        }
    }

    private static List<ModuleData> _modules = new List<ModuleData>();

    static ModuleCheckAndInstaller()
    {
        PopulateModules();
    }

    [InitializeOnLoadMethod]
    private static void CheckModulesInstallation()
    {
        foreach (var module in _modules)
        {
            if (module.Module.Detected && module.Module.Installed == false &&
                EditorPrefs.HasKey($"iTS-{module.Module.Name}") == false)
            {
                var decision = EditorUtility.DisplayDialogComplex($"iTS - New Asset detected! - {module.Module.Name}",
                    $"A new asset have been detected({module.Module.Name}), would you like to install it's integration module for iTS?",
                    "Yes",
                    "No", "Don't show again for this asset");
                if (decision == 0)
                {
                    DoModuleInstallation(module.Module);
                }

                if (decision == 2)
                {
                    EditorPrefs.SetBool($"iTS-{module.Module.Name}", true);
                }
            }
        }
    }

    [MenuItem("Window/iTS/iTS Module Manager")]
    public static void Init()
    {
        ModuleCheckAndInstaller window =
            (ModuleCheckAndInstaller) EditorWindow.GetWindow(typeof(ModuleCheckAndInstaller));
        window.titleContent = new GUIContent("iTS Module Manager");
        window.Show(true);
        PopulateModules();
    }

    private static void PopulateModules()
    {
        _modules.Clear();
        var moduleTypes = TSUtils.GetTypes<BaseInstallableModule>(true);
        foreach (var moduleType in moduleTypes)
        {
            var instance = System.Activator.CreateInstance(moduleType);
            _modules.Add(new ModuleData((BaseInstallableModule) instance));
        }
    }

    private void OnGUI()
    {
        foreach (var module in _modules)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(module.Module.Name, GUILayout.Width(150));
            var detected = module.Detected;
            var installed = module.Module.Installed;
            var label = installed ? "Installed!" :
                detected ? "Detected!" : "";
            EditorGUILayout.LabelField(label, GUILayout.Width(installed ? 100 : 150));

            if (GUILayout.Button(installed ? "Uninstall" : detected ? "Install" : "Not Detected!"))
            {
                Action<BaseInstallableModule> action1 = DoModuleInstallation;
                Action<BaseInstallableModule> action2 = DoModuleUninstallation;
                ExecuteAction(installed ? action2 : action1, module.Module);
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    private static void ExecuteAction(Action<BaseInstallableModule> action, BaseInstallableModule data)
    {
        action.Invoke(data);
    }

    private static void DoModuleUninstallation(BaseInstallableModule module)
    {
        string definesString =
            PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        List<string> allDefines = definesString.Split(';').ToList();
        allDefines.Remove(module.Define);
        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup,
            string.Join(";", allDefines.ToArray()));
        AssetDatabase.Refresh();
    }

    private static void DoModuleInstallation(BaseInstallableModule module)
    {
        if (module.Detected == false){return;}
        
        List<string> datalist = new List<string>(_modules.Count);
        datalist.Add(module.Define);
        foreach (var baseInstallableModule in _modules)
        {
            if (baseInstallableModule.Module.Installed)
            {
                datalist.Add(baseInstallableModule.Module.Define);
            }
        }

        string definesString =
            PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        List<string> allDefines = definesString.Split(';').ToList();
        allDefines.AddRange(datalist.Except(allDefines));
        PlayerSettings.SetScriptingDefineSymbolsForGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup,
            string.Join(";", allDefines.ToArray()));
        AssetDatabase.Refresh();
    }

    public static void ShowModulesOnGUI(TSMainManager mainManager)
    {
        GUIStyle style = GUI.skin.box;
        bool pro = EditorGUIUtility.isProSkin;
        var c_on = pro ? Color.white : new Color(51 / 255f, 102 / 255f, 204 / 255f, 1);
        var uiTex_in = Resources.Load<Texture2D>("IN foldout focus-6510");
        var uiTex_in_on = Resources.Load<Texture2D>("IN foldout focus on-5718");
        style.normal.background = uiTex_in_on;
        style.padding = new RectOffset(15, 0, 0, 0);
        style.fixedWidth = EditorGUIUtility.currentViewWidth - 25f;
        style.alignment = TextAnchor.MiddleLeft;


        foreach (var module in _modules)
        {
            if (module.Module.Installed == false)
            {
                continue;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            var name = module.FoldOut ? $" ▾ {module.Module.Name}" : $" ▸ {module.Module.Name}";
            module.FoldOut = EditorGUILayout.BeginFoldoutHeaderGroup(module.FoldOut, name, style);
            module.UpdateFadeValue();

            using (var group = new EditorGUILayout.FadeGroupScope(module.Fade))
            {
                if (group.visible)
                {
                    module.Module.OnGUI(mainManager);
                }
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndVertical();
        }
    }
}