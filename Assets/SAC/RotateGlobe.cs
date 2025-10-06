using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateGlobe : MonoBehaviour
{

    public float angle;

    // Update is called once per frame
    void Update()
    {
        this.transform.RotateAround(this.transform.position, Vector3.down, angle * Time.deltaTime);
    }
}
