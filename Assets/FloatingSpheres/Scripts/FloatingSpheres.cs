using NewtonVR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK.Prefabs.Interactions.Interactables;
using VRTK.Prefabs.Interactions.Interactors;
using Zinnia.Data.Type;

namespace FloatingSpheres
{
    public class FloatingSpheres : MonoBehaviour
    {
        public GameObject head;
        public GameObject leftController;
        public GameObject rightController;
        public GameObject spheres;
        public GameObject edges;
        public InteractableFacade template;
        public bool createMode;
        internal SelectLabels selectLabels;
        private InteractorFacade leftInteractor;
        private InteractorFacade rightInteractor;
        private bool twoHandedDrag;
        private float twoHandedDragStartSeparation;
        private Vector3 twoHandedDragStartPosition;
        private Vector3 twoHandedDragStartAngle;
        internal float modelScale = 1;
        internal float nodeScale = 0.3f;
        internal float edgeScale = 0.05f;

        void Start()
        {
            this.createMode = true;
            this.twoHandedDrag = false;
            if (this.template == null)
            {
                foreach (InteractableFacade obj in this.GetComponentsInChildren<InteractableFacade>())
                {
                    Debug.Log("Found facade: " + obj);
                    if (obj.name == "Template")
                    {
                        this.template = obj;
                    }
                }
            }
            if (this.template == null)
            {
                Debug.LogError("No template sphere found or configured");
            }
            else
            {
                Debug.Log("Found template: " + this.template);
                Debug.Log("Found template game object: " + this.template.gameObject);
            }
            if (this.spheres == null)
            {
                this.spheres = this.template.gameObject.transform.parent.gameObject;
            }
            if (this.spheres == null)
            {
                Debug.LogError("No parent for spheres found or configured");
            }
            if (leftController != null)
            {
                leftInteractor = leftController.GetComponentInChildren<InteractorFacade>();
            }
            if (rightController != null)
            {
                rightInteractor = rightController.GetComponentInChildren<InteractorFacade>();
            }
        }

        public void FixedUpdate()
        {
            if (pulling != null)
            {
                Rigidbody rigid = pulling.GetComponent<Rigidbody>();
                Vector3 move = (rightController.transform.position - pulling.position).normalized / 100f;
                Debug.Log("Pulling node by " + move);
                pulling.position = pulling.position + move;
            }
            else
            {
                lastPullTime = System.DateTime.Now;
            }
        }

        private NodeObject hovering;
        private Color previousColor;
        private Transform pulling;
        private DateTime lastPullTime = System.DateTime.Now;
        private DateTime lastSelectionTime = System.DateTime.Now;

        public virtual void PointerEntered(TransformData destination)
        {
            if (hovering != null)
            {
                PointerExited(destination);
            }
            Debug.Log("Searching for hovering sphere at: " + destination.Position);
            hovering = FindNodeAt(destination);
            if (hovering != null)
            {
                Debug.Log("Entering hovering of node: " + hovering);
                HighlightNode(hovering);
            }
        }

        public virtual void PointerExited(TransformData destination)
        {
            if (hovering != null)
            {
                Debug.Log("Exiting hovering of node: " + hovering);
                NormalizeNode(hovering);
                hovering = null;
            }
        }

        private void HighlightNode(NodeObject node)
        {
            if (node != null)
            {
                MeshRenderer renderer = node.GetComponentInChildren<MeshRenderer>();
                previousColor = renderer.material.color;
                Color color = renderer.material.color;
                color.a = 0.5f;
                color.b = 1f;
                color.g = 1f;
                renderer.material.color = color;
            }
        }

        private void NormalizeNode(NodeObject node)
        {
            if (node != null && previousColor != null)
            {
                MeshRenderer renderer = node.GetComponentInChildren<MeshRenderer>();
                renderer.material.color = previousColor;
            }
        }

        public virtual void Selected(TransformData destination)
        {
            DateTime now = System.DateTime.Now;
            if (now - lastSelectionTime > TimeSpan.FromSeconds(0.2))
            {
                lastSelectionTime = now;
                Debug.Log("Searching for selected sphere at: " + destination.Position);
                pulling = null;
                NodeObject node = FindNodeAt(destination);
                if (node != null)
                {
                    PullNode(node, 0.5f, null);
                }
            }
        }

        private NodeObject FindNodeAt(TransformData destination)
        {
            foreach (NodeObject sphere in spheres.transform.GetComponentsInChildren<NodeObject>())
            {
                SphereCollider collider = sphere.GetComponentInChildren<SphereCollider>();
                if (collider == null)
                {
                    Debug.LogError("Sphere has no collider: " + sphere);
                }
                else
                {
                    if (collider.bounds.Contains(destination.Position))
                    {
                        Debug.Log("Selected sphere: " + sphere.name);
                        return sphere;
                    }
                }
            }
            return null;
        }

