using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PanelFlowController : MonoBehaviour
{
    public enum FlowState
    {
        Connect,               // ConnectPanel
        Illustrate,            // IllustratePanel�]�������^
        //AuthorizationModal,    // AuthorizationDeclarationPanel�]�л\�b Illustrate �W�^
        //TakingPhoto,           // TakingPhotoPanel�]�˼ơ���ӡ^
        //ChooseStyle,           // ������
        //WaitingTransform,      // �����ഫ
        TransformFinishHint,   // �ഫ�������ܡ]�˼ƫ�i�J���֡^
        ScratchGame,           // �u��� Tex_HiddenImage + ScratchSurface
        DragPicturesHint       // ���ܥΤ�V����
    }

    [Header("Panels")]
    [SerializeField] GameObject connectPanel;
    [SerializeField] GameObject illustratePanel;
    //[SerializeField] GameObject authorizationDeclarationPanel;
    //[SerializeField] GameObject takingPhotoPanel;
    //[SerializeField] GameObject chooseStylePanel;
    //[SerializeField] GameObject waitingStyleTransformPanel;
    [SerializeField] GameObject styleTransformFinishHintPanel;
    [SerializeField] GameObject scratchSurfacePanel;    // �B�n
    [SerializeField] GameObject texHiddenImagePanel;    // ���� RawImage �Ҧb
    [SerializeField] GameObject dragPicturesHintPanel;

    //[Header("UI Refs")]
    //[SerializeField] Text takingPhotoCountdownText;     // TakingPhotoPanel �W���˼� Text

    [Header("Timings (seconds)")]
    //[SerializeField] int takingPhotoCountdown = 3;      // ��ӭ˼�
    [SerializeField] int transformFinishStaySeconds = 5;// �������ܰ��d���
    [SerializeField] int revealStaySeconds = 3;         // �Ϥ����ܫᰱ�d���

    Coroutine runningRoutine;
    public FlowState State { get; private set; } = FlowState.Connect;

    [Header("Connect deps")]
    [SerializeField] WebSocketConnectUI wsUI; // �b Inspector ����
    [Header("Controllers")]
    //[SerializeField] private TakingPhotoController takingPhotoCtrl; // ���� TakingPhotoPanel �W���s���
    //                                                        // PanelFlowController.cs
    //[SerializeField] private ChooseStyleController chooseStyleCtrl;  // �� �s�W���
    //private string pendingTaskId;

    //[SerializeField] private WaitingStyleController waitingStyleCtrl;    // ���]�s�W�^
    // pendingTaskId�G�A�b HandleStyleTaskCreated() ���w�g���s�_��
    [SerializeField] private StyleTransformFinishHintController finishHintCtrl;
    [SerializeField] private DragPicturesHintController dragPicturesHintCtrl;


    void Start()
    {
        if (wsUI != null) wsUI.OnConnectResult += HandleConnectResult;
        //if (takingPhotoCtrl != null)
        //    takingPhotoCtrl.OnCaptureCompleted += HandleCaptureCompleted; // �� �q�\�����ƥ�

        //if (chooseStyleCtrl != null)
        //{
        //    chooseStyleCtrl.OnStyleTaskCreated += HandleStyleTaskCreated; // �� ���\�G���� taskId
        //    chooseStyleCtrl.OnStyleTaskFailed += HandleStyleTaskFailed;  // �� ���ѡG�d�b�쭱�O
        //}
        //// �����ഫ�]�d�i�� / �U���^
        //if (waitingStyleCtrl != null)
        //{
        //    waitingStyleCtrl.OnProgressCompleted += HandleWaitingProgressCompleted;     // 100% �~Ĳ�o
        //    waitingStyleCtrl.OnProgressNotCompleted += HandleWaitingProgressNotCompleted;  // �|������ �� �A�d�@��
        //    waitingStyleCtrl.OnProgressFailed += HandleWaitingProgressFailed;        // �u������
        //    waitingStyleCtrl.OnDownloadSucceeded += HandleWaitingDownloadSucceeded;     // �U�����\ �� �i��������
        //    waitingStyleCtrl.OnDownloadFailed += HandleWaitingDownloadFailed;        // �U������
        //}
        if (finishHintCtrl != null)
            finishHintCtrl.OnFinishCountdown += UI_OnGoToScratchGame;
        if (dragPicturesHintCtrl != null)
            dragPicturesHintCtrl.OnDragSimulated += HandleDragSimulated;
        GoTo(FlowState.Connect);
    }

    void OnDestroy()
    {
        if (wsUI != null) wsUI.OnConnectResult -= HandleConnectResult;
        //if (takingPhotoCtrl != null)
        //    takingPhotoCtrl.OnCaptureCompleted -= HandleCaptureCompleted;

        //if (chooseStyleCtrl != null)
        //{
        //    chooseStyleCtrl.OnStyleTaskCreated -= HandleStyleTaskCreated;
        //    chooseStyleCtrl.OnStyleTaskFailed -= HandleStyleTaskFailed;
        //}

        //if (waitingStyleCtrl != null)
        //{
        //    waitingStyleCtrl.OnProgressCompleted -= HandleWaitingProgressCompleted;
        //    waitingStyleCtrl.OnProgressNotCompleted -= HandleWaitingProgressNotCompleted;
        //    waitingStyleCtrl.OnProgressFailed -= HandleWaitingProgressFailed;
        //    waitingStyleCtrl.OnDownloadSucceeded -= HandleWaitingDownloadSucceeded;
        //    waitingStyleCtrl.OnDownloadFailed -= HandleWaitingDownloadFailed;
        //    waitingStyleCtrl.CancelAll(); // �O�I�M�z��{
        //}
        if (finishHintCtrl != null)
            finishHintCtrl.OnFinishCountdown -= UI_OnGoToScratchGame;
        if (dragPicturesHintCtrl != null)
            dragPicturesHintCtrl.OnDragSimulated -= HandleDragSimulated;

    }

    // ======================
    // Public: �� UI/�t�ΩI�s
    // ======================

    // ConnectPanel�G���u�s�u�v
    public void UI_OnConnectClicked()
    {
        //Debug.Log("���U�s�u����");
        if (wsUI != null) wsUI.TryConnect();
    }

    private void HandleConnectResult(bool ok, string msg)
    {
        if (ok) GoTo(FlowState.Illustrate);
        // ���ѡGWebSocketConnectUI �|�b message Text ��ܡA�������O
    }

    //// IllustratePanel�G���uStart�v�� ���}���v�n��
    //public void UI_OnShowAuthorization()
    //{
    //    GoTo(FlowState.AuthorizationModal);
    //}

    //// AuthorizationDeclarationPanel�G���u���P�N�v
    //public void UI_OnDeclineAuthorization()
    //{
    //    // �����n���A�^�컡�����O���d
    //    authorizationDeclarationPanel.SetActive(false);
    //    GoTo(FlowState.Illustrate);
    //}

    //// AuthorizationDeclarationPanel�G���u�P�N�v
    //public void UI_OnAcceptAuthorization()
    //{
    //    GoTo(FlowState.TakingPhoto);
    //}
    //public void UI_OnTakingPhotoDone()
    //{
    //    //Debug.Log("���U��ӫ���");
    //    if (takingPhotoCtrl != null)
    //        takingPhotoCtrl.StartCaptureCountdown();  // ���������A�A�u��Ӥ���ܦb previewRawImage
    //    //GoTo(FlowState.ChooseStyle);
    //}
    //private void HandleCaptureCompleted()
    //{
    //    // �{�b�~�����e���U�@���O�]�ﭷ��^
    //    GoTo(FlowState.ChooseStyle);
    //}
    //// ChooseStyle�G�I����@����
    //public void UI_OnChooseStyle(string styleIndex)
    //{
    //    Debug.Log("���U�����ܫ���");
    //    chooseStyleCtrl.ShowResult($"�}�l���խ����ഫ...");
    //    // TODO: �i�� AI �}�l�����ഫ�]styleIndex�^
    //    chooseStyleCtrl.ChooseStyle(styleIndex);
    //}

    //// �t�Ψƥ�G�� AI �����ഫ���� & �w�U���n���~��
    //public void UI_OnFakeStyleTransformDone()
    //{
    //    // �]����A�ɹϤ��U��/��ܡ^
    //    GoTo(FlowState.TransformFinishHint);
    //}
    // IllustratePanel�G���u�}�l����v
    public void UI_OnStartExperience()
    {
        GoTo(FlowState.TransformFinishHint); // �����i�J�u����Y�N�}�l���ܡv
    }
    // StyleTransformFinishHint �� ScratchGame
    public void UI_OnGoToScratchGame()
    {
        GoTo(FlowState.ScratchGame);
    }
    // ���֤����ƥ�G����W�L 60% �� ���㴦�ܫ�I�s
    public void Sys_OnScratchRevealComplete()
    {
        Debug.Log("����Sys_OnScratchRevealComplete");
        if (runningRoutine != null) StopCoroutine(runningRoutine);
        UI_OnGoToDragHint();
        //runningRoutine = StartCoroutine(ShowRevealedThenDragHint());
    }
    public void UI_OnGoToDragHint()
    {
        Debug.Log("����UI_OnGoToDragHint");
        GoTo(FlowState.DragPicturesHint);
    }
    private void HandleDragSimulated()
    {
        Debug.Log("[PanelFlowController] ���ưʧ��� �� �^�� IllustratePanel");
        GoTo(FlowState.Illustrate);
    }

    // ======================
    // Core: ���A����
    // ======================
    void GoTo(FlowState next)
    {
        //// �Y�O�q TakingPhoto ���}�A�������Y
        //if (State == FlowState.TakingPhoto && takingPhotoCtrl != null)
        //    takingPhotoCtrl.Exit();
        State = next;
        // �Τ@����������
        SetAll(false);

        // �Y�����y�{/�˼ơA�����ɥ������ª�
        if (runningRoutine != null)
        {
            StopCoroutine(runningRoutine);
            runningRoutine = null;
        }

        switch (next)
        {
            case FlowState.Connect:
                connectPanel.SetActive(true);
                break;

            case FlowState.Illustrate:
                illustratePanel.SetActive(true);
                break;

            //case FlowState.AuthorizationModal:
            //    illustratePanel.SetActive(true);                // �I�ᤴ���
            //    authorizationDeclarationPanel.SetActive(true);  // �\�b�W��
            //    break;

            //case FlowState.TakingPhoto:
            //    takingPhotoPanel.SetActive(true);
            //    // �����˼ơA�ѱ���}���Y
            //    if (takingPhotoCtrl != null)
            //    {
            //        takingPhotoCtrl.Enter();
            //        takingPhotoCtrl.StartCaptureCountdown();
            //    }
            //    //runningRoutine = StartCoroutine(Co_TakingPhotoCountdown());
            //    break;

            //case FlowState.ChooseStyle:
            //    chooseStylePanel.SetActive(true);
            //    if (chooseStyleCtrl != null && takingPhotoCtrl != null)
            //        chooseStyleCtrl.SetSourcePhoto(takingPhotoCtrl.LastPhoto); // �� ����G���ӷ��Ӥ�
            //    break;

            //case FlowState.WaitingTransform:
            //    waitingStyleTransformPanel.SetActive(true);
            //    if (waitingStyleCtrl != null)
            //        waitingStyleCtrl.BeginTracking(pendingTaskId);
            //    // ���ݴ����ѥ~���]AI�^������A�I�s Sys_OnStyleTransformDone
            //    break;

            case FlowState.TransformFinishHint:
                styleTransformFinishHintPanel.SetActive(true);
                if (finishHintCtrl != null)
                    finishHintCtrl.BeginCountdown(transformFinishStaySeconds);
                //runningRoutine = StartCoroutine(Co_WaitThenEnterScratch());
                break;

            case FlowState.ScratchGame:
                // �u��ܩ��ϻP���־B�n
                texHiddenImagePanel.SetActive(true);
                scratchSurfacePanel.SetActive(true);
                break;

            case FlowState.DragPicturesHint:
                dragPicturesHintPanel.SetActive(true);
                texHiddenImagePanel.SetActive(true);
                break;
        }
    }

    void SetAll(bool active)
    {
        connectPanel.SetActive(active);
        illustratePanel.SetActive(active);
        //authorizationDeclarationPanel.SetActive(active);
        //takingPhotoPanel.SetActive(active);
        //chooseStylePanel.SetActive(active);
        //waitingStyleTransformPanel.SetActive(active);
        styleTransformFinishHintPanel.SetActive(active);
        scratchSurfacePanel.SetActive(active);
        texHiddenImagePanel.SetActive(active);
        dragPicturesHintPanel.SetActive(active);
    }

    // ======================
    // Routines
    // ======================
    //IEnumerator Co_TakingPhotoCountdown()
    //{
    //    int t = Mathf.Max(1, takingPhotoCountdown);
    //    while (t > 0)
    //    {
    //        if (takingPhotoCountdownText) takingPhotoCountdownText.text = t.ToString();
    //        yield return new WaitForSeconds(1f);
    //        t--;
    //    }

    //    // TODO: �o��Ĳ�o�u������޿�]�֪�/�^���^
    //    // �秹�i�J�ﭷ��
    //    GoTo(FlowState.ChooseStyle);
    //}

    IEnumerator Co_WaitThenEnterScratch()
    {
        yield return new WaitForSeconds(Mathf.Max(0, transformFinishStaySeconds));
        // �i�J���֡G�u�}�ҩ��� + �B�n
        GoTo(FlowState.ScratchGame);
    }

    IEnumerator ShowRevealedThenDragHint()
    {
        // �Ϥ��w���ܡG�u�Ȯi��
        yield return new WaitForSeconds(Mathf.Max(0, revealStaySeconds));
        GoTo(FlowState.DragPicturesHint);
    }
    //private void HandleStyleTaskCreated(string taskId)
    //{
    //    Debug.Log("�I�sHandleStyleTaskCreated()");
    //    pendingTaskId = taskId;           // ���s�_��

    //    // ��ܰT���]�i��^
    //    chooseStyleCtrl.ShowResult("������ȫإߦ��\�A���b���ݦ��A���B�z�K");
    //    GoTo(FlowState.WaitingTransform); // �� ���\�~���쵥�ݭ��O
    //                                      // ����A���F WaitingStyleController�A�A�b���� taskId �ǵ��� BeginTracking(taskId)
    //}

    //private void HandleStyleTaskFailed(string reason)
    //{
    //    Debug.Log("�I�sHandleStyleTaskFailed()");
    //    Debug.LogError($"Style task create failed: {reason}");
    //    // �d�b ChooseStyle ���O�A�Φb���O�W��ܿ��~�r��
    //    // ��ܿ��~�T���A���d�b ChooseStylePanel
    //    chooseStyleCtrl.ShowResult($"���ȫإߥ��ѡG{reason}");

    //    //GoTo(FlowState.WaitingTransform); // ���եΡA�Y�ϥ��Ѥ]�i�U�@�B
    //}

    //// ���ݭ��O�G�i�� = 100% �� �i�J�U�����q
    //private void HandleWaitingProgressCompleted()
    //{
    //    if (waitingStyleCtrl != null)
    //        waitingStyleCtrl.BeginDownload();
    //}

    //// ���ݭ��O�G�i�ץ����� �� �A�d�@���]�i�[�L�����קK�L�W�^
    //private void HandleWaitingProgressNotCompleted()
    //{
    //    StartCoroutine(RequeryAfterDelay(0.5f));
    //}
    //private IEnumerator RequeryAfterDelay(float seconds)
    //{
    //    yield return new WaitForSeconds(seconds);
    //    if (State == FlowState.WaitingTransform && waitingStyleCtrl != null)
    //        waitingStyleCtrl.BeginTracking(pendingTaskId);
    //}

    //// ���ݭ��O�G�d�i�ׯu������
    //private void HandleWaitingProgressFailed(string reason)
    //{
    //    Debug.LogError($"Progress failed: {reason}");
    //    // ���ݨD�G���d�b Waiting ��ܿ��~�A�ΰh�^ ChooseStyle
    //}

    //// ���ݭ��O�G�U�����\ �� �i��������
    //private void HandleWaitingDownloadSucceeded()
    //{
    //    GoTo(FlowState.TransformFinishHint);
    //}

    //// ���ݭ��O�G�U������
    //private void HandleWaitingDownloadFailed(string reason)
    //{
    //    Debug.LogError($"Download failed: {reason}");
    //    // ���ݨD�B�z�]���d�ΰh�^�^
    //}
}
