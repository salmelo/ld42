using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager current;

    public Item selectedItem;
    public Vector3 oldPosition;

    public List<ItemCombination> combinations;

    public Vector3 PointerPosition => Camera.main.ScreenToWorldPoint(Input.mousePosition).WithZ(0);

    private PointerEventData pointerData;
    private List<RaycastResult> raycastResults;

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        pointerData = new PointerEventData(EventSystem.current);
        raycastResults = new List<RaycastResult>(5) { new RaycastResult(), new RaycastResult(), new RaycastResult() };
    }

    void Update()
    {
        if (selectedItem)
        {
            if (Input.GetMouseButtonUp(1))
            {
                DeselectItem();
            }
            else
            {
                selectedItem.transform.position = PointerPosition;

                pointerData.position = Input.mousePosition;
                EventSystem.current.RaycastAll(pointerData, raycastResults);
                var canPlace = true;
                foreach (var res in raycastResults)
                {
                    if (res.isValid)
                    {
                        var item = res.gameObject.GetComponent<Item>();
                        if (item)
                        {
                            var combo = GetCombination(item, selectedItem);
                            if (combo == null)
                            {
                                canPlace = false;
                                break;
                            }

                            if (CanCombineSelected(item, combo))
                            {
                                canPlace = true;
                                break;
                            }
                        }
                        if (res.gameObject == Inventory.current.gameObject)
                        {
                            if (!Inventory.current.CanFit(selectedItem, Inventory.current.WorldToGridPosition(PointerPosition)))
                            {
                                canPlace = false;
                                break;
                            }
                        }
                    }
                }
                if (canPlace)
                {
                    selectedItem.ClearInvalid();
                }
                else
                {
                    selectedItem.ShowInvalid();
                }
            }
        }
    }

    private bool CanCombineSelected(Item item, ItemCombination combo)
    {
        //if (!combo.CanCombine(item, selectedItem)) return false;

        Vector2Int? invPosition = Inventory.current.GetPosition(item);
        if (invPosition.HasValue)
        {
            return Inventory.current.CanFit(combo.result, invPosition.Value, item);
        }

        return true;
    }

    public void SelectItem(Item item)
    {
        if (selectedItem)
        {
            var combo = GetCombination(item, selectedItem);
            if (combo != null && CanCombineSelected(item, combo))
            {
                var pos = Inventory.current.GetPosition(item);
                Inventory.current.TryRemoveItem(item);
                Inventory.current.TryRemoveItem(selectedItem);

                var newItem = Instantiate(combo.result);
                if (pos.HasValue)
                {
                    newItem.transform.position = Inventory.current.GridToWorldPosition(pos.Value);
                    Inventory.current.InsertItem(newItem, pos.Value);
                }
                else
                {
                    newItem.transform.position = item.transform.position;
                }

                Destroy(item.gameObject);
                selectedItem.Deselected();
                Destroy(selectedItem.gameObject);
                selectedItem = null;
            }
            return;
        }

        selectedItem = item;
        oldPosition = item.transform.position;
        item.Selected();
    }

    public void DeselectItem()
    {
        if (selectedItem)
        {
            selectedItem.transform.position = oldPosition;
            selectedItem.Deselected();
            selectedItem = null;
        }
    }

    public void DropItem(Vector3? position = null)
    {
        if (selectedItem)
        {
            if (Inventory.current.ContainsItem(selectedItem))
            {
                Inventory.current.RemoveItem(selectedItem);
            }
            oldPosition = position ?? PointerPosition;
            DeselectItem();
        }
    }

    public ItemCombination GetCombination(Item a, Item b)
    {
        return combinations.FirstOrDefault(c => c.CanCombine(a, b));
    }
}
