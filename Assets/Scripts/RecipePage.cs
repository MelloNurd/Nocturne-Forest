using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecipePage : Interactable
{
    // This is for pages to pick up in the world, not the pages that show up in the book

    public Recipe recipeToUnlock;

    protected override void Awake() {
        base.Awake();
    }

    // Start is called before the first frame update
    protected override void Start() {
        base.Start();

        if (recipeToUnlock == null) Debug.LogError("No recipe assigned to page: " + gameObject.name);
    }

    // Update is called once per frame
    protected override void Update() {
        base.Update();
    }

    public override void Interact() {
        if (!canInteract) return;

        PlayerPrefs.SetInt("RecipePage_" + recipeToUnlock.name, 1);

        gameObject.SetActive(false);
    }
}
