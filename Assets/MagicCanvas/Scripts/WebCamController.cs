using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class WebCamController : MonoBehaviour
{
    [SerializeField] private Dropdown dropdown;
    [SerializeField] private ImageStyleTransferHandler imageStyleTransferHandler;
    private WebCamTexture webCamTexture;
    public string selectedDeviceName => dropdown.options[dropdown.value].text;
    public WebCamTexture PreviewTexture => webCamTexture; // ���~���j�� RawImage.texture
    void Start()
    {
        SetDropdoenOptions(GetDevices(), dropdown);
    }
    List<string> GetDevices()
    {
        List<string> names = new List<string>();
        foreach (WebCamDevice device in WebCamTexture.devices)
        {
            names.Add(device.name);
        }
        return names;
    }
    void SetDropdoenOptions(List<string> deviceNeams, Dropdown dropdown)
    {
        dropdown.ClearOptions();
        List<Dropdown.OptionData> optionDatas = new List<Dropdown.OptionData>();
        foreach (string name in deviceNeams)
        {
            optionDatas.Add(new Dropdown.OptionData(name));
        }
        dropdown.AddOptions(optionDatas);
    }
    //public void OnDropdownValueChange()
    //{
    //    ActivateDevice(dropdown.options[dropdown.value].text, displayImage);
    //    Debug.Log($"Webcam {dropdown.options[dropdown.value].text} has been activated.");
    //}
    //void ActivateDevice(string deviceName, RawImage displayImage)
    //{
    //    try
    //    {
    //        if (webCamTexture != null && webCamTexture.isPlaying && webCamTexture.deviceName == deviceName)
    //        {
    //            Debug.Log("�w�g�b�ϥΦ���v���G" + deviceName);
    //            return;
    //        }
    //        if (webCamTexture.isPlaying || webCamTexture != null)
    //        {
    //            webCamTexture.Stop();
    //            Destroy(webCamTexture);
    //            webCamTexture = null;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.Log(ex);
    //    }

    //    webCamTexture = new WebCamTexture(deviceName);
    //    displayImage.texture = webCamTexture;
    //    webCamTexture.Play();
    //}
    // �}�۾��]�i�� deviceName�A���ǫh�βĤ@�ӡ^
    public void OpenCamera(string selectedDeviceName)
    {
        Debug.Log("�I�sOpenCamera()");
        // �Y�w�b�P�@�x�˸m�W����A�����ƶ}
        if (webCamTexture != null && webCamTexture.isPlaying &&
            (string.IsNullOrEmpty(selectedDeviceName) || webCamTexture.deviceName == selectedDeviceName)) return;

        if (webCamTexture != null && webCamTexture.isPlaying) webCamTexture.Stop();

        if (string.IsNullOrEmpty(selectedDeviceName))
        {
            var devices = WebCamTexture.devices;
            if (devices.Length == 0) { Debug.LogError("No webcam found."); return; }
        }

        webCamTexture = new WebCamTexture(selectedDeviceName, 1920, 1080);
        webCamTexture.Play();
    }
    //public void OpenCamera(string deviceName = null, int width = 1280, int height = 720, int fps = 30)
    //{
    //    // �Y�w�b�P�@�x�˸m�W����A�����ƶ}
    //    if (webCamTexture != null && webCamTexture.isPlaying &&
    //        (string.IsNullOrEmpty(deviceName) || webCamTexture.deviceName == deviceName)) return;

    //    if (webCamTexture != null && webCamTexture.isPlaying) webCamTexture.Stop();

    //    if (string.IsNullOrEmpty(deviceName))
    //    {
    //        var devices = WebCamTexture.devices;
    //        if (devices.Length == 0) { Debug.LogError("No webcam found."); return; }
    //        deviceName = devices[0].name;
    //    }

    //    webCamTexture = new WebCamTexture(deviceName, width, height, fps);
    //    webCamTexture.Play();
    //}

    public void CloseCamera()
    {
        Debug.Log("�I�sCloseCamera()");
        if (webCamTexture != null && webCamTexture.isPlaying) webCamTexture.Stop();
    }

    // �P�B���o�ثe�@�i�v���]Texture2D�^
    public Texture2D CaptureFrame()
    {
        Debug.Log("�I�sCaptureFrame()");
        if (webCamTexture == null || !webCamTexture.isPlaying) return null;
        var tex = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
        tex.SetPixels(webCamTexture.GetPixels());
        tex.Apply();
        return tex;
    }

    // �@���s�G��ӡ��W�ǡ����ߡ��U���F���G�z�L�^�I�^�� Texture2D
    public IEnumerator CaptureAndStylize(string style, Action<Texture2D> onDone, int jpgQuality = 85)
    {
        if (webCamTexture == null || !webCamTexture.isPlaying)
        {
            Debug.LogWarning("Camera not running.");
            onDone?.Invoke(null);
            yield break;
        }

        Texture2D photo = null;
        try
        {
            // 1) ���
            photo = CaptureFrame();
            if (photo == null) { onDone?.Invoke(null); yield break; }

            byte[] bytes = photo.EncodeToJPG(Mathf.Clamp(jpgQuality, 1, 100));
            string image64 = Convert.ToBase64String(bytes);

            // 2) �e�ШD
            bool ok = false;
            yield return imageStyleTransferHandler.SendStyleRequest(image64, style, tid =>
            {
                ok = !string.IsNullOrEmpty(tid);
            });
            if (!ok) { onDone?.Invoke(null); yield break; }

            // 3) ���߶i��
            yield return imageStyleTransferHandler.CheckProgress(_ => { }, () => { });

            // 4) �U���Ϥ�
            Texture2D result = null;
            yield return imageStyleTransferHandler.DownloadImage(tex => result = tex);
            onDone?.Invoke(result);
        }
        finally
        {
            if (photo != null) Destroy(photo);  // Unity ����� Destroy ����
        }
    }

}
