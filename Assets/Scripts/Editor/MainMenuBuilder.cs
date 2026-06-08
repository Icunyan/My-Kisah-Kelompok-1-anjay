using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor tool untuk otomatis membangun Scene Main Menu dari nol.
/// Cara pakai: Klik menu Tools > Build Main Menu Scene
/// Script ini akan membuat scene baru "MainMenu" lengkap dengan Canvas, tombol, dan wiring.
/// </summary>
public class MainMenuBuilder : EditorWindow
{
    [MenuItem("Tools/Build Main Menu Scene")]
    public static void BuildMainMenu()
    {
        // Konfirmasi dulu
        if (!EditorUtility.DisplayDialog("Build Main Menu",
            "Ini akan membuat scene baru 'MainMenu' di Assets/Scenes/.\n\n" +
            "Pastikan kamu sudah SAVE scene yang sedang terbuka!\n\nLanjutkan?",
            "Ya, Buat!", "Batal"))
        {
            return;
        }

        // =====================================================================
        // 1. BUAT SCENE BARU
        // =====================================================================
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // =====================================================================
        // 2. BUAT CANVAS
        // =====================================================================
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // =====================================================================
        // 3. BUAT EVENT SYSTEM (wajib agar tombol bisa diklik)
        // =====================================================================
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // =====================================================================
        // 4. BACKGROUND GELAP
        // =====================================================================
        GameObject bgObj = CreateUIObject(canvasObj, "Background");
        RectTransform bgR = bgObj.GetComponent<RectTransform>();
        bgR.anchorMin = Vector2.zero;
        bgR.anchorMax = Vector2.one;
        bgR.offsetMin = Vector2.zero;
        bgR.offsetMax = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.06f, 0.14f, 1f); // Dark purple-ish

        // =====================================================================
        // 5. MAIN MENU PANEL
        // =====================================================================
        GameObject mainMenuPanel = CreateUIObject(canvasObj, "MainMenuPanel");
        RectTransform mmRect = mainMenuPanel.GetComponent<RectTransform>();
        mmRect.anchorMin = new Vector2(0.5f, 0.5f);
        mmRect.anchorMax = new Vector2(0.5f, 0.5f);
        mmRect.pivot = new Vector2(0.5f, 0.5f);
        mmRect.anchoredPosition = Vector2.zero;
        mmRect.sizeDelta = new Vector2(400f, 400f);

        VerticalLayoutGroup mmLayout = mainMenuPanel.AddComponent<VerticalLayoutGroup>();
        mmLayout.spacing = 20;
        mmLayout.padding = new RectOffset(30, 30, 30, 30);
        mmLayout.childAlignment = TextAnchor.UpperCenter;
        mmLayout.childControlWidth = true;
        mmLayout.childControlHeight = false;
        mmLayout.childForceExpandWidth = true;
        mmLayout.childForceExpandHeight = false;

