using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("Card View")]
    [SerializeField] private GameObject back;
    [SerializeField] private GameObject front;
    [SerializeField] private Image frontImage;
    [SerializeField] private Button button;

    [Header("Flip")]
    [SerializeField] private float flipDuration = 0.15f;

    [SerializeField] private ParticleSystem matchParticle;

    private int cardId;
    private GameManager gameManager;

    private bool isFlipped;
    private bool isMatched;
    private bool isAnimating;

    public int CardId => cardId;
    public bool IsAnimating => isAnimating;

    private void Reset()
    {
        button = GetComponent<Button>();
    }

    public void Init(int id, Sprite frontSprite, GameManager manager)
    {
        cardId = id;
        gameManager = manager;

        isFlipped = false;
        isMatched = false;
        isAnimating = false;

        frontImage.sprite = frontSprite;

        SetBackInstant();

        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickCard);
    }

    // 카드 클릭 로직
    private void OnClickCard()
    {
        if (isFlipped) return;
        if (isMatched) return;
        if (isAnimating) return;

        gameManager.SelectCard(this);
    }

    // 카드 앞면 보여주는 로직
    public void ShowFront()
    {
        if (isAnimating) return;

        isFlipped = true;
        StartCoroutine(FlipRoutine(true));
    }

    // 카드 뒷면 보여주는 로직
    public void ShowBack()
    {
        if (isAnimating) return;

        isFlipped = false;
        StartCoroutine(FlipRoutine(false));
    }

    public void SetFrontInstant()
    {
        isFlipped = true;
        front.SetActive(true);
        back.SetActive(false);
        transform.localScale = Vector3.one;
    }

    public void SetBackInstant()
    {
        isFlipped = false;
        front.SetActive(false);
        back.SetActive(true);
        transform.localScale = Vector3.one;
    }

    // 카드 뒤집는 코루틴 함수
    private IEnumerator FlipRoutine(bool showFront)
    {
        isAnimating = true;

        Vector3 startScale = transform.localScale;

        yield return ScaleX(1f, 0f);

        front.SetActive(showFront);
        back.SetActive(!showFront);

        yield return ScaleX(0f, 1f);

        transform.localScale = startScale;
        isAnimating = false;
    }

    private IEnumerator ScaleX(float start, float end)
    {
        float timer = 0f;
        Vector3 scale = transform.localScale;

        while (timer < flipDuration)
        {
            timer += Time.deltaTime;

            float x = Mathf.Lerp(start, end, timer / flipDuration);
            transform.localScale = new Vector3(x, scale.y, scale.z);

            yield return null;
        }

        transform.localScale = new Vector3(end, scale.y, scale.z);
    }

    // 매치된 카드 로직
    public void SetMatched()
    {
        isMatched = true;
        button.interactable = false;
    }

    // 매치했을때만 나오는 파티클 로직
    public void PlayMatchEffect()
    {
        if (matchParticle != null)
        {
            matchParticle.Stop();
            matchParticle.Clear();
            matchParticle.Play();
        }
    }
}