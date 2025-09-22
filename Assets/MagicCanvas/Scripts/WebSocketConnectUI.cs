using UnityEngine;
using UnityEngine.UI;
using System.Net;
using System;

public class WebSocketConnectUI : MonoBehaviour
{
    [Header("UI ����")]
    public Text message;
    public GameObject connectPanel;
    public InputField ipInput;
    public InputField portInput;
    public Button connectButton;

    private string ip = "127.0.0.1";
    private string port = "9999";
    //private string ip = "10.66.66.57";
    //private string port = "8765";
    //private string ip = "10.66.66.51";
    //private string port = "8765";
    [Header("�s�u������")]
    public WebSocketMessageReceiverAsync receiver;
    public event Action<bool, string> OnConnectResult; // true=���\�Ffalse=���ѡFstring=�T��
    private void Start()
    {
        // �Y�n�w�]�i��b�o�̡]�ثe�w���ѡ^
        ipInput.text = ip;
        portInput.text = port;

        //connectButton.onClick.AddListener(OnClickConnect);
    }
    // �� PanelFlowController �I�s
    public void TryConnect()
    {
        //Debug.Log("�I�sTryConnect()");
        message.text = "";
        //string ip = this.ip;
        //string portText = this.port;
        string ip = ipInput.text;
        string portText = portInput.text;

        if (!System.Net.IPAddress.TryParse(ip, out _))
        {
            message.text += "IP �榡�����T\n";
            OnConnectResult?.Invoke(false, "IP �榡�����T");
            return;
        }
        if (!int.TryParse(portText, out int port) || port < 1 || port > 65535)
        {
            message.text += "Port �榡�����T�]���Ľd��G1~65535�^";
            OnConnectResult?.Invoke(false, "Port �榡�����T");
            return;
        }

        // �浹�������h�s�u�F���\/���ѽЦ^�I��U���Ӥ�k
        receiver.ConnectToServer(ip, portText);
    }
    // ���������b�u�s�u���\�v�ɩI�s
    public void OnConnectionSucceeded()
    {
        //Debug.Log("�I�sOnConnectionSucceeded()");
        message.text = "�s�u���\";
        OnConnectResult?.Invoke(true, "�s�u���\");
    }
    // �]�쥻�N�����^�s�u���Ѧ^�I�G�X�R���ƥ�^��
    public void OnConnectionFaild(string reason = "�s�u����")
    {
        Debug.Log("�I�sOnConnectionFaild()");
        if (connectPanel.activeSelf)
        {
            message.text = reason;
        }
        OnConnectResult?.Invoke(false, reason);
    }
    public void OnInputFieldValueChanged()
    {
        ip = ipInput.text;
        port = portInput.text;
    }
}
