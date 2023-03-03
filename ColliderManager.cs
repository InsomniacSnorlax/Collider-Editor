using System.Collections.Generic;
using UnityEngine;

namespace Snorlax.Colliders
{
    public class ColliderManager : MonoBehaviour
    {
        public List<ColliderInfo> colliders = new List<ColliderInfo>();
        public List<SelectionInfo> selectionInfo = new List<SelectionInfo>();
        public bool isGizmos;
        public bool isChecking;

        public Color GizmosColor = Color.green;
        public Color HandleColor = Color.green;

        public Color SelectedGizmosColor = Color.red;
        public Color SelectedHandleColor = Color.red;
        public float HandleSize = 0.5f;
    }

    [System.Serializable]
    public class ColliderInfo
    {
        public string Name;
        public Transform transform;
        public Collider collider;
        public ColliderType type;
    }

    public enum ColliderType { Box, Sphere, Capsule }

    [System.Serializable]
    public class SelectionInfo
    {
        public ColliderInfo colliderInfo;
        public bool hasCollider;
        public bool isSelected;
        public bool changeName;
        public bool isUsingCenter;
        public int selectedTransformIndex;
    }
}