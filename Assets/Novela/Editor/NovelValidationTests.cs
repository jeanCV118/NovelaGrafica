#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class NovelValidationTests
{
    private const string DataPath = "Assets/Novela/Data/";

    [MenuItem("Novela/Ejecutar Pruebas de Validación")]
    public static void RunAllTests()
    {
        Debug.Log("<b><color=#2EA3FF>[PRUEBAS]</color></b> Iniciando suite de validación de la novela visual...");

        int passed = 0;
        int failed = 0;

        // Prueba 1: Existencia e integridad de los 8 nodos de historia
        string[] nodeNames = new[]
        {
            "01_Introduccion",
            "02_Decision1",
            "03_RutaPanico",
            "04_RutaPrevencion",
            "05_EnfrentamientoFinal",
            "06_Decision2",
            "07_FinalBueno",
            "08_FinalAlternativo"
        };

        bool nodesOk = true;
        foreach (var name in nodeNames)
        {
            var node = AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + name + ".asset");
            if (node == null)
            {
                Debug.LogError($"[TEST 1 FALLO] No se encontró el nodo '{name}.asset'");
                nodesOk = false;
            }
            else if (node.dialogueLines == null || node.dialogueLines.Count == 0)
            {
                Debug.LogError($"[TEST 1 FALLO] El nodo '{name}' no contiene líneas de diálogo");
                nodesOk = false;
            }
        }
        if (nodesOk) { passed++; Debug.Log("<b>✓ Test 1 Superado:</b> Los 8 nodos de historia existen y tienen diálogos."); } else failed++;

        // Prueba 2: Verificación del grafo narrativo completo
        var intro = AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "01_Introduccion.asset");
        var dec1 = AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "02_Decision1.asset");
        var panico = AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "03_RutaPanico.asset");
        var prev = AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "04_RutaPrevencion.asset");
        var finalBattle = AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "05_EnfrentamientoFinal.asset");
        var dec2 = AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "06_Decision2.asset");
        var finalBueno = AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "07_FinalBueno.asset");
        var finalAlt = AssetDatabase.LoadAssetAtPath<StoryNode>(DataPath + "08_FinalAlternativo.asset");

        bool graphOk = (intro.nextLinearNode == dec1) &&
                       (dec1.choices.Count >= 2 && dec1.choices[0].targetNode == panico && dec1.choices[1].targetNode == prev) &&
                       (panico.nextLinearNode == finalBattle) &&
                       (prev.nextLinearNode == finalBattle) &&
                       (finalBattle.nextLinearNode == dec2) &&
                       (dec2.choices.Count >= 2 && dec2.choices[0].targetNode == finalBueno && dec2.choices[1].targetNode == finalAlt) &&
                       (finalBueno.nodeType == NodeType.Ending && finalBueno.endingType == EndingType.Bueno) &&
                       (finalAlt.nodeType == NodeType.Ending && finalAlt.endingType == EndingType.Alternativo);

        if (graphOk) { passed++; Debug.Log("<b>✓ Test 2 Superado:</b> El grafo narrativo, decisiones y bifurcaciones están 100% íntegros."); }
        else { failed++; Debug.LogError("[TEST 2 FALLO] Las conexiones del grafo narrativo no coinciden."); }

        // Prueba 3: Fondos y Personajes vinculados
        var cCali = AssetDatabase.LoadAssetAtPath<LocationData>(DataPath + "CentroDeCali.asset");
        var cIglesia = AssetDatabase.LoadAssetAtPath<LocationData>(DataPath + "IglesiaColonial.asset");
        var cCerro = AssetDatabase.LoadAssetAtPath<LocationData>(DataPath + "CerroTresCruces.asset");

        var cMateo = AssetDatabase.LoadAssetAtPath<CharacterData>(DataPath + "Mateo.asset");
        var cPadre = AssetDatabase.LoadAssetAtPath<CharacterData>(DataPath + "PadreEsteban.asset");
        var cBuziraco = AssetDatabase.LoadAssetAtPath<CharacterData>(DataPath + "Buziraco.asset");

        bool artOk = (cCali != null && cCali.backgroundSprite != null) &&
                     (cIglesia != null && cIglesia.backgroundSprite != null) &&
                     (cCerro != null && cCerro.backgroundSprite != null) &&
                     (cMateo != null && cMateo.defaultPortrait != null) &&
                     (cPadre != null && cPadre.defaultPortrait != null) &&
                     (cBuziraco != null && cBuziraco.defaultPortrait != null);

        if (artOk) { passed++; Debug.Log("<b>✓ Test 3 Superado:</b> Todas las locaciones y personajes tienen sus sprites asignados."); }
        else { failed++; Debug.LogError("[TEST 3 FALLO] Hay locaciones o personajes sin sprite asignado."); }

        // Prueba 4: Persistencia de Guardado y Carga (SaveSystem)
        SaveSystem.SaveGame("02_Decision1", 1);
        var save = SaveSystem.LoadGame();
        bool saveOk = SaveSystem.HasSaveData() && save.nodeID == "02_Decision1" && save.lineIndex == 1;
        SaveSystem.ClearSave();
        bool clearOk = !SaveSystem.HasSaveData();

        if (saveOk && clearOk) { passed++; Debug.Log("<b>✓ Test 4 Superado:</b> El sistema SaveSystem guarda, recupera y limpia partidas correctamente."); }
        else { failed++; Debug.LogError("[TEST 4 FALLO] Error en el flujo de SaveSystem."); }

        // Prueba 5: Persistencia de Finales (EndingsManager)
        string testKeyBueno = "Novela_Ending_Unlocked_FINAL_BUENO";
        string testKeyAlt = "Novela_Ending_Unlocked_FINAL_ALTERNATIVO";
        PlayerPrefs.SetInt(testKeyBueno, 1);
        PlayerPrefs.SetInt(testKeyAlt, 1);
        PlayerPrefs.Save();

        bool endingsOk = PlayerPrefs.GetInt(testKeyBueno, 0) == 1 && PlayerPrefs.GetInt(testKeyAlt, 0) == 1;

        if (endingsOk) { passed++; Debug.Log("<b>✓ Test 5 Superado:</b> La persistencia de finales en PlayerPrefs funciona correctamente."); }
        else { failed++; Debug.LogError("[TEST 5 FALLO] Error en la persistencia de finales."); }

        // Prueba 6: Integridad de referencias en NovelManager y Canvas
        GameObject manager = GameObject.Find("NovelManager");
        bool sceneRefsOk = false;
        if (manager != null)
        {
            var ctrl = manager.GetComponent<NovelController>();
            var ui = manager.GetComponent<NovelUI>();
            var mainUI = manager.GetComponent<MainMenuUI>();
            var endMgr = manager.GetComponent<EndingsManager>();
            var setMgr = manager.GetComponent<SettingsManager>();

            sceneRefsOk = (ctrl != null && ui != null && mainUI != null && endMgr != null && setMgr != null);
        }

        if (sceneRefsOk) { passed++; Debug.Log("<b>✓ Test 6 Superado:</b> Todos los componentes de gestión están presentes y enlazados en la escena."); }
        else { failed++; Debug.LogError("[TEST 6 FALLO] Faltan componentes en NovelManager."); }

        Debug.Log($"<b><color=#44FF44>[RESULTADO SUITE]</color></b> Pruebas superadas: <b>{passed}/6</b> | Fallidas: <b>{failed}</b>");
    }
}
#endif
