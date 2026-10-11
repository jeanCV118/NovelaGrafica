using System;
using System.Collections.Generic;
using UnityEngine;

public class NovelController : MonoBehaviour
{
    [Header("Historia")]
    [SerializeField] private StoryNode startingNode;
    [SerializeField] private List<StoryNode> allStoryNodes = new List<StoryNode>();
    [SerializeField] private bool autoStartStory = false;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private StoryNode currentNode;
    private int currentLineIndex = 0;
    private LocationData currentLocation;
    private bool isWaitingForChoice = false;
    private bool isAtEnding = false;

    // Eventos para desacoplar la lógica de la interfaz de usuario
    public event Action<LocationData> OnLocationChanged;
    public event Action<DialogueLine> OnDialogueLineDisplayed;
    public event Action<IReadOnlyList<Choice>> OnChoicesPresented;
    public event Action<EndingType> OnEndingReached;
    public event Action OnStoryRestarted;

    // Propiedades públicas de consulta de estado
    public StoryNode CurrentNode => currentNode;
    public int CurrentLineIndex => currentLineIndex;
    public bool IsWaitingForChoice => isWaitingForChoice;
    public bool IsAtEnding => isAtEnding;

    private void Start()
    {
        if (autoStartStory && startingNode != null)
        {
            PlayStory(startingNode);
        }
    }

    public void PlayStory(StoryNode startNode = null)
    {
        if (startNode != null)
        {
            startingNode = startNode;
        }

        if (startingNode == null)
        {
            Debug.LogError("[NovelController] No hay startingNode asignado para iniciar la historia.");
            return;
        }

        isWaitingForChoice = false;
        isAtEnding = false;
        OnStoryRestarted?.Invoke();
        SetNode(startingNode, 0);
    }

    public void ContinueStory()
    {
        if (!SaveSystem.HasSaveData())
        {
            PlayStory(startingNode);
            return;
        }

        var save = SaveSystem.LoadGame();
        StoryNode nodeToLoad = FindNodeByID(save.nodeID);

        if (nodeToLoad != null)
        {
            isWaitingForChoice = false;
            isAtEnding = false;
            OnStoryRestarted?.Invoke();
            SetNode(nodeToLoad, save.lineIndex);
        }
        else
        {
            Debug.LogWarning($"[NovelController] Nodo guardado '{save.nodeID}' no encontrado. Iniciando desde el principio.");
            PlayStory(startingNode);
        }
    }

    public void RestartStory()
    {
        SaveSystem.ClearSave();
        PlayStory(startingNode);
    }

    public void Advance()
    {
        if (isWaitingForChoice || isAtEnding || currentNode == null)
        {
            return;
        }

        currentLineIndex++;

        if (currentNode.dialogueLines != null && currentLineIndex < currentNode.dialogueLines.Count)
        {
            DisplayCurrentLine();
        }
        else
        {
            HandleNodeCompletion();
        }
    }

    public void SelectChoice(int choiceIndex)
    {
        if (!isWaitingForChoice || currentNode == null)
        {
            return;
        }

        if (currentNode.choices != null && choiceIndex >= 0 && choiceIndex < currentNode.choices.Count)
        {
            Choice selectedChoice = currentNode.choices[choiceIndex];
            if (selectedChoice.targetNode != null)
            {
                isWaitingForChoice = false;
                SetNode(selectedChoice.targetNode, 0);
            }
            else
            {
                Debug.LogWarning($"[NovelController] La opción ({choiceIndex}) no tiene 'targetNode' asignado.");
            }
        }
        else
        {
            Debug.LogWarning($"[NovelController] Índice de opción inválido: {choiceIndex}");
        }
    }

