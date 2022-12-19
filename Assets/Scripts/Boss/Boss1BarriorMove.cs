using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1BarriorMove : MonoBehaviour
{
    public GameObject boss;
    public float radius = 2.5f;
    public Vector2 _fixedPosition;

    private Vector2 _position;
    private float _time = 0.0f;
    private float _lifeTime = 7.0f;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 offset;
        Vector2 center = _fixedPosition;
        offset.x = (radius + (0.5f * Mathf.Sin(0.5f * Mathf.PI * _time) + 0.5f)) * Mathf.Cos(Mathf.PI * _time);
        offset.y = (radius + (0.5f * Mathf.Sin(0.5f * Mathf.PI * _time) + 0.5f)) * Mathf.Sin(Mathf.PI * _time);
        _position = center + offset;
        transform.localPosition = _position;

        _time += Time.deltaTime;

        if (_time >= _lifeTime)
            Destroy(gameObject);
    }
}
