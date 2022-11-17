using System;
using UnityEngine;

/*
 * © jaredRenCode & Peter Leth https://github.com/renaissanceCoder/Simple-Scripting-Series/blob/master/Orbit.cs
 */
[RequireComponent(typeof(Animator))]
public class Orbit : MonoBehaviour {

    public float xSpread;
    public float zSpread;
    public float yOffset;
    public Transform centerPoint;
    public float startTimeOffset;
    public float rotSpeed;
    public bool rotateClockwise;
    public int animatorStartIndex;

    private float timer = 0;
    private Animator animator;
    
    private void Start()
    {
        timer = startTimeOffset;
        
        // Start animation at specific index if provided
        animator = GetComponent<Animator>();
        var animatorState = animator.GetCurrentAnimatorStateInfo(0);
        var normalizedIndex = animatorStartIndex / animator.runtimeAnimatorController.animationClips[0].frameRate;
        animator.Play(animatorState.fullPathHash, -1, normalizedIndex);
    }

    private void Update () {
        timer += Time.deltaTime * rotSpeed;
        Rotate();
    }

    private void Rotate()
    {
        var x = (rotateClockwise ? -Mathf.Cos(timer) : Mathf.Cos(timer)) * xSpread;
        var z = Mathf.Sin(timer) * zSpread;
        var pos = new Vector3(x, yOffset, z);
        var centerPointPosition = centerPoint.position;
        transform.position = pos + new Vector3(centerPointPosition.x, 0, centerPointPosition.z);
        transform.LookAt(centerPoint);
    }
}