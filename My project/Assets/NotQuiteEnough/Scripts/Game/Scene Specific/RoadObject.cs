using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadObject : MonoBehaviour
{

    public float Speed;
    public float Distance;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(0, 0, Speed * Time.deltaTime);

        if (transform.position.z < -Distance)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, Distance);
        }

    }
}
