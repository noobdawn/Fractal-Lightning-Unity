using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMotion : MonoBehaviour {
    [SerializeField]
    public float XSpeed;
    [SerializeField]
    public float XRange;
    [SerializeField]
    public float XOffset;
    [SerializeField]
    public float YSpeed;
    [SerializeField]
    public float YRange;
    [SerializeField]
    public float YOffset;

    private Vector3 pos;

    void Awake()
    {
        pos = transform.position;
    }

    void Update()
    {
        transform.position = pos + new Vector3(
            XRange * Mathf.Sin(XSpeed * Time.time * 0.1f),
            YRange * Mathf.Sin(YSpeed * Time.time * 0.1f),
            0);
    }
}
