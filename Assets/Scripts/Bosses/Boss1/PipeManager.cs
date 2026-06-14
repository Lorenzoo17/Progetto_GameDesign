using UnityEngine;
using System.Collections.Generic;


public class PipeWrap
{
    public bool isPipeLocked;
    public int pipeId;
    public Transform pipeTransform;
}

public class PipeGroups
{
    public List<PipeWrap> upPipe = new List<PipeWrap>();
    public List<PipeWrap> leftPipe = new List<PipeWrap>();
    public List<PipeWrap> rightPipe = new List<PipeWrap>();

    public int lastGroupUsed = 2;
    public List<int> lastIdUsedForgroup = new List<int> { -1, -1, -1 };
}

public class PipeManager : MonoBehaviour
{
    public static PipeManager Instance { get; private set; }

    [Header("Assegna i tubi dall'Inspector")]
    public List<Transform> tubi_up = new List<Transform>();
    public List<Transform> tubi_left = new List<Transform>();
    public List<Transform> tubi_right = new List<Transform>();

    [SerializeField] private PipeGroups groups = new PipeGroups();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        int i = 0;
        foreach (Transform pipe in tubi_up) { groups.upPipe.Add(new PipeWrap { isPipeLocked = false, pipeTransform = pipe, pipeId = i }); i++; } i = 0;
        foreach (Transform pipe in tubi_left) { groups.leftPipe.Add(new PipeWrap { isPipeLocked = false, pipeTransform = pipe , pipeId = i }); i++; } i = 0;
        foreach (Transform pipe in tubi_right) { groups.rightPipe.Add(new PipeWrap { isPipeLocked = false, pipeTransform = pipe , pipeId = i }); i++; }
    }

    public void LockRandomPipeInGroup(int groupId)
    {
        List<PipeWrap> targetList = null;

        switch (groupId)
        {
            case 0:
                targetList = groups.upPipe;
                
                break;
            case 1:
                targetList = groups.rightPipe;
                
                break;
            case 2:
                targetList = groups.leftPipe;
                
                break;
            default:
                Debug.LogWarning($"[SABOTAGGIO] ID Gruppo {groupId} non valido!");
                return;
        }

        
        List<PipeWrap> availablePipesInGroup = new List<PipeWrap>();
        foreach (PipeWrap wrap in targetList)
        {
            if (!wrap.isPipeLocked)
            {
                availablePipesInGroup.Add(wrap);
            }
        }

        
        if (availablePipesInGroup.Count > 2)
        {
            int randomIndex = Random.Range(0, availablePipesInGroup.Count);
            availablePipesInGroup[randomIndex].isPipeLocked = true;
            Debug.Log($"[SABOTAGGIO] TUBO CHIUSO! Gruppo: {groupId}, ID Tubo: {availablePipesInGroup[randomIndex].pipeId}");
            
        }
        else
        {
            Debug.Log($"[SABOTAGGIO] Tutti i tubi del gruppo erano già chiusi! 2 Devono essere liberi");
        }
    }


    public Transform GetRandomAvailablePipe()
    {
        List<PipeWrap> availablePipes = new List<PipeWrap>();

        switch (groups.lastGroupUsed)
        {
            case 0:
                foreach (PipeWrap wrap in groups.upPipe) if (!wrap.isPipeLocked) availablePipes.Add(wrap);
                break;
            case 1:
                foreach (PipeWrap wrap in groups.rightPipe) if (!wrap.isPipeLocked) availablePipes.Add(wrap);
                break;
            case 2:
                foreach (PipeWrap wrap in groups.leftPipe) if (!wrap.isPipeLocked) availablePipes.Add(wrap);
                break;
            default:
                Debug.LogWarning($"[PIPE MANAGER] ID Gruppo {groups.lastGroupUsed} non valido!");
                return null;
        }

        if (availablePipes.Count == 0)
        {
            Debug.LogWarning("TUTTI I TUBI SONO CHIUSI!");
            groups.lastGroupUsed--;
            if (groups.lastGroupUsed < 0) groups.lastGroupUsed = 2;
            return null;
        }

        List<PipeWrap> validPipes = new List<PipeWrap>();
        foreach (PipeWrap wrap in availablePipes)
        {
            if (wrap.pipeId != groups.lastIdUsedForgroup[groups.lastGroupUsed])
            {
                validPipes.Add(wrap);
            }
        }

        if (validPipes.Count == 0)
        {
            validPipes = availablePipes;
        }

        int randomIndex = Random.Range(0, validPipes.Count);
        PipeWrap selectedWrap = validPipes[randomIndex];

        groups.lastIdUsedForgroup[groups.lastGroupUsed] = selectedWrap.pipeId;

        groups.lastGroupUsed--;
        if (groups.lastGroupUsed < 0)
        {
            groups.lastGroupUsed = 2;
        }

        return selectedWrap.pipeTransform;
    }
}


