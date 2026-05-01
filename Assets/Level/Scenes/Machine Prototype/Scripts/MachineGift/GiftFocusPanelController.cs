using UnityEngine;
using TMPro;

public class GiftFocusPanelController : MonoBehaviour
{
    [SerializeField] private GameObject focusInfoPanel;
    [SerializeField] private TMP_Text itemNameText;

    private RectTransform panelRect;

    void Awake()
    {
        panelRect = focusInfoPanel.GetComponent<RectTransform>();
        focusInfoPanel.SetActive(false);
    }

    public void ShowInfo(GiftItem data, RectTransform targetRect)
    {
        Vector3 pos = panelRect.position;
        pos.x = targetRect.position.x;
        pos.y = targetRect.position.y 
                + targetRect.rect.height / 2 
                + panelRect.rect.height / 2 
                + 10f;

        panelRect.position = pos;
        itemNameText.text = data.itemName;
        focusInfoPanel.SetActive(true);
    }

    public void HideInfo()
    {
        focusInfoPanel.SetActive(false);
    }
}