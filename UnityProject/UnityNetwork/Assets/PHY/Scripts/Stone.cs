using UnityEngine;

public class Stone : MonoBehaviour
{
    // 흑돌과 백돌의 색 머터리얼
    public Material[] colorMaterials;
    public bool SetInit(bool isBlack)
    {
        Material[] mats = new Material[1];
        mats[0] = colorMaterials[isBlack ? 0 : 1];

        GetComponent<MeshRenderer>().materials = mats;

        return isBlack;
    }
}
