using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuScript : MonoBehaviour
{
    public void ClosePauseMenu() {
        InventoryManager.currentInstance.ToggleInventory(InventoryManager.InventoryOpening.Closing, gameObject);
    }

    public void GoToMainMenu() {
        SceneManager.LoadScene(0);
    }
}
