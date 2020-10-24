using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCamMovement : MonoBehaviour
{
    public float dragSpeed = 2.0f;
    private Vector3 dragOrigin;


    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(1)) return;

        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
        Vector3 move = new Vector3(pos.x * dragSpeed, pos.y * dragSpeed,0);

        transform.Translate(move * Time.deltaTime, Space.World);
    }
}
