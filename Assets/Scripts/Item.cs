using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public static Dictionary<int, Item> items = new Dictionary<int, Item>();
    private static int nextItemId = 1;

    public int id;
    public Rigidbody2D rigidbody;
    public Server server;

    public Item(Server server)
    {
        this.server = server;
    }

    private void Start()
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
