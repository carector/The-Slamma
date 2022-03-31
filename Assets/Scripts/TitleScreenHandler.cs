using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class TitleScreenHandler : MonoBehaviour
{
    public AudioMixer mixer;
    RectTransform settingsScreen;
    Slider sfxSlider;
    Slider musicSlider;
    Slider ambSlider;


    Animator anim;
    AudioSource audio;
    public AudioClip[] sfx;
    public AudioSource ambience;
    public AudioSource music;
    public Animator skipTextAnim;
    bool loading;
    bool playing;
    bool pressedSkip;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;

        anim = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
        settingsScreen = GameObject.Find("SettingsScreen").GetComponent<RectTransform>();

        sfxSlider = GameObject.Find("SFXSlider").GetComponent<Slider>();
        ambSlider = GameObject.Find("AmbienceSlider").GetComponent<Slider>();
        musicSlider = GameObject.Find("MusicSlider").GetComponent<Slider>();
        LoadVolumeFromPlayerPrefs();
        GameObject.Find("FadeScreen").GetComponent<Animator>().Play("BlackFadeIn");
    }

    private void Update()
    {
        if (playing)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                LoadTutorial();
            else if (Input.anyKeyDown && !pressedSkip)
            {
                pressedSkip = true;
                skipTextAnim.Play("ShowSkipText");
            }
        }
    }

    public void ShowOptionsScreen()
    {
        settingsScreen.anchoredPosition = Vector2.zero;
    }

    public void HideOptionsScreen()
    {
        settingsScreen.anchoredPosition = Vector2.up * 1000;
    }

    public void UpdateMusicVolume()
    {
        if (musicSlider == null)
            return;

        int volume = (int)musicSlider.value * 4;

        if (volume == -40)
            volume = -80;

        PlayerPrefs.SetInt("SLAMMA_MUS_VOLUME", volume);
        mixer.SetFloat("MusicVolume", volume);
    }
    public void UpdateSFXVolume()
    {
        if (sfxSlider == null)
            return;

        int volume = (int)sfxSlider.value * 4;

        if (volume == -40)
            volume = -80;

        PlayerPrefs.SetInt("SLAMMA_SFX_VOLUME", volume);
        mixer.SetFloat("SFXVolume", volume);
    }

    public void UpdateAmbienceVolume()
    {
        if (ambSlider == null)
            return;

        int volume = (int)ambSlider.value * 4;

        if (volume == -40)
            volume = -80;

        PlayerPrefs.SetInt("SLAMMA_AMB_VOLUME", volume);
        mixer.SetFloat("AmbienceVolume", volume);
    }

    public void LoadVolumeFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("SLAMMA_AMB_VOLUME"))
        {
            musicSlider.value = PlayerPrefs.GetInt("SLAMMA_MUS_VOLUME") / 4;
            sfxSlider.value = PlayerPrefs.GetInt("SLAMMA_SFX_VOLUME") / 4;
            ambSlider.value = PlayerPrefs.GetInt("SLAMMA_AMB_VOLUME") / 4;
        }

        UpdateMusicVolume();
        UpdateSFXVolume();
        UpdateAmbienceVolume();
    }


    public void ResetSkipText()
    {
        pressedSkip = false;
    }

    public void StartCutscene()
    {
        playing = true;
        anim.Play("IntroCutscene");
    }

    public void PlaySFX(int index)
    {
        audio.PlayOneShot(sfx[index]);
    }

    public void LoadTutorial()
    {
        if (loading)
            return;

        loading = true;
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        GameObject.Find("FadeScreen").GetComponent<Animator>().Play("BlackFadeOut");
        yield return new WaitForSeconds(0.8f);
        Application.LoadLevel(1);
    }

    public void FadeOutAmbience()
    {
        StartCoroutine(FadeOutAmb());
    }

    IEnumerator FadeOutAmb()
    {
        while (ambience.volume > 0.25f)
        {
            music.volume -= Time.fixedDeltaTime;
            ambience.volume -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

}
