using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System;

namespace FloatingSpheres
{
    public class ModelManager : MonoBehaviour
    {
        private string directory;
        public FloatingSpheres floatingSpheres;
        private List<Button> modelButtons;
        private Dictionary<string, Button> models;
        public InputField inputField;
        private float buttonX = 0;
        private float buttonYGap = 20;
        private float buttonYTop = 40;

        public void Start()
        {
            this.directory = System.IO.Path.GetFullPath(Application.dataPath + "/../Models");
            System.IO.Directory.CreateDirectory(directory);
            this.models = new Dictionary<string, Button>();
            this.modelButtons = new List<Button>();
            foreach (Button button in this.GetComponentsInChildren<Button>())
            {
                if (button.name != "X")
                {
                    this.modelButtons.Add(button);
                    string name = button.GetComponentInChildren<Text>().text;
                    this.models[name] = button;
                    Debug.Log("Created model import button: " + name);
                }
            }
            foreach (string file in System.IO.Directory.GetFiles(directory))
            {
                Debug.Log("Found model file: " + file);
                AddModelButton(ModelName(file));
            }
            if (this.inputField == null)
            {
                this.inputField = this.GetComponentInChildren<InputField>();
            }
            if (this.floatingSpheres == null)
            {
                this.floatingSpheres = Component.FindObjectOfType<FloatingSpheres>();
            }
            Debug.Log("After initialization we have " + this.models.Count + " models: ");
            foreach (string name in models.Keys)
            {
                Debug.Log("\t\t" + name);
            }
        }

        internal string ModelFileName(string directory, string name)
        {
            return System.IO.Path.GetFullPath(string.Format("{0}/{1}.json", directory, name));
        }

        internal string ModelName(string filename)
        {
            string[] pathElements = Regex.Split(filename, "[./\\\\]");
            return pathElements[pathElements.Length - 2];
        }

        internal void DeleteModel(ModelAction modelAction)
        {
            if (this.models.ContainsKey(modelAction.name))
            {
                Button model = this.models[modelAction.name];
                this.models.Remove(model.name);
                this.modelButtons.Remove(model);
                model.GetComponent<ModelAction>().modelManager = null;
                model.transform.SetParent(null);
                Destroy(model);
                for (int i = 0; i < this.modelButtons.Count; i++)
                {
                    PositionButton(i, this.modelButtons[i]);
                }
            }
            else
            {
                Debug.Log("Could not find model to delete: " + modelAction.name);
                Debug.Log("Have: " + models.Keys);
            }
        }

        private void PositionButton(int buttonIndex, Button button)
        {
            Debug.Log("Positioning button " + buttonIndex + ": " + button.name);
            RectTransform buttonTransform = button.GetComponent<RectTransform>();
            buttonTransform.localEulerAngles = Vector3.zero;
            buttonTransform.localScale = Vector3.one;
            buttonTransform.localPosition = new Vector3(this.buttonX, this.buttonYTop - this.buttonYGap * buttonIndex, 0);
        }

        private void AddModelButton(string name)
        {
            GameObject buttonObject = (GameObject)Instantiate(Resources.Load("LoadModelButton"));
            Button button = buttonObject.GetComponent<Button>();
            button.name = name;
            button.transform.SetParent(this.transform.Find("Models"));
            button.GetComponentInChildren<Text>().text = name;
            int labelIndex = this.models.Count;
            PositionButton(labelIndex, button);
            ModelAction modelAction = buttonObject.AddComponent<ModelAction>();
            modelAction.modelManager = this;
            Button deleteButton = button.transform.Find("X").GetComponent<Button>();
            Button saveButton = button.transform.Find("V").GetComponent<Button>();
            deleteButton.onClick.AddListener(modelAction.DeleteModel);
            saveButton.onClick.AddListener(modelAction.SaveModel);
            button.onClick.AddListener(modelAction.ImportModel);
            this.models.Add(name, button);
            this.modelButtons.Add(button);
        }

        internal void ImportModel(ModelAction modelAction)
        {
            Debug.Log("Importing model from action: " + modelAction);
            ImportModel(modelAction.transform.Find("Text").GetComponent<Text>().text);
        }

        public void SaveModelAs()
        {
            if (this.inputField == null)
            {
                Debug.Log("Cannot save model to name from null InputField");
            }
            else
            {
                string name = this.inputField.text;
                this.inputField.text = "";
                Debug.Log("Creating new model button: " + name);
                if (name != null && name.Length > 0)
                {
                    if (!models.ContainsKey(name))
                    {
                        AddModelButton(name);
                    }
                    SaveModelAs(name);
                }
                else
                {
                    Debug.Log("Invalid model name: " + name);
                }
            }
        }

