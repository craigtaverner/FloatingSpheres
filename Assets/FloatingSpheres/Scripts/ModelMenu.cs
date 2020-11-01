using UnityEngine;
using UnityEngine.SceneManagement;
using Zinnia.Data.Type;

namespace FloatingSpheres
{
    public class ModelMenu : MonoBehaviour
    {
        public Camera head;
        public FloatingSpheres floatingSpheres;
        public int imageCounter;
        public int superSize = 2;
        public float waitSeconds = 1;
        public string cameraTag = "picCam";
        private string directory;

        public void Start()
        {
            this.imageCounter = 0;
            if (this.floatingSpheres == null)
            {
                this.floatingSpheres = Component.FindObjectOfType<FloatingSpheres>();
            }
            this.directory = System.IO.Path.GetFullPath(Application.dataPath + "/../Screenshots");
            System.IO.Directory.CreateDirectory(directory);
        }

        public void ResetScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void TakeScreenshot()
        {
            Invoke("DoTakeScreenshot", waitSeconds);
        }

        public void DoTakeScreenshot()
        {
            this.imageCounter++;
            ScreenCapture.CaptureScreenshot(ScreenShotName("Screenshots", "FloatingSpheres"), superSize);
            this.head.enabled = false;
            TakePics(cameraTag);
            this.head.enabled = true;
        }

        public string ScreenShotName(string directory, string prefix)
        {
            return string.Format("{0}/{1}_{2}_{3}.png", directory, prefix, imageCounter, System.DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        }

        public void TakePics(string cameraTag)
        {
            foreach (GameObject go in GameObject.FindGameObjectsWithTag(cameraTag))
            {
                Camera cam = go.GetComponent<Camera>();
                int resWidth = cam.pixelWidth;
                int resHeight = cam.pixelHeight;
                RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
                cam.targetTexture = rt;
                cam.Render();
                Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
                RenderTexture.active = rt;
                screenShot.ReadPixels(cam.pixelRect, 0, 0);
                screenShot.Apply();
                byte[] bytes = screenShot.EncodeToPNG();
                string filename = ScreenShotName(directory, cam.name);
                Debug.Log("Saving screenshot to " + filename);
                System.IO.File.WriteAllBytes(filename, bytes);
                cam.targetTexture = null;
                RenderTexture.active = null;
                rt.Release();
            }
        }

        private void MakeOneNode()
        {
            float x = head.transform.position.x + Random.Range(-0.5f, 0.5f);
            float y = head.transform.position.y + Random.Range(-0.5f, 0.5f);
            float z = head.transform.position.z + Random.Range(-0.5f, 0.5f);
            floatingSpheres.MakeOne(x, y, z);
        }

        public void BringModel()
        {
            foreach (NodeObject node in GameObject.FindObjectsOfType<NodeObject>())
            {
                Vector3 pos = node.gameObject.transform.position;
                node.gameObject.transform.position = new Vector3(pos.x / 2, pos.y / 10, pos.z / 2);
            }
        }

        public void MakeMany()
        {
            for (int i = 0; i < 20; i++)
            {
                MakeOneNode();
            }
        }
    }
}
