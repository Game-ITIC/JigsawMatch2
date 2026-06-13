using UnityEditor;
using UnityEngine;

namespace FigmaUIGuides.Editor
{
    internal static class SmartGuidesEditorCompat
    {
        public static void DrawLine(Vector3 start, Vector3 end, float thickness)
        {
#if UNITY_2022_3_OR_NEWER
            Handles.DrawAAPolyLine(Mathf.Max(1f, thickness), start, end);
#else
            Handles.DrawLine(start, end);
#endif
        }
    }
}
