using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDrag : MonoBehaviour
{
    Vector3 offset;

    public void BeginDrag()
    {
        offset = transform.position - Input.mousePosition;
    }

    public void OnDrag()
    {
        transform.position = offset + Input.mousePosition;
    }
}
