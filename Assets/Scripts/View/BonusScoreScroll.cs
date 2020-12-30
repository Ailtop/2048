using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonusScoreScroll : MonoBehaviour
{
    private float speed = 100f;
    private float duration = 0.6f;
    private float timer;
    private Text scoreText;

    private void Awake()
    {
        enabled = false;
        speed = Screen.height / 10;
        scoreText = GetComponent<Text>();
    }

    public void Init(string text)
    {
        scoreText.text = text;
        enabled = true;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        transform.Translate(Vector3.up * Time.deltaTime * speed);
        var lerp = Mathf.InverseLerp(duration, 0, timer);
        scoreText.color = new Color(0, 0, 0, lerp);
        if (timer >= duration)
        {
            Destroy(gameObject);
            return;
        }
    }
}