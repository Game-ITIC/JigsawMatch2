using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FigmaUIGuides.Editor
{
    public static class SmartGuidesRenderer
    {
        private static GUIStyle labelStyle;

        // Deferred label queue — collected during Handles drawing, flushed in a single BeginGUI/EndGUI block
        private struct DeferredLabel
        {
            public Vector2 screenPos;
            public string text;
            public Vector2 size;
        }
        private static readonly List<DeferredLabel> pendingLabels = new List<DeferredLabel>();

        private static void InitStyles()
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = SmartGuidesSettingsWindow.FontSize,
                    padding = new RectOffset(4, 4, 2, 2)
                };
            }
            else
            {
                labelStyle.fontSize = SmartGuidesSettingsWindow.FontSize;
            }
        }

        /// <summary>
        /// Queue a label to be drawn later in a single BeginGUI/EndGUI pass.
        /// Call FlushLabels() once after all Handles drawing is done.
        /// </summary>
        private static void QueueLabel(Vector3 worldPosition, string text)
        {
            InitStyles();
            GUIContent content = new GUIContent(text);
            Vector2 size = labelStyle.CalcSize(content);
            Vector2 screenPos = HandleUtility.WorldToGUIPoint(worldPosition);
            
            pendingLabels.Add(new DeferredLabel
            {
                screenPos = screenPos,
                text = text,
                size = size
            });
        }

        /// <summary>
        /// Draw all queued labels in a single BeginGUI/EndGUI block.
        /// This avoids multiple BeginGUI/EndGUI pairs that can corrupt the GUI stack.
        /// </summary>
        public static void FlushLabels()
        {
            if (pendingLabels.Count == 0) return;

            InitStyles();

            Handles.BeginGUI();
            try
            {
                Color bgColor = SmartGuidesSettingsWindow.GuideColor;
                bgColor.a = 1f;

                GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.normal.background = Texture2D.whiteTexture;

                foreach (var label in pendingLabels)
                {
                    Rect bgRect = new Rect(
                        label.screenPos.x - label.size.x / 2f,
                        label.screenPos.y - label.size.y / 2f,
                        label.size.x,
                        label.size.y
                    );

                    GUI.backgroundColor = bgColor;
                    GUI.Box(bgRect, GUIContent.none, boxStyle);
                    GUI.backgroundColor = Color.white;

                    GUI.Label(bgRect, label.text, labelStyle);
                }
            }
            finally
            {
                Handles.EndGUI();
            }
            
            pendingLabels.Clear();
        }

        private static void DrawMeasurementLine(Vector3 worldStart, Vector3 worldEnd, float value)
        {
            if (Mathf.Abs(value) < 0.1f) return;

            Handles.color = SmartGuidesSettingsWindow.GuideColor;
            
            // Draw main line
            SmartGuidesEditorCompat.DrawLine(worldStart, worldEnd, SmartGuidesSettingsWindow.LineThickness);

            // Draw end ticks (perpendicular to the line)
            Vector3 dir = (worldEnd - worldStart).normalized;
            
            // Determine normal direction based on camera view or simply 2D Z-axis
            Vector3 normal = Vector3.Cross(dir, Vector3.forward).normalized;
            if (normal.sqrMagnitude < 0.1f) normal = Vector3.Cross(dir, Vector3.up).normalized; // fallback if aligned with Z

            float tickSize = 4f * HandleUtility.GetHandleSize(worldStart) * 0.05f;
            SmartGuidesEditorCompat.DrawLine(worldStart - normal * tickSize, worldStart + normal * tickSize, SmartGuidesSettingsWindow.LineThickness);
            SmartGuidesEditorCompat.DrawLine(worldEnd - normal * tickSize, worldEnd + normal * tickSize, SmartGuidesSettingsWindow.LineThickness);

            Vector3 midPoint = (worldStart + worldEnd) / 2f;
            QueueLabel(midPoint, Mathf.RoundToInt(Mathf.Abs(value)).ToString());
        }

        private static Rect GetCanvasRect(RectTransform rectTransform, Transform canvasTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            foreach (var corner in corners)
            {
                Vector3 localP = canvasTransform.InverseTransformPoint(corner);
                min.x = Mathf.Min(min.x, localP.x);
                min.y = Mathf.Min(min.y, localP.y);
                max.x = Mathf.Max(max.x, localP.x);
                max.y = Mathf.Max(max.y, localP.y);
            }

            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        public static void DrawMeasurements(RectTransform source, RectTransform target)
        {
            if (source == null || target == null) return;

            Canvas canvas = source.GetComponentInParent<Canvas>();
            if (canvas == null) return;

            Transform cTransform = canvas.transform;
            Rect rectA = GetCanvasRect(source, cTransform);
            Rect rectB = GetCanvasRect(target, cTransform);

            bool overlapX = rectA.xMax >= rectB.xMin && rectA.xMin <= rectB.xMax;
            bool overlapY = rectA.yMax >= rectB.yMin && rectA.yMin <= rectB.yMax;
            
            bool insideX = rectA.xMin >= rectB.xMin && rectA.xMax <= rectB.xMax;
            bool insideY = rectA.yMin >= rectB.yMin && rectA.yMax <= rectB.yMax;

            // X-Axis Measurements
            if (overlapX || insideX)
            {
                // Measure Top & Bottom distances if they overlap on X
                float overlapXCenter = (Mathf.Max(rectA.xMin, rectB.xMin) + Mathf.Min(rectA.xMax, rectB.xMax)) / 2f;
                
                // Top distance
                if (rectA.yMax <= rectB.yMax)
                {
                    Vector3 start = cTransform.TransformPoint(new Vector3(overlapXCenter, rectA.yMax, 0));
                    Vector3 end = cTransform.TransformPoint(new Vector3(overlapXCenter, rectB.yMax, 0));
                    DrawMeasurementLine(start, end, rectB.yMax - rectA.yMax);
                }
                else // A is above B
                {
                    Vector3 start = cTransform.TransformPoint(new Vector3(overlapXCenter, rectA.yMin, 0));
                    Vector3 end = cTransform.TransformPoint(new Vector3(overlapXCenter, rectB.yMax, 0));
                    DrawMeasurementLine(start, end, rectA.yMin - rectB.yMax);
                }

                // Bottom distance
                if (rectA.yMin >= rectB.yMin)
                {
                    Vector3 start = cTransform.TransformPoint(new Vector3(overlapXCenter, rectA.yMin, 0));
                    Vector3 end = cTransform.TransformPoint(new Vector3(overlapXCenter, rectB.yMin, 0));
                    DrawMeasurementLine(start, end, rectA.yMin - rectB.yMin);
                }
                else // A is below B
                {
                    Vector3 start = cTransform.TransformPoint(new Vector3(overlapXCenter, rectA.yMax, 0));
                    Vector3 end = cTransform.TransformPoint(new Vector3(overlapXCenter, rectB.yMin, 0));
                    DrawMeasurementLine(start, end, rectB.yMin - rectA.yMax);
                }
            }
            else
            {
                // Source is completely to the left or right of Target
                float yPos = rectA.center.y; // Draw line at A's center Y

                if (rectA.xMax < rectB.xMin) // Left
                {
                    Vector3 start = cTransform.TransformPoint(new Vector3(rectA.xMax, yPos, 0));
                    Vector3 end = cTransform.TransformPoint(new Vector3(rectB.xMin, yPos, 0));
                    DrawMeasurementLine(start, end, rectB.xMin - rectA.xMax);
                    DrawExtensionLineY(cTransform, rectB.xMin, rectB.center.y, yPos);
                }
                else if (rectA.xMin > rectB.xMax) // Right
                {
                    Vector3 start = cTransform.TransformPoint(new Vector3(rectA.xMin, yPos, 0));
                    Vector3 end = cTransform.TransformPoint(new Vector3(rectB.xMax, yPos, 0));
                    DrawMeasurementLine(start, end, rectA.xMin - rectB.xMax);
                    DrawExtensionLineY(cTransform, rectB.xMax, rectB.center.y, yPos);
                }
            }

            // Y-Axis Measurements
            if (overlapY || insideY)
            {
                // Measure Left & Right distances if they overlap on Y
                float overlapYCenter = (Mathf.Max(rectA.yMin, rectB.yMin) + Mathf.Min(rectA.yMax, rectB.yMax)) / 2f;
                
                // Left distance
                if (rectA.xMin >= rectB.xMin)
                {
                    Vector3 start = cTransform.TransformPoint(new Vector3(rectA.xMin, overlapYCenter, 0));
                    Vector3 end = cTransform.TransformPoint(new Vector3(rectB.xMin, overlapYCenter, 0));
                    DrawMeasurementLine(start, end, rectA.xMin - rectB.xMin);
                }
                else // A is to the left of B
                {
                    Vector3 start = cTransform.TransformPoint(new Vector3(rectA.xMax, overlapYCenter, 0));
                    Vector3 end = cTransform.TransformPoint(new Vector3(rectB.xMin, overlapYCenter, 0));
                    DrawMeasurementLine(start, end, rectB.xMin - rectA.xMax);
                }

                // Right distance
                if (rectA.xMax <= rectB.xMax)
                {
                    Vector3 start = cTransform.TransformPoint(new Vector3(rectA.xMax, overlapYCenter, 0));
                    Vector3 end = cTransform.TransformPoint(new Vector3(rectB.xMax, overlapYCenter, 0));
                    DrawMeasurementLine(start, end, rectB.xMax - rectA.xMax);
                }
                else // A is to the right of B
                {
                    Vector3 start = cTransform.TransformPoint(new Vector3(rectA.xMin, overlapYCenter, 0));
                    Vector3 end = cTransform.TransformPoint(new Vector3(rectB.xMax, overlapYCenter, 0));
                    DrawMeasurementLine(start, end, rectA.xMin - rectB.xMax);
                }
            }
            else
            {
                // Source is completely above or below Target
                float xPos = rectA.center.x; // Draw line at A's center X

                if (rectA.yMax < rectB.yMin) // Below
                {
                    Vector3 start = cTransform.TransformPoint(new Vector3(xPos, rectA.yMax, 0));
                    Vector3 end = cTransform.TransformPoint(new Vector3(xPos, rectB.yMin, 0));
                    DrawMeasurementLine(start, end, rectB.yMin - rectA.yMax);
                    DrawExtensionLineX(cTransform, rectB.yMin, rectB.center.x, xPos);
                }
                else if (rectA.yMin > rectB.yMax) // Above
                {
                    Vector3 start = cTransform.TransformPoint(new Vector3(xPos, rectA.yMin, 0));
                    Vector3 end = cTransform.TransformPoint(new Vector3(xPos, rectB.yMax, 0));
                    DrawMeasurementLine(start, end, rectA.yMin - rectB.yMax);
                    DrawExtensionLineX(cTransform, rectB.yMax, rectB.center.x, xPos);
                }
            }

            // Also draw its own sizes inside the object if requested
            if (SmartGuidesSettingsWindow.ShowSizes)
            {
                Vector3 centerWorld = cTransform.TransformPoint(new Vector3(rectA.center.x, rectA.center.y, 0));
                
                // Draw inner W and H
                QueueLabel(centerWorld, $"{Mathf.RoundToInt(rectA.width)} x {Mathf.RoundToInt(rectA.height)}");
            }

            // Flush all labels at the end in ONE BeginGUI/EndGUI pair
            FlushLabels();
        }

        private static void DrawExtensionLineX(Transform cTransform, float yPos, float xStart, float xEnd)
        {
            Handles.color = new Color(SmartGuidesSettingsWindow.GuideColor.r, SmartGuidesSettingsWindow.GuideColor.g, SmartGuidesSettingsWindow.GuideColor.b, 0.5f);
            Vector3 start = cTransform.TransformPoint(new Vector3(xStart, yPos, 0));
            Vector3 end = cTransform.TransformPoint(new Vector3(xEnd, yPos, 0));
            Handles.DrawDottedLine(start, end, 4f);
        }

        private static void DrawExtensionLineY(Transform cTransform, float xPos, float yStart, float yEnd)
        {
            Handles.color = new Color(SmartGuidesSettingsWindow.GuideColor.r, SmartGuidesSettingsWindow.GuideColor.g, SmartGuidesSettingsWindow.GuideColor.b, 0.5f);
            Vector3 start = cTransform.TransformPoint(new Vector3(xPos, yStart, 0));
            Vector3 end = cTransform.TransformPoint(new Vector3(xPos, yEnd, 0));
            Handles.DrawDottedLine(start, end, 4f);
        }

        public static void DrawSizesOnly(RectTransform source)
        {
            Canvas canvas = source.GetComponentInParent<Canvas>();
            if (canvas == null) return;
            Rect rectA = GetCanvasRect(source, canvas.transform);
            Vector3 centerWorld = canvas.transform.TransformPoint(new Vector3(rectA.center.x, rectA.center.y, 0));
            QueueLabel(centerWorld, $"{Mathf.RoundToInt(rectA.width)} x {Mathf.RoundToInt(rectA.height)}");
            FlushLabels();
        }

        public static void DrawNearestDistances(RectTransform source, Canvas canvas)
        {
            Transform cTransform = canvas.transform;
            Rect rectA = GetCanvasRect(source, cTransform);
            Rect canvasRect = GetCanvasRect(canvas.GetComponent<RectTransform>(), cTransform);

            float leftBound = canvasRect.xMin;
            float rightBound = canvasRect.xMax;
            float bottomBound = canvasRect.yMin;
            float topBound = canvasRect.yMax;

            RectTransform leftObj = null, rightObj = null, bottomObj = null, topObj = null;
            Rect leftRect = default(Rect);
            Rect rightRect = default(Rect);
            Rect bottomRect = default(Rect);
            Rect topRect = default(Rect);

            float minDistLeft = float.MaxValue;
            float minDistRight = float.MaxValue;
            float minDistBottom = float.MaxValue;
            float minDistTop = float.MaxValue;

            RectTransform[] allRects = canvas.GetComponentsInChildren<RectTransform>(false);
            
            foreach (var b in allRects)
            {
                if (b == source || b == canvas.GetComponent<RectTransform>()) continue;
                if (b.IsChildOf(source) || source.IsChildOf(b)) continue;

                Rect rectB = GetCanvasRect(b, cTransform);

                // Use a small epsilon for overlaps to catch perfectly aligned edges
                bool overlapsX = rectA.xMax > rectB.xMin + 0.1f && rectA.xMin < rectB.xMax - 0.1f;
                bool overlapsY = rectA.yMax > rectB.yMin + 0.1f && rectA.yMin < rectB.yMax - 0.1f;

                if (overlapsY)
                {
                    if (rectB.xMax <= rectA.xMin + 0.1f) // Left
                    {
                        float dist = rectA.xMin - rectB.xMax;
                        if (dist < minDistLeft) { minDistLeft = dist; leftBound = rectB.xMax; leftObj = b; leftRect = rectB; }
                    }
                    if (rectB.xMin >= rectA.xMax - 0.1f) // Right
                    {
                        float dist = rectB.xMin - rectA.xMax;
                        if (dist < minDistRight) { minDistRight = dist; rightBound = rectB.xMin; rightObj = b; rightRect = rectB; }
                    }
                }

                if (overlapsX)
                {
                    if (rectB.yMax <= rectA.yMin + 0.1f) // Bottom
                    {
                        float dist = rectA.yMin - rectB.yMax;
                        if (dist < minDistBottom) { minDistBottom = dist; bottomBound = rectB.yMax; bottomObj = b; bottomRect = rectB; }
                    }
                    if (rectB.yMin >= rectA.yMax - 0.1f) // Top
                    {
                        float dist = rectB.yMin - rectA.yMax;
                        if (dist < minDistTop) { minDistTop = dist; topBound = rectB.yMin; topObj = b; topRect = rectB; }
                    }
                }
            }

            float xCenter = rectA.center.x;
            float yCenter = rectA.center.y;

            if (rectA.xMin > canvasRect.xMin - 0.1f)
            {
                float yPos = yCenter;
                if (leftObj != null) yPos = (Mathf.Max(rectA.yMin, leftRect.yMin) + Mathf.Min(rectA.yMax, leftRect.yMax)) / 2f;
                Vector3 start = cTransform.TransformPoint(new Vector3(rectA.xMin, yPos, 0));
                Vector3 end = cTransform.TransformPoint(new Vector3(leftBound, yPos, 0));
                DrawMeasurementLine(start, end, rectA.xMin - leftBound);
            }

            if (rectA.xMax < canvasRect.xMax + 0.1f)
            {
                float yPos = yCenter;
                if (rightObj != null) yPos = (Mathf.Max(rectA.yMin, rightRect.yMin) + Mathf.Min(rectA.yMax, rightRect.yMax)) / 2f;
                Vector3 start = cTransform.TransformPoint(new Vector3(rectA.xMax, yPos, 0));
                Vector3 end = cTransform.TransformPoint(new Vector3(rightBound, yPos, 0));
                DrawMeasurementLine(start, end, rightBound - rectA.xMax);
            }

            if (rectA.yMin > canvasRect.yMin - 0.1f)
            {
                float xPos = xCenter;
                if (bottomObj != null) xPos = (Mathf.Max(rectA.xMin, bottomRect.xMin) + Mathf.Min(rectA.xMax, bottomRect.xMax)) / 2f;
                Vector3 start = cTransform.TransformPoint(new Vector3(xPos, rectA.yMin, 0));
                Vector3 end = cTransform.TransformPoint(new Vector3(xPos, bottomBound, 0));
                DrawMeasurementLine(start, end, rectA.yMin - bottomBound);
            }

            if (rectA.yMax < canvasRect.yMax + 0.1f)
            {
                float xPos = xCenter;
                if (topObj != null) xPos = (Mathf.Max(rectA.xMin, topRect.xMin) + Mathf.Min(rectA.xMax, topRect.xMax)) / 2f;
                Vector3 start = cTransform.TransformPoint(new Vector3(xPos, rectA.yMax, 0));
                Vector3 end = cTransform.TransformPoint(new Vector3(xPos, topBound, 0));
                DrawMeasurementLine(start, end, topBound - rectA.yMax);
            }

            // Flush all labels at the end in ONE BeginGUI/EndGUI pair
            FlushLabels();
        }
    }
}
