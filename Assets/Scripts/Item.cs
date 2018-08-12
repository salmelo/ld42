using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Item : MonoBehaviour, IPointerClickHandler
{
    public new string name;
    public Vector2Int size = new Vector2Int(1, 1);
    public string stackWith;
    public GameObject graphic;

    private GameObject ghost;

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
}
