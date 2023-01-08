using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Circle           lastCircle;
    public GameObject       circlePrefab;
    public Transform        circleGroup;


    void Awake()
    {
        Application.targetFrameRate = 144;   // 목표 프레임 제한    
    }
    void Start()
    {
        NextCircle();
    }

    Circle GetCircle(){        // Circle 오브젝트 생성
        GameObject instate = Instantiate(circlePrefab, circleGroup);
        Circle instateCircle  = instate.GetComponent<Circle>();
        return instateCircle;     
    }

    void NextCircle(){
        Circle newCircle = GetCircle();
        lastCircle = newCircle;
        //lastCircle.circleLevel = Random.Range(0, 8);    // 오브젝트 랜덤 레벨 선 결정
        lastCircle.gameObject.SetActive(true);          // 후 오브젝트 활성화

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
}
