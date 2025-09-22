using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaitingStyleController : MonoBehaviour
{
    [Header("Deps")]
    [SerializeField] private ImageStyleTransferHandler styleHandler;
    [SerializeField] private RawImage resultRawImage;

    [Header("UI")]
    [SerializeField] private Text progressText;
    [SerializeField] private ProgressCircleController progressCircleController;

    private Coroutine progressRoutine;
    private Coroutine downloadRoutine;

    private string currentTaskId;
    private bool progressCompleted;

    // �ƥ�G�~���]PanelFlowController�^�i�̳o�Ǩƥ�M�w�U�@�B
    public event Action OnProgressCompleted;        // �u�N��i�ר� 100%
    public event Action OnProgressNotCompleted;       // �s�W�G�|�������]�Х~���A�I�s BeginTracking�^
    public event Action<string> OnProgressFailed;   // �d�i�ץ���/����
    public event Action OnDownloadSucceeded;
    public event Action<string> OnDownloadFailed;

    // �Ѫ��A���b�i�J Waiting ���O��I�s�G�u�}�l�u�d�i�סv
    public void BeginTracking(string taskId)
    {
        currentTaskId = taskId;
        progressCompleted = false;

        if (progressRoutine != null) StopCoroutine(progressRoutine);
        progressRoutine = StartCoroutine(Co_TrackProgressOnly());
    }

    // �ѥ~���b�u�i�ק����v��A�I�s�G�u�t�d�U��
    public void BeginDownload()
    {
        if (!progressCompleted)
        {
            OnDownloadFailed?.Invoke("�|�������A����U��");
            return;
        }
        if (downloadRoutine != null) StopCoroutine(downloadRoutine);
        downloadRoutine = StartCoroutine(Co_DownloadOnly());
    }

    public void CancelAll()
    {
        if (progressRoutine != null) { StopCoroutine(progressRoutine); progressRoutine = null; }
        if (downloadRoutine != null) { StopCoroutine(downloadRoutine); downloadRoutine = null; }
        SetProgressText(string.Empty);
    }

    // ---------- coroutines ----------
    private int fakeProgress = 0;
    private IEnumerator Co_TrackProgressOnly()
    {
        bool completed = false;

        // �u���߶i�סA���U��
        {  //  �I�s�ˬd�ഫ�i�ת�API
            yield return styleHandler.CheckProgress(
                p =>
                {
                    // p: 0~100
                    SetProgressText($"{Mathf.RoundToInt(p)}%");
                    progressCircleController.SetByPercentage(Mathf.RoundToInt(p));
                },
                () =>
                {
                    completed = true;
                    SetProgressText("100%");
                    progressCircleController.SetByPercentage(100);
                });
        }

        //{  //  �Ω���զʤ������
        //    if (fakeProgress < 100)
        //    {
        //        SetProgressText($"{fakeProgress.ToString()}%");
        //        progressCircleController.SetByPercentage(fakeProgress);
        //        OnProgressNotCompleted?.Invoke();
        //        fakeProgress = Mathf.Clamp(fakeProgress + 29, 0, 100);
        //        yield return new WaitForSeconds(1);
        //        yield break;
        //    }
        //    else if (fakeProgress >= 100)
        //    {
        //        SetProgressText($"{100.ToString()}%");
        //        progressCircleController.SetByPercentage(100);
        //        progressRoutine = null;
        //        progressCompleted = true;
        //        OnProgressCompleted?.Invoke();
        //        fakeProgress = 0;
        //        yield return new WaitForSeconds(1);
        //        yield break;
        //    }
        //}

        // ��������
        progressRoutine = null;

        // �A������G���F 100% �N�w�R�����A���������ѡB���o�ƥ�
        if (!completed)
        {
            OnProgressNotCompleted?.Invoke();  // �� PanelFlowController �A�I�s BeginTracking
            yield break;
        }

        // �u���u�������~�аO & �q��
        progressCompleted = true;
        OnProgressCompleted?.Invoke();
    }

    private IEnumerator Co_DownloadOnly()
    {
        Texture2D result = null;

        //{  //  ���եΡA����Ū���Ϥ��é��resultRawImage
        //    string filePath = Path.Combine(Application.dataPath, "test.jpg");
        //    result = LoadJPGAsTexture(filePath);
        //    if (resultRawImage != null) resultRawImage.texture = result;
        //    OnDownloadSucceeded?.Invoke(result);
        //    yield break;
        //}

        yield return styleHandler.DownloadImage(tex => result = tex);
        downloadRoutine = null;

        if (result == null)
        {
            OnDownloadFailed?.Invoke("�U������ƹϤ�����");
            yield break;
        }

        if (resultRawImage != null) resultRawImage.texture = result;
        OnDownloadSucceeded?.Invoke();
    }

    // ---------- helpers ----------
    private void SetProgressText(string t)
    {
        if (progressText != null) progressText.text = t;
    }
    /// <summary>
    /// �q���a���|Ū�� JPG �ɡA�ഫ�� Texture2D
    /// </summary>
    /// <param name="filePath">�ɮק�����|�A�Ҧp Application.persistentDataPath + "/test.jpg"</param>
    /// <returns>���\��^ Texture2D�A���Ѫ�^ null</returns>
    public static Texture2D LoadJPGAsTexture(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            Debug.LogWarning($"LoadJPGAsTexture: �ɮפ��s�b {filePath}");
            return null;
        }

        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);

            // �إߤ@�ӪŪ� Texture2D�A�j�p�|�b LoadImage �ɦ۰ʽվ�
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
            if (tex.LoadImage(fileData))
            {
                Debug.Log($"���\���J JPG �� Texture2D: {filePath}");
                return tex;
            }
            else
            {
                Debug.LogError("LoadImage ���ѡA�ɮץi��l�a�ή榡���~");
                return null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ū�� JPG �o�Ϳ��~: {ex.Message}");
            return null;
        }
    }
}
