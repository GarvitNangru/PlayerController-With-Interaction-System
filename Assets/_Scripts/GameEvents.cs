using System;
//using PlayerInventory;

public static class GameEvents
{
    public static event Action CloseInventoryEvent;

    public static event Action OnInventoryInputEvent;

    public static event Action<bool> OnUIToggleEvent;

    //  public event Action<InventoryItemBase, InventoryItemData> OnItemCollectedEvent;

    public static void CloseInventory()
    {
        CloseInventoryEvent?.Invoke();
    }

    //public void OnItemCollected(InventoryItemBase item, InventoryItemData data)
    //{
    //    OnItemCollectedEvent?.Invoke(item, data);
    //}

    public static void OnInventoryInput()
    {
        OnInventoryInputEvent?.Invoke();
    }

    public static void OnUIToggle(bool isActive)
    {
        OnUIToggleEvent?.Invoke(isActive);
    }
}
