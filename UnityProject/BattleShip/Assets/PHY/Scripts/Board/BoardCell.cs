using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 보드 한 칸의 좌표, 상태, Sprite, X/O 표시와 포인터 이벤트 전달을 담당하는 셀 스크립트
public class BoardCell : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text markText;     // x, o 표시

    private Action<BoardCell> onLeftClick;
    private Action<BoardCell> onRightClick;
    private Action<BoardCell> onPointerEnter;
    private Action<BoardCell> onPointerExit;
    private Action<BoardCell, PointerEventData> onDrop;

    public int X {  get; private set; }
    public int Y { get; private set; }

    public CellState State { get; private set; }

    // 셀 좌표와 입력 콜백 연결
    public void Init(int x, int y, 
        Action<BoardCell> _onLeftClick, Action<BoardCell> _onRightClick,
        Action<BoardCell> _onPointerEnter, Action<BoardCell> _onPointerExit,
        Action<BoardCell, PointerEventData> _onDrop)
    {
        X = x;
        Y = y;
        
        onLeftClick = _onLeftClick;
        onRightClick = _onRightClick;
        onPointerEnter = _onPointerEnter;
        onPointerExit = _onPointerExit;
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
