using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardCell : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text markText;     // x, o 표시
    [SerializeField] private Button button;         // 마우스 클릭으로 판단

    public int X {  get; private set; }
    public int Y { get; private set; }

    public CellState State { get; private set; }

    private Action<BoardCell> onClick;

    public void Init(int x, int y, Action<BoardCell> _onClick)
    {
        X = x;
        Y = y;
        onClick = _onClick;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleClick);

        SetState(CellState.Empty);
    }

    private void HandleClick()
    {
        onClick?.Invoke(this);
    }

    public void SetState(CellState state)
    {
        State = state;

        if(markText != null)
        {
            markText.text = "";
        }
    }

    public void SetSprite(Sprite sprite)
    {
        if(backgroundImage != null)
        {
            backgroundImage.sprite = sprite;
        }
    }

    public void SetMark(string mark)
    {
        if (markText != null)
        {
            markText.text = mark;
        }
    }
}
