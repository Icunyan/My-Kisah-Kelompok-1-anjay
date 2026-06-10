using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum CyclePhase
{
    Morning,   // Pagi
    Afternoon, // Siang
    Night      // Malam
}

public class GameManager : MonoBehaviour
{
    // Singleton agar mudah diakses dari script lainnya
    public static GameManager Instance { get; private set; }

    [Header("Progres Penelitian Kutukan")]
    [Tooltip("Tingkat kemajuan riset kutukan (Level 0 - 30).")]
    public int researchProgress = 0;
    [Tooltip("Tingkat kemajuan maksimal untuk menyelesaikan riset.")]
    public int maxResearchLevel = 30;

    [Header("Manajemen Hari & Energi")]
    [Tooltip("Hari ke-berapa sekarang.")]
    public int day = 1;
    [Tooltip("Fase waktu saat ini (Morning, Afternoon, Night).")]
    public CyclePhase cyclePhase = CyclePhase.Morning;
    [Tooltip("Energi player saat ini.")]
    public int energy = 3;
    [Tooltip("Maksimal energi player yang dapat dimiliki.")]
    public int maxEnergy = 3;

    [Header("UI Teks Tampilan")]
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text cycleText;

    [Header("UI Premium HUD References")]
    [SerializeField] private Image researchFillImage;         // Progress bar fill untuk riset
    [SerializeField] private TMP_Text researchValText;        // Teks nilai riset
    [SerializeField] private Image energyFillImage;           // Progress bar fill untuk Energy
    [SerializeField] private Image[] energyOrbs;              // Indikator orb energi
    [SerializeField] private TMP_Text energyValText;          // Teks nilai Energy
    [SerializeField] private TMP_Text dayValText;             // Teks nilai Hari
    [SerializeField] private TMP_Text cycleValText;           // Teks fase siklus
    [SerializeField] private Image cycleCardBg;               // Background kartu waktu (untuk warna dinamis)
    
    [Header("Save/Load UI")]
    [SerializeField] private SaveLoadUI saveLoadUI;       // Reference ke SaveLoadUI script
    [SerializeField] private Button saveButton;            // Tombol Save di HUD
    [SerializeField] private Button loadButton;            // Tombol Load di HUD
    [SerializeField] private Button mainMenuButton;        // Tombol kembali ke Main Menu

    [Header("Milestones & Friend Visit Dialogues")]
    [SerializeField] private DialogueData level10Dialogue;
    [SerializeField] private DialogueData level15Dialogue;
    [SerializeField] private DialogueData level20Dialogue;
    [SerializeField] private DialogueData level30Dialogue;
    [SerializeField] private DialogueData luciaVisitDialogue;
    [SerializeField] private DialogueData marcoVisitDialogue;

    [Header("Bad Ending Settings")]
    [SerializeField] private GameObject badEndingPanel;
    [SerializeField] private DialogueData badEndingDialogue;
    [SerializeField] private DialogueData dayLimitDialogue;
    public bool isBadEnding { get; private set; } = false;
    private int daysSinceLastLaraVisit = 0;

    // Status bantuan teman
    private bool isLaraWatchedByFriend = false;
    private int laraWardDaysRemaining = 0;
    private bool isResearchBuffed = false;

    // Accessor untuk state bantuan teman (Save/Load)
    public bool GetIsLaraWatchedByFriend() => isLaraWatchedByFriend;
    public void SetIsLaraWatchedByFriend(bool value) => isLaraWatchedByFriend = value;
    public int GetLaraWardDaysRemaining() => laraWardDaysRemaining;
    public void SetLaraWardDaysRemaining(int value) => laraWardDaysRemaining = value;
    public bool GetIsResearchBuffed() => isResearchBuffed;
    public void SetIsResearchBuffed(bool value) => isResearchBuffed = value;

    // --- Accessor methods untuk SaveLoadManager ---
    public int GetDaysSinceLastLaraVisit() => daysSinceLastLaraVisit;
    public void SetDaysSinceLastLaraVisit(int value) => daysSinceLastLaraVisit = value;

