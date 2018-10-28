using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingSpheres
{
    [DataContract]
    public class EdgeObject : MonoBehaviour
    {
        public FloatingSpheres floatingSpheres;
        [DataMember]
        public NodeObject startNode;
        [DataMember]
        public NodeObject endNode;
        internal float scale;
        internal float targetLength = 1.0f;

        void FixedUpdate()
        {
            UpdateConnection();
        }

        public void UpdateConnection()
        {
            if (startNode != null && endNode != null)
            {
                UpdateNodePositions(this.startNode, this.endNode);
                UpdateCylinderPosition(this.gameObject, this.startNode.gameObject.transform.position, this.endNode.gameObject.transform.position);
                //UpdateCylinderPosition(this.gameObject, this.startNode.gameObject.transform.position, this.endNode.gameObject.transform.position);
            }
        }

        private void UpdateNodePositions(NodeObject startNode, NodeObject endNode)
        {
            Vector3 startPoint = startNode.gameObject.transform.position;
            Vector3 endPoint = endNode.gameObject.transform.position;
            Vector3 offset = endPoint - startPoint;
            float length = offset.magnitude;
            float shiftBy = (length - targetLength * floatingSpheres.modelScale) * 0.05f;
            if (System.Math.Abs(shiftBy) > 0.001d)
            {
                Vector3 shift = new Vector3(offset.x * shiftBy, offset.y * shiftBy, offset.z * shiftBy);
                startNode.gameObject.transform.position = startPoint + shift;
                endNode.gameObject.transform.position = endPoint - shift;
            }
        }

        private void UpdateCylinderPosition(GameObject cylinder, Vector3 beginPoint, Vector3 endPoint)
        {
            Vector3 offset = endPoint - beginPoint;
            Vector3 position = beginPoint + (offset / 2.0f);
            cylinder.transform.position = position;
            cylinder.transform.LookAt(beginPoint);
            Vector3 localScale = cylinder.transform.localScale;
            localScale.z = (endPoint - beginPoint).magnitude / 1.0f;
            localScale.x = floatingSpheres.modelScale * scale;
            localScale.y = floatingSpheres.modelScale * scale;
            cylinder.transform.localScale = localScale;
        }

        public void UpdateConnectionX()
        {
            GameObject first = this.startNode.gameObject;
            GameObject second = this.endNode.gameObject;
            GameObject connection = this.gameObject;
            float x = first.transform.position.x;
            float y = first.transform.position.y;
            float z = first.transform.position.z;
            float dx = second.transform.position.x - first.transform.position.x;
            float dy = second.transform.position.y - first.transform.position.y;
            float dz = second.transform.position.z - first.transform.position.z;
            double length = System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
            double tilt = ToDegrees(Tilt(first.transform, second.transform));
            double azimuth = ToDegrees(Azimuth(first.transform, second.transform));
            float width = (float)System.Math.Max(0.03, floatingSpheres.modelScale * scale / 100);
            float height = (float)System.Math.Min(0.1, length / 2);
            Debug.Log("Tilt: " + tilt);
            Debug.Log("Azimuth: " + azimuth);
            Vector3 halfWayToSecond = new Vector3(dx / 4, dy / 4, dz / 4);
            connection.transform.Translate(halfWayToSecond);
            connection.transform.eulerAngles = new Vector3((float)tilt, 0, (float)azimuth);
            //connection.transform.eulerAngles = new Vector3((float)tilt, 0, (float)azimuth);
            connection.transform.localScale = new Vector3(width, height, width);
            connection.transform.position = new Vector3(x, y, z);
        }

        public static double ToDegrees(double angle)
        {
            return 180.0 * angle / System.Math.PI;
        }

        public static double Azimuth(Transform from, Transform to)
        {
            float dx = to.position.x - from.position.x;
            float dz = to.position.z - from.position.z;
            double r = System.Math.Sqrt(dx * dx + dz * dz);
            double azimuth = System.Math.Asin(dx / r);
            return azimuth;
        }

        public static double Tilt(Transform from, Transform to)
        {
            float dx = to.position.x - from.position.x;
            float dy = to.position.y - from.position.y;
            float dz = to.position.z - from.position.z;
            double r = System.Math.Sqrt(dx * dx + dz * dz);
            double d = System.Math.Sqrt(dx * dx + dz * dz + dy * dy);
            double tilt = System.Math.Asin(r / d);
            return tilt;
        }
    }
}