        private void PullNode(NodeObject sphere, float factor, NodeObject parent)
        {
            Rigidbody rigid = sphere.GetComponent<Rigidbody>();
            if (rigid == null)
            {
                Debug.LogError("Sphere has no rigidbody, cannot pull it: " + sphere);
            }
            else
            {
                Vector3 move = (rightController.transform.position - sphere.transform.position) * factor;
                Debug.Log("Pulling node by " + move);
                rigid.velocity = rigid.velocity + move;
                if (factor > 0.1)
                {
                    foreach (NodeObject other in sphere.OtherNodes())
                    {
                        if (other != parent)
                        {
                            PullNode(other, factor / 2f, sphere);
                        }
                    }
                }
            }
        }

        public void SetCreateMode(bool value)
        {
            Debug.Log("Changing edit mode to createMode=" + value);
            this.createMode = value;
        }

        public void TriggerPressed(InteractorFacade hand)
        {
            if (this.createMode)
            {
                NodeObject grabbedNode = GrabbedNode(hand);
                NodeObject otherGrabbedNode = OtherGrabbedNode(hand);
                if (grabbedNode != null && otherGrabbedNode != null)
                {
                    MakeConnection(grabbedNode, otherGrabbedNode);
                }
                else
                {
                    NodeObject node = TriggerCreate(head.transform, WhichHand(hand).transform);
                    if (grabbedNode != null)
                    {
                        MakeConnection(grabbedNode, node);
                    }
                    if (otherGrabbedNode != null)
                    {
                        MakeConnection(otherGrabbedNode, node);
                    }
                }
            }
            else if (hovering != null)
            {
                hovering.Detach();
                Destroy(hovering.gameObject, 0.1f);
                hovering = null;
            }
        }

        private NodeObject GrabbedNode(InteractorFacade hand)
        {
            foreach (GameObject obj in hand.GrabbedObjects)
            {
                Debug.Log("Trigger pressed while object is grabbed: " + obj);
                NodeObject grabbedNode = obj.GetComponent<NodeObject>();
                if (grabbedNode != null)
                {
                    return grabbedNode;
                }
            }
            return null;
        }

        private NodeObject OtherGrabbedNode(InteractorFacade hand)
        {
            if (hand == leftInteractor)
            {
                return GrabbedNode(rightInteractor);
            }
            else if (hand == rightInteractor)
            {
                return GrabbedNode(leftInteractor);
            }
            else
            {
                Debug.LogError("Trigger event was not from one of the known controlled hands: " + hand);
                return null;
            }
        }

        GameObject WhichHand(InteractorFacade hand)
        {
            if (hand == leftInteractor)
            {
                return rightController;
            }
            else if (hand == rightInteractor)
            {
                return leftController;
            }
            else
            {
                Debug.LogError("Trigger event was not from one of the known controlled hands: " + hand);
                return null;
            }
        }

        private NodeObject TriggerCreate(Transform head, Transform hand)
        {
            float dx = hand.transform.position.x - head.transform.position.x;
            float dz = hand.transform.position.z - head.transform.position.z;
            float x = head.transform.position.x + 2 * dx;
            float z = head.transform.position.z + 2 * dz;
            float height = 1;
            height = Math.Max(height, Math.Max(head.transform.position.y, hand.transform.position.y));
            return MakeOne(x, height, z);
        }

        private Color GetColor()
        {
            if (this.selectLabels == null)
            {
                return UnityEngine.Random.ColorHSV();
            }
            else
            {
                return selectLabels.GetSelectedLabelColor();
            }
        }

        private static Vector3 Angles(Vector3 a, Vector3 b)
        {
            Vector3 delta = b - a;
            float az = Vector3.Angle(Vector3.right, delta);
            float ax = Vector3.Angle(Vector3.up, delta);
            float ay = Vector3.Angle(Vector3.back, delta);
            return new Vector3(ax, ay, az);
        }

