using UnityEngine;

[System.Serializable]
public struct DialogueChoice
{
    [Tooltip("Teks yang akan muncul pada tombol pilihan/opsi.")]
    public string choiceText;
    
    [Tooltip("Node dialog berikutnya yang akan diputar saat pilihan ini ditekan.")]
    public DialogueData nextDialogue;
}

[System.Serializable]
public struct DialogueLine
{
    [Tooltip("Nama karakter yang sedang berbicara (misal: Lara).")]
    public string speakerName;
    
    [Tooltip("ID karakter (misal: REN, LARA, MARCO, LUCIA).")]
    public string speakerID;

    [Tooltip("Opsional: Sprite portrait kustom untuk baris ini (untuk ekspresi wajah berbeda). Jika kosong, menggunakan sprite default.")]
    public Sprite customPortrait;
    
    [TextArea(3, 5)]
    [Tooltip("Baris dialog.")]
    public string text;
}

[CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Dialog System/Dialogue Node")]
public class DialogueData : ScriptableObject
{
    [Header("Informasi Karakter (Single Speaker - Legacy)")]
    [Tooltip("Nama karakter yang sedang berbicara (misal: Lara).")]
    public string characterName;

    [Tooltip("ID karakter (misal: REN, LARA, MARCO, LUCIA).")]
    public string characterID;

    [Header("Konten Dialog (Single Speaker - Legacy)")]
    [TextArea(3, 5)]
    [Tooltip("Baris dialog yang akan ditampilkan satu per satu.")]
    public string[] dialogueLines;

    [Header("Konten Dialog Multi-Speaker (Recommended)")]
    [Tooltip("Daftar baris dialog dengan pembicara dinamis.")]
    public DialogueLine[] lines;

    [Header("Pilihan Percabangan")]
    [Tooltip("Centang jika dialog ini memiliki pilihan cabang di akhir cerita.")]
    public bool hasChoices;

    [Tooltip("Daftar tombol pilihan (Maksimal 3 tombol).")]
    public DialogueChoice[] choices;

    [Header("Transisi Linear")]
    [Tooltip("Node dialog berikutnya jika tidak ada pilihan percabangan.")]
    public DialogueData nextDialogue;
}

