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
    public LayerMask stationLayer;
    public float interactRadius = 0.6f; // jy: when "/" is pressed, unity searches in this circle

    [Header("Food Position Offsets")]
    public Vector3 heldItemOffset = Vector3.zero;
    public Vector3 counterItemOffset = Vector3.zero;

    [Header("Food Sorting")]
    public string heldSortingLayer = "player";
    public int heldOrderInLayer = 10;

    public string counterSortingLayer = "counter";
    public int counterOrderInLayer = 10;

    public GameObject heldItem;

    // keep track of which tile has which item -> which crate contains which item?
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
            if (TryInteractWithNearbyObject()) // can oc interact with this thing? else put it down
            {
                return;
            }
            if (TryGetNearbyCounterCell(out Vector3Int counterCell))
            {
                DropItemOnCounter(counterCell);
            }
            else
            {
            }

            return;
        }

        // CASE 2: Barty is not holding anything and there is food on a tile nearby
        if (TryGetNearbyCounterCell(out Vector3Int nearbyCounterCell))
        {
            if (itemOnCounters.ContainsKey(nearbyCounterCell) && itemOnCounters[nearbyCounterCell] != null)
            {
                PickUpItemFromCounter(nearbyCounterCell); // there is something on the counter!
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
   }

    bool TryInteractWithNearbyObject()
    {
        Vector2 checkPosition = GetInteractPosition();

        Collider2D hit = Physics2D.OverlapCircle(
            checkPosition,
            interactRadius,
            stationLayer
        );

        if (hit == null)
        {
            Debug.Log("No station found.");
            return false;
            

        }
        Debug.Log("Found: " + hit.name);

        IInteractable interactable = hit.GetComponent<IInteractable>(); // is this object interactable?

        if (interactable == null)
        {
            Debug.Log("nothing found");
            return false;
            
        }
        Debug.Log("can interact");
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

        return foodCrate;

      //  return crateHit != null;
    }

    bool TryGetNearbyCounterCell(out Vector3Int bestCell)
    {
        bestCell = Vector3Int.zero;

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
        // check surrounding tiles
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3Int cellToCheck = originCell + new Vector3Int(x, y, 0);

                if (!counterTilemap.HasTile(cellToCheck)) // closest tile
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
        heldItem = Instantiate(foodCrate.foodPrefab, holdPoint.position, Quaternion.identity);
        heldItem.transform.SetParent(holdPoint);
        heldItem.transform.localPosition = heldItemOffset;

        SetItemSorting(heldItem, heldSortingLayer, heldOrderInLayer);

   }

    void DropItemOnCounter(Vector3Int counterCell)
    {
        if (itemOnCounters.ContainsKey(counterCell) && itemOnCounters[counterCell] != null)
        {
           return;
        }

        Vector3 dropPosition = counterTilemap.GetCellCenterWorld(counterCell) + counterItemOffset;

        heldItem.transform.SetParent(null);
        heldItem.transform.position = dropPosition;

        SetItemSorting(heldItem, counterSortingLayer, counterOrderInLayer);

        itemOnCounters[counterCell] = heldItem;
        heldItem = null;

   }

    void PickUpItemFromCounter(Vector3Int counterCell)
    {
        GameObject item = itemOnCounters[counterCell];

        itemOnCounters.Remove(counterCell);

        heldItem = item;
        heldItem.transform.SetParent(holdPoint);
        heldItem.transform.localPosition = heldItemOffset;

        SetItemSorting(heldItem, heldSortingLayer, heldOrderInLayer);

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

        heldItem = item;
        heldItem.transform.SetParent(holdPoint);
        heldItem.transform.localPosition = heldItemOffset;

        SetItemSorting(heldItem, heldSortingLayer, heldOrderInLayer);
    }

    void SetItemSorting(GameObject itemObject, string sortingLayerName, int orderInLayer)
    {
        SpriteRenderer spriteRenderer = itemObject.GetComponent<SpriteRenderer>();

        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = orderInLayer;
    }

    public Vector2 GetInteractPosition()
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
