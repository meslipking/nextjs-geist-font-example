using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SummoningTower : MonoBehaviour
{
    public static SummoningTower Instance { get; private set; }

    [System.Serializable]
    public class RarityRate
    {
        public Rarity rarity;
        public float baseRate;
        public float pityRate; // Additional rate added per failed summon
    }

    [Header("Summoning Settings")]
    [SerializeField] private RarityRate[] rarityRates;
    [SerializeField] private int pityCounter = 50; // Guaranteed SSR or higher after this many summons
    [SerializeField] private AnimalData[] availableAnimals;
    [SerializeField] private Transform summoningPoint;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem summoningCircle;
    [SerializeField] private ParticleSystem rarityEffect;
    [SerializeField] private AudioClip[] summoningSounds;

    private int currentPityCount;
    private Dictionary<Rarity, List<AnimalData>> animalsByRarity;
    private System.Random random;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        random = new System.Random();
        animalsByRarity = new Dictionary<Rarity, List<AnimalData>>();

        // Group animals by rarity
        foreach (Rarity rarity in System.Enum.GetValues(typeof(Rarity)))
        {
            animalsByRarity[rarity] = availableAnimals
                .Where(a => a.rarity == rarity)
                .ToList();
        }
    }

    public AnimalData Summon(int resourceCost)
    {
        currentPityCount++;

        // Check if pity system should activate
        if (currentPityCount >= pityCounter)
        {
            return GuaranteedHighRaritySummon();
        }

        // Normal summoning
        float summonValue = (float)random.NextDouble();
        float currentRate = 0f;

        foreach (var rate in rarityRates.OrderByDescending(r => (int)r.rarity))
        {
            currentRate += rate.baseRate + (rate.pityRate * currentPityCount);
            if (summonValue <= currentRate)
            {
                return SummonFromRarity(rate.rarity);
            }
        }

        // Fallback to N rarity if nothing else is selected
        return SummonFromRarity(Rarity.N);
    }

    private AnimalData GuaranteedHighRaritySummon()
    {
        currentPityCount = 0;
        Rarity[] highRarities = new[] { Rarity.SSSPlus, Rarity.SSS, Rarity.SSR };
        
        foreach (var rarity in highRarities)
        {
            if (animalsByRarity[rarity].Count > 0)
            {
                return SummonFromRarity(rarity);
            }
        }

        // Fallback to SR if no higher rarity animals are available
        return SummonFromRarity(Rarity.SR);
    }

    private AnimalData SummonFromRarity(Rarity rarity)
    {
        List<AnimalData> possibleAnimals = animalsByRarity[rarity];
        if (possibleAnimals.Count == 0)
        {
            Debug.LogWarning($"No animals available for rarity {rarity}");
            return null;
        }

        int index = random.Next(possibleAnimals.Count);
        AnimalData summonedAnimal = possibleAnimals[index];

        // Play summoning effects
        PlaySummoningEffects(rarity);

        return summonedAnimal;
    }

    private void PlaySummoningEffects(Rarity rarity)
    {
        // Play base summoning circle effect
        if (summoningCircle != null)
        {
            ParticleSystem.MainModule main = summoningCircle.main;
            main.startColor = GetRarityColor(rarity);
            summoningCircle.Play();
        }

        // Play rarity-specific effect
        if (rarityEffect != null)
        {
            ParticleSystem.MainModule main = rarityEffect.main;
            main.startColor = GetRarityColor(rarity);
            rarityEffect.Play();
        }

        // Play sound effect
        int soundIndex = Mathf.Min((int)rarity, summoningSounds.Length - 1);
        AudioManager.Instance.PlaySound(summoningSounds[soundIndex].name);
    }

    private Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.N:
                return Color.white;
            case Rarity.R:
                return Color.blue;
            case Rarity.SR:
                return Color.magenta;
            case Rarity.SSR:
                return Color.yellow;
            case Rarity.SSS:
                return Color.red;
            case Rarity.SSSPlus:
                return new Color(1f, 0f, 1f); // Bright purple
            default:
                return Color.white;
        }
    }

    public void SpawnSummonedAnimal(AnimalData animalData, Vector2Int position, bool isPlayer1)
    {
        // Create the animal GameObject
        Vector3 worldPos = GameBoard.Instance.GetWorldPosition(position);
        GameObject animalObj = Instantiate(animalData.prefab, worldPos, Quaternion.identity);
        
        // Set up the animal component
        Animal animal = animalObj.GetComponent<Animal>();
        if (animal != null)
        {
            animal.Initialize(animalData);
            animal.gameObject.tag = isPlayer1 ? "Player1" : "Player2";
            
            // Add to game manager's tracking
            GameManager.Instance.RegisterSummonedAnimal(animal, isPlayer1);
        }

        // Play spawn effects
        if (animalData.summonEffect != null)
        {
            ParticleSystem effect = Instantiate(animalData.summonEffect, worldPos, Quaternion.identity);
            Destroy(effect.gameObject, effect.main.duration);
        }

        // Play summon sound
        if (animalData.summonSound != null)
        {
            AudioManager.Instance.PlaySound(animalData.summonSound.name);
        }
    }

    public float GetCurrentPityProgress()
    {
        return (float)currentPityCount / pityCounter;
    }

    public Dictionary<Rarity, float> GetCurrentRates()
    {
        Dictionary<Rarity, float> currentRates = new Dictionary<Rarity, float>();
        foreach (var rate in rarityRates)
        {
            currentRates[rate.rarity] = rate.baseRate + (rate.pityRate * currentPityCount);
        }
        return currentRates;
    }
}
