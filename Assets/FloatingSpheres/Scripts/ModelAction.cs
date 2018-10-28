using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatingSpheres
{
    public class ModelAction : MonoBehaviour
    {
        public ModelManager modelManager;

        public void ImportModel()
        {
            modelManager.ImportModel(this);
        }

        public void DeleteModel()
        {
            if (modelManager != null)
            {
                modelManager.DeleteModel(this);
            }
        }

        public void SaveModel()
        {
            if (modelManager != null)
            {
                modelManager.SaveModelAs(this.name);
            }
        }
    }
}
