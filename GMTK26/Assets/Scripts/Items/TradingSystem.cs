using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TradingSystem : MonoBehaviour
{
    public static TradingSystem Instance;
    
    // List of items to trade for lesser value items and the wedding items
    public ItemDatabase tradableItemDatabase;

    private int currentTradableItemIndex;

    // List of items player must obtain for the wedding
    public ItemDatabase weddingItemList;
    public Dictionary<string, WeddingItem> weddingItemChecklist = new Dictionary<string, WeddingItem>();

    private int numWeddingItemsObtained = 0;

    private void Awake()
    {
        if (Instance)
        {
            Debug.Log("Duplicate TradingSystem found");
            return;
        }

        Instance = this;
        
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

    public ItemsSO GetNextTradableItem()
    {
        return tradableItemDatabase.items[currentTradableItemIndex + 1];
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

    public List<ItemsSO> GetObtainedWeddingItems()
    {
        return weddingItemChecklist.Values
            .Where(weddingItem => weddingItem.obtained)
            .Select(weddingItem => weddingItem.item)
            .ToList();
    }

    public int GetTotalWeddingItems()
    {
        return weddingItemList.items.Count;
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

