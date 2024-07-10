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
    private bool isDamaged = false; // 데미지 입은 상태 확인

    void Start()
    {
        _currentHp = maxHp;
        GameManager.Instance.AddPig(this);
        Debug.Log("킹돼지 소환");
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
        // 기타 충돌 시(ex: 바닥 등)
        else
        {
            float damage = collision.relativeVelocity.magnitude * 3;
            TakeDamage(damage);
            Debug.Log("Other Damage: " + damage);
        }
    }

    void TakeDamage(float damage)
    {
        _currentHp -= damage;
        if (_currentHp <= 0)
        {
            Die();
        }
        else if (_currentHp <= 100 && !isDamaged)
        {
            ChangeDamagedPig();
        }
    }

    void ChangeDamagedPig()
    {
        if (isDamaged) return;  // 중복 호출 방지
        isDamaged = true;  // 중복 호출 방지 플래그 설정

        Vector3 currentPosition = transform.position;
        Quaternion currentRotation = transform.rotation;

        // 새로운 damagedKingPig 생성
        GameObject newPig = Instantiate(damagedKingPig, currentPosition, currentRotation);
        KingPigController newPigController = newPig.GetComponent<KingPigController>();

        // 새로운 damagedKingPig의 현재 체력을 유지
        newPigController._currentHp = _currentHp;

        // 새로운 damagedKingPig 등록
        GameManager.Instance.AddPig(newPigController);

        // 기존 KingPig 삭제 알림 및 오브젝트 삭제
        GameManager.Instance.PigDestroyed(this);
        Destroy(gameObject);
    }

    void Die()
    {
        // 애니메이션, 오디오, 파티클 등 설정
        Instantiate(explosionEffect, transform.position, transform.rotation); // 폭발 이펙트 생성
        Destroy(gameObject);
        Debug.Log("킹돼지 삭제");

        GameManager.Instance.PigDestroyed(this);
    }
}
