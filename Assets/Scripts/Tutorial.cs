using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Player player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    public void Switch()
    {
        Debug.Log("Triggered");
        animator.SetTrigger("SwitchModel");
        spriteRenderer.color = Color.white;
        player.canAttack = true;
        player.canRoll = true;
    }
}
