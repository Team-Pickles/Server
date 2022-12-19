using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningPool : MonoBehaviour
{
    public GameObject obj;
    public float spawnTime;
    public Server server;
    public Room room;
    GameObject Itemgroup;
    bool IsAlive()
    {
        return true;
    }
    IEnumerator Spawn()
    {
        while(IsAlive())
        {
            GameObject itemClone = Instantiate(obj, Itemgroup.transform);
            itemClone.transform.position = transform.position;
            Item _item = itemClone.GetComponent<Item>();
            _item.Init(room.roomId, (int)TileTypes.Spring_Sheet_0);
            _item.server = NetworkManager.instance.servers[room.serverPort];
            server.serverSend.SpawnItem(room, _item);
            room.items.Add(_item.id, _item);
            yield return new WaitForSeconds(spawnTime);
        }
        yield return 0;
    }
    
    public void init(Server server , Room room)
    {
        this.server = server;
        this.room = room;
        Itemgroup = GameObject.Find("ItemGroup");
        StartCoroutine(Spawn());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
