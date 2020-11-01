using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zinnia.Data.Type;

public class MenuSelector : MonoBehaviour
{
    public virtual void Selected(TransformData destination)
    {
        Debug.Log("Selected menu item: " + destination);
        Debug.Log("Selected menu item transform: " + destination.Transform);
        Debug.Log("Selected menu item transform gameObject: " + destination.Transform.gameObject);
        Debug.Log("Selected menu item at: " + destination.Position);
        Debug.Log("Selected menu item transform parent: " + destination.Transform.parent);
        Debug.Log("Selected menu item transform parent gameObject: " + destination.Transform.parent.gameObject);
        Debug.Log("Selected menu item transform children: " + destination.Transform.childCount);
        Button button = destination.Transform.GetComponent<Button>();
        if(button == null)
        {
            Debug.LogError("Failed to find a button at target: " + destination);
        }
        else
        {
            button.onClick.Invoke();
        }
    }
}
