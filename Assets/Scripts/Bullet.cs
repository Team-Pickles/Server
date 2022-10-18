using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public static Dictionary<int, Bullet> bullet = new Dictionary<int, Bullet>();
    private static int nextBulletId = 1;

    public int id;
    public Rigidbody2D rigidbody;
    public int thrownByPlayer;
    public Vector3 initalForce;

    private void Start()
    {
        id = nextBulletId;
        nextBulletId++;
        bullet.Add(id, this);

        ServerSend.SpawnBullet(this, thrownByPlayer);

        rigidbody.AddForce(initalForce);

    }

    private void FixedUpdate()
    {
        ServerSend.BulletPosition(this);
    }

    //�浹 ����̱� ������ �÷��̾���� �浹�� �����Ϳ��� �����ؾ���
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //take damage... etc
        bullet.Remove(id);
        Destroy(gameObject);
        ServerSend.BulletCollide(this);
    }

    public void Initialize(Vector3 _initialMovementDirection, float _initialForceStrength, int _thrownByPlayer)
    {
        initalForce = _initialMovementDirection * _initialForceStrength;
        thrownByPlayer = _thrownByPlayer;
    }

}
