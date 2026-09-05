using System.Collections.Generic;
using UnityEngine;


public class HealthBarManager : MonoBehaviour
{
    public static HealthBarManager Instance { get; private set; }


    [Header("ÑªÌõ")]
    [SerializeField]
    private WorldHealthBar prefab;


    [SerializeField]
    private Transform canvasRoot;


    [SerializeField]
    private int preloadCount = 50;



    private readonly Queue<WorldHealthBar> pool =
        new Queue<WorldHealthBar>();



    private Transform poolRoot;



    private void Awake()
    {
        Instance = this;


        poolRoot =
            new GameObject("HealthBarPool")
            .transform;


        poolRoot.SetParent(transform);


        for (int i = 0; i < preloadCount; i++)
        {
            Create();
        }
    }



    private WorldHealthBar Create()
    {
        WorldHealthBar bar =
            Instantiate(
                prefab,
                canvasRoot
            );


        bar.gameObject.SetActive(false);


        pool.Enqueue(bar);


        return bar;
    }



    public WorldHealthBar Get()
    {
        WorldHealthBar bar;


        if (pool.Count > 0)
        {
            bar = pool.Dequeue();
        }
        else
        {
            bar = Create();
            pool.Dequeue();
        }


        bar.transform.SetParent(canvasRoot);


        bar.gameObject.SetActive(true);


        return bar;
    }



    public void Release(
        WorldHealthBar bar)
    {
        if (bar == null)
            return;


        bar.gameObject.SetActive(false);


        bar.transform.SetParent(poolRoot);


        pool.Enqueue(bar);
    }
}