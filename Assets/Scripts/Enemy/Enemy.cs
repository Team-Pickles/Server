using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

enum EnemyState { Normal, Captive}

public class Enemy : MonoBehaviour
{
    public int id;
    private static int nextEnemyId = 1;
    public Server server;
    public Room room;
    public MapManager mapManager;
    private Vector3Int enemyPosition;
    private const float _threshold = 1.0f;
    private float currentHold = 0.0f;
    private bool onGround = false;
    public bool OnGround
    {
        get { return onGround; }
        set { onGround = value; }
    }
    private bool isDead = false;
    private int hitPoint = 1;

    private EnemyState state = EnemyState.Normal;

    public void Initialize(Server _server, Room _room) {
        id = nextEnemyId;
        nextEnemyId++;
        server = _server;
        room = _room;
        mapManager = _room.TileGroupGrid.GetComponent<MapManager>();
        enemyPosition = mapManager.GetTopLeftBasePosition(transform.position);
    }

    void Update()
    {
        enemyPosition = mapManager.GetTopLeftBasePosition(transform.position);

        Dictionary<Vector3, float> playerEnemyDistances = new Dictionary<Vector3, float>();
        foreach(Vector3 _playerPos in mapManager.playerPositions)
        {
            playerEnemyDistances.TryAdd(_playerPos, Mathf.Pow(enemyPosition.x - _playerPos.x,2) + Mathf.Pow(enemyPosition.y - _playerPos.y,2));
        }
        Debug.Log(Mathf.Pow(enemyPosition.x - mapManager.playerPositions[0].x,2));
        Debug.Log((enemyPosition.x - mapManager.playerPositions[0].x) * (enemyPosition.x - mapManager.playerPositions[0].x));
        Debug.Log(Mathf.Pow(enemyPosition.y - mapManager.playerPositions[0].y,2));
        Debug.Log((enemyPosition.y - mapManager.playerPositions[0].y) * (enemyPosition.y - mapManager.playerPositions[0].y));

        if (playerEnemyDistances.Values.Min() <= 49)
        {
            Vector3 playerPosition = playerEnemyDistances.FirstOrDefault(entry =>
            EqualityComparer<float>.Default.Equals(entry.Value, playerEnemyDistances.Values.Min())).Key;

            if (onGround && enemyPosition.y > playerPosition.y)
            {
                GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 5.0f);
            }

            if (GetComponent<Rigidbody2D>().velocity.x > 0.3f)
            {
                if (playerPosition.x < enemyPosition.x)
                {
                    currentHold += 5.0f * Time.deltaTime;
                }
                else
                {
                    currentHold = 0.0f;
                }
                if (currentHold >= _threshold && onGround)
                {
                    GetComponent<Rigidbody2D>().velocity = new Vector2(-2.0f, GetComponent<Rigidbody2D>().velocity.y);
                    currentHold = 0.0f;
                }
            }
            else if (GetComponent<Rigidbody2D>().velocity.x < -0.3f)
            {
                if (playerPosition.x > enemyPosition.x)
                {
                    currentHold += 5.0f * Time.deltaTime;
                }
                else
                {
                    currentHold = 0.0f;
                }
                if (currentHold >= _threshold && onGround)
                {
                    GetComponent<Rigidbody2D>().velocity = new Vector2(2.0f, GetComponent<Rigidbody2D>().velocity.y);
                    currentHold = 0.0f;
                }
            }
            else
            {
                if (playerPosition.x < enemyPosition.x)
                {
                    GetComponent<Rigidbody2D>().velocity = new Vector2(-2.0f, GetComponent<Rigidbody2D>().velocity.y);
                }
                else if (playerPosition.x > enemyPosition.x)
                {
                    GetComponent<Rigidbody2D>().velocity = new Vector2(2.0f, GetComponent<Rigidbody2D>().velocity.y);
                }
                if (onGround)
                {
                    GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 5.0f);
                }
            }
        }
        else
        {
            if (GetComponent<Rigidbody2D>().velocity == new Vector2(0,0))
            {
                if (Random.Range(1,10) <= 5)
                {
                    GetComponent<Rigidbody2D>().velocity = new Vector2(-2.0f, GetComponent<Rigidbody2D>().velocity.y);
                }
                else
                {
                    GetComponent<Rigidbody2D>().velocity = new Vector2(2.0f, GetComponent<Rigidbody2D>().velocity.y);
                }
            }
        }

        server.serverSend.EnemyPosition(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "bullet" && isDead == false)
        {
            OnDie();
            Debug.Log(collision.transform.tag);
            Destroy(collision.gameObject);
            hitPoint -= 1;
            if (hitPoint <= 0)
            {
                isDead = true;
                Destroy(gameObject);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.tag == "bullet" && isDead == false)
        {
            OnDie();
            Debug.Log(collision.transform.tag);
            Destroy(collision.gameObject);
            hitPoint -= 1;
            if (hitPoint <= 0)
            {
                isDead = true;
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (state == EnemyState.Normal && collision.transform.tag == "player" && isDead == false)
        {
            collision.transform.GetComponent<Player>().OnDamaged();
        }
    }

    private void OnDie() {
        server.serverSend.EnemyHealth(this);
    }
}
