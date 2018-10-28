using NewtonVR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingSpheres
{
    public class FloatingSpheres : MonoBehaviour
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
            edge.floatingSpheres = this;
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
