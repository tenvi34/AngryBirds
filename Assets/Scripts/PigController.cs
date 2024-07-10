using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PigController : MonoBehaviour
{
    public float maxHp = 200f;
    private float _currentHp;

    // 폭발 이펙트
    public GameObject explosionEffect;
    
    void Start()
    {
        _currentHp = maxHp;
        GameManager.Instance.AddPig(this);
        // Debug.Log("돼지 소환");
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
        // 나무 블럭과 충돌 시
        else if (collision.gameObject.CompareTag("WoodBlock"))
        {
            float damage = 30;
            TakeDamage(damage);
            // Debug.Log("WoodBlock Damage: " + damage);
        }
        // 돌 블럭과 충돌 시
        else if (collision.gameObject.CompareTag("StoneBlock"))
        {
            float damage = 30;
            TakeDamage(damage);
            // Debug.Log("StoneBlock Damage: " + damage);
        }
        // 기타 충돌 시(ex: 바닥 등)
        else
        {
            float damage = collision.relativeVelocity.magnitude * 3;
            TakeDamage(damage);
            // Debug.Log("Other Damage: " + damage);
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
        Instantiate(explosionEffect, transform.position, transform.rotation); // 폭발 이펙트 생성
        Destroy(gameObject);
        // Debug.Log("돼지 삭제");
        
        GameManager.Instance.PigDestroyed(this);
    }
}