        internal void TwoHandedDrag()
        {
            float distance = Vector3.Distance(leftController.transform.position, rightController.transform.position);
            if (distance > 0.001)
            {
                if (!twoHandedDrag)
                {
                    twoHandedDrag = true;
                    twoHandedDragStartSeparation = distance;
                    twoHandedDragStartPosition = (leftController.transform.position + rightController.transform.position) / 2f;
                    twoHandedDragStartAngle = Angles(leftController.transform.position, rightController.transform.position);
                    Debug.Log("Starting two handed drag with separation: " + twoHandedDragStartSeparation);
                }
                else
                {
                    Vector3 center = (leftController.transform.position + rightController.transform.position) / 2f;
                    Vector3 moved = center - twoHandedDragStartPosition;
                    float movedBy = Vector3.Distance(Vector3.zero, moved);
                    bool hasMoved = movedBy > 0.001;
                    float scaleFactor = distance / twoHandedDragStartSeparation;
                    bool hasScaled = Mathf.Abs(1.0f - scaleFactor) > 0.01;
                    Vector3 angles = Angles(leftController.transform.position, rightController.transform.position);
                    Vector3 deltaAngles = angles - twoHandedDragStartAngle;
                    bool hasRotated = Vector3.Distance(Vector3.zero, deltaAngles) > 1;
                    if (hasScaled || hasMoved || hasRotated)
                    {
                        twoHandedDragStartSeparation = distance;
                        twoHandedDragStartPosition = center;
                        twoHandedDragStartAngle = angles;
                        if (hasScaled)
                            Debug.Log("Dragged more than 1%: " + scaleFactor);
                        if (hasRotated)
                            Debug.Log("Angles changed by more than 1 degree: " + deltaAngles);
                        if (hasMoved)
                            Debug.Log("Position changed by more than 1 mm: " + movedBy);
                        this.modelScale *= scaleFactor;
                        foreach (NodeObject node in Component.FindObjectsOfType<NodeObject>())
                        {
                            Quaternion rotation = Quaternion.Euler(deltaAngles.x, deltaAngles.y, deltaAngles.z);
                            Vector3 pos = node.transform.position;
                            Vector3 delta = pos - twoHandedDragStartPosition;
                            Vector3 scaledDelta = delta * scaleFactor;
                            Vector3 scaledAndRotatedDelta = rotation * scaledDelta;
                            node.transform.position = scaledAndRotatedDelta + twoHandedDragStartPosition + moved;
                            node.transform.localScale = node.transform.localScale * scaleFactor;
                        }
                    }
                }
            }
        }

        internal void TwoHandedDragEnd()
        {
            this.twoHandedDrag = false;
        }

        internal NodeObject MakeNode(string label, Vector3 position, Color color)
        {
            Debug.Log("Making a sphere from the template: " + template);
            GameObject sphere = Instantiate(template.gameObject);
            sphere.transform.parent = this.spheres.transform;
            sphere.name = "Node " + spheres.transform.childCount;
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one * (float)System.Math.Max(0.1, nodeScale) * this.modelScale;
            MeshRenderer renderer = sphere.GetComponentInChildren<MeshRenderer>();
            renderer.material.color = color;
            Rigidbody body = sphere.GetComponentInChildren<Rigidbody>();
            body.mass = 0.05F;
            body.angularDrag = 0.05f;
            body.useGravity = false;  // turn off gravity
            body.drag = 0.8f;
            SphereCollider collider = sphere.GetComponentInChildren<SphereCollider>();
            collider.material.bounciness = 1.0F;
            sphere.AddComponent<NVRCollisionSoundObject>();
            /*
            NVRInteractableItem interactable = sphere.AddComponent<NVRInteractableItem>();
            interactable.CanAttach = true;
            interactable.DisableKinematicOnAttach = true;
            interactable.EnableKinematicOnDetach = false;
            interactable.EnableGravityOnDetach = false;
            */
            NodeObject node = sphere.AddComponent<NodeObject>();
            node.label = label;
            sphere.SetActive(true);
            return node;
        }

        public NodeObject MakeOne(float x, float y, float z)
        {
            NodeObject node = MakeNode("unknown", new Vector3(x, y, z), GetColor());
            if (selectLabels != null)
            {
                LabelAction selectedLabel = selectLabels.GetSelectedLabelAction();
                if (selectedLabel != null)
                {
                    node.label = selectedLabel.name;
                }
            }
            //DebugSphere(node, "Created");
            return node;
        }

        internal static void DebugSphere(Component node, String prefix)
        {
            Debug.Log(prefix + " sphere with components:");
            foreach (Component obj in node.GetComponents<Component>())
            {
                Debug.Log("\t\t" + obj);
            }
        }

        internal EdgeObject MakeConnection(NodeObject first, NodeObject second)
        {
            Debug.Log("Making connection from " + first + " to " + second);
            GameObject connection = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyColliders(connection);
            MeshRenderer renderer = connection.GetComponent<MeshRenderer>();
            renderer.material.color = Color.gray;
            connection.transform.localScale = Vector3.one * edgeScale;  // make very small at first
            EdgeObject edge = connection.AddComponent<EdgeObject>();
            edge.floatingSpheres = this;
            edge.startNode = first;
            edge.endNode = second;
            edge.scale = edgeScale;
            edge.UpdateConnection();
            first.AddEdge(edge);
            second.AddEdge(edge);
            if (edges != null)
            {
                connection.transform.parent = edges.transform;
            }
            return edge;
        }

