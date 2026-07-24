using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelsRotate : MonoBehaviour
{
    [SerializeField] private SkateboardController parentMovement;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Vector3 WheelAxis = Vector3.forward;
    private float WheelRadius;
    private BoxCollider WheelboxCollider;
    // Start is called before the first frame update
    void Awake()
    {

        WheelboxCollider = GetComponent<BoxCollider>();
        if (parentMovement == null)
        {
            parentMovement = GetComponentInParent<SkateboardController>();
        }
        if (WheelboxCollider != null) {
            WheelRadius =
             transform.TransformVector(
        Vector3.forward * WheelboxCollider.size.z ).magnitude * 0.5f; ; 
        }
    }
    


    // Update is called once per frame
    void Update()
    {   
       
            float localvelocity = Vector3.Dot(rb.velocity, parentMovement.transform.forward);
        float rotationDegrees =
           localvelocity / WheelRadius *
           Mathf.Rad2Deg *
           Time.deltaTime;
        transform.Rotate(
            WheelAxis,
            rotationDegrees,
            Space.Self
        );
    }
}
