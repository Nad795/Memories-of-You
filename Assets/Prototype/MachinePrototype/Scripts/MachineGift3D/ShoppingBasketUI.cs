using TMPro;
using UnityEngine;

public class ShoppingBasketUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text[] slotTexts = new TMP_Text[6];

    [SerializeField]
    private TMP_Text counterText;

    [SerializeField]
    private string emptySlotText = "-";

    private PlayerInventory inventory;

    private void OnEnable()
    {
        BindInventory();
        Refresh();
    }

    private void OnDisable()
    {
        if (inventory != null)
        {
            inventory.InventoryChanged -= Refresh;
        }
    }

    private void Update()
    {
        if (inventory == null && PlayerInventory.Active != null)
        {
            BindInventory();
            Refresh();
        }
    }

    private void BindInventory()
    {
        if (inventory == PlayerInventory.Active)
        {
            return;
        }

        if (inventory != null)
        {
            inventory.InventoryChanged -= Refresh;
        }

        inventory = PlayerInventory.Active;

        if (inventory != null)
        {
            inventory.InventoryChanged += Refresh;
        }
    }

    private void Refresh()
    {
        if (inventory == null)
        {
            ClearSlots();
            SetCounter(0, slotTexts.Length);
            return;
        }

        for (int i = 0; i < slotTexts.Length; i++)
        {
            TMP_Text slotText = slotTexts[i];

            if (slotText == null)
            {
                continue;
            }

            GiftItem item = inventory.GetItemAt(i);
            slotText.text = item != null ? item.itemName : emptySlotText;
        }

        SetCounter(inventory.ItemCount, inventory.MaxItems);
    }

    private void ClearSlots()
    {
        foreach (TMP_Text slotText in slotTexts)
        {
            if (slotText != null)
            {
                slotText.text = emptySlotText;
            }
        }
    }

    private void SetCounter(int count, int max)
    {
        if (counterText != null)
        {
            counterText.text = $"{count}/{max}";
        }
    }
}
