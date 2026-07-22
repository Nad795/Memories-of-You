using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CashierCheckoutStation : MonoBehaviour, IInteractable
{
    [Header("Checkout UI")]
    [SerializeField]
    private ShoppingCheckoutPopup checkoutPopup;

    [Header("Result")]
    [SerializeField]
    private bool loadSceneOnConfirm;

    [SerializeField]
    private string completionSceneName;

    [Header("Expert System")]
    [SerializeField]
    private int requiredItemCount = 6;

    [SerializeField]
    private string emptyInventoryMessage = "Your basket is empty.";

    [SerializeField]
    private string confirmMessageFormat =
        "Warning: this will end your current daily task. Proceed to pay for {0} item(s)?";

    public bool CanInteract(PlayerInventory inventory)
    {
        return inventory != null &&
               inventory.HasItem &&
               checkoutPopup != null;
    }

    public void Interact(PlayerInventory inventory)
    {
        if (!CanInteract(inventory))
        {
            if (inventory == null || inventory.IsEmpty)
            {
                Debug.Log(emptyInventoryMessage, this);
            }

            return;
        }

        FPSController controller =
            inventory.GetComponent<FPSController>();

        if (controller != null)
        {
            controller.SetGameplayInputLocked(true);
        }

        string message = BuildConfirmationMessage(inventory);

        checkoutPopup.Show(
            message,
            () => ConfirmCheckout(inventory, controller),
            () => CancelCheckout(controller)
        );
    }

    private string BuildConfirmationMessage(PlayerInventory inventory)
    {
        StringBuilder builder = new StringBuilder();
        List<PlayerInventory.BasketItem> items =
            new List<PlayerInventory.BasketItem>(inventory.Items);

        builder.AppendLine(
            string.Format(
                confirmMessageFormat,
                items.Count
            )
        );

        for (int i = 0; i < items.Count; i++)
        {
            GiftItem item = items[i].Item;

            if (item == null || string.IsNullOrWhiteSpace(item.itemName))
            {
                continue;
            }

            builder.Append("- ");
            builder.AppendLine(item.itemName);
        }

        return builder.ToString().TrimEnd();
    }

    private void ConfirmCheckout(
        PlayerInventory inventory,
        FPSController controller
    )
    {
        int purchasedItemCount = inventory.ItemCount;
        int targetItemCount = ResolveRequiredItemCount(inventory);
        ShoppingOutcome outcome =
            purchasedItemCount >= targetItemCount
                ? ShoppingOutcome.BoughtAllRequiredItems
                : ShoppingOutcome.BoughtSomeItems;

        inventory.ClearAllItems();

        if (controller != null)
        {
            controller.SetGameplayInputLocked(false);
        }

        bool handledByExpertSystem = GriefExpertSystem.ProcessShoppingOutcome(
            null,
            outcome,
            purchasedItemCount,
            targetItemCount,
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

        Debug.Log("Checkout complete. Daily task finished.", this);
    }

    private int ResolveRequiredItemCount(PlayerInventory inventory)
    {
        if (requiredItemCount > 0)
        {
            return requiredItemCount;
        }

        if (inventory != null && inventory.MaxItems > 0)
        {
            return inventory.MaxItems;
        }

        return 0;
    }

    private void CancelCheckout(FPSController controller)
    {
        if (controller != null)
        {
            controller.SetGameplayInputLocked(false);
        }
    }
}
