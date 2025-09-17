// File: _Scripts/Gameplay/DominoPresenter.cs

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class spawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int initialCount = 5;   
    [SerializeField] private float spacing = 1.2f;  
    private ObjectPool<GameObject> pool;
    private dominoCard[] cards;
    private void Awake()
    {
        pool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(prefab),
            actionOnRelease: (obj) => obj.SetActive(false)
        );
    }

    private void Start()
    {
        for (int i = 0; i < initialCount; i++)
        {
            Spawn(i);
        }
        prefab.GetComponent<dominoCard>();

    }

    public void Spawn(int index)
    {
        GameObject obj = pool.Get();
        obj.transform.position = spawnPoint.position + new Vector3(index * spacing, 0f, 0f);
    }
}