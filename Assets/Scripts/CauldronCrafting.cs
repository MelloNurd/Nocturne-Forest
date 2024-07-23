using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CauldronCrafting : MonoBehaviour
{
    public List<Item> addedIngredients = new List<Item>();
    
    InventorySlot inputSlot;

    private void Awake() {
        inputSlot = transform.Find("InventorySlot").GetComponent<InventorySlot>();
    }

    public void AddInputItemToIngredients() {
        StartCoroutine(AddInputItem());
    }

    IEnumerator AddInputItem() {
        yield return new WaitForSeconds(0.1f); // Have to wait a second for the InventorySlot to register its item
        InventoryItem newItem = inputSlot.GetItemInSlot();
        if (newItem != null) {
            Destroy(inputSlot.transform.GetChild(0).gameObject);

            AddItemToIngredients(newItem.item);

            Debug.Log("Current ingredients: ");
            foreach (Item item in addedIngredients) {
                Debug.Log(item.name);
            }
        }
    }

    public void AddItemToIngredients(Item item) {
        addedIngredients.Add(item);
    }

    public void ClearIngredients() {
        addedIngredients.Clear();
    }

    public void TryCraft() {
        Recipe currentRecipe = InventoryManager.GetAllCauldronRecipes().FirstOrDefault(x => CompareIngredientLists(x.craftingIngredients, addedIngredients));
        if(currentRecipe == null) {
            Debug.Log("Invalid recipe.");
        }
        else {
            Debug.Log("Found recipe! Player will craft: " + currentRecipe.craftedItem.name);
            ClearIngredients();
        }
    }

    bool CompareIngredientLists(List<Item> A, List<Item> B) {
        if (A.Count != B.Count) {
            return false;
        }

        List<Item> ASort = A.ToList();
        ASort.Sort((x, y) => x.name.CompareTo(y.name));
        List<Item> BSort = B.ToList();
        BSort.Sort((x, y) => x.name.CompareTo(y.name));

        for (int i = 0; i < ASort.Count; i++) {
            if (ASort[i] != BSort[i]) {
                return false;
            }
        }

        return true;
    }
}
