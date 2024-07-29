using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public string scene;
    [SerializeField] Animator animator;
    public float transitionTime = 1f;

    private void Awake() {
        animator.gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        StartCoroutine(loadLevel(scene));
    }
    
    IEnumerator loadLevel(string scene)
    {
        animator.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(scene);
    }

    public void triggerTransition()
    {
        StartCoroutine(loadLevel(scene));
    }
}
