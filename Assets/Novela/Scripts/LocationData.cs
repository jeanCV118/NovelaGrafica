using UnityEngine;

[CreateAssetMenu(fileName = "NewLocationData", menuName = "Novela/Location Data")]
public class LocationData : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite backgroundSprite;
    public AudioClip ambientMusic;
}
