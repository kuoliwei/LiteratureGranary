using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TakingPhotoController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private WebCamController webCam;   // ����A�{���� WebCamController
    [SerializeField] private RawImage previewRawImage;  // TakingPhotoPanel �W��ܹw���� RawImage

    // TakingPhotoController.cs�]�b CapturePhotoToPreview() �����^
    public event Action OnCaptureCompleted;  // �� �s�W�G�L�Ѽƨƥ�

    [Header("Countdown")]
    [SerializeField] private Text countdownText;     // ���쭱�O�W�� Text�]�i��^
    [SerializeField] private int countdownSeconds = 5;
    // �J����첤�K
    [SerializeField] private int aftershotSeconds = 2;   // �秹�ᰱ�d��ơ]�w�] 1 ��^
    private Coroutine aftershotRoutine;                  // �M�ΡG���˼�
    private Texture2D lastPhoto;
    public Texture2D LastPhoto => lastPhoto;  // �ݭn�ɥi����
    private Coroutine countdownRoutine;

    // �� PanelFlowController �b�u�i�J TakingPhoto ���A�v�ɩI�s
    public void Enter()
    {
        if (webCam == null) return;
        // �}�۾��øj�w��
        webCam.OpenCamera(webCam.selectedDeviceName);
        if (previewRawImage != null) previewRawImage.texture = webCam.PreviewTexture;
        SetCountdownText(""); // �M�����
        //StartCaptureCountdown();
    }

    // �� PanelFlowController �b�u���} TakingPhoto ���A�v�ɩI�s
    public void Exit()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }
        if (aftershotRoutine != null) { StopCoroutine(aftershotRoutine); aftershotRoutine = null; }
        if (webCam == null) return;
        webCam.CloseCamera();
        SetCountdownText("");
    }

    // ====== �� UI ���s�Ρ]�L�ѼƾA�X Inspector �j�w�^ ======

    // ��ʶ}���Y�]�Y�A�Q�Ϋ��s����A�Ӥ��O�Ѫ��A���i���O�ɦ۰ʶ}�^
    public void UI_OnOpenPreview()
    {
        Enter();
    }
    // PanelFlowController �I�s�G�}�l�˼ơA������۰ʩ�Ө���ܩ� previewRawImage
    public void StartCaptureCountdown()
    {
        //Debug.Log("�I�sStartCaptureCountdown()");
        if (countdownRoutine != null) return;           // �����Ĳ�o
        if (webCam == null || previewRawImage == null) return;

        countdownRoutine = StartCoroutine(Co_CountdownThenCapture());
    }
    //�]�i��^���Ѩ����˼ƪ����s
    public void CancelCaptureCountdown()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
            SetCountdownText("");
        }
    }
    IEnumerator Co_CountdownThenCapture()
    {
        int t = Mathf.Max(1, countdownSeconds);
        while (t > 0)
        {
            SetCountdownText(t.ToString());
            yield return new WaitForSeconds(1f);
            t--;
        }
        SetCountdownText("");           // �M���Ʀr
        countdownRoutine = null;

        // �˼Ƶ��� �� �^�������
        CapturePhotoToPreview();
    }
    private void SetCountdownText(string s)
    {
        if (countdownText != null) countdownText.text = s;
    }
    // ���
    public void CapturePhotoToPreview()
    {
        //Debug.Log("�I�sCapturePhotoToPreview()");
        if (webCam == null || previewRawImage == null) return;

        var photo = webCam.CaptureFrame();
        if (photo == null) return;

        webCam.CloseCamera();

        if (lastPhoto != null) Destroy(lastPhoto);
        lastPhoto = photo;

        previewRawImage.texture = lastPhoto;
        SaveTextureAsJPG(lastPhoto);
        StartAftershotDelay();
    }
    public void StartAftershotDelay(int seconds = -1)
    {
        if (aftershotRoutine != null) return;                                // �����J
        int wait = seconds > 0 ? seconds : Mathf.Max(0, aftershotSeconds);
        aftershotRoutine = StartCoroutine(Co_AftershotDelay(wait));
    }

    private IEnumerator Co_AftershotDelay(int seconds)
    {
        if (seconds > 0) yield return new WaitForSeconds(seconds);
        aftershotRoutine = null;
        OnCaptureCompleted?.Invoke();                                        // �{�b�~�q���~�����U�@�B
    }
    //�]�i�d�@�ƥΡ^
    public void UI_OnCapture() => CapturePhotoToPreview();

    // ��������Y�]�Y�A�Q�Ϋ��s����^
    public void UI_OnClosePreview()
    {
        Exit();
    }
    // �x�s��J�� Texture2D �� .jpg ��
    public async void SaveTextureAsJPG(Texture2D tex)
    {
        string filePath = Path.Combine(Application.dataPath, "test.jpg");
        if (tex == null)
        {
            Debug.LogWarning("SaveTextureAsJPG: ��J�� Texture2D �� null�A�L�k�s�ɡC");
            return;
        }

        try
        {
            byte[] bytes = tex.EncodeToJPG();
            await File.WriteAllBytesAsync(filePath, bytes);
            Debug.Log($"�Ӥ��w�s�� JPG�G{filePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"�s�ɥ��ѡG{ex.Message}");
        }
    }
}
