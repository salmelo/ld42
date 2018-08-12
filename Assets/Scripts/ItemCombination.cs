using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class ItemCombination : ScriptableObject
{
    public string item1, item2;
    public Item result;

    public bool CanCombine(Item a, Item b)
    {
        return (a.name == item1 && b.name == item2) || (a.name == item2 && b.name == item1);
    }
}
