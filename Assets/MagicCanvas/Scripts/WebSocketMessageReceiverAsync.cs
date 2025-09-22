using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

public class WebSocketMessageReceiverAsync : MonoBehaviour
{
    [Header("�s�� UI ����")]
    [SerializeField] private WebSocketConnectUI webSocketConnectUI;
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private ReconnectPanelController reconnectUI;

    [Header("WebSocket �Ȥ��")]
    [SerializeField] private WebSocketClient webSocketClient;

    [Header("�O�_���\�B�z Brush ���")]
    public bool CanReceiveBrushMessage = true;

    [System.Serializable]
    public class BrushUpdateEvent : UnityEvent<List<BrushData>> { }
    [Header("���� Brush ��ƨƥ�")]
    public BrushUpdateEvent OnBrushDataReceived = new();
    private readonly ConcurrentQueue<List<BrushData>> mainThreadQueue = new();
    private int messagesProcessedThisSecond = 0;
    private float messageTimer = 0f;

    [Header("�O�_���\�B�z Pose ���")]
    public bool CanReceivePoseMessage = true;

    [System.Serializable]
    public class PoseFrameEvent : UnityEvent<PoseTypes.FrameSample> { }

    [Header("���� Pose ��ƨƥ�")]
    public PoseFrameEvent OnPoseFrameReceived = new();

    private readonly ConcurrentQueue<PoseTypes.FrameSample> poseMainThreadQueue = new();

    private void Start()
    {
        if (webSocketClient != null)
        {
            webSocketClient.OnMessageReceive.AddListener(message =>
            {
                //ReceiveMessage(message);    //  ����uv�y��
                ReceiveSkeletonMessage(message);    //  �������[���
            });
            webSocketClient.OnConnected.AddListener(OnWebSocketConnected);
            webSocketClient.OnConnectionError.AddListener(() =>
            {
                webSocketConnectUI.OnConnectionFaild("�s�u����");
            });
            webSocketClient.OnDisconnected.AddListener(OnWebSocketDisconnected);
        }
    }

    private void Update()
    {
        //if (Input.GetMouseButton(0))
        //{
        //    // 1. �إ߼��� BrushData �� JSON �r��
        //    Vector2 mousePos = Input.mousePosition;
        //    Vector2 normalized = new Vector2(
        //        mousePos.x / Screen.width,
        //        1f - (mousePos.y / Screen.height)
        //    );

        //    string json = $"{{\"data\":[{{\"roller_id\":0,\"point\":[{normalized.x},{normalized.y}]}}]}}";
        //    Debug.Log("normalized.y :" + normalized.y);
        //    // 2. �I�s WebSocketMessageReceiverAsync �B�z JSON
        //    SendMessageManually(json);

        //    //scratchCount++;
        //}
        int processed = 0;
        {//����C�V��Ƶ���
            //int maxProcessPerFrame = 128;
            //while (processed < maxProcessPerFrame && mainThreadQueue.TryDequeue(out var brushList))
            //{
            //    OnBrushDataReceived.Invoke(brushList);

            //    processed++;
            //}
        }

        {//������C�V��Ƶ���
            while (mainThreadQueue.TryDequeue(out var brushList))
            {
                OnBrushDataReceived.Invoke(brushList);

                processed++;
            }
        }
        // ���X Pose �ƥ�]������ơ^
        {
            int processedPose = 0;
            while (poseMainThreadQueue.TryDequeue(out var frame))
            {
                OnPoseFrameReceived.Invoke(frame);
                processedPose++;
            }
            //Debug.Log($"���V�B�z{processedPose}��frame���");
            // �ݭn�ʱ��ɥi�L�X processedPose
        }
        // �ʱ�
        messageTimer += Time.deltaTime;
        if (messageTimer >= 1f)
        {
            //Debug.Log($"[�ʱ�] ���V�B�z {processed} �� brushList�CQueue �Ѿl�G{mainThreadQueue.Count}");
            messageTimer = 0f;
        }
    }

