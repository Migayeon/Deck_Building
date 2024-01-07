using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public class SubjectManager : MonoBehaviour
{
    public void Awake()
    {
        SubjectTree.initSubjectsAndInfo();
    }
}
public static class SubjectTree
{
    /* << Attributes >>
     *  - List<Subject> subjects : 과목들을 저장합니다.
     *  - Dictionary<string, int> subjectsInfo : 전체적인 정보를 저장합니다.
     *  - List<State> subjectState : 과목의 상태를 저장합니다. (닫힘, 열림, 준비됨)
     *  
     *  << Funtions >>
     *  - void init() : 정보를 json파일로부터 불러옵니다.
     *  
     *  - bool isSubjectOpen(int id) : 강좌가 개설되어 있는 지 확인합니다.
     *  - void openSubject(int id) : 강좌를 개설합니다.
     *  - void closeSubject(int id) : 강좌를 폐쇄합니다.
     *  - void changeSubjectState(int id) : 강좌 상태를 변경합니다.
     *  
     *  - List<int> getEnforceType(int id) : return EnforceType
     *  
     *  - bool isVaildCurriculum(List<int> subjectsId) : 해당 커리큘럼이 유효한 지 판단하여 반환합니다.
     */
    public enum State
    {
        Closed,
        Open,
        ReadyToOpen
    }
    private const int DONT_HAVE_GROUP = 0;
    private const int NORMAL_ROOT = 0;
    private static string INFO_PATH = Path.Combine(Application.dataPath, "Resources/Subjects/subjectsInfo.json");
    public static List<bool> professors = new List<bool>();
    public static List<Subject> subjects = new List<Subject>();
    public static SubjectInfo subjectsInfo;
    public static List<State> subjectState = new List<State>();
    public static List<int> subjectStateNeedCnt = new List<int>();

    public static List<int> studentInSubjectCnt = new List<int>();
    public static List<int> professorInSubjectCnt = new List<int>();
    public static Dictionary<long, List<bool>> professorsLecture = new Dictionary<long, List<bool>>();
    public static int subjectsCount = 0;

    public static void initSubjectsAndInfo()
    {
        subjects = new List<Subject>();
        subjectStateNeedCnt = new List<int>();
        string loadJson = File.ReadAllText(INFO_PATH);
        subjectsInfo = JsonUtility.FromJson<SubjectInfo>(loadJson);
        subjectsCount = subjectsInfo.count;
        for (int i = 0; i < subjectsCount; i++)
        {
            string subjectPath = Path.Combine(Application.dataPath, "Resources/Subjects/" + i.ToString() + ".json");
            loadJson = File.ReadAllText(subjectPath);
            subjects.Add(JsonUtility.FromJson<Subject>(loadJson));
            subjectStateNeedCnt.Add(subjects[i].needCount);
        }
        studentInSubjectCnt = new List<int>();
        professorInSubjectCnt = new List<int>();
        for (int i = 0; i < subjectsCount; i++)
        {
            studentInSubjectCnt.Add(0);
            professorInSubjectCnt.Add(0);
        }
    }

    public static bool isSubjectOpen(int id)
    {
        return subjectState[id] == State.Open;
    }
    public static void openSubject(int id)
    {
        if (subjects[id].subjectGroupId != DONT_HAVE_GROUP)
        {
            for (int i = 0; i < subjectsCount; i++)
            {
                if (subjects[i].subjectGroupId == subjects[id].subjectGroupId)
                    subjectState[i] = State.Closed;
            }
        }
        subjectState[id] = State.Open;
        foreach (int nextNode in subjects[id].nextSubjects)
        {
            if (--subjectStateNeedCnt[nextNode] == 0)
                subjectState[nextNode] = State.ReadyToOpen;
        }
    }
    public static void closeSubject(int id)
    {
        if (subjects[id].subjectGroupId != DONT_HAVE_GROUP)
        {
            for (int i = 0; i < subjectsCount; i++)
            {
                if (subjects[i].subjectGroupId == subjects[id].subjectGroupId)
                    subjectState[i] = State.ReadyToOpen;
            }
        }
        subjectState[id] = State.ReadyToOpen;
        foreach (int nextNode in subjects[id].nextSubjects)
        {
            subjectStateNeedCnt[nextNode]++;
            subjectState[nextNode] = State.Closed;
        }
    }

    public static Subject getSubject(int id)
    {
        return subjects[id];
    }

