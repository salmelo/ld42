using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu]
public class Room : ScriptableObject
{
    public List<ItemDrop> drops;

    private int weightSum;

    private void OnEnable()
    {
        weightSum = drops.Sum(d => d.weight);
    }

    public Item SpawnItem(Vector3 position, Transform parent)
    {
        var roll = Random.Range(0, weightSum);
        var tally = 0;
        ItemDrop drop = default(ItemDrop);
        foreach (var d in drops)
        {
            tally += d.weight;
            if (roll < tally)
            {
                drop = d;
                break;
            }
        }

        //this shouldn't trigger but just in case
        if (drop.item == null)
        {
            Debug.LogError("Random Item Weight Tally Broke");
            drop = drops[Random.Range(0, drops.Count)];
        }

        var item = Instantiate(drop.item, position, Quaternion.identity, parent);
        if (item.stackOf != "")
        {
            item.startStack = drop.stack.Random();
        }
        return item;
    }
}

[System.Serializable]
public struct ItemDrop
{
    public Item item;
    [Range(1, 100)]
    public int weight;
    public Range stack;
}

[System.Serializable]
public struct Range
{
    [Range(2,30)]
    public int min, max;

    public Range(int min, int max)
    {
        this.min = min;
        this.max = max;
    }

    public int Random()
    {
        return UnityEngine.Random.Range(min, max);
    }
}