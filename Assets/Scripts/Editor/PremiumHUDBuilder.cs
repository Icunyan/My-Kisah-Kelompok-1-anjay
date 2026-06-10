using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PremiumHUDBuilder : EditorWindow
{
    [MenuItem("Tools/Build Premium HUD")]
    public static void BuildPremiumHUD()
    {
        // 1. Dapatkan Canvas
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError("BuildPremiumHUD: Canvas tidak ditemukan di scene!");
            return;
        }

        // Hapus Panel HUD lama yang bersarang di dalam Panel Kamar Ren jika ada
        Transform kamarRen = canvasObj.transform.Find("Panel Kamar Ren");
        if (kamarRen != null)
        {
            Transform oldNestedHUD = kamarRen.Find("Panel HUD");
            if (oldNestedHUD != null)
            {
                Debug.Log("BuildPremiumHUD: Menghapus Panel HUD legacy/bersarang di Kamar Ren...");
                Undo.DestroyObjectImmediate(oldNestedHUD.gameObject);
            }
        }

        // 2. Dapatkan Panel HUD
        Transform panelHUDT = canvasObj.transform.Find("Panel HUD");
        if (panelHUDT == null)
        {
            Debug.Log("BuildPremiumHUD: Panel HUD tidak ditemukan, membuat Panel HUD baru...");
            GameObject newPanelHUD = CreateUIObject(canvasObj, "Panel HUD");
            panelHUDT = newPanelHUD.transform;
            Undo.RegisterCreatedObjectUndo(newPanelHUD, "Build Premium HUD");
        }
        else
        {
            Undo.RegisterCompleteObjectUndo(panelHUDT.gameObject, "Build Premium HUD");
        }

        GameObject panelHUD = panelHUDT.gameObject;

        // Hapus elemen teks lama jika ada
        string[] oldTexts = { "HPText", "ATKText", "DEFText", "EnergyText", "DayText", "CycleText" };
        foreach (string oldName in oldTexts)
        {
            Transform t = panelHUD.transform.Find(oldName);
            if (t != null)
            {
                DestroyImmediate(t.gameObject);
            }
        }

        // Dapatkan atau tambahkan HorizontalLayoutGroup di Panel HUD
        HorizontalLayoutGroup mainLayout = panelHUD.GetComponent<HorizontalLayoutGroup>();
        if (mainLayout == null)
        {
            mainLayout = panelHUD.AddComponent<HorizontalLayoutGroup>();
        }
        mainLayout.spacing = 15;
        mainLayout.childAlignment = TextAnchor.MiddleCenter;
        mainLayout.childControlWidth = false;
        mainLayout.childControlHeight = false;
        mainLayout.childForceExpandWidth = false;
        mainLayout.childForceExpandHeight = false;

        // Set ukuran Panel HUD
        RectTransform hudRect = panelHUD.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0f, 1f);
        hudRect.anchorMax = new Vector2(1f, 1f);
        hudRect.pivot = new Vector2(0.5f, 1f);
        hudRect.anchoredPosition = new Vector2(0f, -10f);
        hudRect.sizeDelta = new Vector2(0f, 90f);

        // Bersihkan Card lama jika ada agar bersih
        string[] cardNames = { "Card_Research", "Card_HP", "Card_Stats", "Card_Energy", "Card_Cycle" };
        foreach (string cardName in cardNames)
        {
            Transform t = panelHUD.transform.Find(cardName);
            if (t != null)
            {
                DestroyImmediate(t.gameObject);
            }
        }

        // Helper untuk mempermudah pembuatan UI
        FontStyles boldStyle = FontStyles.Bold;

        // --- 1. CARD RESEARCH ---
        GameObject cardResearch = CreateCard(panelHUD, "Card_Research", new Vector2(250f, 75f));
        // Title Text
        GameObject researchTitle = CreateText(cardResearch, "Title", "CURE PROGRESS", TextAlignmentOptions.Left, 10, new Vector2(10f, 20f));
        researchTitle.GetComponent<TMP_Text>().color = new Color(0.6f, 0.8f, 1f, 0.7f); // Soft Cyan
        // Value Text
        GameObject researchVal = CreateText(cardResearch, "Value", "0 / 30", TextAlignmentOptions.Right, 14, new Vector2(-10f, 20f), boldStyle);
        // Bar Background
        GameObject researchBarBg = CreateUIObject(cardResearch, "BarBackground");
        SetRect(researchBarBg, new Vector2(230f, 12f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f));
        researchBarBg.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);
        // Bar Fill
        GameObject researchBarFill = CreateUIObject(researchBarBg, "Fill");
        SetRect(researchBarFill, Vector2.zero, new Vector2(0f, 0f), new Vector2(1f, 1f), Vector2.zero);
        Image researchFillImg = researchBarFill.AddComponent<Image>();
        researchFillImg.color = new Color(0.55f, 0.35f, 0.85f, 1f); // Sleek Magical Purple
        researchFillImg.type = Image.Type.Filled;
        researchFillImg.fillMethod = Image.FillMethod.Horizontal;
        researchFillImg.fillOrigin = 0; // Left

        // --- 3. CARD ENERGY ---
        GameObject cardEnergy = CreateCard(panelHUD, "Card_Energy", new Vector2(200f, 75f));
        // Title Text
        GameObject energyTitle = CreateText(cardEnergy, "Title", "ENERGY", TextAlignmentOptions.Left, 10, new Vector2(10f, 20f));
        energyTitle.GetComponent<TMP_Text>().color = new Color(1f, 1f, 1f, 0.6f);
        // Value Text
        GameObject energyVal = CreateText(cardEnergy, "Value", "3 / 3", TextAlignmentOptions.Right, 14, new Vector2(-10f, 20f), boldStyle);
        // Container for Orbs
        GameObject orbsContainer = CreateUIObject(cardEnergy, "OrbsContainer");
        SetRect(orbsContainer, new Vector2(120f, 20f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f));
        HorizontalLayoutGroup orbsLayout = orbsContainer.AddComponent<HorizontalLayoutGroup>();
        orbsLayout.spacing = 10;
        orbsLayout.childAlignment = TextAnchor.MiddleCenter;
        orbsLayout.childControlWidth = false;
        orbsLayout.childControlHeight = false;
        orbsLayout.childForceExpandWidth = false;
        orbsLayout.childForceExpandHeight = false;

        Image[] energyOrbs = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject orb = CreateUIObject(orbsContainer, "Orb_" + i);
            SetRect(orb, new Vector2(16f, 16f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero);
            Image orbImg = orb.AddComponent<Image>();
            // default circle sprite can be fetched or left blank (solid block)
            // let's fetch default Unity UI background sprite (circle-ish if sliced right, or just solid square)
            Sprite circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            if (circle != null) orbImg.sprite = circle;
            orbImg.color = new Color(1f, 0.85f, 0.1f, 1f); // yellow
            energyOrbs[i] = orbImg;
        }

        // --- 4. CARD CYCLE ---
        GameObject cardCycle = CreateCard(panelHUD, "Card_Cycle", new Vector2(170f, 75f));
        Image cycleBgImg = cardCycle.GetComponent<Image>();
        cycleBgImg.color = new Color(0.95f, 0.65f, 0.25f, 0.85f); // Morning warm gold by default
        // Day Text
        GameObject dayValText = CreateText(cardCycle, "Day", "DAY 1", TextAlignmentOptions.Center, 18, new Vector2(0f, 10f), boldStyle);
        // Cycle Text
        GameObject cycleValText = CreateText(cardCycle, "CyclePhase", "MORNING", TextAlignmentOptions.Center, 11, new Vector2(0f, -16f));
        cycleValText.GetComponent<TMP_Text>().color = new Color(1f, 1f, 1f, 0.85f);

        // --- 5. PANEL BAD ENDING ---
        Sprite laradedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/laraded.png");
        Transform oldBadEnding = canvasObj.transform.Find("Panel Bad Ending");
        if (oldBadEnding != null)
        {
            DestroyImmediate(oldBadEnding.gameObject);
        }
        GameObject panelBadEnding = CreateUIObject(canvasObj, "Panel Bad Ending");
        RectTransform badEndingRect = panelBadEnding.GetComponent<RectTransform>();
        badEndingRect.anchorMin = Vector2.zero;
        badEndingRect.anchorMax = Vector2.one;
        badEndingRect.offsetMin = Vector2.zero;
        badEndingRect.offsetMax = Vector2.zero;

        Image badEndingImg = panelBadEnding.AddComponent<Image>();
        if (laradedSprite != null)
        {
            badEndingImg.sprite = laradedSprite;
        }
        else
        {
            Debug.LogWarning("BuildPremiumHUD: laraded.png tidak ditemukan di Assets/!");
        }
        badEndingImg.color = Color.white;
        
        // Posisikan tepat sebelum Panel Dialogue agar di bawahnya secara rendering
        Transform dialoguePanelT = canvasObj.transform.Find("Panel Dialogue");
        if (dialoguePanelT != null)
        {
            panelBadEnding.transform.SetSiblingIndex(dialoguePanelT.GetSiblingIndex());
        }
        panelBadEnding.SetActive(false);

        // 3. Sambungkan ke GameManager
        GameManager gm = UnityEngine.Object.FindObjectOfType<GameManager>();
        if (gm != null)
        {
            SerializedObject so = new SerializedObject(gm);
            
            // Wire premium references
            so.FindProperty("researchFillImage").objectReferenceValue = researchFillImg;
            so.FindProperty("researchValText").objectReferenceValue = researchVal.GetComponent<TMP_Text>();
            so.FindProperty("energyValText").objectReferenceValue = energyVal.GetComponent<TMP_Text>();
            so.FindProperty("dayValText").objectReferenceValue = dayValText.GetComponent<TMP_Text>();
            so.FindProperty("cycleValText").objectReferenceValue = cycleValText.GetComponent<TMP_Text>();
            so.FindProperty("cycleCardBg").objectReferenceValue = cycleBgImg;
            so.FindProperty("badEndingPanel").objectReferenceValue = panelBadEnding;

            // Dapatkan atau buat DialogueData Bad Ending asset
            string dialoguePath = "Assets/Scripts/Dialogue/bad_ending.asset";
            DialogueData badEndingSO = AssetDatabase.LoadAssetAtPath<DialogueData>(dialoguePath);
            if (badEndingSO == null)
            {
                badEndingSO = ScriptableObject.CreateInstance<DialogueData>();
                badEndingSO.characterName = "SYSTEM";
                badEndingSO.characterID = "";
                badEndingSO.dialogueLines = new string[] { "Lara has passed away because you did not visit her for 5 consecutive days..." };
                
                // Pastikan folder exist
                if (!AssetDatabase.IsValidFolder("Assets/Scripts/Dialogue"))
                {
                    System.IO.Directory.CreateDirectory(Application.dataPath + "/Scripts/Dialogue");
                    AssetDatabase.Refresh();
                }
                
                AssetDatabase.CreateAsset(badEndingSO, dialoguePath);
                AssetDatabase.SaveAssets();
                Debug.Log("BuildPremiumHUD: Membuat asset ScriptableObject baru untuk dialog Bad Ending.");
            }
            so.FindProperty("badEndingDialogue").objectReferenceValue = badEndingSO;

            // Dapatkan atau buat DialogueData Day Limit Bad Ending asset
            string dayLimitPath = "Assets/Scripts/Dialogue/day_limit_bad_ending.asset";
            DialogueData dayLimitSO = AssetDatabase.LoadAssetAtPath<DialogueData>(dayLimitPath);
            if (dayLimitSO == null)
            {
                dayLimitSO = ScriptableObject.CreateInstance<DialogueData>();
                dayLimitSO.characterName = "SYSTEM";
                dayLimitSO.characterID = "";
                dayLimitSO.dialogueLines = new string[] { "Time runs out! You reached day 40 and failed to save Lara..." };
                
                // Pastikan folder exist
                if (!AssetDatabase.IsValidFolder("Assets/Scripts/Dialogue"))
                {
                    System.IO.Directory.CreateDirectory(Application.dataPath + "/Scripts/Dialogue");
                    AssetDatabase.Refresh();
                }
                
                AssetDatabase.CreateAsset(dayLimitSO, dayLimitPath);
                AssetDatabase.SaveAssets();
                Debug.Log("BuildPremiumHUD: Membuat asset ScriptableObject baru untuk dialog Day Limit Bad Ending.");
            }
            so.FindProperty("dayLimitDialogue").objectReferenceValue = dayLimitSO;

            // Wire energy orbs array
            SerializedProperty orbsProp = so.FindProperty("energyOrbs");
            orbsProp.ClearArray();
            orbsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                orbsProp.GetArrayElementAtIndex(i).objectReferenceValue = energyOrbs[i];
            }

            // Dapatkan DialogueData GIM_Story asset
            string gimStoryPath = "Assets/Scripts/Dialogue/GIM_Story.asset";
            DialogueData gimStorySO = AssetDatabase.LoadAssetAtPath<DialogueData>(gimStoryPath);
            if (gimStorySO != null)
            {
                so.FindProperty("openingDialogue").objectReferenceValue = gimStorySO;
                Debug.Log("BuildPremiumHUD: Menghubungkan openingDialogue ke GIM_Story.asset.");
            }
            else
            {
                Debug.LogWarning("BuildPremiumHUD: GIM_Story.asset tidak ditemukan di " + gimStoryPath);
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(gm);
            
            // Simpan scene
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gm.gameObject.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(gm.gameObject.scene);
            
            Debug.Log("BuildPremiumHUD: Berhasil membangun HUD Premium, Panel Bad Ending, dan menghubungkan GameManager!");
        }
        else
        {
            Debug.LogError("BuildPremiumHUD: GameManager tidak ditemukan di scene!");
        }
    }

    private static GameObject CreateCard(GameObject parent, string name, Vector2 size)
    {
        GameObject card = CreateUIObject(parent, name);
        RectTransform r = card.GetComponent<RectTransform>();
        r.sizeDelta = size;
        
        Image img = card.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // semi-transparan gelap
        // Pasang standard UI background sprite jika ada untuk rounded corners
        Sprite bgSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        if (bgSprite != null)
        {
            img.sprite = bgSprite;
            img.type = Image.Type.Sliced;
        }

        return card;
    }

    private static GameObject CreateText(GameObject parent, string name, string content, TextAlignmentOptions alignment, float fontSize, Vector2 anchoredPos, FontStyles fontStyle = FontStyles.Normal)
    {
        GameObject textObj = CreateUIObject(parent, name);
        RectTransform r = textObj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0f, 0f);
        r.anchorMax = new Vector2(1f, 1f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        r.anchoredPosition = anchoredPos;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.fontStyle = fontStyle;
        tmp.color = Color.white;
        tmp.raycastTarget = false;

        return textObj;
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
}
