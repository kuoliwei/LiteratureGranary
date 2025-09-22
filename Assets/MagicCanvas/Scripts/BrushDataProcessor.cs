using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BrushDataProcessor : MonoBehaviour
{
    [SerializeField] private List<ScratchCard> scratchCards;
    [SerializeField] private HandReferenceDotSpawner refDotSpawner; // �ѦҬ��I���
                                                                    // BrushDataProcessor.cs�]�`���^
    [SerializeField] private UILaserButtonInteractor buttonInteractor;
    // [NEW] �o�N�O����@�Ϊ��ؼ� UI�]UV �� 0..1 �|�M�b�o�� RectTransform �W�^
    [SerializeField] private RectTransform paintingUIRectTransform; // [NEW]
    [SerializeField] private RectTransform interactiveUIRectTransform; // [NEW]
    [SerializeField] HandParticleEffectSpawner spawner;
    public bool isRevealing = false;
    void Start() // �� Awake()
    {
        // [NEW] �Τ@��������Ӥ���
        if (paintingUIRectTransform != null)
        {
            if (refDotSpawner != null) refDotSpawner.SetTargetRectTransform(paintingUIRectTransform);   // [NEW]
            if (buttonInteractor != null) buttonInteractor.SetTargetRectTransform(paintingUIRectTransform); // [NEW]
        }
        else
        {
            Debug.LogWarning("[BrushDataProcessor] sharedUIRectTransform �����w�AUV �ഫ�N���ġC"); // [NEW]
        }
        // �s�W�G�⤬�ʰϪ��ؼе��S�� Spawner
        if (spawner != null && interactiveUIRectTransform != null)                          // [NEW]
            spawner.SetTargets(interactiveUIRectTransform, interactiveUIRectTransform.GetComponentInParent<Canvas>()); // [NEW]
    }
    public void HandleBrushData(List<BrushData> dataList)
    {
        List<Vector2> screenPosList = new List<Vector2>(); // [�s�W] �����Ҧ���y�Ъ��ù���m
        List<Vector2> uvList = new List<Vector2>();        // [NEW] �������� UV�A�����I UI ��
        foreach (var data in dataList)
        {
            if (data.point == null || data.point.Length < 2)
                continue;

            float x = data.point[0];
            //float y = 1f - data.point[1];            // JSON �ǨӬ����W�����I�A���ର���U�����I
            float y = data.point[1];            // �אּ�����P�_�A��������
            Vector2 uv = new Vector2(x, y);

            screenPosList.Add(new Vector2(x * Screen.width, y * Screen.height)); // [�s�W] �[�J�ù��y�вM��
            uvList.Add(uv); // [NEW] �ֿn UV �Ѭ��I UI �ϥ�

            foreach (var card in scratchCards)
            {
                if (!card.isRevealing && card.gameObject.activeSelf)
                {
                    card.EraseAtNormalizedUV(uv); // �C�@�ӥd�����B�z�o�Ө��I
                }
            }
        }

        //// �� �s�W�G���s�R���P�_
        //if (buttonInteractor != null && screenPosList.Count > 0)
        //    buttonInteractor.Process(screenPosList);
        // �� �s�W�G���s�R���P�_�]UV���^
        int heatedUvIndex = -1;
        float heatedHoldTimesPercentage = 0;
        if (buttonInteractor != null && uvList.Count > 0)
            buttonInteractor.ProcessUV(uvList, true, out heatedUvIndex, out heatedHoldTimesPercentage);

        // �X�X �e�����I�P�B �X�X //
        if (refDotSpawner != null)
        {
            //if (screenPosList.Count > 0)
            //    refDotSpawner.SyncDotsToScreenPositions(screenPosList);
            //else
            //    refDotSpawner.ClearAll();
            if (uvList.Count > 0)                                  // [NEW]
                refDotSpawner.SyncDotsToUVPositions(uvList,true, heatedUvIndex, heatedHoldTimesPercentage);       // [NEW] ������ UV �ǵ� spawner�]�|�� uiRectTransform �p��^
            else                                                   // [NEW]
                refDotSpawner.ClearAll();
        }

        //// [�s�W] ����h�ӯS�Ī���
        //if (scratchCards[0].isRevealing && screenPosList.Count > 0)
        //    spawner.SyncParticlesToScreenPositions(screenPosList); // �h�Ӥ�N�|���h�ӯS��
        //else
        //    spawner.ClearAll(); // �S�H�ɥ����P��
    }
    // BrushDataProcessor.cs ���s�W�]���ʧA�즳 HandleBrushData�^
    public void HandleEffectUV(List<Vector2> effectUVs) // [NEW]
    {
        if (effectUVs == null || effectUVs.Count == 0) return;

        // �X�X ���ʰϯS�ġ]Interactive �ϡ^ �X�X
        if (spawner != null)
        {
            if (effectUVs != null && effectUVs.Count > 0)     // [NEW] �P���I�@�P�G���N�P�B
                spawner.SyncParticlesToUVPositions(effectUVs);
            else                                              // [NEW] �S���N���W�M�šA�קK�ݯd
                spawner.ClearAll();
        }
    }
}
