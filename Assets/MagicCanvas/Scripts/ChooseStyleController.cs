using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChooseStyleController : MonoBehaviour
{
    [Header("Deps")]
    [SerializeField] private ImageStyleTransferHandler styleHandler; // ��ݩI�s

    [Header("Source")]
    private Texture2D sourcePhoto;
    public Texture2D SourcePhoto => sourcePhoto;  // ��Ū�ݩʡA��~���}

    public event Action<string> OnStyleFailed;                       // �q���~���G���ѰT��
                                                                     // �u�^�����ȫإߪ����G
    public event Action<string> OnStyleTaskCreated;     // ���\�G�^�� taskId
    public event Action<string> OnStyleTaskFailed;      // ���ѡG�^�ǭ�]

    private bool isSending;
    [Header("UI")]
    [SerializeField] private Text resultText;   // ����A�s�W�����~�T�� Text
    // ��~��k�A���~���i�H���w�Ӥ�
    public void SetSourcePhoto(Texture2D tex)
    {
        sourcePhoto = tex;
    }
    // ��������s�j�w�]�r��i�� Button �� OnClick �ǤJ�^
    // ������s OnClick �j�o�ӡ]�ǤJ style �W�١^
    public void ChooseStyle(string styleName)
    {
        Debug.Log("�I�sChooseStyle()");
        if (isSending) return;
        if (SourcePhoto == null) { OnStyleTaskFailed?.Invoke("�S���ӷ��Ӥ�"); return; }
        StartCoroutine(Co_SendOnly(styleName, SourcePhoto));
    }

    private IEnumerator Co_SendOnly(string styleName, Texture2D photo)
    {
        Debug.Log("�I�sCo_SendOnly()");
        isSending = true;

        // 1) �� JPG �� base64
        byte[] jpg = photo.EncodeToJPG(85);
        string image64 = Convert.ToBase64String(jpg);

        // 2) �e�X�ШD�A�u���^ taskId
        string taskId = null;
        yield return styleHandler.SendStyleRequest(image64, styleName, tid => taskId = tid);

        if (string.IsNullOrEmpty(taskId))
        {
            OnStyleTaskFailed?.Invoke("�إ߭���ƥ��ȥ���");
            isSending = false;

            yield break;
        }

        // ���\�G�� taskId �ᵹ�~���]PanelFlowController / Waiting ���O�^
        OnStyleTaskCreated?.Invoke(taskId);
        isSending = false;
    }
    public void ShowResult(string result)
    {
        if (resultText != null)
            resultText.text = result;           // �b���O�W���
    }
}
