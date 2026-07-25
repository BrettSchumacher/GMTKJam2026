using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TradingSystem : MonoBehaviour
{
    // List of items to trade for lesser value items and the wedding items
    public ItemDatabase tradableItemDatabase;

    private int currentTradableItemIndex;

    // List of items player must obtain for the wedding
    public ItemDatabase weddingItemList;
    public Dictionary<string, WeddingItem> weddingItemChecklist = new Dictionary<string, WeddingItem>();
    private int numWeddingItemsObtained = 0;

    private void Start()
    {
        currentTradableItemIndex = 0;

        foreach (ItemsSO item in weddingItemList.items)
        {
            weddingItemChecklist.Add(item.ItemName, new WeddingItem(item));
        }
    }


    public ItemsSO GetCurrentTradableItem()
    {
       return tradableItemDatabase.items[currentTradableItemIndex];
    }

    public void TradeItem(ItemsSO weddingItem)
    {
        Debug.Log("Trade item: " + GetCurrentTradableItem() + " for " + weddingItemChecklist[weddingItem.ItemName].item.ItemName);
        currentTradableItemIndex++;
        // TODO: If we have an image of the current tradable item in the HUD, switch that HUD image here.

        weddingItemChecklist[weddingItem.ItemName].obtained = true;
        numWeddingItemsObtained++;
    }

    public int GetNumWeddingItemsObtained()
    {
        return numWeddingItemsObtained;
    }
}

public class WeddingItem
{
    public ItemsSO item;
    public bool obtained;

    public WeddingItem(ItemsSO item)
    {
        this.item = item;
        obtained = false;
    }
}

