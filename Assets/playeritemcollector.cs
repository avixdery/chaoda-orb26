using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BartyRiceHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject ricePrefab;
    public Transform holdPoint;
    public Tilemap counterTilemap;

    [Header("Interaction")]
    public LayerMask crateLayer;
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
    private Queue<Order> orderQueue = new Queue<Order>();
    private int ordersCompleted = 0;

    public GameObject orderTicketPrefab;
    public Transform ticketParent;
    private List<GameObject> activeTickets = new List<GameObject>();


    // This stores which counter tile has rice on it.
    private Dictionary<Vector3Int, GameObject> riceOnCounters = new Dictionary<Vector3Int, GameObject>();
    
    void Start()
    {
        GenerateOrders();
    }

    void GenerateOrders()
    {
        orderQueue.Enqueue(new Order { recipeName = "Rice" });
        orderQueue.Enqueue(new Order { recipeName = "More Rice" });
        orderQueue.Enqueue(new Order { recipeName = "We Only Serve Plain Rice" });

        for (int i = 0; i < 8; i++)
        {
            SpawnTicket();
        }

        Debug.Log("Rice orders created: " + orderQueue.Count);
    }

    void SpawnTicket()
    {
        GameObject ticket = Instantiate(orderTicketPrefab, ticketParent);
        activeTickets.Add(ticket);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            TryInteract();
        }
    }

    void SubmitRice()
    {
        Destroy(heldRice);
        heldRice = null;

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

    void RemoveTicket()
    {
        if (activeTickets.Count == 0) return;

        GameObject ticket = activeTickets[0];
        activeTickets.RemoveAt(0);
        Destroy(ticket);
    }

    public void SubmitFromButton()
    {
        SubmitRice();
    }

    void TryInteract()
    {
        // CASE 1: Barty is already holding rice.
        // Press / near a counter to drop it.
        if (heldRice != null)
        {
            if (TryGetNearbyCounterCell(out Vector3Int counterCell))
            {
                DropRiceOnCounter(counterCell);
            }
            else
            {
                Debug.Log("Barty is holding rice, but there is no counter nearby.");
            }

            return;
        }

        // CASE 2: Barty is not holding rice.
        // First check whether this nearby counter tile already has rice.
        if (TryGetNearbyCounterCell(out Vector3Int nearbyCounterCell))
        {
            if (riceOnCounters.ContainsKey(nearbyCounterCell) && riceOnCounters[nearbyCounterCell] != null)
            {
                PickUpRiceFromCounter(nearbyCounterCell);
                return;
            }
        }

        // CASE 3: Barty is not holding rice and no rice is on the counter.
        // If near rice crate, spawn fresh rice into Barty's hands.
        if (IsNearRiceCrate())
        {
            SpawnRiceInHands();
            return;
        }

        Debug.Log("Nothing to interact with. Go near the rice crate or a counter with rice.");
    }

    bool IsNearRiceCrate()
    {
        Vector2 checkPosition = GetInteractPosition();

        Collider2D crateHit = Physics2D.OverlapCircle(
            checkPosition,
            interactRadius,
            crateLayer
        );

        return crateHit != null;
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

    void SpawnRiceInHands()
    {
        if (ricePrefab == null)
        {
            Debug.LogWarning("Rice Prefab is not assigned on BartyRiceHandler.");
            return;
        }

        if (holdPoint == null)
        {
            Debug.LogWarning("HoldPoint is not assigned on BartyRiceHandler.");
            return;
        }

        heldRice = Instantiate(ricePrefab, holdPoint.position, Quaternion.identity);
        heldRice.transform.SetParent(holdPoint);
        heldRice.transform.localPosition = heldRiceOffset;

        SetRiceSorting(heldRice, heldSortingLayer, heldOrderInLayer);

        Debug.Log("Picked up rice from crate.");
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

    void PickUpRiceFromCounter(Vector3Int counterCell)
    {
        GameObject rice = riceOnCounters[counterCell];

        riceOnCounters.Remove(counterCell);

        heldRice = rice;
        heldRice.transform.SetParent(holdPoint);
        heldRice.transform.localPosition = heldRiceOffset;

        SetRiceSorting(heldRice, heldSortingLayer, heldOrderInLayer);

        Debug.Log("Picked rice back up from counter.");
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

[System.Serializable]
public class Order
{
    public string recipeName;
}

