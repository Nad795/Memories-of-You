using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public class BasketItem
    {
        public BasketItem(
            GiftItem item,
            ShoppingShelfSlot originShelf,
            GameObject previewPrefab,
            GameObject basketVisual
        )
        {
            Item = item;
            OriginShelf = originShelf;
            PreviewPrefab = previewPrefab;
            BasketVisual = basketVisual;
        }

        public GiftItem Item { get; }
        public ShoppingShelfSlot OriginShelf { get; }
        public GameObject PreviewPrefab { get; }
        public GameObject BasketVisual { get; }
    }

    public static PlayerInventory Active { get; private set; }

    public event Action InventoryChanged;

    [Header("Basket")]
    [SerializeField]
    private int maxItems = 6;

    [Header("Basket Item Visuals")]
    [SerializeField]
    private Transform heldItemAnchor;

    [SerializeField]
    private Vector3 heldPreviewLocalPosition;

    [SerializeField]
    private Vector3 heldPreviewLocalEulerAngles;

    [SerializeField]
    private Vector3 heldPreviewLocalScale = Vector3.one;

    [Header("Basket Drop Animation")]
    [SerializeField]
    private float dropPhysicsDuration = 1.2f;

    [SerializeField]
    private Vector3 dropStartLocalOffset = new Vector3(0f, 0.35f, 0f);

    [SerializeField]
    private Vector3 dropInitialVelocity;

    [SerializeField]
    private Vector3 dropInitialTorque = new Vector3(0f, 1.5f, 0.75f);

    private readonly List<BasketItem> items = new List<BasketItem>();
    private readonly List<Coroutine> activeDropAnimations =
        new List<Coroutine>();

    public int MaxItems => maxItems;
    public int ItemCount => items.Count;
    public bool HasItem => ItemCount > 0;
    public bool IsEmpty => ItemCount == 0;
    public bool IsFull => ItemCount >= maxItems;
    public IReadOnlyList<BasketItem> Items => items;
    public GiftItem HeldItem => HasItem ? items[items.Count - 1].Item : null;
    public ShoppingShelfSlot OriginShelf =>
        HasItem ? items[items.Count - 1].OriginShelf : null;

    private void Awake()
    {
        Active = this;
    }

    private void OnDestroy()
    {
        if (Active == this)
        {
            Active = null;
        }
    }

    public bool TryTakeItem(
        GiftItem item,
        ShoppingShelfSlot shelf,
        GameObject previewPrefab
    )
    {
        if (IsFull || item == null || shelf == null)
        {
            return false;
        }

        GameObject basketVisual =
            SpawnBasketVisual(previewPrefab);

        items.Add(
            new BasketItem(
                item,
                shelf,
                previewPrefab,
                basketVisual
            )
        );

        InventoryChanged?.Invoke();

        return true;
    }

    public bool CanReturnTo(ShoppingShelfSlot shelf)
    {
        return FindItemIndexByShelf(shelf) >= 0;
    }

    public bool TryReturnItem(ShoppingShelfSlot shelf)
    {
        int itemIndex = FindItemIndexByShelf(shelf);

        if (itemIndex < 0)
        {
            return false;
        }

        BasketItem item = items[itemIndex];

        items.RemoveAt(itemIndex);
        DestroyBasketVisual(item);

        InventoryChanged?.Invoke();

        return true;
    }

    public void ClearAllItems()
    {
        if (items.Count == 0)
        {
            return;
        }

        ClearDropAnimations();

        foreach (BasketItem item in items)
        {
            DestroyBasketVisual(item);
        }

        items.Clear();
        InventoryChanged?.Invoke();
    }

    public GiftItem GetItemAt(int index)
    {
        if (index < 0 || index >= items.Count)
        {
            return null;
        }

        return items[index].Item;
    }

    private int FindItemIndexByShelf(ShoppingShelfSlot shelf)
    {
        if (shelf == null)
        {
            return -1;
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].OriginShelf == shelf)
            {
                return i;
            }
        }

        return -1;
    }

    private GameObject SpawnBasketVisual(GameObject previewPrefab)
    {
        if (previewPrefab == null || heldItemAnchor == null)
        {
            return null;
        }

        GameObject basketVisual =
            Instantiate(previewPrefab, heldItemAnchor);

        basketVisual.transform.localPosition =
            heldPreviewLocalPosition + dropStartLocalOffset;

        basketVisual.transform.localRotation =
            Quaternion.Euler(heldPreviewLocalEulerAngles);
        basketVisual.transform.localScale = heldPreviewLocalScale;

        Rigidbody rigidbody = PrepareDropPhysics(basketVisual);

        Coroutine animation = null;
        animation =
            StartCoroutine(
                SettleBasketVisualAfterDelay(
                    basketVisual,
                    rigidbody,
                    dropPhysicsDuration,
                    () => activeDropAnimations.Remove(animation)
                )
            );

        activeDropAnimations.Add(animation);

        return basketVisual;
    }

    private Rigidbody PrepareDropPhysics(GameObject basketVisual)
    {
        Rigidbody rigidbody =
            basketVisual.GetComponent<Rigidbody>();

        if (rigidbody == null)
        {
            rigidbody = basketVisual.AddComponent<Rigidbody>();
        }

        if (basketVisual.GetComponentInChildren<Collider>() == null)
        {
            basketVisual.AddComponent<SphereCollider>();
        }

        rigidbody.isKinematic = false;
        rigidbody.useGravity = true;
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.collisionDetectionMode =
            CollisionDetectionMode.ContinuousDynamic;

        if (dropInitialVelocity != Vector3.zero)
        {
            rigidbody.linearVelocity =
                heldItemAnchor.TransformDirection(dropInitialVelocity);
        }

        if (dropInitialTorque != Vector3.zero)
        {
            rigidbody.AddRelativeTorque(
                dropInitialTorque,
                ForceMode.VelocityChange
            );
        }

        return rigidbody;
    }

    private IEnumerator SettleBasketVisualAfterDelay(
        GameObject basketVisual,
        Rigidbody rigidbody,
        float delay,
        Action onComplete
    )
    {
        yield return new WaitForSeconds(delay);

        if (basketVisual == null || rigidbody == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        onComplete?.Invoke();
    }

    private void DestroyBasketVisual(BasketItem item)
    {
        if (item?.BasketVisual != null)
        {
            Destroy(item.BasketVisual);
        }
    }

    private void ClearDropAnimations()
    {
        foreach (Coroutine animation in activeDropAnimations)
        {
            if (animation != null)
            {
                StopCoroutine(animation);
            }
        }

        activeDropAnimations.Clear();
    }
}
