using UnityEngine;
using UnityEngine.UI;
using System;

public class DragPicturesHintController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RawImage sourceRawImage;   // �@�e�Ϫ� Tex_HiddenImage.RawImage
    [SerializeField] private RawImage interactionImage;    // ���ʰϪ� Image

    // ��h������ �� �q�� PanelFlowController
    public event Action OnDragSimulated;

    /// <summary>
    /// ���˥��ơG�⩳�ϱq�@�e�Ͻƻs�줬�ʰ�
    /// </summary>
    public void SimulateDrag()
    {
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
