using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour {
    public float StayTime = -0.4f;
    public float SpreadTime = 0.3f;

    private Material mat;
    private float interval;
    void Awake()
    {
        mat = GetComponent<MeshRenderer>().material;
        interval = SpreadTime;
        mat.SetFloat("_AlphaScale", 1);
    }

    void Update()
    {
        if (interval > 0)
        {
            mat.SetFloat("_AlphaScale", interval / SpreadTime);
            interval -= Time.deltaTime;
        }
        else if (interval > 0.2f)
        {
            mat.SetFloat("_AlphaScale", 0);
            interval -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
