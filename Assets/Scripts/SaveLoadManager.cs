using UnityEngine;

/// <summary>
/// Singleton Manager untuk menangani Save dan Load permainan.
/// Data game disimpan sebagai JSON string di PlayerPrefs.
/// Mendukung hingga 3 slot save (Slot 1, 2, 3).
/// </summary>
public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }
    public static bool isLoadedGame = false; // Menandai apakah game baru saja di-load

    public const int MAX_SLOTS = 3;
    private const string SAVE_KEY_PREFIX = "SaveSlot_";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================================================================
    // SAVE
    // =========================================================================

    /// <summary>
    /// Menyimpan state GameManager ke slot yang dipilih (1-3).
    /// </summary>
    public void SaveGame(int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > MAX_SLOTS)
        {
            Debug.LogError($"SaveLoadManager: Slot index {slotIndex} di luar jangkauan (1-{MAX_SLOTS}).");
            return;
        }

        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("SaveLoadManager: GameManager.Instance belum ada, tidak bisa save.");
            return;
        }

        // Kumpulkan data dari GameManager
        SaveData data = new SaveData
        {
            researchProgress = gm.researchProgress,
            maxResearchLevel = gm.maxResearchLevel,
            isLaraWatchedByFriend = gm.GetIsLaraWatchedByFriend(),
            laraWardDaysRemaining = gm.GetLaraWardDaysRemaining(),
            isResearchBuffed = gm.GetIsResearchBuffed(),
            day = gm.day,
            cyclePhase = (int)gm.cyclePhase,
            energy = gm.energy,
            maxEnergy = gm.maxEnergy,
            daysSinceLastLaraVisit = gm.GetDaysSinceLastLaraVisit(),
            saveDateTime = System.DateTime.Now.ToString("dd MMM yyyy  HH:mm")
        };

        // Serialize ke JSON lalu simpan ke PlayerPrefs
        string json = JsonUtility.ToJson(data, true);
        string key = SAVE_KEY_PREFIX + slotIndex;
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();

        Debug.Log($"SaveLoadManager: Game berhasil disimpan di Slot {slotIndex}.\n{json}");
    }

    // =========================================================================
    // LOAD
    // =========================================================================

    /// <summary>
    /// Memuat data save dari slot yang dipilih dan menerapkannya ke GameManager.
    /// Returns true jika berhasil load, false jika slot kosong.
    /// </summary>
    public bool LoadGame(int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > MAX_SLOTS)
        {
            Debug.LogError($"SaveLoadManager: Slot index {slotIndex} di luar jangkauan (1-{MAX_SLOTS}).");
            return false;
        }

        string key = SAVE_KEY_PREFIX + slotIndex;

        if (!PlayerPrefs.HasKey(key))
        {
            Debug.LogWarning($"SaveLoadManager: Slot {slotIndex} kosong, tidak ada data untuk dimuat.");
            return false;
        }

        string json = PlayerPrefs.GetString(key);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        GameManager gm = GameManager.Instance;
        if (gm == null)
        {
            Debug.LogError("SaveLoadManager: GameManager.Instance belum ada, tidak bisa load.");
            return false;
        }

        isLoadedGame = true; // Set loaded game flag

        // Terapkan data ke GameManager
        gm.researchProgress = data.researchProgress;
        gm.maxResearchLevel = data.maxResearchLevel;
        gm.SetIsLaraWatchedByFriend(data.isLaraWatchedByFriend);
        gm.SetLaraWardDaysRemaining(data.laraWardDaysRemaining);
        gm.SetIsResearchBuffed(data.isResearchBuffed);
        gm.day = data.day;
        gm.cyclePhase = (CyclePhase)data.cyclePhase;
        gm.energy = data.energy;
        gm.maxEnergy = data.maxEnergy;
        gm.SetDaysSinceLastLaraVisit(data.daysSinceLastLaraVisit);

        gm.UpdateUI();

        Debug.Log($"SaveLoadManager: Game berhasil dimuat dari Slot {slotIndex}.");
        return true;
    }

    // =========================================================================
    // UTILITY
    // =========================================================================

    /// <summary>
    /// Mengecek apakah slot tertentu sudah ada data save-nya.
    /// </summary>
    public bool HasSave(int slotIndex)
    {
        return PlayerPrefs.HasKey(SAVE_KEY_PREFIX + slotIndex);
    }

    /// <summary>
    /// Mengambil data save dari slot tertentu tanpa menerapkannya.
    /// Digunakan untuk menampilkan info di UI Save/Load.
    /// </summary>
    public SaveData GetSaveData(int slotIndex)
    {
        string key = SAVE_KEY_PREFIX + slotIndex;
        if (!PlayerPrefs.HasKey(key)) return null;

        string json = PlayerPrefs.GetString(key);
        return JsonUtility.FromJson<SaveData>(json);
    }

    /// <summary>
    /// Menghapus data save di slot tertentu.
    /// </summary>
    public void DeleteSave(int slotIndex)
    {
        string key = SAVE_KEY_PREFIX + slotIndex;
        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            Debug.Log($"SaveLoadManager: Save di Slot {slotIndex} berhasil dihapus.");
        }
    }
}
