using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FigmaUIGuides.Editor
{
    [InitializeOnLoad]
    public static class SmartRulersCore
    {
        private enum DragState { None, DraggingNewH, DraggingNewV, DraggingExistingH, DraggingExistingV }
        private static DragState dragState = DragState.None;
        private static int draggedIndex = -1;
        private static float currentDragWorldPos = 0f;

        private const float RULER_SIZE = 20f;
        private const float SNAP_THRESHOLD_GUI = 10f; // pixels

        static SmartRulersCore()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        public static void ClearAllGuides()
        {
            if (SmartRulersSceneData.Instance != null)
            {
                SmartRulersSceneData.Instance.horizontalGuides.Clear();
                SmartRulersSceneData.Instance.verticalGuides.Clear();
                SmartRulersSceneData.Save();
                SceneView.RepaintAll();
            }
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!SmartGuidesSettingsWindow.IsEnabled) return;

            try
            {
                // Handle mouse events for guide dragging (works on all event types)
                if (SmartGuidesSettingsWindow.ShowRulers)
                {
                    HandleMouseEvents(sceneView);
                }

                // Snapping also needs to work during drag events, not just Repaint
                if (SmartGuidesSettingsWindow.EnableSnapping)
                {
                    HandleSnapping(sceneView);
                }

                // All visual drawing must happen only during Repaint
                if (Event.current.type == EventType.Repaint)
                {
                    DrawCustomGuides(sceneView);

                    if (SmartGuidesSettingsWindow.ShowRulers)
                    {
                        DrawRulers(sceneView);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[SmartRulers] Drawing error (non-fatal): " + ex.Message);
            }
        }

        private static void HandleMouseEvents(SceneView sceneView)
        {
            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // Check if clicking on Rulers to create new guides
                if (e.mousePosition.y <= RULER_SIZE && e.mousePosition.x > RULER_SIZE)
                {
                    dragState = DragState.DraggingNewH;
                    currentDragWorldPos = GetWorldPos(e.mousePosition).y;
                    GUIUtility.hotControl = controlID;
                    e.Use();
                }
                else if (e.mousePosition.x <= RULER_SIZE && e.mousePosition.y > RULER_SIZE)
                {
                    dragState = DragState.DraggingNewV;
                    currentDragWorldPos = GetWorldPos(e.mousePosition).x;
                    GUIUtility.hotControl = controlID;
                    e.Use();
                }
                else
                {
                    // Check if clicking on existing guides
                    Vector2 mouseWorld = GetWorldPos(e.mousePosition);
                    float worldSnapThreshold = HandleUtility.GetHandleSize(mouseWorld) * 0.125f; // approx 10 pixels
                    
                    var data = SmartRulersSceneData.Instance;
                    if (data == null) return;

                    var hGuides = data.horizontalGuides;
                    for (int i = 0; i < hGuides.Count; i++)
                    {
                        if (Mathf.Abs(hGuides[i] - mouseWorld.y) < worldSnapThreshold)
                        {
                            dragState = DragState.DraggingExistingH;
                            draggedIndex = i;
                            currentDragWorldPos = hGuides[i];
                            GUIUtility.hotControl = controlID;
                            e.Use();
                            break;
                        }
                    }

                    if (dragState == DragState.None)
                    {
                        var vGuides = data.verticalGuides;
                        for (int i = 0; i < vGuides.Count; i++)
                        {
                            if (Mathf.Abs(vGuides[i] - mouseWorld.x) < worldSnapThreshold)
                            {
                                dragState = DragState.DraggingExistingV;
                                draggedIndex = i;
                                currentDragWorldPos = vGuides[i];
                                GUIUtility.hotControl = controlID;
                                e.Use();
                                break;
                            }
                        }
                    }
                }
            }
            // Right-click on a guide → context menu to delete it
            else if (e.type == EventType.MouseDown && e.button == 1)
            {
                Vector2 mouseWorld = GetWorldPos(e.mousePosition);
                float worldSnapThreshold = HandleUtility.GetHandleSize(mouseWorld) * 0.125f;

                var data = SmartRulersSceneData.Instance;
                if (data == null) return;

                // Check horizontal guides
                for (int i = 0; i < data.horizontalGuides.Count; i++)
                {
                    if (Mathf.Abs(data.horizontalGuides[i] - mouseWorld.y) < worldSnapThreshold)
                    {
                        int indexToRemove = i;
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Delete This Guide"), false, () =>
                        {
                            var d = SmartRulersSceneData.Instance;
                            if (d != null && indexToRemove < d.horizontalGuides.Count)
                            {
                                d.horizontalGuides.RemoveAt(indexToRemove);
                                SmartRulersSceneData.Save();
                                SceneView.RepaintAll();
                            }
                        });
                        menu.AddItem(new GUIContent("Delete All Guides"), false, () => ClearAllGuides());
                        menu.ShowAsContext();
                        e.Use();
                        return;
                    }
                }

                // Check vertical guides
                for (int i = 0; i < data.verticalGuides.Count; i++)
                {
                    if (Mathf.Abs(data.verticalGuides[i] - mouseWorld.x) < worldSnapThreshold)
                    {
                        int indexToRemove = i;
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Delete This Guide"), false, () =>
                        {
                            var d = SmartRulersSceneData.Instance;
                            if (d != null && indexToRemove < d.verticalGuides.Count)
                            {
                                d.verticalGuides.RemoveAt(indexToRemove);
                                SmartRulersSceneData.Save();
                                SceneView.RepaintAll();
                            }
                        });
                        menu.AddItem(new GUIContent("Delete All Guides"), false, () => ClearAllGuides());
                        menu.ShowAsContext();
                        e.Use();
                        return;
                    }
                }
            }
            else if (e.type == EventType.MouseDrag && GUIUtility.hotControl == controlID)
            {
                if (dragState == DragState.DraggingNewH || dragState == DragState.DraggingExistingH)
                {
                    currentDragWorldPos = GetWorldPos(e.mousePosition).y;
                    e.Use();
                }
                else if (dragState == DragState.DraggingNewV || dragState == DragState.DraggingExistingV)
                {
                    currentDragWorldPos = GetWorldPos(e.mousePosition).x;
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseUp && GUIUtility.hotControl == controlID)
            {
                var data = SmartRulersSceneData.Instance;
                if (data == null) return;

                if (dragState == DragState.DraggingNewH)
                {
                    if (e.mousePosition.y > RULER_SIZE)
                        data.horizontalGuides.Add(currentDragWorldPos);
                }
                else if (dragState == DragState.DraggingNewV)
                {
                    if (e.mousePosition.x > RULER_SIZE)
                        data.verticalGuides.Add(currentDragWorldPos);
                }
                else if (dragState == DragState.DraggingExistingH)
                {
                    if (e.mousePosition.y <= RULER_SIZE) // Dragged back to ruler = delete
                        data.horizontalGuides.RemoveAt(draggedIndex);
                    else
                        data.horizontalGuides[draggedIndex] = currentDragWorldPos;
                }
                else if (dragState == DragState.DraggingExistingV)
                {
                    if (e.mousePosition.x <= RULER_SIZE) // Dragged back to ruler = delete
                        data.verticalGuides.RemoveAt(draggedIndex);
                    else
                        data.verticalGuides[draggedIndex] = currentDragWorldPos;
                }

                SmartRulersSceneData.Save();
                dragState = DragState.None;
                draggedIndex = -1;
                GUIUtility.hotControl = 0;
                e.Use();
            }
        }

        private static Vector3 GetWorldPos(Vector2 mousePosition)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
            if (Mathf.Abs(ray.direction.z) < 0.0001f)
            {
                return ray.origin;
            }
            float t = -ray.origin.z / ray.direction.z;
            return ray.origin + ray.direction * t;
        }

        private static void DrawRulers(SceneView sceneView)
        {
            if (Event.current.type != EventType.Repaint) return;

            Handles.BeginGUI();
            try
            {
                Color rulerBg = new Color(0.2f, 0.2f, 0.2f, 0.9f);

                // Top Ruler
                EditorGUI.DrawRect(new Rect(0, 0, sceneView.position.width, RULER_SIZE), rulerBg);
                // Left Ruler
                EditorGUI.DrawRect(new Rect(0, 0, RULER_SIZE, sceneView.position.height), rulerBg);

                // Draw intersection square
                EditorGUI.DrawRect(new Rect(0, 0, RULER_SIZE, RULER_SIZE), new Color(0.15f, 0.15f, 0.15f, 1f));

                if (sceneView.camera != null)
                {
                    float camWidth = sceneView.camera.orthographicSize * 2f * sceneView.camera.aspect;
                    if (camWidth > 0.01f)
                    {
                        float step = Mathf.Pow(10, Mathf.FloorToInt(Mathf.Log10(camWidth / 2f)));
                        if (step > 0 && camWidth / step < 5) step /= 2f;
                    }
                }
            }
            finally
            {
                Handles.EndGUI();
            }
        }

        private static void DrawCustomGuides(SceneView sceneView)
        {
            if (Event.current.type != EventType.Repaint) return;

            var data = SmartRulersSceneData.Instance;
            if (data == null) return;
            
            if (data.horizontalGuides == null) data.horizontalGuides = new List<float>();
            if (data.verticalGuides == null) data.verticalGuides = new List<float>();

            Handles.color = SmartGuidesSettingsWindow.CustomGuideColor;

            var hGuides = data.horizontalGuides;
            var vGuides = data.verticalGuides;

            float lineThickness = SmartGuidesSettingsWindow.LineThickness;

            // Draw saved guides
            foreach (float y in hGuides)
            {
                Vector3 p1 = new Vector3(-100000, y, 0);
                Vector3 p2 = new Vector3(100000, y, 0);
                SmartGuidesEditorCompat.DrawLine(p1, p2, lineThickness);
            }

            foreach (float x in vGuides)
            {
                Vector3 p1 = new Vector3(x, -100000, 0);
                Vector3 p2 = new Vector3(x, 100000, 0);
                SmartGuidesEditorCompat.DrawLine(p1, p2, lineThickness);
            }

            // Draw currently dragged guide
            if (dragState == DragState.DraggingNewH || dragState == DragState.DraggingExistingH)
            {
                Vector3 p1 = new Vector3(-100000, currentDragWorldPos, 0);
                Vector3 p2 = new Vector3(100000, currentDragWorldPos, 0);
                SmartGuidesEditorCompat.DrawLine(p1, p2, lineThickness);
            }
            if (dragState == DragState.DraggingNewV || dragState == DragState.DraggingExistingV)
            {
                Vector3 p1 = new Vector3(currentDragWorldPos, -100000, 0);
                Vector3 p2 = new Vector3(currentDragWorldPos, 100000, 0);
                SmartGuidesEditorCompat.DrawLine(p1, p2, lineThickness);
            }
        }

        private static Vector3 lastSelectedPos;

        private static void HandleSnapping(SceneView sceneView)
        {
            GameObject selectedGo = Selection.activeGameObject;
            if (selectedGo == null || Selection.gameObjects.Length > 1) return;

            RectTransform rect = selectedGo.GetComponent<RectTransform>();
            if (rect == null) return;

            if (GUIUtility.hotControl != 0)
            {
                if (rect.position != lastSelectedPos)
                {
                    Vector3 pos = rect.position;
                    float worldSnapThreshold = HandleUtility.GetHandleSize(pos) * 0.125f; // approx 10 pixels
                    
                    float shiftX = 0f;
                    bool snappedX = false;
                    float bestDistX = float.MaxValue;

                    Vector3 bl = rect.TransformPoint(rect.rect.min);
                    Vector3 tr = rect.TransformPoint(rect.rect.max);

                    var data = SmartRulersSceneData.Instance;
                    if (data == null) return;

                    foreach (float guideX in data.verticalGuides)
                    {
                        float[] pointsToCheckX = { pos.x, bl.x, tr.x };
                        foreach (float px in pointsToCheckX)
                        {
                            float distWorld = Mathf.Abs(px - guideX);
                            if (distWorld < worldSnapThreshold && distWorld < bestDistX)
                            {
                                bestDistX = distWorld;
                                shiftX = guideX - px;
                                snappedX = true;
                            }
                        }
                    }

                    float shiftY = 0f;
                    bool snappedY = false;
                    float bestDistY = float.MaxValue;

                    foreach (float guideY in data.horizontalGuides)
                    {
                        float[] pointsToCheckY = { pos.y, bl.y, tr.y };
                        foreach (float py in pointsToCheckY)
                        {
                            float distWorld = Mathf.Abs(py - guideY);
                            if (distWorld < worldSnapThreshold && distWorld < bestDistY)
                            {
                                bestDistY = distWorld;
                                shiftY = guideY - py;
                                snappedY = true;
                            }
                        }
                    }

                    if (snappedX || snappedY)
                    {
                        if (snappedX) pos.x += shiftX;
                        if (snappedY) pos.y += shiftY;
                        rect.position = pos;
                    }

                    lastSelectedPos = rect.position;
                }
            }
            else
            {
                lastSelectedPos = rect.position;
            }
        }
    }
}
