using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    public float maxHp = 150f;
    private float _currentHp;
    
    public GameObject breakEffect;
    
    void Start()
    {
        _currentHp = maxHp;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Debug.Log("Collision 감지: " + collision.gameObject.name);
        
        // 새와 충돌 시
        if (collision.gameObject.CompareTag("Bird"))
        {
            float damage = collision.relativeVelocity.magnitude * 10;
            TakeDamage(damage);
            // Debug.Log("Bird Damage: " + damage);
        }
    }

    void TakeDamage(float damage)
    {
        _currentHp -= damage;
        if (_currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 애니메이션, 오디오, 파티클 등 설정
        Instantiate(breakEffect, transform.position, transform.rotation); // 폭발 이펙트 생성
        Destroy(gameObject);
        // Debug.Log("블럭 삭제");
    }
}
