using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreboardPlayer : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text killText;
    public TMP_Text deathText;
    public TMP_Text adrText;
    public TMP_Text weaponText;
    public TMP_Text kevlarText;
    public TMP_Text positionText;

    public Image hpBar;
    public Image posBar;

    public Image[] panels;

    public Color aliveColor;
    public Color deadColor;

    Vector3 hpOriginalPos;
    Vector2 hpOriginalSize;
    Vector3 posOriginalPos;
    Vector2 posOriginalSize;

    public void SetHP(int hp)
    {
        RectTransform rTransform = (RectTransform)hpBar.transform;
        rTransform.sizeDelta = new Vector2((float)hp / 100f * hpOriginalSize.x, hpOriginalSize.y);
        rTransform.position = hpOriginalPos - new Vector3((float)(100-hp) / 200f * 1.5f * hpOriginalSize.x, 0);

        if (hp > 0)
            for (int i = 0; i < panels.Length; i++)
                panels[i].color = aliveColor;
        else for (int i = 0; i < panels.Length; i++)
                panels[i].color = deadColor;

    }

    public void SetPosition(float percent)
    {
        
        RectTransform rTransform = (RectTransform)posBar.transform;
        rTransform.sizeDelta = new Vector2(percent * posOriginalSize.x, posOriginalSize.y);
        rTransform.position = posOriginalPos - new Vector3((1f-percent) / 2f * 1.5f* posOriginalSize.x, 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        RectTransform rTransform = (RectTransform)hpBar.transform;
        hpOriginalPos = rTransform.position;
        hpOriginalSize = rTransform.sizeDelta;
        rTransform = (RectTransform)posBar.transform;
        posOriginalPos = rTransform.position;
        posOriginalSize = rTransform.sizeDelta;


    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
