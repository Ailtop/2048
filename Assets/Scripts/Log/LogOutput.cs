using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogOutput : MonoBehaviour
{
    private Text outputText;
    private CanvasGroup outputCanvasGroup;
    private const float Duration = 5f;
    private float timer;

    private string ExceptionText
    {
        get { return outputText.text; }
        set
        {
            outputText.text = value;
            if (string.IsNullOrEmpty(outputText.text))
            {
                outputCanvasGroup.alpha = 0;
            }
        }
    }

    private void Start()
    {
        outputText = GetComponent<Text>();
        outputText.text = null;
        outputCanvasGroup = GetComponent<CanvasGroup>();
        outputCanvasGroup.alpha = 0;
        outputCanvasGroup.interactable = false;
        Application.logMessageReceived += Application_logMessageReceived;
    }

    private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            timer = 0;
            ExceptionText = $"{condition} : {stackTrace}";
            print(ExceptionText);
        }
    }

    private void Update()
    {
        if (!string.IsNullOrEmpty(ExceptionText))
        {
            timer += Time.deltaTime;
            var lerp = Mathf.InverseLerp(0, Duration, timer);
            outputCanvasGroup.alpha = Mathf.Lerp(1, 0, lerp);
            if (timer >= Duration)
            {
                timer = 0;
                ExceptionText = null;
            }
        }
    }
}