using System;
using System.Collections.Generic;
using UnityEngine;

public class NovelController : MonoBehaviour
{
    [Header("Historia")]
    [SerializeField] private StoryNode startingNode;

    [Header("Audio (Opcional)")]
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
        if (startingNode != null)
        {
            PlayStory(startingNode);
        }
    }

    public void PlayStory(StoryNode startNode)
    {
        startingNode = startNode;
        isWaitingForChoice = false;
        isAtEnding = false;
        OnStoryRestarted?.Invoke();
        SetNode(startNode);
    }

    public void RestartStory()
    {
        if (startingNode != null)
        {
            PlayStory(startingNode);
        }
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
                SetNode(selectedChoice.targetNode);
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

    private void SetNode(StoryNode newNode)
    {
        if (newNode == null)
        {
            Debug.LogError("[NovelController] Intento de cargar un StoryNode nulo.");
            return;
        }

        currentNode = newNode;
        currentLineIndex = 0;

        // Actualizar locación y música si cambió
        if (currentNode.location != null && currentNode.location != currentLocation)
        {
            currentLocation = currentNode.location;
            OnLocationChanged?.Invoke(currentLocation);
            PlayLocationMusic(currentLocation);
        }

        // Mostrar primera línea o completar si no tiene líneas
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
        DialogueLine line = currentNode.dialogueLines[currentLineIndex];

        if (line.sfx != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(line.sfx);
        }

        OnDialogueLineDisplayed?.Invoke(line);
    }

    private void HandleNodeCompletion()
    {
        switch (currentNode.nodeType)
        {
            case NodeType.Linear:
                if (currentNode.nextLinearNode != null)
                {
                    SetNode(currentNode.nextLinearNode);
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
}
