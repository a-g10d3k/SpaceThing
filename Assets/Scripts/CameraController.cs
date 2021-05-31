using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject LockedGameObject;
    private float _zoomLevel = 5;

    void Follow(GameObject target)
    {
        if(target == null) { return; }
        Vector3 newPos = target.transform.position;
        newPos.z = -10;
        transform.position = newPos;
    }

    void Zoom()
    {
        _zoomLevel -= Input.mouseScrollDelta.y;
        if (_zoomLevel < 1) { _zoomLevel = 1; }
        if(_zoomLevel > 200) { _zoomLevel = 200; }
        gameObject.GetComponent<Camera>().orthographicSize = 5 * _zoomLevel / 5;
    }

    // Update is called once per frame
    void Update()
    {
        Follow(LockedGameObject);
        Zoom();
    }
}