    private static List<int> newCntList()
    {
        List<int> rst = new List<int>();
        for (int i = 0; i < subjectsCount; i++)
            rst.Add(subjects[i].needCount);
        return rst;
    }
    private static List<bool> flattenList(List<int> idList)
    {
        List<bool> rst = new List<bool>();
        for (int i = 0; i < subjectsCount; i++)
            rst.Add(false);
        for (int i = 0; i < idList.Count; i++)
            rst[idList[i]] = true;
        return rst;
    }
    public static void initSubjectStates(List<int> openedSubjectsId)
    {
        subjectState = new List<State>();
        subjectStateNeedCnt = new List<int>();
        for (int i = 0; i < subjectsCount; i++)
            subjectState.Add(State.Closed);
        for (int i = 0; i < openedSubjectsId.Count; i++)
        {
            subjectState[openedSubjectsId[i]] = State.Open;
            subjectStateNeedCnt.Add(subjects[i].needCount);
        }
        for (int i = 0; i < subjectsCount; i++)
            subjectState.Add(State.Closed);
        List<int> cntList = newCntList();
        List<bool> flatSearchList = flattenList(new List<int>());
        List<bool> isSameGroup = new List<bool>();
        for (int i = 0; i < subjectsInfo.groupCount; i++)
            isSameGroup.Add(false);
        Queue<int> searchQ = new Queue<int>();
        for (int i = 0; i < subjectsCount; i++)
        {
            if (subjects[i].needCount == 0)
            {
                if (subjectState[i] == State.Open)
                {
                    searchQ.Enqueue(i);
                    flatSearchList[i] = true;
                }
                else
                {
                    subjectState[i] = State.ReadyToOpen;
                }
            }
        }
        while (searchQ.Count > 0)
        {
            int nowNodeId = searchQ.Dequeue();
            List<int> next = subjects[nowNodeId].nextSubjects;
            for (int i = 0; i < next.Count; i++)
            {
                int index = next[i];
                subjectState[index] = State.ReadyToOpen;
                if (--cntList[index] == 0 && subjectState[i] == State.Open)
                {
                    searchQ.Enqueue(index);
                    flatSearchList[i] = true;
                }
            }
            if (subjects[nowNodeId].subjectGroupId != DONT_HAVE_GROUP)
            {
                if (!isSameGroup[subjects[nowNodeId].subjectGroupId] && subjectState[nowNodeId] == State.Open)
                    isSameGroup[subjects[nowNodeId].subjectGroupId] = true;
                else
                    subjectState[nowNodeId] = State.Closed;
            }
        }
    }

    public static bool isVaildCurriculum(List<int> subjectsId)
    {
        List<int> cntList = newCntList();
        List<bool> flatIdList = flattenList(subjectsId);
        List<bool> flatSearchList = flattenList(new List<int>());
        Queue<int> searchQ = new Queue<int>();
        for (int i = 0; i < subjectsCount; i++)
        {
            if (subjects[i].needCount == 0 && flatIdList[i])
            {
                searchQ.Enqueue(i);
                flatSearchList[i] = true;
            }
        }
        while (searchQ.Count > 0)
        {
            int nowNodeId = searchQ.Dequeue();
            List<int> next = subjects[nowNodeId].nextSubjects;
            for (int i = 0; i < next.Count; i++)
            {
                int index = next[i];
                if (--cntList[index] == 0 && flatIdList[index])
                {
                    searchQ.Enqueue(index);
                    flatSearchList[index] = true;
                }
            }
        }

        List<bool> isSameGroup = new List<bool>();
        for (int i = 0; i < subjectsCount; i++)
            isSameGroup.Add(false);
        foreach (int id in subjectsId)
        {
            if (subjects[id].subjectGroupId != DONT_HAVE_GROUP)
            {
                if (!isSameGroup[subjects[id].subjectGroupId])
                    isSameGroup[subjects[id].subjectGroupId] = true;
                else
                    return false;
            }
            if (!flatSearchList[id])
                return false;
        }
        return true;
    }

    public static void addSubjectStudentCnt(List<int> curriculum)
    {
        foreach(int subjectId in curriculum)
            studentInSubjectCnt[subjectId]++;
    }

    public static void removeSubjectStudentCnt(List<int> curriculum)
    {
        foreach (int subjectId in curriculum)
            studentInSubjectCnt[subjectId]--;
    }

