using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SummoningUI : MonoBehaviour
{
    [Header("Main UI Elements")]
    [SerializeField] private GameObject summoningPanel;
    [SerializeField] private Button summonButton;
    [SerializeField] private Button multiSummonButton;
    [SerializeField] private TextMeshProUGUI resourceText;
    [SerializeField] private Slider pitySlider;
    [SerializeField] private TextMeshProUGUI pityText;

    [Header("Rates Display")]
    [SerializeField] private Transform ratesContainer;
    [SerializeField] private GameObject rateDisplayPrefab;

    [Header("Results UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Image animalImage;
    [SerializeField] private TextMeshProUGUI animalNameText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI skillsText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private ParticleSystem summonEffect;
    [SerializeField] private ParticleSystem rarityEffect;

    [Header("Animation Settings")]
    [SerializeField] private float cardFlipDuration = 1f;
    [SerializeField] private AnimationCurve flipCurve;
    [SerializeField] private GameObject cardPrefab;

    [Header("Multi-Summon UI")]
    [SerializeField] private Transform multiSummonContainer;
    [SerializeField] private GameObject multiSummonCardPrefab;
    [SerializeField] private Button skipAnimationButton;

    private Dictionary<Rarity, Color> rarityColors = new Dictionary<Rarity, Color>
    {
        { Rarity.N, Color.white },
        { Rarity.R, Color.blue },
        { Rarity.SR, Color.magenta },
        { Rarity.SSR, Color.yellow },
        { Rarity.SSS, Color.red },
        { Rarity.SSSPlus, new Color(1f, 0f, 1f) }
    };

    private void Start()
    {
        InitializeUI();
        UpdateRatesDisplay();
    }

    private void InitializeUI()
    {
        summonButton.onClick.AddListener(OnSummonClick);
        multiSummonButton.onClick.AddListener(OnMultiSummonClick);
        confirmButton.onClick.AddListener(OnConfirmClick);
        skipAnimationButton.onClick.AddListener(SkipAnimation);
        skipAnimationButton.gameObject.SetActive(false);

        // Initialize pity slider
        pitySlider.value = SummoningTower.Instance.GetCurrentPityProgress();
        UpdatePityText();
    }

    private void UpdateRatesDisplay()
    {
        // Clear existing rate displays
        foreach (Transform child in ratesContainer)
        {
            Destroy(child.gameObject);
        }

        // Get current rates
        Dictionary<Rarity, float> rates = SummoningTower.Instance.GetCurrentRates();

        // Create rate displays
        foreach (var rate in rates)
        {
            GameObject rateObj = Instantiate(rateDisplayPrefab, ratesContainer);
            TextMeshProUGUI rateText = rateObj.GetComponent<TextMeshProUGUI>();
            rateText.text = $"{rate.Key}: {rate.Value:P2}";
            rateText.color = rarityColors[rate.Key];
        }
    }

    private void OnSummonClick()
    {
        int cost = 100; // Base cost for single summon
        if (GameManager.Instance.HasEnoughResources(cost))
        {
            GameManager.Instance.SpendResources(cost);
            StartCoroutine(PerformSummon());
        }
        else
        {
            ShowInsufficientResourcesMessage();
        }
    }

    private void OnMultiSummonClick()
    {
        int cost = 1000; // Base cost for multi-summon (10+1)
        if (GameManager.Instance.HasEnoughResources(cost))
        {
            GameManager.Instance.SpendResources(cost);
            StartCoroutine(PerformMultiSummon());
        }
        else
        {
            ShowInsufficientResourcesMessage();
        }
    }

    private IEnumerator PerformSummon()
    {
        summonButton.interactable = false;
        multiSummonButton.interactable = false;

        // Play summoning animation
        if (summonEffect != null)
        {
            summonEffect.Play();
        }

        yield return new WaitForSeconds(1f);

        // Get summoned animal
        AnimalData summonedAnimal = SummoningTower.Instance.Summon(100);

        // Show result
        ShowSummonResult(summonedAnimal);

        // Update UI
        UpdatePitySlider();
        UpdateRatesDisplay();

        summonButton.interactable = true;
        multiSummonButton.interactable = true;
    }

    private IEnumerator PerformMultiSummon()
    {
        summonButton.interactable = false;
        multiSummonButton.interactable = false;
        skipAnimationButton.gameObject.SetActive(true);

        List<AnimalData> summonedAnimals = new List<AnimalData>();
        for (int i = 0; i < 11; i++) // 10+1 summon
        {
            summonedAnimals.Add(SummoningTower.Instance.Summon(100));
        }

        // Sort by rarity for dramatic effect
        summonedAnimals.Sort((a, b) => ((int)b.rarity).CompareTo((int)a.rarity));

        foreach (var animal in summonedAnimals)
        {
            yield return StartCoroutine(ShowMultiSummonCard(animal));
        }

        skipAnimationButton.gameObject.SetActive(false);
        UpdatePitySlider();
        UpdateRatesDisplay();

        summonButton.interactable = true;
        multiSummonButton.interactable = true;
    }

    private IEnumerator ShowMultiSummonCard(AnimalData animal)
    {
        GameObject card = Instantiate(multiSummonCardPrefab, multiSummonContainer);
        
        // Set up card visuals
        Image cardImage = card.GetComponentInChildren<Image>();
        TextMeshProUGUI cardName = card.GetComponentInChildren<TextMeshProUGUI>();
        
        cardImage.sprite = animal.icon;
        cardName.text = animal.animalName;
        cardName.color = rarityColors[animal.rarity];

        // Animate card reveal
        float elapsed = 0f;
        Vector3 startRotation = card.transform.rotation.eulerAngles;
        Vector3 endRotation = startRotation + new Vector3(0, 180, 0);

        while (elapsed < cardFlipDuration)
        {
            elapsed += Time.deltaTime;
            float t = flipCurve.Evaluate(elapsed / cardFlipDuration);
            card.transform.rotation = Quaternion.Euler(Vector3.Lerp(startRotation, endRotation, t));
            yield return null;
        }

        // Play rarity effect
        if (animal.rarity >= Rarity.SSR)
        {
            PlayRarityEffect(animal.rarity);
        }

        yield return new WaitForSeconds(0.5f);
    }

    private void ShowSummonResult(AnimalData animal)
    {
        resultPanel.SetActive(true);
        animalImage.sprite = animal.icon;
        animalNameText.text = animal.animalName;
        rarityText.text = animal.rarity.ToString();
        rarityText.color = rarityColors[animal.rarity];

        // Display stats
        statsText.text = $"HP: {animal.baseHealth}\n" +
                        $"Attack: {animal.baseAttack}\n" +
                        $"Defense: {animal.baseDefense}\n" +
                        $"Move Range: {animal.moveRange}";

        // Display skills
        SkillData[] skills = AnimalSkillsDatabase.Instance.GetAnimalSkills(animal.type);
        string skillText = "Skills:\n";
        foreach (var skill in skills)
        {
            skillText += $"• {skill.skillName}: {skill.description}\n";
        }
        skillsText.text = skillText;

        // Play rarity effect
        PlayRarityEffect(animal.rarity);
    }

    private void PlayRarityEffect(Rarity rarity)
    {
        if (rarityEffect != null)
        {
            var main = rarityEffect.main;
            main.startColor = rarityColors[rarity];
            rarityEffect.Play();

            // Play appropriate sound
            string soundName = $"Summon_{rarity}";
            AudioManager.Instance.PlaySound(soundName);
        }
    }

    private void UpdatePitySlider()
    {
        float pityProgress = SummoningTower.Instance.GetCurrentPityProgress();
        pitySlider.value = pityProgress;
        UpdatePityText();
    }

    private void UpdatePityText()
    {
        int remaining = Mathf.CeilToInt((1 - pitySlider.value) * 50); // Assuming pity at 50 summons
        pityText.text = $"Guaranteed SSR in {remaining} summons";
    }

    private void OnConfirmClick()
    {
        resultPanel.SetActive(false);
    }

    private void SkipAnimation()
    {
        StopAllCoroutines();
        // Clear all cards
        foreach (Transform child in multiSummonContainer)
        {
            Destroy(child.gameObject);
        }
        skipAnimationButton.gameObject.SetActive(false);
        summonButton.interactable = true;
        multiSummonButton.interactable = true;
    }

    private void ShowInsufficientResourcesMessage()
    {
        UIManager.Instance.ShowNotification("Insufficient resources for summoning!");
    }

    public void Show()
    {
        summoningPanel.SetActive(true);
        UpdateRatesDisplay();
        UpdatePitySlider();
    }

    public void Hide()
    {
        summoningPanel.SetActive(false);
        resultPanel.SetActive(false);
    }
}
