using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [System.Serializable]
    public class TutorialStep
    {
        public string stepId;
        public string title;
        [TextArea(3, 10)]
        public string description;
        public Vector3 highlightPosition;
        public Vector2 highlightSize;
        public bool requiresInput;
        public string nextStepId;
        public AudioClip voiceover;
    }

    [Header("UI References")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Image highlightMask;
    [SerializeField] private GameObject pointer;

    [Header("Tutorial Data")]
    [SerializeField] private TutorialStep[] tutorialSteps;
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool showTutorialOnFirstLaunch = true;

    private Dictionary<string, TutorialStep> stepDictionary;
    private TutorialStep currentStep;
    private bool isTutorialActive;
    private Coroutine typingCoroutine;
    private const string TUTORIAL_COMPLETED_KEY = "TutorialCompleted";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        // Initialize step dictionary
        stepDictionary = new Dictionary<string, TutorialStep>();
        foreach (var step in tutorialSteps)
        {
            stepDictionary[step.stepId] = step;
        }

        // Set up button listeners
        nextButton.onClick.AddListener(OnNextButtonClicked);
        skipButton.onClick.AddListener(SkipTutorial);

        // Check if tutorial should be shown
        if (showTutorialOnFirstLaunch && !PlayerPrefs.HasKey(TUTORIAL_COMPLETED_KEY))
        {
            StartTutorial();
        }
    }

    public void StartTutorial()
    {
        if (tutorialSteps.Length == 0)
        {
            Debug.LogWarning("No tutorial steps defined!");
            return;
        }

        isTutorialActive = true;
        tutorialPanel.SetActive(true);
        ShowStep(tutorialSteps[0].stepId);

        // Disable normal game input
        InputManager.Instance.SetEnabled(false);
    }

    private void ShowStep(string stepId)
    {
        if (!stepDictionary.TryGetValue(stepId, out TutorialStep step))
        {
            Debug.LogError($"Tutorial step {stepId} not found!");
            return;
        }

        currentStep = step;

        // Update UI
        titleText.text = step.title;
        
        // Stop any existing typing animation
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // Start new typing animation
        typingCoroutine = StartCoroutine(TypeText(step.description));

        // Position highlight and pointer
        UpdateHighlight(step.highlightPosition, step.highlightSize);

        // Play voiceover if available
        if (step.voiceover != null)
        {
            AudioManager.Instance.PlaySound(step.voiceover.name);
        }

        // Update button state
        nextButton.gameObject.SetActive(!step.requiresInput);
    }

    private IEnumerator TypeText(string text)
    {
        descriptionText.text = "";
        foreach (char c in text)
        {
            descriptionText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private void UpdateHighlight(Vector3 position, Vector2 size)
    {
        // Convert world position to screen position
        Vector2 screenPos = Camera.main.WorldToScreenPoint(position);
        
        // Update highlight mask position and size
        highlightMask.rectTransform.position = screenPos;
        highlightMask.rectTransform.sizeDelta = size;

        // Update pointer position
        pointer.transform.position = screenPos + Vector2.up * (size.y / 2 + 50f); // Position above highlight
    }

    public void OnGameAction(string actionId)
    {
        if (!isTutorialActive || currentStep == null || !currentStep.requiresInput)
            return;

        // Check if this action completes the current step
        if (actionId == currentStep.stepId)
        {
            OnStepCompleted();
        }
    }

    private void OnStepCompleted()
    {
        if (string.IsNullOrEmpty(currentStep.nextStepId))
        {
            CompleteTutorial();
        }
        else
        {
            ShowStep(currentStep.nextStepId);
        }
    }

    private void OnNextButtonClicked()
    {
        if (currentStep != null && !string.IsNullOrEmpty(currentStep.nextStepId))
        {
            ShowStep(currentStep.nextStepId);
        }
        else
        {
            CompleteTutorial();
        }
    }

    public void SkipTutorial()
    {
        if (!isTutorialActive) return;

        // Show confirmation dialog
        UIManager.Instance.ShowConfirmationDialog(
            "Skip Tutorial",
            "Are you sure you want to skip the tutorial?",
            () => CompleteTutorial(),
            null
        );
    }

    private void CompleteTutorial()
    {
        isTutorialActive = false;
        tutorialPanel.SetActive(false);
        
        // Mark tutorial as completed
        PlayerPrefs.SetInt(TUTORIAL_COMPLETED_KEY, 1);
        PlayerPrefs.Save();

        // Re-enable normal game input
        InputManager.Instance.SetEnabled(true);

        // Notify game manager
        GameManager.Instance.OnTutorialCompleted();
    }

    public bool IsTutorialActive()
    {
        return isTutorialActive;
    }

    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(TUTORIAL_COMPLETED_KEY);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

// Example tutorial steps implementation
[System.Serializable]
public class TutorialSteps
{
    public static readonly TutorialStep[] Steps = new TutorialStep[]
    {
        new TutorialStep
        {
            stepId = "welcome",
            title = "Welcome to Cờ Thú Việt Nam!",
            description = "Let's learn how to play this exciting strategic game.",
            requiresInput = false,
            nextStepId = "select_animals"
        },
        new TutorialStep
        {
            stepId = "select_animals",
            title = "Selecting Your Animals",
            description = "First, choose your animals. Each animal has unique abilities and strengths.",
            highlightPosition = new Vector3(0, 0, 0), // Position will be set to character selection area
            highlightSize = new Vector2(300, 200),
            requiresInput = true,
            nextStepId = "movement"
        },
        new TutorialStep
        {
            stepId = "movement",
            title = "Moving Your Animals",
            description = "Click on an animal to select it, then click on a highlighted cell to move.",
            requiresInput = true,
            nextStepId = "combat"
        },
        new TutorialStep
        {
            stepId = "combat",
            title = "Combat",
            description = "Move your animal onto an enemy's position to attack. Different animals have advantages against others!",
            requiresInput = true,
            nextStepId = "special_abilities"
        },
        new TutorialStep
        {
            stepId = "special_abilities",
            title = "Special Abilities",
            description = "Each animal has unique abilities. Click the skill button to use them!",
            requiresInput = true,
            nextStepId = "water_tiles"
        },
        new TutorialStep
        {
            stepId = "water_tiles",
            title = "Water Tiles",
            description = "Be careful of water tiles! Only mice can swim, but tigers can jump over them.",
            requiresInput = false,
            nextStepId = "victory"
        },
        new TutorialStep
        {
            stepId = "victory",
            title = "Victory",
            description = "Defeat all enemy animals to win! Good luck!",
            requiresInput = false,
            nextStepId = ""
        }
    };
}
