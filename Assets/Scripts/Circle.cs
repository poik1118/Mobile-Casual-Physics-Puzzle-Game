using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    public int              circleLevel;
    public bool             isDrag;
    
    Rigidbody2D             rigid;
    Animator                anime;
    
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anime = GetComponent<Animator>();
    }

    void OnEnable()
    {
        anime.SetInteger("Level", circleLevel);
    }

    void Update()
    {
        if(isDrag){
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 스크린 좌표를 월드 좌표로 변환

            // X축 경계 설정
            float leftBorder = -4.2f + transform.localScale.x / 2.0f;
            float rightBorder = 4.2f - transform.localScale.x / 2.0f;

            if(mousePos.x < leftBorder){
                mousePos.x = leftBorder;            // 마우스가 왼쪽을 넘어가도 보이도록
            }
            else if(mousePos.x > rightBorder){
                mousePos.x = rightBorder;           // 마우스가 오른쪽을 넘어가도 보이도록 
            }
        
            mousePos.y = 8;     // y축 값 고정
            mousePos.z = 0;     // z축 값 고정

            transform.position = Vector3.Lerp(transform.position, mousePos, 0.1f);  // 마우스 따라오는 속도
        }
        
    }

    public void Drag(){
        isDrag = true;
        rigid.simulated = false;
    }
    public void Drop(){
        isDrag = false;
        rigid.simulated = true;
    }
}