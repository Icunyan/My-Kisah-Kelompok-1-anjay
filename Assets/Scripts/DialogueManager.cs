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
    [SerializeField] private Sprite spriteLucia;                // Lucia's portrait sprite
    [SerializeField] private Sprite spriteMarco;                // Marco's portrait sprite
    [SerializeField] private Sprite spriteRen;                  // Ren's portrait sprite

    [Header("Tombol Opsi (Maksimal 3)")]
    [SerializeField] private Button[] choiceButtons = new Button[3]; // Tombol pilihan yang bisa dipasang

    [Header("Pengaturan")]
    [SerializeField] private float typingSpeed = 0.02f;         // Kecepatan teks muncul per huruf

    private DialogueData currentDialogue;
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
                selectedSprite = spriteLara;
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

            currentLineText = line.text;
        }
        else
        {
            // Menggunakan legacy single-speaker
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
        if (GameManager.Instance != null && GameManager.Instance.isBadEnding)
        {
            if (isTyping)
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                }
                dialogueBodyText.text = currentLineText;
                isTyping = false;
            }
            else
            {
                GameManager.Instance.ResetGame();
            }
            return;
        }

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
            currentLineIndex++;
            DisplayLine();
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
        
        // Reset Lara's expression to the default (first) sprite
        ResetLaraExpression();

        currentDialogue = null;

        // Panggil callback agar GameManager tahu dialog sudah selesai
        if (onDialogueEndCallback != null)
        {
            onDialogueEndCallback.Invoke();
            onDialogueEndCallback = null;
        }
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
    public void StartSystemDialogue(string speaker, string text)
    {
        currentDialogue = null;
        currentLineIndex = 0;

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
}
