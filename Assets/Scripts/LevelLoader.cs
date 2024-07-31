using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public string scene;
    [SerializeField] Animator animator;
    public float transitionTime = 1f;

    public bool isTutorialArea = false;

    private void Start() {
        Time.timeScale = 1;
        animator.gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        StartCoroutine(loadLevel(scene));
    }
    
    public IEnumerator loadLevel(string scene)
    {
        if (isTutorialArea) {
            PlayerPrefs.SetInt("tutorial_completed", 1);
        }
        animator.SetTrigger("Start");
        yield return new WaitForSecondsRealtime(transitionTime);
        animator.ResetTrigger("Start");
        SceneManager.LoadScene(scene);
    }

    public void triggerTransition()
    {
        Time.timeScale = 1;

        StartCoroutine(loadLevel(scene));
    }
}
