public interface IInteractable
{
    bool CanInteract(PlayerInventory inventory);

    void Interact(PlayerInventory inventory);
}
