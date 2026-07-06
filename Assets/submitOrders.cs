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

/*
public class submitOrders : MonoBehaviour
{
    public float radius = playerItemCollector.interactRadius;
    Vector2 checkPosition = playerItemCollector.GetInteractPosition();
    
    // Update is called once per frame
    void Update()
    {
        GameObject heldItem = playerItemCollector.HeldItem();
        // define object barty is holding
        // if food is 1) cooked 2) on a plate (limit to these few elements)
        // food disappears
        Collider2D submitTileHit = Physics2D.OverlapCircle(
            checkPosition,
            radius,
            public LayerMask submitLayer;
        );

   // figure out what submitTileHit returns 

        if (heldItem != null && submitTileHit == true) 
        {
            SubmitRice();
        }
        // order count += 1
        // order disappears
        // order line moves
    }

    void SubmitRice()
    {
        Destroy(heldItem);
        heldItem = null;
    }  

void orderUpdate()
    {
        if (orderQueue.Count > 0)
        {
            orderQueue.Dequeue();
            ordersCompleted++;

            Debug.Log("Rice submitted!");
            RemoveTicket();
        }
        else
        {
            Debug.Log("No active orders!");
        }
    }
}

*/
