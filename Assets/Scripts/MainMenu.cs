using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class MainMenu : MonoBehaviour
{
    [SerializeField] Button newGameContinueButton;
    [SerializeField] Animator animator;

    string sceneToLoad;

    AudioSource audioSource;

    [SerializeField] AudioSource musicAudioSource;
    [SerializeField] AudioClip musicClip;

    [SerializeField] AudioClip buttonClickClip;

    public void PlaySound(AudioClip audioClip, float volume = 1f, float pitch = 1f) {
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(audioClip);
        Debug.Log(audioSource.volume);
    }

    private void Awake() {
        Time.timeScale = 1f;
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator.SetTrigger("Start");
        int hasPlayed = PlayerPrefs.GetInt("tutorial_completed", 0);
        Debug.Log(hasPlayed);
        newGameContinueButton.GetComponentInChildren<TMP_Text>().text = (hasPlayed != 0) ? "Continue" : "New Game";
        sceneToLoad = (hasPlayed != 0) ? "Shop" : "Tutorial";

        if (musicAudioSource != null) {
            musicAudioSource.clip = musicClip;
            musicAudioSource.volume = 0.2f;
            musicAudioSource.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayButtonSound() {
        PlaySound(buttonClickClip, 1, Random.Range(0.8f, 1.2f));
    }

    public void OnPlay() {
        
        StartCoroutine(LoadSceneWithDelay());
    }

    public void DeleteSaveData() {
        PlayerPrefs.DeleteAll();
        newGameContinueButton.GetComponentInChildren<TMP_Text>().text = "New Game";
        sceneToLoad = "Tutorial";
    }

    IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSecondsRealtime(1);
        SceneManager.LoadScene(sceneToLoad);
    }
}
