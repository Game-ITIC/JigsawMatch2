#if UNITY_EDITOR && SORTIFY
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sortify
{
    public static class SortifyHierarchyHelper
    {
        private static readonly Type _windowType;
        private static readonly MethodInfo _getAllWindowsMethod;
        private static readonly PropertyInfo _sceneHierarchyProp;
        private static readonly MethodInfo _getExpGOMethod;
        private static readonly MethodInfo _getExpandedIDsMethod;
        private static readonly MethodInfo _setExpandedMethod;
        private static readonly MethodInfo _setExpandedRecursiveMethod;

        static SortifyHierarchyHelper()
        {
            _windowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
            if (_windowType != null)
            {
                _getAllWindowsMethod = _windowType.GetMethod("GetAllSceneHierarchyWindows", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                _sceneHierarchyProp = _windowType.GetProperty("sceneHierarchy", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                _getExpandedIDsMethod = _windowType.GetMethod("GetExpandedIDs", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                _setExpandedMethod = _windowType.GetMethod("SetExpanded", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(int), typeof(bool) }, null);
                _setExpandedRecursiveMethod = _windowType.GetMethod("SetExpandedRecursive", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(int), typeof(bool) }, null);

                Type shType = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchy");
                if (shType != null)
                {
                    _getExpGOMethod = shType.GetMethod("GetExpandedGameObjects", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                }
            }
        }

        public static bool IsExpanded(GameObject go)
        {
            if (go == null || _getAllWindowsMethod == null)
                return false;

            var windows = _getAllWindowsMethod.Invoke(null, null) as IList;
            if (windows == null || windows.Count == 0)
                return false;

            var win = windows[0];

            if (_sceneHierarchyProp != null && _getExpGOMethod != null)
            {
                var sh = _sceneHierarchyProp.GetValue(win);
                if (sh != null)
                {
                    var expGOs = _getExpGOMethod.Invoke(sh, null) as IEnumerable;
                    if (expGOs != null)
                    {
                        foreach (var item in expGOs)
                        {
                            if (item is GameObject g && g == go)
                                return true;
                        }
                    }
                }
            }

            if (_getExpandedIDsMethod != null)
            {
                var idsObj = _getExpandedIDsMethod.Invoke(win, null) as IEnumerable;
                if (idsObj != null)
                {
                    int instanceID = go.GetInstanceID();
                    foreach (var id in idsObj)
                    {
                        if (id is int i && i == instanceID)
                            return true;
                        if (id != null && id.ToString() == instanceID.ToString())
                            return true;
                    }
                }
            }

            return false;
        }

        public static void SetExpanded(GameObject go, bool expand, bool recursive = false)
        {
            if (go == null || _getAllWindowsMethod == null)
                return;

            var windows = _getAllWindowsMethod.Invoke(null, null) as IList;
            if (windows == null)
                return;

            var method = recursive ? (_setExpandedRecursiveMethod ?? _setExpandedMethod) : _setExpandedMethod;
            if (method == null)
                return;

            int id = go.GetInstanceID();
            object[] args = new object[] { id, expand };

            foreach (var win in windows)
            {
                method.Invoke(win, args);
            }
        }

        public static void ToggleExpanded(GameObject go, bool recursive = false)
        {
            bool current = IsExpanded(go);
            SetExpanded(go, !current, recursive);
        }
    }
}
#endif