    private void SetNode(StoryNode newNode, int lineIndex = 0)
    {
        if (newNode == null)
        {
            Debug.LogError("[NovelController] Intento de cargar un StoryNode nulo.");
            return;
        }

        currentNode = newNode;
        currentLineIndex = Mathf.Clamp(lineIndex, 0, (currentNode.dialogueLines != null && currentNode.dialogueLines.Count > 0) ? currentNode.dialogueLines.Count - 1 : 0);

        // Actualizar locación y música si cambió
        if (currentNode.location != null && currentNode.location != currentLocation)
        {
            currentLocation = currentNode.location;
            OnLocationChanged?.Invoke(currentLocation);
            PlayLocationMusic(currentLocation);
        }

        // Mostrar línea o completar
        if (currentNode.dialogueLines != null && currentNode.dialogueLines.Count > 0)
        {
            DisplayCurrentLine();
        }
        else
        {
            HandleNodeCompletion();
        }
    }

    private void DisplayCurrentLine()
    {
        if (currentNode == null || currentNode.dialogueLines == null || currentLineIndex >= currentNode.dialogueLines.Count)
            return;

        DialogueLine line = currentNode.dialogueLines[currentLineIndex];

        // Guardar progreso automáticamente
        if (currentNode.nodeType != NodeType.Ending)
        {
            SaveSystem.SaveGame(currentNode.nodeID, currentLineIndex);
        }

        // Detectar si requiere sacudida de pantalla (sismo / réplica / rugido de Buziraco)
        CheckForScreenShake(line);

        // Sonido SFX de la línea
        if (line.sfx != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(line.sfx);
        }

        OnDialogueLineDisplayed?.Invoke(line);
    }

    private void CheckForScreenShake(DialogueLine line)
    {
        bool isTremor = false;
        float intensity = 14f;
        float duration = 0.5f;

        if (line.speaker != null && line.speaker.displayName.IndexOf("Buziraco", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            isTremor = true;
            intensity = 20f;
            duration = 0.7f;
        }
        else if (!string.IsNullOrEmpty(line.text))
        {
            string lower = line.text.ToLower();
            if (lower.Contains("terremoto") || lower.Contains("sismo") || lower.Contains("temblor") ||
                lower.Contains("sacude") || lower.Contains("escombros") || lower.Contains("grieta") ||
                lower.Contains("réplica") || lower.Contains("rugido"))
            {
                isTremor = true;
                intensity = 16f;
                duration = 0.6f;
            }
        }

        if (isTremor)
        {
            ScreenShaker.Instance?.TriggerShake(duration, intensity);
        }
    }

    private void HandleNodeCompletion()
    {
        switch (currentNode.nodeType)
        {
            case NodeType.Linear:
                if (currentNode.nextLinearNode != null)
                {
                    SetNode(currentNode.nextLinearNode, 0);
                }
                else
                {
                    Debug.LogWarning($"[NovelController] El nodo lineal '{currentNode.nodeID}' no tiene 'nextLinearNode'.");
                }
                break;

            case NodeType.Decision:
                isWaitingForChoice = true;
                OnChoicesPresented?.Invoke(currentNode.choices);
                break;

            case NodeType.Ending:
                isAtEnding = true;
                // Registrar el final alcanzado en el gestor de finales
                EndingsManager.Instance?.UnlockEnding(currentNode.nodeID);
                // Como se llegó a un final, la partida activa concluye
                SaveSystem.ClearSave();
                OnEndingReached?.Invoke(currentNode.endingType);
                break;
        }
    }

    private void PlayLocationMusic(LocationData location)
    {
        if (musicSource == null || location == null)
        {
            return;
        }

        if (location.ambientMusic != null)
        {
            if (musicSource.clip != location.ambientMusic || !musicSource.isPlaying)
            {
                musicSource.clip = location.ambientMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
        }
        else
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
    }

    private StoryNode FindNodeByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        // Primero buscar en la lista registrada
        foreach (var node in allStoryNodes)
        {
            if (node != null && (node.nodeID == id || node.name == id))
            {
                return node;
            }
        }

        // Si coincide con el startingNode
        if (startingNode != null && (startingNode.nodeID == id || startingNode.name == id))
        {
            return startingNode;
        }

        return null;
    }

    public void SetAllStoryNodes(IEnumerable<StoryNode> nodes)
    {
        allStoryNodes = new List<StoryNode>(nodes);
    }
}