    public static void addProfessorAt(long professorId, int subjectId)
    {
        if (professorsLecture.ContainsKey(professorId))
        {
            professorInSubjectCnt[subjectId]++;
            professorsLecture[professorId][subjectId] = true;
        }
        else
        {
            professorsLecture[professorId] = new List<bool>();
            for (int i = 0; i < subjectsCount; i++)
                professorsLecture[professorId].Add(i == subjectId);
        }
    }
    public static void removeProfessor(long professorId)
    {
        professorsLecture.Remove(professorId);
        for (int subjectId = 0; subjectId < subjectsCount; subjectId++)
        {
            if (professorsLecture[professorId][subjectId])
                professorInSubjectCnt[subjectId]--;
        }
    }
    public static void removeProfessorAt(long professorId, int subjectId)
    {
        if (professorsLecture.ContainsKey(professorId))
        {
            professorInSubjectCnt[subjectId]--;
            professorsLecture[professorId][subjectId] = false;
            if (!professorsLecture[professorId].Contains(true))
                professorsLecture.Remove(professorId);
        }
    }
    public static bool canRemoveProfessor(long professorId, List<int> curriculum)
    {
        for (int subjectId = 0; subjectId < subjectsCount; subjectId++)
        {
            if (!curriculum.Contains(subjectId)) continue;
            if (professorsLecture[professorId][subjectId] && professorInSubjectCnt[subjectId] == 1)
                return false;
        }
        return true;
    }

    public static bool canFreeProfessorInSubject(long professorId, int querySubjectId)
    {
        if (studentInSubjectCnt[querySubjectId] > 0 && professorsLecture[professorId][querySubjectId] && professorInSubjectCnt[querySubjectId] == 1)
            return false;
        return true;
    }

    public class SaveData
    {
        public int professorCnt = 0;
        public List<long> professorsId = new List<long>();
        public List<string> lecturesId = new List<string>();
        public List<int> subjectStates = new List<int>();
    }

    public static string save()
    {
        SaveData rst = new SaveData();
        rst.professorCnt = professorsLecture.Count;
        rst.professorsId = professorsLecture.Keys.ToArray().ToList();
        foreach (long i in professorsLecture.Keys)
        {
            rst.lecturesId.Add("");
            for (int j = 0; j < subjectsCount; j += 3)
                rst.lecturesId[(int) i] += ((Convert.ToInt32(professorsLecture[i][j]) + Convert.ToInt32(professorsLecture[i][j + 1]) * 2 + Convert.ToInt32(professorsLecture[i][j + 2]) * 4)).ToString();
            int startIdx = (subjectsCount / 3) * 3;
            int mul = 1;
            int sum = 0;
            for (int j = startIdx; j < subjectsCount; j++)
            {
                sum += Convert.ToInt32(professorsLecture[i][j]) * mul;
                mul *= 2;
            }
            rst.lecturesId[(int) i] += sum.ToString();
        }
        rst.subjectStates = subjectState.Select(s => (int) s).ToList();
        return JsonUtility.ToJson(rst);
    }

    //아직 구현 안 됨
    public static void load(string jsonContents)
    {
        initSubjectsAndInfo();
        SaveData data = JsonUtility.FromJson<SaveData>(jsonContents);
        for (int i = 0; i < data.professorCnt; i++)
        {
            for (int j = 0; j < data.lecturesId.Count; j++)
            {

            }
        }
        initSubjectStates(data.subjectStates);
    }
}

[System.Serializable]
public class SubjectInfo
{
    public int count;
    public string[] enforceTypeName;
    public int groupCount;
    public SubjectInfo(int Count, string[] EnforceTypeName, int GroupCount)
    {
        count = Count;
        enforceTypeName = EnforceTypeName;
        groupCount = GroupCount;
    }
}

[System.Serializable]
public class Subject
{
    public const int MAGIC_THEORY = 0, MANA_TELE = 1, HAND_CRAFT = 2, ELEMENT = 3, CHANT_MAGIC = 4;
    public int id;
    public int tier;
    public string name;
    public List<int> enforceContents;
    public List<int> nextSubjects;
    public int subjectGroupId;
    public int root;
    public int needCount;
    
    public Subject(int Id, int Tier, string Name, List<int> EnforceContents, List<int> NextSubjects, int SubjectGroupId, int Root, int NeedCount)
    {
        id = Id;
        tier = Tier;
        name = Name;
        enforceContents = EnforceContents;
        nextSubjects = NextSubjects;
        subjectGroupId = SubjectGroupId;
        root = Root;
        needCount = NeedCount;
    }
}