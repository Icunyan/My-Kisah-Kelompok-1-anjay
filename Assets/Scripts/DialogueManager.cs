using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    // Singleton agar mudah dipanggil dari script lain (seperti GameManager)
    public static DialogueManager Instance { get; private set; }

    [Header("UI Panel References")]
    [SerializeField] private GameObject panelDialogue;          // Panel utama kotak dialog
    [SerializeField] private TMP_Text speakerNameText;          // Teks nama karakter
    [SerializeField] private TMP_Text dialogueBodyText;         // Teks isi dialog
    [SerializeField] private GameObject choicesContainer;       // Objek kontainer tombol pilihan
    [SerializeField] private Button nextButton;                 // Tombol untuk lanjut (klik layar/next)

    [Header("Portrait References")]
    [SerializeField] private Image portraitImage;               // Image component for character portrait
    [SerializeField] private Sprite spriteLara;                 // Lara's portrait sprite
    [SerializeField] private Sprite spriteLaraNgomong;          // Lara's talking portrait sprite
    [SerializeField] private Sprite spriteLucia;                // Lucia's portrait sprite
    [SerializeField] private Sprite spriteMarco;                // Marco's portrait sprite
    [SerializeField] private Sprite spriteRen;                  // Ren's portrait sprite

    [Header("Visual Novel Character Emotion Sprites")]
    [Tooltip("Lara's emotion sprites (Size 3: 0=Default, 1=Emotion 2, 2=Emotion 3)")]
    [SerializeField] private Sprite[] laraSprites = new Sprite[3];

    [Tooltip("Ren's emotion sprites (Size 4: 0=Default, 1=Emotion 2, 2=Emotion 3, 3=Emotion 4)")]
    [SerializeField] private Sprite[] renSprites = new Sprite[4];

    [Tooltip("Marco's emotion sprites (Size 1: 0=Default)")]
    [SerializeField] private Sprite[] marcoSprites = new Sprite[1];

    [Tooltip("Lucia's emotion sprites (Size 8: 0=Default, 1=Emotion 2, 2=Emotion 3, ..., 7=Emotion 8)")]
    [SerializeField] private Sprite[] luciaSprites = new Sprite[8];

    [Header("Visual Novel Cutscene UI References")]
    [SerializeField] private Image vnBackground;                // Background kustom cutscene
    [SerializeField] private Image vnCharLeft;                  // Karakter kiri
    [SerializeField] private Image vnCharCenter;                // Karakter tengah
    [SerializeField] private Image vnCharRight;                 // Karakter kanan
    [SerializeField] private Image vnScreenFlash;               // Overlay putih untuk efek flash

    [Header("Tombol Opsi (Maksimal 3)")]
    [SerializeField] private Button[] choiceButtons = new Button[3]; // Tombol pilihan yang bisa dipasang

    [Header("Pengaturan")]
    [SerializeField] private float typingSpeed = 0.02f;         // Kecepatan teks muncul per huruf

    private DialogueData currentDialogue;
    private DialogueData lastActiveDialogueNode; // Menghafal node dialog yang baru saja aktif
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private string currentLineText = "";
    private Coroutine typingCoroutine;

    // Callback aksi yang dipanggil saat dialog selesai diputar
    private System.Action onDialogueEndCallback;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Tutup kotak dialog dan tombol pilihan di awal permainan
        if (panelDialogue != null) panelDialogue.SetActive(false);
        if (choicesContainer != null) choicesContainer.SetActive(false);

        // Daftarkan listener klik ke tombol Next
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        // Daftarkan listener klik ke setiap tombol pilihan
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i; // Closure capture untuk menyimpan indeks yang benar
            if (choiceButtons[i] != null)
            {
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(index));
            }
        }

        // Inisialisasi programmatik visual novel layout
        InitializeVNUI();
    }

    /// <summary>
    /// Memulai rangkaian percakapan dari Scriptable Object DialogueData.
    /// </summary>
    public void StartDialogue(DialogueData dialogueData, System.Action onDialogueEnd = null)
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("DialogueManager: Mencoba memulai dialog dengan data kosong (null).");
            return;
        }

        currentDialogue = dialogueData;
        currentLineIndex = 0;
        onDialogueEndCallback = onDialogueEnd;

        // Bawa seluruh UI VN dan Panel Dialog ke depan agar tidak tertutup ruangan
        BringDialogueUIToFront();

        if (panelDialogue != null) panelDialogue.SetActive(true);
        if (choicesContainer != null) choicesContainer.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(true);

        DisplayLine();
    }

    private void UpdatePortrait(string speakerID)
    {
        if (portraitImage == null) return;

        if (string.IsNullOrEmpty(speakerID))
        {
            portraitImage.gameObject.SetActive(false);
            return;
        }

        string id = speakerID.ToUpper().Trim();
        Sprite selectedSprite = null;

        switch (id)
        {
            case "LARA":
                selectedSprite = spriteLaraNgomong != null ? spriteLaraNgomong : spriteLara;
                break;
            case "LUCIA":
                selectedSprite = spriteLucia;
                break;
            case "MARCO":
                selectedSprite = spriteMarco;
                break;
            case "REN":
                selectedSprite = spriteRen;
                break;
        }

        if (selectedSprite != null)
        {
            portraitImage.sprite = selectedSprite;
            portraitImage.gameObject.SetActive(true);
        }
        else
        {
            portraitImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Menampilkan baris dialog saat ini.
    /// </summary>
    private void DisplayLine()
    {
        if (currentDialogue == null)
        {
            EndDialogue();
            return;
        }

        // Simpan node saat ini sebagai node yang terakhir aktif
        lastActiveDialogueNode = currentDialogue;

        // Cek apakah menggunakan sistem multi-speaker
        if (currentDialogue.lines != null && currentDialogue.lines.Length > 0)
        {
            if (currentLineIndex >= currentDialogue.lines.Length)
            {
                CheckForChoices();
                return;
            }

            var line = currentDialogue.lines[currentLineIndex];
            if (speakerNameText != null)
            {
                speakerNameText.text = line.speakerName;
            }

            // Tentukan sprite pembicara
            Sprite charSprite = line.customPortrait != null ? line.customPortrait : GetEmotionSprite(line.speakerID, line.emotionIndex);

            // 1. Update Background
            if (vnBackground != null)
            {
                if (line.customBackground != null)
                {
                    vnBackground.sprite = line.customBackground;
                    vnBackground.color = Color.white;
                    vnBackground.gameObject.SetActive(true);
                }
                else
                {
                    // Jika tidak ada background kustom
                    bool showDefaultBlack = (GameManager.Instance != null && !GameManager.Instance.IsInRoomGameplay())
                                            || (currentDialogue != null && currentDialogue.isCutscene);
                    if (showDefaultBlack)
                    {
                        vnBackground.sprite = null;
                        vnBackground.color = Color.black;
                        vnBackground.gameObject.SetActive(true);
                    }
                    else if (currentLineIndex == 0)
                    {
                        // Di awal dialog, matikan background VN agar tembus pandang ke room
                        vnBackground.gameObject.SetActive(false);
                    }
                }
            }

            // 2. Update Character Sprite Placement
            if (vnCharLeft != null) vnCharLeft.gameObject.SetActive(false);
            if (vnCharCenter != null) vnCharCenter.gameObject.SetActive(false);
            if (vnCharRight != null) vnCharRight.gameObject.SetActive(false);

            bool hasVnPosition = !string.IsNullOrEmpty(line.characterPosition) && line.characterPosition.ToUpper().Trim() != "NONE";

            if (hasVnPosition && charSprite != null)
            {
                // Sembunyikan portrait kecil di box
                if (portraitImage != null) portraitImage.gameObject.SetActive(false);

                string pos = line.characterPosition.ToUpper().Trim();
                Image targetImage = null;

                if (pos == "L" && vnCharLeft != null) targetImage = vnCharLeft;
                else if (pos == "C" && vnCharCenter != null) targetImage = vnCharCenter;
                else if (pos == "R" && vnCharRight != null) targetImage = vnCharRight;

                if (targetImage != null)
                {
                    targetImage.sprite = charSprite;
                    targetImage.color = Color.white;
                    targetImage.gameObject.SetActive(true);

                    // Pemicu Animasi Character (misal: jump)
                    if (!string.IsNullOrEmpty(line.animationTrigger))
                    {
                        string anim = line.animationTrigger.ToLower().Trim();
                        if (anim == "jump")
                        {
                            StartCoroutine(PlayJumpEffect(targetImage));
                        }
                    }
                }
            }
            else
            {
                // Tampilkan portrait kecil biasa di dialog box
                if (line.customPortrait != null)
                {
                    if (portraitImage != null)
                    {
                        portraitImage.sprite = line.customPortrait;
                        portraitImage.gameObject.SetActive(true);
                    }
                }
                else
                {
                    UpdatePortrait(line.speakerID);
                }
            }

            // 3. Pemicu Animasi Screen (shake, flash)
            if (!string.IsNullOrEmpty(line.animationTrigger))
            {
                string anim = line.animationTrigger.ToLower().Trim();
                if (anim == "shake")
                {
                    StartCoroutine(PlayShakeEffect());
                }
                else if (anim == "flash")
                {
                    StartCoroutine(PlayFlashEffect());
                }
            }

            currentLineText = line.text;
        }
        else
        {
            // Menggunakan legacy single-speaker
            if (vnBackground != null)
            {
                bool showDefaultBlack = GameManager.Instance != null && !GameManager.Instance.IsInRoomGameplay();
                if (showDefaultBlack)
                {
                    vnBackground.sprite = null;
                    vnBackground.color = Color.black;
                    vnBackground.gameObject.SetActive(true);
                }
                else
                {
                    vnBackground.gameObject.SetActive(false);
                }
            }
            if (vnCharLeft != null) vnCharLeft.gameObject.SetActive(false);
            if (vnCharCenter != null) vnCharCenter.gameObject.SetActive(false);
            if (vnCharRight != null) vnCharRight.gameObject.SetActive(false);

            if (currentDialogue.dialogueLines == null || currentDialogue.dialogueLines.Length == 0)
            {
                EndDialogue();
                return;
            }

            if (currentLineIndex >= currentDialogue.dialogueLines.Length)
            {
                CheckForChoices();
                return;
            }

            if (speakerNameText != null)
            {
                speakerNameText.text = currentDialogue.characterName;
            }
            UpdatePortrait(currentDialogue.characterID);
            currentLineText = currentDialogue.dialogueLines[currentLineIndex];
        }

        // Mulai animasi mengetik teks dialog
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeTextCoroutine(currentLineText));
    }

    /// <summary>
    /// Efek mengetik huruf demi huruf.
    /// </summary>
    private IEnumerator TypeTextCoroutine(string text)
    {
        isTyping = true;
        dialogueBodyText.text = "";

        foreach (char c in text.ToCharArray())
        {
            dialogueBodyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    /// <summary>
    /// Fungsi yang dipanggil ketika layar/tombol next ditekan.
    /// </summary>
    public void OnNextButtonClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        if (isTyping)
        {
            // Jika teks masih mengetik, langsung selesaikan teks baris ini
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            dialogueBodyText.text = currentLineText;
            isTyping = false;
        }
        else
        {
            // Pindah ke baris teks berikutnya
            if (currentDialogue != null)
            {
                currentLineIndex++;
                DisplayLine();
            }
            else
            {
                // Jika tidak ada currentDialogue (seperti system dialogue), langsung akhiri
                EndDialogue();
            }
        }
    }

    /// <summary>
    /// Menampilkan pilihan percabangan jika ada.
    /// </summary>
    private void CheckForChoices()
    {
        if (currentDialogue.hasChoices && currentDialogue.choices != null && currentDialogue.choices.Length > 0)
        {
            // Sembunyikan tombol lanjut, tampilkan kontainer pilihan
            if (nextButton != null) nextButton.gameObject.SetActive(false);
            if (choicesContainer != null) choicesContainer.SetActive(true);

            // Atur teks dan aktifkan tombol pilihan yang sesuai
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] == null) continue;

                if (i < currentDialogue.choices.Length)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    
                    // Isi teks tombol pilihan (cari komponen TMP_Text di dalam tombol)
                    TMP_Text btnText = choiceButtons[i].GetComponentInChildren<TMP_Text>();
                    if (btnText != null)
                    {
                        btnText.text = currentDialogue.choices[i].choiceText;
                    }
                }
                else
                {
                    // Sembunyikan tombol sisa yang tidak terpakai
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
        }
        else if (currentDialogue.nextDialogue != null)
        {
            // Transisi ke dialog linear berikutnya
            StartDialogue(currentDialogue.nextDialogue, onDialogueEndCallback);
        }
        else
        {
            // Jika tidak ada pilihan percabangan dan tidak ada node linear berikutnya, akhiri dialog
            EndDialogue();
        }
    }

    /// <summary>
    /// Aksi ketika salah satu pilihan opsi ditekan.
    /// </summary>
    private void OnChoiceSelected(int index)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

        if (currentDialogue == null || index >= currentDialogue.choices.Length) return;

        DialogueData nextNode = currentDialogue.choices[index].nextDialogue;

        if (nextNode != null)
        {
            // Jalankan percakapan cabang berikutnya
            StartDialogue(nextNode, onDialogueEndCallback);
        }
        else
        {
            // Jika cabang berikutnya kosong, dialog selesai
            EndDialogue();
        }
    }

    /// <summary>
    /// Menutup kotak dialog dan mengembalikan status UI ke awal.
    /// </summary>
    public void EndDialogue()
    {
        if (panelDialogue != null) panelDialogue.SetActive(false);
        if (choicesContainer != null) choicesContainer.SetActive(false);
        
        // Sembunyikan UI Visual Novel
        if (vnBackground != null) vnBackground.gameObject.SetActive(false);
        if (vnCharLeft != null) vnCharLeft.gameObject.SetActive(false);
        if (vnCharCenter != null) vnCharCenter.gameObject.SetActive(false);
        if (vnCharRight != null) vnCharRight.gameObject.SetActive(false);

        // Reset Lara's expression to the default (first) sprite
        ResetLaraExpression();

        currentDialogue = null;

        // Panggil callback agar GameManager tahu dialog sudah selesai
        if (onDialogueEndCallback != null)
        {
            System.Action callback = onDialogueEndCallback;
            onDialogueEndCallback = null;
            callback.Invoke();
        }
    }

    /// <summary>
    /// Mengembalikan node dialog yang baru saja aktif atau sedang diputar.
    /// </summary>
    public DialogueData GetLastActiveNode()
    {
        return lastActiveDialogueNode;
    }

    /// <summary>
    /// Mereset ekspresi wajah Lara kembali ke default (sprite pertama).
    /// </summary>
    public void ResetLaraExpression()
    {
        if (portraitImage != null && spriteLara != null)
        {
            portraitImage.sprite = spriteLara;
            portraitImage.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Memulai dialog buatan sistem (seperti game over).
    /// </summary>
    public void StartSystemDialogue(string speaker, string text, System.Action onDialogueEnd = null)
    {
        currentDialogue = null;
        currentLineIndex = 0;
        onDialogueEndCallback = onDialogueEnd;

        // Bawa seluruh UI VN dan Panel Dialog ke depan agar tidak tertutup ruangan
        BringDialogueUIToFront();

        if (panelDialogue != null) panelDialogue.SetActive(true);
        if (choicesContainer != null) choicesContainer.SetActive(false);
        if (nextButton != null) nextButton.gameObject.SetActive(true);

        if (speakerNameText != null) speakerNameText.text = speaker;
        currentLineText = text;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeTextCoroutine(text));
    }

    private Sprite GetDefaultPortrait(string speakerID)
    {
        if (string.IsNullOrEmpty(speakerID)) return null;

        string id = speakerID.ToUpper().Trim();
        switch (id)
        {
            case "LARA":
                return spriteLaraNgomong != null ? spriteLaraNgomong : spriteLara;
            case "LUCIA":
                return spriteLucia;
            case "MARCO":
                return spriteMarco;
            case "REN":
                return spriteRen;
            default:
                return null;
        }
    }

    public Sprite GetEmotionSprite(string speakerID, int emotionIndex)
    {
        if (string.IsNullOrEmpty(speakerID)) return null;
        string id = speakerID.ToUpper().Trim();

        switch (id)
        {
            case "REN":
                if (renSprites != null && emotionIndex >= 0 && emotionIndex < renSprites.Length)
                {
                    if (renSprites[emotionIndex] != null) return renSprites[emotionIndex];
                    if (renSprites.Length > 0 && renSprites[0] != null) return renSprites[0];
                }
                break;
            case "LARA":
                if (laraSprites != null && emotionIndex >= 0 && emotionIndex < laraSprites.Length)
                {
                    if (laraSprites[emotionIndex] != null) return laraSprites[emotionIndex];
                    if (laraSprites.Length > 0 && laraSprites[0] != null) return laraSprites[0];
                }
                break;
            case "MARCO":
                if (marcoSprites != null && emotionIndex >= 0 && emotionIndex < marcoSprites.Length)
                {
                    if (marcoSprites[emotionIndex] != null) return marcoSprites[emotionIndex];
                    if (marcoSprites.Length > 0 && marcoSprites[0] != null) return marcoSprites[0];
                }
                break;
            case "LUCIA":
                if (luciaSprites != null && emotionIndex >= 0 && emotionIndex < luciaSprites.Length)
                {
                    if (luciaSprites[emotionIndex] != null) return luciaSprites[emotionIndex];
                    if (luciaSprites.Length > 0 && luciaSprites[0] != null) return luciaSprites[0];
                }
                break;
        }

        return GetDefaultPortrait(speakerID);
    }

    private void InitializeVNUI()
    {
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null) return;

        // 1. Inisialisasi vnBackground
        if (vnBackground == null)
        {
            Transform bgT = canvasObj.transform.Find("VN_Background");
            if (bgT == null)
            {
                GameObject bgObj = new GameObject("VN_Background", typeof(RectTransform));
                bgObj.transform.SetParent(canvasObj.transform, false);
                bgObj.transform.SetAsFirstSibling(); // tempatkan di belakang agar di bawah panel dialog

                RectTransform rect = bgObj.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                vnBackground = bgObj.AddComponent<Image>();
                vnBackground.color = Color.black;
                vnBackground.raycastTarget = false;
                bgObj.SetActive(false);
            }
            else
            {
                vnBackground = bgT.GetComponent<Image>();
            }
        }

        // 2. Inisialisasi VN_Characters Container
        Transform charsContainerT = canvasObj.transform.Find("VN_Characters");
        GameObject charsContainer = null;
        if (charsContainerT == null)
        {
            charsContainer = new GameObject("VN_Characters", typeof(RectTransform));
            charsContainer.transform.SetParent(canvasObj.transform, false);
            if (vnBackground != null)
            {
                charsContainer.transform.SetSiblingIndex(vnBackground.gameObject.transform.GetSiblingIndex() + 1);
            }
            RectTransform rect = charsContainer.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        else
        {
            charsContainer = charsContainerT.gameObject;
        }

        // Helper setup slot karakter
        if (vnCharLeft == null) vnCharLeft = SetupCharSlot(charsContainer, "VN_CharLeft", new Vector2(0.25f, 0.4f));
        if (vnCharCenter == null) vnCharCenter = SetupCharSlot(charsContainer, "VN_CharCenter", new Vector2(0.5f, 0.4f));
        if (vnCharRight == null) vnCharRight = SetupCharSlot(charsContainer, "VN_CharRight", new Vector2(0.75f, 0.4f));

        // 3. Inisialisasi vnScreenFlash
        if (vnScreenFlash == null)
        {
            Transform flashT = canvasObj.transform.Find("VN_ScreenFlash");
            if (flashT == null)
            {
                GameObject flashObj = new GameObject("VN_ScreenFlash", typeof(RectTransform));
                flashObj.transform.SetParent(canvasObj.transform, false);
                flashObj.transform.SetAsLastSibling(); // Paling depan

                RectTransform rect = flashObj.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                vnScreenFlash = flashObj.AddComponent<Image>();
                vnScreenFlash.color = new Color(1f, 1f, 1f, 0f);
                vnScreenFlash.raycastTarget = false;
            }
            else
            {
                vnScreenFlash = flashT.GetComponent<Image>();
            }
        }
    }

    private Image SetupCharSlot(GameObject parent, string name, Vector2 anchorX)
    {
        Transform t = parent.transform.Find(name);
        if (t != null) return t.GetComponent<Image>();

        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent.transform, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(anchorX.x - 0.22f, 0f);
        rect.anchorMax = new Vector2(anchorX.x + 0.22f, 0.9f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = obj.AddComponent<Image>();
        img.preserveAspect = true;
        img.color = new Color(1f, 1f, 1f, 0f);
        img.raycastTarget = false;
        obj.SetActive(false);

        return img;
    }

    private IEnumerator PlayShakeEffect()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayImpact();

        // Shake the VN elements (background and characters) instead of the dialogue panel
        // so that the text itself does not vibrate, maintaining readability.
        RectTransform bgRt = vnBackground != null ? vnBackground.GetComponent<RectTransform>() : null;

        GameObject canvasObj = GameObject.Find("Canvas");
        RectTransform charsRt = null;
        if (canvasObj != null)
        {
            Transform t = canvasObj.transform.Find("VN_Characters");
            if (t != null) charsRt = t.GetComponent<RectTransform>();
        }

        Vector2 bgOriginalPos = bgRt != null ? bgRt.anchoredPosition : Vector2.zero;
        Vector2 charsOriginalPos = charsRt != null ? charsRt.anchoredPosition : Vector2.zero;

        float elapsed = 0f;
        float duration = 0.4f;
        float magnitude = 15f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            if (bgRt != null) bgRt.anchoredPosition = bgOriginalPos + new Vector2(x, y);
            if (charsRt != null) charsRt.anchoredPosition = charsOriginalPos + new Vector2(x, y);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (bgRt != null) bgRt.anchoredPosition = bgOriginalPos;
        if (charsRt != null) charsRt.anchoredPosition = charsOriginalPos;
    }

    private IEnumerator PlayJumpEffect(Image characterImage)
    {
        if (characterImage == null) yield break;

        RectTransform rt = characterImage.GetComponent<RectTransform>();
        Vector2 originalPos = rt.anchoredPosition;
        float elapsed = 0f;
        float duration = 0.25f;
        float jumpHeight = 35f;

        while (elapsed < duration)
        {
            float normalizedTime = elapsed / duration;
            float y = jumpHeight * (1f - 4f * Mathf.Pow(normalizedTime - 0.5f, 2f));
            rt.anchoredPosition = new Vector2(originalPos.x, originalPos.y + Mathf.Max(0f, y));
            elapsed += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = originalPos;
    }

    private IEnumerator PlayFlashEffect()
    {
        if (vnScreenFlash == null) yield break;

        vnScreenFlash.color = Color.white;
        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            float alpha = 1f - (elapsed / duration);
            vnScreenFlash.color = new Color(1f, 1f, 1f, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }
        vnScreenFlash.color = new Color(1f, 1f, 1f, 0f);
    }

    private void BringDialogueUIToFront()
    {
        if (vnBackground != null) vnBackground.transform.SetAsLastSibling();
        
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            Transform charsContainerT = canvasObj.transform.Find("VN_Characters");
            if (charsContainerT != null) charsContainerT.SetAsLastSibling();
        }

        if (panelDialogue != null) panelDialogue.transform.SetAsLastSibling();
        if (vnScreenFlash != null) vnScreenFlash.transform.SetAsLastSibling();
    }
}
