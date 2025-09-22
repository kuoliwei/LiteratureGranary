using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandParticleEffectSpawner : MonoBehaviour
{
    public GameObject particlePrefab;
    //public RectTransform canvasRect;

    //private List<GameObject> currentEffects = new List<GameObject>();
    private Dictionary<Canvas, List<GameObject>> effectsByCanvas = new();
    private float noUpdateTimer = 0f;
    private float noUpdateThreshold = 0.1f; // 0.5��L��s�N�۰ʲM��
    // =========================
    // UV �����ݭn�����
    // =========================
    [Header("UV �����]�w")]                             // [NEW]
    private RectTransform uiRectTransform; // [NEW] UV �ҹ������������� UI�]InteractiveQuad�^
    private Canvas targetCanvas;           // [NEW] �n��S�ĥͦb���� Canvas ���U�]�Y�ŷ|�۰ʧ�^
    void Syart() // [NEW]
    {
        // �Y�����w Canvas�A�q uiRectTransform ���W��
        if (targetCanvas == null && uiRectTransform != null) // [NEW]
            targetCanvas = uiRectTransform.GetComponentInParent<Canvas>(); // [NEW]
    }
    /// <summary>
    /// ���~���ǤJ�h�ծy�СA�C�ծy�з|���@�ӹ����S��
    /// </summary>
    public void SyncParticlesToScreenPositions(Canvas canvas, List<Vector2> screenPosList)
    {
        noUpdateTimer = 0f;

        if (!effectsByCanvas.ContainsKey(canvas))
        {
            effectsByCanvas[canvas] = new List<GameObject>();
        }

        var list = effectsByCanvas[canvas];

        // �ɨ�
        while (list.Count < screenPosList.Count)
        {
            var go = Instantiate(particlePrefab, canvas.transform);
            var ps = go.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.maxParticles = 1000;
            }
            list.Add(go);
        }

        // �M���h��
        while (list.Count > screenPosList.Count)
        {
            int last = list.Count - 1;
            Destroy(list[last]);
            list.RemoveAt(last);
        }

        // �ഫ��m
        var canvasRect = canvas.GetComponent<RectTransform>();
        Camera cam = canvas.worldCamera;

        for (int i = 0; i < screenPosList.Count; i++)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPosList[i], cam, out Vector2 localPos))
            {
                list[i].transform.localPosition = new Vector3(localPos.x, localPos.y, -500f);
            }
        }
    }

    // =========================================================
    // [NEW] UV �����GInteractiveQuad �γo��
    //      uv (0,0)=���U, (1,1)=�k�W�F�|�� uiRectTransform �� UV ��v�� targetCanvas
    // =========================================================
    public void SyncParticlesToUVPositions(List<Vector2> uvList, bool clamp01 = true) // [NEW]
    {
        noUpdateTimer = 0f; // [NEW]
        if (uiRectTransform == null) { Debug.LogWarning("[HandParticleEffectSpawner] uiRectTransform �����w"); return; } // [NEW]

        // ��� Canvas
        var canvas = targetCanvas != null ? targetCanvas : uiRectTransform.GetComponentInParent<Canvas>(); // [NEW]
        if (canvas == null) { Debug.LogWarning("[HandParticleEffectSpawner] �䤣�� Canvas"); return; }    // [NEW]

        if (!effectsByCanvas.ContainsKey(canvas)) // [NEW]
            effectsByCanvas[canvas] = new List<GameObject>(); // [NEW]
        var list = effectsByCanvas[canvas]; // [NEW]

        // �ƶq���ޡG�����ɡB�W�X�R // [NEW]
        while (list.Count < uvList.Count) // [NEW]
        {
            var go = Instantiate(particlePrefab, canvas.transform); // [NEW]
            var ps = go.GetComponent<ParticleSystem>();             // [NEW]
            if (ps != null)                                         // [NEW]
            {                                                       // [NEW]
                var main = ps.main;                                  // [NEW]
                main.maxParticles = 1000;                            // [NEW]
            }                                                       // [NEW]
            list.Add(go);                                           // [NEW]
        }                                                           // [NEW]
        while (list.Count > uvList.Count)                           // [NEW]
        {                                                           // [NEW]
            int last = list.Count - 1;                              // [NEW]
            Destroy(list[last]);                                    // [NEW]
            list.RemoveAt(last);                                    // [NEW]
        }                                                           // [NEW]

        // UV �� local(canvas) // [NEW]
        var canvasRect = canvas.GetComponent<RectTransform>(); // [NEW]
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : (canvas.worldCamera != null ? canvas.worldCamera : Camera.main); // [NEW]

        Rect r = uiRectTransform.rect;   // [NEW]
        Vector2 size = r.size;           // [NEW]
        Vector2 pivot = uiRectTransform.pivot; // [NEW]

        for (int i = 0; i < uvList.Count; i++) // [NEW]
        {
            Vector2 uv = uvList[i];            // [NEW]
            if (clamp01) { uv.x = Mathf.Clamp01(uv.x); uv.y = Mathf.Clamp01(uv.y); } // [NEW]

            // UV �� target local�]�Ҽ{ pivot�^ // [NEW]
            Vector2 localInTarget = new Vector2(uv.x * size.x - pivot.x * size.x,
                                                uv.y * size.y - pivot.y * size.y);  // [NEW]
            // target local �� world // [NEW]
            Vector3 world = uiRectTransform.TransformPoint(localInTarget);           // [NEW]
            // world �� screen�]�� Canvas �۾��^ // [NEW]
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, world);    // [NEW]
            // screen �� canvas local // [NEW]
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, cam, out Vector2 localOnCanvas)) // [NEW]
            {
                list[i].transform.localPosition = new Vector3(localOnCanvas.x, localOnCanvas.y, -500f); // [NEW]
            }
        }
    }

    /// <summary>
    /// �S�����󦳮Įy�ЮɩI�s�A�����^��
    /// </summary>
    public void ClearAll()
    {
        foreach (var kv in effectsByCanvas)
        {
            foreach (var go in kv.Value)
            {
                StartCoroutine(DelayedParticleDestroy(go));
            }
        }

        effectsByCanvas.Clear();
    }
    private IEnumerator DelayedParticleDestroy(GameObject go)
    {
        var ps = go.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // �]�w�̤j�ɤl�Ƭ�0
            var main = ps.main;
            main.maxParticles = 0;
        }
        // ��1��
        yield return new WaitForSeconds(3f);

        Destroy(go);
    }
    private void Update()
    {
        noUpdateTimer += Time.deltaTime;
        if (noUpdateTimer > noUpdateThreshold)
        {
            ClearAll();
        }
    }
    // ===== �i��G�ѥ~���Τ@���w ===== // [NEW]
    public void SetTargets(RectTransform ui, Canvas canvas = null)  // [NEW]
    {
        uiRectTransform = ui;                                       // [NEW]
        targetCanvas = canvas != null ? canvas : (ui != null ? ui.GetComponentInParent<Canvas>() : null); // [NEW]
    }
}