        internal void SaveModelAs(string name)
        {
            string filename = ModelFileName(directory, name);
            Debug.Log("Saving model '" + name + "' to file: " + filename);
            System.IO.FileStream file = System.IO.File.Open(filename, System.IO.FileMode.Create);
            List<LabelAction> labels = floatingSpheres.selectLabels.labelActions;
            NodeObject[] nodes = Component.FindObjectsOfType<NodeObject>();
            EdgeObject[] edges = Component.FindObjectsOfType<EdgeObject>();
            Model model = new Model(name, floatingSpheres.modelScale, labels, nodes, edges);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Model));
            ser.WriteObject(file, model);
            file.Close();
        }

        internal void ClearModel()
        {
            foreach (NodeObject node in Component.FindObjectsOfType<NodeObject>())
            {
                node.Detach();
                Destroy(node.gameObject);
            }
            foreach (EdgeObject edge in Component.FindObjectsOfType<EdgeObject>())
            {
                Destroy(edge.gameObject);
            }
            floatingSpheres.selectLabels.ClearLabels();
        }

        internal void ImportModel(string name)
        {
            Debug.Log("Importing model: " + name);
            ClearModel();
            string filename = ModelFileName(directory, name);
            Debug.Log("Importing model from: " + filename);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Model));
            System.IO.FileStream file = System.IO.File.Open(filename, System.IO.FileMode.Open);
            Model model = (Model)ser.ReadObject(file);
            file.Close();
            floatingSpheres.modelScale = model.scale;
            Dictionary<Node, NodeObject> nodes = new Dictionary<Node, NodeObject>();
            foreach (Label label in model.labels)
            {
                floatingSpheres.selectLabels.AddLabel(label.labelId, label.name, label.color);
            }
            foreach (Node node in model.nodes)
            {
                string labelName = null;
                Color color = UnityEngine.Random.ColorHSV();
                if (node.labelId >= 0 && node.labelId < model.labels.Length)
                {
                    Label label = model.labels[node.labelId];
                    labelName = label.name;
                    color = label.color;
                }
                NodeObject nodeObject = floatingSpheres.MakeNode(labelName, node.position, color);
                nodes[node] = nodeObject;
            }
            foreach (Edge edge in model.edges)
            {
                Node startNode = model.nodes[edge.startNode];
                Node endNode = model.nodes[edge.endNode];
                floatingSpheres.MakeConnection(nodes[startNode], nodes[endNode]);
            }
        }

        [DataContract]
        class Model
        {
            [DataMember]
            public string name;
            [DataMember]
            public float scale;
            [DataMember]
            public Label[] labels;
            [DataMember]
            public Node[] nodes;
            [DataMember]
            public Edge[] edges;

            private Dictionary<string, Label> knownLabels = new Dictionary<string, Label>();
            private Dictionary<NodeObject, Node> knownNodes = new Dictionary<NodeObject, Node>();

            public Model(string name, float scale, List<LabelAction> labelActions, NodeObject[] nodeObjects, EdgeObject[] edgeObjects)
            {
                this.name = name;
                this.scale = scale;
                this.labels = new Label[labelActions.Count];
                this.nodes = new Node[nodeObjects.Length];
                this.edges = new Edge[edgeObjects.Length];
                for (int i = 0; i < labels.Length; i++)
                {
                    this.labels[i] = new Label(i, labelActions[i]);
                    this.knownLabels[labelActions[i].name] = this.labels[i];
                }
                for (int i = 0; i < nodes.Length; i++)
                {
                    NodeObject obj = nodeObjects[i];
                    Label label = (obj.label != null && knownLabels.ContainsKey(obj.label)) ? knownLabels[obj.label] : null;
                    int labelId = (label == null) ? FindLabelIdByColor(labels, nodeObjects[i]) : label.labelId;
                    this.nodes[i] = new Node(i, labelId, nodeObjects[i]);
                    this.knownNodes[nodeObjects[i]] = this.nodes[i];
                }
                for (int i = 0; i < edges.Length; i++)
                {
                    Node startNode = knownNodes[edgeObjects[i].startNode];
                    Node endNode = knownNodes[edgeObjects[i].endNode];
                    this.edges[i] = new Edge(startNode.nodeId, endNode.nodeId);
                }
            }

            private int FindLabelIdByColor(Label[] labels, NodeObject nodeObject)
            {
                MeshRenderer renderer = nodeObject.GetComponent<MeshRenderer>();
                Color color = renderer.material.color;
                Debug.Log("Searching for label by color: " + color);
                foreach (Label label in labels)
                {
                    if (label.color.Equals(color))
                    {
                        Debug.Log("Found label: " + label);
                        return label.labelId;
                    }
                }
                Debug.Log("Found no label matching color: " + color);
                return -1;
            }
        }

        [DataContract]
        class Label
        {
            [DataMember]
            public int labelId;
            [DataMember]
            public string name;
            [DataMember]
            public Color color;
            public Label(int labelId, LabelAction labelAction)
            {
                this.labelId = labelId;
                this.name = labelAction.name;
                this.color = labelAction.GetColor();
            }
            override public string ToString()
            {
                return string.Format("Label[{0}]: '{1}' ({2})", labelId, name, color);
            }
        }

        [DataContract]
        class Edge
        {
            [DataMember]
            public int startNode;
            [DataMember]
            public int endNode;
            public Edge(int startNode, int endNode)
            {
                this.startNode = startNode;
                this.endNode = endNode;
            }
            override public string ToString()
            {
                return string.Format("Edge[{0}-{1}]", startNode, endNode);
            }
        }

        [DataContract]
        class Node
        {
            [DataMember]
            public int nodeId;
            [DataMember]
            public int labelId;
            [DataMember]
            public Vector3 position;
            public Node(int nodeId, int labelId, NodeObject node)
            {
                this.nodeId = nodeId;
                this.labelId = labelId;
                this.position = node.transform.position;
            }
            override public string ToString()
            {
                return string.Format("Node[{0}]: label:{1} at ({2})", nodeId, labelId, position);
            }
        }
    }
}
