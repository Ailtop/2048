using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static InputManager;

public class GameManagerBackup : Singleton<GameManagerBackup>
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
            /*foreach (var cell in Cell.Instances)
            {
                if (cell.InChanging)
                {
                    return true;
                }
            }*/
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
        MoveHandler += OnMoveChanged;
    }

    private void OnDisable()
    {
        MoveHandler -= OnMoveChanged;
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
            GenerateRandomCell();
        }

        CanvasManager.Instance.RefreshScore(TotalScore);
    }

    #endregion Init

    #region Event

    private void OnMoveChanged(object sender, MoveArgs moveArgs)
    {
        if (IsBuys) return;
        Mergeing = StartCoroutine(CellMoveDirChanged(moveArgs.MoveDir));
    }

    #endregion Event

    #region Coroutines
    private bool CellMoved;

    private IEnumerator CellMoveDirChanged(MoveDirection moveDir)
    {
        CellMoved = false;
        yield return MergeHandle(moveDir);
        yield return MoveHandle(moveDir);

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

    private IEnumerator MergeHandle(MoveDirection moveDir)
    {
        int additionScore = 0;
        var slots = CanvasManager.Instance.Slots;
        switch (moveDir)
        {
            case MoveDirection.Up:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            var current = slots[j, i];
                            if (current.MyCell != null)
                            {
                                Slot next = null; int k = j;
                                while (k < 3 && (next?.MyCell == null))
                                {
                                    k++;
                                    next = slots[k, i];
                                }
                                if (next?.MyCell == null) break;
                                if (current.MyCell.Score == next.MyCell.Score)
                                {
                                    yield return MoveToTarget(next, current, () =>
                                     {
                                         current.MyCell.Score += next.MyCell.Score;
                                         additionScore += current.MyCell.Score;
                                         DestroyImmediate(next.MyCell.gameObject);
                                     });
                                    //yield return current.MyCell.Merge();
                                    //current.MyCell.Score += next.MyCell.Score;
                                    //TotalScore += current.MyCell.Score;
                                    //yield return next.MyCell.Destroy();
                                }
                            }
                        }
                    }
                    break;
                }
            case MoveDirection.Down:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 3; j >= 1; j--)
                        {
                            var current = slots[j, i];
                            if (current.MyCell != null)
                            {
                                Slot prev = null; int k = j;
                                while (k > 0 && (prev?.MyCell == null))
                                {
                                    k--;
                                    prev = slots[k, i];
                                }
                                if (prev?.MyCell != null)
                                {
                                    if (current.MyCell.Score == prev.MyCell.Score)
                                    {
                                        yield return MoveToTarget(prev, current, () =>
                                         {
                                             current.MyCell.Score += prev.MyCell.Score;
                                             additionScore += current.MyCell.Score;
                                             DestroyImmediate(prev.MyCell.gameObject);
                                         });
                                        //yield return current.MyCell.Merge();
                                        //current.MyCell.Score += prev.MyCell.Score;
                                        //TotalScore += current.MyCell.Score;
                                        //yield return prev.MyCell.Destroy();
                                    }
                                }
                            }
                        }
                    }
                    break;
                }
            case MoveDirection.Left:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            var current = slots[i, j];
                            if (current.MyCell != null)
                            {
                                Slot next = null; int k = j;
                                while (k < 3 && (next?.MyCell == null))
                                {
                                    k++;
                                    next = slots[i, k];
                                }
                                if (next?.MyCell == null) break;
                                if (current.MyCell.Score == next.MyCell.Score)
                                {
                                    yield return MoveToTarget(next, current, () =>
                                     {
                                         current.MyCell.Score += next.MyCell.Score;
                                         additionScore += current.MyCell.Score;
                                         DestroyImmediate(next.MyCell.gameObject);
                                     });
                                    //yield return current.MyCell.Merge();
                                    //current.MyCell.Score += next.MyCell.Score;
                                    //TotalScore += current.MyCell.Score;
                                    //yield return next.MyCell.Destroy();
                                }
                            }
                        }
                    }
                    break;
                }
            case MoveDirection.Right:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 3; j >= 1; j--)
                        {
                            var current = slots[i, j];
                            if (current.MyCell != null)
                            {
                                Slot prev = null; int k = j;
                                while (k > 0 && (prev?.MyCell == null))
                                {
                                    k--;
                                    prev = slots[i, k];
                                }
                                if (prev?.MyCell == null) continue;
                                if (current.MyCell.Score == prev.MyCell.Score)
                                {
                                    yield return MoveToTarget(prev, current, () =>
                                     {
                                         current.MyCell.Score += prev.MyCell.Score;
                                         additionScore += current.MyCell.Score;
                                         DestroyImmediate(prev.MyCell.gameObject);
                                     });
                                    //yield return current.MyCell.Merge();
                                    //current.MyCell.Score += prev.MyCell.Score;
                                    //TotalScore += current.MyCell.Score;
                                    //yield return prev.MyCell.Destroy();
                                }
                            }
                        }
                    }
                    break;
                }
        }
        TotalScore += additionScore;
        CanvasManager.Instance.RefreshScore(TotalScore, additionScore);
        yield break;
    }

    private IEnumerator MoveHandle(MoveDirection moveDir)
    {
        var slots = CanvasManager.Instance.Slots;
        switch (moveDir)
        {
            case MoveDirection.Up:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            var current = slots[j, i];
                            if (current.MyCell == null)
                            {
                                Slot prev = null; int k = j;
                                while (k < 3 && (prev == null || prev.MyCell == null))
                                {
                                    k++;
                                    prev = slots[k, i];
                                }
                                if (prev?.MyCell == null) break;
                                yield return MoveToTarget(prev, current, () => { current.AttachCell(prev); });
                            }
                        }
                    }
                    break;
                }
            case MoveDirection.Down:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 3; j >= 1; j--)
                        {
                            var current = slots[j, i];
                            if (current.MyCell == null)
                            {
                                Slot prev = null; int k = j;
                                while (k > 0 && (prev?.MyCell == null))
                                {
                                    k--;
                                    prev = slots[k, i];
                                }
                                if (prev?.MyCell == null) break;
                                yield return MoveToTarget(prev, current, () => { current.AttachCell(prev); });
                            }
                        }
                    }
                    break;
                }
            case MoveDirection.Left:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            var current = slots[i, j];
                            if (current.MyCell == null)
                            {
                                Slot prev = null; int k = j;
                                while (k < 3 && (prev?.MyCell == null))
                                {
                                    k++;
                                    prev = slots[i, k];
                                }
                                if (prev?.MyCell == null) break;
                                yield return MoveToTarget(prev, current, () => { current.AttachCell(prev); });
                            }
                        }
                    }
                    break;
                }
            case MoveDirection.Right:
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 3; j >= 1; j--)
                        {
                            var current = slots[i, j];
                            if (current.MyCell == null)
                            {
                                Slot prev = null; int k = j;
                                while (k > 0 && (prev?.MyCell == null))
                                {
                                    k--;
                                    prev = slots[i, k];
                                }
                                if (prev?.MyCell == null) continue;
                                yield return MoveToTarget(prev, current, () => { current.AttachCell(prev); });
                            }
                        }
                    }
                    break;
                }
        }
        yield break;
    }

    private IEnumerator MoveToTarget(Slot source, Slot target, Action callback = null)
    {
        var sourceTrans = source.MyCell.transform;
        sourceTrans.transform.SetParent(CanvasManager.Instance.BgTrans, true);
        var targetPos = target.transform.position;
        while ((sourceTrans.position - targetPos).magnitude > 0.5f)
        {
            sourceTrans.position = Vector3.MoveTowards(sourceTrans.position, targetPos, Time.deltaTime * moveSpeed);
            yield return null;
        }
        CellMoved = true;
        callback?.Invoke();
    }

    #endregion Coroutines

    #region Methods

    private static bool RandomFlag => UnityEngine.Random.value < 0.5;

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