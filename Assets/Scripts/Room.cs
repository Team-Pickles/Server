using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room
{
    public string roomId;
    public int mapId;
    public string roomName;
    public int maxPlayer;
    public int masterId;
    public int serverPort;
    
    public Vector3 mapAddPosition;

    public GameObject room;
    public GameObject PlayerGroup;
    public Grid TileGroupGrid;
    public Tilemap TileGroup;
    public GameObject EnemyGroup;
    public GameObject ItemGroup;

    public Dictionary<int, Client> members = new Dictionary<int, Client>();
    public Dictionary<Vector3, int> ItemInfos = new Dictionary<Vector3, int>();
    public Dictionary<Vector3, int> EnemyInfos = new Dictionary<Vector3, int>();
    public Vector3 spawnPoint = new Vector3(0, 0, 0);
    public Dictionary<int, Item> items = new Dictionary<int, Item>();
    public Dictionary<int, Enemy> enemies = new Dictionary<int, Enemy>();

    public Room(string _roomId, string _roomName, int _serverPort, Vector3 _mapAddPosition, GameObject _room)
    {
        roomId = _roomId;
        roomName = _roomName;
        serverPort = _serverPort;
        mapAddPosition = _mapAddPosition;
        room = _room;
        room.transform.localPosition = new Vector3(0, 0, 0) + _mapAddPosition;
        for(int i = 0; i < room.transform.childCount; ++i)
        {
            GameObject now = room.transform.GetChild(i).gameObject;
            switch(now.name)
            {
                case "TileGroup":
                    TileGroupGrid = now.GetComponent<Grid>();
                    TileGroup = now.GetComponentInChildren<Tilemap>();
                    break;
                case "ItemGroup":
                    ItemGroup = now;
                    break;
                case "EnemyGroup":
                    EnemyGroup = now;
                    break;
                case "PlayerGroup":
                    PlayerGroup = now;
                    break;
            }
        }
    }

    public void StartGame(int map_id)
    {
        if(room == null)
        {
            Debug.Log("There is no mapInfo.");
            return;
        }
        Debug.Log("Spawn");
        ThreadManager.ExecuteOnMainThread(() => 
            {
                SetMapInfo(map_id);
                SpawnPlayerAndItems();
            }
        );
        
    }

    public void SpawnPlayerAndItems()
    {
        foreach(Client _member in members.Values)
        {
            GameObject playerClone = RoomManager.instance.InstatiatePlayer(PlayerGroup, 0);
            playerClone.name = "Player " + _member.id;
            playerClone.transform.localPosition = spawnPoint;

            Player _player = playerClone.GetComponent<Player>();
            _player.Initialize(_member.id, "tester");
            _player.server = NetworkManager.instance.servers[serverPort];
            _player.room = this;
            _member.player = _player;

            TileGroupGrid.GetComponent<MapManager>().players.Add(playerClone);
        }

        TileGroupGrid.GetComponent<MapManager>().Init();

        foreach(Client _member in members.Values)
        {
            // 내 클론을 스폰
            foreach (Client _client in members.Values)
            {
                Debug.Log(_member.id + ", " + _client.id);

                if (_client.player != null)
                {
                    NetworkManager.instance.servers[serverPort].serverSend.SpawnPlayer(_client.id, _member.player);
                }
            }
            if(items != null) {
                foreach (Item _item in items.Values)
                {
                    NetworkManager.instance.servers[serverPort].serverSend.SpawnItem(_member.id, _item);
                }
            }
            if(enemies != null) {
                foreach (Enemy _enemy in enemies.Values)
                {
                    NetworkManager.instance.servers[serverPort].serverSend.SpawnEnemy(_enemy, _member.id);
                }
            }
        }
    }

    private void SetMapInfo(int map_id)
    {
        RoomManager.instance.LoadMap(this, map_id);
    }

    public Vector3Int GetTopLeftBasePosition(Vector3 position)
    {
        // local X. Calculate with world position.
        Vector3Int temp = TileGroup.WorldToCell(position);
        return new Vector3Int(temp.x + TileGroup.size.x, TileGroup.size.y - temp.y);
    }
}