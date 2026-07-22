using UnityEngine;
using UnityEngine.SceneManagement;

public static class GriefExpertSystem
{
    private const string DefaultAngerSceneName = "Bab2_Anger";
    private const string DefaultAcceptanceSceneName = "Bab5_Acceptance";

    private static GriefState sharedState;

    public static GriefState ResolveState(GriefState preferredState = null)
    {
        if (sharedState != null)
        {
            return sharedState;
        }

        if (preferredState != null)
        {
            sharedState = preferredState;
            return sharedState;
        }

        sharedState = ScriptableObject.CreateInstance<GriefState>();
        sharedState.ResetData();

        return sharedState;
    }

    public static void ResetSharedState()
    {
        ResolveState(null).ResetData();
    }

    public static bool ProcessPlayerChoice(
        GriefState state,
        Choice selectedChoice,
        string angerSceneName = DefaultAngerSceneName,
        string acceptanceSceneName = DefaultAcceptanceSceneName
    )
    {
        GriefState currentState = ResolveState(state);

        if (currentState == null || selectedChoice == null)
        {
            Debug.LogWarning("Expert system did not receive a valid choice.");
            return false;
        }

        currentState.ModifyEmotion(
            ref currentState.distress,
            selectedChoice.addDistress
        );
        currentState.ModifyEmotion(
            ref currentState.denial,
            selectedChoice.addDenial
        );
        currentState.ModifyEmotion(
            ref currentState.rumination,
            selectedChoice.addRumination
        );
        currentState.ModifyEmotion(
            ref currentState.hope,
            selectedChoice.addHope
        );

        currentState.questionCount++;

        bool sceneLoaded = UpdateCategoryCounters(
            currentState,
            selectedChoice.category,
            angerSceneName,
            acceptanceSceneName
        );

        if (sceneLoaded)
        {
            return true;
        }

        sceneLoaded = EvaluateRules(
            currentState,
            selectedChoice,
            angerSceneName
        );

        return sceneLoaded;
    }

    public static bool ProcessShoppingOutcome(
        GriefState state,
        ShoppingOutcome outcome,
        int purchasedItemCount,
        int requiredItemCount,
        string angerSceneName = DefaultAngerSceneName,
        string acceptanceSceneName = DefaultAcceptanceSceneName
    )
    {
        Choice shoppingChoice =
            CreateShoppingOutcomeChoice(
                outcome,
                purchasedItemCount,
                requiredItemCount
            );

        return ProcessPlayerChoice(
            state,
            shoppingChoice,
            angerSceneName,
            acceptanceSceneName
        );
    }

    private static Choice CreateShoppingOutcomeChoice(
        ShoppingOutcome outcome,
        int purchasedItemCount,
        int requiredItemCount
    )
    {
        Choice choice = new Choice();

        switch (outcome)
        {
            case ShoppingOutcome.LeftWithoutBuying:
                choice.choiceText = "Langsung keluar toko";
                choice.systemResponse =
                    "Kamu memilih pergi tanpa membawa apa-apa.";
                choice.addDistress = 12f;
                choice.addDenial = 8f;
                choice.addRumination = 10f;
                choice.addHope = -10f;
                choice.category = ChoiceCategory.Avoidance;
                break;

            case ShoppingOutcome.BoughtAllRequiredItems:
                choice.choiceText = "Membeli semua barang yang diperlukan";
                choice.systemResponse =
                    "Kamu menyelesaikan belanja dengan lebih tenang.";
                choice.addDistress = -8f;
                choice.addDenial = -6f;
                choice.addRumination = -4f;
                choice.addHope = 12f;
                choice.category = ChoiceCategory.CleanUpAction;
                break;

            case ShoppingOutcome.BoughtSomeItems:
            default:
            {
                float completionRatio = 0.5f;

                if (requiredItemCount > 0)
                {
                    completionRatio =
                        Mathf.Clamp01(
                            (float) purchasedItemCount /
                            requiredItemCount
                        );
                }

                choice.choiceText = "Membeli beberapa barang";
                choice.systemResponse =
                    "Kamu masih ragu-ragu, tapi setidaknya tidak pulang kosong.";
                choice.addDistress = Mathf.Lerp(4f, -2f, completionRatio);
                choice.addDenial = Mathf.Lerp(4f, -1f, completionRatio);
                choice.addRumination = Mathf.Lerp(5f, 0f, completionRatio);
                choice.addHope = Mathf.Lerp(4f, 10f, completionRatio);
                choice.category = ChoiceCategory.Rational;
                break;
            }
        }

        return choice;
    }

    private static bool UpdateCategoryCounters(
        GriefState currentState,
        ChoiceCategory category,
        string angerSceneName,
        string acceptanceSceneName
    )
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
                return TryLoadScene(acceptanceSceneName);

            case ChoiceCategory.Neutral:
                return false;
        }

        return false;
    }

    private static bool EvaluateRules(
        GriefState currentState,
        Choice lastChoice,
        string angerSceneName
    )
    {
        if (
            currentState.distress >= 80 ||
            currentState.rumination >= 80 ||
            currentState.questionCount > 23
        )
        {
            Debug.Log("Pindah ke Anger.");
            return TryLoadScene(angerSceneName);
        }

        if (currentState.hope >= 80 || currentState.denial >= 80)
        {
            Debug.Log("Notifikasi teman chat melihat mantan!");
            currentState.hope = 0;
            currentState.distress = 100;
            return TryLoadScene(angerSceneName);
        }

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

        if (lastChoice.category == ChoiceCategory.Logical &&
            currentState.distress > 50)
        {
            Debug.Log(
                "SISTEM RESPONSE: 'Logikamu berkata tidak apa-apa, tapi tanganmu gemetar hebat.'"
            );
        }

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

        return false;
    }

    private static bool TryLoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("Scene target belum diatur untuk expert system.");
            return false;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning(
                $"Scene '{sceneName}' belum tersedia di build settings."
            );
            return false;
        }

        SceneManager.LoadScene(sceneName);
        return true;
    }
}
