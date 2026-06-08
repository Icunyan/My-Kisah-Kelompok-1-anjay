/// <summary>
/// Kelas data yang merepresentasikan seluruh state permainan untuk disimpan ke file.
/// Semua field publik agar bisa di-serialize oleh JsonUtility.
/// </summary>
[System.Serializable]
public class SaveData
{
    // --- Statistik Player ---
    public int hp;
    public int atk;
    public int def;

    // --- Manajemen Hari & Energi ---
    public int day;
    public int cyclePhase; // Disimpan sebagai int dari enum CyclePhase
    public int energy;
    public int maxEnergy;

    // --- Internal State ---
    public int daysSinceLastLaraVisit;

    // --- Metadata Save ---
    public string saveDateTime; // Tanggal & waktu saat save dibuat (untuk ditampilkan di UI)
}
