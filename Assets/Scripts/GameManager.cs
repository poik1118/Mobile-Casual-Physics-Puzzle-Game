using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Circle           lastCircle;

    public GameObject       circlePrefab;
    public Transform        circleGroup;
    public GameObject       effectPrefab;
    public Transform        effectGroup;

    public AudioSource      bgmPlayer;
    public AudioSource[]    sfxPlayer;
    public AudioClip[]      sfxClip;
    public enum sfx { LevelUp, Next, Attach, Button, GameOver };
    int sfxCursor;

    public int maxLevel;
    public int score;
    public bool isOver;

    void Awake()
    {
        Application.targetFrameRate = 144;   // 목표 프레임 제한    
    }
    void Start()
    {
        bgmPlayer.Play();
        NextCircle();
    }

    Circle GetCircle(){     
        // 이펙트 생성
        GameObject instateEffectObj = Instantiate(effectPrefab, effectGroup);
        ParticleSystem instateEffect  = instateEffectObj.GetComponent<ParticleSystem>();

        // Circle 오브젝트 생성
        GameObject instate = Instantiate(circlePrefab, circleGroup);
        Circle instateCircle  = instate.GetComponent<Circle>();
        instateCircle.particleEffect = instateEffect;

        return instateCircle;
    }

    void NextCircle(){
        if(isOver){
            return;     // 게임오버시 서클 호출 정지
        }

        Circle newCircle = GetCircle();
        lastCircle = newCircle;
        lastCircle.gameManager = this;
        lastCircle.circleLevel = Random.Range(0, maxLevel);   // 오브젝트 랜덤 레벨 선 결정
        lastCircle.gameObject.SetActive(true);                // 후 오브젝트 활성화

        SFXplay(sfx.Next);
        StartCoroutine(WaitNextCircle());
    }

    IEnumerator WaitNextCircle(){
        while(lastCircle != null){
            yield return null;

        }
        yield return new WaitForSeconds(1.5f);

        NextCircle();
    }

    public void TouchDown(){
        if(lastCircle == null){
            return;
        }
        lastCircle.Drag();
    }
    public void TouchUp(){
        if(lastCircle == null){
            return;
        }
        lastCircle.Drop();
        lastCircle = null;
    }

    public void GameOver(){
        if(isOver){
            return;
        }

        isOver = true;
        
        StartCoroutine(GameOverRoutine());
    }
    IEnumerator GameOverRoutine(){
        // 장면 안에 활성화 되어있는 모든 동글을 가져오기
        // circle타입의 모든 오브젝트를 배열로 가져온다
        Circle[] circles = GameObject.FindObjectsOfType<Circle>();  

        // 지우는 과정 중 합쳐짐 방지를 위해 물리효과 비활성
        for (int index = 0; index < circles.Length; index++){
            circles[index].rigid.simulated = false;
        }

        // 배열 목록을 하나씩 접근해서 지우기
        for (int index = 0; index < circles.Length; index++){
            circles[index].Hide(Vector3.up * 100);  // 게임 플레이중 나올수 없는 값 생성
            yield return new WaitForSeconds(0.1f);  // 순서대로 circle을 0.1초마다 지우기 
        }

        yield return new WaitForSeconds(1.0f);
        SFXplay(sfx.GameOver);
    }

    public void SFXplay(sfx type){
        switch(type){
            case sfx.LevelUp:
                sfxPlayer[sfxCursor].clip = sfxClip[Random.Range(0, 3)];
                break;

            case sfx.Next:
                sfxPlayer[sfxCursor].clip = sfxClip[3];
                break;

            case sfx.Attach:
                sfxPlayer[sfxCursor].clip = sfxClip[4];
                break;

            case sfx.Button:
                sfxPlayer[sfxCursor].clip = sfxClip[5];
                break;

            case sfx.GameOver:
                sfxPlayer[sfxCursor].clip = sfxClip[6];
                break;
        }

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }
}