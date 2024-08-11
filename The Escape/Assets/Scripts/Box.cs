using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    Rigidbody rigid;
    Vector3 firstPosition;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        firstPosition = rigid.position;
    }

    private void OnCollisionStay(Collision collision)
    {
       if (collision.gameObject.tag == "Wall")
        {
            rigid.position = firstPosition;
        }
    }
}
