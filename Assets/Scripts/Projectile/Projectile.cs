using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Projectile : MonoBehaviour
{
    public Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private GameObject _tilemapFragile, _tilemapBlock;
    private static int nextProjectiledId = 1;
    private int _x, _y;

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


    public void Initialize(int _thrownByPlayer, Server server, Room room, int isFlip)
    {
        thrownByPlayer = _thrownByPlayer;
        this.server = server;
        this.room = room;

        id = nextProjectiledId;
        nextProjectiledId++;
        projectiles.Add(id, this);

        _tilemapFragile = GameObject.Find("Fragile");
        _tilemapBlock = GameObject.Find("Block");

        server.serverSend.SpawnProjectile(this, thrownByPlayer);

        GetComponent<Rigidbody2D>().AddForce(new Vector2(100.0f * isFlip, 500.0f));
        GetComponent<Rigidbody2D>().angularVelocity = 300.0f;
        StartCoroutine(ExplodeAfterTime());

    }

    private void Explode()
    {
        _x = (int)Math.Floor(transform.position.x);
        _y = (int)Math.Floor(transform.position.y);
        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                Vector3Int position = new Vector3Int(_x + i, _y + j, 0);
                _tilemapFragile.GetComponent<Tilemap>().SetTile(position, null);
                _tilemapBlock.GetComponent<Tilemap>().SetTile(position, null);
            }
        }
        server.serverSend.ProjectilesExploded(this);
        projectiles.Remove(id);
        Destroy(gameObject);
    }

    private IEnumerator ExplodeAfterTime()
    {
        yield return new WaitForSeconds(5f);

        Explode();
    }

}
