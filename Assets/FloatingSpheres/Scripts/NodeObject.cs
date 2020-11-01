using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatingSpheres
{
    public class NodeObject : MonoBehaviour
    {
        public string label;
        private HashSet<EdgeObject> incoming = new HashSet<EdgeObject>();
        private HashSet<EdgeObject> outgoing = new HashSet<EdgeObject>();

        internal void Detach()
        {
            Detach(incoming);
            Detach(outgoing);
        }

        private void Detach(HashSet<EdgeObject> edges)
        {
            EdgeObject[] toDestroy = new EdgeObject[edges.Count];
            edges.CopyTo(toDestroy);
            edges.Clear();
            foreach (EdgeObject edge in toDestroy)
            {
                if (edge.startNode != null) edge.startNode.RemoveEdge(edge);
                if (edge.endNode != null) edge.endNode.RemoveEdge(edge);
                edge.startNode = null;
                edge.endNode = null;
                Destroy(edge.gameObject);
            }
        }

        internal EdgeObject FindEdge(NodeObject other)
        {
            EdgeObject edge = FindEdge(other, this, incoming);
            if (edge == null)
            {
                edge = FindEdge(this, other, outgoing);
            }
            return edge;
        }

        private EdgeObject FindEdge(NodeObject first, NodeObject second, HashSet<EdgeObject> edges)
        {
            HashSet<EdgeObject>.Enumerator edge = edges.GetEnumerator();
            while (edge.MoveNext())
            {
                EdgeObject current = edge.Current;
                if (current.endNode == second && current.startNode == first)
                {
                    return current;
                }
            }
            return null;
        }

        internal List<NodeObject> OtherNodes()
        {
            List<NodeObject> result = new List<NodeObject>();
            foreach (EdgeObject edge in incoming)
            {
                result.Add(edge.startNode);
            }
            foreach (EdgeObject edge in outgoing)
            {
                result.Add(edge.endNode);
            }
            return result;
        }

        internal void RemoveEdge(EdgeObject edge)
        {
            EdgeObject toRemove = null;
            if (edge.startNode == this)
            {
                toRemove = FindEdge(edge.startNode, edge.endNode, outgoing);
                outgoing.Remove(toRemove);
            }
            else if (edge.endNode == this)
            {
                toRemove = FindEdge(edge.endNode, edge.startNode, incoming);
                incoming.Remove(toRemove);
            }
            if (toRemove == null)
            {
                Debug.Log("Invalid edge: " + edge);
            }
        }

        internal void AddEdge(EdgeObject edge)
        {
            if (edge.startNode == this)
            {
                if (FindEdge(edge.startNode, edge.endNode, outgoing) == null)
                {
                    outgoing.Add(edge);
                }
            }
            else if (edge.endNode == this)
            {
                if (FindEdge(edge.endNode, edge.startNode, incoming) == null)
                {
                    incoming.Add(edge);
                }
            }
            else
            {
                Debug.Log("Invalid edge: " + edge);
            }
        }
    }
}
