using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class GiftFocusItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GiftItem itemData;
    private GiftFocusPanelController panelController;

    private RectTransform rect;

    void Awake()
    {
        panelController = GetComponentInParent<GiftFocusPanelController>();
        rect = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemData != null && panelController != null)
        {
            panelController.ShowInfo(itemData, rect);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (panelController != null)
        {
            panelController.HideInfo();
        }
    }
}