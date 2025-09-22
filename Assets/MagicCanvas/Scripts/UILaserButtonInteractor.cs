using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILaserButtonInteractor : MonoBehaviour
{
    [Header("UI Raycast")]
    [SerializeField] private GraphicRaycaster raycaster;
    [SerializeField] private EventSystem eventSystem;

    [Header("���ʳ]�w")]
    [Tooltip("�R���P�@�����s�ֿn�h�[�~Ĳ�o onClick�]��^")]
    [SerializeField] private float holdTimeToClick;
    [SerializeField] private bool autoClickOnHit = true;

    // [NEW] �S�Q�R�����e����ơ]�W�L�~�M�s�^
    [SerializeField] private float missGrace;// 0.x ��A�ӻݨD��

    // [NEW] �����C�����s�̫�@���Q�R�����ɶ�
    private readonly Dictionary<Button, float> firstHitAt = new();
    private readonly Dictionary<Button, float> lastHitAt = new();
    private readonly Dictionary<Button, float> holdTimers = new(); // �P�V/�s��V�ֿn
    private readonly HashSet<Button> hitThisFrame = new();
    // �Ĥ@���R���ӫ��s�ɪ� UV index
    private readonly Dictionary<Button, int> firstHitUvIndex = new();

    // [NEW] UV �������ؼ� UI�]UV �� 0..1 �d��|�M��o�� RectTransform�^
    [Header("UV �����]�w")]
    private RectTransform uiRectTransform; // [NEW]
    public void SetTargetRectTransform(RectTransform rt) // [NEW]
    {
        uiRectTransform = rt; // [NEW]
    }



    void Reset()
    {
        raycaster = GetComponentInParent<GraphicRaycaster>();
        eventSystem = FindAnyObjectByType<EventSystem>();
    }

    /// <summary>��P�@�V���Ҧ��ù��y�аe�i�ӡA�v�@�� UI Raycast�C</summary>
    public void Process(List<Vector2> screenPosList)
    {
        hitThisFrame.Clear();
        if (raycaster == null || eventSystem == null || screenPosList == null) return;

        var results = new List<RaycastResult>();
        var ped = new PointerEventData(eventSystem);

        foreach (var pos in screenPosList)
        {
            ped.position = pos;
            results.Clear();
            raycaster.Raycast(ped, results);

            // ���̤W�h�� Button�]�ΧA�n�����w����^
            for (int i = 0; i < results.Count; i++)
            {
                var go = results[i].gameObject;
                var btn = go.GetComponentInParent<Button>();
                if (btn != null && btn.interactable)
                {
                    hitThisFrame.Add(btn);

                    // [NEW] �O���̫�R���ɶ�
                    lastHitAt[btn] = Time.time;

                    break; // �R���@���N���F�]�קK�P�@�������h�h�^
                }
            }
        }

        // ��s hold �ֿn / Ĳ�o click
        // 1) �R�����֥[�p��
        foreach (var btn in hitThisFrame)
        {
            if (!holdTimers.ContainsKey(btn)) holdTimers[btn] = 0f;
            holdTimers[btn] += Time.deltaTime;

            if (autoClickOnHit && holdTimers[btn] >= holdTimeToClick)
            {
                holdTimers[btn] = 0f; // ���m�קK�s�I
                btn.onClick?.Invoke();
            }
        }

        // [CHANGED] �M�z�G�ȷ�u�Z���̫�R���W�L missGrace�v�~�M�s
        var now = Time.time;
        var toRemove = new List<Button>();
        foreach (var kv in holdTimers)
        {
            var btn = kv.Key;
            if (!hitThisFrame.Contains(btn))
            {
                // �S�b���V�R�� �� �ˬd�e��
                if (!lastHitAt.TryGetValue(btn, out float last) || (now - last) > missGrace)
                {
                    toRemove.Add(btn);
                }
            }
        }
        foreach (var b in toRemove)
        {
            holdTimers.Remove(b);
            lastHitAt.Remove(b); // [NEW] �@�ֲ����̫�R���ɶ�
        }
    }
    // ======================================================================
    // [NEW] UV �����G�Y�@�� 0..1 ���y�С](0,0)=���U, (1,1)=�k�W�^�A
    //      �|�H uiRectTransform ���Ѧҥ����A�ন�ù��y�Ы�u�έ쥻�� Process�C
    // ======================================================================
    // ===== �������N�ª� ProcessUV =====
    public void ProcessUV(List<Vector2> uvList, bool clamp01, out int heatedUvIndex, out float heatedHoldTimesPercentage)
    {
        heatedUvIndex = -1;
        heatedHoldTimesPercentage = 0f;

        hitThisFrame.Clear();
        if (uvList == null || uvList.Count == 0) return;
        if (raycaster == null || eventSystem == null) return;
        if (uiRectTransform == null) return;

        float now = Time.time; // �p�ݩ��� timeScale �i��� Time.unscaledTime

        // ���� Raycast �۾�
        var canvas = raycaster.GetComponent<Canvas>();
        Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? (canvas.worldCamera ?? raycaster.eventCamera ?? Camera.main)
            : null;

        // UV �� local(uiRect) �� world �� screen �� Raycast(Button)
        Rect r = uiRectTransform.rect;
        Vector2 size = r.size, pivot = uiRectTransform.pivot;

        var ped = new PointerEventData(eventSystem);
        var results = new List<RaycastResult>();

        // ---------- �^��/�M�z�G���Ĥ���P missGrace �W�� ----------
        // �Χַ��קK�M���ɭקﶰ�X
        var trackedSnapshot = new List<Button>(firstHitAt.Keys);
        foreach (var btn in trackedSnapshot)
        {
            bool invalid = (btn == null) || !btn.gameObject || !btn.gameObject.activeInHierarchy || !btn.interactable;
            if (invalid)
            {
                firstHitAt.Remove(btn);
                lastHitAt.Remove(btn);
                holdTimers.Remove(btn);
                firstHitUvIndex.Remove(btn);
                continue;
            }

            // �W�L missGrace ���A�R�� �� �^��
            if (!lastHitAt.TryGetValue(btn, out float last))
            {
                // �S���̫�R���ɶ������A�����L��
                firstHitAt.Remove(btn);
                holdTimers.Remove(btn);
                firstHitUvIndex.Remove(btn);
                continue;
            }
            if ((now - last) > missGrace)
            {
                firstHitAt.Remove(btn);
                lastHitAt.Remove(btn);
                holdTimers.Remove(btn);
                firstHitUvIndex.Remove(btn);
                Debug.Log($"now:{now}, last:{last}, {now - last}>{missGrace},�W��");
            }
            Debug.Log($"now:{now}, last:{last}");
        }

        for (int i = 0; i < uvList.Count; i++)
        {
            Vector2 uv = uvList[i];
            if (clamp01) { uv.x = Mathf.Clamp01(uv.x); uv.y = Mathf.Clamp01(uv.y); }

            Vector2 local = new Vector2(
                uv.x * size.x - pivot.x * size.x,
                uv.y * size.y - pivot.y * size.y
            );
            Vector3 world = uiRectTransform.TransformPoint(local);
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam, world);

            ped.position = screen;
            results.Clear();
            raycaster.Raycast(ped, results);

            // �����G���Ĥ@���i���ʪ� Button�]�̤W�h�^
            for (int j = 0; j < results.Count; j++)
            {
                var go = results[j].gameObject;
                var btn = go != null ? go.GetComponentInParent<Button>() : null;
                if (btn != null && btn.interactable && btn.gameObject.activeInHierarchy)
                {
                    hitThisFrame.Add(btn);

                    // �Ĥ@���Q���� �� ���� firstHitAt / firstHitUvIndex
                    if (!firstHitAt.ContainsKey(btn))
                    {
                        firstHitAt[btn] = now;
                        firstHitUvIndex[btn] = i;
                    }

                    // �C�V�R������s�̫�R���ɶ�
                    lastHitAt[btn] = now;
                    break; // �o�� UV �w�R���@������ Button�A���U�@�� UV
                }
            }
        }

        // ---------- �H�u�ɶ��t�v��s�ثe���������ơ]�D�֥[�^ ----------
        //�]�u�n�b�l�ܤ��A�N�|�H�ɶ��e�i�FmissGrace �������_�N���M�s�^
        holdTimers.Clear();
        foreach (var kv in firstHitAt)
        {
            var btn = kv.Key;
            float started = kv.Value;
            float held = Mathf.Max(0f, now - started);
            holdTimers[btn] = held;
        }

        // ---------- ��X�ثe�uhold �̤[�v������̡A�Ω� UI �^�� ----------
        Button leader = null;
        float bestHeld = -1f;
        float bestLast = -1f;
        int bestId = int.MaxValue;

        foreach (var kv in holdTimers)
        {
            var btn = kv.Key;
            float held = kv.Value;
            float last = lastHitAt.TryGetValue(btn, out var l) ? l : 0f;
            int id = btn != null ? btn.GetInstanceID() : int.MinValue;

            bool better =
                (held > bestHeld) ||
                (Mathf.Approximately(held, bestHeld) && last > bestLast) ||
                (Mathf.Approximately(held, bestHeld) && Mathf.Approximately(last, bestLast) && id < bestId);

            if (better)
            {
                leader = btn;
                bestHeld = held;
                bestLast = last;
                bestId = id;
            }
        }

        if (leader != null)
        {
            // �ʤ���P UV
            heatedHoldTimesPercentage = Mathf.Clamp01(bestHeld / holdTimeToClick) * 100f;
            if (firstHitUvIndex.TryGetValue(leader, out var idx)) heatedUvIndex = idx;
            Debug.Log($"bestHeld:{bestHeld},heatedHoldTimesPercentage:{heatedHoldTimesPercentage}");
            // ---------- �۰�Ĳ�o�G�u���\�@�����\ ----------
            if (autoClickOnHit && bestHeld >= holdTimeToClick)
            {
                leader.onClick?.Invoke();

                // Ĳ�o��M���A�T�O�P�@�ɨ�u�|���@�����\
                firstHitAt.Clear();
                lastHitAt.Clear();
                holdTimers.Clear();
                firstHitUvIndex.Clear();
                hitThisFrame.Clear();

                // Ĳ�o��V�AUI �A�i��ܺ��� 100% �Υѥ~�h��ø
            }
        }
        else
        {
            // �S������Կ� �� �^�ǹw�]
            heatedUvIndex = -1;
            heatedHoldTimesPercentage = 0f;
        }
    }
    // �]�i��^�Y�A���M���A�禡�A�O�o�[�W lastHitAt �M��
    public void ClearState() // �Y�w�s�b�N�ɤ@��
    {
        holdTimers.Clear();
        hitThisFrame.Clear();
        lastHitAt.Clear(); // [NEW]
    }
}