    [Header("Panel Ruangan (kamar_ren & kamar_lara)")]
    [SerializeField] private GameObject kamarRenPanel;  // Panel utama (Kamar Ren)
    [SerializeField] private GameObject kamarLaraPanel; // Panel Kamar Lara

    [Header("Tombol-Tombol Aksi Utama (Kamar Ren)")]
    [SerializeField] private Button trainButton;         // Tombol Melatih Diri
    [SerializeField] private Button visitLaraButton;     // Tombol Visit Lara
    [SerializeField] private Button restButton;          // Tombol Rest (Istirahat)

    [Header("Tombol-Tombol Kamar Lara")]
    [SerializeField] private GameObject laraOptionsPanel; // Panel yang berisi 3 tombol di Kamar Lara (Talk, Upgrade, Back)
    [SerializeField] private Button talkToLaraButton;     // Tombol Talk to Lara
    [SerializeField] private Button backToRenButton;      // Tombol Kembali ke Kamar Ren

    [Header("Data Dialog Lara (Scriptable Object)")]
    [SerializeField] private DialogueData talkToLaraDialogue; // Taruh file Scriptable Object dialog Lara di sini

    [Header("Data Dialog Pembuka (Opening Dialogue)")]
    [SerializeField] private DialogueData openingDialogue; // Dialog pembuka saat New Game

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

    /// <summary>
    /// Mengetahui apakah pemain sedang berada di dalam gameplay ruangan (Kamar Ren atau Kamar Lara).
    /// </summary>
    public bool IsInRoomGameplay()
    {
        return (kamarRenPanel != null && kamarRenPanel.activeSelf) || (kamarLaraPanel != null && kamarLaraPanel.activeSelf);
    }

    private void Start()
    {
        // Hubungkan tombol-tombol dengan fungsinya di kode
        if (trainButton != null) trainButton.onClick.AddListener(ResearchCurse);
        if (visitLaraButton != null) visitLaraButton.onClick.AddListener(VisitLara);
        if (restButton != null) restButton.onClick.AddListener(Rest);

        if (talkToLaraButton != null) talkToLaraButton.onClick.AddListener(TalkToLara);
        if (backToRenButton != null) backToRenButton.onClick.AddListener(ReturnToKamarRen);

        // Tombol Save/Load di HUD
        if (saveButton != null) saveButton.onClick.AddListener(() => { if (saveLoadUI != null) saveLoadUI.OpenPanel(true); });
        if (loadButton != null) loadButton.onClick.AddListener(() => { if (saveLoadUI != null) saveLoadUI.OpenPanel(false); });
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        // Reparent HUD buttons to Canvas root so they remain visible in both Kamar Ren and Kamar Lara
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            for (int i = 0; i < canvasObj.transform.childCount; i++)
            {
                Transform child = canvasObj.transform.GetChild(i);
                Debug.Log($"[DEBUG CANVAS CHILD] Index {i}: {child.name}, activeSelf: {child.gameObject.activeSelf}");
            }

            Transform[] allTransforms = canvasObj.GetComponentsInChildren<Transform>(true);
            Transform hudContainer = null;
            foreach (Transform t in allTransforms)
            {
                if (t.gameObject.name == "HUD_SaveLoadButtons")
                {
                    hudContainer = t;
                    break;
                }
            }

            if (hudContainer != null)
            {
                hudContainer.SetParent(canvasObj.transform, false);
                hudContainer.gameObject.SetActive(true);
                
                Transform dialoguePanelT = canvasObj.transform.Find("Panel Dialogue");
                if (dialoguePanelT != null)
                {
                    hudContainer.SetSiblingIndex(dialoguePanelT.GetSiblingIndex());
                }
                else
                {
                    hudContainer.SetAsLastSibling();
                }
            }
            else if (saveButton != null)
            {
                saveButton.transform.SetParent(canvasObj.transform, false);
                saveButton.gameObject.SetActive(true);
                if (loadButton != null)
                {
                    loadButton.transform.SetParent(canvasObj.transform, false);
                    loadButton.gameObject.SetActive(true);
                }
                if (mainMenuButton != null)
                {
                    mainMenuButton.transform.SetParent(canvasObj.transform, false);
                    mainMenuButton.gameObject.SetActive(true);
                }
            }
        }

