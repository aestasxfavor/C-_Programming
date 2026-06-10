using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BoardCell : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text markText;     // x, o Ç¥½Ã

    private Action<BoardCell> onLeftClick;
    private Action<BoardCell> onRightClick;
    private Action<BoardCell> onPointerEnter;
    private Action<BoardCell> onPointerExit;
    private Action<BoardCell, PointerEventData> onDrop;

    public int X {  get; private set; }
    public int Y { get; private set; }

    public CellState State { get; private set; }

    public void Init(int x, int y, 
        Action<BoardCell> _onLeftClick, Action<BoardCell> _onRightClick,
        Action<BoardCell> _onPointerEnter, Action<BoardCell> _onPoinerExit,
        Action<BoardCell, PointerEventData> _onDrop)
    {
        X = x;
        Y = y;
        
        onLeftClick = _onLeftClick;
        onRightClick = _onRightClick;
        onPointerEnter = _onPointerEnter;
        onPointerExit = _onPoinerExit;
        onDrop = _onDrop;

        SetState(CellState.Empty);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            onLeftClick?.Invoke(this);
        }
        else
        {
            onRightClick?.Invoke(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
       onPointerEnter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onPointerExit?.Invoke(this);
    }

    public void OnDrop(PointerEventData eventData)
    {
        onDrop?.Invoke(this, eventData);
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
