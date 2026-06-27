using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class submit_an_order : MonoBehaviour
{
    private int ordersCompleted = 0;
    [Header("References")]
    public Transform holdPoint;
    public Tilemap counterTilemap;

    [Header("Interaction")]
    public LayerMask submissionLayer;
    public LayerMask counterLayer;
    public float interactRadius = 0.6f;

    [Header("Rice Position Offsets")]
    public Vector3 heldRiceOffset = Vector3.zero;
    public Vector3 counterRiceOffset = Vector3.zero;

    [Header("Rice Sorting")]
    public string heldSortingLayer = "player";
    public int heldOrderInLayer = 10;

    public string counterSortingLayer = "counter";
    public int counterOrderInLayer = 10;

    private GameObject heldRice;

    // This stores which counter tile has rice on it.
    private Dictionary<Vector3Int, GameObject> riceOnCounters = new Dictionary<Vector3Int, GameObject>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            TryInteract();
        }
    }

    void SubmitRice()
    {
        Debug.Log("submitting rice!");
        Destroy(heldRice);
        heldRice = null;
        ordersCompleted++;
        Debug.Log("order update: " + ordersCompleted);
    }

    void TryInteract()
    {
        // CASE 1: Barty is already holding rice.
        if (heldRice != null)
        {
            if (IsNearSubmitTile())
            {
                SubmitRice();
                return;
            }

            if (TryGetNearbyCounterCell(out Vector3Int counterCell))
            {
                DropRiceOnCounter(counterCell);
            }
            return;
        }
    }
    
    bool IsNearSubmitTile()
    {
        Vector2 checkPosition = GetInteractPosition();

        Collider2D submitHit = Physics2D.OverlapCircle(
            checkPosition,
            interactRadius,
            submissionLayer
        );
        
        Debug.Log("Submit hit: " + submitHit);
        return submitHit != null;
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

        // First make sure there is actually a counter collider nearby.
        Collider2D counterHit = Physics2D.OverlapCircle(
            checkPosition,
            interactRadius,
            counterLayer
        );

        if (counterHit == null)
        {
            return false;
        }

        // Convert Barty's interact position to a Tilemap cell.
        Vector3Int originCell = counterTilemap.WorldToCell(checkPosition);

        bool foundTile = false;
        float bestDistance = Mathf.Infinity;

        // Search nearby cells around Barty's hands.
        // This makes it less strict, so you don't need to stand perfectly on the tile.
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

    void DropRiceOnCounter(Vector3Int counterCell)
    {
        if (riceOnCounters.ContainsKey(counterCell) && riceOnCounters[counterCell] != null)
        {
            Debug.Log("This counter tile already has rice on it.");
            return;
        }

        Vector3 dropPosition = counterTilemap.GetCellCenterWorld(counterCell) + counterRiceOffset;

        heldRice.transform.SetParent(null);
        heldRice.transform.position = dropPosition;

        SetRiceSorting(heldRice, counterSortingLayer, counterOrderInLayer);

        riceOnCounters[counterCell] = heldRice;
        heldRice = null;

        Debug.Log("Dropped rice on counter.");
    }


    void SetRiceSorting(GameObject riceObject, string sortingLayerName, int orderInLayer)
    {
        SpriteRenderer spriteRenderer = riceObject.GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning("Rice prefab has no SpriteRenderer.");
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
