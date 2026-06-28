using UnityEngine;

public enum FoodType
{
    UncookedRice,
    UncookedVege,
    RawFish,
    CutFish,

    EmptyPlate,

    CookedRicePlate,
    CookedVegePlate,
    CookedFishPlate
}

public class FoodItem : MonoBehaviour
{
    public FoodType foodType;
}