        internal static void DestroyColliders(GameObject gameObject)
        {
            foreach (Collider collider in gameObject.GetComponents<Collider>())
            {
                DestroyImmediate(collider);
            }
        }

        internal void DeleteObjectAt(NVRHand hand)
        {
            Debug.Log("Deleting object interacting with " + hand);
            if (hand.IsInteracting)
            {
                //DebugSphere(hand.CurrentlyInteracting, "Interacting with");
                NodeObject sphere = hand.CurrentlyInteracting.GetComponent<NodeObject>();
                if (sphere == null)
                {
                    Debug.Log("Hand is interacting with something that is not a NodeObject: " + hand.CurrentlyInteracting);
                }
                else
                {
                    Debug.Log("Deleting sphere " + sphere);
                    hand.EndInteraction(hand.CurrentlyInteracting);
                    sphere.Detach();
                    Destroy(sphere.gameObject, 0.1f);
                }
            }
            else if (hand.IsHovering)
            {
                Debug.Log("No interacting object, but hand is hovering");
            }
        }
    }

    public class FloatingSpheresNVR : MonoBehaviour
    {
        NVRPlayer player;
        internal NVRCanvasInput canvasInput;
        public bool createMode;
        internal SelectLabels selectLabels;
        private bool twoHandedDrag;
        private float twoHandedDragStartSeparation;
        private Vector3 twoHandedDragStartPosition;
        private Vector3 twoHandedDragStartAngle;
        internal float modelScale = 1;
        internal float nodeScale = 0.3f;
        internal float edgeScale = 0.05f;

        void Start()
        {
            this.player = this.GetComponent<NVRPlayer>();
            this.canvasInput = this.GetComponent<NVRCanvasInput>();
            this.createMode = true;
            this.twoHandedDrag = false;
        }

        private static Vector3 Angles(Vector3 a, Vector3 b)
        {
            Vector3 delta = b - a;
            float az = Vector3.Angle(Vector3.right, delta);
            float ax = Vector3.Angle(Vector3.up, delta);
            float ay = Vector3.Angle(Vector3.back, delta);
            return new Vector3(ax, ay, az);
        }

        internal void TwoHandedDrag()
        {
            float distance = Vector3.Distance(player.LeftHand.CurrentPosition, player.RightHand.CurrentPosition);
            if (distance > 0.001)
            {
                if (!twoHandedDrag)
                {
                    twoHandedDrag = true;
                    twoHandedDragStartSeparation = distance;
                    twoHandedDragStartPosition = (player.LeftHand.CurrentPosition + player.LeftHand.CurrentPosition) / 2f;
                    twoHandedDragStartAngle = Angles(player.LeftHand.CurrentPosition, player.RightHand.CurrentPosition);
                    Debug.Log("Starting two handed drag with separation: " + twoHandedDragStartSeparation);
                }
                else
                {
                    Vector3 center = (player.LeftHand.CurrentPosition + player.LeftHand.CurrentPosition) / 2f;
                    Vector3 moved = center - twoHandedDragStartPosition;
                    float movedBy = Vector3.Distance(Vector3.zero, moved);
                    bool hasMoved = movedBy > 0.001;
                    float scaleFactor = distance / twoHandedDragStartSeparation;
                    bool hasScaled = Mathf.Abs(1.0f - scaleFactor) > 0.01;
                    Vector3 angles = Angles(player.LeftHand.CurrentPosition, player.RightHand.CurrentPosition);
                    Vector3 deltaAngles = angles - twoHandedDragStartAngle;
                    bool hasRotated = Vector3.Distance(Vector3.zero, deltaAngles) > 1;
                    if (hasScaled || hasMoved || hasRotated)
                    {
                        twoHandedDragStartSeparation = distance;
                        twoHandedDragStartPosition = center;
                        twoHandedDragStartAngle = angles;
                        if (hasScaled)
                            Debug.Log("Dragged more than 1%: " + scaleFactor);
                        if(hasRotated)
                            Debug.Log("Angles changed by more than 1 degree: " + deltaAngles);
                        if (hasMoved)
                            Debug.Log("Position changed by more than 1 mm: " + movedBy);
                        this.modelScale *= scaleFactor;
                        foreach (NodeObject node in Component.FindObjectsOfType<NodeObject>())
                        {
                            Quaternion rotation = Quaternion.Euler(deltaAngles.x, deltaAngles.y, deltaAngles.z);
                            Vector3 pos = node.transform.position;
                            Vector3 delta = pos - twoHandedDragStartPosition;
                            Vector3 scaledDelta = delta * scaleFactor;
                            Vector3 scaledAndRotatedDelta = rotation * scaledDelta;
                            node.transform.position = scaledAndRotatedDelta + twoHandedDragStartPosition + moved;
                            node.transform.localScale = node.transform.localScale * scaleFactor;
                        }
                    }
                }
            }
        }

