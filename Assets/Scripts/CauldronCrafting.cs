using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;

public class CauldronCrafting : MonoBehaviour
{
    public List<Item> addedIngredients = new List<Item>();

    [SerializeField] Image cauldronImage;

    [SerializeField] Image movingItem;

    [SerializeField] Sprite blueWaterSprite;
    [SerializeField] Sprite greenWaterSprite;
    
    InventorySlot inputSlot;

    Sequence sequence;

    private void Awake() {
        inputSlot = transform.Find("InventorySlot").GetComponent<InventorySlot>();
        movingItem.rectTransform.localScale = Vector3.one * 0.5f;
    }

    public void AddInputItemToIngredients() {
        StartCoroutine(AddInputItem());
    }

    IEnumerator AddInputItem() {
        yield return new WaitForSeconds(0.1f); // Have to wait a second for the InventorySlot to register its item
        InventoryItem newItem = inputSlot.GetItemInSlot();
        Debug.Log("test");
        movingItem.sprite = newItem.item.image;
        movingItem.rectTransform.position = inputSlot.transform.position;
        movingItem.rectTransform.localScale = Vector3.one * 0.5f;
        if (sequence.IsActive()) sequence.Kill();
        sequence.Append(movingItem.rectTransform.DOScale(Vector3.one, 0.25f));
        sequence.Append(movingItem.rectTransform.DOMove(cauldronImage.rectTransform.position, 0.5f));
        sequence.Append(movingItem.rectTransform.DOScale(Vector3.one * 0.5f, 0.25f).SetDelay(0.25f));
        sequence.Play();
        if (newItem != null) {
            Destroy(inputSlot.transform.GetChild(0).gameObject);

            StartCoroutine(AddItemToIngredients(newItem.item));
        }
    }

    public IEnumerator AddItemToIngredients(Item item) {
        yield return new WaitForSeconds(0.5f);
        addedIngredients.Add(item);
        cauldronImage.sprite = greenWaterSprite;
    }

    public void ClearIngredients() {
        addedIngredients.Clear();
        cauldronImage.sprite = blueWaterSprite;
    }

    public void TryCraft() {
        if (addedIngredients.Count <= 0) return;

        Recipe currentRecipe = InventoryManager.GetAllCauldronRecipes().FirstOrDefault(x => CompareIngredientLists(x.craftingIngredients, addedIngredients));

        if (currentRecipe == null) {
            InventoryManager.itemLookup.TryGetValue("Vile Concoction", out Item crafted);
            movingItem.sprite = crafted.image;
            if (crafted != null) StartCoroutine(CraftItem(crafted));
        }
        else {
            InventoryManager.itemLookup.TryGetValue(currentRecipe.craftedItem.name, out Item crafted);
            movingItem.sprite = crafted.image;
            if (crafted != null) StartCoroutine(CraftItem(crafted));
        }

        movingItem.rectTransform.position = cauldronImage.rectTransform.position;
        movingItem.rectTransform.localScale = Vector3.one * 0.5f;
        if (sequence.IsActive()) sequence.Kill();
        sequence.Append(movingItem.rectTransform.DOScale(Vector3.one, 0.25f));
        sequence.Append(movingItem.rectTransform.DOMove(inputSlot.transform.position, 0.5f));
        sequence.Append(movingItem.rectTransform.DOScale(Vector3.one * 0.5f, 0.25f).SetDelay(0.25f));
        sequence.Play();

        ClearIngredients();
    }

    IEnumerator CraftItem(Item outputItem) {
        yield return new WaitForSeconds(0.5f);
        InventoryManager.currentInstance.SpawnNewItem(outputItem, inputSlot);
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
