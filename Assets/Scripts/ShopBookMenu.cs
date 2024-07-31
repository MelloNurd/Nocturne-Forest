using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopBookMenu : MonoBehaviour
{
    [Header("Book Components")]
    [SerializeField] GameObject leftPageFlip;
    [SerializeField] GameObject rightPageFlip;
    [SerializeField] List<GameObject> chapters = new List<GameObject>();

    [Header("Stats Page")]
    [SerializeField] TMP_Text itemsSoldText;
    [SerializeField] TMP_Text itemsCollectedText;
    [SerializeField] TMP_Text recipesCollectedText;
    [SerializeField] TMP_Text loreCollectedText;

    [SerializeField] GameObject totalSlainText;
    [SerializeField] GameObject recipePagePrefab;
    [SerializeField] GameObject lorePagePrefab;

    [Header("Upgrades")]
    [SerializeField] GameObject healthUpgradeObj;
    [SerializeField] GameObject speedUpgradeObj;
    [SerializeField] GameObject attackDmgUpgradeObj;
    [SerializeField] GameObject shopUpgradeObj;

    [SerializeField] AudioClip pageFlipClip;

    int baseCost = 25;
    int costLevelMultiplier = 18; // Basically... Upgrade Cost = baseCost + (costLevelMultiplier * (level-1))
    public bool resetUpgradesOnStart = false;

    Color UpgradeBarColorOff = new Color(0.52f, 0.34f, 0.34f, 0.41f);
    Color UpgradeBarColorOn = new Color(0.52f, 0.34f, 0.34f, 1);

    List<GameObject> textObjs = new List<GameObject>(); // This is used to store/delete the duplicated text on the various pages

    Player player;

    private void Awake() {
        if (resetUpgradesOnStart) {
            PlayerPrefs.SetInt("player_stats_health", 1);
            PlayerPrefs.SetInt("player_stats_speed", 1);
            PlayerPrefs.SetInt("player_stats_atkdmg", 1);
            PlayerPrefs.SetInt("player_stats_shopupg", 1);
        }
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    private void Start() {
        // We want to load all the enemies slain texts right from the start because it wont ever increase while in the shop.
        totalSlainText.GetComponent<TMP_Text>().text = "Total Slain: " + PlayerPrefs.GetInt("total_enemies_killed", 0).ToString();

        var enemyTypes = Enum.GetValues(typeof(EnemyTypes));
        for (int i = 0; i < enemyTypes.Length; i++) {
            GameObject newText = Instantiate(totalSlainText, totalSlainText.transform.position + Vector3.down * 25 * (i+1), totalSlainText.transform.rotation);
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

        recipesCollectedText.text = "Recipes  Collected:\n" +
            InventoryManager.currentInstance.globalCraftingRecipes.FindAll(x => x.showRecipeInBook).Count + " / " + InventoryManager.currentInstance.globalCraftingRecipes.Count;
        loreCollectedText.text = "Lore  Collected:\n" + 
            InventoryManager.currentInstance.globalLoreList.FindAll(x => x.showLoreInBook).Count + " / " + InventoryManager.currentInstance.globalLoreList.Count;

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
        foreach (Lore lore in InventoryManager.currentInstance.globalLoreList.FindAll(x => x.showLoreInBook)) {
            if (String.IsNullOrEmpty(lore.loreText)) continue;

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

        UpgradePlayerHealthVisuals();
        UpgradePlayerSpeedVisuals();
        UpgradePlayerDamageVisuals();
        UpgradeShopVisuals();
    }

    private void OnDisable() {
        foreach(GameObject obj in textObjs) {
            Destroy(obj);
        }
        textObjs.Clear();
    }

    public void FlipPage(int direction) {
        player.PlaySound(pageFlipClip, 1, UnityEngine.Random.Range(0.85f, 1.15f));
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
        player.PlaySound(pageFlipClip, 1, UnityEngine.Random.Range(0.85f, 1.15f)); 
        
        foreach (GameObject obj in chapters) { // Go through all chapters in the book
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

    public void UpgradePlayerStat(int type) {
        switch ((PlayerUpgrades)type) {
            case PlayerUpgrades.Health:
                int upgradeLevel = PlayerPrefs.GetInt("player_stats_health", 1);
                if (upgradeLevel >= 6) return;

                int cost = baseCost + (costLevelMultiplier * (upgradeLevel-1));

                if (InventoryManager.currentInstance.playerCash < cost) {
                    InventoryManager.currentInstance.playerCashText.rectTransform.DOShakeAnchorPos(0.15f, 20f, 50).SetUpdate(true);
                    return;
                }

                InventoryManager.currentInstance.playerCash -= cost;

                PlayerPrefs.SetInt("player_stats_health", upgradeLevel + 1);
                UpgradePlayerHealthVisuals();
                break;
            case PlayerUpgrades.Speed:
                upgradeLevel = PlayerPrefs.GetInt("player_stats_speed", 1);
                if (upgradeLevel >= 6) return;

                cost = baseCost + (costLevelMultiplier * (upgradeLevel-1));

                if (InventoryManager.currentInstance.playerCash < cost) {
                    InventoryManager.currentInstance.playerCashText.rectTransform.DOShakeAnchorPos(0.15f, 20f, 50).SetUpdate(true);
                    return;
                }

                InventoryManager.currentInstance.playerCash -= cost;

                PlayerPrefs.SetInt("player_stats_speed", upgradeLevel + 1);
                UpgradePlayerSpeedVisuals();
                break;
            case PlayerUpgrades.AttackDmg:
                upgradeLevel = PlayerPrefs.GetInt("player_stats_atkdmg", 1);
                if (upgradeLevel >= 6) return;

                cost = baseCost + (costLevelMultiplier * (upgradeLevel-1));

                if (InventoryManager.currentInstance.playerCash < cost) {
                    InventoryManager.currentInstance.playerCashText.rectTransform.DOShakeAnchorPos(0.15f, 20f, 50).SetUpdate(true);
                    return;
                }

                InventoryManager.currentInstance.playerCash -= cost;

                PlayerPrefs.SetInt("player_stats_atkdmg", upgradeLevel + 1);
                UpgradePlayerDamageVisuals();
                break;
            case PlayerUpgrades.Shop:
                upgradeLevel = PlayerPrefs.GetInt("player_stats_shopupg", 1);
                if (upgradeLevel >= 2) return;

                cost = 500;

                if (InventoryManager.currentInstance.playerCash < cost) {
                    InventoryManager.currentInstance.playerCashText.rectTransform.DOShakeAnchorPos(0.15f, 20f, 50).SetUpdate(true);
                    return;
                }

                InventoryManager.currentInstance.playerCash -= cost;

                PlayerPrefs.SetInt("player_stats_shopupg", upgradeLevel + 1);
                UpgradeShopVisuals();
                break;
            default:
                break;
        }
        InventoryManager.currentInstance.UpdatePlayerMoneyString();
    }

    public void UpgradePlayerHealthVisuals() {
        int upgradeLevel = PlayerPrefs.GetInt("player_stats_health", 1);

        Transform barsTransform = healthUpgradeObj.transform.Find("Bars");

        int maxUpgrades = barsTransform.childCount;

        for (int i = 0; i < maxUpgrades; i++) {
            if (upgradeLevel <= i) barsTransform.GetChild(i).GetComponent<Image>().color = UpgradeBarColorOff;
            else barsTransform.GetChild(i).GetComponent<Image>().color = UpgradeBarColorOn;
        }

        int cost = baseCost + (costLevelMultiplier * (upgradeLevel-1));
        healthUpgradeObj.transform.Find("Cash Amount").GetComponent<TMP_Text>().text = upgradeLevel >= maxUpgrades ? "MAX" : "$" + cost;
    }

    public void UpgradePlayerSpeedVisuals() {
        int upgradeLevel = PlayerPrefs.GetInt("player_stats_speed", 1);

        Transform barsTransform = speedUpgradeObj.transform.Find("Bars");

        int maxUpgrades = barsTransform.childCount;

        for (int i = 0; i < maxUpgrades; i++) {
            if (upgradeLevel <= i) barsTransform.GetChild(i).GetComponent<Image>().color = UpgradeBarColorOff;
            else barsTransform.GetChild(i).GetComponent<Image>().color = UpgradeBarColorOn;
        }

        int cost = baseCost + (costLevelMultiplier * (upgradeLevel-1));
        speedUpgradeObj.transform.Find("Cash Amount").GetComponent<TMP_Text>().text = upgradeLevel >= maxUpgrades ? "MAX" : "$" + cost;
    }

    public void UpgradePlayerDamageVisuals() {
        int upgradeLevel = PlayerPrefs.GetInt("player_stats_atkdmg", 1);

        Transform barsTransform = attackDmgUpgradeObj.transform.Find("Bars");

        int maxUpgrades = barsTransform.childCount;

        for (int i = 0; i < maxUpgrades; i++) {
            if (upgradeLevel <= i) barsTransform.GetChild(i).GetComponent<Image>().color = UpgradeBarColorOff;
            else barsTransform.GetChild(i).GetComponent<Image>().color = UpgradeBarColorOn;
        }

        int cost = baseCost + (costLevelMultiplier * (upgradeLevel-1));
        attackDmgUpgradeObj.transform.Find("Cash Amount").GetComponent<TMP_Text>().text = upgradeLevel >= maxUpgrades ? "MAX" : "$" + cost;
    }
    public void UpgradeShopVisuals() {
        int upgradeLevel = PlayerPrefs.GetInt("player_stats_shopupg", 1);

        Transform barsTransform = shopUpgradeObj.transform.Find("Bars");

        int maxUpgrades = barsTransform.childCount;

        for (int i = 0; i < maxUpgrades; i++) {
            if (upgradeLevel <= i) barsTransform.GetChild(i).GetComponent<Image>().color = UpgradeBarColorOff;
            else barsTransform.GetChild(i).GetComponent<Image>().color = UpgradeBarColorOn;
        }

        // overriding the cost for the shop. lazy way but quick
        int cost = 500; // baseCost + (costLevelMultiplier * (upgradeLevel - 1));
        shopUpgradeObj.transform.Find("Cash Amount").GetComponent<TMP_Text>().text = upgradeLevel >= maxUpgrades ? "MAX" : "$" + cost;
    }
}

[Serializable]
public enum PlayerUpgrades {
    Health,
    Speed,
    AttackDmg,
    Shop,
}
