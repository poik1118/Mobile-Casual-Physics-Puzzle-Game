using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    public int              circleLevel;
    public bool             isDrag;
    public bool             isMerge;

    public GameManager      gameManager;
    
    Rigidbody2D             rigid;
    CircleCollider2D        circleCollider;
    Animator                anime;
    
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
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

    void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Circle"){
            Circle other = collision.gameObject.GetComponent<Circle>();

            if(circleLevel == other.circleLevel && !isMerge && !other.isMerge && circleLevel < 7){
                // 나와 상대편 위치 가져오기
                float myX = transform.position.x;
                float myY = transform.position.y;

                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;
                
                // 내가 아래에 있을  때, 동일한 높이일 때 내가 오른쪽에 있을때
                if(myY < otherY ||  (myY == otherY && myX > otherX)){
                    // 상대방은 숨기기
                    other.Hide(transform.position);

                    // 자신 레벨업
                    LevelUp();
                }

            }

        }
    }

    public void Hide(Vector3 targetPos){
        isMerge = true;

        rigid.simulated = false;            // 물리효과 비활성

        circleCollider.enabled = false;     // 콜라이더 비활성화

        StartCoroutine(HideRoutine(targetPos));

    }
    IEnumerator HideRoutine(Vector3 targetPos){
        int frameCount = 0;

        while(frameCount < 20){
            frameCount++;
            transform.position = Vector3.Lerp(transform.position, targetPos, 1.0f); 

            yield return null;
        }

        isMerge = false;
        gameObject.SetActive(false);
        
    }

    void LevelUp(){
        isMerge = true;

        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());
    }
    IEnumerator LevelUpRoutine(){
        yield return new WaitForSeconds(0.1f);

        anime.SetInteger("Level", circleLevel + 1);     // 애니메이션

        yield return new WaitForSeconds(0.1f);
        circleLevel++;                                  // 실제 서클 레벨 증가

        gameManager.maxLevel = Mathf.Max(circleLevel, gameManager.maxLevel);

        isMerge = false;
    }
}