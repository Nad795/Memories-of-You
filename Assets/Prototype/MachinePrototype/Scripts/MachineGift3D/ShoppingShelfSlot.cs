using UnityEngine;

public class ShoppingShelfSlot : MonoBehaviour, IInteractable
{
    [Header("Item")]
    [SerializeField]
    private GiftItem itemData;

    [SerializeField]
    private GameObject itemVisual;

    [SerializeField]
    private GameObject heldPreviewPrefab;

    [SerializeField]
    private bool hideRenderersOnly = true;

    [Header("Debug")]
    [SerializeField]
    private bool logInteractionResult = true;

    private bool hasItem = true;

    public GiftItem ItemData => itemData;
    public bool HasItem => hasItem;

    private void Awake()
    {
        SetVisualActive(hasItem);
    }

    public bool CanInteract(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            return false;
        }

        if (hasItem)
        {
            return itemData != null && !inventory.IsFull;
        }

        return inventory.CanReturnTo(this);
    }

    public void Interact(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            Log("Tidak ada PlayerInventory di scene.");
            return;
        }

        if (hasItem)
        {
            TryGiveItemTo(inventory);
            return;
        }

        TryReceiveItemFrom(inventory);
    }

    private void TryGiveItemTo(PlayerInventory inventory)
    {
        if (!inventory.TryTakeItem(itemData, this, heldPreviewPrefab))
        {
            Log("Keranjang penuh atau item shelf belum diset.");
            return;
        }

        hasItem = false;
        SetVisualActive(false);

        Log($"Mengambil item: {itemData.itemName}");
    }

    private void TryReceiveItemFrom(PlayerInventory inventory)
    {
        if (!inventory.TryReturnItem(this))
        {
            Log("Item hanya bisa dikembalikan ke shelf tempat item diambil.");
            return;
        }

        hasItem = true;
        SetVisualActive(true);

        Log($"Mengembalikan item: {itemData.itemName}");
    }

    private void SetVisualActive(bool active)
    {
        if (itemVisual != null)
        {
            if (hideRenderersOnly)
            {
                Renderer[] renderers =
                    itemVisual.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer itemRenderer in renderers)
                {
                    itemRenderer.enabled = active;
                }

                return;
            }

            itemVisual.SetActive(active);
        }
    }

    private void Log(string message)
    {
        if (logInteractionResult)
        {
            Debug.Log(message, this);
        }
    }
}
