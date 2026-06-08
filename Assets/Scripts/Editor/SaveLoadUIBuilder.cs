using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor tool untuk otomatis membangun UI Save/Load Panel + tombol HUD di scene gameplay.
/// Cara pakai: Buka scene Gamedigital, lalu klik menu Tools > Build Save/Load UI
/// </summary>
public class SaveLoadUIBuilder : EditorWindow
{
    [MenuItem("Tools/Build Save-Load UI")]
    public static void BuildSaveLoadUI()
    {
        // =====================================================================
        // 1. CARI CANVAS
        // =====================================================================
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("SaveLoadUIBuilder: Canvas tidak ditemukan di scene! Pastikan ada Canvas.");
            return;
        }

        // =====================================================================
        // 2. BUAT SAVELOADMANAGER (DontDestroyOnLoad)
        // =====================================================================
        SaveLoadManager existingSLM = Object.FindObjectOfType<SaveLoadManager>();
        if (existingSLM == null)
        {
            GameObject slmObj = new GameObject("SaveLoadManager");
            slmObj.AddComponent<SaveLoadManager>();
            Undo.RegisterCreatedObjectUndo(slmObj, "Build SaveLoad UI");
            Debug.Log("SaveLoadUIBuilder: Membuat GameObject SaveLoadManager.");
        }
        else
        {
            Debug.Log("SaveLoadUIBuilder: SaveLoadManager sudah ada, skip.");
        }

        // =====================================================================
        // 3. HAPUS UI LAMA JIKA ADA (untuk rebuild bersih)
        // =====================================================================
        Transform oldHudBtns = canvasObj.transform.Find("HUD_SaveLoadButtons");
        if (oldHudBtns != null) DestroyImmediate(oldHudBtns.gameObject);

        Transform oldPanel = canvasObj.transform.Find("SaveLoadPanel");
        if (oldPanel != null) DestroyImmediate(oldPanel.gameObject);

        // =====================================================================
        // 4. BUAT TOMBOL HUD (SAVE / LOAD / MENU) - di pojok kanan atas
        // =====================================================================
        GameObject hudBtns = CreateUIObject(canvasObj, "HUD_SaveLoadButtons");
        RectTransform hudRect = hudBtns.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(1f, 1f);
        hudRect.anchorMax = new Vector2(1f, 1f);
        hudRect.pivot = new Vector2(1f, 1f);
        hudRect.anchoredPosition = new Vector2(-10f, -10f);
        hudRect.sizeDelta = new Vector2(310f, 40f);

        HorizontalLayoutGroup hudLayout = hudBtns.AddComponent<HorizontalLayoutGroup>();
        hudLayout.spacing = 8;
        hudLayout.childAlignment = TextAnchor.MiddleRight;
        hudLayout.childControlWidth = false;
        hudLayout.childControlHeight = false;
        hudLayout.childForceExpandWidth = false;
        hudLayout.childForceExpandHeight = false;

        // Tombol SAVE
        Button saveBtn = CreateSimpleButton(hudBtns, "SaveButton", "SAVE",
            new Vector2(90f, 35f), new Color(0.2f, 0.55f, 0.3f, 0.9f));

        // Tombol LOAD
        Button loadBtn = CreateSimpleButton(hudBtns, "LoadButton", "LOAD",
            new Vector2(90f, 35f), new Color(0.25f, 0.45f, 0.7f, 0.9f));

        // Tombol MENU
        Button menuBtn = CreateSimpleButton(hudBtns, "MenuButton", "MENU",
            new Vector2(90f, 35f), new Color(0.55f, 0.25f, 0.25f, 0.9f));

        // Posisikan tepat sebelum Panel Dialogue agar tidak menutupi dialog
        Transform dialoguePanelT = canvasObj.transform.Find("Panel Dialogue");
        if (dialoguePanelT != null)
        {
            hudBtns.transform.SetSiblingIndex(dialoguePanelT.GetSiblingIndex());
        }

