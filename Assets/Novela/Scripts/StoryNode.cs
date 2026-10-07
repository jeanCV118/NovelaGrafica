using System.Collections.Generic;
using UnityEngine;

public enum NodeType
{
    Linear,
    Decision,
    Ending
}

public enum EndingType
{
    Bueno,
    Alternativo
}

[System.Serializable]
public struct DialogueLine
{
    public CharacterData speaker;
    public Sprite customPortrait;
    [TextArea(3, 10)]
    public string text;
    public AudioClip sfx;
}

[System.Serializable]
public struct Choice
{
    public string choiceText;
    public StoryNode targetNode;
}

[CreateAssetMenu(fileName = "NewStoryNode", menuName = "Novela/Story Node")]
public class StoryNode : ScriptableObject
{
    public string nodeID;
    public LocationData location;
    
    public List<DialogueLine> dialogueLines;
    
    public NodeType nodeType;
    
    // Solo se utiliza si nodeType es NodeType.Linear
    public StoryNode nextLinearNode;
    
    // Solo se utiliza si nodeType es NodeType.Decision
    public List<Choice> choices;
    
    // Solo se utiliza si nodeType es NodeType.Ending
    public EndingType endingType;
}
