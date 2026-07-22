using UnityEngine;

public class GriefInferenceEngine : MonoBehaviour
{
    [Header("Referensi Data")]
    public GriefState currentState;

    [Header("Scene Targets")]
    [SerializeField]
    private string angerSceneName = "Bab2_Anger";

    [SerializeField]
    private string acceptanceSceneName = "Bab5_Acceptance";

    private void Awake()
    {
        currentState = GriefExpertSystem.ResolveState(currentState);
    }

    //dipanggil saat pemain klik tombol jawaban A/B/C
    public void ProcessPlayerChoice(Choice selectedChoice)
    {
        currentState = GriefExpertSystem.ResolveState(currentState);
        GriefExpertSystem.ProcessPlayerChoice(
            currentState,
            selectedChoice,
            angerSceneName,
            acceptanceSceneName
        );
    }

    public void ProcessShoppingOutcome(
        ShoppingOutcome outcome,
        int purchasedItemCount,
        int requiredItemCount
    )
    {
        currentState = GriefExpertSystem.ResolveState(currentState);
        GriefExpertSystem.ProcessShoppingOutcome(
            currentState,
            outcome,
            purchasedItemCount,
            requiredItemCount,
            angerSceneName,
            acceptanceSceneName
        );
    }
}
