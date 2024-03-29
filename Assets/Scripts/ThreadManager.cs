using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateRoomData {
    public string roomId;
    public string roomName;
    public int serverPort;

    public int mapId;

    public CreateRoomData(string _roomId, string _roomName, int _serverPort, int _mapId)
    {
        this.roomId = _roomId;
        this.roomName = _roomName;
        this.serverPort = _serverPort;

        this.mapId = _mapId;

    }
}

public class ThreadManager : MonoBehaviour
{
    private static readonly List<Action> executeOnMainThread = new List<Action>();
    private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
    public static readonly List<CreateRoomData> createRoomOnMainThread = new List<CreateRoomData>();
    private static bool actionToExecuteOnMainThread = false;
    private static bool executeCreateRoomOnMainThread = false;

    private void FixedUpdate()
    {
        UpdateMain();
    }

    /// <summary>Sets an action to be executed on the main thread.</summary>
    /// <param name="_action">The action to be executed on the main thread.</param>
    public static void ExecuteOnMainThread(Action _action)
    {
        if (_action == null)
        {
            Console.WriteLine("No action to execute on main thread!");
            return;
        }

        lock (executeOnMainThread)
        {
            executeOnMainThread.Add(_action);
            actionToExecuteOnMainThread = true;
        }
    }

    /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
    public static void UpdateMain()
    {
        if (actionToExecuteOnMainThread)
        {
            executeCopiedOnMainThread.Clear();
            lock (executeOnMainThread)
            {
                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
                actionToExecuteOnMainThread = false;
            }

            for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
            {
                executeCopiedOnMainThread[i]();
            }
        }
        if (createRoomOnMainThread.Count > 0 && !executeCreateRoomOnMainThread)
        {
            executeCreateRoomOnMainThread = true;
            foreach(CreateRoomData _roomData in createRoomOnMainThread)
            {
                GameObject _room = NetworkManager.instance.InstantiateRoomPrefab();
                _room.name = _roomData.roomId;

                Room _roomObject = new Room(_roomData.roomId, _roomData.roomName, _roomData.serverPort, _room);
                _roomObject.mapId = _roomData.mapId;

                NetworkManager.instance.servers[_roomData.serverPort].rooms.Add(_roomData.roomId, _roomObject);
                RoomManager.instance.InitRoomPos(_roomData.roomId, _roomData.mapId, _roomData.serverPort);
            }
            createRoomOnMainThread.Clear();
            executeCreateRoomOnMainThread = false;
        }
    }
}