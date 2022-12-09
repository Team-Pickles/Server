using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public int id;
    public Door nextPortal;
    public Room room;
    public Server server;
    public bool isInDoor;

    public void Initialize(int _id, bool _isInDoor, Room _room)
    {
        id = _id;
        isInDoor = _isInDoor;
        room = _room;
    }

    public void GoToNextPortal(){
        room.GoToNextPortal(nextPortal.transform.position);
    }
}
