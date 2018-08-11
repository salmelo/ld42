using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ItemDropper : MonoBehaviour, IPointerClickHandler
{
    public PointerEventData.InputButton button;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == button)
        {
            GameManager.current.DropItem();
        }
    }
}
