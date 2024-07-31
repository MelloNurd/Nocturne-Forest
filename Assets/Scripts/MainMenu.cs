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
    [SerializeField] GameObject darkness;

    string sceneToLoad;

    AudioSource audioSource;

    [SerializeField] AudioClip buttonClickClip;

    public void PlaySound(AudioClip audioClip, float volume = 1f, float pitch = 1f) {
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(audioClip);
        Debug.Log(audioSource.volume);
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        darkness.SetActive(true);
        int hasPlayed = PlayerPrefs.GetInt("tutorial_completed", 0);
        Debug.Log(hasPlayed);
        newGameContinueButton.GetComponentInChildren<TMP_Text>().text = (hasPlayed != 0) ? "Continue" : "New Game";
        sceneToLoad = (hasPlayed != 0) ? "Shop" : "Tutorial";
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
    }

    IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSecondsRealtime(1);
        SceneManager.LoadScene(sceneToLoad);
    }
}
