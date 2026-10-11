#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class GenerateVisualNovelArt
{
    private const string ArtPath = "Assets/Novela/Art/";
    private const string DataPath = "Assets/Novela/Data/";

    [MenuItem("Novela/Generar Arte y Configurar Assets")]
    public static void GenerateAllArtAndSetup()
    {
        EnsureFolderExists(ArtPath);

        // 1. Generar Fondos
        GenerateCentroCaliBG(ArtPath + "CentroDeCali_BG.png");
        GenerateIglesiaColonialBG(ArtPath + "IglesiaColonial_BG.png");
        GenerateCerroTresCrucesBG(ArtPath + "CerroTresCruces_BG.png");
        GenerateMainMenuBG(ArtPath + "MenuPrincipal_BG.png");

        // 2. Generar Tarjetas de Finales
        GenerateFinalBuenoCard(ArtPath + "FinalBueno_Card.png");
        GenerateFinalAlternativoCard(ArtPath + "FinalAlternativo_Card.png");

        // 3. Generar Retratos de Personajes
        GenerateMateoPortrait(ArtPath + "Mateo_Default.png", false);
        GenerateMateoPortrait(ArtPath + "Mateo_Panico.png", true);
        GeneratePadreEstebanPortrait(ArtPath + "PadreEsteban_Default.png");
        GenerateBuziracoPortrait(ArtPath + "Buziraco_Default.png");

        AssetDatabase.Refresh();

        // 4. Configurar como Sprites en Unity
        ConfigureAsSprite(ArtPath + "CentroDeCali_BG.png");
        ConfigureAsSprite(ArtPath + "IglesiaColonial_BG.png");
        ConfigureAsSprite(ArtPath + "CerroTresCruces_BG.png");
        ConfigureAsSprite(ArtPath + "MenuPrincipal_BG.png");
        ConfigureAsSprite(ArtPath + "FinalBueno_Card.png");
        ConfigureAsSprite(ArtPath + "FinalAlternativo_Card.png");
        ConfigureAsSprite(ArtPath + "Mateo_Default.png");
        ConfigureAsSprite(ArtPath + "Mateo_Panico.png");
        ConfigureAsSprite(ArtPath + "PadreEsteban_Default.png");
        ConfigureAsSprite(ArtPath + "Buziraco_Default.png");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 5. Vincular a ScriptableObjects
        LinkSpritesToData();

        Debug.Log("<color=#44FF44><b>[Novela]</b> ¡Todos los assets visuales fueron generados y vinculados con éxito!</color>");
    }

    private static void EnsureFolderExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    private static void ConfigureAsSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }

    private static void LinkSpritesToData()
    {
        // Ubicaciones
        var centroCali = AssetDatabase.LoadAssetAtPath<LocationData>(DataPath + "CentroDeCali.asset");
        if (centroCali != null)
        {
            centroCali.backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "CentroDeCali_BG.png");
            EditorUtility.SetDirty(centroCali);
        }

        var iglesia = AssetDatabase.LoadAssetAtPath<LocationData>(DataPath + "IglesiaColonial.asset");
        if (iglesia != null)
        {
            iglesia.backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "IglesiaColonial_BG.png");
            EditorUtility.SetDirty(iglesia);
        }

        var cerro = AssetDatabase.LoadAssetAtPath<LocationData>(DataPath + "CerroTresCruces.asset");
        if (cerro != null)
        {
            cerro.backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "CerroTresCruces_BG.png");
            EditorUtility.SetDirty(cerro);
        }

        // Personajes
        var mateo = AssetDatabase.LoadAssetAtPath<CharacterData>(DataPath + "Mateo.asset");
        if (mateo != null)
        {
            mateo.defaultPortrait = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "Mateo_Default.png");
            mateo.nameColor = new Color(0.35f, 0.75f, 1f, 1f); // Azul cielo valiente
            EditorUtility.SetDirty(mateo);
        }

        var padre = AssetDatabase.LoadAssetAtPath<CharacterData>(DataPath + "PadreEsteban.asset");
        if (padre != null)
        {
            padre.defaultPortrait = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "PadreEsteban_Default.png");
            padre.nameColor = new Color(1f, 0.85f, 0.4f, 1f); // Dorado sabio
            EditorUtility.SetDirty(padre);
        }

        var buziraco = AssetDatabase.LoadAssetAtPath<CharacterData>(DataPath + "Buziraco.asset");
        if (buziraco != null)
        {
            buziraco.defaultPortrait = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "Buziraco_Default.png");
            buziraco.nameColor = new Color(1f, 0.3f, 0.25f, 1f); // Rojo volcánico amenazante
            EditorUtility.SetDirty(buziraco);
        }

        // Expresión de pánico / daño en 03_RutaPanico
        var rutaPanico = AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "03_RutaPanico.asset");
        if (rutaPanico != null && rutaPanico.dialogueLines != null && rutaPanico.dialogueLines.Count > 2)
        {
            var line = rutaPanico.dialogueLines[2]; // Mateo lastimado por escombros
            line.customPortrait = AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + "Mateo_Panico.png");
            rutaPanico.dialogueLines[2] = line;
            EditorUtility.SetDirty(rutaPanico);
        }

        AssetDatabase.SaveAssets();
    }

    // ==========================================
    // GENERADORES PROCEDURALES DE FONDOS
    // ==========================================

    private static void GenerateCentroCaliBG(string filePath)
    {
        int w = 1280, h = 720;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var canvas = new CanvasPainter(w, h);

        // Cielo que cubre 100% del fondo: gradiente de atardecer tormentoso
        canvas.FillVerticalGradient(0, 0, w, h,
            new Color(0.68f, 0.35f, 0.18f), // Abajo cerca del horizonte: ocre cálido
            new Color(0.12f, 0.06f, 0.10f)); // Arriba: púrpura crepuscular profundo

        // Nubes de humo y tormenta de Buziraco
        canvas.FillEllipse(new Vector2(w * 0.5f, h * 0.70f), 320, 140, new Color(0.08f, 0.04f, 0.06f, 0.90f));
        canvas.FillEllipse(new Vector2(w * 0.20f, h * 0.75f), 240, 120, new Color(0.08f, 0.04f, 0.06f, 0.75f));
        canvas.FillEllipse(new Vector2(w * 0.80f, h * 0.74f), 260, 120, new Color(0.08f, 0.04f, 0.06f, 0.75f));

        // Ojos incandescentes de Buziraco
        canvas.FillEllipse(new Vector2(w * 0.465f, h * 0.72f), 18, 9, new Color(1f, 0.25f, 0.05f, 0.95f));
        canvas.FillEllipse(new Vector2(w * 0.535f, h * 0.72f), 18, 9, new Color(1f, 0.25f, 0.05f, 0.95f));
        canvas.FillEllipse(new Vector2(w * 0.465f, h * 0.72f), 8, 4, new Color(1f, 0.90f, 0.30f, 0.95f));
        canvas.FillEllipse(new Vector2(w * 0.535f, h * 0.72f), 8, 4, new Color(1f, 0.90f, 0.30f, 0.95f));

        // Silueta de los cerros tutelares de Cali en el horizonte
        canvas.FillPolygon(new[]
        {
            new Vector2(0, h * 0.42f),
            new Vector2(w * 0.25f, h * 0.50f),
            new Vector2(w * 0.50f, h * 0.45f),
            new Vector2(w * 0.78f, h * 0.52f),
            new Vector2(w, h * 0.44f),
            new Vector2(w, 0),
            new Vector2(0, 0)
        }, new Color(0.24f, 0.14f, 0.16f));

        // Edificio colonial izquierdo (Casona con balcones y tejas)
        canvas.FillRectangle(new RectInt(0, 0, 380, (int)(h * 0.52f)), new Color(0.62f, 0.44f, 0.32f));
        canvas.FillPolygon(new[]
        {
            new Vector2(-20, h * 0.52f),
            new Vector2(180, h * 0.65f),
            new Vector2(400, h * 0.52f)
        }, new Color(0.52f, 0.18f, 0.10f));
        // Ventanas y postigos
        canvas.FillRectangle(new RectInt(60, (int)(h * 0.22f), 65, 110), new Color(0.12f, 0.07f, 0.06f));
        canvas.FillRectangle(new RectInt(170, (int)(h * 0.22f), 65, 110), new Color(0.12f, 0.07f, 0.06f));
        canvas.FillRectangle(new RectInt(280, (int)(h * 0.22f), 65, 110), new Color(0.12f, 0.07f, 0.06f));
        // Balcón colonial corrido
        canvas.FillRectangle(new RectInt(40, (int)(h * 0.20f), 320, 16), new Color(0.35f, 0.22f, 0.15f));

        // Grieta sísmica vertical en la fachada izquierda
        canvas.DrawPolyline(new[]
        {
            new Vector2(130, h * 0.52f),
            new Vector2(150, h * 0.40f),
            new Vector2(120, h * 0.28f),
            new Vector2(160, h * 0.16f),
            new Vector2(135, 0)
        }, new Color(0.14f, 0.06f, 0.04f), 5);

        // Edificio colonial derecho (Iglesia y campanario de estilo colonial)
        canvas.FillRectangle(new RectInt(900, 0, 380, (int)(h * 0.58f)), new Color(0.70f, 0.52f, 0.38f));
        canvas.FillPolygon(new[]
        {
            new Vector2(880, h * 0.58f),
            new Vector2(1090, h * 0.72f),
            new Vector2(1280, h * 0.58f)
        }, new Color(0.52f, 0.18f, 0.10f));
        // Arcos de campanario
        canvas.FillEllipse(new Vector2(985, h * 0.42f), 32, 55, new Color(0.12f, 0.07f, 0.06f));
        canvas.FillEllipse(new Vector2(1115, h * 0.42f), 32, 55, new Color(0.12f, 0.07f, 0.06f));

        // Calle empedrada colonial (primer plano)
        canvas.FillVerticalGradient(0, 0, w, (int)(h * 0.32f),
            new Color(0.16f, 0.12f, 0.10f),
            new Color(0.35f, 0.27f, 0.23f));

        // Gran fractura sísmica en la calle
        canvas.FillPolygon(new[]
        {
            new Vector2(430, 0),
            new Vector2(495, h * 0.08f),
            new Vector2(550, h * 0.16f),
            new Vector2(610, h * 0.24f),
            new Vector2(660, h * 0.30f),
            new Vector2(685, h * 0.30f),
            new Vector2(635, h * 0.23f),
            new Vector2(580, h * 0.15f),
            new Vector2(520, h * 0.07f),
            new Vector2(470, 0)
        }, new Color(0.06f, 0.03f, 0.03f));

        // Resplandor telúrico de polvo en la grieta
        canvas.DrawPolyline(new[]
        {
            new Vector2(450, 0),
            new Vector2(508, h * 0.07f),
            new Vector2(565, h * 0.15f),
            new Vector2(622, h * 0.23f),
            new Vector2(672, h * 0.30f)
        }, new Color(0.98f, 0.45f, 0.15f, 0.75f), 4);

        // Escombros de ladrillo y mampostería caídos
        canvas.FillPolygon(new[] { new Vector2(240, 50), new Vector2(290, 35), new Vector2(275, 85) }, new Color(0.48f, 0.36f, 0.28f));
        canvas.FillPolygon(new[] { new Vector2(340, 35), new Vector2(380, 15), new Vector2(370, 60) }, new Color(0.52f, 0.39f, 0.30f));
        canvas.FillPolygon(new[] { new Vector2(760, 45), new Vector2(810, 25), new Vector2(795, 75) }, new Color(0.48f, 0.36f, 0.28f));

        // Partículas de polvo en suspensión
        canvas.AddRandomParticles(75, new Rect(0, h * 0.05f, w, h * 0.85f), new Color(0.95f, 0.82f, 0.65f, 0.55f), 2, 5, 42);

        canvas.ApplyToTexture(tex);
        File.WriteAllBytes(filePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void GenerateIglesiaColonialBG(string filePath)
    {
        int w = 1280, h = 720;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var canvas = new CanvasPainter(w, h);

        // Bóveda de la iglesia en penumbra solemne (100% de la altura)
        canvas.FillVerticalGradient(0, 0, w, h,
            new Color(0.10f, 0.07f, 0.06f),
            new Color(0.24f, 0.18f, 0.14f));

        // Muro de fondo del presbiterio
        canvas.FillRectangle(new RectInt(360, (int)(h * 0.12f), 560, (int)(h * 0.75f)), new Color(0.18f, 0.14f, 0.11f));

        // Gran Vitral Colonial circular en lo alto
        canvas.FillEllipse(new Vector2(w * 0.5f, h * 0.70f), 115, 115, new Color(0.45f, 0.34f, 0.18f));
        canvas.FillEllipse(new Vector2(w * 0.5f, h * 0.70f), 102, 102, new Color(1f, 0.85f, 0.42f, 0.95f));
        // Cruz en el vitral
        canvas.DrawLine(new Vector2(w * 0.5f, h * 0.82f), new Vector2(w * 0.5f, h * 0.58f), new Color(0.12f, 0.08f, 0.06f), 8);
        canvas.DrawLine(new Vector2(w * 0.5f - 65, h * 0.72f), new Vector2(w * 0.5f + 65, h * 0.72f), new Color(0.12f, 0.08f, 0.06f), 8);

        // Haz de luz celestial que desciende del vitral
        canvas.FillPolygon(new[]
        {
            new Vector2(w * 0.45f, h * 0.62f),
            new Vector2(w * 0.55f, h * 0.62f),
            new Vector2(w * 0.72f, 0),
            new Vector2(w * 0.28f, 0)
        }, new Color(1f, 0.92f, 0.65f, 0.24f));

        // Columnas masivas de hormigón y piedra estructural (ZONA SEGURA)
        // Columna Izquierda
        canvas.FillVerticalGradient(110, 0, 240, h, new Color(0.46f, 0.36f, 0.30f), new Color(0.28f, 0.22f, 0.18f));
        canvas.FillRectangle(new RectInt(80, (int)(h * 0.82f), 300, 45), new Color(0.58f, 0.46f, 0.38f));
        canvas.FillRectangle(new RectInt(80, 40, 300, 60), new Color(0.58f, 0.46f, 0.38f));

        // Columna Derecha
        canvas.FillVerticalGradient(930, 0, 240, h, new Color(0.46f, 0.36f, 0.30f), new Color(0.28f, 0.22f, 0.18f));
        canvas.FillRectangle(new RectInt(900, (int)(h * 0.82f), 300, 45), new Color(0.58f, 0.46f, 0.38f));
        canvas.FillRectangle(new RectInt(900, 40, 300, 60), new Color(0.58f, 0.46f, 0.38f));

        // Arco colonial superior conectando las columnas
        for (int i = 0; i < 30; i++)
        {
            float t = i / 29f;
            float angle = Mathf.Lerp(0, Mathf.PI, t);
            float cx = Mathf.Lerp(230, 1050, t);
            float cy = h * 0.70f + Mathf.Sin(angle) * 125;
            canvas.FillCircle(new Vector2(cx, cy), 20, new Color(0.36f, 0.28f, 0.24f));
        }

        // Suelo de losas coloniales
        canvas.FillVerticalGradient(0, 0, w, (int)(h * 0.25f),
            new Color(0.14f, 0.11f, 0.09f),
            new Color(0.25f, 0.20f, 0.16f));

        // Partículas doradas flotantes
        canvas.AddRandomParticles(65, new Rect(w * 0.30f, h * 0.10f, w * 0.40f, h * 0.70f), new Color(1f, 0.95f, 0.65f, 0.55f), 2, 5, 88);

        canvas.ApplyToTexture(tex);
        File.WriteAllBytes(filePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void GenerateCerroTresCrucesBG(string filePath)
    {
        int w = 1280, h = 720;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var canvas = new CanvasPainter(w, h);

        // Cielo que cubre 100% de la altura: tormenta telúrica de Buziraco
        canvas.FillVerticalGradient(0, 0, w, h,
            new Color(0.60f, 0.22f, 0.14f), // Abajo: resplandor rojizo de fallas
            new Color(0.12f, 0.05f, 0.14f)); // Arriba: oscuridad tormentosa

        // Fumarolas de ceniza volcánica
        canvas.FillEllipse(new Vector2(w * 0.15f, h * 0.75f), 300, 150, new Color(0.10f, 0.05f, 0.07f, 0.85f));
        canvas.FillEllipse(new Vector2(w * 0.85f, h * 0.78f), 320, 160, new Color(0.10f, 0.05f, 0.07f, 0.85f));

        // Silueta colosal de Buziraco emergiendo tras la cima
        canvas.FillPolygon(new[]
        {
            new Vector2(w * 0.30f, h * 0.45f),
            new Vector2(w * 0.38f, h * 0.72f),
            new Vector2(w * 0.50f, h * 0.92f),
            new Vector2(w * 0.62f, h * 0.72f),
            new Vector2(w * 0.70f, h * 0.45f)
        }, new Color(0.06f, 0.02f, 0.04f, 0.95f));

        // Cuernos demoníacos
        canvas.FillPolygon(new[]
        {
            new Vector2(w * 0.44f, h * 0.86f),
            new Vector2(w * 0.38f, h * 0.98f),
            new Vector2(w * 0.42f, h * 0.89f)
        }, new Color(0.14f, 0.06f, 0.08f));
        canvas.FillPolygon(new[]
        {
            new Vector2(w * 0.56f, h * 0.86f),
            new Vector2(w * 0.62f, h * 0.98f),
            new Vector2(w * 0.58f, h * 0.89f)
        }, new Color(0.14f, 0.06f, 0.08f));

        // Ojos de magma incandescentes de Buziraco
        canvas.FillEllipse(new Vector2(w * 0.465f, h * 0.78f), 16, 9, new Color(1f, 0.28f, 0.08f, 0.95f));
        canvas.FillEllipse(new Vector2(w * 0.535f, h * 0.78f), 16, 9, new Color(1f, 0.28f, 0.08f, 0.95f));
        canvas.FillEllipse(new Vector2(w * 0.465f, h * 0.78f), 7, 4, new Color(1f, 0.90f, 0.25f, 0.95f));
        canvas.FillEllipse(new Vector2(w * 0.535f, h * 0.78f), 7, 4, new Color(1f, 0.90f, 0.25f, 0.95f));

        // Cima rocosa del Cerro de las Tres Cruces
        canvas.FillPolygon(new[]
        {
            new Vector2(0, h * 0.35f),
            new Vector2(w * 0.25f, h * 0.48f),
            new Vector2(w * 0.50f, h * 0.54f),
            new Vector2(w * 0.75f, h * 0.49f),
            new Vector2(w, h * 0.37f),
            new Vector2(w, 0),
            new Vector2(0, 0)
        }, new Color(0.26f, 0.18f, 0.15f));

        // LAS TRES CRUCES MONUMENTALES DE CALI
        // Cruz Central (La más grande y alta)
        canvas.FillRectangle(new RectInt((int)(w * 0.50f - 18), (int)(h * 0.50f), 36, 240), new Color(0.88f, 0.85f, 0.80f));
        canvas.FillRectangle(new RectInt((int)(w * 0.50f - 110), (int)(h * 0.65f), 220, 36), new Color(0.88f, 0.85f, 0.80f));

        // Cruz Izquierda
        canvas.FillRectangle(new RectInt((int)(w * 0.35f - 13), (int)(h * 0.44f), 26, 180), new Color(0.82f, 0.78f, 0.74f));
        canvas.FillRectangle(new RectInt((int)(w * 0.35f - 80), (int)(h * 0.55f), 160, 26), new Color(0.82f, 0.78f, 0.74f));

        // Cruz Derecha
        canvas.FillRectangle(new RectInt((int)(w * 0.65f - 13), (int)(h * 0.44f), 26, 180), new Color(0.82f, 0.78f, 0.74f));
        canvas.FillRectangle(new RectInt((int)(w * 0.65f - 80), (int)(h * 0.55f), 160, 26), new Color(0.82f, 0.78f, 0.74f));

        // Grietas tectónicas resplandecientes en la montaña
        canvas.DrawPolyline(new[]
        {
            new Vector2(140, 20),
            new Vector2(300, h * 0.20f),
            new Vector2(420, h * 0.17f),
            new Vector2(560, h * 0.28f),
            new Vector2(640, h * 0.24f)
        }, new Color(1f, 0.45f, 0.10f, 0.90f), 6);

        canvas.DrawPolyline(new[]
        {
            new Vector2(1140, 20),
            new Vector2(980, h * 0.20f),
            new Vector2(860, h * 0.18f),
            new Vector2(740, h * 0.26f)
        }, new Color(1f, 0.45f, 0.10f, 0.90f), 6);

        // Cenizas volcánicas flotantes
        canvas.AddRandomParticles(80, new Rect(0, h * 0.05f, w, h * 0.90f), new Color(1f, 0.65f, 0.35f, 0.75f), 2, 6, 99);

        canvas.ApplyToTexture(tex);
        File.WriteAllBytes(filePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void GenerateMainMenuBG(string filePath)
    {
        int w = 1280, h = 720;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var canvas = new CanvasPainter(w, h);

        // Cielo crepuscular dramático sobre Cali (100% pantalla)
        canvas.FillVerticalGradient(0, 0, w, h,
            new Color(0.42f, 0.16f, 0.20f),
            new Color(0.08f, 0.04f, 0.12f));

        // Farallones de Cali en el horizonte
        canvas.FillPolygon(new[]
        {
            new Vector2(0, h * 0.46f),
            new Vector2(w * 0.22f, h * 0.58f),
            new Vector2(w * 0.42f, h * 0.52f),
            new Vector2(w * 0.65f, h * 0.60f),
            new Vector2(w * 0.88f, h * 0.55f),
            new Vector2(w, h * 0.58f),
            new Vector2(w, 0),
            new Vector2(0, 0)
        }, new Color(0.18f, 0.10f, 0.16f));

        // Cerro de las Tres Cruces a la izquierda
        canvas.FillPolygon(new[]
        {
            new Vector2(0, h * 0.62f),
            new Vector2(w * 0.30f, h * 0.65f),
            new Vector2(w * 0.54f, h * 0.32f),
            new Vector2(w * 0.64f, 0),
            new Vector2(0, 0)
        }, new Color(0.22f, 0.14f, 0.18f));

        // Silueta de las Tres Cruces iluminadas
        // Cruz principal
        canvas.FillRectangle(new RectInt((int)(w * 0.30f - 10), (int)(h * 0.65f), 20, 130), new Color(0.95f, 0.92f, 0.88f));
        canvas.FillRectangle(new RectInt((int)(w * 0.30f - 60), (int)(h * 0.73f), 120, 20), new Color(0.95f, 0.92f, 0.88f));
        // Cruces laterales
        canvas.FillRectangle(new RectInt((int)(w * 0.21f - 7), (int)(h * 0.60f), 14, 90), new Color(0.88f, 0.85f, 0.80f));
        canvas.FillRectangle(new RectInt((int)(w * 0.21f - 42), (int)(h * 0.66f), 84, 14), new Color(0.88f, 0.85f, 0.80f));

        canvas.FillRectangle(new RectInt((int)(w * 0.39f - 7), (int)(h * 0.60f), 14, 90), new Color(0.88f, 0.85f, 0.80f));
        canvas.FillRectangle(new RectInt((int)(w * 0.39f - 42), (int)(h * 0.66f), 84, 14), new Color(0.88f, 0.85f, 0.80f));

        // Silueta demoníaca de Buziraco en las nubes a la derecha
        canvas.FillPolygon(new[]
        {
            new Vector2(w * 0.65f, h * 0.40f),
            new Vector2(w * 0.75f, h * 0.70f),
            new Vector2(w * 0.86f, h * 0.86f),
            new Vector2(w * 0.96f, h * 0.68f),
            new Vector2(w, h * 0.40f)
        }, new Color(0.06f, 0.03f, 0.06f, 0.90f));
        // Ojos de fuego
        canvas.FillEllipse(new Vector2(w * 0.83f, h * 0.75f), 14, 7, new Color(1f, 0.28f, 0.08f, 0.95f));
        canvas.FillEllipse(new Vector2(w * 0.89f, h * 0.75f), 14, 7, new Color(1f, 0.28f, 0.08f, 0.95f));

        // Luces nocturnas de Cali en el valle
        canvas.AddRandomParticles(140, new Rect(w * 0.38f, h * 0.08f, w * 0.60f, h * 0.32f), new Color(1f, 0.82f, 0.42f, 0.75f), 2, 4, 12);

        // Viñeteado cinematográfico
        canvas.ApplyVignette(0.50f);

        canvas.ApplyToTexture(tex);
        File.WriteAllBytes(filePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void GenerateFinalBuenoCard(string filePath)
    {
        int w = 640, h = 360;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var canvas = new CanvasPainter(w, h);

        // Amanecer radiante dorado sobre las montañas
        canvas.FillVerticalGradient(0, 0, w, h,
            new Color(0.96f, 0.78f, 0.38f),
            new Color(0.18f, 0.38f, 0.58f));

        // Rayos solares dorados del ritual
        for (int i = 0; i < 12; i++)
        {
            float angle = i * Mathf.PI / 11f;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            canvas.DrawLine(new Vector2(w * 0.5f, h * 0.45f), new Vector2(w * 0.5f, h * 0.45f) + dir * 300, new Color(1f, 0.95f, 0.70f, 0.25f), 10);
        }

        // Resplandor del ritual divino
        canvas.FillEllipse(new Vector2(w * 0.5f, h * 0.55f), 190, 130, new Color(1f, 0.96f, 0.78f, 0.55f));

        // Cerro pacífico verdeante
        canvas.FillPolygon(new[]
        {
            new Vector2(0, h * 0.32f),
            new Vector2(w * 0.5f, h * 0.48f),
            new Vector2(w, h * 0.32f),
            new Vector2(w, 0),
            new Vector2(0, 0)
        }, new Color(0.22f, 0.40f, 0.26f));

        // Las Tres Cruces doradas resplandecientes
        canvas.FillRectangle(new RectInt(w / 2 - 9, (int)(h * 0.46f), 18, 100), new Color(1f, 0.94f, 0.45f));
        canvas.FillRectangle(new RectInt(w / 2 - 48, (int)(h * 0.57f), 96, 18), new Color(1f, 0.94f, 0.45f));

        canvas.FillRectangle(new RectInt((int)(w * 0.35f - 7), (int)(h * 0.41f), 14, 70), new Color(1f, 0.90f, 0.40f));
        canvas.FillRectangle(new RectInt((int)(w * 0.35f - 38), (int)(h * 0.49f), 76, 14), new Color(1f, 0.90f, 0.40f));

        canvas.FillRectangle(new RectInt((int)(w * 0.65f - 7), (int)(h * 0.41f), 14, 70), new Color(1f, 0.90f, 0.40f));
        canvas.FillRectangle(new RectInt((int)(w * 0.65f - 38), (int)(h * 0.49f), 76, 14), new Color(1f, 0.90f, 0.40f));

        // Borde decorativo
        canvas.DrawFrame(new Color(0.96f, 0.85f, 0.35f), 4);

        canvas.ApplyToTexture(tex);
        File.WriteAllBytes(filePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void GenerateFinalAlternativoCard(string filePath)
    {
        int w = 640, h = 360;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var canvas = new CanvasPainter(w, h);

        // Cielo de ruina y sombras carmesí
        canvas.FillVerticalGradient(0, 0, w, h,
            new Color(0.42f, 0.08f, 0.06f),
            new Color(0.08f, 0.02f, 0.03f));

        // Silueta gigante de Buziraco triunfante en la tormenta
        canvas.FillPolygon(new[]
        {
            new Vector2(w * 0.30f, h * 0.30f),
            new Vector2(w * 0.40f, h * 0.68f),
            new Vector2(w * 0.50f, h * 0.90f),
            new Vector2(w * 0.60f, h * 0.68f),
            new Vector2(w * 0.70f, h * 0.30f)
        }, new Color(0.05f, 0.02f, 0.03f));

        // Ojos crueles de fuego
        canvas.FillEllipse(new Vector2(w * 0.465f, h * 0.72f), 14, 7, new Color(1f, 0.20f, 0.10f));
        canvas.FillEllipse(new Vector2(w * 0.535f, h * 0.72f), 14, 7, new Color(1f, 0.20f, 0.10f));

        // Tierra agrietada y ardiendo
        canvas.FillPolygon(new[]
        {
            new Vector2(0, h * 0.35f),
            new Vector2(w * 0.5f, h * 0.38f),
            new Vector2(w, h * 0.32f),
            new Vector2(w, 0),
            new Vector2(0, 0)
        }, new Color(0.18f, 0.10f, 0.10f));

        // Grieta ardiente
        canvas.DrawPolyline(new[]
        {
            new Vector2(50, 20),
            new Vector2(170, h * 0.15f),
            new Vector2(310, h * 0.12f),
            new Vector2(450, h * 0.22f),
            new Vector2(590, 20)
        }, new Color(1f, 0.38f, 0.10f), 5);

        // Borde carmesí
        canvas.DrawFrame(new Color(0.80f, 0.20f, 0.15f), 4);

        canvas.ApplyToTexture(tex);
        File.WriteAllBytes(filePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    // ==========================================
    // GENERADORES DE RETRATOS DE PERSONAJES
    // ==========================================

    private static void GenerateMateoPortrait(string filePath, bool inPanic)
    {
        int w = 512, h = 768;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var canvas = new CanvasPainter(w, h);
        canvas.Clear(Color.clear);

        // Torso / Chaqueta azul deportiva con proporciones anatómicas naturales
        canvas.FillPolygon(new[]
        {
            new Vector2(190, 390),
            new Vector2(80, 280),
            new Vector2(20, 0),
            new Vector2(492, 0),
            new Vector2(432, 280),
            new Vector2(322, 390)
        }, new Color(0.15f, 0.35f, 0.60f));

        // Sombra de pliegues en la chaqueta
        canvas.FillPolygon(new[]
        {
            new Vector2(190, 390),
            new Vector2(80, 280),
            new Vector2(120, 0),
            new Vector2(210, 0)
        }, new Color(0.10f, 0.24f, 0.44f));

        // Cuello de camiseta interior
        canvas.FillPolygon(new[]
        {
            new Vector2(215, 390),
            new Vector2(256, 320),
            new Vector2(297, 390)
        }, new Color(0.88f, 0.90f, 0.92f));

        // Manuscrito antiguo en el bolsillo/pecho
        canvas.FillRectangle(new RectInt(280, 100, 130, 95), new Color(0.86f, 0.74f, 0.54f));
        canvas.DrawFrameInRect(new RectInt(280, 100, 130, 95), new Color(0.55f, 0.40f, 0.25f), 3);

        // Cuello natural y proporcionado
        canvas.FillRectangle(new RectInt(210, 360, 92, 80), new Color(0.92f, 0.75f, 0.60f));
        // Sombra bajo la barbilla
        canvas.FillRectangle(new RectInt(210, 400, 92, 35), new Color(0.82f, 0.64f, 0.48f));

        // Cabeza y rostro de Mateo (joven)
        canvas.FillEllipse(new Vector2(256, 500), 85, 95, new Color(0.92f, 0.75f, 0.60f));
        // Barbilla
        canvas.FillPolygon(new[]
        {
            new Vector2(190, 490),
            new Vector2(256, 415),
            new Vector2(322, 490)
        }, new Color(0.92f, 0.75f, 0.60f));

        // Cabello castaño estilizado (volumen superior y flequillo moderno)
        canvas.FillEllipse(new Vector2(256, 550), 95, 75, new Color(0.28f, 0.18f, 0.12f));
        canvas.FillPolygon(new[]
        {
            new Vector2(165, 520),
            new Vector2(200, 470),
            new Vector2(230, 525),
            new Vector2(265, 475),
            new Vector2(295, 525),
            new Vector2(330, 470),
            new Vector2(347, 520),
            new Vector2(310, 600),
            new Vector2(202, 600)
        }, new Color(0.28f, 0.18f, 0.12f));

        // Ojos, cejas y expresión
        if (inPanic)
        {
            // Ojos abiertos en alerta / impacto
            canvas.FillEllipse(new Vector2(220, 490), 18, 14, Color.white);
            canvas.FillEllipse(new Vector2(292, 490), 18, 14, Color.white);
            canvas.FillEllipse(new Vector2(220, 490), 8, 8, new Color(0.22f, 0.15f, 0.10f));
            canvas.FillEllipse(new Vector2(292, 490), 8, 8, new Color(0.22f, 0.15f, 0.10f));
            // Brillo en los ojos
            canvas.FillCircle(new Vector2(222, 493), 3, Color.white);
            canvas.FillCircle(new Vector2(294, 493), 3, Color.white);

            // Cejas inclinadas de pánico
            canvas.DrawLine(new Vector2(200, 515), new Vector2(238, 508), new Color(0.22f, 0.14f, 0.10f), 4);
            canvas.DrawLine(new Vector2(312, 515), new Vector2(274, 508), new Color(0.22f, 0.14f, 0.10f), 4);

            // Herida sangrante de escombros en la mejilla
            canvas.DrawLine(new Vector2(295, 455), new Vector2(325, 435), new Color(0.78f, 0.18f, 0.15f), 3);
            canvas.DrawLine(new Vector2(305, 450), new Vector2(320, 420), new Color(0.78f, 0.18f, 0.15f), 2);

            // Boca jadeando
            canvas.FillEllipse(new Vector2(256, 445), 10, 8, new Color(0.48f, 0.20f, 0.18f));
        }
        else
        {
            // Ojos determinados y serios
            canvas.FillEllipse(new Vector2(220, 490), 17, 11, Color.white);
            canvas.FillEllipse(new Vector2(292, 490), 17, 11, Color.white);
            canvas.FillEllipse(new Vector2(220, 490), 8, 8, new Color(0.22f, 0.15f, 0.10f));
            canvas.FillEllipse(new Vector2(292, 490), 8, 8, new Color(0.22f, 0.15f, 0.10f));
            // Brillo
            canvas.FillCircle(new Vector2(222, 492), 3, Color.white);
            canvas.FillCircle(new Vector2(294, 492), 3, Color.white);

            // Cejas firmes
            canvas.DrawLine(new Vector2(200, 510), new Vector2(238, 505), new Color(0.22f, 0.14f, 0.10f), 4);
            canvas.DrawLine(new Vector2(312, 510), new Vector2(274, 505), new Color(0.22f, 0.14f, 0.10f), 4);

            // Nariz sutil
            canvas.DrawLine(new Vector2(256, 485), new Vector2(253, 460), new Color(0.78f, 0.58f, 0.44f), 2);
            canvas.DrawLine(new Vector2(253, 460), new Vector2(260, 460), new Color(0.78f, 0.58f, 0.44f), 2);

            // Boca firme
            canvas.DrawLine(new Vector2(242, 442), new Vector2(270, 442), new Color(0.58f, 0.32f, 0.28f), 3);
        }

        canvas.ApplyToTexture(tex);
        File.WriteAllBytes(filePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void GeneratePadreEstebanPortrait(string filePath)
    {
        int w = 512, h = 768;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var canvas = new CanvasPainter(w, h);
        canvas.Clear(Color.clear);

        // Sotana negra con hombros anchos solemnes
        canvas.FillPolygon(new[]
        {
            new Vector2(190, 390),
            new Vector2(75, 280),
            new Vector2(15, 0),
            new Vector2(497, 0),
            new Vector2(437, 280),
            new Vector2(322, 390)
        }, new Color(0.12f, 0.12f, 0.15f));

        // Sombra de sotana
        canvas.FillPolygon(new[]
        {
            new Vector2(190, 390),
            new Vector2(75, 280),
            new Vector2(115, 0),
            new Vector2(210, 0)
        }, new Color(0.08f, 0.08f, 0.10f));

        // Cruz pectoral dorada sobre la sotana
        canvas.DrawLine(new Vector2(256, 110), new Vector2(256, 210), new Color(0.80f, 0.65f, 0.35f), 9);
        canvas.DrawLine(new Vector2(225, 175), new Vector2(287, 175), new Color(0.80f, 0.65f, 0.35f), 9);
        // Cordón
        canvas.DrawLine(new Vector2(256, 210), new Vector2(230, 360), new Color(0.55f, 0.45f, 0.25f), 3);
        canvas.DrawLine(new Vector2(256, 210), new Vector2(282, 360), new Color(0.55f, 0.45f, 0.25f), 3);

        // Cuello clerical romano blanco
        canvas.FillRectangle(new RectInt(238, 375, 36, 18), Color.white);

        // Cuello natural
        canvas.FillRectangle(new RectInt(210, 360, 92, 75), new Color(0.88f, 0.72f, 0.58f));
        canvas.FillRectangle(new RectInt(210, 395, 92, 35), new Color(0.78f, 0.60f, 0.46f));

        // Rostro maduro
        canvas.FillEllipse(new Vector2(256, 500), 85, 95, new Color(0.88f, 0.72f, 0.58f));
        canvas.FillPolygon(new[]
        {
            new Vector2(190, 490),
            new Vector2(256, 415),
            new Vector2(322, 490)
        }, new Color(0.88f, 0.72f, 0.58f));

        // Barba canosa plateada
        canvas.FillPolygon(new[]
        {
            new Vector2(215, 470),
            new Vector2(256, 400),
            new Vector2(297, 470),
            new Vector2(256, 450)
        }, new Color(0.76f, 0.78f, 0.82f));

        // Cabello canoso plateado peinado hacia atrás
        canvas.FillEllipse(new Vector2(256, 555), 95, 75, new Color(0.76f, 0.78f, 0.82f));
        // Sienes plateadas
        canvas.FillPolygon(new[]
        {
            new Vector2(165, 520),
            new Vector2(195, 480),
            new Vector2(220, 540),
            new Vector2(292, 540),
            new Vector2(317, 480),
            new Vector2(347, 520),
            new Vector2(310, 605),
            new Vector2(202, 605)
        }, new Color(0.76f, 0.78f, 0.82f));

        // Ojos serenos y vigilantes
        canvas.FillEllipse(new Vector2(220, 490), 16, 11, Color.white);
        canvas.FillEllipse(new Vector2(292, 490), 16, 11, Color.white);
        canvas.FillEllipse(new Vector2(220, 490), 8, 8, new Color(0.24f, 0.18f, 0.14f));
        canvas.FillEllipse(new Vector2(292, 490), 8, 8, new Color(0.24f, 0.18f, 0.14f));
        canvas.FillCircle(new Vector2(222, 492), 3, Color.white);
        canvas.FillCircle(new Vector2(294, 492), 3, Color.white);

        // Cejas canosas pobladas
        canvas.DrawLine(new Vector2(200, 510), new Vector2(238, 508), new Color(0.65f, 0.68f, 0.72f), 4);
        canvas.DrawLine(new Vector2(312, 510), new Vector2(274, 508), new Color(0.65f, 0.68f, 0.72f), 4);

        // Líneas de expresión y arrugas de sabiduría
        canvas.DrawLine(new Vector2(256, 485), new Vector2(253, 460), new Color(0.72f, 0.55f, 0.42f), 2);
        canvas.DrawLine(new Vector2(253, 460), new Vector2(260, 460), new Color(0.72f, 0.55f, 0.42f), 2);

        // Boca serena
        canvas.DrawLine(new Vector2(242, 442), new Vector2(270, 442), new Color(0.55f, 0.35f, 0.30f), 3);

        canvas.ApplyToTexture(tex);
        File.WriteAllBytes(filePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    private static void GenerateBuziracoPortrait(string filePath)
    {
        int w = 512, h = 768;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var canvas = new CanvasPainter(w, h);
        canvas.Clear(Color.clear);

        // Torso masivo de humo demoníaco y piedra oscura
        canvas.FillPolygon(new[]
        {
            new Vector2(256, 620),
            new Vector2(400, 480),
            new Vector2(480, 240),
            new Vector2(502, 0),
            new Vector2(10, 0),
            new Vector2(32, 240),
            new Vector2(112, 480)
        }, new Color(0.06f, 0.02f, 0.04f, 0.96f));

        // Cuernos volcánicos amenazantes de basalto
        canvas.FillPolygon(new[]
        {
            new Vector2(180, 560),
            new Vector2(100, 710),
            new Vector2(145, 590)
        }, new Color(0.20f, 0.08f, 0.10f));

        canvas.FillPolygon(new[]
        {
            new Vector2(332, 560),
            new Vector2(412, 710),
            new Vector2(367, 590)
        }, new Color(0.20f, 0.08f, 0.10f));

        // Resplandor de ojos incandescentes
        canvas.FillEllipse(new Vector2(195, 490), 32, 22, new Color(1f, 0.20f, 0.05f, 0.45f));
        canvas.FillEllipse(new Vector2(317, 490), 32, 22, new Color(1f, 0.20f, 0.05f, 0.45f));

        // Ojos de magma con núcleo amarillo de fuego
        canvas.FillEllipse(new Vector2(195, 490), 20, 11, new Color(1f, 0.40f, 0.08f));
        canvas.FillEllipse(new Vector2(317, 490), 20, 11, new Color(1f, 0.40f, 0.08f));
        canvas.FillEllipse(new Vector2(195, 490), 10, 5, new Color(1f, 0.95f, 0.30f));
        canvas.FillEllipse(new Vector2(317, 490), 10, 5, new Color(1f, 0.95f, 0.30f));

        // Fisuras tectónicas ardientes en el pecho
        canvas.DrawPolyline(new[]
        {
            new Vector2(256, 390),
            new Vector2(225, 300),
            new Vector2(275, 210),
            new Vector2(235, 110),
            new Vector2(265, 0)
        }, new Color(1f, 0.45f, 0.10f), 6);

        canvas.DrawPolyline(new[]
        {
            new Vector2(225, 300),
            new Vector2(150, 230),
            new Vector2(130, 110)
        }, new Color(1f, 0.45f, 0.10f), 5);

        canvas.DrawPolyline(new[]
        {
            new Vector2(275, 210),
            new Vector2(360, 160),
            new Vector2(385, 40)
        }, new Color(1f, 0.45f, 0.10f), 5);

        // Sonrisa demoníaca en las sombras
        canvas.DrawLine(new Vector2(215, 425), new Vector2(256, 410), new Color(1f, 0.30f, 0.08f), 4);
        canvas.DrawLine(new Vector2(256, 410), new Vector2(297, 425), new Color(1f, 0.30f, 0.08f), 4);

        canvas.ApplyToTexture(tex);
        File.WriteAllBytes(filePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
    }

    // ==========================================
    // CLASE AUXILIAR DE PINTURA PROCEDURAL 2D
    // ==========================================
    private class CanvasPainter
    {
        private readonly int width;
        private readonly int height;
        private readonly Color[] pixels;

        public CanvasPainter(int w, int h)
        {
            width = w;
            height = h;
            pixels = new Color[w * h];
        }

        public void Clear(Color c)
        {
            for (int i = 0; i < pixels.Length; i++) pixels[i] = c;
        }

        public void FillRectangle(RectInt rect, Color c)
        {
            int minX = Mathf.Clamp(rect.xMin, 0, width);
            int maxX = Mathf.Clamp(rect.xMax, 0, width);
            int minY = Mathf.Clamp(rect.yMin, 0, height);
            int maxY = Mathf.Clamp(rect.yMax, 0, height);

            for (int y = minY; y < maxY; y++)
            {
                int row = y * width;
                for (int x = minX; x < maxX; x++)
                {
                    BlendPixel(row + x, c);
                }
            }
        }

        public void FillVerticalGradient(int startX, int startY, int w, int h, Color bottomColor, Color topColor)
        {
            int minX = Mathf.Clamp(startX, 0, width);
            int maxX = Mathf.Clamp(startX + w, 0, width);
            int minY = Mathf.Clamp(startY, 0, height);
            int maxY = Mathf.Clamp(startY + h, 0, height);

            for (int y = minY; y < maxY; y++)
            {
                float t = Mathf.InverseLerp(startY, startY + h, y);
                Color rowColor = Color.Lerp(bottomColor, topColor, t);
                int row = y * width;
                for (int x = minX; x < maxX; x++)
                {
                    BlendPixel(row + x, rowColor);
                }
            }
        }

        public void FillCircle(Vector2 center, float radius, Color c)
        {
            FillEllipse(center, radius, radius, c);
        }

        public void FillEllipse(Vector2 center, float rx, float ry, Color c)
        {
            int minX = Mathf.Clamp((int)(center.x - rx), 0, width);
            int maxX = Mathf.Clamp((int)(center.x + rx + 1), 0, width);
            int minY = Mathf.Clamp((int)(center.y - ry), 0, height);
            int maxY = Mathf.Clamp((int)(center.y + ry + 1), 0, height);

            float invRx2 = 1f / (rx * rx);
            float invRy2 = 1f / (ry * ry);

            for (int y = minY; y < maxY; y++)
            {
                float dy = y - center.y;
                float dy2 = dy * dy * invRy2;
                int row = y * width;
                for (int x = minX; x < maxX; x++)
                {
                    float dx = x - center.x;
                    if (dx * dx * invRx2 + dy2 <= 1.0f)
                    {
                        BlendPixel(row + x, c);
                    }
                }
            }
        }

        public void FillPolygon(Vector2[] pts, Color c)
        {
            if (pts == null || pts.Length < 3) return;

            float minYF = float.MaxValue, maxYF = float.MinValue;
            float minXF = float.MaxValue, maxXF = float.MinValue;
            for (int i = 0; i < pts.Length; i++)
            {
                if (pts[i].y < minYF) minYF = pts[i].y;
                if (pts[i].y > maxYF) maxYF = pts[i].y;
                if (pts[i].x < minXF) minXF = pts[i].x;
                if (pts[i].x > maxXF) maxXF = pts[i].x;
            }

            int minY = Mathf.Clamp((int)minYF, 0, height);
            int maxY = Mathf.Clamp((int)maxYF + 1, 0, height);

            float[] nodeX = new float[pts.Length];

            for (int y = minY; y < maxY; y++)
            {
                int nodes = 0;
                int j = pts.Length - 1;
                for (int i = 0; i < pts.Length; i++)
                {
                    if ((pts[i].y < y && pts[j].y >= y) || (pts[j].y < y && pts[i].y >= y))
                    {
                        nodeX[nodes++] = pts[i].x + (y - pts[i].y) / (pts[j].y - pts[i].y) * (pts[j].x - pts[i].x);
                    }
                    j = i;
                }

                System.Array.Sort(nodeX, 0, nodes);

                int row = y * width;
                for (int i = 0; i < nodes; i += 2)
                {
                    if (i + 1 >= nodes) break;
                    int startX = Mathf.Clamp((int)nodeX[i], 0, width);
                    int endX = Mathf.Clamp((int)nodeX[i + 1], 0, width);
                    for (int x = startX; x < endX; x++)
                    {
                        BlendPixel(row + x, c);
                    }
                }
            }
        }

        public void DrawLine(Vector2 p1, Vector2 p2, Color c, float thickness)
        {
            float dist = Vector2.Distance(p1, p2);
            if (dist < 1f) return;

            int steps = (int)(dist * 2);
            for (int i = 0; i <= steps; i++)
            {
                Vector2 pos = Vector2.Lerp(p1, p2, i / (float)steps);
                FillCircle(pos, thickness * 0.5f, c);
            }
        }

        public void DrawPolyline(Vector2[] pts, Color c, float thickness)
        {
            for (int i = 0; i < pts.Length - 1; i++)
            {
                DrawLine(pts[i], pts[i + 1], c, thickness);
            }
        }

        public void DrawFrame(Color c, int thickness)
        {
            FillRectangle(new RectInt(0, 0, width, thickness), c);
            FillRectangle(new RectInt(0, height - thickness, width, thickness), c);
            FillRectangle(new RectInt(0, 0, thickness, height), c);
            FillRectangle(new RectInt(width - thickness, 0, thickness, height), c);
        }

        public void DrawFrameInRect(RectInt r, Color c, int thickness)
        {
            FillRectangle(new RectInt(r.x, r.y, r.width, thickness), c);
            FillRectangle(new RectInt(r.x, r.yMax - thickness, r.width, thickness), c);
            FillRectangle(new RectInt(r.x, r.y, thickness, r.height), c);
            FillRectangle(new RectInt(r.xMax - thickness, r.y, thickness, r.height), c);
        }

        public void AddRandomParticles(int count, Rect area, Color c, float minR, float maxR, int seed)
        {
            var rand = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                float px = Mathf.Lerp(area.xMin, area.xMax, (float)rand.NextDouble());
                float py = Mathf.Lerp(area.yMin, area.yMax, (float)rand.NextDouble());
                float r = Mathf.Lerp(minR, maxR, (float)rand.NextDouble());
                FillCircle(new Vector2(px, py), r, c);
            }
        }

        public void ApplyVignette(float intensity)
        {
            Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
            float maxDist = center.magnitude;

            for (int y = 0; y < height; y++)
            {
                int row = y * width;
                for (int x = 0; x < width; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float factor = Mathf.Clamp01((dist / maxDist) * intensity);
                    if (factor > 0)
                    {
                        Color orig = pixels[row + x];
                        orig.r *= (1f - factor);
                        orig.g *= (1f - factor);
                        orig.b *= (1f - factor);
                        pixels[row + x] = orig;
                    }
                }
            }
        }

        private void BlendPixel(int idx, Color c)
        {
            if (idx < 0 || idx >= pixels.Length) return;
            if (c.a >= 0.999f)
            {
                pixels[idx] = c;
            }
            else
            {
                Color dst = pixels[idx];
                float outA = c.a + dst.a * (1f - c.a);
                if (outA > 0)
                {
                    pixels[idx] = new Color(
                        (c.r * c.a + dst.r * dst.a * (1f - c.a)) / outA,
                        (c.g * c.a + dst.g * dst.a * (1f - c.a)) / outA,
                        (c.b * c.a + dst.b * dst.a * (1f - c.a)) / outA,
                        outA
                    );
                }
            }
        }

        public void ApplyToTexture(Texture2D tex)
        {
            tex.SetPixels(pixels);
            tex.Apply();
        }
    }
}
#endif