        // =====================================================================
        // 5. BUAT PANEL SAVE/LOAD (overlay full screen)
        // =====================================================================
        GameObject saveLoadPanel = CreateUIObject(canvasObj, "SaveLoadPanel");
        RectTransform panelRect = saveLoadPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Background overlay gelap
        Image panelBg = saveLoadPanel.AddComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0.85f);

        // Posisikan di paling atas Canvas hierarchy (terakhir di-render = di depan)
        saveLoadPanel.transform.SetAsLastSibling();

        // --- Container utama (tengah layar) ---
        GameObject container = CreateUIObject(saveLoadPanel, "Container");
        RectTransform contRect = container.GetComponent<RectTransform>();
        contRect.anchorMin = new Vector2(0.5f, 0.5f);
        contRect.anchorMax = new Vector2(0.5f, 0.5f);
        contRect.pivot = new Vector2(0.5f, 0.5f);
        contRect.anchoredPosition = Vector2.zero;
        contRect.sizeDelta = new Vector2(500f, 420f);

        Image contBg = container.AddComponent<Image>();
        Sprite bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        if (bgSprite != null) { contBg.sprite = bgSprite; contBg.type = Image.Type.Sliced; }
        contBg.color = new Color(0.12f, 0.12f, 0.16f, 0.95f);

        VerticalLayoutGroup contLayout = container.AddComponent<VerticalLayoutGroup>();
        contLayout.spacing = 10;
        contLayout.padding = new RectOffset(20, 20, 15, 15);
        contLayout.childAlignment = TextAnchor.UpperCenter;
        contLayout.childControlWidth = true;
        contLayout.childControlHeight = false;
        contLayout.childForceExpandWidth = true;
        contLayout.childForceExpandHeight = false;

        // --- Judul Panel ---
        GameObject titleObj = CreateUIObject(container, "PanelTitle");
        titleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 40f);
        TextMeshProUGUI titleTmp = titleObj.AddComponent<TextMeshProUGUI>();
        titleTmp.text = "SAVE GAME";
        titleTmp.fontSize = 22;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = Color.white;

        // --- Tab Buttons (Save / Load) ---
        GameObject tabRow = CreateUIObject(container, "TabRow");
        tabRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 35f);
        HorizontalLayoutGroup tabLayout = tabRow.AddComponent<HorizontalLayoutGroup>();
        tabLayout.spacing = 10;
        tabLayout.childAlignment = TextAnchor.MiddleCenter;
        tabLayout.childControlWidth = false;
        tabLayout.childControlHeight = false;
        tabLayout.childForceExpandWidth = false;
        tabLayout.childForceExpandHeight = false;

        Button tabSaveBtn = CreateSimpleButton(tabRow, "TabSave", "SAVE",
            new Vector2(120f, 30f), new Color(0.3f, 0.7f, 0.4f, 1f));
        Button tabLoadBtn = CreateSimpleButton(tabRow, "TabLoad", "LOAD",
            new Vector2(120f, 30f), new Color(0.25f, 0.25f, 0.3f, 1f));

        // --- 3 Slot rows ---
        TMP_Text[] slotInfoTexts = new TMP_Text[3];
        Button[] slotActionButtons = new Button[3];
        TMP_Text[] slotActionLabels = new TMP_Text[3];
        Button[] slotDeleteButtons = new Button[3];

        for (int i = 0; i < 3; i++)
        {
            int slotNum = i + 1;
            GameObject slotRow = CreateUIObject(container, "SlotRow_" + slotNum);
            slotRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 80f);

            Image slotBg = slotRow.AddComponent<Image>();
            if (bgSprite != null) { slotBg.sprite = bgSprite; slotBg.type = Image.Type.Sliced; }
            slotBg.color = new Color(0.18f, 0.18f, 0.22f, 0.9f);

            HorizontalLayoutGroup slotLayout = slotRow.AddComponent<HorizontalLayoutGroup>();
            slotLayout.spacing = 10;
            slotLayout.padding = new RectOffset(12, 12, 8, 8);
            slotLayout.childAlignment = TextAnchor.MiddleLeft;
            slotLayout.childControlWidth = false;
            slotLayout.childControlHeight = false;
            slotLayout.childForceExpandWidth = false;
            slotLayout.childForceExpandHeight = false;

            // Info Text (lebar, sisi kiri)
            GameObject infoObj = CreateUIObject(slotRow, "SlotInfo_" + slotNum);
            RectTransform infoR = infoObj.GetComponent<RectTransform>();
            infoR.sizeDelta = new Vector2(280f, 64f);
            TextMeshProUGUI infoTmp = infoObj.AddComponent<TextMeshProUGUI>();
            infoTmp.text = $"Slot {slotNum}  |  --- Kosong ---";
            infoTmp.fontSize = 13;
            infoTmp.alignment = TextAlignmentOptions.Left;
            infoTmp.color = new Color(0.85f, 0.85f, 0.85f);
            infoTmp.raycastTarget = false;
            slotInfoTexts[i] = infoTmp;

            // Action Button (Save/Load)
            Button actionBtn = CreateSimpleButton(slotRow, "SlotAction_" + slotNum, "SAVE",
                new Vector2(80f, 40f), new Color(0.3f, 0.6f, 0.4f, 1f));
            slotActionButtons[i] = actionBtn;
            slotActionLabels[i] = actionBtn.GetComponentInChildren<TMP_Text>();

            // Delete Button
            Button delBtn = CreateSimpleButton(slotRow, "SlotDelete_" + slotNum, "DEL",
                new Vector2(50f, 40f), new Color(0.6f, 0.2f, 0.2f, 1f));
            slotDeleteButtons[i] = delBtn;
        }

        // --- Tombol Close (X) ---
        GameObject closeObj = CreateUIObject(saveLoadPanel, "CloseButton");
        RectTransform closeRect = closeObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-15f, -15f);
        closeRect.sizeDelta = new Vector2(40f, 40f);

        Image closeBg = closeObj.AddComponent<Image>();
        closeBg.color = new Color(0.6f, 0.15f, 0.15f, 0.9f);
        if (bgSprite != null) { closeBg.sprite = bgSprite; closeBg.type = Image.Type.Sliced; }
        Button closeBtn = closeObj.AddComponent<Button>();
        var closeCols = closeBtn.colors;
        closeCols.highlightedColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        closeBtn.colors = closeCols;

        GameObject closeLabel = CreateUIObject(closeObj, "Label");
        closeLabel.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        closeLabel.GetComponent<RectTransform>().anchorMax = Vector2.one;
        closeLabel.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        closeLabel.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        TextMeshProUGUI closeTmp = closeLabel.AddComponent<TextMeshProUGUI>();
        closeTmp.text = "X";
        closeTmp.fontSize = 20;
        closeTmp.fontStyle = FontStyles.Bold;
        closeTmp.alignment = TextAlignmentOptions.Center;
        closeTmp.color = Color.white;

        // Panel default: nonaktif
        saveLoadPanel.SetActive(false);

        // =====================================================================
        // 6. BUAT SAVELOADUI COMPONENT & WIRING
        // =====================================================================
        // Attach SaveLoadUI ke panel
        SaveLoadUI slUI = saveLoadPanel.AddComponent<SaveLoadUI>();
        SerializedObject slSO = new SerializedObject(slUI);

        slSO.FindProperty("saveLoadPanel").objectReferenceValue = saveLoadPanel;
        slSO.FindProperty("tabSaveButton").objectReferenceValue = tabSaveBtn;
        slSO.FindProperty("tabLoadButton").objectReferenceValue = tabLoadBtn;
        slSO.FindProperty("panelTitleText").objectReferenceValue = titleTmp;

        slSO.FindProperty("slot1InfoText").objectReferenceValue = slotInfoTexts[0];
        slSO.FindProperty("slot1ActionButton").objectReferenceValue = slotActionButtons[0];
        slSO.FindProperty("slot1ActionLabel").objectReferenceValue = slotActionLabels[0];
        slSO.FindProperty("slot1DeleteButton").objectReferenceValue = slotDeleteButtons[0];

        slSO.FindProperty("slot2InfoText").objectReferenceValue = slotInfoTexts[1];
        slSO.FindProperty("slot2ActionButton").objectReferenceValue = slotActionButtons[1];
        slSO.FindProperty("slot2ActionLabel").objectReferenceValue = slotActionLabels[1];
        slSO.FindProperty("slot2DeleteButton").objectReferenceValue = slotDeleteButtons[1];

        slSO.FindProperty("slot3InfoText").objectReferenceValue = slotInfoTexts[2];
        slSO.FindProperty("slot3ActionButton").objectReferenceValue = slotActionButtons[2];
        slSO.FindProperty("slot3ActionLabel").objectReferenceValue = slotActionLabels[2];
        slSO.FindProperty("slot3DeleteButton").objectReferenceValue = slotDeleteButtons[2];

        slSO.FindProperty("closeButton").objectReferenceValue = closeBtn;
        slSO.ApplyModifiedProperties();

        // =====================================================================
        // 7. WIRING KE GAMEMANAGER
        // =====================================================================
        GameManager gm = Object.FindObjectOfType<GameManager>();
        if (gm != null)
        {
            SerializedObject gmSO = new SerializedObject(gm);
            gmSO.FindProperty("saveLoadUI").objectReferenceValue = slUI;
            gmSO.FindProperty("saveButton").objectReferenceValue = saveBtn;
            gmSO.FindProperty("loadButton").objectReferenceValue = loadBtn;
            gmSO.FindProperty("mainMenuButton").objectReferenceValue = menuBtn;
            gmSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(gm);
        }
        else
        {
            Debug.LogWarning("SaveLoadUIBuilder: GameManager tidak ditemukan! Assign manual nanti.");
        }

        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("SaveLoadUIBuilder: ✅ UI Save/Load berhasil dibangun dan dihubungkan!");
        EditorUtility.DisplayDialog("Berhasil!", 
            "UI Save/Load berhasil dibangun!\n\n" +
            "• 3 tombol HUD (SAVE, LOAD, MENU) di pojok kanan atas\n" +
            "• Panel Save/Load overlay dengan 3 slot\n" +
            "• Semua sudah terhubung ke GameManager", "OK");
    }

    // =========================================================================
    // HELPER METHODS
    // =========================================================================

    private static Button CreateSimpleButton(GameObject parent, string name, string label,
        Vector2 size, Color bgColor)
    {
        GameObject btnObj = CreateUIObject(parent, name);
        RectTransform r = btnObj.GetComponent<RectTransform>();
        r.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        Sprite bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        if (bgSprite != null) { img.sprite = bgSprite; img.type = Image.Type.Sliced; }
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(
            Mathf.Min(bgColor.r + 0.15f, 1f),
            Mathf.Min(bgColor.g + 0.15f, 1f),
            Mathf.Min(bgColor.b + 0.15f, 1f),
            1f);
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
