using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Item : MonoBehaviour, IPointerClickHandler
{
    public new string name;
    public Vector2Int size = new Vector2Int(1, 1);
    public string stackOf;
    public int startStack;
    public int maxStack;
    public GameObject graphic;

    private GameObject ghost;
    private int currentStack;

    private void Start()
    {
        if (stackOf != "")
        {
            currentStack = startStack;
            SetStack();
        }
    }

    private void SetStack()
    {
        var count = 0;
        foreach (Transform t in graphic.transform)
        {
            t.GetComponent<SpriteRenderer>().enabled = count++ < currentStack;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                GameManager.current.SelectItem(this);
                break;
            case PointerEventData.InputButton.Right:
            case PointerEventData.InputButton.Middle:
            default:
                break;
        }
    }

    public void Selected()
    {
        foreach (var col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }
        if (graphic)
        {
            ghost = Instantiate(graphic, graphic.transform.position.WithZ(5), graphic.transform.rotation);
            Tint(new Color(1, 1, 1, .4f), ghost);
        }
            
    }

    public void Deselected()
    {
        foreach (var col in GetComponents<Collider2D>())
        {
            col.enabled = true;
        }
        Tint(Color.white);
        if (ghost)
        {
            Destroy(ghost);
        }
    }

    public void ShowInvalid()
    {
        Tint(Color.red);
    }

    public void ClearInvalid()
    {
        Tint(Color.white);
    }

    private void Tint(Color color, GameObject root = null)
    {
        foreach (var r in (root ?? gameObject).GetComponentsInChildren<SpriteRenderer>())
        {
            r.color = color;
        }
    }

    public bool CanStack(Item other)
    {
        return (other.name == stackOf && currentStack < maxStack)
            || (other.stackOf == stackOf && currentStack + other.currentStack <= maxStack);
    }

    public void AddStack(Item other)
    {
        currentStack += Mathf.Max(1, other.currentStack);
        SetStack();
    }
}
