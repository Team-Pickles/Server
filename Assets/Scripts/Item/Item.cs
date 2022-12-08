using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    private static int nextItemId = 1;

    public int id;
    
    public Server server;
    public string roomId;
    public int itemType;


    public void Init(string _roomId, int _itemType)
    {
        id = nextItemId;
        nextItemId++;
        
        roomId = _roomId;
        itemType = _itemType;
    }

    private void Update()
    {
        if (this.tag == "trash")
            server.serverSend.ItemPosition(this);
    }

   public void DeleteItem()
    {
        Destroy(gameObject);
    }

    public void SpringColorChange()
    {
        server.serverSend.SpringColorChange(this);
    }

}
