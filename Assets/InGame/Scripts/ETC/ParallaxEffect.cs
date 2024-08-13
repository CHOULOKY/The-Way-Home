using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    private float length, startpos;
    public float parallaxFactor;
    public GameObject cam;


    void Start() 
    {
        startpos = transform.position.x;
        length = 22.5f; // GetComponent<SpriteRenderer>().bounds.size.x - 3; 
    }

    void Update()
    {
        float temp = cam.transform.position.x * (1 - parallaxFactor);
        float distance = cam.transform.position.x * parallaxFactor;

        Vector3 newPosition = new Vector3(startpos + distance, transform.position.y, transform.position.z);
        
        transform.position = newPosition;

        if (temp > startpos + (length / 2))     startpos += length;
        else if (temp < startpos - (length / 2))     startpos -= length;
    }
}