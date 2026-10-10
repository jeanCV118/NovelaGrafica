using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NovelUI : MonoBehaviour
{
    [Header("Controlador")]
    [SerializeField] private NovelController controller;

    [Header("Elementos Visuales")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image portraitImage;

    [Header("Caja de Diálogo")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button advanceButton;

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

    private void Awake()
    {
        if (controller == null)
        {
            controller = FindFirstObjectByType<NovelController>();
        }

        // Configurar botones de avance y reinicio
        if (advanceButton != null)
        {
            advanceButton.onClick.AddListener(OnAdvanceClicked);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
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

    private void OnAdvanceClicked()
    {
        if (controller != null)
        {
            controller.Advance();
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

        // Nombre del personaje
        if (speakerNameText != null)
        {
            if (line.speaker != null)
            {
                speakerNameText.text = line.speaker.displayName;
                speakerNameText.color = line.speaker.nameColor;
                speakerNameText.gameObject.SetActive(true);
            }
            else
            {
                speakerNameText.text = "";
                speakerNameText.gameObject.SetActive(false);
            }
        }

        // Texto de diálogo
        if (dialogueText != null)
        {
            dialogueText.text = line.text;
        }
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
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }

        if (endingPanel != null)
        {
            endingPanel.SetActive(true);

            if (endingTitleText != null)
            {
                endingTitleText.text = endingType == EndingType.Bueno ? "FINAL BUENO" : "FINAL ALTERNATIVO";
                endingTitleText.color = endingType == EndingType.Bueno ? Color.green : Color.red;
            }

            if (endingDescriptionText != null)
            {
                endingDescriptionText.text = endingType == EndingType.Bueno
                    ? "¡La prevención y el ritual salvaron a Cali! Buziraco ha sido sellado."
                    : "El pánico y la imprudencia provocaron la caída de la ciudad ante Buziraco.";
            }
        }
    }

    private void HandleStoryRestarted()
    {
        if (endingPanel != null)
        {
            endingPanel.SetActive(false);
        }

        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
    }
}
