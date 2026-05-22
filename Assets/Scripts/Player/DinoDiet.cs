using UnityEngine;
using System.Collections.Generic;

public enum DinoDietType
{
    Carnivore,   // Плотоядный (мясо, падаль)
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
    [Tooltip("Если оставить пустым – будут использованы стандартные типы для выбранного режима.")]
    [SerializeField] private List<FoodType> customEdibleFoods = new List<FoodType>();

    // Стандартные рационы
    private Dictionary<DinoDietType, FoodType[]> standardDiet = new Dictionary<DinoDietType, FoodType[]>()
    {
        { DinoDietType.Carnivore, new FoodType[] { FoodType.Meat, FoodType.Carrion } },
        { DinoDietType.Herbivore, new FoodType[] { FoodType.Grass } },
        { DinoDietType.Omnivore, new FoodType[] { FoodType.Meat, FoodType.Grass, FoodType.Fish, FoodType.Insect, FoodType.Mollusk, FoodType.Carrion } },
        { DinoDietType.Piscivore, new FoodType[] { FoodType.Fish, FoodType.Mollusk } },
        { DinoDietType.Insectivore, new FoodType[] { FoodType.Insect } }
    };

    public DinoDietType DietType => dietType;

    /// <summary>
    /// Возвращает актуальный список съедобных типов: кастомный, если он не пуст, иначе стандартный.
    /// </summary>
    private List<FoodType> GetEffectiveFoodList()
    {
        if (customEdibleFoods != null && customEdibleFoods.Count > 0)
            return customEdibleFoods;
        else
            return new List<FoodType>(standardDiet[dietType]);
    }

    public bool CanEat(FoodType foodType)
    {
        bool can = GetEffectiveFoodList().Contains(foodType);
        return can;
    }

    public bool CanEat(FoodSource food)
    {
        return food != null && food.IsAvailable && CanEat(food.Type);
    }

    public List<FoodType> GetEdibleFoods()
    {
        return GetEffectiveFoodList();
    }

    public string GetDietDescription()
    {
        string foods = "";
        foreach (FoodType food in GetEffectiveFoodList())
        {
            foods += food.ToString() + ", ";
        }
        return $"{dietType}: {foods.TrimEnd(',', ' ')}";
    }
}