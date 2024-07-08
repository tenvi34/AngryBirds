using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KingPigController : MonoBehaviour
{
    public float maxHp = 200f;
    private float _currentHp;

    // 폭발 이펙트
    public GameObject explosionEffect;
    
    // 데미지버전 돼지
    public GameObject damagedKingPig;
    private bool isDamaged = false; // 데미지입은 상태 확인
    
    void Start()
    {
        _currentHp = maxHp;
        Debug.Log("돼지 소환");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision 감지: " + collision.gameObject.name);
        
        // 새와 충돌 시
        if (collision.gameObject.CompareTag("Bird"))
        {
            float damage = collision.relativeVelocity.magnitude * 10;
            TakeDamage(damage);
            Debug.Log("Bird Damage: " + damage);
        }
        // 나무 블럭과 충돌 시
        else if (collision.gameObject.CompareTag("WoodBlock"))
        {
            float damage = 30;
            TakeDamage(damage);
            Debug.Log("WoodBlock Damage: " + damage);
        }
        // 돌 블럭과 충돌 시
        else if (collision.gameObject.CompareTag("StoneBlock"))
        {
            float damage = 30;
            TakeDamage(damage);
            Debug.Log("StoneBlock Damage: " + damage);
        }
    }

    void TakeDamage(float damage)
    {
        _currentHp -= damage;
        if (_currentHp <= 0)
        {
            Die();
        }
        else if (_currentHp <= 100 & !isDamaged)
        {
            ChangeDamagedPig();
        }
    }

    void ChangeDamagedPig()
    {
        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;
        
        Destroy(gameObject); // 기존 멀쩡한 돼지 삭제
        Instantiate(damagedKingPig, currentPosition, currentRotation); // 데미지 버전으로 교체
        isDamaged = true;
    }

    void Die()
    {
        // 애니메이션, 오디오, 파티클 등 설정
        Instantiate(explosionEffect, transform.position, transform.rotation); // 폭발 이펙트 생성
        Destroy(gameObject);
        Debug.Log("돼지 삭제");
    }
}
