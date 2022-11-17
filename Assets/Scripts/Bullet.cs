using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Dictionary<int, Bullet> bullet = new Dictionary<int, Bullet>();
    private static int nextBulletId = 1;

    public int id;
    public Rigidbody2D rigidbody;
    public int thrownByPlayer;
    public Vector3 initalForce;
    public Server server;

    private void FixedUpdate()
    {
        server.serverSend.BulletPosition(this);
    }

    //충돌 기반이기 때문에 플레이어와의 충돌을 에디터에서 무시해야함
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //take damage... etc
        bullet.Remove(id);
        Destroy(gameObject);
        server.serverSend.BulletCollide(this);
    }

    public void Initialize(Vector3 _initialMovementDirection, float _initialForceStrength, int _thrownByPlayer, Server _server)
    {
        initalForce = _initialMovementDirection * _initialForceStrength;
        thrownByPlayer = _thrownByPlayer;
        server = _server;

        id = nextBulletId;
        nextBulletId++;
        bullet.Add(id, this);

        server.serverSend.SpawnBullet(this, thrownByPlayer);

        rigidbody.AddForce(initalForce);
        Debug.Log(nextBulletId);
    }

}
