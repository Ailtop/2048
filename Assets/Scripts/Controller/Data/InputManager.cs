using System;
using UnityEditor;
using UnityEngine;

public class InputManager : SingletonComponent<InputManager>
{
    public static event EventHandler<MoveArgs> MoveHandler;

    public class MoveArgs : EventArgs
    {
        private readonly MoveDirection moveDir;
        public MoveDirection MoveDir => moveDir;

        public MoveArgs(MoveDirection moveDir)
        {
            this.moveDir = moveDir;
        }
    }

    public enum MoveDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    private const float MaxDragTime = 3f;

    private float minDragDistance = 200f;
    private bool isPress;
    private float timer;
    private Vector3 pressedPos;

    #region Quit

    private const float QuitTipsFadeDuration = 2f;
    private bool quitTipsFaded = true;
    private float quitTipsFadeTimer;

    #endregion Quit

    private void Start()
    {
        minDragDistance = (Screen.height + Screen.width) / 30;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (quitTipsFaded)
            {
                quitTipsFaded = false;
                CanvasManager.Instance.QuitGameTips.gameObject.SetActive(true);
            }
            else
            {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#elif UNITY_ANDROID || UNITY_IOS
                Application.Quit();
#endif
                return;
            }
        }
        if (!quitTipsFaded)
        {
            quitTipsFadeTimer += Time.deltaTime;
            var lerp = Mathf.InverseLerp(QuitTipsFadeDuration, 0, quitTipsFadeTimer);
            CanvasManager.Instance.QuitGameTips.color = new Color(0, 0, 0, lerp);
            if (quitTipsFadeTimer >= QuitTipsFadeDuration)
            {
                quitTipsFaded = true;
                quitTipsFadeTimer = 0;
                CanvasManager.Instance.QuitGameTips.gameObject.SetActive(false);
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            isPress = false;
            MoveHandler?.Invoke(this, new MoveArgs(MoveDirection.Up));
            return;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            isPress = false;
            MoveHandler?.Invoke(this, new MoveArgs(MoveDirection.Down));
            return;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            isPress = false;
            MoveHandler?.Invoke(this, new MoveArgs(MoveDirection.Left));
            return;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            isPress = false;
            MoveHandler?.Invoke(this, new MoveArgs(MoveDirection.Right));
            return;
        }
#if   UNITY_EDITOR
        MouseUpdate();
#elif UNITY_ANDROID || UNITY_IOS
        TouchUpdate();
#endif
    }

#if UNITY_EDITOR

    private void MouseUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BeginPress(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (EndPress(Input.mousePosition))
            {
                return;
            }
        }
        PressUpdate();
    }

#endif
#if UNITY_ANDROID || UNITY_IOS

    private void TouchUpdate()
    {
        if (Input.touchCount == 0)
        {
            return;
        }
        var touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            BeginPress(touch.position);
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            if (EndPress(touch.position))
            {
                return;
            }
        }
        PressUpdate();
    }

#endif

    private void PressUpdate()
    {
        if (isPress)
        {
            timer += Time.deltaTime;
            if (timer >= MaxDragTime)
            {
                isPress = false;
            }
        }
    }

    private void BeginPress(Vector3 pos)
    {
        isPress = true;
        timer = 0;
        pressedPos = pos;
    }

    private bool EndPress(Vector3 pos)
    {
        if (!isPress) return true;
        isPress = false;
        var dir = pos - pressedPos;
        if (dir.magnitude < minDragDistance)
        {
            return true;
        }
        var angle = Mathf.Rad2Deg * Mathf.Atan2(dir.y, dir.x);
        if (angle >= 45 && angle < 135)
        {
            MoveHandler?.Invoke(this, new MoveArgs(MoveDirection.Up));
            return true;
        }
        if (angle >= -135 && angle < -45)
        {
            MoveHandler?.Invoke(this, new MoveArgs(MoveDirection.Down));
            return true;
        }
        if (angle < -135 || angle >= 135)
        {
            MoveHandler?.Invoke(this, new MoveArgs(MoveDirection.Left));
            return true;
        }
        if (angle >= -45 && angle < 45)
        {
            MoveHandler?.Invoke(this, new MoveArgs(MoveDirection.Right));
            return true;
        }
        return false;
    }
}