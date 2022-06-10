using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public string playerName;

    public int aim;
    public int dodge;
    public int tSkill;
    public int ctSkill;
    public int intelligence;
    public int[] mapAttack;
    public int[] mapDefense;
    public int pistolSkill;
    public int smgSkill;
    public int rifleSkill;
    public int sniperSkill;

    public float mood;
    public float energy;
    public float maxEnergy;

    public int rarity = 0;
    public float learningEffectiveness;

    public int CalculateValue()
    {
        return 5000;
    }
    public int AverageSkill()
    {
        int attackAverage = 0;
        int defendAverage = 0;
        for (int i = 0; i < mapAttack.Length;i++)
            attackAverage += mapAttack[i];
        for (int i = 0; i < mapDefense.Length;i++)
            defendAverage += mapDefense[i];
        attackAverage = attackAverage / mapAttack.Length;
        defendAverage = defendAverage / mapDefense.Length;

        int total = attackAverage + defendAverage + aim + dodge + tSkill + ctSkill + intelligence + pistolSkill + smgSkill + rifleSkill + sniperSkill;

        total = total / 11;
        return total;
    }

    static public Player Default(int sk = 5000)
    {
        Player p = new Player();
        p.playerName = "Player" + Random.Range(0, 100000).ToString();
        p.aim = sk;
        p.dodge = sk;
        p.tSkill = sk;
        p.ctSkill = sk;
        p.intelligence = sk;



        p.mapAttack = new int[10] { sk, sk, sk, sk, sk, sk, sk, sk, sk, sk };
        p.mapDefense = new int[10] { sk, sk, sk, sk, sk, sk, sk, sk, sk, sk };
        p.pistolSkill = sk;
        p.smgSkill = sk;
        p.rifleSkill = sk;
        p.sniperSkill = sk;

        p.mood = 0f;
        p.energy = 5f;
        p.maxEnergy = 5f;



        return p;
    }

}
