using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InputManager;

public class GameManager : SingletonComponent<GameManager>
{
    private const float moveSpeed = 10000f;

    #region Fields

    public bool IsOver;
    private readonly List<Slot> freeSlots = new List<Slot>();
    private List<DataManager.CellData> tempCellList;

    #endregion Fields

    #region Properties

    public Coroutine Mergeing { get; set; }

    public int TotalScore
    {
        get { return DataManager.Instance.CurrentScore; }
        set { DataManager.Instance.CurrentScore = value; }
    }

    public bool IsBuys
    {
        get
        {
            if (IsOver) return true;
            if (Mergeing != null) return true;
            return false;
        }
    }

    #endregion Properties

    #region Unity API

    private void Start()
    {
        Invoke(nameof(Initialize), 0.25f);
    }

    private void OnEnable()
    {
        MoveHandler += OnMoveDirChanged;
    }

    private void OnDisable()
    {
        MoveHandler -= OnMoveDirChanged;
    }

    #endregion Unity API

    #region Init

    private void Initialize()
    {
        tempCellList = DataManager.Instance.StoredCellList;
        if (tempCellList.Count > 0)
        {
            RestoreGame();
        }
        else
        {
            TotalScore = 0;
            GenerateRandomCell();
        }
        CanvasManager.Instance.RefreshScore(TotalScore);
    }

    #endregion Init

    #region Event

    private void OnMoveDirChanged(object sender, MoveArgs moveArgs)
    {
        if (IsBuys) return;
        Mergeing = StartCoroutine(GameHandler(moveArgs.MoveDir));
    }

    #endregion Event

    #region Coroutines

    private bool CellMoved;

    private IEnumerator GameHandler(MoveDirection moveDir)
    {
        CellMoved = false;
        ResetCells();
        yield return MoveDirHandler(moveDir);

        if (CellMoved)
        {
            GenerateRandomCell();
            if (!AnyCanMove())
            {
                GameOver();
            }
            else
            {
                StoreGame();
            }
        }
        Mergeing = null;
    }

