using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private static int nextProjectiledId = 1;

    public int id;
    public Rigidbody2D rigidbody;
    public int thrownByPlayer;
    public Vector3 initalForce;
    public float explosionRadius = 1.5f;
    public float explosionDamage = 75f;

    private void Start()
    {
        id = nextProjectiledId;
        nextProjectiledId++;
        projectiles.Add(id, this);

        ServerSend.SpawnProjectile(this, thrownByPlayer);

        rigidbody.AddForce(initalForce);
        StartCoroutine(ExplodeAfterTime());

    }

    private void FixedUpdate()
    {
        ServerSend.ProjectilesPosition(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"{collision.collider.name} - {collision.otherCollider.name}");
        Explode();
    }

    public void Initialize(Vector3 _initialMovementDirection, float _initialForceStrength, int _thrownByPlayer)
    {
        initalForce = _initialMovementDirection * _initialForceStrength;
        thrownByPlayer = _thrownByPlayer;
    }

    private void Explode()
    {
        Collider2D[] _colliders = Physics2D.OverlapCircleAll(this.transform.position, explosionRadius);
        ServerSend.ProjectilesExploded(this, _colliders);

        foreach (Collider2D _collider in _colliders)
        {
            // if (_collider.CompareTag("Player"))
            // {
            //     _collider.GetComponent<Player>().TakeDamage(explosionDamage);
            // }
            // else if (_collider.CompareTag("Enemy"))
            // {
            //     _collider.GetComponent<Enemy>().TakeDamage(explosionDamage);
            // }
            if(_collider.CompareTag("floor"))
            {
                Debug.Log(_collider.transform.position);
                Destroy(_collider.gameObject);
            }
        }
        projectiles.Remove(id);
        Destroy(gameObject);
    }

    private IEnumerator ExplodeAfterTime()
    {
        yield return new WaitForSeconds(5f);

        Explode();
    }


}