using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingSpheres
{
    public class LabelAction : MonoBehaviour
    {
        public SelectLabels selectLabels;

        public void DeleteLabel()
        {
            if (selectLabels != null)
            {
                selectLabels.DeleteLabel(this);
            }
        }

        public Color GetColor()
        {
            foreach (Image img in this.GetComponentsInChildren<Image>())
            {
                if (img.gameObject.name.Equals("Color"))
                {
                    return img.color;
                }
            }
            return Color.gray;
        }
    }
}
