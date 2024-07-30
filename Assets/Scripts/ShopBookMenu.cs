using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopBookMenu : MonoBehaviour
{
    [SerializeField] GameObject leftPageFlip;
    [SerializeField] GameObject rightPageFlip;

    [SerializeField] TMP_Text itemsSoldText;
    [SerializeField] TMP_Text itemsCollectedText;
    [SerializeField] GameObject totalSlainText;
    [SerializeField] GameObject recipePagePrefab;
    [SerializeField] GameObject lorePagePrefab;

    [SerializeField] List<GameObject> chapters = new List<GameObject>();

    List<GameObject> textObjs = new List<GameObject>();

    private void Start() {
        // We want to load all the enemies slain texts right from the start because it wont ever increase while in the shop.
        totalSlainText.GetComponent<TMP_Text>().text = "Total Slain: " + PlayerPrefs.GetInt("total_enemies_killed", 0).ToString();

        var enemyTypes = Enum.GetValues(typeof(EnemyTypes));
        for (int i = 0; i < enemyTypes.Length; i++) {
            GameObject newText = Instantiate(totalSlainText, totalSlainText.transform.position + Vector3.down * 40 * (i+1), totalSlainText.transform.rotation);
            newText.transform.SetParent(totalSlainText.transform.parent, true);
            newText.transform.localScale = Vector3.one;
            string enemyTypeName = enemyTypes.GetValue(i).ToString().Replace("_", " ");
            newText.GetComponent<TMP_Text>().text = enemyTypeName + "s  Slain: " + PlayerPrefs.GetInt(enemyTypeName + "_killed", 0).ToString();
        }
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        ChangeChapter(chapters[0]);
        itemsSoldText.text = "Items  Sold: " + PlayerPrefs.GetInt("total_items_sold", 0).ToString();
        itemsCollectedText.text = "Items  Collected: " + PlayerPrefs.GetInt("total_items_collected", 0).ToString();
        StartCoroutine(LoadBook());
    }

    public void ManuallyLoadBook() {
        StartCoroutine(LoadBook());
    }

    IEnumerator LoadBook() {
        yield return new WaitForSeconds(0.1f);

        int pageIndex = 0;
        foreach (Recipe recipe in InventoryManager.currentInstance.globalCraftingRecipes.FindAll(x => x.showRecipeInBook)) {
            if (recipe.craftingIngredients.Count <= 0) continue;

            GameObject newPage = Instantiate(recipePagePrefab, transform.position + Vector3.down * 50, Quaternion.identity);
            newPage.name = recipe.craftedItem.name + " Recipe Page";
            newPage.transform.SetParent(transform.Find("Recipe Chapter"), true);
            newPage.transform.localScale = Vector3.one;
            if (pageIndex % 2 != 0) newPage.transform.position = new Vector2(newPage.transform.position.x * 1.75f, newPage.transform.position.y);

            newPage.transform.Find("Crafted Name").GetComponent<TMP_Text>().text = recipe.craftedItem.name;
            newPage.transform.Find("Crafted Icon").GetComponent<Image>().sprite = recipe.craftedItem.image;

            GameObject ingredientsGridObj = newPage.transform.Find("Ingredients Grid").gameObject;
            GameObject ingredientSprite = ingredientsGridObj.transform.GetChild(0).gameObject;
            for (int i = 0; i < recipe.craftingIngredients.Count; i++) {
                if (i < 1) {
                    ingredientSprite.GetComponent<Image>().sprite = recipe.craftingIngredients[i].image;
                    ingredientSprite.gameObject.name = recipe.craftingIngredients[i].name + " (Ingredient)";
                }
                else {
                    Image ingredient = Instantiate(ingredientSprite).GetComponent<Image>();
                    ingredient.gameObject.name = recipe.craftingIngredients[i].name + " (Ingredient)";
                    ingredient.transform.SetParent(ingredientsGridObj.transform); // We use a grid, which will do the layout for us. No need to move anything.
                    ingredient.transform.localScale = Vector3.one;
                    ingredient.sprite = recipe.craftingIngredients[i].image;
                }
            }

            if (pageIndex > 1) newPage.SetActive(false);
            pageIndex++;

            textObjs.Add(newPage);
        }

        pageIndex = 0;
        Debug.Log(InventoryManager.currentInstance.globalLoreList.FindAll(x => x.showLoreInBook).Count);
        foreach (Lore lore in InventoryManager.currentInstance.globalLoreList.FindAll(x => x.showLoreInBook)) {
            if (String.IsNullOrEmpty(lore.loreText)) continue;

            Debug.Log(lore.name);

            GameObject newPage = Instantiate(lorePagePrefab, transform.position + Vector3.up * 15, Quaternion.identity);
            newPage.name = lore.name + " Lore Page";
            newPage.transform.SetParent(transform.Find("Lore Chapter"), true);
            newPage.transform.localScale = Vector3.one;
            if (pageIndex % 2 != 0) newPage.transform.position = new Vector2(newPage.transform.position.x * 1.75f, newPage.transform.position.y);

            newPage.transform.GetChild(0).GetComponent<TMP_Text>().text = lore.loreText;

            if (pageIndex > 1) newPage.SetActive(false);
            pageIndex++;

            textObjs.Add(newPage);
        }

        // This is dumb but we're doubling all of the spaces in the text to make it for better readability.
        Transform loreChapter = transform.Find("Lore Chapter");
        for (int i = 0; i < loreChapter.childCount; i++) {
            TMP_Text text = loreChapter.GetChild(i).GetComponentInChildren<TMP_Text>();
            text.text = text.text.Replace(" ", "  ");
        }
    }

    private void OnDisable() {
        foreach(GameObject obj in textObjs) {
            Destroy(obj);
        }
        textObjs.Clear();
    }

    public void FlipPage(int direction) {
        Transform activeChapter = chapters.FirstOrDefault(x => x.activeSelf).transform;
        if(activeChapter == null) {
            Debug.LogWarning("No active chapter was found when flipping chapter in the shop book!");
            return;
        }

        int numPages = activeChapter.childCount;
        if (numPages <= 0) {
            Debug.LogWarning("No pages found when trying to flip page!");
        }

        int activePageIndex = GetIndexOfFirstActiveChild(activeChapter);
        if(activePageIndex < 0) { // If no active pages are found...
            activeChapter.GetChild(0).gameObject.SetActive(true);
            if(numPages > 1) activeChapter.GetChild(1).gameObject.SetActive(true);
            return;
        }

        // Theoretically, activePageIndex will always be an even number (first page), since we won't ever load the second page but not the first..

        if(direction >= 0 && activePageIndex < numPages-1) {
            // Disabling current
            activeChapter.GetChild(activePageIndex).gameObject.SetActive(false);
            if(activeChapter.transform.childCount > activePageIndex + 1) activeChapter.GetChild(activePageIndex + 1).gameObject.SetActive(false);

            // Going to next page
            activePageIndex += 2;

            // Enabling next
            activeChapter.GetChild(activePageIndex).gameObject.SetActive(true);
            if (activeChapter.transform.childCount > activePageIndex + 1) activeChapter.GetChild(activePageIndex + 1).gameObject.SetActive(true);

            Debug.Log(activePageIndex);
            Debug.Log(activePageIndex+2);
            Debug.Log(activeChapter.transform.childCount);

            leftPageFlip.SetActive(true);
            if (activePageIndex >= numPages - 2) rightPageFlip.SetActive(false);

        }
        else if (direction < 0  && activePageIndex > 1) {
            // Disabling current
            activeChapter.GetChild(activePageIndex).gameObject.SetActive(false);
            if (activeChapter.transform.childCount > activePageIndex + 1) activeChapter.GetChild(activePageIndex + 1).gameObject.SetActive(false);

            activePageIndex -= 2;

            // Enabling next
            activeChapter.GetChild(activePageIndex).gameObject.SetActive(true);
            if (activeChapter.transform.childCount > activePageIndex + 1) activeChapter.GetChild(activePageIndex + 1).gameObject.SetActive(true);

            rightPageFlip.SetActive(true);
            if (activePageIndex < 2) leftPageFlip.SetActive(false);
        }
    }

    public void ChangeChapter(GameObject chapterObj) {
        foreach(GameObject obj in chapters) { // Go through all chapters in the book
            for(int i = 0; i < obj.transform.childCount; i++) {
                obj.transform.GetChild(i).gameObject.SetActive(false); // Disable all the children of the chapter(the pages)
            }
            obj.SetActive(false); // Then disable the chapter itself
        }

        // Enable current chapter, and enable the flip buttons as needed
        chapterObj.SetActive(true); 
        leftPageFlip.SetActive(false);
        rightPageFlip.SetActive(chapterObj.transform.childCount > 2);

        // Enable the first child, and the second if it exists. To clarify, the children are the individual pages.
        if (chapterObj.transform.childCount > 0) chapterObj.transform.GetChild(0).gameObject.SetActive(true);
        if (chapterObj.transform.childCount > 1) chapterObj.transform.GetChild(1).gameObject.SetActive(true);
    }

    int GetIndexOfFirstActiveChild(Transform transform) { // Will be -1 if no active children found
        for(int i = 0; i < transform.childCount; i++) {
            if (transform.GetChild(i).gameObject.activeSelf) return i;
        }
        return -1;
    }
}
