using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class YellowRing : MonoBehaviour
{
    public bool redPillar = false;
    public bool bluePillar = false;
    public bool greenPillar = false;
    public string scene;
    CircleCollider2D colliderCircle;
    Light2D glow;
    [SerializeField] LevelLoader levelLoader;
    // Start is called before the first frame update
    void Start()
    {
        colliderCircle = GetComponent<CircleCollider2D>();
        glow = GetComponent<Light2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (redPillar && bluePillar && greenPillar) 
        { 
            glow.enabled = true;
            colliderCircle.enabled = true;
            redPillar = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Time.timeScale = 1;
            StartCoroutine(levelLoader.loadLevel("FinalScene"));
        }
    }

    public void activateRed()
    { redPillar=true; }
    public void activateGreen() 
    {  greenPillar=true; }

    public void activateBlue()
    { bluePillar=true; }


}
