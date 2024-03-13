using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpArc : MonoBehaviour
{
    [SerializeField]private float InitVelocity;
    private float JumpVelocity;
    void Start()
    {
        JumpVelocity = InitVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
