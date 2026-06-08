using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Controller untuk panel Save/Load yang bisa diakses dari in-game.
/// Menampilkan 3 slot save beserta info singkat (Hari, Waktu, Tanggal Save).
/// Panel ini bisa di-toggle buka/tutup saat bermain.
/// 
/// SETUP DI UNITY:
/// 1. Buat Canvas baru atau gunakan Canvas yang ada.
/// 2. Buat panel utama "SaveLoadPanel" sebagai overlay gelap.
/// 3. Di dalamnya buat container dengan 3 baris slot.
/// 4. Tiap slot punya: Label info (TMP_Text), Tombol Save, Tombol Load, Tombol Delete.
/// 5. Assign semua reference di Inspector.
/// </summary>
public class SaveLoadUI : MonoBehaviour
{
    [Header("Panel Utama")]
    [SerializeField] private GameObject saveLoadPanel;

    [Header("Mode Toggle")]
    [SerializeField] private Button tabSaveButton;     // Tombol tab "SAVE"
    [SerializeField] private Button tabLoadButton;     // Tombol tab "LOAD"
    [SerializeField] private TMP_Text panelTitleText;  // Judul panel ("SAVE GAME" / "LOAD GAME")

    [Header("Slot 1")]
    [SerializeField] private TMP_Text slot1InfoText;
    [SerializeField] private Button slot1ActionButton;
    [SerializeField] private TMP_Text slot1ActionLabel;
    [SerializeField] private Button slot1DeleteButton;

    [Header("Slot 2")]
    [SerializeField] private TMP_Text slot2InfoText;
    [SerializeField] private Button slot2ActionButton;
    [SerializeField] private TMP_Text slot2ActionLabel;
    [SerializeField] private Button slot2DeleteButton;

    [Header("Slot 3")]
    [SerializeField] private TMP_Text slot3InfoText;
    [SerializeField] private Button slot3ActionButton;
    [SerializeField] private TMP_Text slot3ActionLabel;
    [SerializeField] private Button slot3DeleteButton;

    [Header("Tombol Tutup")]
    [SerializeField] private Button closeButton;

    // Referensi array untuk akses mudah
    private TMP_Text[] slotInfoTexts;
    private Button[] slotActionButtons;
    private TMP_Text[] slotActionLabels;
    private Button[] slotDeleteButtons;

    private bool isSaveMode = true; // true = Save, false = Load

    private void Start()
    {
        // Masukkan ke array agar bisa di-loop
        slotInfoTexts = new TMP_Text[] { slot1InfoText, slot2InfoText, slot3InfoText };
        slotActionButtons = new Button[] { slot1ActionButton, slot2ActionButton, slot3ActionButton };
        slotActionLabels = new TMP_Text[] { slot1ActionLabel, slot2ActionLabel, slot3ActionLabel };
        slotDeleteButtons = new Button[] { slot1DeleteButton, slot2DeleteButton, slot3DeleteButton };

        // Listener tombol aksi tiap slot
        for (int i = 0; i < 3; i++)
        {
            int slotIndex = i + 1; // Slot 1-3
            if (slotActionButtons[i] != null)
                slotActionButtons[i].onClick.AddListener(() => OnSlotActionClicked(slotIndex));
            if (slotDeleteButtons[i] != null)
                slotDeleteButtons[i].onClick.AddListener(() => OnDeleteClicked(slotIndex));
        }

        // Listener tab Save/Load
        if (tabSaveButton != null) tabSaveButton.onClick.AddListener(() => SetMode(true));
        if (tabLoadButton != null) tabLoadButton.onClick.AddListener(() => SetMode(false));

        // Listener tombol tutup
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);

