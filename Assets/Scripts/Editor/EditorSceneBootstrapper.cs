#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class EditorSceneBootstrapper
{
    private const string BootstrapperEnabledKey = "EditorSceneBootstrapper_Enabled";

    static EditorSceneBootstrapper()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // We don't need to do anything special here unless needed
    }

    [MenuItem("Tools/Toggle Auto-Load Main Menu", false, 100)]
    public static void ToggleBootstrapper()
    {
        bool isEnabled = EditorPrefs.GetBool(BootstrapperEnabledKey, true);
        EditorPrefs.SetBool(BootstrapperEnabledKey, !isEnabled);
        Debug.Log($"<b>[Bootstrapper]</b> Auto-load Main Menu is now {( !isEnabled ? "ENABLED" : "DISABLED" )}.");
    }

    [MenuItem("Tools/Toggle Auto-Load Main Menu", true)]
    public static bool ToggleBootstrapperValidate()
    {
        bool isEnabled = EditorPrefs.GetBool(BootstrapperEnabledKey, true);
        Menu.SetChecked("Tools/Toggle Auto-Load Main Menu", isEnabled);
        return true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadMainMenuFirst()
    {
        // Check if bootstrapper is enabled
        if (!EditorPrefs.GetBool(BootstrapperEnabledKey, true))
        {
            return;
        }

        // Check if MainMenuNew scene is in the build settings
        bool hasMainMenuInBuild = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            if (scenePath.Contains("MainMenuNew"))
            {
                hasMainMenuInBuild = true;
                break;
            }
        }

        // If it is in build, and the currently active scene is not MainMenuNew, redirect to MainMenuNew
        if (hasMainMenuInBuild && SceneManager.GetActiveScene().name != "MainMenuNew")
        {
            Debug.Log("<b>[Bootstrapper]</b> Redirecting to MainMenuNew first. You can disable this via Tools > Toggle Auto-Load Main Menu.");
            SceneManager.LoadScene("MainMenuNew");
        }
    }
}
#endif
