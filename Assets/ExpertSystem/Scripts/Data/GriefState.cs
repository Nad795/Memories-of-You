using UnityEngine;

[CreateAssetMenu(fileName = "NewGriefState", menuName = "Grief/State")]
public class GriefState : ScriptableObject
{

    [Header("Variabel Emosi Utama (0 - 100)")]
    //[Range] membuat slider di Inspector agar nilai tidak bisa disetting ngawur
    [Range(-100f, 100f)] public float distress;
    [Range(-100f, 100f)] public float denial;
    [Range(-100f, 100f)] public float rumination;
    [Range(-100f, 100f)] public float hope;

    [Header("Variabel Penghitung Umum")]
    public int questionCount;

    [Header("Penghitung Tipe Pilihan")]
    public int consecutiveHardDenial;
    public int aggressiveChoiceCount;
    public int denialChoiceCount;
    public int internalizedAngerCount;
    public int falseHopeChoiceCount;
    public int ruminationChoiceCount;  
    public int beggingChoiceCount;   
    public int avoidanceChoiceCount;    
    public int passiveSuicidalChoiceCount; 
    public int rationalChoiceCount;

    [Header("Ending Data")]
    public string finalEndingName; //judul ending
    public bool isEndingTriggered; //gembok agar ending tidak dihitung berulang kali

    //mereset data saat game baru dimulai
    public void ResetData()
    {
        distress = 0;
        denial = 0;
        rumination = 0;
        hope = 0;

        questionCount = 0;
        consecutiveHardDenial = 0;
        aggressiveChoiceCount = 0;
        internalizedAngerCount = 0;
        falseHopeChoiceCount = 0;
        denialChoiceCount = 0;
        ruminationChoiceCount = 0;
        beggingChoiceCount = 0;
        avoidanceChoiceCount = 0;
        passiveSuicidalChoiceCount = 0;
        rationalChoiceCount = 0;
    }

    //menambah/mengurangi emosi agar tidak tembus batas 0-100
    public void ModifyEmotion(ref float emotionVariable, float amount)
    {
        emotionVariable += amount;
        //Mathf.Clamp paksa nilai selalu berada di antara 0 dan 100
        emotionVariable = Mathf.Clamp(emotionVariable, -100f, 100f);
    }
}