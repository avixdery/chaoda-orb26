using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class playerItemCollector : MonoBehaviour
{
    [Header("References")]
//    public GameObject ricePrefab;
    public Transform holdPoint;
    public Tilemap counterTilemap;

    [Header("Interaction")]
    public LayerMask crateLayer;
    public LayerMask counterLayer;
    public float interactRadius = 0.6f;

    [Header("Food Position Offsets")]
    public Vector3 heldItemOffset = Vector3.zero;
    public Vector3 counterItemOffset = Vector3.zero;

    [Header("Food Sorting")]
    public string heldSortingLayer = "player";
    public int heldOrderInLayer = 10;

    public string counterSortingLayer = "counter";
    public int counterOrderInLayer = 10;

    private GameObject heldItem;

    // keep track of which tile has which item
    private Dictionary<Vector3Int, GameObject> itemOnCounters = new Dictionary<Vector3Int, GameObject>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
       // CASE 1: Barty is already holding an item, press / to let go near a counter
        if (heldItem != null)
        {
            if (TryInteractWithNearbyObject())
            {
                return;
            }
            if (TryGetNearbyCounterCell(out Vector3Int counterCell))
            {
                DropItemOnCounter(counterCell);
            }
            else
            {
                Debug.Log("Barty is holding something, but there is no counter nearby.");
            }

            return;
        }

        // CASE 2: Barty is not holding anything and there is food on a tile nearby
        if (TryGetNearbyCounterCell(out Vector3Int nearbyCounterCell))
        {
            if (itemOnCounters.ContainsKey(nearbyCounterCell) && itemOnCounters[nearbyCounterCell] != null)
            {
                PickUpItemFromCounter(nearbyCounterCell);
                return;
            }
        }

        // CASE 3: Barty is not holding anything and is near a food crate

        FoodCrate nearbyFoodCrate = GetNearbyFoodCrate();
        if (nearbyFoodCrate)
        {
            SpawnItemInHands(nearbyFoodCrate);
            return;
        }

        Debug.Log("Nothing to interact with. Go near the rice crate or a counter with rice.");
    }

    bool TryInteractWithNearbyObject()
    {
        Vector2 checkPosition = GetInteractPosition();

        Collider2D hit = Physics2D.OverlapCircle(
            checkPosition,
            interactRadius,
            crateLayer
        );

        if (hit == null)
        {
            return false;
        }

        IInteractable interactable = hit.GetComponent<IInteractable>();

        if (interactable == null)
        {
            return false;
        }

        interactable.Interact(this);
        return true;
    }


    FoodCrate GetNearbyFoodCrate()
    {
        Vector2 checkPosition = GetInteractPosition();

        Collider2D crateHit = Physics2D.OverlapCircle(
            checkPosition,
            interactRadius,
            crateLayer
        );
        
        if (crateHit == null)
        {
            return null;
        }

        FoodCrate foodCrate = crateHit.GetComponent<FoodCrate>();

        if (foodCrate == null)
        {
            Debug.Log("Object is on Crate layer, but has no FoodCrate script attached.");
            return null;
        }

        if (foodCrate.foodPrefab == null)
        {
            Debug.Log("FoodCrate has no food prefab assigned.");
            return null;
        }

        return foodCrate;

      //  return crateHit != null;
    }

    bool TryGetNearbyCounterCell(out Vector3Int bestCell)
    {
        bestCell = Vector3Int.zero;

        if (counterTilemap == null)
        {
            Debug.LogWarning("Counter Tilemap is not assigned on BartyRiceHandler.");
            return false;
        }

        Vector3 checkPosition = GetInteractPosition();

        Collider2D counterHit = Physics2D.OverlapCircle(
            checkPosition,
            interactRadius,
            counterLayer
        );

        if (counterHit == null)
        {
            return false;
        }

        Vector3Int originCell = counterTilemap.WorldToCell(checkPosition);

        bool foundTile = false;
        float bestDistance = Mathf.Infinity;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int cellToCheck = originCell + new Vector3Int(x, y, 0);

                if (!counterTilemap.HasTile(cellToCheck))
                {
                    continue;
                }

                Vector3 cellCenter = counterTilemap.GetCellCenterWorld(cellToCheck);
                float distance = Vector2.Distance(checkPosition, cellCenter);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestCell = cellToCheck;
                    foundTile = true;
                }
            }
        }

        return foundTile;
    }

    void SpawnItemInHands(FoodCrate foodCrate)
    {
/*        if (ricePrefab == null)
        {
            Debug.LogWarning("Rice Prefab is not assigned on BartyRiceHandler.");
            return;
        }
*/
        if (holdPoint == null)
        {
            Debug.LogWarning("HoldPoint is not assigned");
            return;
        }

        heldItem = Instantiate(foodCrate.foodPrefab, holdPoint.position, Quaternion.identity);
        heldItem.transform.SetParent(holdPoint);
        heldItem.transform.localPosition = heldItemOffset;

        SetItemSorting(heldItem, heldSortingLayer, heldOrderInLayer);

        Debug.Log("Picked up food from crate.");
    }

    void DropItemOnCounter(Vector3Int counterCell)
    {
        if (itemOnCounters.ContainsKey(counterCell) && itemOnCounters[counterCell] != null)
        {
            Debug.Log("This counter tile already has food on it.");
            return;
        }

        Vector3 dropPosition = counterTilemap.GetCellCenterWorld(counterCell) + counterItemOffset;

        heldItem.transform.SetParent(null);
        heldItem.transform.position = dropPosition;

        SetItemSorting(heldItem, counterSortingLayer, counterOrderInLayer);

        itemOnCounters[counterCell] = heldItem;
        heldItem = null;

        Debug.Log("Dropped food on counter.");
    }

    void PickUpItemFromCounter(Vector3Int counterCell)
    {
        GameObject item = itemOnCounters[counterCell];

        itemOnCounters.Remove(counterCell);

        heldItem = item;
        heldItem.transform.SetParent(holdPoint);
        heldItem.transform.localPosition = heldItemOffset;

        SetItemSorting(heldItem, heldSortingLayer, heldOrderInLayer);

        Debug.Log("Picked food back up from counter.");
    }
    
    public bool IsHoldingItem()
    {
        return heldItem != null;
    }

    public GameObject GetHeldItem()
    {
        return heldItem;
    }

    public void ClearHeldItem()
    {
        heldItem = null;
    }

    public void PickUpExistingItem(GameObject item)
    {
        if (item == null)
        {
            return;
        }

        if (holdPoint == null)
        {
            Debug.LogWarning("HoldPoint is not assigned");
            return;
        }

        heldItem = item;
        heldItem.transform.SetParent(holdPoint);
        heldItem.transform.localPosition = heldItemOffset;

        SetItemSorting(heldItem, heldSortingLayer, heldOrderInLayer);
    }

    void SetItemSorting(GameObject itemObject, string sortingLayerName, int orderInLayer)
    {
        SpriteRenderer spriteRenderer = itemObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning("Prefab has no SpriteRenderer.");
            return;
        }

        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = orderInLayer;
    }

    Vector2 GetInteractPosition()
    {
        if (holdPoint != null)
        {
            return holdPoint.position;
        }

        return transform.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetInteractPosition(), interactRadius);
    }
}
