using UnityEngine;

//enum karena biar no typo & muncul dropdown
public enum ChoiceCategory
{
    Neutral,
    HardDenial,
    Logical,
    Aggressive,
    Denial,
    InternalizedAnger,
    FalseHope,
    Rumination,
    Begging,
    Avoidance,
    PassiveSuicidal,
    Rational,
    CleanUpAction //pemicu bab Acceptance
}

[System.Serializable]
public class Choice
{
    [TextArea(2, 3)]
    public string choiceText; //teks opsi jawaban pemain

    [TextArea(2, 3)]
    public string systemResponse; //teks balasan puitis dari sistem

    [Header("Efek Perubahan Emosi")]
    public float addDistress;
    public float addDenial;
    public float addRumination;
    public float addHope;

    [Header("Pemicu Rule Base Khusus")]
    public ChoiceCategory category; //tipe pilihan untuk menaikkan counter
}

[CreateAssetMenu(fileName = "NewQuestion", menuName = "Grief/Question")]
public class QuestionData : ScriptableObject
{

    [Header("Narasi Situasi")]
    [TextArea(3, 6)]
    public string mainQuestion; //teks skenario utama

    [Header("Daftar Pilihan (A, B, C)")]
    public Choice[] choices;
}