        // Title
        GameObject titleObj = CreateUIObject(mainMenuPanel, "TitleText");
        titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 70f);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "Fantasy Life VN";
        titleTmp.fontSize = 36;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = new Color(0.95f, 0.85f, 0.55f); // Warm gold

        // Subtitle
        GameObject subtitleObj = CreateUIObject(mainMenuPanel, "SubtitleText");
        subtitleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 25f);
        TextMeshProUGUI subTmp = subtitleObj.AddComponent<TextMeshProUGUI>();
        subTmp.text = "Visual Novel / Life Simulation";
        subTmp.fontSize = 14;
        subTmp.alignment = TextAlignmentOptions.Center;
        subTmp.color = new Color(0.7f, 0.7f, 0.8f, 0.7f);

        // Spacer
        GameObject spacer = CreateUIObject(mainMenuPanel, "Spacer");
        spacer.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 20f);

        // New Game Button
        Button newGameBtn = CreateMenuButton(mainMenuPanel, "NewGameButton", "New Game",
            new Color(0.25f, 0.6f, 0.35f, 1f));

        // Load Game Button
        Button loadGameBtn = CreateMenuButton(mainMenuPanel, "LoadGameButton", "Load Game",
            new Color(0.3f, 0.45f, 0.7f, 1f));

        // Quit Button
        Button quitBtn = CreateMenuButton(mainMenuPanel, "QuitButton", "Quit",
            new Color(0.55f, 0.2f, 0.2f, 1f));

        // =====================================================================
        // 6. LOAD PANEL (hidden by default)
        // =====================================================================
        GameObject loadPanel = CreateUIObject(canvasObj, "LoadPanel");
        RectTransform lpRect = loadPanel.GetComponent<RectTransform>();
        lpRect.anchorMin = new Vector2(0.5f, 0.5f);
        lpRect.anchorMax = new Vector2(0.5f, 0.5f);
        lpRect.pivot = new Vector2(0.5f, 0.5f);
        lpRect.anchoredPosition = Vector2.zero;
        lpRect.sizeDelta = new Vector2(450f, 420f);

        Sprite bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        Image lpBg = loadPanel.AddComponent<Image>();
        if (bgSprite != null) { lpBg.sprite = bgSprite; lpBg.type = Image.Type.Sliced; }
        lpBg.color = new Color(0.1f, 0.1f, 0.14f, 0.95f);

        VerticalLayoutGroup lpLayout = loadPanel.AddComponent<VerticalLayoutGroup>();
        lpLayout.spacing = 12;
        lpLayout.padding = new RectOffset(20, 20, 20, 20);
        lpLayout.childAlignment = TextAnchor.UpperCenter;
        lpLayout.childControlWidth = true;
        lpLayout.childControlHeight = false;
        lpLayout.childForceExpandWidth = true;
        lpLayout.childForceExpandHeight = false;

        // Load Panel Title
        GameObject lpTitle = CreateUIObject(loadPanel, "LoadTitle");
        lpTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 40f);
        TextMeshProUGUI lpTitleTmp = lpTitle.AddComponent<TextMeshProUGUI>();
        lpTitleTmp.text = "LOAD GAME";
        lpTitleTmp.fontSize = 24;
        lpTitleTmp.fontStyle = FontStyles.Bold;
        lpTitleTmp.alignment = TextAlignmentOptions.Center;
        lpTitleTmp.color = Color.white;

        // 3 Load Slots
        TMP_Text[] loadSlotInfoTexts = new TMP_Text[3];
        Button[] loadSlotButtons = new Button[3];

        for (int i = 0; i < 3; i++)
        {
            int slotNum = i + 1;

            GameObject slotRow = CreateUIObject(loadPanel, "LoadSlotRow_" + slotNum);
            slotRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 75f);

            Image slotBg = slotRow.AddComponent<Image>();
            if (bgSprite != null) { slotBg.sprite = bgSprite; slotBg.type = Image.Type.Sliced; }
            slotBg.color = new Color(0.16f, 0.16f, 0.2f, 0.9f);

            HorizontalLayoutGroup slotLayout = slotRow.AddComponent<HorizontalLayoutGroup>();
            slotLayout.spacing = 10;
            slotLayout.padding = new RectOffset(15, 15, 10, 10);
            slotLayout.childAlignment = TextAnchor.MiddleLeft;
            slotLayout.childControlWidth = false;
            slotLayout.childControlHeight = false;
            slotLayout.childForceExpandWidth = false;
            slotLayout.childForceExpandHeight = false;

            // Info text
            GameObject infoObj = CreateUIObject(slotRow, "SlotInfo_" + slotNum);
            infoObj.GetComponent<RectTransform>().sizeDelta = new Vector2(280f, 55f);
            TextMeshProUGUI infoTmp = infoObj.AddComponent<TextMeshProUGUI>();
            infoTmp.text = $"Slot {slotNum}  |  --- Kosong ---";
            infoTmp.fontSize = 13;
            infoTmp.alignment = TextAlignmentOptions.Left;
            infoTmp.color = new Color(0.85f, 0.85f, 0.85f);
            infoTmp.raycastTarget = false;
            loadSlotInfoTexts[i] = infoTmp;

            // Load button
            loadSlotButtons[i] = CreateSimpleButton(slotRow, "LoadSlot_" + slotNum, "LOAD",
                new Vector2(80f, 40f), new Color(0.3f, 0.5f, 0.8f, 1f));
        }

        // Back Button
        Button backBtn = CreateMenuButton(loadPanel, "BackButton", "Back",
            new Color(0.4f, 0.35f, 0.35f, 1f), 50f);

        // Load panel default: hidden
        loadPanel.SetActive(false);

        // =====================================================================
        // 7. SAVELOADMANAGER (DontDestroyOnLoad)
        // =====================================================================
        GameObject slmObj = new GameObject("SaveLoadManager");
        slmObj.AddComponent<SaveLoadManager>();

        // =====================================================================
        // 8. MAINMENUMANAGER + WIRING
        // =====================================================================
        GameObject mmManagerObj = new GameObject("MainMenuManager");
        MainMenuManager mmManager = mmManagerObj.AddComponent<MainMenuManager>();

        SerializedObject so = new SerializedObject(mmManager);
        so.FindProperty("gameplaySceneName").stringValue = "Gamedigital";
        so.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuPanel;
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

        // =====================================================================
        // 9. SAVE SCENE
        // =====================================================================
        string scenePath = "Assets/Scenes/MainMenu.unity";

        // Pastikan folder Scenes ada
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            System.IO.Directory.CreateDirectory(Application.dataPath + "/Scenes");
            AssetDatabase.Refresh();
        }

        EditorSceneManager.SaveScene(newScene, scenePath);

        // Tambahkan ke Build Settings
        AddSceneToBuildSettings(scenePath);

        Debug.Log("MainMenuBuilder: ✅ Scene MainMenu berhasil dibuat di " + scenePath);
        EditorUtility.DisplayDialog("Berhasil!",
            "Scene Main Menu berhasil dibuat!\n\n" +
            "📁 Lokasi: Assets/Scenes/MainMenu.unity\n" +
            "✅ Sudah ditambahkan ke Build Settings\n\n" +
            "Pastikan scene 'Gamedigital' juga ada di Build Settings!",
            "OK");
    }

    // =========================================================================
    // HELPER METHODS
    // =========================================================================

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        // Cek apakah sudah ada
        foreach (var s in scenes)
        {
            if (s.path == scenePath) return; // Sudah ada, skip
        }

        // Tambahkan di index 0 (agar jadi scene pertama)
        scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log("MainMenuBuilder: Scene ditambahkan ke Build Settings di index 0.");
    }

    private static Button CreateMenuButton(GameObject parent, string name, string label,
        Color bgColor, float height = 55f)
    {
        GameObject btnObj = CreateUIObject(parent, name);
        RectTransform r = btnObj.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(0f, height);

        Sprite bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        Image img = btnObj.AddComponent<Image>();
        if (bgSprite != null) { img.sprite = bgSprite; img.type = Image.Type.Sliced; }
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(
            Mathf.Min(bgColor.r + 0.15f, 1f),
            Mathf.Min(bgColor.g + 0.15f, 1f),
            Mathf.Min(bgColor.b + 0.15f, 1f), 1f);
        colors.pressedColor = new Color(bgColor.r * 0.6f, bgColor.g * 0.6f, bgColor.b * 0.6f, 1f);
        btn.colors = colors;

        GameObject labelObj = CreateUIObject(btnObj, "Label");
        RectTransform lr = labelObj.GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero;
        lr.anchorMax = Vector2.one;
        lr.offsetMin = Vector2.zero;
        lr.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return btn;
    }

    private static Button CreateSimpleButton(GameObject parent, string name, string label,
        Vector2 size, Color bgColor)
    {
        GameObject btnObj = CreateUIObject(parent, name);
        RectTransform r = btnObj.GetComponent<RectTransform>();
        r.sizeDelta = size;

        Sprite bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        Image img = btnObj.AddComponent<Image>();
        if (bgSprite != null) { img.sprite = bgSprite; img.type = Image.Type.Sliced; }
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(
            Mathf.Min(bgColor.r + 0.15f, 1f),
            Mathf.Min(bgColor.g + 0.15f, 1f),
            Mathf.Min(bgColor.b + 0.15f, 1f), 1f);
        colors.pressedColor = new Color(bgColor.r * 0.7f, bgColor.g * 0.7f, bgColor.b * 0.7f, 1f);
        btn.colors = colors;

        GameObject labelObj = CreateUIObject(btnObj, "Label");
        RectTransform lr = labelObj.GetComponent<RectTransform>();
        lr.anchorMin = Vector2.zero;
        lr.anchorMax = Vector2.one;
        lr.offsetMin = Vector2.zero;
        lr.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 14;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return btn;
    }

    private static GameObject CreateUIObject(GameObject parent, string name)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent.transform, false);
        return obj;
    }
}
