using UnityEngine;

public class CuttingStation : MonoBehaviour
{
    [Header("Cutting Settings")]
    public Sprite cutFishSprite;
    public string rawFishNameContains = "Fish";
    public KeyCode cutKey = KeyCode.Period;

    [Header("Detection")]
    public Vector2 detectionSize = new Vector2(1f, 1f);
    public Vector2 detectionOffset = Vector2.zero;
    public LayerMask foodLayer = ~0;

    void Update()
    {
        if (Input.GetKeyDown(cutKey))
        {
            TryCutFish();
        }
    }

    void TryCutFish()
    {
        Vector2 center = (Vector2)transform.position + detectionOffset;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            center,
            detectionSize,
            0f,
            foodLayer
        );

        foreach (Collider2D hit in hits)
        {
            GameObject foodObject = hit.gameObject;

            if (foodObject.name.Contains(rawFishNameContains))
            {
                SpriteRenderer sr = foodObject.GetComponent<SpriteRenderer>();

                if (sr == null)
                {
                    sr = foodObject.GetComponentInChildren<SpriteRenderer>();
                }

                sr.sprite = cutFishSprite;
                foodObject.name = "CutFish";

                FoodItem foodItem = foodObject.GetComponent<FoodItem>();

                if (foodItem != null)
                {
                    foodItem.foodType = FoodType.CutFish;
                }

                return;
            }
        }
   }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(
            (Vector2)transform.position + detectionOffset,
            detectionSize
        );
    }
}
