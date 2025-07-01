using System;
using System.Collections.Generic;
using Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    public class BuildingsAnimationConfig : MonoBehaviour
    {
        public Animator animator;
        public AnimationClip animationClip;
        public List<BuildingsAnimationData> data;
    }
    
    [Serializable]
    public class BuildingsAnimationData
    {
        public int startFrame;
        public int endFrame;
    }
}