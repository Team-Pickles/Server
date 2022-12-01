using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Random = System.Random;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;
    public GameObject projectilePrefab;
    public GameObject enemyPrefab;
    public GameObject bulletPrefab;
    public GameObject itemPrefab;
    public GameObject DefaultMapPrefab;
    public Transform RoomGroup;

    private int _port;

    public Dictionary<int, Server> servers = new Dictionary<int, Server>();
    public Dictionary<string, int> roomInfos = new Dictionary<string, int>();
    public Dictionary<string, string> roomNameIds = new Dictionary<string, string>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        Listener _listener = new Listener();
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        string host = "127.0.0.1";
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        //주소가 여러개일 수 있어서 배열로 받음
        IPAddress ipAddr = ipHost.AddressList[0];
        //최종적인 주소
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        try
        {
            //나의 엔드포인트를 알려주고, 연결이 되면 뒤에있는 핸들러를 호출해
            _listener.Init(endPoint, OnAcceptHandleer);
            Debug.Log("listening...");

        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

        //Server.Start(4, 26950);
        //loadtile
        //load item
    }

    private void OnApplicationQuit()
    {
        //server1.Stop();
    }
    static void OnAcceptHandleer(Socket clientSocket)
    {
        try
        {
            Debug.Log("test");
            Sender _sender = new Sender();
            _sender.Start(clientSocket);
            string msg = "Welcome to Server";
            foreach (KeyValuePair<string, int> roomInfo in instance.roomInfos)
            {
                msg += $",{roomInfo.Key}-{instance.roomNameIds[roomInfo.Key]}";
            }

            byte[] sendBuff = Encoding.UTF8.GetBytes(msg);
            _sender.Send(sendBuff);

            //Thread.Sleep(1000);
            //_session.Disconnect();

        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }

    }

    public class Sender
    {
        Socket _socket;
        int _disconnected = 0;

        object _lock = new object();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        bool _pending = false;

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();


        public void Start(Socket socket)
        {
            _socket = socket;
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024);

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            RegisterRecv(recvArgs);
        }
        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pending == false)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }
