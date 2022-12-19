using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Fragile : MonoBehaviour
{
    public Room room;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.tag);
        if (collision.gameObject.CompareTag("bullet"))
        {
            float _x = collision.transform.position.x;
            float _y = collision.transform.position.y;
            int _posX = (int)Math.Floor(_x);
            int _posY = (int)Math.Floor(_y);
            Tilemap _tilemap = gameObject.GetComponent<Tilemap>();
            List<Vector3Int> _breakPos = new List<Vector3Int>();
            for (int i = -1; i < 2; i++)
            {
                for(int j = -1; j < 2; j++)
                {
                    Vector3Int _position = new Vector3Int((_posX + i), (_posY + j), 0);
                    _tilemap.SetTile(_position, null);
                    _breakPos.Add(_position);
                }
            }
            NetworkManager.instance.servers[room.serverPort].serverSend.FragileBreak(room.roomId, _breakPos);
        }
    }
}
