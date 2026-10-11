using System;
using System.Collections.Generic;
using UnityEngine;

public class EndingsManager : MonoBehaviour
{
    public static EndingsManager Instance { get; private set; }

    [System.Serializable]
    public class EndingData
    {
        public string id;
        public string title;
        [TextArea(3, 6)]
        public string description;
        public Sprite cardImage;
    }

    [Header("Registro de Finales")]
    [SerializeField] private List<EndingData> endings = new List<EndingData>();

    public const string BuenoID = "FINAL_BUENO";
    public const string AlternativoID = "FINAL_ALTERNATIVO";

    private const string PrefsPrefix = "Novela_Ending_Unlocked_";

    public event Action<string> OnEndingUnlocked;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UnlockEnding(string endingID)
    {
        if (string.IsNullOrEmpty(endingID)) return;

        string key = PrefsPrefix + endingID;
        if (PlayerPrefs.GetInt(key, 0) == 0)
        {
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
            Debug.Log($"<color=#FFD700><b>[EndingsManager]</b> ¡Final desbloqueado y registrado: {endingID}!</color>");
            OnEndingUnlocked?.Invoke(endingID);
        }
    }

    public bool IsEndingUnlocked(string endingID)
    {
        if (string.IsNullOrEmpty(endingID)) return false;
        return PlayerPrefs.GetInt(PrefsPrefix + endingID, 0) == 1;
    }

    public int GetUnlockedCount()
    {
        int count = 0;
        foreach (var ending in endings)
        {
            if (IsEndingUnlocked(ending.id))
            {
                count++;
            }
        }
        return count;
    }

    public int GetTotalCount()
    {
        return endings.Count;
    }

    public IReadOnlyList<EndingData> GetAllEndings()
    {
        return endings;
    }

    public EndingData GetEnding(string endingID)
    {
        return endings.Find(e => e.id.Equals(endingID, StringComparison.OrdinalIgnoreCase));
    }
}
