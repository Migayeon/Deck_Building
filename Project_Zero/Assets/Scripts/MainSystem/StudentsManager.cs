using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class StudentGroup
{
    //SubjectManager.SubjectTree s = new SubjectManager.SubjectTree();
    public StudentGroup(int div , int num , int c)
    {
        division = div;
        number = num;
        age = 0;
        cost = c;
        curriculum = null; // 함수 연결
        stat = new List<int> { 0, 0, 0, 0, 0 };

        int lim = 0;
        int statSum = Random.Range(300, 400);
        int lastSum = statSum;

        for (int i = 0; i < 5; i++)
        {
            stat[i] = 10 + statSum * (Random.Range(lim, lim + 20) - lim) / 100;
            lastSum -= stat[i];
        }
        for (int i = 0; i < lastSum; i++)
        {
            if (stat[i % 5] >= 100)
            {
                lastSum++;
                continue;
            }
            stat[i % 5]++;
        }
    }
    public StudentGroup() { }

    private int division; // 분반
    private int number; // 학생 수
    private int age; // 공부 기간
    private int cost; // 학원비
    private List<int> curriculum; // 커리큘럼
    private List<int> stat; // 스탯

    public int GetDivision(){ return division; }
    public int GetNumber() { return number; }
    public int GetAge() { return age; }
    public int GetCost() { return cost; }
    public List<int> GetCurriculum() { return curriculum; }
    public List<int> GetStat() { return stat; }

    public void SetCurriCulum(List<int> newCurri)
    {
        curriculum = newCurri;
        return;
    }

    public void RandomStatUp()
    {
        for (int i = 0; i < 10; i++)
        {
            stat[Random.Range(0, 4)]++;
        }
    }

    public void CurriculumSequence()
    {
        Subject subject = SubjectTree.getSubject(curriculum[age]);
        List<int> enforceType = subject.enforceContents;
        for (int i = 0; i < enforceType.Count; i++)
        {
            stat[i] += enforceType[i];
        }
        age++;
        if (age == 8)
        {
            SelectTest();

            return;
        }
    }

    public void SelectTest()
    {
        /*
         
        시험 선택 함수. 

         */
        foreach (int i in stat)
        {
            Debug.Log(i);
        }
    }
    
}

public class StudentsManager : MonoBehaviour
{
   
    private void Awake()
    {
        /*
         
        구현할 것
        1. 활성화된 과목 비활성화 된 과목 분리
        2. 스크립트 오브젝트에 부여
        3.  

         */
    }
}
