using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerCarryFood : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.Slash;
    public float interactRadius = 1.2f;

    [Header("Crate")]
    public LayerMask crateLayer;

    [Header("Tilemaps")]
    public Tilemap backgroundTilemap;
    public Tilemap counterTilemap;

    [Header("Carry Point")]
    public Transform carryPoint;

    private GameObject heldFood;

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (heldFood == null)
            {
                TryPickUpFoodFromCrate();
            }
            else
            {
                DropFood();
            }
        }
    }

    void TryPickUpFoodFromCrate()
    {
        Collider2D crateHit = Physics2D.OverlapCircle(
            transform.position,
            interactRadius,
            crateLayer
        );

        if (crateHit == null)
        {
            Debug.Log("No crate nearby.");
            return;
        }

        Crate crate = crateHit.GetComponent<Crate>();

        if (crate == null)
        {
            Debug.Log("The object nearby is on the Crate layer but has no FoodCrate script.");
            return;
        }

        if (crate.foodPrefab == null)
        {
            Debug.Log("This crate has no food prefab assigned.");
            return;
        }

        heldFood = Instantiate(
            crate.foodPrefab,
            carryPoint.position,
            Quaternion.identity,
            carryPoint
        );

        heldFood.transform.localPosition = Vector3.zero;

        Collider2D foodCollider = heldFood.GetComponent<Collider2D>();

        if (foodCollider != null)
        {
            foodCollider.enabled = false;
        }

        SpriteRenderer foodSprite = heldFood.GetComponent<SpriteRenderer>();
        SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();

        if (foodSprite != null && playerSprite != null)
        {
            foodSprite.sortingLayerID = playerSprite.sortingLayerID;
            foodSprite.sortingOrder = playerSprite.sortingOrder + 1;
        }

        Debug.Log("Picked up food.");
    }

    void DropFood()
    {
        Vector3 dropPosition;

        Vector3Int nearestCounterCell = FindNearestCounterCell();

        if (counterTilemap != null && counterTilemap.HasTile(nearestCounterCell))
        {
            dropPosition = counterTilemap.GetCellCenterWorld(nearestCounterCell);
            Debug.Log("Dropped food on counter.");
        }
        else
        {
            Vector3Int floorCell = backgroundTilemap.WorldToCell(transform.position);
            dropPosition = backgroundTilemap.GetCellCenterWorld(floorCell);
            Debug.Log("Dropped food on floor.");
        }

        heldFood.transform.SetParent(null);
        heldFood.transform.position = dropPosition;

        Collider2D foodCollider = heldFood.GetComponent<Collider2D>();

        if (foodCollider != null)
        {
            foodCollider.enabled = true;
        }

        heldFood = null;
    }

    Vector3Int FindNearestCounterCell()
    {
        Vector3Int playerCell = counterTilemap.WorldToCell(transform.position);

        Vector3Int nearestCell = playerCell;
        float nearestDistance = Mathf.Infinity;

        int searchRange = Mathf.CeilToInt(interactRadius) + 1;

        for (int x = -searchRange; x <= searchRange; x++)
        {
            for (int y = -searchRange; y <= searchRange; y++)
            {
                Vector3Int currentCell = playerCell + new Vector3Int(x, y, 0);

                if (!counterTilemap.HasTile(currentCell))
                {
                    continue;
                }

                Vector3 cellWorldPosition = counterTilemap.GetCellCenterWorld(currentCell);
                float distance = Vector2.Distance(transform.position, cellWorldPosition);

                if (distance <= interactRadius && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCell = currentCell;
                }
            }
        }

        return nearestCell;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
