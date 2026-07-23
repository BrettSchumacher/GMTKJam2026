using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateboardController : MonoBehaviour
{

    Rigidbody rigidbody;
    public float forwardForce = 20f;
    private Vector3 dir;

    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        //rigidbody.AddForce(forwardForce);
    }
}
