using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;
    public GameObject projectilePrefab;
    public GameObject enemyPrefab;
    public GameObject bulletPrefab;
    public GameObject itemPrefab;

    public Item item;

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
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        Server.Start(4, 26950);

        //loadtile
        //load item
        NetworkManager.instance.InstantiatItemPrefab();
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, new Vector3(0f, 0.0f, 0f), Quaternion.identity).GetComponent<Player>();
    }

    public Projectile InstantiatProjectile(Transform _shootOrigin)
    {
        return Instantiate(projectilePrefab, _shootOrigin.position + _shootOrigin.right * 1.3f, Quaternion.identity).GetComponent<Projectile>();
    }

    public Bullet InstantiatbBulletPrefab(Transform _shootOrigin)
    {
        return Instantiate(bulletPrefab, _shootOrigin.position + _shootOrigin.right * 1.3f, Quaternion.identity).GetComponent<Bullet>();
    }

    public Item InstantiatItemPrefab()
    {
        return Instantiate(itemPrefab, new Vector3(5f, -2.5f, 0f), Quaternion.identity).GetComponent<Item>();
    }

}
