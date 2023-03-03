using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Snorlax.Colliders
{
    [CustomEditor(typeof(ColliderManager))]

    public class ColliderEditor : Editor
    {
        #region Variables
        // Styles
        ColliderManager main;
        private GUIStyle leftButton;
        private Color color_selected = Color.grey;
        private Color color_confirm = Color.green;
        private Color color_notFound = Color.red;
        private Color color_default;

        // Variables
        private ColliderType colliderType;
        private Animator animator;
        private Transform targetTransform;
        private HumanBodyBones humanBodyBones;

        // Textures
        private Texture2D iconRename;
        private Texture2D iconTrash;
        private Texture2D[] selectedTexture = new Texture2D[2];
        private Texture2D[] selectedTransform = new Texture2D[2];
        private Texture2D[] TabTransform = new Texture2D[3];

        // Editor Values
        private string[] TabNames = { "Create", "Edit", "Debug" };
        private bool EditFoldout;
        private string EditFoldoutString = "Transform";
        private int TabSelect = 0;
        private int seleactedTextureIndex = 0;
        private int ToolSelect = 0;
        private int InspectorSelect = 0;
        Vector2 scrollView = Vector2.zero;
        #endregion

        #region Main Methods
        private void OnEnable()
        {
            main = (ColliderManager)target;
            animator = main.GetComponent<Animator>();
            selectedTexture[0] = EditorGUIUtility.FindTexture("PreMatCube");
            selectedTexture[1] = EditorGUIUtility.FindTexture("AvatarSelector");

            selectedTransform[0] = EditorGUIUtility.FindTexture("d_AvatarPivot");
            selectedTransform[1] = EditorGUIUtility.FindTexture("d_ToolHandleLocal");

            iconRename = EditorGUIUtility.FindTexture("SceneViewTools");
            iconTrash = EditorGUIUtility.FindTexture("TreeEditor.Trash");

            TabTransform[0] = EditorGUIUtility.FindTexture("MoveTool");
            TabTransform[1] = EditorGUIUtility.FindTexture("RotateTool");
            TabTransform[2] = EditorGUIUtility.FindTexture("ScaleTool");
        }

        public override void OnInspectorGUI()
        {
            if (leftButton == null)
            {
                leftButton = new GUIStyle("toolbarbutton");
                leftButton.alignment = TextAnchor.MiddleLeft;
                color_default = GUI.backgroundColor;
            }

            EditorGUI.BeginChangeCheck();
            {
                Tab();
            }
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI()
        {
            if (!main.isGizmos)
                return;

            for(int i = 0; i < main.selectionInfo.Count; i++)
            {
                SelectionInfo info = main.selectionInfo[i];

                if (!info.isSelected)
                    continue;

                Color gizmos = InspectorSelect == i ? main.SelectedGizmosColor : main.GizmosColor;
                Color handle = InspectorSelect == i ? main.SelectedHandleColor : main.HandleColor;

                switch (info.colliderInfo.type)
                {
                    case ColliderType.Box:
                        var box = (BoxCollider)info.colliderInfo.collider;
                        DrawBounds(info.colliderInfo.transform, box.center, box.size, gizmos);

                        if (TabSelect == 1)
                        {
                            if (ToolSelect == 0) box.center = DrawPositionHandle(info.colliderInfo.transform, box.center, info.isUsingCenter);
                            else if (ToolSelect == 1) DrawRotationHandle(info.colliderInfo.transform);
                            else if (ToolSelect == 2) DrawBoundsHandle(info.colliderInfo.transform, box.center, handle, box);
                        }
                        break;

                    case ColliderType.Sphere:
                        var sph = (SphereCollider)info.colliderInfo.collider;
                        DrawSphere(info.colliderInfo.transform, sph.center, gizmos, sph.radius);

                        if (TabSelect == 1)
                        {
                            if (ToolSelect == 0) sph.center = DrawPositionHandle(info.colliderInfo.transform, sph.center, info.isUsingCenter);
                            else if (ToolSelect == 1) DrawRotationHandle(info.colliderInfo.transform);
                            else if (ToolSelect == 2) DrawSphereHandle(info.colliderInfo.transform, sph.center, handle, sph);
                        }
                        break;

                    case ColliderType.Capsule:
                        var cap = (CapsuleCollider)info.colliderInfo.collider;
                        DrawCapsule(info.colliderInfo.transform, cap.center, cap.height, gizmos, cap.radius);

                        if (TabSelect == 1)
                        {
                            if (ToolSelect == 0) cap.center = DrawPositionHandle(info.colliderInfo.transform, cap.center, info.isUsingCenter);
                            else if (ToolSelect == 1) DrawRotationHandle(info.colliderInfo.transform);
                            else if (ToolSelect == 2) DrawCapsuleHandle(info.colliderInfo.transform, cap.center, cap.height, handle, cap);
                        }
                        break;
                }
            }
        }
        #endregion

        #region Draw 
        public static void DrawCapsule(Transform transform, Vector3 center, float height, Color color, float radius = 1)
        {
            Matrix4x4 oldMatrix = Handles.matrix;
            Handles.matrix = transform.localToWorldMatrix;//Matrix4x4.TRS(transform.position + center, transform.rotation, Vector3.one);

            Vector3 origin = Vector3.zero + center;

            Vector3 start = new Vector3(origin.x, origin.y + height / 2, origin.z);
            Vector3 end = new Vector3(origin.x, origin.y - height / 2, origin.z);

            Vector3 up = (end - start).normalized * radius;
            Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
            Vector3 right = Vector3.Cross(up, forward).normalized * radius;

            Color oldColor = Handles.color;
            Handles.color = color;

            //float height = (start-end).magnitude;
            float sideLength = Mathf.Max(0, (height * 0.5f) - radius);
            Vector3 middle = (end + start) * 0.5f;

            start = middle + ((start - middle).normalized * sideLength);
            end = middle + ((end - middle).normalized * sideLength);

            //Radial circles
            DrawCircle(start, up, color, radius);
            DrawCircle(end, -up, color, radius);

            //Side lines
            Handles.DrawLine(start + right, end + right);
            Handles.DrawLine(start - right, end - right);

            Handles.DrawLine(start + forward, end + forward);
            Handles.DrawLine(start - forward, end - forward);

            for (int i = 1; i < 26; i++)
            {

                //Start endcap
                Handles.DrawLine(Vector3.Slerp(right, -up, i / 25.0f) + start, Vector3.Slerp(right, -up, (i - 1) / 25.0f) + start);
                Handles.DrawLine(Vector3.Slerp(-right, -up, i / 25.0f) + start, Vector3.Slerp(-right, -up, (i - 1) / 25.0f) + start);
                Handles.DrawLine(Vector3.Slerp(forward, -up, i / 25.0f) + start, Vector3.Slerp(forward, -up, (i - 1) / 25.0f) + start);
                Handles.DrawLine(Vector3.Slerp(-forward, -up, i / 25.0f) + start, Vector3.Slerp(-forward, -up, (i - 1) / 25.0f) + start);

                //End endcap
                Handles.DrawLine(Vector3.Slerp(right, up, i / 25.0f) + end, Vector3.Slerp(right, up, (i - 1) / 25.0f) + end);
                Handles.DrawLine(Vector3.Slerp(-right, up, i / 25.0f) + end, Vector3.Slerp(-right, up, (i - 1) / 25.0f) + end);
                Handles.DrawLine(Vector3.Slerp(forward, up, i / 25.0f) + end, Vector3.Slerp(forward, up, (i - 1) / 25.0f) + end);
                Handles.DrawLine(Vector3.Slerp(-forward, up, i / 25.0f) + end, Vector3.Slerp(-forward, up, (i - 1) / 25.0f) + end);
            }
            Handles.matrix = oldMatrix;
            Handles.color = oldColor;
        }

        public static void DrawCircle(Vector3 position, Vector3 up, Color color, float radius = 1.0f)
        {
            up = ((up == Vector3.zero) ? Vector3.up : up).normalized * radius;
            Vector3 _forward = Vector3.Slerp(up, -up, 0.5f);
            Vector3 _right = Vector3.Cross(up, _forward).normalized * radius;

            Matrix4x4 matrix = new Matrix4x4();

            matrix[0] = _right.x;
            matrix[1] = _right.y;
            matrix[2] = _right.z;

            matrix[4] = up.x;
            matrix[5] = up.y;
            matrix[6] = up.z;

            matrix[8] = _forward.x;
            matrix[9] = _forward.y;
            matrix[10] = _forward.z;

            Vector3 _lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
            Vector3 _nextPoint = Vector3.zero;

            Color oldColor = Handles.color;
            Handles.color = (color == default(Color)) ? Color.white : color;

            for (var i = 0; i < 91; i++)
            {
                _nextPoint.x = Mathf.Cos((i * 4) * Mathf.Deg2Rad);
                _nextPoint.z = Mathf.Sin((i * 4) * Mathf.Deg2Rad);
                _nextPoint.y = 0;

                _nextPoint = position + matrix.MultiplyPoint3x4(_nextPoint);

                Handles.DrawLine(_lastPoint, _nextPoint);
                _lastPoint = _nextPoint;
            }

            Handles.color = oldColor;
        }

        public static void DrawBounds(Transform transform, Vector3 center, Vector3 size, Color color)
        {
            Color oldColor = Handles.color;
            Handles.color = color;

            Matrix4x4 oldMatrix = Handles.matrix;
            Handles.matrix = transform.localToWorldMatrix;

            Handles.DrawWireCube(Vector3.zero + center, size);

            Handles.matrix = oldMatrix;
            Handles.color = oldColor;
        }

        public static void DrawSphere(Transform transform, Vector3 center, Color color, float radius = 1.0f)
        {
            Color oldColor = Handles.color;
            Handles.color = color;

            Matrix4x4 oldMatrix = Handles.matrix;
            Handles.matrix = transform.localToWorldMatrix;

            Vector3 origin = Vector3.zero + center;
            Handles.DrawWireDisc(origin, Vector3.right, radius);
            Handles.DrawWireDisc(origin, Vector3.up, radius);
            Handles.DrawWireDisc(origin, Vector3.forward, radius);

            if (Camera.current.orthographic)
            {
                Vector3 normal = origin - Handles.inverseMatrix.MultiplyVector(Camera.current.transform.forward);
                float sqrMagnitude = normal.sqrMagnitude;
                float num0 = radius * radius;
                Handles.DrawWireDisc(origin - num0 * normal / sqrMagnitude, normal, radius);
            }
            else
            {
                Vector3 normal = origin - Handles.inverseMatrix.MultiplyPoint(Camera.current.transform.position);
                float sqrMagnitude = normal.sqrMagnitude;
                float num0 = radius * radius;
                float num1 = num0 * num0 / sqrMagnitude;
                float num2 = Mathf.Sqrt(num0 - num1);
                Handles.DrawWireDisc(origin - num0 * normal / sqrMagnitude, normal, num2);
            }

            Handles.matrix = oldMatrix;
            Handles.color = oldColor;
        }
        #endregion

        #region Handle
        private void DrawCapsuleHandle(Transform transform, Vector3 center, float height, Color color, CapsuleCollider cap)
        {
            Color oldColor = Handles.color;
            Handles.color = color;

            Matrix4x4 oldMatrix = Handles.matrix;
            Handles.matrix = transform.localToWorldMatrix; //Matrix4x4.TRS(transform.position + center, transform.rotation, Vector3.one);

            float size = HandleUtility.GetHandleSize(Vector3.zero) * main.HandleSize;
            cap.height = Handles.ScaleValueHandle(cap.height, new Vector3(center.x, height / 2 + center.y, center.z), Quaternion.identity, size, Handles.DotHandleCap, 0.5f);
            cap.radius = Handles.ScaleValueHandle(cap.radius, new Vector3(center.x, center.y, cap.radius + center.z), Quaternion.identity, size, Handles.DotHandleCap, 0.5f);

            Handles.matrix = oldMatrix;
            Handles.color = oldColor;
        }

        private void DrawSphereHandle(Transform transform, Vector3 center, Color color, SphereCollider sph)
        {
            Color oldColor = Handles.color;
            Handles.color = color;

            Matrix4x4 oldMatrix = Handles.matrix;
            Handles.matrix = transform.localToWorldMatrix; //Matrix4x4.TRS(transform.position + center, transform.rotation, Vector3.one);

            float size = HandleUtility.GetHandleSize(Vector3.zero) * main.HandleSize;
            sph.radius = Handles.ScaleValueHandle(sph.radius, new Vector3(center.x, sph.radius + center.y, center.z), Quaternion.identity, size, Handles.DotHandleCap, 0.5f);
            sph.radius = Handles.ScaleValueHandle(sph.radius, new Vector3(-sph.radius + center.x, center.y, center.z), Quaternion.identity, size, Handles.DotHandleCap, 0.5f);
            sph.radius = Handles.ScaleValueHandle(sph.radius, new Vector3(center.x, center.y, -sph.radius + center.z), Quaternion.identity, size, Handles.DotHandleCap, 0.5f);

            Handles.matrix = oldMatrix;
            Handles.color = oldColor;
        }

        private void DrawBoundsHandle(Transform transform, Vector3 center, Color color, BoxCollider box)
        {
            Color oldColor = Handles.color;
            Handles.color = color;

            Matrix4x4 oldMatrix = Handles.matrix;
            Handles.matrix = transform.localToWorldMatrix; //Matrix4x4.TRS(transform.position + center, transform.rotation, Vector3.one);

            float size = HandleUtility.GetHandleSize(Vector3.zero) * main.HandleSize;
            Vector3 bounds = new Vector3();
            bounds.x = Handles.ScaleValueHandle(box.size.x, new Vector3(-box.size.x / 2 + center.x, center.y, center.z), Quaternion.identity, size, Handles.DotHandleCap, 0.5f);
            bounds.y = Handles.ScaleValueHandle(box.size.y, new Vector3(center.x, box.size.y / 2 + center.y, center.z), Quaternion.identity, size, Handles.DotHandleCap, 0.5f);
            bounds.z = Handles.ScaleValueHandle(box.size.z, new Vector3(center.x, center.y, box.size.z / 2 + center.z), Quaternion.identity, size, Handles.DotHandleCap, 0.5f);
            box.size = bounds;

            Handles.matrix = oldMatrix;
            Handles.color = oldColor;
        }

        private Vector3 DrawPositionHandle(Transform transform, Vector3 center, bool isCentered)
        {
            if (isCentered)
            {
                Matrix4x4 oldMatrix = Handles.matrix;
                Handles.matrix = transform.localToWorldMatrix;                                                                                                            //center = Handles.DoPositionHandle(center, transform.localToWorldMatrix.rotation);
                center = Handles.DoPositionHandle(center, Quaternion.identity);
                Handles.matrix = oldMatrix;
            }
            else
            {
                transform.position = Handles.DoPositionHandle(transform.position, Quaternion.identity);
            }

            return center;
        }

        private void DrawRotationHandle(Transform transform)
        {
            //Matrix4x4 oldMatrix = Handles.matrix;
            //Handles.matrix = Matrix4x4.TRS(transform.localPosition + center, transform.localToWorldMatrix.rotation, transform.localToWorldMatrix.lossyScale);
            transform.rotation = Handles.DoRotationHandle(transform.rotation, transform.position);
            //Handles.matrix = oldMatrix;

            /*
             Matrix4x4 oldMatrix = Handles.matrix;
            Handles.matrix = Matrix4x4.TRS(transform.TransformPoint(center), transform.localToWorldMatrix.rotation, transform.localToWorldMatrix.lossyScale);
            transform.rotation = Handles.DoRotationHandle(transform.localToWorldMatrix.rotation, Vector3.zero);

            //transform.RotateAround(center, target,target.eulerAngles)
            Handles.matrix = oldMatrix;


            */

            /// return rotation;
        }

        #endregion

        #region Methods
        private void CreateTab()
        {
            GUILayout.BeginVertical("Box");
            {
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Find Colliders"))
                    {
                        main.isChecking = false;
                        var list = main.GetComponentsInChildren<Collider>();

                        //main.colliders.Clear();

                        foreach (Collider temp in list)
                        {
                            var type = temp.GetType();
                            ColliderType colliderType =
                                type == typeof(CapsuleCollider) ? ColliderType.Capsule :
                                type == typeof(BoxCollider) ? ColliderType.Box :
                                ColliderType.Sphere;

                            ColliderInfo info = new ColliderInfo() { Name = temp.name, transform = temp.transform, collider = temp, type = colliderType };

                            if (!HasValue(main.colliders, info))
                            {
                                main.colliders.Add(info);

                                main.selectionInfo.Add(new SelectionInfo() { colliderInfo = info });
                            }
                        }
                    }

                    if (GUILayout.Button("Check Colliders"))
                    {
                        main.isChecking = !main.isChecking;

                        if (!main.isChecking) return;

                        var list = main.GetComponentsInChildren<Collider>().ToList();

                        foreach (SelectionInfo info in main.selectionInfo)
                        {
                            if (info.colliderInfo.collider == null)
                            {
                                info.hasCollider = false;
                                continue;
                            }

                            info.hasCollider = list.Contains(info.colliderInfo.collider);
                        }
                    }

                    if (GUILayout.Button("Clear List"))
                    {
                        main.isChecking = false;
                        main.colliders.Clear();
                        main.selectionInfo.Clear();
                    }

                }
                GUILayout.EndHorizontal();

                main.isGizmos = EditorGUILayout.Toggle("Gizmos", main.isGizmos);

                GUILayout.BeginHorizontal();
                {
                    if (seleactedTextureIndex == 0)
                    {
                        targetTransform = (Transform)EditorGUILayout.ObjectField("Transform", targetTransform, typeof(Transform), true);
                    }
                    else if (seleactedTextureIndex == 1)
                    {
                        humanBodyBones = (HumanBodyBones)EditorGUILayout.EnumPopup("Transform", humanBodyBones);
                    }

                    seleactedTextureIndex = GUILayout.SelectionGrid(seleactedTextureIndex, selectedTexture, 2, GUILayout.Width(55));
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    colliderType = (ColliderType)EditorGUILayout.EnumPopup("Collider Type", colliderType);


                    if (GUILayout.Button("Add Collider", GUILayout.Width(80)))
                    {
                        if (seleactedTextureIndex == 1 && animator == null)
                        {
                            Debug.LogError("No Animator detected");
                            return;
                        }

                        if (seleactedTextureIndex == 0 && targetTransform == null)
                        {
                            Debug.LogError("No Target Tranform detected");
                            return;
                        }

                        var trans = seleactedTextureIndex == 0 ? targetTransform : animator.GetBoneTransform(humanBodyBones);
                        var gameObject = new GameObject($"{trans.name} {colliderType.ToString()} Collider");
                        gameObject.transform.SetParent(trans, false);
                        Transform target = gameObject.transform;

                        var col =
                            ColliderType.Box == colliderType ? (Collider)gameObject.AddComponent<BoxCollider>() :
                            ColliderType.Capsule == colliderType ? (Collider)gameObject.AddComponent<CapsuleCollider>() :
                            (Collider)gameObject.AddComponent<SphereCollider>();


                        ColliderInfo info = new ColliderInfo() { Name = $"{trans.name} {colliderType.ToString()} Collider", transform = target, collider = col, type = colliderType };

                        main.selectionInfo.Add(new SelectionInfo() { colliderInfo = info, isSelected = true });

                        main.colliders.Add(info);
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            scrollView = GUILayout.BeginScrollView(scrollView, GUILayout.MinHeight(150));
            {
                for (int i = 0; i < main.selectionInfo.Count; i++)
                {
                    ColliderInfo info = main.selectionInfo[i].colliderInfo;
                    SelectionInfo selection = main.selectionInfo[i];
                    if (main.isChecking)
                        GUI.backgroundColor = selection.hasCollider ? color_confirm : color_notFound;
                    else
                        GUI.backgroundColor = selection.isSelected ? color_selected : color_default;

                    GUILayout.BeginHorizontal();
                    {
                        if (!selection.changeName)
                        {
                            if (GUILayout.Button(info.Name, leftButton))
                            {
                                selection.isSelected = !selection.isSelected;
                            }
                        }
                        else
                        {
                            info.Name = GUILayout.TextField(info.Name, leftButton);

                            if (info.transform != null) info.transform.gameObject.name = info.Name;
                        }
                        GUI.backgroundColor = color_default;

                        GUI.backgroundColor = selection.changeName ? color_selected : color_default;

                        if (GUILayout.Button(iconRename, GUILayout.Width(27)))
                        {
                            selection.changeName = !selection.changeName;
                        }

                        GUI.backgroundColor = color_default;
                        //selectionInfo[i].selected = GUILayout.SelectionGrid(selectionInfo[i].selected, selectingStrings, 2, GUILayout.Width(55));

                        if (info.transform == main.transform)
                            GUI.enabled = false;

                        if (GUILayout.Button(iconTrash, GUILayout.Width(27)))
                        {
                            var temp = info.transform;
                            main.selectionInfo.RemoveAt(i);
                            if (temp != null)
                                DestroyImmediate(temp.gameObject);

                        }
                        GUI.enabled = true;
                    }
                    GUILayout.EndHorizontal();
                }
                GUI.backgroundColor = color_default;
            }
            GUILayout.EndScrollView();
        }

        private void EditTab()
        {
            main.GizmosColor = EditorGUILayout.ColorField("Gizmos Color", main.GizmosColor);
            main.HandleColor = EditorGUILayout.ColorField("Handle Color", main.HandleColor);
            main.SelectedGizmosColor = EditorGUILayout.ColorField("Selected Gizmos Color", main.SelectedGizmosColor);
            main.SelectedHandleColor = EditorGUILayout.ColorField("Selected Handle Color", main.SelectedHandleColor);
            main.HandleSize = EditorGUILayout.FloatField("Handle Size", main.HandleSize);

            ToolSelect = GUILayout.Toolbar(ToolSelect, TabTransform);
            Tool currentTool = Tools.current;
            Tools.current = Tool.None;
            switch (currentTool)
            {
                case Tool.Move:
                    ToolSelect = 0;
                    break;
                case Tool.Rotate:
                    ToolSelect = 1;
                    break;
                case Tool.Scale:
                    ToolSelect = 2;
                    break;
            }

            EditFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(EditFoldout, EditFoldoutString);

            if (EditFoldout)
            {
                if (main.selectionInfo[InspectorSelect] != null && main.selectionInfo[InspectorSelect].isSelected == true)
                {
                    EditFoldoutString = main.selectionInfo[InspectorSelect].colliderInfo.Name;
                    ColliderInspector(main.selectionInfo[InspectorSelect].colliderInfo);
                }
            }
            else
            {
                EditFoldoutString = "Transform";
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            for (int i=0;i <main.selectionInfo.Count;i++)
            {
                SelectionInfo info = main.selectionInfo[i];
                if (!info.isSelected)
                    continue;
                GUI.backgroundColor = i == InspectorSelect ? color_selected : color_default;

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(info.colliderInfo.Name, leftButton))
                    {
                        InspectorSelect = i;
                    }
                    GUI.backgroundColor = color_default;
                    if(ToolSelect == 0) info.selectedTransformIndex = GUILayout.SelectionGrid(info.selectedTransformIndex, selectedTransform, 2, GUILayout.Width(55));
                    info.isUsingCenter = info.selectedTransformIndex == 1;
                }
                EditorGUILayout.EndHorizontal();

            }

            GUI.backgroundColor = color_default;
        }

        private void ColliderInspector(ColliderInfo colliderInfo)
        {
            GUILayout.BeginVertical("Box");

            colliderInfo.transform.position = EditorGUILayout.Vector3Field("Transform",colliderInfo.transform.position);
            colliderInfo.transform.localEulerAngles = EditorGUILayout.Vector3Field("Transform", colliderInfo.transform.localEulerAngles);
            colliderInfo.collider.isTrigger = EditorGUILayout.Toggle("Is Trigger", colliderInfo.collider.isTrigger);
            colliderInfo.collider.material = (PhysicMaterial)EditorGUILayout.ObjectField("Physics Material", colliderInfo.collider.material, typeof(PhysicMaterial), true);
            switch (colliderInfo.type)
            {
                case ColliderType.Box:
                    var box = (BoxCollider)colliderInfo.collider;
                    BoxInspector(box);
                    break;
                case ColliderType.Sphere:
                    var sph = (SphereCollider)colliderInfo.collider;
                    SphereInspector(sph);
                    break;
                case ColliderType.Capsule:
                    var cap = (CapsuleCollider)colliderInfo.collider;
                    CapsuleInspector(cap);
                    break;
            }

            GUILayout.EndVertical();

            GUILayout.Space(1);
        }

        private void DebugTab()
        {
            base.OnInspectorGUI();
        }

        private void Tab()
        {
            TabSelect = GUILayout.Toolbar(TabSelect, TabNames);

            switch (TabSelect)
            {
                case 0:
                    CreateTab();
                    break;
                case 1:
                    EditTab();
                    break;
                case 2:
                    DebugTab();
                    break;
            }
        }

        private bool HasValue(List<ColliderInfo> list, ColliderInfo target)
        {
            bool check1 = false;
            bool check2 = false;
            bool check3 = false;

            foreach (ColliderInfo info in list)
            {
                if (!check1) check1 = info.collider == target.collider;
                if (!check2) check2 = info.transform == target.transform;
                if (!check3) check3 = info.type == target.type;

                if (check1 && check2 && check3)
                    return true;
            }
            return false;
        }
        #endregion

        #region Inspectors
        private void CapsuleInspector(CapsuleCollider collider)
        {
            collider.center = EditorGUILayout.Vector3Field("Center", collider.center);
            collider.radius = EditorGUILayout.FloatField("Radius", collider.radius);
            collider.height = EditorGUILayout.FloatField("Height", collider.height);
        }

        private void BoxInspector(BoxCollider collider)
        {
            collider.center = EditorGUILayout.Vector3Field("Center", collider.center);
            collider.size = EditorGUILayout.Vector3Field("Size", collider.size);
        }

        private void SphereInspector(SphereCollider collider)
        {
            collider.center = EditorGUILayout.Vector3Field("Center", collider.center);
            collider.radius = EditorGUILayout.FloatField("Radius", collider.radius);
        }
        #endregion
    }
}