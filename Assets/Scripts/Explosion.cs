using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float explosionDuration = 1.0f;
    
    void Start()
    {
        StartCoroutine(DestroyTime());
    }

    IEnumerator DestroyTime()
    {
        yield return new WaitForSeconds(explosionDuration);
        Destroy(gameObject);
    }
}
