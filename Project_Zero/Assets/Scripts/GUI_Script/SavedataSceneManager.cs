using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SavedataSceneManager : MonoBehaviour
{
    public static string backPath = "Title";
    public static string workType = "New";

    public Button backButton;
    public GameObject warningMessage;
 
    public GameObject[] save = new GameObject[3];
    string[] dataPreview;

    public void NewGame(int i) // 게임 시작.
    {
        PlayerInfo.dataIndex = i;
        TurnManager.turn = 1;
        GoodsManager.goodsAr = 20000;
        GoodsManager.goodsStone = 0;
        GoodsManager.goodsConstFame = 10;
        GoodsManager.goodsStudent = 0;

        CreateProfessor.professorGenerateSeed = Random.Range(-2147483648, 2147483647);

        PlayerInfo.ProfessorList.Clear();
        PlayerInfo.studentGroups.Clear();

        SubjectTree.initSubjectsAndInfo();
        SubjectTree.initSubjectStates(new List<int>());
        SubjectTree.callOnlyOneTimeWhenGameStart();
        BeforeTurn.HaveToGenerateNewProfessors = true;
        System.Random randomGenerator = new System.Random();
        CreateProfessor.professorGenerateSeed = randomGenerator.Next(-2147483648, 2147483647);
        for (int roll = 0; roll < 3; roll++)
        {
            ProfessorSystem.Professor tmp = CreateProfessor.CreateStandardProfessor(roll, roll / 2);
            PlayerInfo.ProfessorList.Add(tmp);
            if (tmp.ProfessorGetName() == "대 시후")
                AchievementManager.Achieve(0);
        }
        PlayerInfo.ProfessorPickedStatus = new bool[3] { true, true, true };
        AchievementManager.Achieve(1);
        /*
         
        초기화할 내용 추가. 

         */


        SceneManager.LoadScene("Intro");
    }

    public void NewGameWarning(int i)
    {
        warningMessage.SetActive(true);
        warningMessage.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate { NewGame(i); });
        warningMessage.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(CloseWarning);
    }

    public void CloseWarning()
    {
        warningMessage.SetActive(false);
    }

    public void LoadGame(int i)
    {
        PlayerInfo.dataIndex = i;
        SaveManager.LoadProcess();
        LoadingSceneManager.LoadScene("Main");
    }

    public void ErrorMessage()
    {

    }

    public void BackTracking()
    {
        SceneManager.LoadScene(backPath);
    }

    private void Awake()
    {
        backButton.onClick.AddListener(BackTracking);
        warningMessage.SetActive(false);
        if(workType == "New")
        {
            for (int i = 0; i < 3; i++)
            {
                int j = i;
                if (PlayerPrefs.HasKey("save" + (i + 1).ToString()))
                {
                    save[j].GetComponent<Button>().onClick.AddListener(delegate { NewGameWarning(j+1); });
                }
                else
                {
                    save[j].GetComponent<Button>().onClick.AddListener(delegate { NewGame(j+1); });
                }
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                int j = i;
                if (!PlayerPrefs.HasKey("save" + (i + 1).ToString()))
                {
                    save[j].GetComponent<Button>().onClick.AddListener(ErrorMessage);
                }
                else
                {
                    save[j].GetComponent<Button>().onClick.AddListener(delegate { LoadGame(j + 1); });
                }
            }
        }

        for (int i = 1; i <= 3;i++)
        {
            GameObject tmp = save[i - 1];
            if (PlayerPrefs.HasKey($"save{i}"))
            {
                dataPreview = SaveManager.PlayerDataPreview(i);
                tmp.transform.GetChild(0).gameObject.SetActive(false);
                tmp.transform.GetChild(1).GetComponent<Text>().text
                    = dataPreview[0] + "의 " + dataPreview[1] + "아카데미\r\n" +
                    "Turn : " + dataPreview[2] + "\r\n아르 : " + dataPreview[3] +
                    "\r\n명성 : " + dataPreview[4];
                tmp.transform.GetChild(1).gameObject.SetActive(true);
            }
            else
            {
                tmp.transform.GetChild(0).gameObject.SetActive(true);
                tmp.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }

}
