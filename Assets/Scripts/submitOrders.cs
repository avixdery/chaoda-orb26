using Unity.VisualScripting;
using UnityEngine;

public class submitOrders : MonoBehaviour, IInteractable
{
    public int orderCount = 0;

    public void Interact(playerItemCollector player)
    {
        GameObject heldItem = player.GetHeldItem();

        if (heldItem == null)
            return;

        Destroy(heldItem);
        player.ClearHeldItem();

        orderCount++;


        Debug.Log("what a nice day! order count: " + orderCount);
    }
}