using UnityEditor;
using UnityEngine;

namespace FigmaUIGuides.Editor
{
    [InitializeOnLoad]
    public static class SmartGuidesCore
    {
        private static GameObject lastPickedObject = null;

        static SmartGuidesCore()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!SmartGuidesSettingsWindow.IsEnabled)
                return;

            Event e = Event.current;

            // Only pick objects during input events — NEVER during Repaint or Layout!
            // PickGameObject starts an internal render pass, calling it during Repaint
            // corrupts Unity's render state and causes "device.IsInsideFrame()" errors.
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag)
            {
                lastPickedObject = HandleUtility.PickGameObject(e.mousePosition, false);
                sceneView.Repaint();
            }

            if (e.type == EventType.Repaint)
            {
                try
                {
                    DrawGuides();
                }
                catch (System.Exception ex)
                {
                    // Swallow drawing exceptions to prevent cascading GUI errors
                    Debug.LogWarning("[SmartGuides] Drawing error (non-fatal): " + ex.Message);
                }
            }
        }

        private static void DrawGuides()
        {
            GameObject selectedGo = Selection.activeGameObject;
            if (selectedGo == null || Selection.gameObjects.Length > 1) return;

            RectTransform selectedRect = selectedGo.GetComponent<RectTransform>();
            if (selectedRect == null) return;

            // 1. Внутреннее: всегда активно
            if (SmartGuidesSettingsWindow.ShowSizes)
            {
                SmartGuidesRenderer.DrawSizesOnly(selectedRect);
            }

            // 2. Оно не активно, когда курсор внутри выделенного объекта
            if (lastPickedObject == selectedGo)
            {
                return;
            }

            Canvas canvas = selectedRect.GetComponentInParent<Canvas>();
            if (canvas == null) return;

            // 3. Наружнее активно!
            // Если мы навели на конкретный другой объект (не Canvas) — показываем расстояние до него
            if (lastPickedObject != null && lastPickedObject != canvas.gameObject)
            {
                RectTransform targetRect = lastPickedObject.GetComponent<RectTransform>();
                if (targetRect != null && SmartGuidesSettingsWindow.ShowDistancesToObjects)
                {
                    SmartGuidesRenderer.DrawMeasurements(selectedRect, targetRect);
                    return;
                }
            }

            // Если мышка на пустом фоне — показываем ближайшие объекты во все 4 стороны (фулл фотошоп)
            if (SmartGuidesSettingsWindow.ShowDistancesToCanvas)
            {
                SmartGuidesRenderer.DrawNearestDistances(selectedRect, canvas);
            }
        }
    }
}