    private IEnumerator MoveDirHandler(MoveDirection moveDir)
    {
        int additionScore = 0;
        var slots = CanvasManager.Instance.Slots;
        bool moving;
        switch (moveDir)
        {
            case MoveDirection.Up:
                {
                    while (true)
                    {
                        moving = false;
                        for (int i = 0; i < 4; i++)
                        {
                            for (int j = 1; j < 4; j++)
                            {
                                var current = slots[j, i];
                                if (current.MyCell != null)
                                {
                                    var prev = slots[j - 1, i];
                                    if (prev.MyCell == null || prev.MyCell.IsMoving)
                                    {
                                        if (!MoveToTarget(current, prev))
                                        {
                                            moving = true;
                                        }
                                        else
                                        {
                                            //为了多运行一帧，继续寻找前面的空白位置
                                            if (current.MyCell.IsMoving)
                                            {
                                                current.MyCell.IsMoving = false;
                                                moving = true;
                                            }
                                            prev.AttachCell(current);
                                        }
                                    }
                                    else
                                    {
                                        if (!prev.MyCell.Merged && !current.MyCell.Merged && current.MyCell.Score == prev.MyCell.Score)
                                        {
                                            if (!MoveToTarget(current, prev))
                                            {
                                                moving = true;
                                            }
                                            else
                                            {
                                                MergeToTarget(current, prev);
                                                additionScore += prev.MyCell.Score;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!moving)
                        {
                            break;
                        }
                        yield return null;
                    }
                    break;
                }

            case MoveDirection.Down:
                {
                    while (true)
                    {
                        moving = false;
                        for (int i = 0; i < 4; i++)
                        {
                            for (int j = 2; j >= 0; j--)
                            {
                                var current = slots[j, i];
                                if (current.MyCell != null)
                                {
                                    var next = slots[j + 1, i];
                                    if (next.MyCell == null || next.MyCell.IsMoving)
                                    {
                                        if (!MoveToTarget(current, next))
                                        {
                                            moving = true;
                                        }
                                        else
                                        {
                                            if (current.MyCell.IsMoving)
                                            {
                                                current.MyCell.IsMoving = false;
                                                moving = true;
                                            }
                                            next.AttachCell(current);
                                        }
                                    }
                                    else
                                    {
                                        if (!next.MyCell.Merged && !current.MyCell.Merged && current.MyCell.Score == next.MyCell.Score)
                                        {
                                            if (!MoveToTarget(current, next))
                                            {
                                                moving = true;
                                            }
                                            else
                                            {
                                                MergeToTarget(current, next);
                                                additionScore += next.MyCell.Score;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!moving)
                        {
                            break;
                        }
                        yield return null;
                    }
                    break;
                }

            case MoveDirection.Left:
                {
                    while (true)
                    {
                        moving = false;
                        for (int i = 0; i < 4; i++)
                        {
                            for (int j = 1; j < 4; j++)
                            {
                                var current = slots[i, j];
                                if (current.MyCell != null)
                                {
                                    var prev = slots[i, j - 1];
                                    if (prev.MyCell == null || prev.MyCell.IsMoving)
                                    {
                                        if (!MoveToTarget(current, prev))
                                        {
                                            moving = true;
                                        }
                                        else
                                        {
                                            if (current.MyCell.IsMoving)
                                            {
                                                current.MyCell.IsMoving = false;
                                                moving = true;
                                            }
                                            prev.AttachCell(current);
                                        }
                                    }
                                    else
                                    {
                                        if (!prev.MyCell.Merged && !current.MyCell.Merged && current.MyCell.Score == prev.MyCell.Score)
                                        {
                                            if (!MoveToTarget(current, prev))
                                            {
                                                moving = true;
                                            }
                                            else
                                            {
                                                MergeToTarget(current, prev);
                                                additionScore += prev.MyCell.Score;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!moving)
                        {
                            break;
                        }
                        yield return null;
                    }
                    break;
                }

            case MoveDirection.Right:
                {
                    while (true)
                    {
                        moving = false;
                        for (int i = 0; i < 4; i++)
                        {
                            for (int j = 2; j >= 0; j--)
                            {
                                var current = slots[i, j];
                                if (current.MyCell != null)
                                {
                                    var next = slots[i, j + 1];
                                    if (next.MyCell == null || next.MyCell.IsMoving)
                                    {
                                        if (!MoveToTarget(current, next))
                                        {
                                            moving = true;
                                        }
                                        else
                                        {
                                            if (current.MyCell.IsMoving)
                                            {
                                                current.MyCell.IsMoving = false;
                                                moving = true;
                                            }
                                            next.AttachCell(current);
                                        }
                                    }
                                    else
                                    {
                                        if (!next.MyCell.Merged && !current.MyCell.Merged && current.MyCell.Score == next.MyCell.Score)
                                        {
                                            if (!MoveToTarget(current, next))
                                            {
                                                moving = true;
                                            }
                                            else
                                            {
                                                MergeToTarget(current, next);
                                                additionScore += next.MyCell.Score;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (!moving)
                        {
                            break;
                        }
                        yield return null;
                    }
                    break;
                }
        }
        TotalScore += additionScore;
        CanvasManager.Instance.RefreshScore(TotalScore, additionScore);
    }

    private bool MoveToTarget(Slot source, Slot target)
    {
        var sourceTrans = source.MyCell.transform;
        var targetPos = target.transform.position;
        var distance = (sourceTrans.position - targetPos).magnitude;
        if (distance > 0f)
        {
            if (!source.MyCell.IsMoving)
            {
                source.MyCell.IsMoving = true;
                sourceTrans.SetParent(CanvasManager.Instance.BgTrans, true);
            }
            sourceTrans.position = Vector3.MoveTowards(sourceTrans.position, targetPos, Time.deltaTime * moveSpeed);
            return false;
        }
        CellMoved = true;
        return true;
    }

    private void MergeToTarget(Slot source, Slot target)
    {
        target.MyCell.Merged = true;
        source.MyCell.IsMoving = false;
        target.MyCell.Score += source.MyCell.Score;
        //target.MyCell.MergeMe();
        //source.MyCell.DestroyMe();
        DestroyImmediate(source.MyCell.gameObject);
    }

    #endregion Coroutines

    #region Methods

    private static bool RandomFlag => UnityEngine.Random.value < 0.5f;

    private static bool InRange(int value) => value >= 0 && value <= 3;

    private bool AnyCanMove()
    {
        if (!IsFull()) return true;
        var slots = CanvasManager.Instance.Slots;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                var currentScore = slots[i, j].MyCell.Score;
                int nearScore;
                if (InRange(i - 1))
                {
                    nearScore = slots[i - 1, j].MyCell.Score;
                    if (nearScore == currentScore)
                    {
                        return true;
                    }
                }
                if (InRange(i + 1))
                {
                    nearScore = slots[i + 1, j].MyCell.Score;
                    if (nearScore == currentScore)
                    {
                        return true;
                    }
                }
                if (InRange(j - 1))
                {
                    nearScore = slots[i, j - 1].MyCell.Score;
                    if (nearScore == currentScore)
                    {
                        return true;
                    }
                }
                if (InRange(j + 1))
                {
                    nearScore = slots[i, j + 1].MyCell.Score;
                    if (nearScore == currentScore)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool IsFull()
    {
        foreach (var slot in Slot.Instances)
        {
            if (slot.MyCell == null)
            {
                return false;
            }
        }
        return true;
    }

    private void GetFreeSlots()
    {
        freeSlots.Clear();
        foreach (var slot in Slot.Instances)
        {
            if (slot.MyCell == null)
            {
                freeSlots.Add(slot);
            }
        }
    }

    private void GenerateRandomCell()
    {
        GetFreeSlots();
        var freeCount = freeSlots.Count;
        if (freeCount > 0)
        {
            if (freeCount > 4) freeCount = 2;
            else freeCount = 1;
            for (int i = 0; i < freeCount; i++)
            {
                var slot = freeSlots[UnityEngine.Random.Range(0, freeSlots.Count)];
                freeSlots.Remove(slot);
                CanvasManager.Instance.InstantiateCell(slot, RandomFlag ? 2 : 4);
            }
        }
    }

    private void ResetCells()
    {
        foreach (var cell in Cell.Instances)
        {
            cell.Merged = false;
        }
    }

    #endregion Methods

    #region Game

    private void StoreGame()
    {
        tempCellList.Clear();
        foreach (var cell in Cell.Instances)
        {
            if (cell.AttachedSlot != null)
            {
                tempCellList.Add(new DataManager.CellData(cell.AttachedSlot.coord, cell.Score));
            }
        }
        DataManager.Instance.StoredCellList = tempCellList;
    }

    private void RestoreGame()
    {
        var slots = CanvasManager.Instance.Slots;
        foreach (var cellData in tempCellList)
        {
            var slot = slots[cellData.CoordX, cellData.CoordY];
            if (slot.MyCell == null)
            {
                CanvasManager.Instance.InstantiateCell(slot, cellData.Score);
            }
        }
    }

    private void ClearStoredGame()
    {
        DataManager.Instance.ClearCellData();
    }

    public void GameOver()
    {
        IsOver = true;
        ClearStoredGame();
        CanvasManager.Instance.OnGameOver();
    }

    public void Win()
    {
        IsOver = true;
        ClearStoredGame();
        CanvasManager.Instance.OnWin();
    }

    public void NewGame()
    {
        IsOver = false;
        TotalScore = 0;
        var cells = Cell.Instances;
        for (int i = cells.Count - 1; i >= 0; i--)
        {
            var cell = cells[i];
            Destroy(cell.gameObject);
        }
        ClearStoredGame();
        CanvasManager.Instance.OnNewGame();
        Invoke(nameof(GenerateRandomCell), 0.5f); 
    } 

    #endregion Game
}