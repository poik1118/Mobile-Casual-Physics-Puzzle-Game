using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header(" -----[ Core ]------ ")]
    public int maxLevel;
    public int score;
    public bool isOver;

    [Header(" -----[ Object Pool ]----- ")]
    public Circle           lastCircle;

    public GameObject       circlePrefab;
    public Transform        circleGroup;
    public List<Circle>     circlePool;

    public GameObject       effectPrefab;
    public Transform        effectGroup;
    public List<ParticleSystem> effectPool;

    [Range(1, 30)]
    public int              poolSize;
    public int              poolCursor;

    [Header(" -----[ Audio ]----- ")]
    public AudioSource      bgmPlayer;
    public AudioSource[]    sfxPlayer;
    public AudioClip[]      sfxClip;
    public enum sfx { LevelUp, Next, Attach, Button, GameOver };
    int sfxCursor;

    [Header(" -----[ UI ]----- ")]
    public GameObject       gameStartGroup;
    public GameObject       gameOverGroup;
    public Text             scoreText;
    public Text             maxScoreText;
    public Text             resultScoreText;

    [Header(" -----[ Etc ]----- ")]
    public GameObject       mapGroup;

    void Awake()
    {
        Application.targetFrameRate = 144;          // 목표 프레임 제한    

        circlePool = new List<Circle>();            // circle 리스트 초기화
        effectPool = new List<ParticleSystem>();    // effect 리스트 초기화

        for( int index = 0; index < poolSize; index++ ){
            MakeCircle();
        }

        if(!PlayerPrefs.HasKey("MaxScore")){
            PlayerPrefs.SetInt("MaxScore", 0);
        }
        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }

    public void GameStart()
    {
        gameStartGroup.SetActive(false);
        mapGroup.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        

        bgmPlayer.Play();
        SFXplay(sfx.Button);

        Invoke("NextCircle", 1.0f);
    }

    Circle MakeCircle(){
        // 이펙트 생성
        GameObject instateEffectObj = Instantiate(effectPrefab, effectGroup);
        instateEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instateEffect  = instateEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instateEffect);

        // Circle 오브젝트 생성
        GameObject instateCircleObj = Instantiate(circlePrefab, circleGroup);
        instateCircleObj.name = "Circle " + circlePool.Count;
        Circle instateCircle  = instateCircleObj.GetComponent<Circle>();
        instateCircle.gameManager = this;
        instateCircle.particleEffect = instateEffect;
        circlePool.Add(instateCircle);

        return instateCircle;
    }

    Circle GetCircle(){     
        for( int index = 0; index < circlePool.Count; index++){
            poolCursor = (poolCursor + 1) % circlePool.Count;

            if(!circlePool[poolCursor].gameObject.activeSelf){
                return circlePool[poolCursor];
            }
        }

        return MakeCircle();
    }

    void NextCircle(){
        if(isOver){
            return;         // 게임오버시 서클 호출 정지
        }

        lastCircle = GetCircle();
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

        yield return new WaitForSeconds(0.5f);

        // 최고 점수 갱신
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        // game over UI 표시
        resultScoreText.text = "최종 점수 : " + scoreText.text;

        gameOverGroup.SetActive(true);
        mapGroup.SetActive(false);
        scoreText.gameObject.SetActive(false);
        maxScoreText.gameObject.SetActive(false);

        bgmPlayer.Stop();
        SFXplay(sfx.GameOver);
    }

    public void Reset()
    {
        SFXplay(sfx.Button);
        StartCoroutine(RestartRoutine());
    }
    IEnumerator RestartRoutine(){
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Main");
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

    void Update(){
        if(Input.GetButtonDown("Cancel")){
            Application.Quit();
        }
    }

    void LateUpdate()
    {
        scoreText.text = score.ToString();
    }
}