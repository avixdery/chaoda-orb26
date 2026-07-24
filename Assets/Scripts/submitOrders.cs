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

        FoodItem food = heldItem.GetComponent<FoodItem>();

        if (food == null)
            return;

        bool canSubmit =
        food.foodType == FoodType.CookedRicePlate ||
        food.foodType == FoodType.CookedVegePlate ||
        food.foodType == FoodType.CookedFishPlate;

        if (!canSubmit)
        {
            Debug.Log("cannot submit.");
            return;
        }


        Destroy(heldItem);
        player.ClearHeldItem();

        orderCount++;


        Debug.Log("what a nice day! order count: " + orderCount);
    }
}