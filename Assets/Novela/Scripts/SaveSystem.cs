using UnityEngine;

public static class SaveSystem
{
    private const string SaveKeyNodeID = "Novela_Save_NodeID";
    private const string SaveKeyLineIndex = "Novela_Save_LineIndex";
    private const string SaveKeyHasData = "Novela_Save_HasData";

    public struct SaveData
    {
        public string nodeID;
        public int lineIndex;
    }

    public static bool HasSaveData()
    {
        return PlayerPrefs.GetInt(SaveKeyHasData, 0) == 1;
    }

    public static void SaveGame(string nodeID, int lineIndex)
    {
        if (string.IsNullOrEmpty(nodeID)) return;

        PlayerPrefs.SetString(SaveKeyNodeID, nodeID);
        PlayerPrefs.SetInt(SaveKeyLineIndex, lineIndex);
        PlayerPrefs.SetInt(SaveKeyHasData, 1);
        PlayerPrefs.Save();
    }

    public static SaveData LoadGame()
    {
        return new SaveData
        {
            nodeID = PlayerPrefs.GetString(SaveKeyNodeID, "01_Introduccion"),
            lineIndex = PlayerPrefs.GetInt(SaveKeyLineIndex, 0)
        };
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(SaveKeyNodeID);
        PlayerPrefs.DeleteKey(SaveKeyLineIndex);
        PlayerPrefs.DeleteKey(SaveKeyHasData);
        PlayerPrefs.Save();
    }
}
