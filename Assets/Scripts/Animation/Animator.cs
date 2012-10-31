using UnityEngine;
using System;
using System.Collections;
using System.Reflection;

//The function below is from : http://answers.unity3d.com/questions/30942/how-can-i-wait-for-an-animation-to-complete
public static class AnimationExtensions
{
    public static IEnumerator WaitForAnim( this Animation animation )
    {
        do
        {
            yield return null;
        } while ( animation.isPlaying );
    }

    public static IEnumerator WaitForAnim( this Animation animation, string animationName )
    {
        animation.PlayQueued(animationName);
        yield return animation.WaitForAnim();
    }
}


public class AnimSummary : ICloneable {
    
    public string FirstAnim = null;
    public string SecondAnim = null;
    public string NewIdleAnim = null;
    public string NewWalkAnim = null;
    
    public AnimSummary(){
    }
    
    public AnimSummary (string _1stAnim, string _2ndAnim = null, string newIdleAnim = null, string newWalkAnim = null) {
        FirstAnim = _1stAnim;
        SecondAnim = _2ndAnim;
        NewIdleAnim = newIdleAnim;
        NewWalkAnim = newWalkAnim;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public class Animator : MonoBehaviour {
    public bool isPlayer = false;
    public string IdleAnim = "idle";
    public string WalkAnim = "walk";
    private Avatar avatar;
    private bool Normal = true;
    private OCBehaviour CurOCB;
    private Hashtable CurParameters;

    void Start() {
        avatar = gameObject.transform.parent.GetComponent<Avatar>() as Avatar;
        if(transform.parent.tag == "Player")
            isPlayer = true;
        else
            isPlayer = false;
    }
    
    void Update() {
        if(!isPlayer && Normal) {
            if(avatar.isMoving()) {
                if(animation.IsPlaying(WalkAnim) == false) {
                    Debug.LogWarning("playing walk");
                    animation.CrossFade(WalkAnim,0.1f);
                }
            } else {
                if(animation.IsPlaying(IdleAnim) == false) {
                    Debug.LogWarning("playing idle");
                    animation.CrossFade(IdleAnim,0.1f);
                }
            }
        }
		
    }
    
    public void StopNormal() {
        Normal = false;
        animation.Stop();
    }
    
    public void PlayNormal() {
        Normal = true;
    }
    
    public void SetIdleAnim(string AnimName) {
        if(animation.GetClip(AnimName))
            IdleAnim = AnimName;
        else
            Debug.Log("Error : Animation named \"AnimName\" doesn't exist!");
    }
    
    public void ResetIdleAnim(){
        SetIdleAnim("idle");
    }
    
    public void SetWalkAnim(string AnimName){
        if(animation.GetClip(AnimName))
            WalkAnim = AnimName;
        else
            Debug.Log("Error : Animation named \"AnimName\" doesn't exist!");
    }
    
    public void ResetWalkAnim(){
        SetWalkAnim("idle");
    }
    
}
