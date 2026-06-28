using System.Collections;
using UnityEngine;

public class Pot : MonoBehaviour, IInteractable
{
    [Header("Cooking Time")]
    public float cookTime = 5f;

    [Header("Pot Sprites")]
    public Sprite emptyPotSprite;

    public Sprite riceCookingSprite;
    public Sprite riceCookedSprite;

    public Sprite vegeCookingSprite;
    public Sprite vegeCookedSprite;

    public Sprite fishCookingSprite;
    public Sprite fishCookedSprite;

    [Header("Food On Plate Prefabs")]
    public GameObject cookedRicePlatePrefab;
    public GameObject cookedVegePlatePrefab;
    public GameObject cookedFishPlatePrefab;

    private bool isCooking = false;
    private bool hasCookedFood = false;

    private FoodType foodInPot;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (emptyPotSprite != null)
        {
            spriteRenderer.sprite = emptyPotSprite;
        }
    }

    public void Interact(playerItemCollector playerCarry)
    {
        if (playerCarry == null)
        {
            return;
        }

        if (isCooking)
        {
            Debug.Log("Pot is still cooking.");
            return;
        }

        if (hasCookedFood)
        {
            PlateCookedFood(playerCarry);
            return;
        }

        if (!playerCarry.IsHoldingItem())
        {
            Debug.Log("You are not holding any food.");
            return;
        }

        GameObject heldItem = playerCarry.GetHeldItem();
        FoodItem foodItem = heldItem.GetComponent<FoodItem>();

        if (foodItem == null)
        {
            Debug.Log("Held item has no FoodItem script.");
            return;
        }

        Sprite cookingSprite = GetCookingSprite(foodItem.foodType);
        Sprite cookedSprite = GetCookedSprite(foodItem.foodType);

        if (cookingSprite == null || cookedSprite == null)
        {
            Debug.Log("This food cannot be cooked: " + foodItem.foodType);
            return;
        }

        foodInPot = foodItem.foodType;

        playerCarry.ClearHeldItem();
        Destroy(heldItem);

        StartCoroutine(CookFood(cookingSprite, cookedSprite));
    }

    private IEnumerator CookFood(Sprite cookingSprite, Sprite cookedSprite)
    {
        isCooking = true;

        spriteRenderer.sprite = cookingSprite;

        yield return new WaitForSeconds(cookTime);

        spriteRenderer.sprite = cookedSprite;

        hasCookedFood = true;
        isCooking = false;
    }

    private void PlateCookedFood(playerItemCollector playerCarry)
    {
        if (!playerCarry.IsHoldingItem())
        {
            Debug.Log("Need an empty plate.");
            return;
        }

        GameObject heldItem = playerCarry.GetHeldItem();
        FoodItem foodItem = heldItem.GetComponent<FoodItem>();

        if (foodItem == null || foodItem.foodType != FoodType.EmptyPlate)
        {
            Debug.Log("Need an empty plate.");
            return;
        }

        GameObject platedFoodPrefab = GetPlatedFoodPrefab();

        if (platedFoodPrefab == null)
        {
            Debug.Log("Missing cooked plate prefab.");
            return;
        }

        playerCarry.ClearHeldItem();
        Destroy(heldItem);

        GameObject platedFood = Instantiate(
            platedFoodPrefab,
            playerCarry.holdPoint.position,
            Quaternion.identity
        );

        playerCarry.PickUpExistingItem(platedFood);

        hasCookedFood = false;

        if (emptyPotSprite != null)
        {
            spriteRenderer.sprite = emptyPotSprite;
        }
    }

    private Sprite GetCookingSprite(FoodType foodType)
    {
        switch (foodType)
        {
            case FoodType.UncookedRice:
                return riceCookingSprite;

            case FoodType.UncookedVege:
                return vegeCookingSprite;

            case FoodType.CutFish:
                return fishCookingSprite;

            default:
                return null;
        }
    }

    private Sprite GetCookedSprite(FoodType foodType)
    {
        switch (foodType)
        {
            case FoodType.UncookedRice:
                return riceCookedSprite;

            case FoodType.UncookedVege:
                return vegeCookedSprite;

            case FoodType.CutFish:
                return fishCookedSprite;

            default:
                return null;
        }
    }

    private GameObject GetPlatedFoodPrefab()
    {
        switch (foodInPot)
        {
            case FoodType.UncookedRice:
                return cookedRicePlatePrefab;

            case FoodType.UncookedVege:
                return cookedVegePlatePrefab;

            case FoodType.CutFish:
                return cookedFishPlatePrefab;

            default:
                return null;
        }
    }
}
