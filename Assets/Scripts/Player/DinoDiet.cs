using UnityEngine;
using System.Collections.Generic;

public enum DinoDietType
{
    Carnivore,   // Плотоядный (мясо, рыба, падаль)
    Herbivore,   // Травоядный (трава)
    Omnivore,    // Всеядный (всё)
    Piscivore,   // Рыбоядный (рыба, моллюски)
    Insectivore  // Насекомоядный (насекомые)
}

public class DinoDiet : MonoBehaviour
{
    [Header("Dino Diet Type")]
    [SerializeField] private DinoDietType dietType = DinoDietType.Carnivore;

    [Header("Custom Edible Types")]
    [Tooltip("Если не указано — используются стандартные для типа")]
    [SerializeField] private List<FoodType> customEdibleFoods = new List<FoodType>();

    private Dictionary<DinoDietType, FoodType[]> standardDiet = new Dictionary<DinoDietType, FoodType[]>()
    {
        { DinoDietType.Carnivore, new FoodType[] { FoodType.Meat, FoodType.Carrion } },
        { DinoDietType.Herbivore, new FoodType[] { FoodType.Grass } },
        { DinoDietType.Omnivore, new FoodType[] { FoodType.Meat, FoodType.Grass, FoodType.Fish, FoodType.Insect, FoodType.Mollusk, FoodType.Carrion } },
        { DinoDietType.Piscivore, new FoodType[] { FoodType.Fish, FoodType.Mollusk } },
        { DinoDietType.Insectivore, new FoodType[] { FoodType.Insect } }
    };

    public DinoDietType DietType => dietType;

    void Start()
    {
        // Если нет кастомных — используем стандартные
        if (customEdibleFoods.Count == 0)
        {
            customEdibleFoods = new List<FoodType>(standardDiet[dietType]);
        }
    }

    public bool CanEat(FoodType foodType)
    {
        return customEdibleFoods.Contains(foodType);
    }

    public bool CanEat(FoodSource food)
    {
        return food != null && food.IsAvailable && CanEat(food.Type);
    }

    public List<FoodType> GetEdibleFoods()
    {
        return customEdibleFoods;
    }

    // Для отладки
    public string GetDietDescription()
    {
        string foods = "";
        foreach (FoodType food in customEdibleFoods)
        {
            foods += food.ToString() + ", ";
        }
        return $"{dietType}: {foods.TrimEnd(',', ' ')}";
    }
}