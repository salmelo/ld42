using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class Inventory : MonoBehaviour, IPointerClickHandler
{
    public static Inventory current;

    public Vector2Int gridSize = new Vector2Int(6, 6);
    public float cellSize = 1;
    public LineRenderer linePrefab;

    private List<ItemContent> contents;

    void Awake()
    {
        current = this;
    }

    void Start()
    {
        DrawGrid();

        contents = new List<ItemContent>();
    }

    void DrawGrid()
    {
        for (int x = 0; x <= gridSize.x; x++)
        {
            var line = Instantiate(linePrefab, transform, false);
            line.positionCount = 2;
            line.SetPosition(0, new Vector3(x * cellSize, 0));
            line.SetPosition(1, new Vector3(x * cellSize, gridSize.y * cellSize));
        }
        for (int y = 0; y <= gridSize.y; y++)
        {
            var line = Instantiate(linePrefab, transform, false);
            line.positionCount = 2;
            line.SetPosition(0, new Vector3(0, y * cellSize));
            line.SetPosition(1, new Vector3(gridSize.x * cellSize, y * cellSize));
        }

        var box = GetComponent<BoxCollider2D>();
        if (box)
        {
            box.size = (Vector2)gridSize * cellSize;
            box.offset = box.size * 0.5f;
        }
    }

    private void OnDrawGizmos()
    {
        for (int x = 0; x <= gridSize.x; x++)
        {
            Gizmos.DrawLine(transform.position + new Vector3(x * cellSize, 0)
                          , transform.position + new Vector3(x * cellSize, gridSize.y * cellSize));
        }
        for (int y = 0; y <= gridSize.y; y++)
        {
            Gizmos.DrawLine(transform.position + new Vector3(0, y * cellSize)
                          , transform.position + new Vector3(gridSize.x * cellSize, y * cellSize));
        }
    }

    public Item GetItem(int x, int y)
    {
        return contents.FirstOrDefault(i => i.IsInCell(x, y)).item;
    }

    public Item GetItem(Vector2Int position)
    {
        return contents.FirstOrDefault(i => i.IsInCell(position)).item;
    }

    public Vector2Int? GetPosition(Item item)
    {
        foreach (var i in contents)
        {
            if (i.item == item) return i.position;
        }
        return null;
    }

    public bool ContainsItem(Item item)
    {
        return contents.Any(i => i.item == item);
    }

    public bool CanFit(Item item, Vector2Int position, Item ignore = null)
    {
        for (int x = 0; x < item.size.x; x++)
        {
            for (int y = 0; y < item.size.y; y++)
            {
                Item present = GetItem(position + new Vector2Int(x, y));
                if (present != null && present != ignore)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void InsertItem(Item item, Vector2Int position)
    {
        contents.Add(new ItemContent(item, position));
    }

    public void InsertItem(Item item, int x, int y)
    {
        if (item == null) throw new System.ArgumentException("Item cannot be null.", nameof(item));
        contents.Add(new ItemContent(item, x, y));
    }

    public void RemoveItem(Item item)
    {
        contents.Remove(contents.First(i => i.item == item));
    }

    public void TryRemoveItem(Item item)
    {
        var i = contents.FirstOrDefault(it => it.item == item);
        if (i.item != null)
        {
            contents.Remove(i);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left 
            || !GameManager.current.selectedItem) return;

        var position = WorldToGridPosition(eventData.pointerCurrentRaycast.worldPosition);

        if (CanFit(GameManager.current.selectedItem, position))
        {
            var i = GameManager.current.selectedItem; 
            GameManager.current.DropItem(GridToWorldPosition(position));
            InsertItem(i, position);
        }
    }

    public Vector2Int WorldToGridPosition(Vector3 pos)
    {
        var local = transform.InverseTransformPoint(pos);
        local = local / cellSize;
        return Vector2Int.FloorToInt(local);
    }

    public Vector3 GridToWorldPosition(Vector2Int pos)
    {
        var local = (Vector2)pos * cellSize + Vector2.one * (cellSize * .5f);
        return transform.TransformPoint(local).WithZ(0);
    }

    private struct ItemContent
    {
        public Item item;
        public Vector2Int position;

        public ItemContent(Item item, Vector2Int position)
        {
            this.item = item;
            this.position = position;
        }

        public ItemContent(Item item, int x, int y) : this(item, new Vector2Int(x, y)) { }

        public bool IsInCell(int x, int y)
        {
            return IsInCell(new Vector2Int(x, y));
        }

        public bool IsInCell(Vector2Int position)
        {
            for (int x = 0; x < item.size.x; x++)
            {
                for (int y = 0; y < item.size.y; y++)
                {
                    if (position == this.position + new Vector2Int(x, y)) return true;
                }
            }

            return false;
        }
    }
}
