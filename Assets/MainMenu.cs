using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    [SerializeField] Button newGameContinueButton;

    string sceneToLoad;


    // Start is called before the first frame update
    void Start()
    {
        int hasPlayed = PlayerPrefs.GetInt("tutorial_completed", 0);
        newGameContinueButton.GetComponentInChildren<TMP_Text>().text = (hasPlayed != 0) ? "Continue" : "New Game";
        sceneToLoad = (hasPlayed != 0) ? "Shop" : "Tutorial";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlay() {

    }
}
