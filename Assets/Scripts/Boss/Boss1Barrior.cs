using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1Barrior : Boss1State
{
    int barriorCount = 5;
    public GameObject Itemgroup;
    public GameObject Enemygroup;
    public Boss1Barrior(Boss1 boss) : base(boss)
    {
    }
    public override IEnumerator Start(Room room, Server server)
    {
        Debug.Log("Barrior start");
        Itemgroup = GameObject.Find(room.roomId).transform.GetChild(0).gameObject;
        Enemygroup = GameObject.Find(room.roomId).transform.GetChild(1).gameObject;
        yield return new WaitForSeconds(1.0f);
        yield return Skill(room, server);


        //yield return new WaitForSeconds(3.0f);
        //_boss.OnEnd();
        //yield break;
    }
    public override IEnumerator Skill(Room room, Server server)
    {
        var temp = GameObject.Find(room.roomId).transform.GetChild(1).GetChild(0).transform;
        Vector2 barriorPosition = new Vector2(temp.localPosition.x, temp.localPosition.y);

        int barrior = 2;
        for (int i=0;i < barriorCount;i++)
        {
            GameObject enemyClone = Object.Instantiate(_boss.barrior,Enemygroup.transform);
            enemyClone.transform.localPosition = barriorPosition;
            Boss1BarriorMove _move = enemyClone.GetComponent<Boss1BarriorMove>();
            _move._fixedPosition = barriorPosition;
            enemyClone.name = "barror(Clone)";
            Enemy _enemy = enemyClone.GetComponent<Enemy>();
            _enemy.Initialize(NetworkManager.instance.servers[room.serverPort], room, barrior);
            room.enemies.Add(_enemy.id, _enemy);
            server.serverSend.SpawnEnemy(room, _enemy, barrior);
            enemyClone.GetComponent<Rigidbody2D>().angularVelocity = 100.0f;
            yield return new WaitForSeconds(2.0f / barriorCount);
        }
        _boss.SetState(new Boss1Rain(_boss));
        yield break;
    }
    public override IEnumerator End()
    {
        yield break;
    }
}

