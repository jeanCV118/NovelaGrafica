using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Novela/Character Data")]
public class CharacterData : ScriptableObject
{
    public string id;
    public string displayName;
    public Color nameColor = Color.white;
    public Sprite defaultPortrait;
}
