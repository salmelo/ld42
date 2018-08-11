using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager current;

    public Item selectedItem;
    public Vector3 oldPosition;


    public Vector3 PointerPosition => Camera.main.ScreenToWorldPoint(Input.mousePosition).WithZ(0);

    private PointerEventData pointerData;
    private List<RaycastResult> raycastResults;
    private Inventory inventory;

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        pointerData = new PointerEventData(EventSystem.current);
        raycastResults = new List<RaycastResult>(5) { new RaycastResult(), new RaycastResult(), new RaycastResult() };
        inventory = FindObjectOfType<Inventory>();
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
                foreach (var res in raycastResults)
                {
                    if (res.isValid)
                    {
                        //Debug.Log(res.screenPosition, res.gameObject);
                        if (res.gameObject != inventory.gameObject) continue;

                        if (inventory.CanFit(selectedItem, inventory.WorldToGridPosition(PointerPosition)))
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
        }
    }

    public void SelectItem(Item item)
    {
        if (selectedItem) return;
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
            if (inventory.ContainsItem(selectedItem))
            {
                inventory.RemoveItem(selectedItem);
            }
            oldPosition = position ?? PointerPosition;
            DeselectItem();
        }
    }
}
