using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class DetailedPlayerInfo : MonoBehaviour
{
    public TMP_Text nameText;
    public Image energyBar;
    public Image moodBar;
    private Vector3 enOrPos;
    private Vector2 enOrSize;
    private Vector3 moOrPos;
    private Vector2 moOrSize;
    private Player playerRef;
    public TMP_Text aimText;
    public TMP_Text dodgeText;
    public TMP_Text intelligenceText;
    public TMP_Text ctskillText;
    public TMP_Text tskillText;
    public TMP_Text pistolText;
    public TMP_Text smgText;
    public TMP_Text rifleText;
    public TMP_Text sniperText;

    public TMP_Dropdown mapSelection;
    public TMP_Text mapAttackText;
    public TMP_Text mapDefenseText;

    public TMP_Text priceText;

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

    public void DisableSelf()
    {
        gameObject.SetActive(false);
    }

    public void UpdateData()
    {
        nameText.text = playerRef.playerName;

        float energyPercent = playerRef.energy / playerRef.maxEnergy;
        energyBar.rectTransform.sizeDelta = new Vector2(energyPercent * enOrSize.x, enOrSize.y);
        energyBar.transform.position = new Vector3(enOrPos.x - 0.75f * energyPercent * enOrSize.x, enOrPos.y, 0);
        float moodPercent = (1f + playerRef.mood) / 2f;
        moodBar.rectTransform.sizeDelta = new Vector2(moodPercent * moOrSize.x, moOrSize.y);
        moodBar.transform.position = new Vector3(moOrPos.x - 0.75f * moodPercent * moOrSize.x, moOrPos.y, 0);

        aimText.text = "AIM: " + playerRef.aim;
        dodgeText.text = "DODGE: " + playerRef.dodge;
        intelligenceText.text = "INTELLIGENCE: " + playerRef.intelligence;
        ctskillText.text = "CT SKILL: " + playerRef.ctSkill;
        tskillText.text = "T SKILL: " + playerRef.tSkill;

        pistolText.text = "PISTOL: " + playerRef.pistolSkill;
        smgText.text = "SMG: " + playerRef.smgSkill;
        rifleText.text = "RIFLE: " + playerRef.rifleSkill;
        sniperText.text = "SNIPER: " + playerRef.sniperSkill;

        int mapIndex = mapSelection.value;
        mapAttackText.text = "MAP ATTACK: " + playerRef.mapAttack[mapIndex];
        mapDefenseText.text = "MAP DEFENSE: " + playerRef.mapDefense[mapIndex];

        priceText.text = "PRICE: Ȼ" + playerRef.CalculateValue();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
