using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Controladores")]
    [SerializeField] private NovelController controller;
    [SerializeField] private NovelUI novelUI;

    [Header("Paneles Principales")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject galleryPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Botones del Menú Principal")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button galleryButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;

    [Header("Galería de Finales")]
    [SerializeField] private TextMeshProUGUI galleryProgressText;
    [SerializeField] private Button galleryBackButton;
    [SerializeField] private GameObject endingCard1; // Final Bueno
    [SerializeField] private Image card1Image;
    [SerializeField] private TextMeshProUGUI card1TitleText;
    [SerializeField] private GameObject card1LockOverlay;
    [SerializeField] private Button card1Button;

    [SerializeField] private GameObject endingCard2; // Final Alternativo
    [SerializeField] private Image card2Image;
    [SerializeField] private TextMeshProUGUI card2TitleText;
    [SerializeField] private GameObject card2LockOverlay;
    [SerializeField] private Button card2Button;

    [Header("Modal Detalle de Final")]
    [SerializeField] private GameObject detailModal;
    [SerializeField] private Image detailImage;
    [SerializeField] private TextMeshProUGUI detailTitleText;
    [SerializeField] private TextMeshProUGUI detailDescriptionText;
    [SerializeField] private Button detailCloseButton;

    [Header("Configuración")]
    [SerializeField] private Button settingsBackButton;

    private void Awake()
    {
        if (controller == null) controller = FindFirstObjectByType<NovelController>();
        if (novelUI == null) novelUI = FindFirstObjectByType<NovelUI>();

        // Asignar listeners del Menú Principal
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGameClicked);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
        if (galleryButton != null) galleryButton.onClick.AddListener(OnGalleryClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);

        // Galería
        if (galleryBackButton != null) galleryBackButton.onClick.AddListener(OnGalleryBackClicked);
        if (card1Button != null) card1Button.onClick.AddListener(() => OnCardSelected(EndingsManager.BuenoID));
        if (card2Button != null) card2Button.onClick.AddListener(() => OnCardSelected(EndingsManager.AlternativoID));
        if (detailCloseButton != null) detailCloseButton.onClick.AddListener(() => { if (detailModal != null) detailModal.SetActive(false); });

        // Configuración
        if (settingsBackButton != null) settingsBackButton.onClick.AddListener(OnSettingsBackClicked);
    }

    private void Start()
    {
        // Al arrancar, mostrar siempre el Menú Principal
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (galleryPanel != null) galleryPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (detailModal != null) detailModal.SetActive(false);

        // Ocultar UI de juego mientras estamos en el menú
        if (novelUI != null) novelUI.gameObject.SetActive(false);

        // Habilitar Continuar sólo si existe partida guardada
        if (continueButton != null)
        {
            continueButton.interactable = SaveSystem.HasSaveData();
        }
    }

    private void OnNewGameClicked()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (novelUI != null) novelUI.gameObject.SetActive(true);

        if (controller != null)
        {
            controller.RestartStory();
        }
    }

    private void OnContinueClicked()
    {
        if (!SaveSystem.HasSaveData()) return;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (novelUI != null) novelUI.gameObject.SetActive(true);

        if (controller != null)
        {
            controller.ContinueStory();
        }
    }

    private void OnGalleryClicked()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (galleryPanel != null) galleryPanel.SetActive(true);
        RefreshGalleryUI();
    }

    private void OnGalleryBackClicked()
    {
        if (galleryPanel != null) galleryPanel.SetActive(false);
        if (detailModal != null) detailModal.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    private void OnSettingsClicked()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    private void OnSettingsBackClicked()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    private void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void RefreshGalleryUI()
    {
        var manager = EndingsManager.Instance;
        if (manager == null) return;

        int unlocked = manager.GetUnlockedCount();
        int total = manager.GetTotalCount();

        if (galleryProgressText != null)
        {
            galleryProgressText.text = $"Progreso de Finales: {unlocked} de {total} Descubiertos";
        }

        // Final Bueno
        bool isBuenoUnlocked = manager.IsEndingUnlocked(EndingsManager.BuenoID);
        var dataBueno = manager.GetEnding(EndingsManager.BuenoID);
        if (card1LockOverlay != null) card1LockOverlay.SetActive(!isBuenoUnlocked);
        if (card1TitleText != null) card1TitleText.text = isBuenoUnlocked ? (dataBueno != null ? dataBueno.title : "Final Bueno") : "??? (Bloqueado)";
        if (card1Image != null && dataBueno != null && dataBueno.cardImage != null)
        {
            card1Image.sprite = dataBueno.cardImage;
            card1Image.color = isBuenoUnlocked ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);
        }

        // Final Alternativo
        bool isAltUnlocked = manager.IsEndingUnlocked(EndingsManager.AlternativoID);
        var dataAlt = manager.GetEnding(EndingsManager.AlternativoID);
        if (card2LockOverlay != null) card2LockOverlay.SetActive(!isAltUnlocked);
        if (card2TitleText != null) card2TitleText.text = isAltUnlocked ? (dataAlt != null ? dataAlt.title : "Final Alternativo") : "??? (Bloqueado)";
        if (card2Image != null && dataAlt != null && dataAlt.cardImage != null)
        {
            card2Image.sprite = dataAlt.cardImage;
            card2Image.color = isAltUnlocked ? Color.white : new Color(0.2f, 0.2f, 0.2f, 1f);
        }
    }

    private void OnCardSelected(string endingID)
    {
        var manager = EndingsManager.Instance;
        if (manager == null || !manager.IsEndingUnlocked(endingID))
        {
            return; // Bloqueado, no se puede inspeccionar
        }

        var endingData = manager.GetEnding(endingID);
        if (endingData == null) return;

        if (detailModal != null)
        {
            detailModal.SetActive(true);
            if (detailTitleText != null) detailTitleText.text = endingData.title;
            if (detailDescriptionText != null) detailDescriptionText.text = endingData.description;
            if (detailImage != null && endingData.cardImage != null)
            {
                detailImage.sprite = endingData.cardImage;
            }
        }
    }
}
