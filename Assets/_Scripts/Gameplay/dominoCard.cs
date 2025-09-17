// File: _Scripts/Gameplay/Domino.cs

using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class dominoCard : MonoBehaviour
{
    private Rigidbody rb;
    void Awake() { rb = GetComponent<Rigidbody>(); }

    private void Start()
    {
        StartCoroutine(asd());
    }

    public void Fall()
    {
        rb.AddRelativeTorque(Vector3.back * 1f, ForceMode.Impulse);
    }

    private IEnumerator asd()
    {
        yield return new WaitForSeconds(0.3f);
        Fall();
    }
}