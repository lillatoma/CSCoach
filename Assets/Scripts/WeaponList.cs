using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct WeaponData
{
    public string name;
    public bool TBuyable;
    public bool CTBuyable;
    public int[] closeDmg;
    public int[] midDmg;
    public int[] longDmg;
    public float closeAccuracy;
    public float midAccuracy;
    public float longAccuracy;
    public float armorPenetration;
    public int cost;
    public float fireRate;
    public float reloadTime;
    public int ammoSize;
    public bool isPistol;
    public bool isSMG;
    public bool isRifle;
    public bool isSniper;

    public float DPSClose()
    {
        return fireRate * midDmg[2] * closeAccuracy * armorPenetration;
    }

    public float DPSMid()
    {
        return fireRate * midDmg[2] * midAccuracy * armorPenetration;
    }

    public float DPSLong()
    {
        return fireRate * midDmg[2] * longAccuracy * armorPenetration;
    }

    public float DPSKevlar()
    {
        return fireRate * midDmg[2] * armorPenetration;
    }
}


public class WeaponList : MonoBehaviour
{
    public WeaponData[] weapons;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
