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

    [Header("Statistik Player (Stats)")]
    [Tooltip("Jumlah nyawa (HP) saat ini. Nilai awal default = 100.")]
    public int hp = 100;
    [Tooltip("Kekuatan serang (ATK) saat ini. Nilai awal default = 20.")]
    public int atk = 20;
    [Tooltip("Kekuatan bertahan (DEF) saat ini. Nilai awal default = 10.")]
    public int def = 10;

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
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text atkText;
    [SerializeField] private TMP_Text defText;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text cycleText;

    [Header("UI Premium HUD References")]
    [SerializeField] private Image hpFillImage;               // Progress bar fill untuk HP
    [SerializeField] private TMP_Text hpValText;              // Teks nilai HP
    [SerializeField] private TMP_Text atkValText;             // Teks nilai ATK
    [SerializeField] private TMP_Text defValText;             // Teks nilai DEF
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

    [Header("Bad Ending Settings")]
    [SerializeField] private GameObject badEndingPanel;
    [SerializeField] private DialogueData badEndingDialogue;
    [SerializeField] private DialogueData dayLimitDialogue;
    public bool isBadEnding { get; private set; } = false;
    private int daysSinceLastLaraVisit = 0;

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
    [SerializeField] private Button upgradeSkillsButton;  // Tombol Upgrade Skills (Placeholder)
    [SerializeField] private Button backToRenButton;      // Tombol Kembali ke Kamar Ren

    [Header("Data Dialog Lara (Scriptable Object)")]
    [SerializeField] private DialogueData talkToLaraDialogue; // Taruh file Scriptable Object dialog Lara di sini

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
        // Set awal posisi ruangan ke Kamar Ren
        if (kamarRenPanel != null) kamarRenPanel.SetActive(true);
        if (kamarLaraPanel != null) kamarLaraPanel.SetActive(false);
        if (badEndingPanel != null) badEndingPanel.SetActive(false);

        // Menghubungkan tombol-tombol dengan fungsinya di kode
        if (trainButton != null) trainButton.onClick.AddListener(Train);
        if (visitLaraButton != null) visitLaraButton.onClick.AddListener(VisitLara);
        if (restButton != null) restButton.onClick.AddListener(Rest);

        if (talkToLaraButton != null) talkToLaraButton.onClick.AddListener(TalkToLara);
        if (upgradeSkillsButton != null) upgradeSkillsButton.onClick.AddListener(UpgradeSkills);
        if (backToRenButton != null) backToRenButton.onClick.AddListener(ReturnToKamarRen);

        // Tombol Save/Load di HUD
        if (saveButton != null) saveButton.onClick.AddListener(() => { if (saveLoadUI != null) saveLoadUI.OpenPanel(true); });
        if (loadButton != null) loadButton.onClick.AddListener(() => { if (saveLoadUI != null) saveLoadUI.OpenPanel(false); });
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        UpdateUI();
    }

    /// <summary>
    /// Memperbarui semua tampilan teks dan status aktif tombol di layar.
    /// </summary>
    public void UpdateUI()
    {
        // Update teks statistik pemain (Legacy)
        if (hpText != null) hpText.text = "HP: " + hp;
        if (atkText != null) atkText.text = "ATK: " + atk;
        if (defText != null) defText.text = "DEF: " + def;

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

        // Update Premium Visual HUD
        if (hpFillImage != null)
        {
            // Karena tidak ada sistem damage, HP saat ini selalu bernilai penuh (maksimal).
            // Bar HP selalu penuh (1.0f) untuk mencerminkan kondisi sehat sempurna.
            hpFillImage.fillAmount = 1f;
        }
        if (hpValText != null) hpValText.text = hp.ToString();

        if (atkValText != null) atkValText.text = atk.ToString();
        if (defValText != null) defValText.text = def.ToString();

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
                    // Nyalakan orb (warna kuning terang) jika indeks di bawah energi saat ini,
                    // jika tidak, redupkan (abu-abu transparan).
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
        // Melatih Diri dan Visit Lara membutuhkan energi > 0
        bool hasEnergy = energy > 0;
        if (trainButton != null) trainButton.interactable = hasEnergy;
        if (visitLaraButton != null) visitLaraButton.interactable = hasEnergy;

        // Rest (Istirahat) selalu aktif tanpa syarat
        if (restButton != null) restButton.interactable = true;
    }

    /// <summary>
    /// Aksi Melatih Diri: Mengurangi 1 energi dan menambahkan stat pemain.
    /// </summary>
    public void Train()
    {
        if (energy > 0)
        {
            energy -= 1;
            
            // Formula peningkatan stat: +10 ATK, +5 DEF, +10 HP
            atk += 10;
            def += 5;
            hp += 10;

            // Majukan siklus waktu: Morning -> Afternoon -> Night -> Morning (Hari baru)
            if (cyclePhase == CyclePhase.Morning)
            {
                cyclePhase = CyclePhase.Afternoon;
                Debug.Log("Melatih Diri berhasil! Energi berkurang 1. Stat bertambah. Waktu berubah ke Afternoon (Siang).");
            }
            else if (cyclePhase == CyclePhase.Afternoon)
            {
                cyclePhase = CyclePhase.Night;
                Debug.Log("Melatih Diri berhasil! Energi berkurang 1. Stat bertambah. Waktu berubah ke Night (Malam).");
            }
            else if (cyclePhase == CyclePhase.Night)
            {
                cyclePhase = CyclePhase.Morning;
                day += 1; // Berganti ke hari baru
                Debug.Log("Melatih Diri berhasil! Energi berkurang 1. Stat bertambah. Hari baru dimulai: Hari " + day + " - Morning (Pagi).");
                OnDayChanged();
            }

            UpdateUI();
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
        // Pulihkan energi ke maksimal
        energy = maxEnergy;

        // Majukan siklus waktu: Morning -> Afternoon -> Night -> Morning (Hari baru)
        if (cyclePhase == CyclePhase.Morning)
        {
            cyclePhase = CyclePhase.Afternoon;
            Debug.Log("Rest berhasil! Waktu berubah ke Afternoon (Siang).");
        }
        else if (cyclePhase == CyclePhase.Afternoon)
        {
            cyclePhase = CyclePhase.Night;
            Debug.Log("Rest berhasil! Waktu berubah ke Night (Malam).");
        }
        else if (cyclePhase == CyclePhase.Night)
        {
            cyclePhase = CyclePhase.Morning;
            day += 1; // Berganti ke hari baru
            Debug.Log("Rest berhasil! Hari baru dimulai: Hari " + day + " - Morning (Pagi).");
            OnDayChanged();
        }

        UpdateUI();
    }

    /// <summary>
    /// Aksi Visit Lara: Pindah ke Kamar Lara.
    /// </summary>
    public void VisitLara()
    {
        if (energy > 0)
        {
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

            Debug.Log("Pindah ke Kamar Lara.");
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
    /// Aksi Upgrade Skills: Masih berupa placeholder kosong untuk nanti.
    /// </summary>
    public void UpgradeSkills()
    {
        Debug.Log("Upgrade Skills ditekan: Aksi ini belum diimplementasikan (Placeholder).");
    }

    /// <summary>
    /// Kembali ke scene Main Menu.
    /// </summary>
    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private void OnDayChanged()
    {
        if (day >= 40)
        {
            TriggerBadEnding();
            return;
        }

        daysSinceLastLaraVisit++;
        if (daysSinceLastLaraVisit >= 5)
        {
            TriggerBadEnding();
        }
    }

    public void TriggerBadEnding()
    {
        isBadEnding = true;
        if (badEndingPanel != null) badEndingPanel.SetActive(true);
        if (kamarRenPanel != null) kamarRenPanel.SetActive(false);
        if (kamarLaraPanel != null) kamarLaraPanel.SetActive(false);

        if (DialogueManager.Instance != null)
        {
            DialogueData dialogueToShow = (day >= 40) ? dayLimitDialogue : badEndingDialogue;
            string fallbackMsg = (day >= 40)
                ? "Time runs out! You reached day 40 and failed to save Lara..."
                : "Lara has passed away because you did not visit her for 5 consecutive days...";

            if (dialogueToShow != null)
            {
                DialogueManager.Instance.StartDialogue(dialogueToShow);
            }
            else
            {
                DialogueManager.Instance.StartSystemDialogue("SYSTEM", fallbackMsg);
            }
        }
    }

    public void ResetGame()
    {
        day = 1;
        cyclePhase = CyclePhase.Morning;
        energy = maxEnergy;
        hp = 100;
        atk = 20;
        def = 10;
        daysSinceLastLaraVisit = 0;
        isBadEnding = false;

        if (badEndingPanel != null) badEndingPanel.SetActive(false);
        if (kamarRenPanel != null) kamarRenPanel.SetActive(true);
        if (kamarLaraPanel != null) kamarLaraPanel.SetActive(false);

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.EndDialogue();
        }

        UpdateUI();
    }
}
