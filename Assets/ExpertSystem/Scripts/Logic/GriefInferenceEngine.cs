using UnityEngine;
using UnityEngine.SceneManagement; 

public class GriefInferenceEngine : MonoBehaviour
{

    [Header("Referensi Data")]
    public GriefState currentState;

    //dipanggil saat pemain klik tombol jawaban A/B/C
    public void ProcessPlayerChoice(Choice selectedChoice)
    {
        currentState.ModifyEmotion(ref currentState.distress, selectedChoice.addDistress);
        currentState.ModifyEmotion(ref currentState.denial, selectedChoice.addDenial);
        currentState.ModifyEmotion(ref currentState.rumination, selectedChoice.addRumination);
        currentState.ModifyEmotion(ref currentState.hope, selectedChoice.addHope);

        currentState.questionCount++;

        //update kategori khusus
        UpdateCategoryCounters(selectedChoice.category);
        EvaluateRules(selectedChoice);
    }

    //menambah counter spesifik berdasarkan pilihan pemain
    private void UpdateCategoryCounters(ChoiceCategory category)
    {
        switch (category)
        {
            case ChoiceCategory.HardDenial:
                currentState.consecutiveHardDenial++;
                break;

            case ChoiceCategory.Aggressive:
                currentState.aggressiveChoiceCount++;
                break;

            case ChoiceCategory.Denial:
                currentState.denialChoiceCount++;
                break;

            case ChoiceCategory.InternalizedAnger:
                currentState.internalizedAngerCount++;
                break;

            case ChoiceCategory.FalseHope:
                currentState.falseHopeChoiceCount++;
                break;

            case ChoiceCategory.Rumination:
                currentState.ruminationChoiceCount++;
                break;

            case ChoiceCategory.Begging:
                currentState.beggingChoiceCount++;
                break;

            case ChoiceCategory.Avoidance:
                currentState.avoidanceChoiceCount++;
                break;

            case ChoiceCategory.PassiveSuicidal:
                currentState.passiveSuicidalChoiceCount++;
                break;

            case ChoiceCategory.Rational:
                currentState.rationalChoiceCount++;
                break;

            case ChoiceCategory.CleanUpAction:
                Debug.Log("Pemain memilih Clean Up. Pindah ke Acceptance.");
                SceneManager.LoadScene("Bab5_Acceptance"); //SESUAIIN NAMANYA SAMA YG ASLI GIMANA
                break;

            case ChoiceCategory.Neutral:
                break;
        }
    }

    private void EvaluateRules(Choice lastChoice)
    {
        //BAB 1 DENIAL - PERPINDAHAN BAB
        if (currentState.distress >= 80 || currentState.rumination >= 80 || currentState.questionCount > 23)
        {
            Debug.Log("Pindah ke Anger.");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Bab2_Anger"); //JANGAN LUPA GANTI YA NAMANYA !!!!!!!
            return;
        }
        if (currentState.hope >= 80 || currentState.denial >= 80)
        {
            Debug.Log("Notifikasi teman chat melihat mantan!");
            currentState.hope = 0;
            currentState.distress = 100;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Bab2_Anger");
            return;
        }

        //BAB 1 DENIAL - KONSEKUENSI
        if (currentState.denial > 50)
        {
            Debug.Log("Sembunyikan opsi jawaban C (logis).");
        }
        if (currentState.rumination > 50)
        {
            Debug.Log("Mempercepat timer pertanyaan (racing thoughts).");
        }
        if (currentState.consecutiveHardDenial >= 3)
        {
            Debug.Log("SISTEM RESPONSE: 'Kamu percaya itu? Benar-benar percaya?'");
        }
        if (lastChoice.category == ChoiceCategory.Logical && currentState.distress > 50)
        {
            Debug.Log("SISTEM RESPONSE: 'Logikamu berkata tidak apa-apa, tapi tanganmu gemetar hebat.'");
        }
        
        //BAB 1 DENIAL - VISUAL & AUDIO
        if (currentState.hope > 60)
        {
            Debug.Log("VISUAL: efek bloom & blur.");
        }
        if (currentState.distress > 70)
        {
            Debug.Log("VISUAL: kurangi saturasi warna dan aktifkan Vignette.");
        }
        if (currentState.rumination > 60)
        {
            Debug.Log("VISUAL: efek Film Grain.");
        }
        if (currentState.denial > 60)
        {
            Debug.Log("AUDIO: suara teredam seperti di air.");
        }
        if (currentState.distress > 80)
        {
            Debug.Log("AUDIO: efek detak jantung lambat.");
        }
    }
}