using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public GameObject headset;
    public GameObject leftHand;
    public GameObject leftForearm;
    public GameObject leftElbow;
    public GameObject leftArm;
    public GameObject leftShoulder;

    public GameObject rightHand;
    public GameObject rightForearm;
    public GameObject rightElbow;
    public GameObject rightArm;
    public GameObject rightShoulder;
    private float limbScale = 1f;
    public float elbowBendFactor = 0.25f;
    public float elbowOutFactor = 0.1f;

    void Start()
    {
        limbScale = Mathf.Max(GetScale(leftHand), GetScale(rightHand));
    }

    private float GetScale(GameObject hand)
    {
        float scale = 0.01f;
        if (hand != null)
        {
            scale = Mathf.Max(scale, hand.transform.localScale.x);
            scale = Mathf.Max(scale, hand.transform.localScale.z);
            Debug.Log("Got limb scale " + scale);
        }
        return scale;
    }

    void FixedUpdate()
    {
        RepositionArm(leftHand, leftForearm, leftElbow, leftArm, leftShoulder, rightShoulder);
        RepositionArm(rightHand, rightForearm, rightElbow, rightArm, rightShoulder, leftShoulder);
    }

    private void RepositionArm(GameObject hand, GameObject forearm, GameObject elbow, GameObject arm, GameObject shoulder, GameObject otherShoulder)
    {
        if (hand == null || forearm == null || elbow == null || arm == null || shoulder == null)
        {
            Debug.LogError("Invalid objects: hand=" + hand + ", forearm=" + forearm + ", elbow=" + elbow + ", arm=" + arm + ", shoulder=" + shoulder);
        }
        else
        {
            float handScale = hand.transform.localScale.y;
            Vector3 forward = hand.transform.forward * handScale;
            Vector3 handPosition = hand.transform.position;// - forward;
            Vector3 shoulderPosition = shoulder.transform.position;
            Vector3 average = (handPosition + 2f * shoulderPosition) / 3f;
            Vector3 armPointing = handPosition - shoulderPosition;
            Vector3 left = (leftShoulder.transform.position - rightShoulder.transform.position).normalized;
            Vector3 side = (shoulderPosition - otherShoulder.transform.position).normalized;
            Vector3 down = Vector3.Cross(armPointing, left).normalized;
            elbow.transform.position = average + elbowBendFactor * down + elbowOutFactor * side;
            UpdateCylinderPosition(forearm, handPosition, elbow.transform.position);
            UpdateCylinderPosition(arm, shoulder.transform.position, elbow.transform.position);
        }
    }

    private void UpdateCylinderPosition(GameObject cylinder, Vector3 beginPoint, Vector3 endPoint)
    {
        Vector3 position = (beginPoint + endPoint) / 2.0f;
        cylinder.transform.position = position;
        cylinder.transform.LookAt(beginPoint);
        cylinder.transform.Rotate(new Vector3(90, 0, 0));
        Vector3 localScale = cylinder.transform.localScale;
        localScale.x = limbScale;
        localScale.y = (endPoint - beginPoint).magnitude / 2.0f;
        localScale.z = limbScale;
        cylinder.transform.localScale = localScale;
    }
}
