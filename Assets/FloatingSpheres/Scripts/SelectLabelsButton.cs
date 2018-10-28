using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatingSpheres
{
    public class SelectLabelsButton : MonoBehaviour
    {
        public Canvas selectLabelsMenu;
        public FloatingSpheres floatingSpheres;
        private SelectLabels selectLabels;

        void Start()
        {
            this.selectLabels = selectLabelsMenu.GetComponent<SelectLabels>();
            if (floatingSpheres == null)
            {
                this.floatingSpheres = Component.FindObjectOfType<FloatingSpheres>();
            }
            //SetActiveMenu(false);
        }

        public void ToggleMenu()
        {
            SetActiveMenu(!this.selectLabelsMenu.gameObject.activeSelf);
        }

        private void SetActiveMenu(bool active)
        {
            this.selectLabelsMenu.gameObject.SetActive(active);
        }
    }
}
