using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AttackIndicator : MonoBehaviour
{
    static int DUMMY = 2;
    SpriteRenderer _sr;
    float _radius;
    float _thickness;
    public GameObject _dummy;
    public GameObject _trash;
    float rand;
    Color32 _attackColor = new Color32(255, 66, 66, 255);
    Color _trashColor = new Color(0.2f, 0.8f, 0.1f, 1.0f);
    public Server server;
    public Room room;
    public int port;

    public GameObject Itemgroup;
    public GameObject Enemygroup;

    public bool isInit = false;
    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _radius = 1.0f;
        _thickness = 0.1f;

        rand = Random.Range(0.0f, 1.0f);
        if (rand <= 0.2f)
            _sr.material.SetColor("_Color", _trashColor);
        else
            _sr.material.SetColor("_Color", _attackColor);
    }
    public void Init(Room room, Server server)
    {
        this.server = server;
        this.room = room;
        this.port = room.serverPort;
        Itemgroup = GameObject.Find(room.roomId).transform.GetChild(0).gameObject;
        Enemygroup = GameObject.Find(room.roomId).transform.GetChild(1).gameObject;
        isInit = true;
    }
    // Update is called once per frame
    void Update()
    {
        if(isInit == true)
        {
            _radius -= Time.deltaTime / 2.0f;
            if (_radius <= _thickness)
            {
                if (rand <= 0.2f)
                    Trash();
                else
                    Attack();
            }
        }
       
    }
    private void Trash()
    {
        int cnt = 2;
        float offset = Random.Range(0.0f, Mathf.PI);
        for (int i = 0; i < cnt; i++)
        {
            try
            {
                float xPos = transform.localPosition.x + 0.5f * Mathf.Cos(2.0f * Mathf.PI / cnt * i + offset);
                float yPos = transform.localPosition.y + 0.5f * Mathf.Sin(2.0f * Mathf.PI / cnt * i + offset);
                Vector2 spawnPos = new Vector2(xPos, yPos);

                GameObject itemClone = Instantiate(_trash, Itemgroup.transform);
                itemClone.transform.position = spawnPos;

                Item _item = itemClone.GetComponent<Item>();
                _item.Init(room.roomId, (int)TileTypes.trash);
                _item.server = NetworkManager.instance.servers[room.serverPort];
                room.items.Add(_item.id, _item);
                server.serverSend.SpawnItem(room, _item);
                itemClone.GetComponent<Rigidbody2D>().AddForce((spawnPos - (Vector2)transform.position).normalized * 200.0f);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }
        }
        DestroyImmediate(gameObject);
    }
    private void Attack()
    {
        int cnt = 3;
        int boomdummy = 2;
        float offset = Random.Range(0.0f, Mathf.PI);
        for (int i = 0; i < cnt; i++)
        {
            float xPos = transform.position.x + 0.1f * Mathf.Cos(2.0f * Mathf.PI / cnt * i + offset);
            float yPos = transform.position.y + 0.1f * Mathf.Sin(2.0f * Mathf.PI / cnt * i + offset);

            GameObject enemyClone = Instantiate(_dummy, Enemygroup.transform);
            enemyClone.transform.position = new Vector3(xPos, yPos, 0);
            Enemy _enemy = enemyClone.GetComponent<Enemy>();
            _enemy.Initialize(NetworkManager.instance.servers[room.serverPort], room, boomdummy);
            room.enemies.Add(_enemy.id, _enemy);
            server.serverSend.SpawnEnemy(room, _enemy, boomdummy);
            enemyClone.GetComponent<Boss1BoomDummy>().Init(transform.position, 10.0f, _enemy);
        }
        DestroyImmediate(gameObject);
    }

}
