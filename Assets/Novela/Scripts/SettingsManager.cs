using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Referencias de UI")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider textSpeedSlider;

    [Header("Fuentes de Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private const string PrefsMusic = "Novela_Settings_Music";
    private const string PrefsSFX = "Novela_Settings_SFX";
    private const string PrefsSpeed = "Novela_Settings_Speed";

    public float TextSpeed { get; private set; } = 0.025f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadSettings();
    }

    private void Start()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        if (textSpeedSlider != null)
        {
            textSpeedSlider.onValueChanged.AddListener(OnTextSpeedChanged);
        }
    }

    private void LoadSettings()
    {
        float musicVol = PlayerPrefs.GetFloat(PrefsMusic, 0.8f);
        float sfxVol = PlayerPrefs.GetFloat(PrefsSFX, 1.0f);
        float speed = PlayerPrefs.GetFloat(PrefsSpeed, 0.025f);

        TextSpeed = speed;

        if (musicSource != null) musicSource.volume = musicVol;
        if (sfxSource != null) sfxSource.volume = sfxVol;

        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVol;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVol;
        if (textSpeedSlider != null) textSpeedSlider.value = speed;
    }

    public void OnMusicVolumeChanged(float val)
    {
        if (musicSource != null) musicSource.volume = val;
        PlayerPrefs.SetFloat(PrefsMusic, val);
        PlayerPrefs.Save();
    }

    public void OnSFXVolumeChanged(float val)
    {
        if (sfxSource != null) sfxSource.volume = val;
        PlayerPrefs.SetFloat(PrefsSFX, val);
        PlayerPrefs.Save();
    }

    public void OnTextSpeedChanged(float val)
    {
        TextSpeed = val;
        PlayerPrefs.SetFloat(PrefsSpeed, val);
        PlayerPrefs.Save();
    }
}
