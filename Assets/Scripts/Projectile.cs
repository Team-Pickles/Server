using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private static int nextProjectiledId = 1;

    public int id;
    public Rigidbody2D rigidbody;
    public int thrownByPlayer;
    public Vector3 initalForce;
    public float explosionRadius = 1.5f;
    public float explosionDamage = 75f;
    public Server server;
    public Room room;


    private void FixedUpdate()
    {
        server.serverSend.ProjectilesPosition(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"{collision.collider.name} - {collision.otherCollider.name}");
        Explode();
    }

    public void Initialize(Vector3 _initialMovementDirection, float _initialForceStrength, int _thrownByPlayer, Server server, Room room)
    {
        initalForce = _initialMovementDirection * _initialForceStrength;
        thrownByPlayer = _thrownByPlayer;
        this.server = server;
        this.room = room;

        id = nextProjectiledId;
        nextProjectiledId++;
        projectiles.Add(id, this);

        server.serverSend.SpawnProjectile(this, thrownByPlayer);

        rigidbody.AddForce(initalForce);
        StartCoroutine(ExplodeAfterTime());

    }

    private void Explode()
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        for (int i=-3;i<=3;i++)
        {
            for (int j=-3;j<=3;j++)
            {
                Vector3Int position = new Vector3Int((int)this.transform.localPosition.x + i, (int)this.transform.localPosition.y + j, 0);
                room.TileGroup.SetTile(position, null);
                positions.Add(position);
            }
        }
        server.serverSend.ProjectilesExploded(this, positions);
        projectiles.Remove(id);
        Destroy(gameObject);
    }

    private IEnumerator ExplodeAfterTime()
    {
        yield return new WaitForSeconds(5f);

        Explode();
    }


}
