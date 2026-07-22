using TMPro;
using UnityEngine;

public class InventoryDebugHUD : MonoBehaviour
{
    [SerializeField]
    private TMP_Text inventoryText;

    [SerializeField]
    private string emptyText = "Inventory: Empty";

    private void Update()
    {
        if (inventoryText == null)
        {
            return;
        }

        PlayerInventory inventory = PlayerInventory.Active;

        if (inventory == null || !inventory.HasItem)
        {
            inventoryText.text = emptyText;
            return;
        }

        inventoryText.text =
            $"Basket: {inventory.ItemCount}/{inventory.MaxItems}";
    }
}
