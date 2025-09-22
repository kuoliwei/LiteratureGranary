using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandReferenceDotSpawner : MonoBehaviour
{
    [Header("UI �P Prefab �]�w")]
    [Tooltip("�n�\����I��������]�q�`�O�Y�� Canvas ���U���@�ӪŪ� RectTransform �e���^�C")]
    public RectTransform container;   // ��ĳ���b Canvas �U���@�ӪŪ���
    [Tooltip("���I�� UI Image �w�m��]��ĳ 24~32 px ����ΡAColor �]����b�z���^�C")]
    public Image dotPrefab;

    [Header("�۰ʲM��")]
    [Tooltip("�h�[�S��s�N�۰ʲM�š]��^")]
    public float noUpdateThreshold = 0.5f;

    private readonly List<Image> _dots = new();
    private float _noUpdateTimer = 0f;
    Canvas _canvas;
    private RectTransform uiRectTransform;
    // [NEW] �Τ@�ѥ~�����w UV �������ؼ� UI
    public void SetTargetRectTransform(RectTransform rt) // [NEW]
    {
        uiRectTransform = rt; // [NEW]
    }

    void Awake()
    {
        if (container == null)
        {
            Debug.LogError("[HandReferenceDotSpawner] container �����w�I");
            enabled = false;
            return;
        }
        _canvas = container.GetComponentInParent<Canvas>();
        if (_canvas == null)
        {
            Debug.LogError("[HandReferenceDotSpawner] �䤣��W�h Canvas�I");
            enabled = false;
            return;
        }
        if (dotPrefab == null)
        {
            Debug.LogError("[HandReferenceDotSpawner] dotPrefab �����w�I");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        _noUpdateTimer += Time.deltaTime;
        if (_noUpdateTimer > noUpdateThreshold)
        {
            ClearAll();
        }
    }

    /// <summary>
    /// �N���I�P�B��h�ӿù��y�С]�����^�C�@�Ӯy�Ф@�Ӭ��I�C
    /// </summary>
    public void SyncDotsToScreenPositions(List<Vector2> screenPosList)
    {
        _noUpdateTimer = 0f;

        // �ƶq����]�����N�ɡB�Ӧh�N���^
        while (_dots.Count < screenPosList.Count)
        {
            var img = Instantiate(dotPrefab, container);
            img.gameObject.SetActive(true);
            _dots.Add(img);
        }
        for (int i = _dots.Count - 1; i >= screenPosList.Count; i--)
        {
            Destroy(_dots[i].gameObject);
            _dots.RemoveAt(i);
        }

        // �ù��y�� �� �e�� localPosition
        var cam = (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : _canvas.worldCamera;
        for (int i = 0; i < screenPosList.Count; i++)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                container, screenPosList[i], cam, out Vector2 localPos))
            {
                _dots[i].rectTransform.anchoredPosition = localPos;
            }
        }
    }
    // ========================================================================
    // [NEW] �H�u���a��� uiRectTransform �� UV�]0..1�^�v���\����I�C
    //       uv (0,0)=���U�F(1,1)=�k�W�C�u�έ쪩���ƶq���޼g�k�]�����ɡB�W�X�R�^�C
    //       ���I�̲ץͦ��b this.container ���U�C
    // ========================================================================
    /// <summary>�H uiRectTransform �� UV�]0..1�^�y���\����I�C</summary> // [NEW]
    /// <param name="uvList">UV �M��A(0,0)=���U, (1,1)=�k�W�C</param> // [NEW]
    /// <param name="clamp01">�O�_�N UV ���� [0,1] �d��C</param> // [NEW]
    public void SyncDotsToUVPositions(List<Vector2> uvList, bool clamp01, int heatedUvIndex, float heatedHoldTimesPercentage) // [NEW]
    { // [NEW]
        _noUpdateTimer = 0f; // [NEW]

        if (uiRectTransform == null) // [NEW]
        { // [NEW]
            Debug.LogError("[HandReferenceDotSpawner] uiRectTransform �����w�A�L�k�H UV �������I��m�C"); // [NEW]
            return; // [NEW]
        } // [NEW]

        // �ƶq����]�����N�ɡB�Ӧh�N���^�X�X�u�έ쪩���� // [NEW]
        while (_dots.Count < uvList.Count) // [NEW]
        { // [NEW]
            var img = Instantiate(dotPrefab, container); // [NEW]
            img.gameObject.SetActive(true);              // [NEW]
            _dots.Add(img);                              // [NEW]
        } // [NEW]
        for (int i = _dots.Count - 1; i >= uvList.Count; i--) // [NEW]
        { // [NEW]
            Destroy(_dots[i].gameObject); // [NEW]
            _dots.RemoveAt(i);            // [NEW]
        } // [NEW]
          // �w����s�i�װ�G�u���R���������]���ʤ���A��L�k 0�]�קK�W�@�V�ݯd�^
        for (int i = 0; i < _dots.Count; i++)
        {
            var pc = _dots[i]?.GetComponentInChildren<ProgressCircleController>(true);
            if (pc == null) continue;

            if (heatedUvIndex >= 0 && heatedUvIndex < _dots.Count && i == heatedUvIndex)
            {
                pc.SetByPercentage(heatedHoldTimesPercentage); // 0..100
            }
            else
            {
                pc.SetByPercentage(0f);
            }
        }
        var cam = (_canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : _canvas.worldCamera; // [NEW]

        for (int i = 0; i < uvList.Count; i++) // [NEW]
        { // [NEW]
            Vector2 uv = uvList[i]; // [NEW]
            if (clamp01) { uv.x = Mathf.Clamp01(uv.x); uv.y = Mathf.Clamp01(uv.y); } // [NEW]

            // Step 1: UV �� uiRectTransform ���a�y�С]�Ҽ{ pivot�^ // [NEW]
            // local = (uv * size) - (pivot * size)                                     // [NEW]
            Rect r = uiRectTransform.rect;                                             // [NEW]
            Vector2 size = r.size;                                                     // [NEW]
            Vector2 pivot = uiRectTransform.pivot;                                     // [NEW]
            Vector2 localInTarget = new Vector2(                                       // [NEW]
                uv.x * size.x - pivot.x * size.x,                                      // [NEW]
                uv.y * size.y - pivot.y * size.y                                       // [NEW]
            );                                                                          // [NEW]

            // Step 2: target ���a�y�� �� �@�ɮy�� // [NEW]
            Vector3 world = uiRectTransform.TransformPoint(localInTarget); // [NEW]

            // Step 3: �@�ɮy�� �� �ù��y�� // [NEW]
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, world); // [NEW]

            // Step 4: �ù��y�� �� container ���a�y�� // [NEW]
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle( // [NEW]
                container, screen, cam, out Vector2 localInContainer))   // [NEW]
            { // [NEW]
                _dots[i].rectTransform.anchoredPosition = localInContainer; // [NEW]
            } // [NEW]
        } // [NEW]
    } // [NEW]
    /// <summary>
    /// �ߧY�M�ũҦ����I�C
    /// </summary>
    public void ClearAll()
    {
        for (int i = 0; i < _dots.Count; i++)
        {
            if (_dots[i] != null)
                Destroy(_dots[i].gameObject);
        }
        _dots.Clear();
        _noUpdateTimer = 0f;
    }
}
