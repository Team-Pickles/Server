using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public Dictionary<int, Item> items = new Dictionary<int, Item>();
    private int nextItemId = 1;

    public int id;
    public Rigidbody2D rigidbody;
    public Server server;


    public void Init()
    {
        id = nextItemId;
        nextItemId++;
        items.Add(id, this);
    }

    private void Update()
    {
        server.serverSend.ItemPosition(this);
    }

   public void DeleteItem()
    {
        Destroy(gameObject);
    }

}
