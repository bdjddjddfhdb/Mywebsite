using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple inventory component for the player.
/// Tracks items the player has collected.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private List<string> items = new List<string>();

    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }

    public void AddItem(string itemName)
    {
        if (!items.Contains(itemName))
        {
            items.Add(itemName);
            Debug.Log("[PlayerInventory] Acquired: " + itemName);
        }
    }

    public void RemoveItem(string itemName)
    {
        if (items.Remove(itemName))
        {
            Debug.Log("[PlayerInventory] Removed: " + itemName);
        }
    }
}
