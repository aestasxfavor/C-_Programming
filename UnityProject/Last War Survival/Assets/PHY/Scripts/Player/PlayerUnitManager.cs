using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerUnitManager : MonoBehaviour
{
    [Header("Unit Settings")]
    [SerializeField] private int currentUnitCount = 1;
    [SerializeField] private int minUnitCount = 1;
    [SerializeField] private int maxUnitCount = 100;

    [Header("Visual Source")]
    [SerializeField] private GameObject armatureSource;
    [SerializeField] private GameObject meshSource;

    [Header("Unit Formation")]
    [SerializeField] private Transform unitParent;
    [SerializeField] private int unitsPerRow = 5;
    [SerializeField] private float spacingX = 0.6f;
    [SerializeField] private float spacingZ = 0.8f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI unitCountText;

    private readonly List<GameObject> unitCopies = new List<GameObject>();

    public int CurrentUnitCount => currentUnitCount;

    private void Start()
    {
        FindVisualSources();
        PrepareUnitParent();

        ClampUnitCount();
        SyncUnitCopies();
        UpdateUnitCountUI();
    }

    public void ApplyGate(OperationType operationType, int value)
    {
        int amount = Mathf.Abs(value);

        Debug.Log($"Gate Applied: {operationType} {value}");

        switch (operationType)
        {
            case OperationType.Plus:
                currentUnitCount += amount;
                break;

            case OperationType.Minus:
                currentUnitCount -= amount;
                break;

            case OperationType.Multiply:
                currentUnitCount *= amount;
                break;

            //게이트 연산은 현재 전체 유닛 수를 기준으로 적용
            //나누기 연산은 정수 계산 기준으로 소수점 버림 처리
            case OperationType.Divide:
                if (amount != 0)
                {
                    currentUnitCount /= amount;
                }
                break;
        }

        ClampUnitCount();

        Debug.Log($"After Gate Unit Count: {currentUnitCount}");

        SyncUnitCopies();
        UpdateUnitCountUI();

        Debug.Log($"Player Unit Count: {currentUnitCount}");
    }

    public void ReduceUnitCount(int damage)
    {
        currentUnitCount -= damage;

        ClampUnitCount();
        SyncUnitCopies();
        UpdateUnitCountUI();

        Debug.Log($"Player Unit Count: {currentUnitCount}");

        if (currentUnitCount <= 0)
        {
            HandleUnitCountZero();
        }
    }

    private void HandleUnitCountZero()
    {
        Debug.Log("Game Over: All player units are dead.");
    }

    private void FindVisualSources()
    {
        if (armatureSource == null)
        {
            Transform armature = transform.Find("Armature");

            if (armature != null)
            {
                armatureSource = armature.gameObject;
            }
        }

        if (meshSource == null)
        {
            Transform mesh = transform.Find("Player");

            if (mesh != null)
            {
                meshSource = mesh.gameObject;
            }
        }

        if (armatureSource == null)
        {
            Debug.LogError("Armature Source를 찾지 못했어요. 루트 Player 밑에 Armature가 있는지 확인하세요.");
        }

        if (meshSource == null)
        {
            Debug.LogError("Mesh Source를 찾지 못했어요. 루트 Player 밑에 자식 Player가 있는지 확인하세요.");
        }
    }

    private void PrepareUnitParent()
    {
        if (unitParent != null)
        {
            return;
        }

        GameObject unitParentObject = new GameObject("UnitParent");
        unitParent = unitParentObject.transform;

        unitParent.SetParent(transform);
        unitParent.localPosition = Vector3.zero;
        unitParent.localRotation = Quaternion.identity;
        unitParent.localScale = Vector3.one;
    }

    private void SyncUnitCopies()
    {
        if (armatureSource == null || meshSource == null)
        {
            Debug.LogWarning("아군 유닛 복제용 소스가 비어 있어서 복제를 중단해요.");
            return;
        }

        int copyCount = currentUnitCount - 1;

        Debug.Log($"Need Copy Count: {copyCount}, Current Copy Count: {unitCopies.Count}");

        while (unitCopies.Count < copyCount)
        {
            AddUnitCopy();
        }

        for (int i = 0; i < unitCopies.Count; i++)
        {
            unitCopies[i].SetActive(i < copyCount);
        }

        UpdateFormation(copyCount);
    }

    private void AddUnitCopy()
    {
        GameObject unitRoot = new GameObject($"PlayerUnit_Copy_{unitCopies.Count + 1}");

        unitRoot.transform.SetParent(unitParent);
        unitRoot.transform.localPosition = Vector3.zero;
        unitRoot.transform.localRotation = Quaternion.identity;
        unitRoot.transform.localScale = Vector3.one;

        GameObject armatureCopy = Instantiate(armatureSource, unitRoot.transform);
        armatureCopy.name = "Armature";
        armatureCopy.transform.localPosition = armatureSource.transform.localPosition;
        armatureCopy.transform.localRotation = armatureSource.transform.localRotation;
        armatureCopy.transform.localScale = armatureSource.transform.localScale;

        GameObject meshCopy = Instantiate(meshSource, unitRoot.transform);
        meshCopy.name = "Player";
        meshCopy.transform.localPosition = meshSource.transform.localPosition;
        meshCopy.transform.localRotation = meshSource.transform.localRotation;
        meshCopy.transform.localScale = meshSource.transform.localScale;

        RebindSkinnedMeshes(meshCopy.transform, armatureCopy.transform);
        DisableUnneededComponents(unitRoot);

        unitRoot.SetActive(false);
        unitCopies.Add(unitRoot);

        Debug.Log($"Unit Copy Created: {unitRoot.name}");
    }

    private void RebindSkinnedMeshes(Transform meshRoot, Transform copiedArmatureRoot)
    {
        SkinnedMeshRenderer[] skinnedMeshRenderers = meshRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            Transform[] originalBones = skinnedMeshRenderer.bones;
            Transform[] copiedBones = new Transform[originalBones.Length];

            for (int i = 0; i < originalBones.Length; i++)
            {
                Transform copiedBone = FindChildByName(copiedArmatureRoot, originalBones[i].name);

                if (copiedBone != null)
                {
                    copiedBones[i] = copiedBone;
                }
                else
                {
                    copiedBones[i] = originalBones[i];
                }
            }

            skinnedMeshRenderer.bones = copiedBones;

            if (skinnedMeshRenderer.rootBone != null)
            {
                Transform copiedRootBone = FindChildByName(copiedArmatureRoot, skinnedMeshRenderer.rootBone.name);

                if (copiedRootBone != null)
                {
                    skinnedMeshRenderer.rootBone = copiedRootBone;
                }
            }
        }
    }

    private Transform FindChildByName(Transform parent, string targetName)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
        {
            if (child.name == targetName)
            {
                return child;
            }
        }

        return null;
    }

    private void DisableUnneededComponents(GameObject unitRoot)
    {
        MonoBehaviour[] scripts = unitRoot.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour script in scripts)
        {
            if (script == null)
            {
                continue;
            }

            script.enabled = false;
        }

        Collider[] colliders = unitRoot.GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        Rigidbody[] rigidbodies = unitRoot.GetComponentsInChildren<Rigidbody>(true);

        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void UpdateFormation(int activeCopyCount)
    {
        for (int i = 0; i < activeCopyCount; i++)
        {
            int row = i / unitsPerRow;
            int column = i % unitsPerRow;

            int rowStartIndex = row * unitsPerRow;
            int countInThisRow = Mathf.Min(unitsPerRow, activeCopyCount - rowStartIndex);

            float centerOffset = (countInThisRow - 1) * 0.5f;

            float x = (column - centerOffset) * spacingX;
            float z = -(row + 1) * spacingZ;

            unitCopies[i].transform.localPosition = new Vector3(x, 0f, z);
            unitCopies[i].transform.localRotation = Quaternion.identity;
            unitCopies[i].transform.localScale = Vector3.one;
        }
    }

    private void ClampUnitCount()
    {
        currentUnitCount = Mathf.Clamp(currentUnitCount, minUnitCount, maxUnitCount);
    }

    private void UpdateUnitCountUI()
    {
        if (unitCountText == null)
        {
            return;
        }

        unitCountText.text = $"Unit : {currentUnitCount}";
    }
}