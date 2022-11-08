using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collisionAvoidance : MonoBehaviour
{
    //void Update()
    //{
    //    GameObject[] collisionObjects = GameObject.FindGameObjectsWithTag("avoidCollision");
    //    Vector3 position = transform.position;
    //    foreach(var colObject in collisionObjects)
    //    {
    //        Vector3 diff = colObject.transform.position - position;
    //        float sqrDistance = diff.sqrMagnitude;
    //        if(sqrDistance < 0.2f)
    //        {
    //            diff.y += 0.2f;
    //            GetComponent<Rigidbody>().AddForce(-diff.normalized * 1);
    //        }
    //    }
    //
    //}

    //void OnCollisionEnter(Collision c)
    //{
    //    if (c.gameObject.tag != "avoidCollision")
    //        return;
    //    float force = 2;
    //    foreach (var point in c.contacts)
    //    {
    //        Vector3 dir = point.point - transform.position;
    //        if (dir.y > 0)
    //            dir = new Vector3(0f, 1.0f, 1.0f);
    //        else
    //            dir = new Vector3(0f, 0.0f, -1.0f);
    //        GetComponent<Rigidbody>().AddForce(dir * force);
    //    }
    //}
}
