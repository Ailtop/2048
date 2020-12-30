using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class Cell : CollectionComponent<Cell>
{
    [HideInInspector]
    public Slot AttachedSlot;

    private float extendDuration = 0.2f;
    private float shrinkDuration = 0.05f;
    private float mergeDuration = 0.5f;
    private float mergeHalfDuration = 0.25f;

    private bool extended;
    private float timer;

    private int score;
    private Text cellText;
    private Image cellImage;
    public bool Merged { get; set; }
    public bool IsMoving { get; set; } 
    public bool InExtending => !extended;

    public Text CellText
    {
        get
        {
            if (cellText == null)
            {
                cellText = GetComponentInChildren<Text>();
            }
            return cellText;
        }
    }

    public Image CellImage
    {
        get
        {
            if (cellImage == null)
            {
                cellImage = GetComponent<Image>();
            }
            return cellImage;
        }
    }

    public int Score
    {
        get { return score; }
        set
        {
            score = value;
            if (score == 0) return;
            CellImage.color = CanvasManager.ColorData.GetCellColor(score);
            CellText.color = CanvasManager.ColorData.GetTextColor(score);
            CellText.text = score.ToString();
            if (score == 2048)
            {
                GameManager.Instance.Win();
            }
        }
    }

    private void Update()
    {
        if (!extended)
        {
            timer += Time.deltaTime;
            var lerp = Mathf.InverseLerp(0f, extendDuration, timer);
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, lerp);
            if (timer >= extendDuration)
            {
                timer = 0;
                extended = true;
            }
        }
    }

    public void DestroyMe()
    {
        StartCoroutine(Destroy());
    }

    public void MergeMe()
    {
        StartCoroutine(Merge());
    }

    private IEnumerator Destroy()
    {
        timer += Time.deltaTime;
        var lerp = Mathf.InverseLerp(0f, shrinkDuration, timer);
        transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, lerp);
        if (timer >= shrinkDuration)
        {
            DestroyImmediate(gameObject);
            yield break;
        }
        yield return null;
    }

    private IEnumerator Merge()
    {
        timer += Time.deltaTime;
        if (timer < mergeHalfDuration)
        {
            var lerp = Mathf.InverseLerp(0f, mergeHalfDuration, timer);
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, lerp);
        }
        else
        {
            var lerp = Mathf.InverseLerp(0f, mergeHalfDuration, timer);
            transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, lerp);
        }
        if (timer >= mergeDuration)
        {
            timer = 0;
            yield break;
        }
        yield return null;
    }
}