#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SetupNovelScene
{
    private const string ScenePath = "Assets/Novela/Scenes/Novela.unity";
    private const string ArtPath = "Assets/Novela/Art/";
    private const string DataPath = "Assets/Novela/Data/";

    [MenuItem("Novela/Configurar Escena Completa")]
    public static void SetupScene()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);

        // Cargar assets
        var tmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        var menuBG = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "MenuPrincipal_BG.png");
        var finalBuenoCard = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "FinalBueno_Card.png");
        var finalAltCard = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "FinalAlternativo_Card.png");

        // Cargar los 8 nodos de historia en orden
        var allNodes = new List<StoryNode>
        {
            AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "01_Introduccion.asset"),
            AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "02_Decision1.asset"),
            AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "03_RutaPanico.asset"),
            AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "04_RutaPrevencion.asset"),
            AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "05_EnfrentamientoFinal.asset"),
            AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "06_Decision2.asset"),
            AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "07_FinalBueno.asset"),
            AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "08_FinalAlternativo.asset")
        };

        // 1. Obtener o crear Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGO = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Agregar ScreenShaker al Canvas
        var shaker = canvas.GetComponent<ScreenShaker>();
        if (shaker == null) shaker = canvas.gameObject.AddComponent<ScreenShaker>();

        // 2. Obtener o crear NovelManager
        GameObject managerGO = GameObject.Find("NovelManager");
        if (managerGO == null)
        {
            managerGO = new GameObject("NovelManager");
        }

        var controller = managerGO.GetComponent<NovelController>();
        if (controller == null) controller = managerGO.AddComponent<NovelController>();

        var novelUI = managerGO.GetComponent<NovelUI>();
        if (novelUI == null) novelUI = managerGO.AddComponent<NovelUI>();

        var endingsManager = managerGO.GetComponent<EndingsManager>();
        if (endingsManager == null) endingsManager = managerGO.AddComponent<EndingsManager>();

        var mainMenuUI = managerGO.GetComponent<MainMenuUI>();
        if (mainMenuUI == null) mainMenuUI = managerGO.AddComponent<MainMenuUI>();

        var settingsManager = managerGO.GetComponent<SettingsManager>();
        if (settingsManager == null) settingsManager = managerGO.AddComponent<SettingsManager>();

        // Fuentes de Audio
        AudioSource[] audioSources = managerGO.GetComponents<AudioSource>();
        AudioSource musicSrc = audioSources.Length > 0 ? audioSources[0] : managerGO.AddComponent<AudioSource>();
        AudioSource sfxSrc = audioSources.Length > 1 ? audioSources[1] : managerGO.AddComponent<AudioSource>();
        musicSrc.playOnAwake = false;
        sfxSrc.playOnAwake = false;

        // Configurar EndingsManager con las 2 tarjetas
        var serializedEndings = new SerializedObject(endingsManager);
        var endingsProp = serializedEndings.FindProperty("endings");
        endingsProp.ClearArray();
        endingsProp.arraySize = 2;

        var e0 = endingsProp.GetArrayElementAtIndex(0);
        e0.FindPropertyRelative("id").stringValue = "FINAL_BUENO";
        e0.FindPropertyRelative("title").stringValue = "Final Bueno: La Luz de las Tres Cruces";
        e0.FindPropertyRelative("description").stringValue = "Mateo y el Padre Esteban aplicaron los protocolos de prevención y activaron el ritual sagrado. Las Tres Cruces brillaron con luz divina, sellando a Buziraco en las profundidades y salvando a Cali.";
        e0.FindPropertyRelative("cardImage").objectReferenceValue = finalBuenoCard;

        var e1 = endingsProp.GetArrayElementAtIndex(1);
        e1.FindPropertyRelative("id").stringValue = "FINAL_ALTERNATIVO";
        e1.FindPropertyRelative("title").stringValue = "Final Alternativo: La Falla Desatada";
        e1.FindPropertyRelative("description").stringValue = "Cegado por la desesperación y el pánico, Mateo intentó enfrentar al demonio a golpes ignorando las medidas de seguridad. El ritual fracasó, la falla telúrica colapsó y Buziraco escapó dejando a Cali en ruinas.";
        e1.FindPropertyRelative("cardImage").objectReferenceValue = finalAltCard;

        serializedEndings.ApplyModifiedProperties();

        // 3. ESTRUCTURAR ELEMENTOS DENTRO DEL CANVAS
        Transform canvasT = canvas.transform;

        // A. Fondo de Novela (Background)
        RectTransform bgRT = GetOrCreateChild(canvasT, "Background");
        StretchFull(bgRT);
        var bgImage = GetOrAddComponent<Image>(bgRT.gameObject);
        bgImage.color = Color.white;

        // B. Retrato de Personaje (CharacterPortrait)
        RectTransform portraitRT = GetOrCreateChild(canvasT, "CharacterPortrait");
        portraitRT.anchorMin = new Vector2(0f, 0f);
        portraitRT.anchorMax = new Vector2(0f, 0f);
        portraitRT.pivot = new Vector2(0f, 0f);
        portraitRT.anchoredPosition = new Vector2(60, 0);
        portraitRT.sizeDelta = new Vector2(480, 720);
        var portraitImage = GetOrAddComponent<Image>(portraitRT.gameObject);
        portraitImage.preserveAspect = true;

        // C. Caja de Diálogo (DialoguePanel)
        RectTransform dialoguePanelRT = GetOrCreateChild(canvasT, "DialoguePanel");
        dialoguePanelRT.anchorMin = new Vector2(0.5f, 0f);
        dialoguePanelRT.anchorMax = new Vector2(0.5f, 0f);
        dialoguePanelRT.pivot = new Vector2(0.5f, 0f);
        dialoguePanelRT.anchoredPosition = new Vector2(0, 25);
        dialoguePanelRT.sizeDelta = new Vector2(1160, 210);
        var dialoguePanelImg = GetOrAddComponent<Image>(dialoguePanelRT.gameObject);
        dialoguePanelImg.color = new Color(0.08f, 0.08f, 0.12f, 0.90f);

        // Recuadro del nombre del personaje (SpeakerNameBox)
        RectTransform nameBoxRT = GetOrCreateChild(dialoguePanelRT, "SpeakerNameBox");
        nameBoxRT.anchorMin = new Vector2(0f, 1f);
        nameBoxRT.anchorMax = new Vector2(0f, 1f);
        nameBoxRT.pivot = new Vector2(0f, 0f);
        nameBoxRT.anchoredPosition = new Vector2(25, -5);
        nameBoxRT.sizeDelta = new Vector2(280, 48);
        var nameBoxImg = GetOrAddComponent<Image>(nameBoxRT.gameObject);
        nameBoxImg.color = new Color(0.16f, 0.14f, 0.22f, 0.96f);

        RectTransform nameTextRT = GetOrCreateChild(nameBoxRT, "SpeakerNameText");
        StretchWithPadding(nameTextRT, 10, 5, 10, 5);
        var speakerNameText = GetOrAddComponent<TextMeshProUGUI>(nameTextRT.gameObject);
        if (tmpFont != null) speakerNameText.font = tmpFont;
        speakerNameText.fontSize = 24;
        speakerNameText.fontStyle = FontStyles.Bold;
        speakerNameText.alignment = TextAlignmentOptions.MidlineLeft;
        speakerNameText.color = Color.white;

        // Texto del diálogo
        RectTransform diagTextRT = GetOrCreateChild(dialoguePanelRT, "DialogueText");
        diagTextRT.anchorMin = new Vector2(0f, 0f);
        diagTextRT.anchorMax = new Vector2(1f, 1f);
        diagTextRT.offsetMin = new Vector2(35, 25);
        diagTextRT.offsetMax = new Vector2(-45, -35);
        var dialogueText = GetOrAddComponent<TextMeshProUGUI>(diagTextRT.gameObject);
        if (tmpFont != null) dialogueText.font = tmpFont;
        dialogueText.fontSize = 21;
        dialogueText.lineSpacing = 15;
        dialogueText.alignment = TextAlignmentOptions.TopLeft;
        dialogueText.color = new Color(0.96f, 0.96f, 0.96f, 1f);

        // Botón transparente de avance sobre el panel
        RectTransform advBtnRT = GetOrCreateChild(dialoguePanelRT, "AdvanceButton");
        StretchFull(advBtnRT);
        var advanceBtn = GetOrAddComponent<Button>(advBtnRT.gameObject);
        var advImg = GetOrAddComponent<Image>(advBtnRT.gameObject);
        advImg.color = new Color(0, 0, 0, 0); // Totalmente invisible para recibir clics

        // Botón MENÚ / PAUSA en la esquina superior derecha
        RectTransform menuBtnRT = GetOrCreateChild(canvasT, "MenuButton");
        menuBtnRT.anchorMin = new Vector2(1f, 1f);
        menuBtnRT.anchorMax = new Vector2(1f, 1f);
        menuBtnRT.pivot = new Vector2(1f, 1f);
        menuBtnRT.anchoredPosition = new Vector2(-25, -25);
        menuBtnRT.sizeDelta = new Vector2(140, 42);
        var menuBtn = GetOrAddComponent<Button>(menuBtnRT.gameObject);
        var menuBtnImg = GetOrAddComponent<Image>(menuBtnRT.gameObject);
        menuBtnImg.color = new Color(0.18f, 0.15f, 0.25f, 0.85f);
        var menuBtnText = CreateTextChild(menuBtnRT, "Text", "MENÚ", tmpFont, 16, FontStyles.Bold, Color.white);
        menuBtnText.alignment = TextAlignmentOptions.Center;

        // D. Panel de Decisiones (ChoicesPanel)
        RectTransform choicesPanelRT = GetOrCreateChild(canvasT, "ChoicesPanel");
        StretchFull(choicesPanelRT);
        var choicesPanelImg = GetOrAddComponent<Image>(choicesPanelRT.gameObject);
        choicesPanelImg.color = new Color(0, 0, 0, 0.45f); // Fondo semitransparente atenuado
        choicesPanelRT.gameObject.SetActive(false);

        // Opción 1
        RectTransform choice1RT = GetOrCreateChild(choicesPanelRT, "ChoiceButton1");
        choice1RT.anchorMin = new Vector2(0.5f, 0.5f);
        choice1RT.anchorMax = new Vector2(0.5f, 0.5f);
        choice1RT.pivot = new Vector2(0.5f, 0.5f);
        choice1RT.anchoredPosition = new Vector2(0, 50);
        choice1RT.sizeDelta = new Vector2(850, 75);
        var choiceBtn1 = GetOrAddComponent<Button>(choice1RT.gameObject);
        var choice1Img = GetOrAddComponent<Image>(choice1RT.gameObject);
        choice1Img.color = new Color(0.18f, 0.22f, 0.35f, 0.95f);
        var choiceText1 = CreateTextChild(choice1RT, "Text", "Opción 1", tmpFont, 20, FontStyles.Bold, Color.white);
        choiceText1.alignment = TextAlignmentOptions.Center;

        // Opción 2
        RectTransform choice2RT = GetOrCreateChild(choicesPanelRT, "ChoiceButton2");
        choice2RT.anchorMin = new Vector2(0.5f, 0.5f);
        choice2RT.anchorMax = new Vector2(0.5f, 0.5f);
        choice2RT.pivot = new Vector2(0.5f, 0.5f);
        choice2RT.anchoredPosition = new Vector2(0, -50);
        choice2RT.sizeDelta = new Vector2(850, 75);
        var choiceBtn2 = GetOrAddComponent<Button>(choice2RT.gameObject);
        var choice2Img = GetOrAddComponent<Image>(choice2RT.gameObject);
        choice2Img.color = new Color(0.32f, 0.18f, 0.20f, 0.95f);
        var choiceText2 = CreateTextChild(choice2RT, "Text", "Opción 2", tmpFont, 20, FontStyles.Bold, Color.white);
        choiceText2.alignment = TextAlignmentOptions.Center;

        // E. Pantalla Final (EndingPanel)
        RectTransform endingPanelRT = GetOrCreateChild(canvasT, "EndingPanel");
        StretchFull(endingPanelRT);
        var endingPanelImg = GetOrAddComponent<Image>(endingPanelRT.gameObject);
        endingPanelImg.color = new Color(0.06f, 0.05f, 0.08f, 0.95f);
        endingPanelRT.gameObject.SetActive(false);

        var endTitle = CreateTextChild(endingPanelRT, "EndingTitleText", "FINAL DE LA HISTORIA", tmpFont, 32, FontStyles.Bold, Color.yellow);
        RectTransform endTitleRT = endTitle.rectTransform;
        endTitleRT.anchorMin = new Vector2(0.5f, 0.70f);
        endTitleRT.anchorMax = new Vector2(0.5f, 0.70f);
        endTitleRT.sizeDelta = new Vector2(900, 60);
        endTitle.alignment = TextAlignmentOptions.Center;

        var endDesc = CreateTextChild(endingPanelRT, "EndingDescriptionText", "Descripción del final...", tmpFont, 22, FontStyles.Normal, Color.white);
        RectTransform endDescRT = endDesc.rectTransform;
        endDescRT.anchorMin = new Vector2(0.5f, 0.50f);
        endDescRT.anchorMax = new Vector2(0.5f, 0.50f);
        endDescRT.sizeDelta = new Vector2(800, 140);
        endDesc.alignment = TextAlignmentOptions.Center;

        // Botón Reintentar
        RectTransform restartBtnRT = GetOrCreateChild(endingPanelRT, "RestartButton");
        restartBtnRT.anchorMin = new Vector2(0.35f, 0.25f);
        restartBtnRT.anchorMax = new Vector2(0.35f, 0.25f);
        restartBtnRT.sizeDelta = new Vector2(260, 55);
        var restartBtn = GetOrAddComponent<Button>(restartBtnRT.gameObject);
        var restartImg = GetOrAddComponent<Image>(restartBtnRT.gameObject);
        restartImg.color = new Color(0.20f, 0.45f, 0.25f, 0.95f);
        var restartText = CreateTextChild(restartBtnRT, "Text", "JUGAR DE NUEVO", tmpFont, 18, FontStyles.Bold, Color.white);
        restartText.alignment = TextAlignmentOptions.Center;

        // Botón Ir al Menú
        RectTransform endingMenuBtnRT = GetOrCreateChild(endingPanelRT, "EndingMenuButton");
        endingMenuBtnRT.anchorMin = new Vector2(0.65f, 0.25f);
        endingMenuBtnRT.anchorMax = new Vector2(0.65f, 0.25f);
        endingMenuBtnRT.sizeDelta = new Vector2(260, 55);
        var endingMenuBtn = GetOrAddComponent<Button>(endingMenuBtnRT.gameObject);
        var endingMenuImg = GetOrAddComponent<Image>(endingMenuBtnRT.gameObject);
        endingMenuImg.color = new Color(0.30f, 0.25f, 0.40f, 0.95f);
        var endMenuText = CreateTextChild(endingMenuBtnRT, "Text", "MENÚ PRINCIPAL", tmpFont, 18, FontStyles.Bold, Color.white);
        endMenuText.alignment = TextAlignmentOptions.Center;

        // F. MENÚ PRINCIPAL (MainMenuPanel)
        RectTransform mainMenuRT = GetOrCreateChild(canvasT, "MainMenuPanel");
        StretchFull(mainMenuRT);
        var menuBGImg = GetOrAddComponent<Image>(mainMenuRT.gameObject);
        if (menuBG != null) menuBGImg.sprite = menuBG;
        menuBGImg.color = Color.white;

        // Título del juego
        var titleText = CreateTextChild(mainMenuRT, "TitleText", "BUZIRACO", tmpFont, 56, FontStyles.Bold, new Color(1f, 0.85f, 0.35f));
        titleText.rectTransform.anchorMin = new Vector2(0.5f, 0.82f);
        titleText.rectTransform.anchorMax = new Vector2(0.5f, 0.82f);
        titleText.rectTransform.sizeDelta = new Vector2(900, 70);
        titleText.alignment = TextAlignmentOptions.Center;

        var subTitleText = CreateTextChild(mainMenuRT, "SubtitleText", "LA SOMBRA DE LAS TRES CRUCES", tmpFont, 24, FontStyles.Bold, Color.white);
        subTitleText.rectTransform.anchorMin = new Vector2(0.5f, 0.73f);
        subTitleText.rectTransform.anchorMax = new Vector2(0.5f, 0.73f);
        subTitleText.rectTransform.sizeDelta = new Vector2(900, 40);
        subTitleText.alignment = TextAlignmentOptions.Center;

        var sloganText = CreateTextChild(mainMenuRT, "SloganText", "Novela Visual de Supervivencia y Prevención Sísmica en Cali", tmpFont, 16, FontStyles.Italic, new Color(0.85f, 0.85f, 0.85f));
        sloganText.rectTransform.anchorMin = new Vector2(0.5f, 0.67f);
        sloganText.rectTransform.anchorMax = new Vector2(0.5f, 0.67f);
        sloganText.rectTransform.sizeDelta = new Vector2(900, 30);
        sloganText.alignment = TextAlignmentOptions.Center;

        // Contenedor de botones del menú principal
        Button newGameBtn = CreateMenuButton(mainMenuRT, "NewGameButton", "NUEVA PARTIDA", 0, new Vector2(0, -20), tmpFont);
        Button continueBtn = CreateMenuButton(mainMenuRT, "ContinueButton", "CONTINUAR", 1, new Vector2(0, -80), tmpFont);
        Button galleryBtn = CreateMenuButton(mainMenuRT, "GalleryButton", "GALERÍA DE FINALES", 2, new Vector2(0, -140), tmpFont);
        Button settingsBtn = CreateMenuButton(mainMenuRT, "SettingsButton", "CONFIGURACIÓN", 3, new Vector2(0, -200), tmpFont);
        Button exitBtn = CreateMenuButton(mainMenuRT, "ExitButton", "SALIR", 4, new Vector2(0, -260), tmpFont);

        // G. PANEL DE GALERÍA DE FINALES (GalleryPanel)
        RectTransform galleryPanelRT = GetOrCreateChild(mainMenuRT, "GalleryPanel");
        StretchFull(galleryPanelRT);
        var galleryBGImg = GetOrAddComponent<Image>(galleryPanelRT.gameObject);
        galleryBGImg.color = new Color(0.08f, 0.06f, 0.10f, 0.98f);
        galleryPanelRT.gameObject.SetActive(false);

        var galTitle = CreateTextChild(galleryPanelRT, "Title", "GALERÍA DE FINALES", tmpFont, 36, FontStyles.Bold, new Color(1f, 0.85f, 0.40f));
        galTitle.rectTransform.anchorMin = new Vector2(0.5f, 0.88f);
        galTitle.rectTransform.anchorMax = new Vector2(0.5f, 0.88f);
        galTitle.rectTransform.sizeDelta = new Vector2(800, 50);
        galTitle.alignment = TextAlignmentOptions.Center;

        var galProgress = CreateTextChild(galleryPanelRT, "ProgressText", "Progreso de Finales: 0 de 2 Descubiertos", tmpFont, 20, FontStyles.Normal, Color.white);
        galProgress.rectTransform.anchorMin = new Vector2(0.5f, 0.80f);
        galProgress.rectTransform.anchorMax = new Vector2(0.5f, 0.80f);
        galProgress.rectTransform.sizeDelta = new Vector2(800, 35);
        galProgress.alignment = TextAlignmentOptions.Center;

        // Tarjeta 1 (Final Bueno)
        var card1RT = GetOrCreateChild(galleryPanelRT, "Card1_Bueno");
        card1RT.anchorMin = new Vector2(0.30f, 0.48f);
        card1RT.anchorMax = new Vector2(0.30f, 0.48f);
        card1RT.sizeDelta = new Vector2(340, 220);
        var card1Btn = GetOrAddComponent<Button>(card1RT.gameObject);
        var card1Img = GetOrAddComponent<Image>(card1RT.gameObject);
        card1Img.sprite = finalBuenoCard;

        var card1Lock = GetOrCreateChild(card1RT, "LockOverlay");
        StretchFull(card1Lock);
        var card1LockImg = GetOrAddComponent<Image>(card1Lock.gameObject);
        card1LockImg.color = new Color(0, 0, 0, 0.75f);
        var card1LockText = CreateTextChild(card1Lock, "LockText", "🔒 BLOQUEADO", tmpFont, 22, FontStyles.Bold, new Color(0.8f, 0.8f, 0.8f));
        card1LockText.alignment = TextAlignmentOptions.Center;

        var card1Title = CreateTextChild(card1RT, "Title", "Final Bueno", tmpFont, 16, FontStyles.Bold, Color.white);
        card1Title.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        card1Title.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        card1Title.rectTransform.pivot = new Vector2(0.5f, 1f);
        card1Title.rectTransform.anchoredPosition = new Vector2(0, -10);
        card1Title.rectTransform.sizeDelta = new Vector2(340, 30);
        card1Title.alignment = TextAlignmentOptions.Center;

        // Tarjeta 2 (Final Alternativo)
        var card2RT = GetOrCreateChild(galleryPanelRT, "Card2_Alternativo");
        card2RT.anchorMin = new Vector2(0.70f, 0.48f);
        card2RT.anchorMax = new Vector2(0.70f, 0.48f);
        card2RT.sizeDelta = new Vector2(340, 220);
        var card2Btn = GetOrAddComponent<Button>(card2RT.gameObject);
        var card2Img = GetOrAddComponent<Image>(card2RT.gameObject);
        card2Img.sprite = finalAltCard;

        var card2Lock = GetOrCreateChild(card2RT, "LockOverlay");
        StretchFull(card2Lock);
        var card2LockImg = GetOrAddComponent<Image>(card2Lock.gameObject);
        card2LockImg.color = new Color(0, 0, 0, 0.75f);
        var card2LockText = CreateTextChild(card2Lock, "LockText", "🔒 BLOQUEADO", tmpFont, 22, FontStyles.Bold, new Color(0.8f, 0.8f, 0.8f));
        card2LockText.alignment = TextAlignmentOptions.Center;

        var card2Title = CreateTextChild(card2RT, "Title", "Final Alternativo", tmpFont, 16, FontStyles.Bold, Color.white);
        card2Title.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        card2Title.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        card2Title.rectTransform.pivot = new Vector2(0.5f, 1f);
        card2Title.rectTransform.anchoredPosition = new Vector2(0, -10);
        card2Title.rectTransform.sizeDelta = new Vector2(340, 30);
        card2Title.alignment = TextAlignmentOptions.Center;

        // Botón Volver de la Galería
        RectTransform galBackRT = GetOrCreateChild(galleryPanelRT, "GalleryBackButton");
        galBackRT.anchorMin = new Vector2(0.5f, 0.12f);
        galBackRT.anchorMax = new Vector2(0.5f, 0.12f);
        galBackRT.sizeDelta = new Vector2(240, 50);
        var galBackBtn = GetOrAddComponent<Button>(galBackRT.gameObject);
        var galBackImg = GetOrAddComponent<Image>(galBackRT.gameObject);
        galBackImg.color = new Color(0.25f, 0.20f, 0.32f, 0.95f);
        var galBackText = CreateTextChild(galBackRT, "Text", "← VOLVER AL MENÚ", tmpFont, 18, FontStyles.Bold, Color.white);
        galBackText.alignment = TextAlignmentOptions.Center;

        // Modal Detalle de Final (DetailModal)
        RectTransform detailModalRT = GetOrCreateChild(galleryPanelRT, "DetailModal");
        StretchFull(detailModalRT);
        var detailModalBG = GetOrAddComponent<Image>(detailModalRT.gameObject);
        detailModalBG.color = new Color(0, 0, 0, 0.85f);
        detailModalRT.gameObject.SetActive(false);

        RectTransform detailBoxRT = GetOrCreateChild(detailModalRT, "DetailBox");
        detailBoxRT.anchorMin = new Vector2(0.5f, 0.5f);
        detailBoxRT.anchorMax = new Vector2(0.5f, 0.5f);
        detailBoxRT.sizeDelta = new Vector2(720, 520);
        var detailBoxImg = GetOrAddComponent<Image>(detailBoxRT.gameObject);
        detailBoxImg.color = new Color(0.12f, 0.10f, 0.16f, 0.98f);

        RectTransform detailImgRT = GetOrCreateChild(detailBoxRT, "DetailImage");
        detailImgRT.anchorMin = new Vector2(0.5f, 0.70f);
        detailImgRT.anchorMax = new Vector2(0.5f, 0.70f);
        detailImgRT.sizeDelta = new Vector2(480, 220);
        var detailImg = GetOrAddComponent<Image>(detailImgRT.gameObject);

        var detailTitle = CreateTextChild(detailBoxRT, "DetailTitle", "Título del Final", tmpFont, 24, FontStyles.Bold, Color.yellow);
        detailTitle.rectTransform.anchorMin = new Vector2(0.5f, 0.40f);
        detailTitle.rectTransform.anchorMax = new Vector2(0.5f, 0.40f);
        detailTitle.rectTransform.sizeDelta = new Vector2(660, 40);
        detailTitle.alignment = TextAlignmentOptions.Center;

        var detailDesc = CreateTextChild(detailBoxRT, "DetailDesc", "Descripción...", tmpFont, 18, FontStyles.Normal, Color.white);
        detailDesc.rectTransform.anchorMin = new Vector2(0.5f, 0.23f);
        detailDesc.rectTransform.anchorMax = new Vector2(0.5f, 0.23f);
        detailDesc.rectTransform.sizeDelta = new Vector2(640, 110);
        detailDesc.alignment = TextAlignmentOptions.Center;

        RectTransform detailCloseRT = GetOrCreateChild(detailBoxRT, "DetailCloseButton");
        detailCloseRT.anchorMin = new Vector2(0.5f, 0.08f);
        detailCloseRT.anchorMax = new Vector2(0.5f, 0.08f);
        detailCloseRT.sizeDelta = new Vector2(200, 42);
        var detailCloseBtn = GetOrAddComponent<Button>(detailCloseRT.gameObject);
        var detailCloseImg = GetOrAddComponent<Image>(detailCloseRT.gameObject);
        detailCloseImg.color = new Color(0.35f, 0.18f, 0.22f, 0.95f);
        var detailCloseText = CreateTextChild(detailCloseRT, "Text", "CERRAR", tmpFont, 16, FontStyles.Bold, Color.white);
        detailCloseText.alignment = TextAlignmentOptions.Center;

        // H. PANEL DE CONFIGURACIÓN (SettingsPanel)
        RectTransform settingsPanelRT = GetOrCreateChild(mainMenuRT, "SettingsPanel");
        StretchFull(settingsPanelRT);
        var settingsBGImg = GetOrAddComponent<Image>(settingsPanelRT.gameObject);
        settingsBGImg.color = new Color(0.08f, 0.06f, 0.10f, 0.98f);
        settingsPanelRT.gameObject.SetActive(false);

        var setHeader = CreateTextChild(settingsPanelRT, "Title", "CONFIGURACIÓN", tmpFont, 36, FontStyles.Bold, new Color(1f, 0.85f, 0.40f));
        setHeader.rectTransform.anchorMin = new Vector2(0.5f, 0.85f);
        setHeader.rectTransform.anchorMax = new Vector2(0.5f, 0.85f);
        setHeader.rectTransform.sizeDelta = new Vector2(800, 50);
        setHeader.alignment = TextAlignmentOptions.Center;

        // Sliders de Configuración
        Slider musicSlider = CreateSettingSlider(settingsPanelRT, "MusicSlider", "Volumen Música", new Vector2(0, 60), tmpFont);
        Slider sfxSlider = CreateSettingSlider(settingsPanelRT, "SFXSlider", "Volumen Efectos de Sonido", new Vector2(0, -10), tmpFont);
        Slider textSpeedSlider = CreateSettingSlider(settingsPanelRT, "TextSpeedSlider", "Velocidad de Texto", new Vector2(0, -80), tmpFont, 0.01f, 0.06f, 0.025f);

        RectTransform setBackRT = GetOrCreateChild(settingsPanelRT, "SettingsBackButton");
        setBackRT.anchorMin = new Vector2(0.5f, 0.15f);
        setBackRT.anchorMax = new Vector2(0.5f, 0.15f);
        setBackRT.sizeDelta = new Vector2(240, 50);
        var setBackBtn = GetOrAddComponent<Button>(setBackRT.gameObject);
        var setBackImg = GetOrAddComponent<Image>(setBackRT.gameObject);
        setBackImg.color = new Color(0.25f, 0.20f, 0.32f, 0.95f);
        var setBackText = CreateTextChild(setBackRT, "Text", "← VOLVER AL MENÚ", tmpFont, 18, FontStyles.Bold, Color.white);
        setBackText.alignment = TextAlignmentOptions.Center;

        // 4. CONECTAR REFERENCIAS SERIALIZADAS
        // A. NovelController
        var serController = new SerializedObject(controller);
        serController.FindProperty("startingNode").objectReferenceValue = allNodes[0];
        serController.FindProperty("autoStartStory").boolValue = false;
        var storyNodesProp = serController.FindProperty("allStoryNodes");
        storyNodesProp.ClearArray();
        for (int i = 0; i < allNodes.Count; i++)
        {
            storyNodesProp.InsertArrayElementAtIndex(i);
            storyNodesProp.GetArrayElementAtIndex(i).objectReferenceValue = allNodes[i];
        }
        serController.FindProperty("musicSource").objectReferenceValue = musicSrc;
        serController.FindProperty("sfxSource").objectReferenceValue = sfxSrc;
        serController.ApplyModifiedProperties();

        // B. NovelUI
        var serNovelUI = new SerializedObject(novelUI);
        serNovelUI.FindProperty("controller").objectReferenceValue = controller;
        serNovelUI.FindProperty("mainMenuUI").objectReferenceValue = mainMenuUI;
        serNovelUI.FindProperty("backgroundImage").objectReferenceValue = bgImage;
        serNovelUI.FindProperty("portraitImage").objectReferenceValue = portraitImage;
        serNovelUI.FindProperty("dialoguePanel").objectReferenceValue = dialoguePanelRT.gameObject;
        serNovelUI.FindProperty("speakerNameText").objectReferenceValue = speakerNameText;
        serNovelUI.FindProperty("speakerNameBox").objectReferenceValue = nameBoxRT.gameObject;
        serNovelUI.FindProperty("dialogueText").objectReferenceValue = dialogueText;
        serNovelUI.FindProperty("advanceButton").objectReferenceValue = advanceBtn;
        serNovelUI.FindProperty("menuButton").objectReferenceValue = menuBtn;
        serNovelUI.FindProperty("choicesPanel").objectReferenceValue = choicesPanelRT.gameObject;
        serNovelUI.FindProperty("choiceButton1").objectReferenceValue = choiceBtn1;
        serNovelUI.FindProperty("choiceText1").objectReferenceValue = choiceText1;
        serNovelUI.FindProperty("choiceButton2").objectReferenceValue = choiceBtn2;
        serNovelUI.FindProperty("choiceText2").objectReferenceValue = choiceText2;
        serNovelUI.FindProperty("endingPanel").objectReferenceValue = endingPanelRT.gameObject;
        serNovelUI.FindProperty("endingTitleText").objectReferenceValue = endTitle;
        serNovelUI.FindProperty("endingDescriptionText").objectReferenceValue = endDesc;
        serNovelUI.FindProperty("restartButton").objectReferenceValue = restartBtn;
        serNovelUI.FindProperty("endingMenuButton").objectReferenceValue = endingMenuBtn;
        serNovelUI.ApplyModifiedProperties();

        // C. MainMenuUI
        var serMainMenu = new SerializedObject(mainMenuUI);
        serMainMenu.FindProperty("controller").objectReferenceValue = controller;
        serMainMenu.FindProperty("novelUI").objectReferenceValue = novelUI;
        serMainMenu.FindProperty("mainMenuPanel").objectReferenceValue = mainMenuRT.gameObject;
        serMainMenu.FindProperty("galleryPanel").objectReferenceValue = galleryPanelRT.gameObject;
        serMainMenu.FindProperty("settingsPanel").objectReferenceValue = settingsPanelRT.gameObject;
        serMainMenu.FindProperty("newGameButton").objectReferenceValue = newGameBtn;
        serMainMenu.FindProperty("continueButton").objectReferenceValue = continueBtn;
        serMainMenu.FindProperty("galleryButton").objectReferenceValue = galleryBtn;
        serMainMenu.FindProperty("settingsButton").objectReferenceValue = settingsBtn;
        serMainMenu.FindProperty("exitButton").objectReferenceValue = exitBtn;
        serMainMenu.FindProperty("galleryProgressText").objectReferenceValue = galProgress;
        serMainMenu.FindProperty("galleryBackButton").objectReferenceValue = galBackBtn;
        serMainMenu.FindProperty("endingCard1").objectReferenceValue = card1RT.gameObject;
        serMainMenu.FindProperty("card1Image").objectReferenceValue = card1Img;
        serMainMenu.FindProperty("card1TitleText").objectReferenceValue = card1Title;
        serMainMenu.FindProperty("card1LockOverlay").objectReferenceValue = card1Lock.gameObject;
        serMainMenu.FindProperty("card1Button").objectReferenceValue = card1Btn;
        serMainMenu.FindProperty("endingCard2").objectReferenceValue = card2RT.gameObject;
        serMainMenu.FindProperty("card2Image").objectReferenceValue = card2Img;
        serMainMenu.FindProperty("card2TitleText").objectReferenceValue = card2Title;
        serMainMenu.FindProperty("card2LockOverlay").objectReferenceValue = card2Lock.gameObject;
        serMainMenu.FindProperty("card2Button").objectReferenceValue = card2Btn;
        serMainMenu.FindProperty("detailModal").objectReferenceValue = detailModalRT.gameObject;
        serMainMenu.FindProperty("detailImage").objectReferenceValue = detailImg;
        serMainMenu.FindProperty("detailTitleText").objectReferenceValue = detailTitle;
        serMainMenu.FindProperty("detailDescriptionText").objectReferenceValue = detailDesc;
        serMainMenu.FindProperty("detailCloseButton").objectReferenceValue = detailCloseBtn;
        serMainMenu.FindProperty("settingsBackButton").objectReferenceValue = setBackBtn;
        serMainMenu.ApplyModifiedProperties();

        // D. SettingsManager
        var serSettings = new SerializedObject(settingsManager);
        serSettings.FindProperty("musicVolumeSlider").objectReferenceValue = musicSlider;
        serSettings.FindProperty("sfxVolumeSlider").objectReferenceValue = sfxSlider;
        serSettings.FindProperty("textSpeedSlider").objectReferenceValue = textSpeedSlider;
        serSettings.FindProperty("musicSource").objectReferenceValue = musicSrc;
        serSettings.FindProperty("sfxSource").objectReferenceValue = sfxSrc;
        serSettings.ApplyModifiedProperties();

        // Guardar escena
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("<color=#44FF44><b>[Novela]</b> ¡La escena 'Novela.unity' ha sido configurada y guardada con éxito con todos los sistemas conectados!</color>");
    }

    private static RectTransform GetOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child == null)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            child = go.transform;
        }
        return child.GetComponent<RectTransform>();
    }

    private static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void StretchWithPadding(RectTransform rt, float left, float bottom, float right, float top)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    private static TextMeshProUGUI CreateTextChild(Transform parent, string name, string text, TMP_FontAsset font, float size, FontStyles style, Color color)
    {
        RectTransform rt = GetOrCreateChild(parent, name);
        var tmp = GetOrAddComponent<TextMeshProUGUI>(rt.gameObject);
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        return tmp;
    }

    private static Button CreateMenuButton(Transform parent, string name, string label, int index, Vector2 pos, TMP_FontAsset font)
    {
        RectTransform rt = GetOrCreateChild(parent, name);
        rt.anchorMin = new Vector2(0.5f, 0.55f);
        rt.anchorMax = new Vector2(0.5f, 0.55f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(320, 50);

        var btn = GetOrAddComponent<Button>(rt.gameObject);
        var img = GetOrAddComponent<Image>(rt.gameObject);
        img.color = new Color(0.16f, 0.12f, 0.22f, 0.92f);

        var colors = btn.colors;
        colors.highlightedColor = new Color(0.35f, 0.25f, 0.50f, 1f);
        colors.pressedColor = new Color(0.12f, 0.08f, 0.18f, 1f);
        colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.45f);
        btn.colors = colors;

        var txt = CreateTextChild(rt, "Text", label, font, 18, FontStyles.Bold, Color.white);
        txt.alignment = TextAlignmentOptions.Center;

        return btn;
    }

    private static Slider CreateSettingSlider(Transform parent, string name, string label, Vector2 pos, TMP_FontAsset font, float min = 0f, float max = 1f, float def = 0.8f)
    {
        RectTransform containerRT = GetOrCreateChild(parent, name + "_Container");
        containerRT.anchorMin = new Vector2(0.5f, 0.5f);
        containerRT.anchorMax = new Vector2(0.5f, 0.5f);
        containerRT.pivot = new Vector2(0.5f, 0.5f);
        containerRT.anchoredPosition = pos;
        containerRT.sizeDelta = new Vector2(500, 50);

        var lbl = CreateTextChild(containerRT, "Label", label, font, 18, FontStyles.Bold, Color.white);
        lbl.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        lbl.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        lbl.rectTransform.pivot = new Vector2(0f, 0.5f);
        lbl.rectTransform.anchoredPosition = new Vector2(0, 0);
        lbl.rectTransform.sizeDelta = new Vector2(240, 40);
        lbl.alignment = TextAlignmentOptions.MidlineLeft;

        RectTransform sliderRT = GetOrCreateChild(containerRT, name);
        sliderRT.anchorMin = new Vector2(1f, 0.5f);
        sliderRT.anchorMax = new Vector2(1f, 0.5f);
        sliderRT.pivot = new Vector2(1f, 0.5f);
        sliderRT.anchoredPosition = new Vector2(0, 0);
        sliderRT.sizeDelta = new Vector2(240, 25);

        var slider = GetOrAddComponent<Slider>(sliderRT.gameObject);
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = def;

        // Background
        RectTransform bgRT = GetOrCreateChild(sliderRT, "Background");
        StretchFull(bgRT);
        var bgImg = GetOrAddComponent<Image>(bgRT.gameObject);
        bgImg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

        // Fill Area
        RectTransform fillAreaRT = GetOrCreateChild(sliderRT, "Fill Area");
        StretchFull(fillAreaRT);
        RectTransform fillRT = GetOrCreateChild(fillAreaRT, "Fill");
        StretchFull(fillRT);
        var fillImg = GetOrAddComponent<Image>(fillRT.gameObject);
        fillImg.color = new Color(0.85f, 0.65f, 0.25f, 1f);

        slider.fillRect = fillRT;

        return slider;
    }
}
#endif
