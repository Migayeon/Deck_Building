using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AfterTurn : MonoBehaviour
{
    public Text turnText;           // 턴
    public Text studentRevenue;     // 학원비 수입
    public Text magicStoneRevenue;  // 마정석 판매 수입
    public Text professorRevenue;   // 교수 수입
    public Text professorCost;      // 교수 봉급 지급
    public Text academyCost;        // 학원 증축비
    public Text marketingCost;      // 마케팅 사용 금액
    public Text magicStoneCost;     // 마정석 구입 비용
    public Text totalResult;        // 총 결산

    [SerializeField]
    GameObject pageOne;
    [SerializeField]
    GameObject pageTwo;

    public List<StudentGroup> toBeGraduated;

    public Button nextTurn;
    public void AfterTurnToBeforeTurn()
    {
        curriculumModManager.loadCurriculumSceneWithMod(0);
    }

    int student_Rev;
    public static int magic_Rev = 0;
    int professor_Rev;
    int professor_Cost;
    public static int academy_Cost = 0;
    public static int marketing_Cost = 0;
    public static int magic_Cost = 0;
    int total_Result;
    bool isGraduated;

    void Awake()
    {
        isGraduated = false;
        pageOne.SetActive(true);
        pageTwo.SetActive(false);
        toBeGraduated = new List<StudentGroup>();
        // 각종 재화 변동 필요함...

        // 학생들 커리큘럼 진행
        student_Rev = GoodsManager.goodsAr;
        nextTurn.onClick.RemoveAllListeners();
        StudentsManager.UpdateProfessorInfoInSubject();
        foreach (StudentGroup[] period in PlayerInfo.studentGroups)
        {
            foreach (StudentGroup group in period)
            {
                bool check = group.CurriculumSequence();
                if (check == false)
                {
                    if(group.GetNumber() > 0)
                    {
                        toBeGraduated.Add(group);
                    }
                    isGraduated = true;
                }
            }
        }
        if (isGraduated == true)
        {
            StudentGroup[] graduateGroup = PlayerInfo.studentGroups[0];

            int numSum = graduateGroup[0].GetNumber() + graduateGroup[1].GetNumber() + graduateGroup[2].GetNumber();
            int costSum = graduateGroup[0].GetCost() * 8 * numSum;

            string graduationData =
                graduateGroup[0].GetPeriod().ToString() + "/" +
                numSum.ToString() + "/" +
                graduateGroup[0].GetCost().ToString() + "/" +
                costSum.ToString();
            PlayerInfo.graduateList.Add(graduationData);

            PlayerInfo.studentGroups.RemoveAt(0);
        }
        student_Rev = GoodsManager.goodsAr - student_Rev;

        // 커리큘럼 마친 학생들 시험 보게 하기
        if (toBeGraduated!= null)
        {
            foreach (StudentGroup grd in toBeGraduated)
            {
                Debug.Log(grd.GetExam());
                int possibility = TestCheckManager.CheckPossiblity(grd, grd.GetExam());
                int passed = 0;
                int passedStudentCnt = 0;
                for (int i = 0; i < grd.GetNumber(); i++)   // 각 학생별로 합격 여부 계산
                {
                    int num = Random.Range(1, 100);
                    if (num > possibility) continue;
                    else
                    {
                        passedStudentCnt += grd.GetNumber();
                        passed++;
                    }
                }
                if (grd.GetExam() < 6) PlayerInfo.nineSuccess += passed;
                else if (grd.GetExam() < 12) PlayerInfo.sevenSuccess += passed;
                else if (grd.GetExam() < 15) PlayerInfo.fiveSuccess += passed;
                else Debug.Log("Error In Test");

                grd.SetPassedNum(passed);
                Debug.Log($"{grd.GetPeriod()}기 {grd.GetDivision()}분반 합격자 수 " +
                    $"{grd.GetPassedNum()}/{grd.GetNumber()}");
                if (grd.GetPassedNum() == 3)
                    AchievementManager.Achieve(5);
                if (grd.GetPassedNum() == 0)
                    AchievementManager.Achieve(14);
                if (grd.GetExam() < 6 && passedStudentCnt > 0)
                {
                    int achieveCode = 9;
                    AchievementManager.CreateLocalStat(achieveCode);
                    AchievementManager.localStat[achieveCode] += passedStudentCnt;
                    if (passedStudentCnt >= 1000)
                        AchievementManager.Achieve(achieveCode);
                }
            }
        }

        // 마장석 구매 / 판매시 정산 완료. ( StockManager.cs 참고 )


        // 교수 출장으로 돈 벌어오는 시스템 구현 바람... To 시후
        professor_Rev = 0;


        // 교수 봉급 지급
        professor_Cost = -GoodsManager.goodsAr;
        foreach(ProfessorSystem.Professor professor in PlayerInfo.ProfessorList)
        {
            professor.ProfessorSetDefaultSalary();
            GoodsManager.goodsAr -= professor.ProfessorGetSalary();
        }
        professor_Cost += GoodsManager.goodsAr;

        // 건물 증축시 비용 적용 완료... ( ClassEx.cs 참고 )

        // 마케팅 비용 정산하는 것 구현 바람... To 동엽 ( ClassEx.cs에 구현 완료 )
        //marketing_Cost = 0;

        // 총 결산 진행.
        total_Result = student_Rev + professor_Rev + magic_Rev +
            professor_Cost + marketing_Cost + magic_Cost + academy_Cost;

        // 정산창에 텍스트 적용
        turnText.text = TurnManager.turn.ToString();
        studentRevenue.text = string.Format("{0:N0}",student_Rev);
        professorRevenue.text = string.Format("{0:N0}",professor_Rev);
        magicStoneRevenue.text = string.Format("{0:N0}", magic_Rev);
        professorCost.text = string.Format("{0:N0}", professor_Cost);
        marketingCost.text = string.Format("{0:N0}", marketing_Cost);
        magicStoneCost.text = string.Format("{0:N0}", magic_Cost);
        academyCost.text = string.Format("{0:N0}", academy_Cost);
        totalResult.text = string.Format("{0:N0}", total_Result);
        if(total_Result > 0)
        {
            totalResult.color = new Color(32/255,131/255,32/255);
        }
        else
        {
            totalResult.color = Color.red;
        }

        // 학생 졸업 결산창
        if (toBeGraduated.Count <= 0)
        {
            pageTwo.transform.GetChild(1).gameObject.SetActive(true);
            pageTwo.transform.GetChild(2).gameObject.SetActive(false);
        }
        else
        {
            pageTwo.transform.GetChild(1).gameObject.SetActive(false);
            pageTwo.transform.GetChild(2).gameObject.SetActive(true);
            int count = 0;
            foreach (StudentGroup group in toBeGraduated)
            {
                pageTwo.transform.GetChild(2).GetChild(count).GetChild(0).
                    GetComponent<TextMeshProUGUI>().text =
                    $"{group.GetPeriod()}기 {group.GetDivision()}반";
                pageTwo.transform.GetChild(2).GetChild(count).GetChild(1).
                    GetComponent<TextMeshProUGUI>().text =
                    $"{TestCheckManager.infoList[group.GetExam()].testname}";
                pageTwo.transform.GetChild(2).GetChild(count).GetChild(2).
                    GetComponent<TextMeshProUGUI>().text =
                    $"{group.GetPassedNum()} / {group.GetNumber()}";
                count++;
            }
            for(int i = count; i < 3; i++)
            {
                Debug.Log(i);
                pageTwo.transform.GetChild(2).GetChild(i).GetChild(0).
                    GetComponent<TextMeshProUGUI>().text =
                    $"";
                pageTwo.transform.GetChild(2).GetChild(i).GetChild(1).
                    GetComponent<TextMeshProUGUI>().text =
                    $"해당 분반에\r\n학생이 없습니다.";
                pageTwo.transform.GetChild(2).GetChild(i).GetChild(2).
                    GetComponent<TextMeshProUGUI>().text =
                    $"";
            }
        }
        // 정산용 변수 초기화
        magic_Rev = 0;
        academy_Cost = 0;
        magic_Cost = 0;
        total_Result = 0;
        marketing_Cost = 0;

        // 졸업생 리스트에 학생 그룹 추가 + 명성 갱신
        foreach (StudentGroup grd in toBeGraduated)
        {
            PlayerInfo.graduatedGroups.Add(grd);
        }
        GoodsManager.CalculateEndedFame();
        // 돈 없으면 울면서 파산함
        Text nextTurnText = nextTurn.transform.GetChild(0).GetComponent<Text>();
        if (GoodsManager.goodsAr >= 0)
        {
            nextTurnText.text = "다음 턴으로";
            nextTurnText.color = new Color32(50, 50, 50, 255);
            nextTurn.GetComponent<Image>().color = Color.white;
            nextTurn.onClick.AddListener(
                delegate
                {
                    curriculumModManager.loadCurriculumSceneWithMod(0);
                }
            );
        }
        else
        {
            nextTurnText.text = "파산하기";
            nextTurnText.color = new Color32(255, 50, 50, 255);
            nextTurn.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);
            nextTurn.onClick.AddListener(
                delegate
                {
                    LoadingSceneManager.LoadScene("Bankrupt");
                }
            );
        }
        foreach (StudentGroup group in toBeGraduated)
        {
            group.GetExam();
        }
        // BeforeTurn 불러오기, 1턴 추가
        TurnManager.turn++;
        //SceneManager.LoadScene("Curriculum");

    }
}
