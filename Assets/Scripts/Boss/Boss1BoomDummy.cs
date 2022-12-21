using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1BoomDummy : MonoBehaviour
{
    private Vector2 _origin;
    private float _lifeTime;
    private float _time = 0.0f;
    Enemy dummy;

    public void Init(Vector2 origin, float lifeTime, Enemy enemy)
    {
        _origin = origin;
        _lifeTime = lifeTime;
        dummy = enemy;
        GetComponent<Rigidbody2D>().AddForce(((Vector2)transform.position - _origin).normalized * 130.0f);
        GetComponent<Rigidbody2D>().angularVelocity = 150.0f;
    }
    private void Update()
    {
        if (_time >= _lifeTime)
        {
            dummy.server.serverSend.EnemyDestroy(dummy);
            Destroy(gameObject);
        }
        _time += Time.deltaTime;
    }
}
