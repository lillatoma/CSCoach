using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SmallPlayerInfo : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text skillText;
    public Image energyBar;
    public Image moodBar;

    private Player playerRef;

    private Vector3 enOrPos;
    private Vector2 enOrSize;
    private Vector3 moOrPos;
    private Vector2 moOrSize;

    // Start is called before the first frame update
    void Start()
    {
        RectTransform rect = energyBar.rectTransform;
        enOrPos = rect.position;
        enOrSize = rect.sizeDelta;
        rect = moodBar.rectTransform;
        moOrPos = rect.position;
        moOrSize = rect.sizeDelta;
    }

    public void LinkPlayer(Player pl)
    {
        playerRef = pl;
    }

    public void UpdateData()
    {
        nameText.text = playerRef.playerName;
        skillText.text = "AVG " + playerRef.AverageSkill();

        float energyPercent = playerRef.energy / playerRef.maxEnergy;
        energyBar.rectTransform.sizeDelta = new Vector2(energyPercent * enOrSize.x, enOrSize.y);
        energyBar.transform.position = new Vector3(enOrPos.x - 0.75f * energyPercent * enOrSize.x, enOrPos.y, 0);
        float moodPercent = (1f+playerRef.mood) / 2f;
        moodBar.rectTransform.sizeDelta = new Vector2(moodPercent * moOrSize.x, moOrSize.y);
        moodBar.transform.position = new Vector3(moOrPos.x - 0.75f * moodPercent * moOrSize.x, moOrPos.y, 0);
    }

    public void OnClick()
    {
        var apInfo = FindObjectOfType<AllPlayerInfo>();
        apInfo.detailedInfo.LinkPlayer(playerRef);
        apInfo.detailedInfo.UpdateData();
        apInfo.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateData();
    }
}
