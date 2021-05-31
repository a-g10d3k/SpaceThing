using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleWithCamera : MonoBehaviour
{
    public Vector3 InitialScale = new Vector3(1,1,1);
    void Update()
    {
        transform.localScale = InitialScale * Camera.main.orthographicSize / 5;
    }
}
