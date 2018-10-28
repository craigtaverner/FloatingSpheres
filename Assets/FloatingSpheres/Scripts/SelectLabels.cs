using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingSpheres
{
    public class SelectLabels : MonoBehaviour
    {
        public FloatingSpheres floatingSpheres;
        internal List<LabelAction> labelActions;
        private Dictionary<string,LabelAction> labels;
        private ToggleGroup toggleGroup;
        public InputField inputField;
        private float labelX = 0;
        private float labelYGap = 20;
        private float labelYTop = 50;
        private Color[] colors = new Color[] { Color.red, Color.blue, Color.green, Color.magenta, Color.cyan, Color.gray };

        void Start()
        {
            this.toggleGroup = this.GetComponentInChildren<ToggleGroup>();
            this.labels = new Dictionary<string, LabelAction>();
            this.labelActions = new List<LabelAction>();
            foreach (LabelAction label in this.GetComponentsInChildren<LabelAction>())
            {
                this.labelActions.Add(label);
                string name = label.GetComponentInChildren<Text>().text;
                this.labels[name] = label;
                Debug.Log("Created label: " + name);
            }
            if (labelActions.Count > 0)
            {
                this.labelX = labelActions[0].transform.localPosition.x;
                this.labelYTop = labelActions[0].transform.localPosition.y;
            }
            if (labelActions.Count > 1)
            {
                this.labelYGap = labelActions[0].transform.localPosition.y - labelActions[1].transform.localPosition.y;
            }
            if (this.inputField == null)
            {
                this.inputField = this.GetComponentInChildren<InputField>();
            }
            if (this.floatingSpheres == null)
            {
                this.floatingSpheres = Component.FindObjectOfType<FloatingSpheres>();
            }
            this.floatingSpheres.selectLabels = this;
        }

        internal void ClearLabels()
        {
            this.labelActions.Clear();
            this.labels.Clear();
            foreach (Toggle label in this.GetComponentsInChildren<Toggle>())
            {
                label.transform.SetParent(null);
                label.GetComponent<LabelAction>().selectLabels = null;
                Destroy(label);
            }
        }

        internal Color GetSelectedLabelColor()
        {
            LabelAction selected = GetSelectedLabelAction();
            if (selected != null)
            {
                return selected.GetColor();
            }
            Debug.Log("Unable to find a selected label color");
            return UnityEngine.Random.ColorHSV();
        }

        internal LabelAction GetSelectedLabelAction()
        {
            foreach (LabelAction label in this.labelActions)
            {
                if (label.GetComponent<Toggle>().isOn)
                {
                    return label;
                }
            }
            return null;
        }

        internal void DeleteLabel(LabelAction labelAction)
        {
            if (this.labels.ContainsKey(labelAction.name))
            {
                LabelAction label = this.labels[labelAction.name];
                int labelIndex = this.labelActions.IndexOf(label);
                if (labelIndex < 0)
                {
                    Debug.Log("Cannot find toggle index for label: " + label.name);
                }
                else
                {
                    this.labels.Remove(label.name);
                    this.labelActions.Remove(label);
                    label.GetComponent<LabelAction>().selectLabels = null;
                    label.transform.SetParent(null);
                    Destroy(label);
                    for (int i = 0; i < this.labelActions.Count; i++)
                    {
                        PositionLabel(i, this.labelActions[i]);
                    }
                }
            }
            else
            {
                Debug.Log("Could not find label to delete: " + labelAction.name);
            }
        }

        private void PositionLabel(int labelIndex, LabelAction labelAction)
        {
            RectTransform toggleTransform = labelAction.GetComponent<RectTransform>();
            toggleTransform.localEulerAngles = Vector3.zero;
            toggleTransform.localScale = Vector3.one;
            toggleTransform.localPosition = new Vector3(this.labelX, this.labelYTop - this.labelYGap * labelIndex, 0);
        }

        internal string MakeNameUnique(string name)
        {
            string unique = name;
            int index = 0;
            while (this.labels.ContainsKey(unique))
            {
                unique = string.Format("{0}_{1}", name, index);
                index++;
            }
            return unique;
        }

        internal void AddLabel(int labelIndex, string name, Color color)
        {
            name = MakeNameUnique(name);
            Debug.Log(string.Format("Creating label '{1}' at index {0} with color '{2}'", labelIndex, name, color));
            GameObject toggleObject = (GameObject)Instantiate(Resources.Load("LabelToggle"));
            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.name = name;
            toggle.isOn = false;
            toggle.transform.SetParent(this.toggleGroup.transform);
            toggle.group = this.toggleGroup;
            toggle.GetComponentInChildren<Text>().text = name;
            Image colorImage = toggle.transform.Find("Color").GetComponent<Image>();
            colorImage.color = color;
            LabelAction labelAction = toggleObject.AddComponent<LabelAction>();
            labelAction.selectLabels = this;
            PositionLabel(labelIndex, labelAction);
            Button deleteButton = toggle.transform.Find("X").GetComponent<Button>();
            deleteButton.onClick.AddListener(labelAction.DeleteLabel);
            this.labels.Add(name, labelAction);
            this.labelActions.Add(labelAction);
        }

        public void AddLabel()
        {
            if (this.inputField == null)
            {
                Debug.Log("Cannot add label with null InputField");
            }
            else
            {
                string name = this.inputField.text;
                this.inputField.text = "";
                Debug.Log("Creating new label: " + name);
                if (name != null && name.Length > 0)
                {
                    int labelIndex = this.labels.Count;
                    Color color = (labelIndex < this.colors.Length) ? colors[labelIndex] : UnityEngine.Random.ColorHSV();
                    AddLabel(labelIndex, name, color);
                }
            }
        }
    }
}
