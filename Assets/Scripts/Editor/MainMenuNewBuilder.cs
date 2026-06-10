using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor tool untuk otomatis membangun Scene Main Menu baru dengan kustom UI asset.
/// Cara pakai: Klik menu Tools > Build New Main Menu Scene
/// </summary>
public class MainMenuNewBuilder : EditorWindow
{
    [MenuItem("Tools/Build New Main Menu Scene")]
    public static void BuildMainMenuNew()
    {
        // 1. Konfigurasi PNG asset di folder Assets/UI sebagai UI Sprite
        SetAsSprite("Assets/UI/Play.png");
        SetAsSprite("Assets/UI/Load.png");
        SetAsSprite("Assets/UI/Exit.png");
        SetAsSprite("Assets/UI/Frame 10.png");
        SetAsSprite("Assets/Background/Screenshot 2026-05-12 072239.png");

        // Load Sprites
        Sprite playSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Play.png");
        Sprite loadSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Load.png");
        Sprite exitSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Exit.png");
        Sprite saveSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Frame 10.png");
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Background/Screenshot 2026-05-12 072239.png");

        // 2. Buat Scene Baru
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 3. Buat Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 4. Buat EventSystem
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // 5. Background kustom ("Screenshot")
        GameObject bgObj = CreateUIObject(canvasObj, "Background");
        SetRect(bgObj, Vector2.zero, Vector2.zero, Vector2.one, Vector2.zero);
        Image bgImg = bgObj.AddComponent<Image>();
        if (bgSprite != null)
        {
            bgImg.sprite = bgSprite;
            bgImg.color = Color.white;
        }
        else
        {
            bgImg.color = new Color(0.1f, 0.12f, 0.18f, 1f);
        }

        // 6. Judul Game
        GameObject titleObj = CreateUIObject(canvasObj, "TitleText");
        SetRect(titleObj, new Vector2(800f, 100f), new Vector2(0.5f, 0.85f), new Vector2(0.5f, 0.85f), Vector2.zero);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "Orenomonogatari";
        titleTmp.fontSize = 54;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = new Color(0.95f, 0.85f, 0.55f, 1f);

        // 7. Panel Tombol Utama
        GameObject buttonsPanel = CreateUIObject(canvasObj, "ButtonsPanel");
        SetRect(buttonsPanel, new Vector2(550f, 650f), new Vector2(0.5f, 0.4f), new Vector2(0.5f, 0.4f), Vector2.zero);
        VerticalLayoutGroup layout = buttonsPanel.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 30;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        Vector2 buttonSize = new Vector2(480f, 130f);

        // New Game (Play) Button
        Button newGameBtn = CreateImageButton(buttonsPanel, "NewGameButton", playSprite, buttonSize);
        // Load Game Button
        Button loadGameBtn = CreateImageButton(buttonsPanel, "LoadGameButton", loadSprite, buttonSize);
        // Quit (Exit) Button
        Button quitBtn = CreateImageButton(buttonsPanel, "QuitButton", exitSprite, buttonSize);

        // 8. Panel Load Game (hidden by default)
        GameObject loadPanel = CreateUIObject(canvasObj, "LoadPanel");
        SetRect(loadPanel, new Vector2(450f, 420f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
        
        Sprite defaultPanelSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        Image lpBg = loadPanel.AddComponent<Image>();
        if (defaultPanelSprite != null) { lpBg.sprite = defaultPanelSprite; lpBg.type = Image.Type.Sliced; }
        lpBg.color = new Color(0.12f, 0.12f, 0.16f, 0.95f);

        VerticalLayoutGroup lpLayout = loadPanel.AddComponent<VerticalLayoutGroup>();
        lpLayout.spacing = 12;
        lpLayout.padding = new RectOffset(20, 20, 20, 20);
        lpLayout.childAlignment = TextAnchor.UpperCenter;
        lpLayout.childControlWidth = true;
        lpLayout.childControlHeight = false;
        lpLayout.childForceExpandWidth = true;
        lpLayout.childForceExpandHeight = false;

        GameObject lpTitle = CreateUIObject(loadPanel, "LoadTitle");
        lpTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 40f);
        TextMeshProUGUI lpTitleTmp = lpTitle.AddComponent<TextMeshProUGUI>();
        lpTitleTmp.text = "LOAD GAME";
        lpTitleTmp.fontSize = 24;
        lpTitleTmp.fontStyle = FontStyles.Bold;
        lpTitleTmp.alignment = TextAlignmentOptions.Center;
        lpTitleTmp.color = Color.white;

        // 3 Slot Load
        TMP_Text[] loadSlotInfoTexts = new TMP_Text[3];
        Button[] loadSlotButtons = new Button[3];

        for (int i = 0; i < 3; i++)
        {
            int slotNum = i + 1;

            GameObject slotRow = CreateUIObject(loadPanel, "LoadSlotRow_" + slotNum);
            slotRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 75f);

            Image slotBg = slotRow.AddComponent<Image>();
            if (defaultPanelSprite != null) { slotBg.sprite = defaultPanelSprite; slotBg.type = Image.Type.Sliced; }
            slotBg.color = new Color(0.18f, 0.18f, 0.24f, 0.9f);

            HorizontalLayoutGroup slotLayout = slotRow.AddComponent<HorizontalLayoutGroup>();
            slotLayout.spacing = 10;
            slotLayout.padding = new RectOffset(15, 15, 10, 10);
            slotLayout.childAlignment = TextAnchor.MiddleLeft;
            slotLayout.childControlWidth = false;
            slotLayout.childControlHeight = false;
            slotLayout.childForceExpandWidth = false;
            slotLayout.childForceExpandHeight = false;

            GameObject infoObj = CreateUIObject(slotRow, "SlotInfo_" + slotNum);
            infoObj.GetComponent<RectTransform>().sizeDelta = new Vector2(280f, 55f);
            TextMeshProUGUI infoTmp = infoObj.AddComponent<TextMeshProUGUI>();
            infoTmp.text = $"Slot {slotNum}  |  --- Kosong ---";
            infoTmp.fontSize = 13;
            infoTmp.alignment = TextAlignmentOptions.Left;
            infoTmp.color = new Color(0.85f, 0.85f, 0.85f);
            infoTmp.raycastTarget = false;
            loadSlotInfoTexts[i] = infoTmp;

            loadSlotButtons[i] = CreateImageButton(slotRow, "LoadSlot_" + slotNum, saveSprite, new Vector2(80f, 40f));
            AddTextOverlay(loadSlotButtons[i].gameObject, "LOAD", 12f);
        }

        // Tombol Back di Panel Load
        Button backBtn = CreateImageButton(loadPanel, "BackButton", saveSprite, new Vector2(160f, 50f));
        AddTextOverlay(backBtn.gameObject, "BACK", 16f);

        loadPanel.SetActive(false);

        // 9. Buat SaveLoadManager
        GameObject slmObj = new GameObject("SaveLoadManager");
        slmObj.AddComponent<SaveLoadManager>();

        // 10. Buat MainMenuManager & Hubungkan Reference
        GameObject mmManagerObj = new GameObject("MainMenuManager");
        MainMenuManager mmManager = mmManagerObj.AddComponent<MainMenuManager>();

        SerializedObject so = new SerializedObject(mmManager);
        so.FindProperty("gameplaySceneName").stringValue = "Gamedigital";
        so.FindProperty("mainMenuPanel").objectReferenceValue = buttonsPanel;
        so.FindProperty("loadPanel").objectReferenceValue = loadPanel;
        so.FindProperty("newGameButton").objectReferenceValue = newGameBtn;
        so.FindProperty("loadGameButton").objectReferenceValue = loadGameBtn;
        so.FindProperty("quitButton").objectReferenceValue = quitBtn;
        so.FindProperty("titleText").objectReferenceValue = titleTmp;

        so.FindProperty("loadSlot1InfoText").objectReferenceValue = loadSlotInfoTexts[0];
        so.FindProperty("loadSlot1Button").objectReferenceValue = loadSlotButtons[0];
        so.FindProperty("loadSlot2InfoText").objectReferenceValue = loadSlotInfoTexts[1];
        so.FindProperty("loadSlot2Button").objectReferenceValue = loadSlotButtons[1];
        so.FindProperty("loadSlot3InfoText").objectReferenceValue = loadSlotInfoTexts[2];
        so.FindProperty("loadSlot3Button").objectReferenceValue = loadSlotButtons[2];
        so.FindProperty("loadBackButton").objectReferenceValue = backBtn;

        so.ApplyModifiedProperties();

        // 11. Simpan Scene ke Folder Scenes
        string scenePath = "Assets/Scenes/MainMenuNew.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);

        // 12. Registrasikan ke Build Settings sebagai scene pertama (Index 0)
        AddSceneToBuildSettings("Assets/Gamedigital.unity");
        AddSceneToBuildSettings(scenePath);

        Debug.Log("MainMenuNewBuilder: ✅ Scene MainMenuNew berhasil dibuat di " + scenePath);
    }

    private static void SetAsSprite(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }
    }

    private static GameObject CreateUIObject(GameObject parent, string name)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent.transform, false);
        return obj;
    }

    private static void SetRect(GameObject obj, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos)
    {
        RectTransform r = obj.GetComponent<RectTransform>();
        r.sizeDelta = size;
        r.anchorMin = anchorMin;
        r.anchorMax = anchorMax;
        r.anchoredPosition = anchoredPos;
    }

    private static Button CreateImageButton(GameObject parent, string name, Sprite sprite, Vector2 size)
    {
        GameObject btnObj = CreateUIObject(parent, name);
        SetRect(btnObj, size, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);

        Image img = btnObj.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        
        Button btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.95f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        btn.colors = colors;

        return btn;
    }

    private static void AddTextOverlay(GameObject parent, string text, float fontSize = 16f)
    {
        GameObject labelObj = CreateUIObject(parent, "Label");
        SetRect(labelObj, Vector2.zero, Vector2.zero, Vector2.one, Vector2.zero);

        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        scenes.RemoveAll(s => s.path == scenePath);
        scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("MainMenuNewBuilder: Added " + scenePath + " ke Build Settings di index 0.");
    }
}
