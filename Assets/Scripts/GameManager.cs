using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager current;

    public Transform background;
    public Transform spawnTopRight, spawnBottomLeft;
    public Range itemsPerRoom = new Range(8, 16);
    public List<ItemCombination> combinations;
    public List<Room> rooms;

    public Vector3 PointerPosition => Camera.main.ScreenToWorldPoint(Input.mousePosition).WithZ(0);

    public Item SelectedItem { get; set; }

    private PointerEventData pointerData;
    private List<RaycastResult> raycastResults;
    private int currentRoom;
    private Vector3 oldPosition;
    private int checkNextRoom;

    private void Awake()
    {
        current = this;
    }

    private void Start()
    {
        pointerData = new PointerEventData(EventSystem.current);
        raycastResults = new List<RaycastResult>(5) { };
        NextRoom();
    }

    void Update()
    {
        if (checkNextRoom > 0)
        {
            checkNextRoom--;
            if (checkNextRoom <= 0)
            {
                DoCheckNextRoom();
            }
        }
        if (SelectedItem)
        {
            if (Input.GetMouseButtonUp(1))
            {
                DeselectItem();
            }
            else
            {
                SelectedItem.transform.position = PointerPosition;

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
                            var combo = GetCombination(item, SelectedItem);
                            if (combo != null)
                            {
                                if (CanCombineSelected(item, combo))
                                {
                                    break;
                                }
                            }

                            if (item.CanStack(SelectedItem))
                            {
                                break;
                            }

                            canPlace = false;
                            break;
                        }
                        if (res.gameObject == Inventory.current.gameObject)
                        {
                            if (!Inventory.current.CanFit(SelectedItem, Inventory.current.WorldToGridPosition(PointerPosition)))
                            {
                                canPlace = false;
                                break;
                            }
                        }
                    }
                }
                if (canPlace)
                {
                    SelectedItem.ClearInvalid();
                }
                else
                {
                    SelectedItem.ShowInvalid();
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
        if (SelectedItem)
        {
            var combo = GetCombination(item, SelectedItem);
            if (combo != null && CanCombineSelected(item, combo))
            {
                var pos = Inventory.current.GetPosition(item);
                Inventory.current.RemoveItem(item);
                Inventory.current.RemoveItem(SelectedItem);

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
                SelectedItem.Deselected();
                Destroy(SelectedItem.gameObject);
                SelectedItem = null;

                CheckNextRoom();
            }
            else if (item.CanStack(SelectedItem))
            {
                item.AddStack(SelectedItem);
                SelectedItem.Deselected();
                Inventory.current.RemoveItem(SelectedItem);
                Destroy(SelectedItem.gameObject);

                CheckNextRoom();
            }
            return;
        }

        SelectedItem = item;
        oldPosition = item.transform.position;
        item.Selected();
    }

    public void DeselectItem()
    {
        if (SelectedItem)
        {
            SelectedItem.transform.position = oldPosition;
            SelectedItem.Deselected();
            SelectedItem = null;
        }
    }

    public void DropItem(Vector3? position = null)
    {
        if (SelectedItem)
        {
            if (Inventory.current.ContainsItem(SelectedItem))
            {
                Inventory.current.RemoveItem(SelectedItem);
            }
            oldPosition = position ?? PointerPosition;
            DeselectItem();
        }
    }

    public ItemCombination GetCombination(Item a, Item b)
    {
        return combinations.FirstOrDefault(c => c.CanCombine(a, b));
    }

    private void NextRoom()
    {
        if (currentRoom >= rooms.Count)
        {
            //todo end game
            Debug.Log("Gama Ovar");
            return;
        }
        var room = rooms[currentRoom];
        currentRoom++;

        for (int toSpawn = itemsPerRoom.Random(); toSpawn > 0; toSpawn--)
        {
            var pos = new Vector3(Random.Range(spawnBottomLeft.position.x, spawnTopRight.position.x)
                                , Random.Range(spawnBottomLeft.position.y, spawnTopRight.position.y));
            room.SpawnItem(pos, background);
        }
    }

    public void CheckNextRoom()
    {
        checkNextRoom += 3;
    }

    private void DoCheckNextRoom()
    {
        if (!background.OfType<Transform>().Select(t => t.GetComponent<Item>()).Any())
        {
            NextRoom();
        }
    }
}
