using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : SingletonComponent<CanvasManager>
{
    [Header("数字方格预制体")]
    public GameObject CellPrefab;

    [Header("卡槽预制体")]
    public GameObject SlotPrefab;

    [Header("加分UI预制体")]
    public GameObject ScoreScrollPrefab;

    [HideInInspector]
    public Slot[,] Slots = new Slot[4, 4];

    [HideInInspector]
    public Transform BgTrans;

    [HideInInspector]
    public Text QuitGameTips;

    private Transform contentTrans;
    private Text scoreText;
    private Text bestText;
    private GameObject gameOverPanel;
    private GameObject winPanel;
    private Transform scoreBgTrans;

    private void Start()
    {
        BgTrans = transform.Find("Bg");
        contentTrans = transform.Find("Bg/Content");
        scoreBgTrans = transform.Find("TitleBg/Score");
        scoreText = transform.Find("TitleBg/Score/Text").GetComponent<Text>();
        bestText = transform.Find("TitleBg/Best/Text").GetComponent<Text>();
        scoreText.text = 0.ToString();
        bestText.text = DataManager.Instance.BestScore.ToString();
        winPanel = transform.Find("Bg/Win").gameObject;
        gameOverPanel = transform.Find("Bg/GameOver").gameObject;
        QuitGameTips = transform.Find("Bg/QuitGame").GetComponent<Text>();
        GenerateSlots();
    }

    #region Game

    private const float PanelFadeInDuration = 1.5f;
    private float timer;

    public void OnWin()
    {
        StartCoroutine(PanelScroll(winPanel));
    }

    public void OnGameOver()
    {
        StartCoroutine(PanelScroll(gameOverPanel));
    }

    private IEnumerator PanelScroll(GameObject panel)
    {
        timer = 0;
        panel.SetActive(true);
        var canvasGroup = panel.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        while (canvasGroup.alpha <= 1f)
        {
            timer += Time.deltaTime;
            var lerp = Mathf.InverseLerp(0, PanelFadeInDuration, timer);
            canvasGroup.alpha = Mathf.Lerp(0, 1, lerp);
            if (timer >= PanelFadeInDuration)
            {
                canvasGroup.alpha = 1;
                canvasGroup.interactable = true;
                yield break;
            }
            yield return null;
        }
    }

    public void OnNewGame()
    {
        winPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        RefreshScore(0);
    }

    #endregion Game

    #region Refresh

    public void RefreshScore(int totalScore, int additionScore = 0)
    {
        if (additionScore > 0) ScoreScroll(additionScore);
        if (totalScore > DataManager.Instance.BestScore)
        {
            DataManager.Instance.BestScore = totalScore;
            bestText.text = totalScore.ToString();
        }
        scoreText.text = totalScore.ToString();
    }

    #endregion Refresh

    #region Scroll

    private void ScoreScroll(int score)
    {
        var scoreScroll = Instantiate(ScoreScrollPrefab);
        scoreScroll.transform.SetParent(scoreBgTrans);
        scoreScroll.transform.localPosition = Vector3.zero;
        scoreScroll.transform.localScale = Vector3.one;

        var rectTransform = scoreScroll.transform as RectTransform;
        rectTransform.offsetMax = rectTransform.offsetMin = Vector2.zero;

        scoreScroll.GetComponent<BonusScoreScroll>().Init($"+{score}");
    }

    #endregion Scroll

    private void GenerateSlots()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                var slot = Instantiate(SlotPrefab).GetComponent<Slot>();
                slot.transform.SetParent(contentTrans);
                slot.transform.localScale = Vector3.one;
                slot.coord = new Vector2(i, j);
                Slots[i, j] = slot;
            }
        }
    }

    public Cell InstantiateCell(Slot slot, int score)
    {
        var cell = Instantiate(CellPrefab).GetComponent<Cell>();

        cell.transform.SetParent(slot.transform, false);
        cell.transform.localPosition = Vector3.zero;
        cell.transform.localScale = Vector3.one;

        cell.AttachedSlot = slot;

        cell.Score = score;

        slot.MyCell = cell;
        return cell;
    }

    public class ColorData
    {
        public static readonly Dictionary<int, ColorData> storedColors = new Dictionary<int, ColorData>
        {
            [2] = new ColorData("#eee4da", "#776e65"),
            [4] = new ColorData("#ede0c8", "#776e65"),
            [8] = new ColorData("#f2b179", "#776e65"),
            [16] = new ColorData("#f59563", "#f9f6f2"),
            [32] = new ColorData("#f67d5f", "#f9f6f2"),
            [64] = new ColorData("#f65e3b", "#f9f6f2"),
            [128] = new ColorData("#edcf72", "#f9f6f2"),
            [256] = new ColorData("#edcc61", "#f9f6f2"), 
            [512] = new ColorData("#eac036", "#f9f6f2"),

            [1024] = new ColorData("#6ED6E3", "#f9f6f2"),
            [2048] = new ColorData("#0DBED4", "#f9f6f2"),
        };

        public static Color GetCellColor(int score)
        {
            if (!storedColors.TryGetValue(score, out ColorData colorData))
            {
                Debug.LogError($"不正确的分数: {score}");
                return Color.white;
            }
            return colorData.CellColor;
        }

        public static Color GetTextColor(int score)
        {
            if (!storedColors.TryGetValue(score, out ColorData colorData))
            {
                Debug.LogError($"不正确的分数: {score}");
                return Color.white;
            }
            return colorData.TextColor;
        }

        public string CellHtmlColor;
        public string TextHtmlColor;
        public Color CellColor;
        public Color TextColor;

        public ColorData()
        {
        }

        public ColorData(string cell, string text)
        {
            var flag = ColorUtility.TryParseHtmlString(cell, out CellColor);
            if (!flag)
            {
                CellColor = Color.white;
                Debug.Log("错误的格子HTML颜色格式");
            }
            flag = ColorUtility.TryParseHtmlString(text, out TextColor);
            if (!flag)
            {
                CellColor = Color.white;
                Debug.Log("错误的文字HTML颜色格式");
            }
        }
    }
}