using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Player;
using UnityEngine.EventSystems;
using static System.TimeZoneInfo;
using UnityEngine.SceneManagement;

public class BlackFigure : MonoBehaviour
{
    public float delayInSeconds;
    public float distance;
    public float durationInSeconds;
    public string scene;
    public float transitionTime;
    Tween tween;
    Animator animator;
    [SerializeField] Animator fade;

    [SerializeField] AudioSource forestAudioSource;
    [SerializeField] AudioClip forestAudioClip;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        Vector3 endPosition = transform.position + Vector3.right * distance;
        tween = transform.DOMove(endPosition, durationInSeconds).SetDelay(delayInSeconds);
        tween.onComplete = doThing;

        if (forestAudioSource != null) {
            forestAudioSource.clip = forestAudioClip;
            forestAudioSource.volume = 0.1f;
            forestAudioSource.loop = true;
            forestAudioSource.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (tween.IsPlaying())
        {
            animator.SetFloat("XInput", 1);
            animator.SetFloat("YInput", 0);
            animator.SetBool("Walking", true);
        }
        else
        {
            animator.SetBool("Walking", false);
        }
    }
    private void doThing()
    { 
     StartCoroutine(fadeBlack());
    }
    private IEnumerator fadeBlack()
    {
        fade.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(scene);
    }
}