        UpdateUI();

        // Putar dialog pembuka jika ini adalah game baru
        if (!SaveLoadManager.isLoadedGame && openingDialogue != null && DialogueManager.Instance != null)
        {
            // Sembunyikan ruangan di awal saat opening cutscene diputar
            if (kamarRenPanel != null) kamarRenPanel.SetActive(false);
            if (kamarLaraPanel != null) kamarLaraPanel.SetActive(false);
            if (badEndingPanel != null) badEndingPanel.SetActive(false);

            DialogueManager.Instance.StartDialogue(openingDialogue, () => {
                // Tampilkan Kamar Ren setelah opening selesai
                if (kamarRenPanel != null) kamarRenPanel.SetActive(true);
                UpdateUI();
            });
        }
        else
        {
            // Jika memuat game atau tidak ada dialog pembuka, langsung ke Kamar Ren
            if (kamarRenPanel != null) kamarRenPanel.SetActive(true);
            if (kamarLaraPanel != null) kamarLaraPanel.SetActive(false);
            if (badEndingPanel != null) badEndingPanel.SetActive(false);
            UpdateUI();
        }

        // Putar BGM Gameplay saat masuk ke scene ini
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayBGM();
        }
    }

    /// <summary>
    /// Memperbarui semua tampilan teks dan status aktif tombol di layar.
    /// </summary>
    public void UpdateUI()
    {
        // Update teks hari dan energi (Legacy)
        if (dayText != null) dayText.text = "Hari: " + day;
        if (energyText != null) energyText.text = "Energi: " + energy + " / " + maxEnergy;

        // Update teks fase waktu (Legacy)
        if (cycleText != null)
        {
            switch (cyclePhase)
            {
                case CyclePhase.Morning:
                    cycleText.text = "Waktu: Pagi (Morning)";
                    break;
                case CyclePhase.Afternoon:
                    cycleText.text = "Waktu: Siang (Afternoon)";
                    break;
                case CyclePhase.Night:
                    cycleText.text = "Waktu: Malam (Night)";
                    break;
            }
        }

        // Update Premium Visual HUD untuk Cure Progress
        if (researchFillImage != null)
        {
            researchFillImage.fillAmount = (float)researchProgress / maxResearchLevel;
        }
        if (researchValText != null)
        {
            researchValText.text = researchProgress + " / " + maxResearchLevel;
        }

        if (energyFillImage != null)
        {
            energyFillImage.fillAmount = (float)energy / maxEnergy;
        }
        if (energyValText != null) energyValText.text = energy + " / " + maxEnergy;

        if (energyOrbs != null && energyOrbs.Length > 0)
        {
            for (int i = 0; i < energyOrbs.Length; i++)
            {
                if (energyOrbs[i] != null)
                {
                    energyOrbs[i].color = (i < energy) ? new Color(1f, 0.85f, 0.1f, 1f) : new Color(0.2f, 0.2f, 0.2f, 0.5f);
                }
            }
        }

        if (dayValText != null) dayValText.text = "DAY " + day;

        if (cycleValText != null)
        {
            switch (cyclePhase)
            {
                case CyclePhase.Morning:
                    cycleValText.text = "MORNING";
                    if (cycleCardBg != null) cycleCardBg.color = new Color(0.95f, 0.65f, 0.25f, 0.85f); // Warm Gold
                    break;
                case CyclePhase.Afternoon:
                    cycleValText.text = "AFTERNOON";
                    if (cycleCardBg != null) cycleCardBg.color = new Color(0.25f, 0.6f, 0.85f, 0.85f); // Sky Blue
                    break;
                case CyclePhase.Night:
                    cycleValText.text = "NIGHT";
                    if (cycleCardBg != null) cycleCardBg.color = new Color(0.18f, 0.12f, 0.35f, 0.85f); // Night Indigo
                    break;
            }
        }

        // Aktifkan / Matikan tombol berdasarkan energi
        bool hasEnergy = energy > 0;
        if (trainButton != null) trainButton.interactable = hasEnergy;
        if (visitLaraButton != null) visitLaraButton.interactable = hasEnergy;

        // Rest (Istirahat) selalu aktif tanpa syarat
        if (restButton != null) restButton.interactable = true;
    }

    /// <summary>
    /// Aksi Riset Kutukan: Mengurangi 1 energi dan meningkatkan progres.
    /// </summary>
    public void ResearchCurse()
    {
        if (energy > 0)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMagicAura();

            energy -= 1;
            
            // Tentukan penambahan progres
            int progressGain = isResearchBuffed ? 3 : 1;
            isResearchBuffed = false; // Reset buff

            researchProgress = Mathf.Min(researchProgress + progressGain, maxResearchLevel);
            Debug.Log($"Melakukan Riset! Menambah +{progressGain} progres. Progres saat ini: {researchProgress}/{maxResearchLevel}");

            // Cek Milestone
            bool triggeredMilestone = CheckMilestones();

            if (!triggeredMilestone)
            {
                // Roll untuk interaksi teman (25% chance)
                RollForFriendEncounter();
            }

            // Majukan siklus waktu
            AdvanceCycle();
        }
        else
        {
            Debug.LogWarning("Energi habis! Silakan lakukan Rest (Istirahat).");
        }
    }

    /// <summary>
    /// Aksi Rest (Istirahat): Memulihkan energi dan memajukan siklus waktu.
    /// </summary>
    public void Rest()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayEnergyFlow();

        energy = maxEnergy;
        Debug.Log("Rest berhasil! Energi pulih sepenuhnya.");
        AdvanceCycle();
    }

    private void AdvanceCycle()
    {
        if (cyclePhase == CyclePhase.Morning)
        {
            cyclePhase = CyclePhase.Afternoon;
            Debug.Log("Waktu berubah ke Afternoon (Siang).");
        }
        else if (cyclePhase == CyclePhase.Afternoon)
        {
            cyclePhase = CyclePhase.Night;
            Debug.Log("Waktu berubah ke Night (Malam).");
        }
        else if (cyclePhase == CyclePhase.Night)
        {
            cyclePhase = CyclePhase.Morning;
            day += 1; // Berganti ke hari baru
            Debug.Log("Hari baru dimulai: Hari " + day + " - Morning (Pagi).");
            OnDayChanged();
        }
        UpdateUI();
    }

    private void SetRenRoomButtonsInteractable(bool interactable)
    {
        if (trainButton != null) trainButton.interactable = interactable && energy > 0;
        if (visitLaraButton != null) visitLaraButton.interactable = interactable && energy > 0;
        if (restButton != null) restButton.interactable = interactable;
    }

    private bool CheckMilestones()
    {
        if (researchProgress == 10 && level10Dialogue != null && DialogueManager.Instance != null)
        {
            TriggerMilestoneDialogue(level10Dialogue);
            return true;
        }
        else if (researchProgress == 15 && level15Dialogue != null && DialogueManager.Instance != null)
        {
            TriggerMilestoneDialogue(level15Dialogue);
            return true;
        }
        else if (researchProgress == 20 && level20Dialogue != null && DialogueManager.Instance != null)
        {
            TriggerMilestoneDialogue(level20Dialogue);
            return true;
        }
        else if (researchProgress == 30 && level30Dialogue != null && DialogueManager.Instance != null)
        {
            SetRenRoomButtonsInteractable(false);
            DialogueManager.Instance.StartDialogue(level30Dialogue, () => {
                ReturnToMainMenu();
            });
            return true;
        }
        return false;
    }

    private void TriggerMilestoneDialogue(DialogueData dialogue)
    {
        SetRenRoomButtonsInteractable(false);
        DialogueManager.Instance.StartDialogue(dialogue, () => {
            SetRenRoomButtonsInteractable(true);
            UpdateUI();
        });
    }

    private void RollForFriendEncounter()
    {
        float rand = Random.value;
        if (rand < 0.25f)
        {
            float friendRand = Random.value;
            if (friendRand < 0.5f && luciaVisitDialogue != null)
            {
                SetRenRoomButtonsInteractable(false);
                DialogueManager.Instance.StartDialogue(luciaVisitDialogue, OnFriendEncounterFinished);
            }
            else if (marcoVisitDialogue != null)
            {
                SetRenRoomButtonsInteractable(false);
                DialogueManager.Instance.StartDialogue(marcoVisitDialogue, OnFriendEncounterFinished);
            }
        }
    }

    private void OnFriendEncounterFinished()
    {
        SetRenRoomButtonsInteractable(true);
        if (DialogueManager.Instance != null)
        {
            DialogueData lastNode = DialogueManager.Instance.GetLastActiveNode();
            if (lastNode != null && !string.IsNullOrEmpty(lastNode.actionID))
            {
                ProcessDialogueAction(lastNode.actionID);
            }
        }
        UpdateUI();
    }

    public void ProcessDialogueAction(string actionID)
    {
        if (string.IsNullOrEmpty(actionID)) return;

        string id = actionID.ToUpper().Trim();
        switch (id)
        {
            case "MARCO_WATCH":
                isLaraWatchedByFriend = true;
                Debug.Log("Gameplay Action: Marco setuju untuk menemani Lara. Lara tidak akan sakit besok.");
                break;
            case "MARCO_MEDICINE":
                daysSinceLastLaraVisit = 0;
                Debug.Log("Gameplay Action: Marco memberikan obat penurun demam. Sickness timer di-reset!");
                break;
            case "LUCIA_COLLABORATIVE":
                isResearchBuffed = true;
                Debug.Log("Gameplay Action: Lucia membantu riset. Progres berikutnya bertambah 3!");
                break;
            case "LUCIA_WARD":
                laraWardDaysRemaining = 3;
                Debug.Log("Gameplay Action: Lucia memasang pelindung mana. Lara aman selama 3 hari.");
                break;
            default:
                Debug.LogWarning("Gameplay Action: actionID tidak dikenal: " + actionID);
                break;
        }
    }

    /// <summary>
    /// Aksi Visit Lara: Pindah ke Kamar Lara. Consumes 1 Energy.
    /// </summary>
    public void VisitLara()
    {
        if (energy > 0)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySwoosh();

            energy -= 1; // Mengurangi 1 energi
            daysSinceLastLaraVisit = 0; // Reset visit counter
            if (kamarRenPanel != null) kamarRenPanel.SetActive(false);
            if (kamarLaraPanel != null) kamarLaraPanel.SetActive(true);
            
            // Aktifkan panel pilihan Kamar Lara
            if (laraOptionsPanel != null) laraOptionsPanel.SetActive(true);

            // Reset ekspresi Lara ke default (first expression)
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ResetLaraExpression();
            }

            Debug.Log("Pindah ke Kamar Lara. Energi berkurang 1.");
            UpdateUI(); // Perbarui tampilan HUD energi
        }
        else
        {
            Debug.LogWarning("Tidak bisa berkunjung, energi tidak cukup.");
        }
    }

    /// <summary>
    /// Aksi Kembali ke Kamar Ren: Kembali ke ruangan utama.
    /// </summary>
    public void ReturnToKamarRen()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySwoosh();

        if (kamarRenPanel != null) kamarRenPanel.SetActive(true);
        if (kamarLaraPanel != null) kamarLaraPanel.SetActive(false);

        Debug.Log("Kembali ke Kamar Ren.");
    }

    /// <summary>
    /// Aksi Talk to Lara: Memutar dialog Lara menggunakan DialogueManager.
    /// </summary>
    public void TalkToLara()
    {
        if (talkToLaraDialogue != null && DialogueManager.Instance != null)
        {
            // Sembunyikan panel pilihan Kamar Lara agar dialog fokus tampil
            if (laraOptionsPanel != null) laraOptionsPanel.SetActive(false);

            // Jalankan dialog. Berikan fungsi callback agar panel pilihan muncul kembali setelah selesai
            DialogueManager.Instance.StartDialogue(talkToLaraDialogue, OnTalkToLaraFinished);
        }
        else
        {
            Debug.LogWarning("DialogueManager atau dialog SO Lara belum dipasang di inspector.");
        }
    }

    /// <summary>
    /// Callback yang dipanggil otomatis saat dialog dengan Lara selesai.
    /// </summary>
    private void OnTalkToLaraFinished()
    {
        // Munculkan kembali tombol pilihan Kamar Lara setelah dialog selesai
        if (laraOptionsPanel != null) laraOptionsPanel.SetActive(true);
        Debug.Log("Selesai berbicara dengan Lara.");
    }



    /// <summary>
    /// Kembali ke scene Main Menu.
    /// </summary>
    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenuNew");
    }

    private void OnDayChanged()
    {
        if (day >= 40)
        {
            TriggerBadEnding();
            return;
        }

        // Cek bantuan teman
        if (isLaraWatchedByFriend)
        {
            isLaraWatchedByFriend = false; // Gunakan efeknya
            Debug.Log("Hari Berganti: Marco menemani Lara, Lara terawat dengan baik.");
        }
        else if (laraWardDaysRemaining > 0)
        {
            laraWardDaysRemaining--;
            Debug.Log($"Hari Berganti: Ward pelindung Lucia aktif. Sisa hari: {laraWardDaysRemaining}");
        }
        else
        {
            daysSinceLastLaraVisit++;
            Debug.Log($"Hari Berganti: Tidak ada kunjungan Lara selama {daysSinceLastLaraVisit} hari.");
            if (daysSinceLastLaraVisit >= 5)
            {
                TriggerBadEnding();
            }
        }
    }

    public void TriggerBadEnding()
    {
        isBadEnding = true;
        if (badEndingPanel != null) badEndingPanel.SetActive(true);
        if (kamarRenPanel != null) kamarRenPanel.SetActive(false);
        if (kamarLaraPanel != null) kamarLaraPanel.SetActive(false);

        if (AudioManager.Instance != null) AudioManager.Instance.PlayTensionBGM();

        if (DialogueManager.Instance != null)
        {
            DialogueData dialogueToShow = (day >= 40) ? dayLimitDialogue : badEndingDialogue;
            string fallbackMsg = (day >= 40)
                ? "Time runs out! You reached day 40 and failed to save Lara..."
                : "Lara has passed away because you did not visit her for 5 consecutive days...";

            if (dialogueToShow != null)
            {
                DialogueManager.Instance.StartDialogue(dialogueToShow, ResetGame);
            }
            else
            {
                DialogueManager.Instance.StartSystemDialogue("SYSTEM", fallbackMsg, ResetGame);
            }
        }
    }

    public void ResetGame()
    {
        day = 1;
        cyclePhase = CyclePhase.Morning;
        energy = maxEnergy;
        researchProgress = 0;
        daysSinceLastLaraVisit = 0;
        isBadEnding = false;

        isLaraWatchedByFriend = false;
        laraWardDaysRemaining = 0;
        isResearchBuffed = false;

        if (badEndingPanel != null) badEndingPanel.SetActive(false);
        if (kamarRenPanel != null) kamarRenPanel.SetActive(true);
        if (kamarLaraPanel != null) kamarLaraPanel.SetActive(false);

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.EndDialogue();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameplayBGM();
        }

        UpdateUI();
    }
}
