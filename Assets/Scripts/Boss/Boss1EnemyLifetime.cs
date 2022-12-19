using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1EnemyLifetime : MonoBehaviour
{
    private float _time = 0.0f;
    private const float _lifeTime = 25.0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _time += Time.deltaTime;
        if (_time >= _lifeTime)
        {
            Destroy(gameObject);
        }
    }
}
