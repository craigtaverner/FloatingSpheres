using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zinnia.Data.Type;

public class MenuManager : MonoBehaviour
{
    public GameObject menuObject;
    public GameObject selectLabelsMenu;
    public FloatingSpheres.FloatingSpheres floatingSpheres;
    private DateTime lastTime = System.DateTime.Now;
    private List<ClickableObject> clickableObjects = new List<ClickableObject>();

    public void Start()
    {
        if (menuObject != null)
        {
            if (selectLabelsMenu == null)
            {
                FloatingSpheres.SelectLabels selectLabels = menuObject.GetComponentInChildren<FloatingSpheres.SelectLabels>();
                if (selectLabels != null)
                {
                    selectLabelsMenu = selectLabels.gameObject;
                    if (floatingSpheres == null)
                    {
                        floatingSpheres = selectLabels.floatingSpheres;
                    }
                }
            }
        }
        buildObjects();
        selectLabelsMenu.SetActive(false);
    }

    private interface ClickableObject
    {
        void ClickIfAt(Vector3 position);
    }

    private class ClickableButton : ClickableObject
    {
        private Bounds bounds;
        private Button button;
        public ClickableButton(Bounds bounds, Button button)
        {
            this.bounds = bounds;
            this.button = button;
        }
        public void ClickIfAt(Vector3 position)
        {
            if (this.bounds.Contains(position))
            {
                Debug.Log("Clicking button: " + this.button);
                this.button.onClick.Invoke();
            }
            else
            {
                Debug.Log("Not clicking button: " + this.button);
            }
        }
    }

    private class ClickableToggle: ClickableObject
    {
        private Bounds bounds;
        private Toggle toggle;
        public ClickableToggle(Bounds bounds, Toggle toggle)
        {
            this.bounds = bounds;
            this.toggle = toggle;
        }
        public void ClickIfAt(Vector3 position)
        {
            if (this.bounds.Contains(position))
            {
                Debug.Log("Clicking toggle: " + this.toggle);
                this.toggle.group.SetAllTogglesOff();
                this.toggle.group.NotifyToggleOn(this.toggle);
                this.toggle.isOn = true;
                this.toggle.onValueChanged.Invoke(true);
            }
            else
            {
                Debug.Log("Not clicking toggle: " + this.toggle);
            }
        }
    }

    private void buildObjects()
    {
        clickableObjects.Clear();
        foreach (Button button in menuObject.GetComponentsInChildren<Button>())
        {
            Debug.Log("Setting up button: " + button);
            BoxCollider collider = button.transform.GetComponent<BoxCollider>();
            if (collider == null)
            {
                Debug.LogError("Menu item has no collider: " + button);
            }
            else
            {
                Debug.Log("Found menu item with collider: " + collider);
                Debug.Log("Found menu item with collider: " + collider.bounds);
                clickableObjects.Add(new ClickableButton(collider.bounds, button));
            }
        }
        foreach (Toggle toggle in menuObject.GetComponentsInChildren<Toggle>())
        {
            Debug.Log("Setting up toggle: " + toggle);
            BoxCollider collider = toggle.transform.GetComponent<BoxCollider>();
            if (collider == null)
            {
                Debug.LogError("Menu item has no collider: " + toggle);
            }
            else
            {
                Debug.Log("Found menu item with collider: " + collider);
                Debug.Log("Found menu item with collider: " + collider.bounds);
                clickableObjects.Add(new ClickableToggle(collider.bounds, toggle));
            }
        }
    }

    public void ToggleMenu(bool ignored)
    {
        DateTime now = System.DateTime.Now;
        if (now - lastTime > TimeSpan.FromSeconds(0.2))
        {
            menuObject.SetActive(!menuObject.activeSelf);
            lastTime = now;
        }
    }

    public virtual void Selected(TransformData destination)
    {
        if (destination != null)
        {
            Vector3 position = destination.Position;
            Debug.Log("Selected menu item at: " + destination.Position);
            foreach (ClickableObject clickable in clickableObjects)
            {
                Debug.Log("Testing possible clickable item: " + clickable.ToString());
                clickable.ClickIfAt(position);
            }
        }
    }

}