        internal void TwoHandedDragEnd()
        {
            this.twoHandedDrag = false;
        }

        public void SetCreateMode(bool value)
        {
            Debug.Log("Changing edit mode to createMode=" + value);
            this.createMode = value;
        }

        private Color GetColor()
        {
            if (this.selectLabels == null)
            {
                return UnityEngine.Random.ColorHSV();
            }
            else
            {
                return selectLabels.GetSelectedLabelColor();
            }
        }

        internal NodeObject MakeNode(string label, Vector3 position, Color color)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = position;
            sphere.transform.localScale = Vector3.one * (float)System.Math.Max(0.1, nodeScale) * this.modelScale;
            MeshRenderer renderer = sphere.GetComponent<MeshRenderer>();
            renderer.material.color = color;
            Rigidbody body = sphere.AddComponent<Rigidbody>();
            body.mass = 0.05F;
            body.angularDrag = 0.05f;
            body.useGravity = false;  // turn off gravity
            body.drag = 0.8f;
            SphereCollider collider = sphere.GetComponent<SphereCollider>();
            collider.material.bounciness = 1.0F;
            sphere.AddComponent<NVRCollisionSoundObject>();
            NVRInteractableItem interactable = sphere.AddComponent<NVRInteractableItem>();
            interactable.CanAttach = true;
            interactable.DisableKinematicOnAttach = true;
            interactable.EnableKinematicOnDetach = false;
            interactable.EnableGravityOnDetach = false;
            NodeObject node = sphere.AddComponent<NodeObject>();
            node.label = label;
            return node;
        }

        public NodeObject MakeOne(float x, float y, float z)
        {
            NodeObject node = MakeNode("unknown", new Vector3(x, y, z), GetColor());
            if (selectLabels != null)
            {
                LabelAction selectedLabel = selectLabels.GetSelectedLabelAction();
                if (selectedLabel != null)
                {
                    node.label = selectedLabel.name;
                }
            }
            //DebugSphere(node, "Created");
            return node;
        }

        internal static void DebugSphere(Component node, String prefix)
        {
            Debug.Log(prefix + " sphere with components:");
            foreach (Component obj in node.GetComponents<Component>())
            {
                Debug.Log("\t\t" + obj);
            }
        }

        internal EdgeObject MakeConnection(NodeObject first, NodeObject second)
        {
            Debug.Log("Making connection from " + first + " to " + second);
            GameObject connection = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyColliders(connection);
            MeshRenderer renderer = connection.GetComponent<MeshRenderer>();
            renderer.material.color = Color.gray;
            connection.transform.localScale = Vector3.one * edgeScale;  // make very small at first
            EdgeObject edge = connection.AddComponent<EdgeObject>();
            //edge.floatingSpheres = this;
            edge.startNode = first;
            edge.endNode = second;
            edge.scale = edgeScale;
            edge.UpdateConnection();
            first.AddEdge(edge);
            second.AddEdge(edge);
            return edge;
        }

        internal static void DestroyColliders(GameObject gameObject)
        {
            foreach (Collider collider in gameObject.GetComponents<Collider>())
            {
                DestroyImmediate(collider);
            }
        }

        internal void DeleteObjectAt(NVRHand hand)
        {
            Debug.Log("Deleting object interacting with " + hand);
            if (hand.IsInteracting)
            {
                //DebugSphere(hand.CurrentlyInteracting, "Interacting with");
                NodeObject sphere = hand.CurrentlyInteracting.GetComponent<NodeObject>();
                if (sphere == null)
                {
                    Debug.Log("Hand is interacting with something that is not a NodeObject: " + hand.CurrentlyInteracting);
                }
                else
                {
                    Debug.Log("Deleting sphere " + sphere);
                    hand.EndInteraction(hand.CurrentlyInteracting);
                    sphere.Detach();
                    Destroy(sphere.gameObject, 0.1f);
                }
            }
            else if (hand.IsHovering)
            {
                Debug.Log("No interacting object, but hand is hovering");
            }
        }
    }
}
