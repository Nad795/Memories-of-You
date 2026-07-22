using UnityEngine;

public class Door :
    MonoBehaviour,
    IInteractable
{
    private bool opened;

    public bool CanInteract(PlayerInventory inventory)
    {
        return true;
    }

    public void Interact(PlayerInventory inventory)
    {
        opened = !opened;

        if (opened)
        {
            transform.rotation =
                Quaternion.Euler(
                    0,
                    90,
                    0
                );
        }
        else
        {
            transform.rotation =
                Quaternion.identity;
        }
    }
}
