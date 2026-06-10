using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controller untuk Main Menu.
/// Menampilkan tombol: New Game, Load Game (Slot 1/2/3), dan Quit.
/// 
/// SETUP DI UNITY:
/// 1. Buat Scene baru bernama "MainMenu".
/// 2. Tambahkan Canvas dengan panel latar belakang.
/// 3. Buat tombol: New Game, Load Game, Quit.
/// 4. Buat sub-panel "LoadPanel" berisi 3 slot + tombol Back.
/// 5. Assign semua reference di Inspector.
/// 6. Pastikan scene "MainMenu" dan scene gameplay (mis. "Gamedigital") 
///    sudah ditambahkan ke Build Settings (File > Build Settings > Add Open Scenes).
/// 7. Tambahkan GameObject kosong dengan SaveLoadManager.cs di scene MainMenu
///    (SaveLoadManager otomatis DontDestroyOnLoad).
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Nama Scene Gameplay")]
    [Tooltip("Nama scene gameplay yang akan di-load saat New Game atau Load Game.")]
    [SerializeField] private string gameplaySceneName = "Gamedigital";

    [Header("Panel Utama")]
    [SerializeField] private GameObject mainMenuPanel;   // Panel berisi tombol utama
    [SerializeField] private GameObject loadPanel;       // Panel berisi slot load

    [Header("Tombol Main Menu")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button quitButton;

    [Header("Judul Game")]
    [SerializeField] private TMP_Text titleText;

    [Header("Load Panel - Slot 1")]
    [SerializeField] private TMP_Text loadSlot1InfoText;
    [SerializeField] private Button loadSlot1Button;

    [Header("Load Panel - Slot 2")]
    [SerializeField] private TMP_Text loadSlot2InfoText;
    [SerializeField] private Button loadSlot2Button;

    [Header("Load Panel - Slot 3")]
    [SerializeField] private TMP_Text loadSlot3InfoText;
    [SerializeField] private Button loadSlot3Button;

    [Header("Load Panel - Back")]
    [SerializeField] private Button loadBackButton;

    // Internal array references
    private TMP_Text[] loadSlotInfoTexts;
    private Button[] loadSlotButtons;

    // Slot yang dipilih pemain untuk di-load (-1 = tidak ada)
    private int pendingLoadSlot = -1;

    private void Start()
    {
        // Setup array
        loadSlotInfoTexts = new TMP_Text[] { loadSlot1InfoText, loadSlot2InfoText, loadSlot3InfoText };
        loadSlotButtons = new Button[] { loadSlot1Button, loadSlot2Button, loadSlot3Button };

        // Panel awal
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (loadPanel != null) loadPanel.SetActive(false);

        // --- Listener tombol utama ---
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGame);
        if (loadGameButton != null) loadGameButton.onClick.AddListener(OnOpenLoadPanel);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);

        // --- Listener tombol load per slot ---
        for (int i = 0; i < 3; i++)
        {
            int slotIndex = i + 1;
            if (loadSlotButtons[i] != null)
                loadSlotButtons[i].onClick.AddListener(() => OnLoadSlot(slotIndex));
        }

        // --- Listener tombol back ---
        if (loadBackButton != null) loadBackButton.onClick.AddListener(OnBackToMainMenu);

        // Update title
        if (titleText != null) titleText.text = "Orenomonogatari";
    }

    // =========================================================================
    // BUTTON ACTIONS
    // =========================================================================

    private void OnNewGame()
    {
        SaveLoadManager.isLoadedGame = false; // Reset loaded game flag
        pendingLoadSlot = -1; // Tidak ada slot yang di-load
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void OnOpenLoadPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (loadPanel != null) loadPanel.SetActive(true);
        RefreshLoadSlots();
    }

    private void OnBackToMainMenu()
    {
        if (loadPanel != null) loadPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    private void OnLoadSlot(int slotIndex)
    {
        if (SaveLoadManager.Instance == null)
        {
            Debug.LogError("MainMenuManager: SaveLoadManager.Instance belum ada!");
            return;
        }

        if (!SaveLoadManager.Instance.HasSave(slotIndex))
        {
            Debug.LogWarning($"MainMenuManager: Slot {slotIndex} kosong!");
            return;
        }

        // Simpan slot yang akan di-load, lalu pindah scene
        pendingLoadSlot = slotIndex;

        // Register callback untuk load setelah scene selesai dimuat
        SceneManager.sceneLoaded += OnGameplaySceneLoaded;
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void OnGameplaySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe agar tidak dipanggil berkali-kali
        SceneManager.sceneLoaded -= OnGameplaySceneLoaded;

        if (pendingLoadSlot > 0 && SaveLoadManager.Instance != null)
        {
            // Tunda 1 frame agar GameManager.Start() selesai dulu
            StartCoroutine(LoadAfterFrame());
        }
    }

    private System.Collections.IEnumerator LoadAfterFrame()
    {
        yield return null; // Tunggu 1 frame

        if (SaveLoadManager.Instance != null && pendingLoadSlot > 0)
        {
            SaveLoadManager.Instance.LoadGame(pendingLoadSlot);
            Debug.Log($"MainMenuManager: Berhasil memuat save Slot {pendingLoadSlot} setelah scene dimuat.");
            pendingLoadSlot = -1;
        }
    }

    private void OnQuit()
    {
        Debug.Log("MainMenuManager: Quit Game.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // =========================================================================
    // REFRESH UI
    // =========================================================================

    private void RefreshLoadSlots()
    {
        for (int i = 0; i < 3; i++)
        {
            int slotNum = i + 1;
            bool hasSave = SaveLoadManager.Instance != null && SaveLoadManager.Instance.HasSave(slotNum);

            if (loadSlotInfoTexts[i] != null)
            {
                if (hasSave)
                {
                    SaveData data = SaveLoadManager.Instance.GetSaveData(slotNum);
                    string phaseName = ((CyclePhase)data.cyclePhase).ToString();
                    loadSlotInfoTexts[i].text = $"Slot {slotNum}  |  Hari {data.day} - {phaseName}\n" +
                                                 $"Riset: {data.researchProgress} / {data.maxResearchLevel}\n" +
                                                 $"Disimpan: {data.saveDateTime}";
                }
                else
                {
                    loadSlotInfoTexts[i].text = $"Slot {slotNum}  |  --- Kosong ---";
                }
            }

            if (loadSlotButtons[i] != null)
            {
                loadSlotButtons[i].interactable = hasSave;
            }
        }
    }

}
