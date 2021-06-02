using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// UI elemnt for the autoGatherer
/// </summary>
public class AutoGatherUI : UI
{
    public EventSystem EventSystemManager;
    /// <summary>
    /// UI for the AutoGather
    /// </summary>
    public GameObject gatherUI;

    /// <summary>
    /// UI collision Layer
    /// </summary>
    public LayerMask uICollision;

    /// <summary>
    /// Text element showing how much of the item is stored
    /// </summary>
    TextMeshProUGUI inventoryDisplay;

    /// <summary>
    /// Display the UI
    /// </summary>
    public void DisplayUI()
    {
        GameObject player = GameObject.FindWithTag("Player");
        gatherUI.SetActive(true);
        player.GetComponent<Player>().canInteract = false;
    }
    /// <summary>
    /// Hide the UI
    /// </summary>
    public void HideUI()
    {
        GameObject player = GameObject.FindWithTag("Player");
        gatherUI.SetActive(false);

        player.GetComponent<Player>().canInteract = true;
    }

    /// <summary>
    /// Update the the display for how much inventory there is
    /// </summary>
    /// <param name="amount">amount currently in the inventory</param>
    public void UpdateResourceAmount(float amount)
    {
        inventoryDisplay = gameObject.GetComponentInChildren<TextMeshProUGUI>(true);
        inventoryDisplay.text = amount.ToString();
    }



}
