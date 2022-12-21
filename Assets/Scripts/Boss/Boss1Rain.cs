using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss1Rain : Boss1State
{
    const int attackCount = 20;
    GameObject attackIndicator;
    SpriteRenderer sr;
    public Boss1Rain(Boss1 boss) : base(boss)
    {
    }
    public override IEnumerator Start(Room room, Server server)
    {
        attackIndicator = Resources.Load("Prefabs/Boss1/AttackIndicator") as GameObject;
        sr = _boss.attackRange.GetComponent<SpriteRenderer>();
        yield return new WaitForSeconds(1.0f);
        yield return Skill(room,server);
    }
    public override IEnumerator Skill(Room room, Server server)
    {
        int cnt = 0;
        float x, y;
        // 20�� ���� �ʴ� 1ȸ ����
        while(cnt < attackCount)
        {
            x = Random.Range(sr.bounds.min.x, sr.bounds.max.x);
            y = Random.Range(sr.bounds.min.y, sr.bounds.max.y);

            GameObject _indicatorClone = Object.Instantiate(attackIndicator, new Vector2(x, y), new Quaternion());
            AttackIndicator _indicator = _indicatorClone.GetComponent<AttackIndicator>();
            server.serverSend.AttackIndeicator(new Vector2(x, y), room);
            _indicator.Init(room, server);

            cnt++;
            yield return new WaitForSeconds(1.0f);
        }
        // 3�� ����
        yield return new WaitForSeconds(3.0f);
        
        // �ٸ� ��������
        if (_boss.hp > 0)
            _boss.SetState(new Boss1Barrior(_boss));
        //_boss.SetState(new Boss1Idle(_boss));
        //yield break;
    }
    public override IEnumerator End()
    {
        yield break;
    }
}
