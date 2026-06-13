using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FigmaUIGuides.Editor
{
    [Serializable]
    public class SmartRulersSceneData
    {
        public List<float> horizontalGuides = new List<float>();
        public List<float> verticalGuides = new List<float>();

        private static SmartRulersSceneData instance;
        private static string currentScenePath;

        public static SmartRulersSceneData Instance
        {
            get
            {
                string activePath = EditorSceneManager.GetActiveScene().path;
                
                if (instance == null || currentScenePath != activePath)
                {
                    currentScenePath = activePath;
                    Load();
                }
                
                return instance;
            }
        }

        public static void Save()
        {
            if (instance != null)
            {
                string json = JsonUtility.ToJson(instance);
                EditorPrefs.SetString("SmartGuidesData_" + currentScenePath, json);
            }
        }

        private static void Load()
        {
            string json = EditorPrefs.GetString("SmartGuidesData_" + currentScenePath, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    instance = JsonUtility.FromJson<SmartRulersSceneData>(json);
                }
                catch
                {
                    instance = new SmartRulersSceneData();
                }
            }
            else
            {
                instance = new SmartRulersSceneData();
            }

            if (instance == null) instance = new SmartRulersSceneData();
            if (instance.horizontalGuides == null) instance.horizontalGuides = new List<float>();
            if (instance.verticalGuides == null) instance.verticalGuides = new List<float>();

            instance.horizontalGuides.RemoveAll(f => float.IsNaN(f) || float.IsInfinity(f));
            instance.verticalGuides.RemoveAll(f => float.IsNaN(f) || float.IsInfinity(f));
        }
    }
}
