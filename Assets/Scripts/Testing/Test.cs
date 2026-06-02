using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    
    private PlayableGraph playableGraph;
    private AnimationPlayableOutput playableOutput;
    private AnimationClipPlayable clipPlayable;

    public Animator animator;
    public AnimationClip animationClip;
    
    void Start() 
    {
        playableGraph = PlayableGraph.Create();
        playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
        
        clipPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);
        playableOutput.SetSourcePlayable(clipPlayable);
        
        playableGraph.Play();
    }
    
    [Button]
    public void JumpToFrame(int frame)
    {
        float frameRate = animationClip.frameRate;
        double targetTime = frame / frameRate;
        
        clipPlayable.SetTime(targetTime);
        clipPlayable.SetSpeed(0); // Останавливаем
        playableGraph.Evaluate(); // Принудительно обновляем
    }
    
    [Button]
    public void PlayToFrame(int targetFrame)
    {
        float frameRate = animationClip.frameRate;
        double targetTime = targetFrame / frameRate;
        
        clipPlayable.SetSpeed(1);
        StartCoroutine(StopAtFrame(targetTime));
    }

    [Button]
    private IEnumerator StopAtFrame(double targetTime)
    {
        while (clipPlayable.GetTime() < targetTime)
        {
            yield return null;
        }
        clipPlayable.SetSpeed(0);
    }
    
    void OnDestroy() 
    {
        playableGraph.Destroy();
    }
}