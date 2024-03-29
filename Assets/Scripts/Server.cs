﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;


public class Server
{
    public int MaxPlayer { get; private set; }
    public int Port { get; private set; }

    private TcpListener tcpListener;
    private UdpClient udpListener;

    public Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public Dictionary<string, Room> rooms = new Dictionary<string, Room>();
    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public Dictionary<int, PacketHandler> packetHandler;

    public ServerHandle serverHandle;
    public ServerSend serverSend;
    string host = "ec2-3-36-114-195.ap-northeast-2.compute.amazonaws.com";
    public bool Start(int _maxPlayer, int _port)
    {
        if (NetworkManager.instance.isEditor == true)
            host = "127.0.0.1";
        MaxPlayer = _maxPlayer;
        Port = _port;

        Debug.Log($"Starting Server....");
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint ipLocalEndPoint = new IPEndPoint(ipAddr, _port);
        InitalizeServerData();
        try
        {
            tcpListener = new TcpListener(ipLocalEndPoint);
            tcpListener.Start();

            //비동기 연결
            tcpListener.BeginAcceptTcpClient(TCPConnectionCallback, null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Debug.Log($"Server started on {Port}.");

            return true;
        }
        catch (Exception _e)
        {
            Debug.Log(_e.Message);
            return true;
        }
    }

    private void TCPConnectionCallback(IAsyncResult _result)
    {
        try {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectionCallback), null);

            Debug.Log($"Incomming connenction from {_client.Client.RemoteEndPoint}...");
            for (int i = 1; i <= MaxPlayer; i++)
            {
                //해당 객체가 비어있다면
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }
            Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server Full!");
        }
        catch(Exception _ex)
        {
            Debug.Log($"Error occured with : {_ex}");
        }
    }

    private void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (_data.Length < 4)
            {
                return;
            }

            using (Packet _packet = new Packet(_data))
            {
                int _clientId = _packet.ReadInt();

                if (_clientId == 0)
                {
                    return;
                }

                //등록 안되어 있으면 등록하기
                if (clients[_clientId].udp.endPoint == null)
                {
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                // 기존의 연결인지 확인 (아무나 못보내게)
                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    clients[_clientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error occured with : {_ex}");
        }
    }



    public void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error occured with : {_ex}");
        }
    }


    private void InitalizeServerData()
    {
        for (int i = 1; i <= MaxPlayer; i++)
        {
            clients.Add(i, new Client(i, $"tester_{i}", this));
        }
        serverHandle = new ServerHandle(this);
        serverSend = new ServerSend(this);
        packetHandler = new Dictionary<int, PacketHandler>()
        {
            { (int)ClientPackets.TCPConnenctinCheckReceived, serverHandle.TCPConnenctinCheckReceived },
            { (int)ClientPackets.udpTestReceive, serverHandle.UDPTestReceive },
            { (int)ClientPackets.playerMovement, serverHandle.PlayerMovement },
            { (int)ClientPackets.playerShootBullet, serverHandle.PlayerBullet },
            { (int)ClientPackets.playerShootGrenade, serverHandle.PlayerGrenade },
            { (int)ClientPackets.playerStartVacuume, serverHandle.PlayerStartVaccume },
            { (int)ClientPackets.playerEndVacuume, serverHandle.PlayerEndVaccume },
            { (int)ClientPackets.ItemCollide, serverHandle.ItemCollide },
            { (int)ClientPackets.readyToStartGame, serverHandle.ReadyToStartGame},
            { (int)ClientPackets.playerJump, serverHandle.PlayerJump },
            { (int)ClientPackets.playerRopeMove, serverHandle.PlayerRopeMove },
            { (int)ClientPackets.goToNextPortal, serverHandle.GoToNextPortal },
            { (int)ClientPackets.readyToRestart, serverHandle.ReadyToRestartGame },
        };

        Debug.Log("Initialized pakcets.");
    }

    public void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }

}
