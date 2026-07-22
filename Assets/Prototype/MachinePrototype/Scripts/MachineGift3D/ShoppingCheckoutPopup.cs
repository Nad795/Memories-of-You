using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShoppingCheckoutPopup : MonoBehaviour
{
    [SerializeField]
    private GameObject popupRoot;

    [SerializeField]
    private TMP_Text messageText;

    [SerializeField]
    private Button confirmButton;

    [SerializeField]
    private Button cancelButton;

    private Action confirmCallback;
    private Action cancelCallback;

    private void Awake()
    {
        if (popupRoot == null)
        {
            popupRoot = gameObject;
        }

        HideInstant();

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(HandleConfirmClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(HandleCancelClicked);
        }
    }

    private void OnDestroy()
    {
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(HandleConfirmClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(HandleCancelClicked);
        }
    }

    public void Show(
        string message,
        Action onConfirm,
        Action onCancel = null
    )
    {
        confirmCallback = onConfirm;
        cancelCallback = onCancel;

        if (messageText != null)
        {
            messageText.text = message;
        }

        popupRoot.SetActive(true);
    }

    public void Hide()
    {
        confirmCallback = null;
        cancelCallback = null;
        HideInstant();
    }

    private void HandleConfirmClicked()
    {
        Action callback = confirmCallback;
        Hide();
        callback?.Invoke();
    }

    private void HandleCancelClicked()
    {
        Action callback = cancelCallback;
        Hide();
        callback?.Invoke();
    }

    private void HideInstant()
    {
        if (popupRoot != null)
        {
            popupRoot.SetActive(false);
        }
    }
}
