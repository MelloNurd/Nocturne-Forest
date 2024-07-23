using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public string scene;
    [SerializeField] Animator animator;
    public float transitionTime = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        StartCoroutine(loadLevel(scene));
    }
    
    IEnumerator loadLevel(string scene)
    {
        animator.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(scene);
    }
}