        // Pastikan panel tertutup di awal
        if (saveLoadPanel != null) saveLoadPanel.SetActive(false);
    }

    // =========================================================================
    // BUKA / TUTUP PANEL
    // =========================================================================

    /// <summary>
    /// Membuka panel Save/Load. Dipanggil dari tombol di HUD.
    /// </summary>
    public void OpenPanel(bool asSaveMode = true)
    {
        isSaveMode = asSaveMode;
        if (saveLoadPanel != null) saveLoadPanel.SetActive(true);
        RefreshUI();
    }

    /// <summary>
    /// Menutup panel Save/Load.
    /// </summary>
    public void ClosePanel()
    {
        if (saveLoadPanel != null) saveLoadPanel.SetActive(false);
    }

    /// <summary>
    /// Toggle panel: buka jika tertutup, tutup jika terbuka.
    /// </summary>
    public void TogglePanel()
    {
        if (saveLoadPanel == null) return;

        if (saveLoadPanel.activeSelf)
            ClosePanel();
        else
            OpenPanel(isSaveMode);
    }

    // =========================================================================
    // MODE SWITCHING
    // =========================================================================

    private void SetMode(bool saveMode)
    {
        isSaveMode = saveMode;
        RefreshUI();
    }

    // =========================================================================
    // UI REFRESH
    // =========================================================================

    /// <summary>
    /// Memperbarui tampilan semua slot berdasarkan data yang tersimpan.
    /// </summary>
    private void RefreshUI()
    {
        // Update judul panel
        if (panelTitleText != null)
            panelTitleText.text = isSaveMode ? "SAVE GAME" : "LOAD GAME";

        // Highlight tab aktif
        if (tabSaveButton != null)
        {
            var colors = tabSaveButton.colors;
            colors.normalColor = isSaveMode ? new Color(0.3f, 0.7f, 0.4f) : new Color(0.25f, 0.25f, 0.3f);
            tabSaveButton.colors = colors;
        }
        if (tabLoadButton != null)
        {
            var colors = tabLoadButton.colors;
            colors.normalColor = !isSaveMode ? new Color(0.3f, 0.5f, 0.8f) : new Color(0.25f, 0.25f, 0.3f);
            tabLoadButton.colors = colors;
        }

        // Update info tiap slot
        for (int i = 0; i < 3; i++)
        {
            int slotNum = i + 1;
            bool hasSave = SaveLoadManager.Instance != null && SaveLoadManager.Instance.HasSave(slotNum);

            // Info teks
            if (slotInfoTexts[i] != null)
            {
                if (hasSave)
                {
                    SaveData data = SaveLoadManager.Instance.GetSaveData(slotNum);
                    string phaseName = ((CyclePhase)data.cyclePhase).ToString();
                    slotInfoTexts[i].text = $"Slot {slotNum}  |  Hari {data.day} - {phaseName}\n" +
                                            $"HP:{data.hp}  ATK:{data.atk}  DEF:{data.def}\n" +
                                            $"Disimpan: {data.saveDateTime}";
                }
                else
                {
                    slotInfoTexts[i].text = $"Slot {slotNum}  |  --- Kosong ---";
                }
            }

            // Label tombol aksi
            if (slotActionLabels[i] != null)
                slotActionLabels[i].text = isSaveMode ? "SAVE" : "LOAD";

            // Aktif/nonaktif tombol
            if (slotActionButtons[i] != null)
            {
                // Save selalu aktif, Load hanya aktif jika ada data
                slotActionButtons[i].interactable = isSaveMode || hasSave;
            }

            if (slotDeleteButtons[i] != null)
            {
                slotDeleteButtons[i].gameObject.SetActive(hasSave);
            }
        }
    }

    // =========================================================================
    // SLOT ACTIONS
    // =========================================================================

    private void OnSlotActionClicked(int slotIndex)
    {
        if (SaveLoadManager.Instance == null) return;

        if (isSaveMode)
        {
            SaveLoadManager.Instance.SaveGame(slotIndex);
            Debug.Log($"SaveLoadUI: Game disimpan ke Slot {slotIndex}.");
        }
        else
        {
            bool success = SaveLoadManager.Instance.LoadGame(slotIndex);
            if (success)
            {
                Debug.Log($"SaveLoadUI: Game dimuat dari Slot {slotIndex}.");
                ClosePanel(); // Tutup panel setelah berhasil load
            }
        }

        RefreshUI(); // Perbarui tampilan setelah aksi
    }

    private void OnDeleteClicked(int slotIndex)
    {
        if (SaveLoadManager.Instance == null) return;

        SaveLoadManager.Instance.DeleteSave(slotIndex);
        Debug.Log($"SaveLoadUI: Save di Slot {slotIndex} dihapus.");
        RefreshUI();
    }
}