//
        #region Network comuunication, creat join room
        void RegisterRecv(SocketAsyncEventArgs recvArgs)
        {
            bool pending = _socket.ReceiveAsync(recvArgs);
            if (pending == false)
                OnRecvCompleted(null, recvArgs);
        }

        void OnRecvCompleted(object obj, SocketAsyncEventArgs recvArgs)
        {
            //연결이 끊긴다거나 하는 경우 체크
            if (recvArgs.BytesTransferred > 0 && recvArgs.SocketError == SocketError.Success)
            {
                //
                try
                {
                    string recvData = Encoding.UTF8.GetString(recvArgs.Buffer, recvArgs.Offset, recvArgs.BytesTransferred);
                    Debug.Log($"[From Client]{recvData}");
                    string[] requests = recvData.Split('-');
                    Debug.Log(requests.Length);
                    if(requests[0] == "RoomNameCheck")
                    {
                        if(instance.roomInfos.ContainsKey(requests[1]))
                        {
                            string result = "Duplicated";
                            byte[] sendBuff = Encoding.UTF8.GetBytes(result);
                            Send(sendBuff);
                        } else {
                            string result = "Ok";
                            byte[] sendBuff = Encoding.UTF8.GetBytes(result);
                            Send(sendBuff);
                        }
                    }
                    else if (requests[0] == "CreateRoom")
                    {
                        Debug.Log("Start to create room.");
                        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
                        l.Start();
                        int port = ((IPEndPoint)l.LocalEndpoint).Port;
                        l.Stop();

                        int _serverPort = port;
                        foreach(Server _server in instance.servers.Values)
                        {
                            if(_server.rooms.Count < 20)
                            {
                                _serverPort = _server.Port;
                                break;
                            }
                        }
                        
                        if(_serverPort == port)
                        {
                            bool isDone = false;
                            while(!isDone) {
                                Server server = new Server();
                                isDone = server.Start(4,port);
                                instance.servers.Add(port, server);
                            }
                        }
                        try {
                            string _roomName = requests[1];
                            string _roomId = RoomManager.instance.CreateRoom(_roomName, _serverPort, Convert.ToInt32(requests[2]));
                            instance.roomInfos.Add(_roomId, _serverPort);
                            instance.roomNameIds.Add(_roomId, _roomName);
                            string result = $"{_roomId}-{_roomName}";
                            byte[] sendBuff = Encoding.UTF8.GetBytes(result);
                            Debug.Log(result);
                            Send(sendBuff);
                        } catch(Exception _e)
                        {
                            Debug.Log(_e.Message);
                        }
                    }
                    else if (requests[0] == "RefreshRoom")
                    {
                        string result = "";
                        foreach (KeyValuePair<string, int> roomInfo in instance.roomInfos)
                        {
                            result += $"{roomInfo.Key}-{instance.roomNameIds[roomInfo.Key]},";
                        }
                        Debug.Log($"REFRESH {result}");
                        byte[] sendBuff = Encoding.UTF8.GetBytes(result);
                        Send(sendBuff);
                    }
                    else
                    {
                        string ReplaceResult = recvData.Replace("JoinRoom", "");
                        int result = -1;
                        instance.roomInfos.TryGetValue(ReplaceResult, out result);
                        byte[] sendBuff = Encoding.UTF8.GetBytes(result.ToString());
                        Send(sendBuff);
                    }

                    RegisterRecv(recvArgs);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted failed { e.ToString()} ");
                }
            }
            else
            {
                Disconnect();
            }
        }

        void RegisterSend()
        {
            _pending = true;
            byte[] buff = _sendQueue.Dequeue();
            _sendArgs.SetBuffer(buff, 0, buff.Length);

            bool pending = _socket.SendAsync(_sendArgs);
            
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object obj, SocketAsyncEventArgs sendArgs)
        {
            lock (_lock)
            {

                if (sendArgs.BytesTransferred > 0 && sendArgs.SocketError == SocketError.Success)
                {
                    try
                    {
                        //재사용을 할 수없음!
                        //RegisterSend(sendArgs)


                        //_sendQueue의 내용을 확인
                        if (_sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                        else
                            _pending = false;

                    }
                    catch (Exception e)
                    {
                        Debug.Log($"OnSendCompleted failed { e.ToString()} ");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
            #endregion
        }
    }
    public class Listener
    {
        Socket _listenSocket;
        Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //이 함수가 완료된 것을 이벤트 형식으로 알려주기 위해서
            _onAcceptHandler += onAcceptHandler;
            _listenSocket.Bind(endPoint);

            //backlog : 최대 대기 수
            _listenSocket.Listen(10);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            //이벤트 핸들러 방식
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);

            //초기화 하면서 등록을 해줌
            //이 상태에서 클라이언트가 연결 요청을 하면 콜백방식로 들어감
            RegisterAccept(args);
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            //기존의 잔재들을 없앰
            args.AcceptSocket = null;

            //연결이 동시에 일어다면 false
            //false
            //if the I/O operation completed synchronously.
            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)
                OnAcceptCompleted(null, args);
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
                Debug.Log(args.SocketError.ToString());

            //다음 클라이언트 연결을위해서
            RegisterAccept(args);
        }
    }


    public GameObject InstantiatePlayer()
    {
        return Instantiate(playerPrefab, new Vector3(0f, 0.0f, 0f), Quaternion.identity);
    }

    public GameObject InstantiatProjectile(Transform _shootOrigin)
    {
        return Instantiate(projectilePrefab, _shootOrigin.position + _shootOrigin.right * 1.3f, Quaternion.identity);
    }

    public GameObject InstantiatbBulletPrefab(Transform _shootOrigin)
    {
        return Instantiate(bulletPrefab, _shootOrigin.position + _shootOrigin.right * 1.3f, Quaternion.identity);
    }

    public GameObject InstantiatItemPrefab()
    {
        return Instantiate(itemPrefab, new Vector3(5f, -2.5f, 0f), Quaternion.identity);
    }
    
    public GameObject InstantiateRoomPrefab() {
        return Instantiate(DefaultMapPrefab, RoomGroup);
    }
}