    private void ReceiveMessage(string messageContent)
    {
        //Debug.Log("[ReceiveMessage] �禡���Q�I�s");

        if (!CanReceiveBrushMessage)
        {
            Debug.LogWarning("�����T���GCanReceiveBrushMessage �� false");
            return;
        }

        //Debug.Log($"[ReceiveMessage] ����T�����e�G{messageContent}");

        try
        {
            var newBrushMessage = JsonConvert.DeserializeObject<BrushPositionJson>(messageContent);
            if (newBrushMessage == null)
            {
                Debug.LogError("[ReceiveMessage] JSON �ѪR���ѡI");
                return;
            }

            if (newBrushMessage.data == null || newBrushMessage.data.Count == 0)
            {
                Debug.LogWarning("[ReceiveMessage] JSON �ѪR���\�� data ����");
                return;
            }

            //Debug.Log($"[ReceiveMessage] ���\�ѪR�A�@ {newBrushMessage.data.Count} �� BrushData");
            mainThreadQueue.Enqueue(newBrushMessage.data);
        }
        catch (Exception e)
        {
            Debug.LogError($"Can't deserialize message: {messageContent}. Error: {e.Message}");
        }
    }
    private void ReceiveSkeletonMessage(string messageContent)
    {
        //Debug.Log("[ReceiveMessage] �禡���Q�I�s (Pose)");

        if (!CanReceivePoseMessage)
        {
            Debug.LogWarning("�����T���GCanReceivePoseMessage �� false");
            return;
        }

        try
        {
            // �榡: { "<frameIndex>": [ [ [x,y,z,conf],...17�I ],  [ ...�H��1... ] ] }
            var root = JObject.Parse(messageContent);

            // ���\�@���u�e�@�� frame�]�`���^�A�N��Ĥ@���ݩ�
            var enumerator = root.Properties().GetEnumerator();
            if (!enumerator.MoveNext())
            {
                Debug.LogWarning("[ReceiveMessage] JSON �ѪR���\���S������ frame key");
                return;
            }

            var frameProp = enumerator.Current;
            if (!int.TryParse(frameProp.Name, out int frameIndex))
            {
                Debug.LogError($"[ReceiveMessage] frame key �L�k�ন int: {frameProp.Name}");
                return;
            }

            var personsArray = frameProp.Value as JArray;
            if (personsArray == null)
            {
                Debug.LogError("[ReceiveMessage] frame value ���O�}�C (persons)");
                return;
            }

            var frame = new PoseTypes.FrameSample(frameIndex);

            for (int personId = 0; personId < personsArray.Count; personId++)
            {
                var personJoints = personsArray[personId] as JArray;
                if (personJoints == null)
                {
                    Debug.LogWarning($"[ReceiveMessage] �H�� {personId} joints ���O�}�C�A���L");
                    continue;
                }

                var person = new PoseTypes.PersonSkeleton();

                // ����T�w 17 �����`
                int jointCount = Math.Min(personJoints.Count, PoseTypes.PoseSchema.JointCount);
                for (int j = 0; j < jointCount; j++)
                {
                    var jArr = personJoints[j] as JArray;
                    if (jArr == null || jArr.Count < 4)
                    {
                        // ���߿��A���w�] 0
                        continue;
                    }

                    float x = jArr[0]!.Value<float>();
                    float y = jArr[1]!.Value<float>();
                    float z = jArr[2]!.Value<float>();
                    float conf = jArr[3]!.Value<float>();

                    person.joints[j] = new PoseTypes.Joint(x, y, z, conf);
                }

                frame.persons.Add(person);
            }

            // ���D������ƥ��C
            poseMainThreadQueue.Enqueue(frame);
            //Debug.Log($"[ReceiveMessage] Pose �ѪR���\�Gframe={frameIndex}, persons={frame.persons.Count}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[ReceiveMessage] �ѪR Pose JSON ���ѡCError: {e.Message}\n���e: {messageContent}");
        }
    }

    private void OnWebSocketDisconnected()
    {
        Debug.Log("�I�sOnWebSocketDisconnected()");
        if (!connectPanel.activeSelf)
        {
            reconnectUI?.ShowFlicker();
            webSocketClient.allowReconnect = true;
            webSocketClient.isReconnectAttempt = true;
            Debug.Log("���u���A�۰ʱҥέ��s����");
        }
        else
        {
            Debug.Log("ConnectPanel �}�Ҥ��A���۰ʭ��s");
        }
    }

    private void OnWebSocketConnected()
    {
        //Debug.Log("�I�sOnWebSocketConnected()");
        reconnectUI?.ShowSuccessAndHide();

        //if (connectPanel != null)
        //{
        //    connectPanel.SetActive(false);
        //}
        webSocketConnectUI?.OnConnectionSucceeded();
        webSocketClient.allowReconnect = false;

        if (webSocketClient.isReconnectAttempt)
        {
            Debug.Log("���s�s�u���\");
            webSocketClient.isReconnectAttempt = false;
        }
    }

    /// <summary>
    /// ���� UI ��J�s�u��T�����f
    /// </summary>
    public void ConnectToServer(string ip, string port)
    {
        //Debug.Log("�I�sConnectToServer()");
        string address = $"ws://{ip}:{port}";
        //Debug.Log($"[WebSocketReceiverAsync] Connecting to: {address}");

        webSocketClient.CloseConnection();
        webSocketClient.StartConnection(address);
    }

    public void SendMessageManually(string json)
    {
        Debug.Log($"[Receiver] SendMessageManually �I�s��");

        if (!CanReceiveBrushMessage)
        {
            //Debug.LogWarning("[Receiver] �����T���GCanReceiveBrushMessage �� false");
            return;
        }

        ReceiveMessage(json);
    }
}

// class BrushPositionJson { public List<BrushData> data; }
// class BrushData { public int roller_id; public List<float> point; }
