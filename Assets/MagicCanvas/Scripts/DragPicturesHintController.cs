using UnityEngine;
using UnityEngine.UI;
using System;

public class DragPicturesHintController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RawImage sourceRawImage;   // �@�e�Ϫ� Tex_HiddenImage.RawImage
    [SerializeField] private RawImage interactionImage;    // ���ʰϪ� Image

    [Header("Settings")] // [PORTED FROM KOKU]
    [SerializeField] private float autoDragDelay; // �X���۰�Ĳ�o [PORTED FROM KOKU]
    private bool alreadyDragged = false; // [PORTED FROM KOKU]

    // ��h������ �� �q�� PanelFlowController
    public event Action OnDragSimulated;
    // [PORTED FROM KOKU] �� �Ұʭ˼Ʀ۰ʩ즲
    private void OnEnable()
    {
        alreadyDragged = false;
        StartCoroutine(AutoDragAfterDelay(autoDragDelay));
    }
    // [PORTED FROM KOKU] �� �˼ƨ�{
    private System.Collections.IEnumerator AutoDragAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (!alreadyDragged)
        {
            Debug.Log("[DragPicturesHintController] �W�ɦ۰ʰ���즲");
            SimulateDrag();
        }
    }
    /// <summary>
    /// ���˥��ơG�⩳�ϱq�@�e�Ͻƻs�줬�ʰ�
    /// </summary>
    public void SimulateDrag()
    {

        if (alreadyDragged) return; // [PORTED FROM KOKU] �����Ĳ�o
        alreadyDragged = true;

        if (sourceRawImage != null && interactionImage != null)
        {
            interactionImage.texture = null;   // �T�O�O�� texture ���
            interactionImage.material = null;
            interactionImage.texture = sourceRawImage.texture;

            Debug.Log("[DragPicturesHintController] ���Ϥw�h���줬�ʰ�");
        }
        else
        {
            Debug.LogWarning("[DragPicturesHintController] �|������ RawImage �� InteractionImage�I");
        }

        OnDragSimulated?.Invoke();
    }
}
