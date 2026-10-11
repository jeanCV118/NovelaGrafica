using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NovelUI : MonoBehaviour
{
    [Header("Controladores")]
    [SerializeField] private NovelController controller;
    [SerializeField] private MainMenuUI mainMenuUI;

    [Header("Elementos Visuales")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image portraitImage;

    [Header("Caja de Diálogo")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private GameObject speakerNameBox;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button advanceButton;

    [Header("Botones de Menú y Control")]
    [SerializeField] private Button menuButton;

    [Header("Decisiones (2 Botones)")]
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private Button choiceButton1;
    [SerializeField] private TextMeshProUGUI choiceText1;
    [SerializeField] private Button choiceButton2;
    [SerializeField] private TextMeshProUGUI choiceText2;

    [Header("Pantalla Final")]
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private TextMeshProUGUI endingTitleText;
    [SerializeField] private TextMeshProUGUI endingDescriptionText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button endingMenuButton;

    // Typewriter
    private Coroutine typewriterCoroutine;
    private string fullCurrentText = "";
    private bool isTyping = false;

    private void Awake()
    {
        if (controller == null)
        {
            controller = FindFirstObjectByType<NovelController>();
        }

        if (mainMenuUI == null)
        {
            mainMenuUI = FindFirstObjectByType<MainMenuUI>();
        }

        // Configurar botones
        if (advanceButton != null)
        {
            advanceButton.onClick.AddListener(OnAdvanceClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuButtonClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (endingMenuButton != null)
        {
            endingMenuButton.onClick.AddListener(OnMenuButtonClicked);
        }

        // Configurar botones de decisión
        if (choiceButton1 != null)
        {
            choiceButton1.onClick.AddListener(() => OnChoiceSelected(0));
        }

        if (choiceButton2 != null)
        {
            choiceButton2.onClick.AddListener(() => OnChoiceSelected(1));
        }
    }

    private void Update()
    {
        // Controles de teclado convenientes para novela visual
        if (dialoguePanel != null && dialoguePanel.activeInHierarchy && (!choicesPanel || !choicesPanel.activeInHierarchy))
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                OnAdvanceClicked();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnMenuButtonClicked();
        }
    }

    private void OnEnable()
    {
        if (controller == null) return;

        controller.OnLocationChanged += HandleLocationChanged;
        controller.OnDialogueLineDisplayed += HandleDialogueLineDisplayed;
        controller.OnChoicesPresented += HandleChoicesPresented;
        controller.OnEndingReached += HandleEndingReached;
        controller.OnStoryRestarted += HandleStoryRestarted;
    }

    private void OnDisable()
    {
        if (controller == null) return;

        controller.OnLocationChanged -= HandleLocationChanged;
        controller.OnDialogueLineDisplayed -= HandleDialogueLineDisplayed;
        controller.OnChoicesPresented -= HandleChoicesPresented;
        controller.OnEndingReached -= HandleEndingReached;
        controller.OnStoryRestarted -= HandleStoryRestarted;
    }

    public void OnAdvanceClicked()
    {
        // Si el texto se está escribiendo, terminar el efecto Typewriter de inmediato
        if (isTyping)
        {
            FinishTypewriterImmediately();
            return;
        }

        if (controller != null)
        {
            controller.Advance();
        }
    }

    private void OnMenuButtonClicked()
    {
        if (mainMenuUI != null)
        {
            mainMenuUI.ShowMainMenu();
        }
    }

    private void OnChoiceSelected(int index)
    {
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }

        if (controller != null)
        {
            controller.SelectChoice(index);
        }
    }

    private void OnRestartClicked()
    {
        if (endingPanel != null)
        {
            endingPanel.SetActive(false);
        }

        if (controller != null)
        {
            controller.RestartStory();
        }
    }

    private void HandleLocationChanged(LocationData location)
    {
        if (backgroundImage != null && location != null)
        {
            backgroundImage.sprite = location.backgroundSprite;
            backgroundImage.gameObject.SetActive(location.backgroundSprite != null);
        }
    }

    private void HandleDialogueLineDisplayed(DialogueLine line)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }

        if (endingPanel != null)
        {
            endingPanel.SetActive(false);
        }

        // Mostrar u ocultar retrato
        Sprite portrait = line.customPortrait;
        if (portrait == null && line.speaker != null)
        {
            portrait = line.speaker.defaultPortrait;
        }

        if (portraitImage != null)
        {
            if (portrait != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.gameObject.SetActive(true);
            }
            else
            {
                portraitImage.gameObject.SetActive(false);
            }
        }

        // Nombre del personaje vs Narración
        if (line.speaker != null)
        {
            if (speakerNameBox != null) speakerNameBox.SetActive(true);
            if (speakerNameText != null)
            {
                speakerNameText.text = line.speaker.displayName;
                speakerNameText.color = line.speaker.nameColor;
                speakerNameText.gameObject.SetActive(true);
            }
            if (dialogueText != null)
            {
                dialogueText.fontStyle = FontStyles.Normal;
            }
        }
        else
        {
            if (speakerNameBox != null) speakerNameBox.SetActive(false);
            if (speakerNameText != null)
            {
                speakerNameText.text = "";
                speakerNameText.gameObject.SetActive(false);
            }
            if (dialogueText != null)
            {
                dialogueText.fontStyle = FontStyles.Italic;
            }
        }

        // Efecto Typewriter para el texto de diálogo
        fullCurrentText = line.text ?? "";
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        float speed = SettingsManager.Instance != null ? SettingsManager.Instance.TextSpeed : 0.025f;
        typewriterCoroutine = StartCoroutine(TypewriterRoutine(fullCurrentText, speed));
    }

    private IEnumerator TypewriterRoutine(string targetText, float delay)
    {
        isTyping = true;
        if (dialogueText != null) dialogueText.text = "";

        for (int i = 0; i <= targetText.Length; i++)
        {
            if (dialogueText != null)
            {
                dialogueText.text = targetText.Substring(0, i);
            }
            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
        typewriterCoroutine = null;
    }

    private void FinishTypewriterImmediately()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = fullCurrentText;
        }

        isTyping = false;
    }

    private void HandleChoicesPresented(IReadOnlyList<Choice> choices)
    {
        if (choicesPanel == null || choices == null || choices.Count < 2)
        {
            return;
        }

        // Opción 1
        if (choiceButton1 != null && choiceText1 != null && choices.Count > 0)
        {
            choiceText1.text = choices[0].choiceText;
            choiceButton1.gameObject.SetActive(true);
        }

        // Opción 2
        if (choiceButton2 != null && choiceText2 != null && choices.Count > 1)
        {
            choiceText2.text = choices[1].choiceText;
            choiceButton2.gameObject.SetActive(true);
        }

        choicesPanel.SetActive(true);
    }

    private void HandleEndingReached(EndingType endingType)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (choicesPanel != null) choicesPanel.SetActive(false);

        if (endingPanel != null)
        {
            endingPanel.SetActive(true);

            if (endingTitleText != null)
            {
                endingTitleText.text = endingType == EndingType.Bueno ? "FINAL BUENO: LA LUZ DE LAS TRES CRUCES" : "FINAL ALTERNATIVO: LA FALLA DESATADA";
                endingTitleText.color = endingType == EndingType.Bueno ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.3f, 0.3f);
            }

            if (endingDescriptionText != null)
            {
                endingDescriptionText.text = endingType == EndingType.Bueno
                    ? "¡La prevención, la calma y el ritual salvaron a Cali!\nBuziraco ha sido sellado para siempre bajo las Tres Cruces."
                    : "El pánico y la imprudencia provocaron la caída de la ciudad ante el poder telúrico de Buziraco.";
            }
        }
    }

    private void HandleStoryRestarted()
    {
        if (endingPanel != null) endingPanel.SetActive(false);
        if (choicesPanel != null) choicesPanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
    }
}
