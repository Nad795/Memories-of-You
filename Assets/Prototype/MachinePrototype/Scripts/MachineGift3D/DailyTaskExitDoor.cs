using UnityEngine;
using UnityEngine.SceneManagement;

public class DailyTaskExitDoor : MonoBehaviour, IInteractable
{
    [Header("Exit UI")]
    [SerializeField]
    private ShoppingCheckoutPopup exitPopup;

    [Header("Result")]
    [SerializeField]
    private bool loadSceneOnConfirm;

    [SerializeField]
    private string completionSceneName;

    [Header("Messages")]
    [SerializeField]
    private string exitWarningMessage =
        "Warning: leaving now will end your current daily task without buying anything. Are you sure you want to leave?";

    [SerializeField]
    private string blockedExitMessage =
        "You cannot leave while carrying items. Return them or check out first.";

    public bool CanInteract(PlayerInventory inventory)
    {
        return inventory != null &&
               inventory.IsEmpty &&
               exitPopup != null;
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
        {
            Debug.Log(blockedExitMessage, this);
            return;
        }

        FPSController controller =
            inventory.GetComponent<FPSController>();

        if (controller != null)
        {
            controller.SetGameplayInputLocked(true);
        }

        exitPopup.Show(
            exitWarningMessage,
            () => ConfirmExit(controller),
            () => CancelExit(controller)
        );
    }

    private void ConfirmExit(FPSController controller)
    {
        if (controller != null)
        {
            controller.SetGameplayInputLocked(false);
        }

        bool handledByExpertSystem = GriefExpertSystem.ProcessShoppingOutcome(
            null,
            ShoppingOutcome.LeftWithoutBuying,
            0,
            0,
            completionSceneName,
            completionSceneName
        );

        if (loadSceneOnConfirm &&
            !handledByExpertSystem &&
            !string.IsNullOrWhiteSpace(completionSceneName))
        {
            if (Application.CanStreamedLevelBeLoaded(completionSceneName))
            {
                SceneManager.LoadScene(completionSceneName);
            }
            else
            {
                Debug.LogWarning(
                    $"Scene '{completionSceneName}' belum tersedia di build settings.",
                    this
                );
            }
        }

        Debug.Log("Daily task finished without purchase.", this);
    }

    private void CancelExit(FPSController controller)
    {
        if (controller != null)
        {
            controller.SetGameplayInputLocked(false);
        }
    }
}
