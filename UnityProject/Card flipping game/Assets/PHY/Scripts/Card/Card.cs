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

    [Header("Particle")]
    [SerializeField] private ParticleSystem matchParticle;

    private GameManager gameManager;

    private int cardId;

    private bool isFlipped;
    private bool isMatched;
    private bool isAnimating;

    public int CardId => cardId;

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

        CloseCard();

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

    // 카드 앞면 보여주기
    public void ShowFront()
    {
        if (isAnimating) return;

        isFlipped = true;
        StartCoroutine(FlipRoutine(true));
    }

    // 카드 뒷면 보여주기
    public void ShowBack()
    {
        if (isAnimating) return;

        isFlipped = false;
        StartCoroutine(FlipRoutine(false));
    }

    public void OpenCard()
    {
        isFlipped = true;
        front.SetActive(true);
        back.SetActive(false);
        transform.localScale = Vector3.one;
    }

    public void CloseCard()
    {
        isFlipped = false;
        front.SetActive(false);
        back.SetActive(true);
        transform.localScale = Vector3.one;
    }

    // 카드 뒤집기 애니메이션
    private IEnumerator FlipRoutine(bool showFront)
    {
        isAnimating = true;

        Vector3 originalScale = transform.localScale;
        float timer = 0f;

        // 카드가 접히는 것처럼 X 크기 줄이기
        while (timer < flipDuration)
        {
            timer += Time.deltaTime;

            float x = Mathf.Lerp(originalScale.x, 0f, timer / flipDuration);
            transform.localScale = new Vector3(x, originalScale.y, originalScale.z);

            yield return null;
        }

        // 카드가 거의 안 보이는 순간 앞면,뒷면 교체
        front.SetActive(showFront);
        back.SetActive(!showFront);

        timer = 0f;

        // 다시 펼쳐지는 것처럼 X 크기를 키우기
        while (timer < flipDuration)
        {
            timer += Time.deltaTime;

            float x = Mathf.Lerp(0f, originalScale.x, timer / flipDuration);
            transform.localScale = new Vector3(x, originalScale.y, originalScale.z);

            yield return null;
        }

        transform.localScale = originalScale;
        isAnimating = false;
    }

    // 매치된 카드 처리
    public void SetMatched()
    {
        isMatched = true;
        button.interactable = false;
    }

    // 매치했을때만 나오는 파티클
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