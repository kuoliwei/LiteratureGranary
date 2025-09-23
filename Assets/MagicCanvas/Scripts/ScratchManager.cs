using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScratchManager : MonoBehaviour
{
    [Header("Scratch")]
    public List<Sprite> maskImages;
    public List<Texture> backgroundImages;
    public List<RawImage> backgroundRenderers;
    public List<ScratchCard> scratchCards;
    public float clearThreshold = 0.6f;
    public float revealHoldTime = 15f;
    public float restoreSpeed = 1f;
    public Texture brushTexture;
    public Material eraseMaterial;
    public float brushSize = 64f;

    [Header("Koku Aesthetic UI")]
    [SerializeField] private RawImage previewImage; // TakingPhotoPanel/RawImage�]�Y�ɹw���^
    [SerializeField] private RawImage resultImage;  // Tex_HiddenImage �� RawImage�]����Ƶ��G�^
    [SerializeField] private GameObject takingPhotoPanel;
    [SerializeField] private GameObject scratchSurface;
    [SerializeField] private WebCamController webCam;
    [SerializeField] private PanelFlowController flow; // �s�W

    private int currentIndex = 0;
    private bool imageFullyRevealed = false;
    private readonly List<Coroutine> restoreRoutines = new();
    private readonly HashSet<ScratchCard> revealedCards = new();

    private void Start()
    {
        foreach (var card in scratchCards)
        {
            card.Init();
            card.SetBrush(brushTexture, eraseMaterial, brushSize);
            card.SetMask(maskImages[0]);
            card.ResetScratch();
        }
        //ShowImageAt(0);
    }

    // === �i�w���ǡG�}�l�w�� ===
    public void UI_StartPreview()
    {
        {  //  �]�t�����ഫ
            takingPhotoPanel.SetActive(true);
            scratchSurface.SetActive(false);
        }
        {  //  ���]�t�����ഫ
            //takingPhotoPanel.SetActive(false);
            //scratchSurface.SetActive(true);
        }

        webCam.OpenCamera(webCam.selectedDeviceName);                         // �}���Y
        if (previewImage != null) previewImage.texture = webCam.PreviewTexture; // �j�w���K��
    }

    // === �i�w���ǡG��Өí���� ===
    public void UI_CaptureAndStylize(string styleName)
    {
        StartCoroutine(webCam.CaptureAndStylize(styleName, OnStyledReady));
    }

    // ����Ƨ����G��ܦb Tex_HiddenImage�A���^�����
    private void OnStyledReady(Texture2D tex)
    {
        if (tex == null) return;

        if (resultImage != null) resultImage.texture = tex; // ��ܨ� Tex_HiddenImage
        // �P�B��s�Ҧ����d����
        foreach (var r in backgroundRenderers) r.texture = tex;
        foreach (var c in scratchCards) { c.ResetScratch(); }

        takingPhotoPanel.SetActive(false);
        scratchSurface.SetActive(true);
    }
    public void ShowImage()
    {
        //int nextIndex = (currentIndex + 1) % backgroundImages.Count;
        int nextIndex = currentIndex;
        ShowImageAt(nextIndex);
    }
    // ==== �J�����d�y�{�]��˫O�d�^ ====
    private void ShowImageAt(int index)
    {
        if (index >= backgroundImages.Count) index = 0;
        currentIndex = index;
        imageFullyRevealed = false;

        foreach (var routine in restoreRoutines) if (routine != null) StopCoroutine(routine);
        restoreRoutines.Clear();
        revealedCards.Clear();

        foreach (var renderer in backgroundRenderers) renderer.texture = backgroundImages[index];
        foreach (var card in scratchCards)
        {
            card.SetMask(maskImages[index]);
            card.ResetScratch();
        }
        currentIndex += 1;
    }

    private IEnumerator AutoRestoreAfterDelay(ScratchCard target)
    {
        yield return new WaitForSeconds(revealHoldTime);
        yield return target.SmoothRestoreMask(restoreSpeed);
        if (revealedCards.Count >= scratchCards.Count) ShowImageAt(GetRandomIndex());
    }

    private int GetRandomIndex()
    {
        if (backgroundImages.Count <= 1) return 0;
        int randomIndex;
        do { randomIndex = Random.Range(0, backgroundImages.Count); }
        while (randomIndex == currentIndex);
        return randomIndex;
    }

    private void Update()
    {
        if (imageFullyRevealed) return;

        foreach (var card in scratchCards)
        {
            if (!revealedCards.Contains(card) && card.GetClearedRatio() >= clearThreshold)
            {
                revealedCards.Add(card);
                card.ShowFullImage();
                // �걵 PanelFlow�G��Ҧ��d���F�СA�ߧY�q���y�{����
                if (revealedCards.Count >= scratchCards.Count && !imageFullyRevealed)
                {
                    imageFullyRevealed = true;
                    if (flow != null) flow.Sys_OnScratchRevealComplete();
                }
                //var routine = StartCoroutine(AutoRestoreAfterDelay(card));
                //restoreRoutines.Add(routine);
            }
            if (revealedCards.Count >= scratchCards.Count) imageFullyRevealed = true;
        }
    }
}
