using NewtonVR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingSpheres
{
    public class NodeEvents : MonoBehaviour
    {
        public FloatingSpheres floatingSpheres;
        public Camera head;
        public NVRHand hand;
        public NVRHand otherHand;
        public Transform elementToTrack;
        public string closeParameter = "pointingToElement";
        public string farParameter = "notPointingToElement";
        private Animator animToTrigger;
        private bool triggerWasPressed;
        private bool appMenuWasPressed;
        private bool twoHandInteraction;
        public GameObject miniMenuTransform;
        public GameObject mainMenuTransform;

        public void Start()
        {
            if (this.head == null)
            {
                this.head = this.GetComponent<Camera>();
                if (this.head == null)
                {
                    Debug.LogError("Could not find a Camera object from this: " + this);
                }
            }
            if (this.hand == null)
            {
                this.hand = this.GetComponent<NVRHand>();
                if (this.hand == null)
                {
                    Debug.LogError("Could not find an NVRHand object from this: " + this);
                }
            }
            if (this.elementToTrack != null)
            {
                this.animToTrigger = elementToTrack.GetComponent<Animator>();
                if (this.animToTrigger == null)
                {
                    Debug.LogError("Could not find an Animator object from element: " + this.elementToTrack);
                }
            }
            this.appMenuWasPressed = false;
            this.triggerWasPressed = false;
            ActivateMenu(false);
            if (floatingSpheres == null)
            {
                this.floatingSpheres = Component.FindObjectOfType<FloatingSpheres>();
            }
        }

        public void Update()
        {
            if (miniMenuTransform != null && miniMenuTransform.gameObject.activeSelf)
            {
                miniMenuTransform.transform.position = this.transform.position;
                miniMenuTransform.transform.Translate(new Vector3(0.07f, 0.02f, 0.03f));
                miniMenuTransform.transform.eulerAngles = this.transform.eulerAngles;
                miniMenuTransform.transform.Rotate(new Vector3(60, 0, 0));
            }
        }

        public void LateUpdate()
        {
            if (floatingSpheres != null && hand != null && otherHand != null && !hand.IsInteracting)
            {
                if (hand.HoldButtonPressed && otherHand.HoldButtonPressed)
                {
                    floatingSpheres.TwoHandedDrag();
                }
                else
                {
                    floatingSpheres.TwoHandedDragEnd();
                }
            }
        }

        public void FixedUpdate()
        {
            if (hand != null)
            {
                if (hand.Inputs[NVRButtons.ApplicationMenu].PressDown)
                {
                    appMenuWasPressed = true;
                }
                if (hand.Inputs[NVRButtons.ApplicationMenu].PressUp)
                {
                    if (appMenuWasPressed)
                    {
                        MenuPressed();
                    }
                    appMenuWasPressed = false;
                }
                if (hand.UseButtonDown)
                {
                    //if(!floatingSpheres.canvasInput.OnCanvas)
                    {
                        triggerWasPressed = true;
                    }
                }
                if (hand.UseButtonUp)
                {
                    if (triggerWasPressed)// && !floatingSpheres.canvasInput.OnCanvas)
                    {
                        TriggerPressed();
                    }
                    triggerWasPressed = false;
                }
                if (hand.IsInteracting)
                {
                    this.twoHandInteraction = false;
                    if (otherHand != null)
                    {
                        if (otherHand.IsInteracting)
                        {
                            this.twoHandInteraction = true;
                            if (otherHand.CurrentlyInteracting == hand.CurrentlyInteracting)
                            {
                                // NewtonVR already filters this out, so we would need more changes to get to this point
                                Debug.Log("Two hands interacting with same object: " + hand.CurrentlyInteracting);
                            }
                            else
                            {
                                //FloatingSpheres.DebugSphere(hand.CurrentlyInteracting, "Interacting with");
                                NodeObject first = hand.CurrentlyInteracting.GetComponent<NodeObject>();
                                NodeObject second = otherHand.CurrentlyInteracting.GetComponent<NodeObject>();
                                if (first == null)
                                {
                                    Debug.Log("Hand is interacting with something that is not a NodeObject: " + hand.CurrentlyInteracting);
                                }
                                else if (second == null)
                                {
                                    Debug.Log("Other hand is interacting with something that is not a NodeObject: " + hand.CurrentlyInteracting);
                                }
                                else
                                {
                                    EdgeObject edge = first.FindEdge(second);
                                    if (edge == null)
                                    {
                                        Debug.Log("No edge exists - making one");
                                        edge = floatingSpheres.MakeConnection(first, second);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("No hand defined for " + this);
            }
            //Debug.Log("FixedUpdate: " + this);
        }

        private void MenuPressed()
        {
            Debug.Log("Menu pressed");
            ActivateMenu(!IsMenuActive());
        }

        private bool IsMenuActive()
        {
            return false || (miniMenuTransform != null && miniMenuTransform.gameObject.activeSelf)
                 || (mainMenuTransform != null && mainMenuTransform.gameObject.activeSelf);
        }

        private void ActivateMenu(bool active)
        {
            ActivateMenu(miniMenuTransform, active);
            ActivateMenu(mainMenuTransform, active);
        }

        private void ActivateMenu(GameObject menu, bool active)
        {
            if (menu != null)
            {
                if (active && menu == mainMenuTransform)
                {
                    float height = menu.transform.position.y;
                    Vector3 pos = this.head.transform.position;
                    menu.transform.position = new Vector3(pos.x, menu.transform.position.y, pos.z);
                    menu.transform.Translate(Vector3.forward * 4, head.transform);
                    menu.transform.Translate(Vector3.right * 0.5f);
                    Vector3 m = menu.transform.position;
                    menu.transform.position = new Vector3(m.x, height, m.z);
                    menu.transform.eulerAngles = new Vector3(0, head.transform.eulerAngles.y, 0);
                }
                menu.SetActive(active);
            }
        }

        private void TriggerPressed()
        {
            Debug.Log("Trigger is pressed, we should create or delete: " + (floatingSpheres == null ? "null" : floatingSpheres.createMode.ToString()));
            if (floatingSpheres == null || floatingSpheres.createMode)
            {
                TriggerCreate();
            }
            else
            {
                TriggerDelete();
            }
        }

        private void TriggerDelete()
        {
            floatingSpheres.DeleteObjectAt(this.hand);
        }

        private void TriggerCreate()
        {
            float dx = hand.transform.position.x - head.transform.position.x;
            float dz = hand.transform.position.z - head.transform.position.z;
            float x = head.transform.position.x + 2 * dx;
            float z = head.transform.position.z + 2 * dz;
            float height = 1;
            if (elementToTrack != null)
            {
                height = Math.Max(height, elementToTrack.position.y);
                if (this.animToTrigger != null)
                {
                    double angleToHand = EdgeObject.Azimuth(head.transform, hand.transform);
                    double angleToElement = EdgeObject.Azimuth(head.transform, elementToTrack.transform);
                    Debug.Log("AngleToHand: " + angleToHand);
                    Debug.Log("AngleToElement: " + angleToElement);
                    Debug.Log("Difference between hand and element: " + Math.Abs(angleToElement - angleToHand));
                    if (Math.Abs(angleToElement - angleToHand) < 0.1)
                    {
                        this.animToTrigger.SetTrigger(this.closeParameter);
                        this.animToTrigger.ResetTrigger(this.farParameter);
                    }
                    else
                    {
                        this.animToTrigger.ResetTrigger(this.closeParameter);
                        this.animToTrigger.SetTrigger(this.farParameter);
                    }
                }
                else
                {
                    Debug.LogError("Could not find animation component on " + elementToTrack);
                }
            }
            height = Math.Max(height, Math.Max(head.transform.position.y, hand.transform.position.y));
            floatingSpheres.MakeOne(x, height, z);
        }

        public void DoButtonDown()
        {
            Debug.Log("Do button down: " + this);
        }
        public void DoButtonUp()
        {
            Debug.Log("Do button up: " + this);
        }
        public void DoHovering()
        {
            Debug.Log("Do hovering: " + this);
        }
        public void DoStartInteraction()
        {
            Debug.Log("Do start interaction: " + this);
        }
        public void DoEndInteraction()
        {
            Debug.Log("Do end interaction: " + this);
        }
    }
}
