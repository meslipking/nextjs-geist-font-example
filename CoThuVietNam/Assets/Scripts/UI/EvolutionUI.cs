using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class EvolutionUI : MonoBehaviour
{
    [Header("Main Panel")]
    [SerializeField] private GameObject evolutionPanel;
    [SerializeField] private Transform animalDisplayArea;
    [SerializeField] private Button evolveButton;
    [SerializeField] private Button closeButton;

    [Header("Animal Display")]
    [SerializeField] private Image currentAnimalImage;
    [SerializeField] private Image evolvedAnimalImage;
    [SerializeField] private TextMeshProUGUI currentStatsText;
    [SerializeField] private TextMeshProUGUI evolvedStatsText;
    [SerializeField] private TextMeshProUGUI currentRarityText;
    [SerializeField] private TextMeshProUGUI evolvedRarityText;

    [Header("Requirements")]
    [SerializeField] private TextMeshProUGUI levelRequirementText;
    [SerializeField] private TextMeshProUGUI goldRequirementText;
    [SerializeField] private TextMeshProUGUI stoneRequirementText;
    [SerializeField] private TextMeshProUGUI duplicateRequirementText;
    [SerializeField] private Image requirementProgressBar;

    [Header("Evolution Effects")]
    [SerializeField] private ParticleSystem evolutionParticles;
    [SerializeField] private GameObject evolutionLightRays;
    [SerializeField] private Image evolutionFlash;
    [SerializeField] private float evolutionDuration = 3f;
    [SerializeField] private AnimationCurve evolutionCurve;

    [Header("Preview")]
    [SerializeField] private GameObject previewPanel;
    [SerializeField] private TextMeshProUGUI previewStatsText;
    [SerializeField] private TextMeshProUGUI previewSkillsText;
    [SerializeField] private Button previewToggle;

    private Animal selectedAnimal;
    private bool isEvolutionInProgress;
    private Vector3 originalAnimalScale;
    private bool isPreviewVisible;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        evolveButton.onClick.AddListener(OnEvolveButtonClicked);
        closeButton.onClick.AddListener(Hide);
        previewToggle.onClick.AddListener(TogglePreview);

        evolutionPanel.SetActive(false);
        previewPanel.SetActive(false);
        evolutionLightRays.SetActive(false);
        evolutionFlash.gameObject.SetActive(false);
    }

    public void Show(Animal animal)
    {
        selectedAnimal = animal;
        evolutionPanel.SetActive(true);
        UpdateUI();
    }

    public void Hide()
    {
        evolutionPanel.SetActive(false);
        previewPanel.SetActive(false);
        selectedAnimal = null;
    }

    private void UpdateUI()
    {
        if (selectedAnimal == null) return;

        // Update current animal display
        currentAnimalImage.sprite = selectedAnimal.GetComponent<SpriteRenderer>().sprite;
        currentRarityText.text = selectedAnimal.CurrentRarity.ToString();
        currentRarityText.color = GetRarityColor(selectedAnimal.CurrentRarity);
        UpdateCurrentStats();

        // Update evolved animal preview
        Rarity nextRarity = GetNextRarity(selectedAnimal.CurrentRarity);
        evolvedRarityText.text = nextRarity.ToString();
        evolvedRarityText.color = GetRarityColor(nextRarity);
        UpdateEvolvedStats(nextRarity);

        // Update requirements
        UpdateRequirements();

        // Update button state
        evolveButton.interactable = AnimalEvolution.Instance.CanEvolve(selectedAnimal);
    }

    private void UpdateCurrentStats()
    {
        currentStatsText.text = $"Level: {selectedAnimal.Level}\n" +
                               $"HP: {selectedAnimal.Stats.health}\n" +
                               $"Attack: {selectedAnimal.Stats.attack}\n" +
                               $"Defense: {selectedAnimal.Stats.defense}\n" +
                               $"Crit Rate: {selectedAnimal.Stats.criticalChance:P0}\n" +
                               $"Crit Damage: {selectedAnimal.Stats.criticalDamage:P0}";
    }

    private void UpdateEvolvedStats(Rarity nextRarity)
    {
        StatBonus bonus = AnimalEvolution.Instance.GetEvolutionBonuses(nextRarity);
        
        evolvedStatsText.text = $"Level: {selectedAnimal.Level}\n" +
                               $"HP: {selectedAnimal.Stats.health * bonus.healthMultiplier}\n" +
                               $"Attack: {selectedAnimal.Stats.attack * bonus.attackMultiplier}\n" +
                               $"Defense: {selectedAnimal.Stats.defense * bonus.defenseMultiplier}\n" +
                               $"Crit Rate: {selectedAnimal.Stats.criticalChance + bonus.criticalChanceBonus:P0}\n" +
                               $"Crit Damage: {selectedAnimal.Stats.criticalDamage + bonus.criticalDamageBonus:P0}";
    }

    private void UpdateRequirements()
    {
        var req = AnimalEvolution.Instance.GetEvolutionRequirements(selectedAnimal.CurrentRarity);
        if (req == null) return;

        // Level requirement
        levelRequirementText.text = $"Level {req.requiredLevel}";
        levelRequirementText.color = selectedAnimal.Level >= req.requiredLevel ? Color.green : Color.red;

        // Gold requirement
        goldRequirementText.text = $"{req.goldCost:N0} Gold";
        goldRequirementText.color = ResourceManager.Instance.GetGold() >= req.goldCost ? Color.green : Color.red;

        // Evolution stone requirement
        stoneRequirementText.text = $"{req.stoneCost} Stones";
        stoneRequirementText.color = ResourceManager.Instance.GetEvolutionStones() >= req.stoneCost ? Color.green : Color.red;

        // Duplicate requirement
        int currentDuplicates = GameManager.Instance.GetDuplicateCount(selectedAnimal.type);
        duplicateRequirementText.text = $"{currentDuplicates}/{req.duplicatesNeeded} Duplicates";
        duplicateRequirementText.color = currentDuplicates >= req.duplicatesNeeded ? Color.green : Color.red;

        // Update progress bar
        float progress = Mathf.Min(1f, (float)currentDuplicates / req.duplicatesNeeded);
        requirementProgressBar.fillAmount = progress;
    }

    private void OnEvolveButtonClicked()
    {
        if (isEvolutionInProgress || selectedAnimal == null) return;

        if (AnimalEvolution.Instance.CanEvolve(selectedAnimal))
        {
            StartCoroutine(PlayEvolutionSequence());
        }
        else
        {
            UIManager.Instance.ShowNotification("Evolution requirements not met!");
        }
    }

    private IEnumerator PlayEvolutionSequence()
    {
        isEvolutionInProgress = true;
        evolveButton.interactable = false;
        closeButton.interactable = false;

        // Store original scale
        originalAnimalScale = currentAnimalImage.transform.localScale;

        // Start evolution effects
        evolutionParticles.Play();
        evolutionLightRays.SetActive(true);
        
        // Animate the animal
        Sequence evolutionSequence = DOTween.Sequence();
        evolutionSequence.Append(currentAnimalImage.transform.DOScale(originalAnimalScale * 1.2f, evolutionDuration * 0.3f));
        evolutionSequence.Join(currentAnimalImage.transform.DORotate(new Vector3(0, 0, 360), evolutionDuration, RotateMode.FastBeyond360));
        
        yield return evolutionSequence.WaitForCompletion();

        // Flash effect
        evolutionFlash.gameObject.SetActive(true);
        evolutionFlash.DOFade(1f, 0.2f).OnComplete(() => {
            evolutionFlash.DOFade(0f, 0.5f);
        });

        // Perform evolution
        if (AnimalEvolution.Instance.EvolveAnimal(selectedAnimal))
        {
            // Update UI with new stats
            UpdateUI();
            
            // Play success effects
            AudioManager.Instance.PlaySound("EvolutionSuccess");
            VFXManager.Instance.PlayEffect("EvolutionComplete", selectedAnimal.transform.position);
        }

        // Reset effects
        evolutionParticles.Stop();
        evolutionLightRays.SetActive(false);
        currentAnimalImage.transform.localScale = originalAnimalScale;
        currentAnimalImage.transform.rotation = Quaternion.identity;

        isEvolutionInProgress = false;
        evolveButton.interactable = true;
        closeButton.interactable = true;
    }

    private void TogglePreview()
    {
        isPreviewVisible = !isPreviewVisible;
        previewPanel.SetActive(isPreviewVisible);

        if (isPreviewVisible)
        {
            UpdatePreviewPanel();
        }
    }

    private void UpdatePreviewPanel()
    {
        if (selectedAnimal == null) return;

        Rarity nextRarity = GetNextRarity(selectedAnimal.CurrentRarity);
        StatBonus bonus = AnimalEvolution.Instance.GetEvolutionBonuses(nextRarity);

        // Show detailed stat changes
        string previewStats = "Evolution Changes:\n\n";
        previewStats += $"HP: {selectedAnimal.Stats.health} → {selectedAnimal.Stats.health * bonus.healthMultiplier:N0} (+{(bonus.healthMultiplier - 1):P0})\n";
        previewStats += $"Attack: {selectedAnimal.Stats.attack} → {selectedAnimal.Stats.attack * bonus.attackMultiplier:N0} (+{(bonus.attackMultiplier - 1):P0})\n";
        previewStats += $"Defense: {selectedAnimal.Stats.defense} → {selectedAnimal.Stats.defense * bonus.defenseMultiplier:N0} (+{(bonus.defenseMultiplier - 1):P0})\n";
        previewStats += $"Crit Rate: {selectedAnimal.Stats.criticalChance:P0} → {(selectedAnimal.Stats.criticalChance + bonus.criticalChanceBonus):P0} (+{bonus.criticalChanceBonus:P0})\n";
        previewStats += $"Crit Damage: {selectedAnimal.Stats.criticalDamage:P0} → {(selectedAnimal.Stats.criticalDamage + bonus.criticalDamageBonus):P0} (+{bonus.criticalDamageBonus:P0})";

        previewStatsText.text = previewStats;

        // Show skill changes
        string skillPreview = "Skill Changes:\n\n";
        SkillData[] newSkills = AnimalSkillsDatabase.Instance.GetAnimalSkills(selectedAnimal.type);
        foreach (var skill in newSkills)
        {
            if (skill.requiredRarity == nextRarity)
            {
                skillPreview += $"New Skill Unlocked: {skill.skillName}\n{skill.description}\n\n";
            }
            else
            {
                skillPreview += $"Upgrade: {skill.skillName}\nDamage +50%\nCooldown -10%\n\n";
            }
        }

        previewSkillsText.text = skillPreview;
    }

    private Rarity GetNextRarity(Rarity currentRarity)
    {
        return currentRarity switch
        {
            Rarity.N => Rarity.R,
            Rarity.R => Rarity.SR,
            Rarity.SR => Rarity.SSR,
            Rarity.SSR => Rarity.SSS,
            Rarity.SSS => Rarity.SSSPlus,
            _ => currentRarity
        };
    }

    private Color GetRarityColor(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.N => Color.white,
            Rarity.R => Color.blue,
            Rarity.SR => Color.magenta,
            Rarity.SSR => Color.yellow,
            Rarity.SSS => Color.red,
            Rarity.SSSPlus => new Color(1f, 0f, 1f),
            _ => Color.white
        };
    }
}
