using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerUnit
{
    public string playerName;

    public int aim;
    public int dodge;
    public int tSkill;
    public int ctSkill;
    public int intelligence;
    public int mapAttack;
    public int mapDefense;
    public int pistolSkill;
    public int smgSkill;
    public int rifleSkill;
    public int sniperSkill;

    public int position;
    public int nextPosition;
    public int finalDestination;
    public float timeTillNextPosition;
    public float nextPositionTotalTime;

    public int weapon;
    public int hp;

    public int kills;
    public int deaths;
    public int totalDamage;
    public bool hasKevlar;
    public bool defending;

    public int flashCount = 0;
    public int hegCount = 0;
    public int molotovCount = 0;

    public int bulletsLeft;
    public float reloadTimeLeft;
    public float timeTillNextShot;

    public int xpCollected;

    public int TotalGrenades()
    {
        return flashCount + hegCount + molotovCount;
    }

    public void Setup(Player p, int map)
    {
        playerName = p.playerName;

        float randoRange = 0.1f;
        float pFactor = (0.75f + p.mood * 0.25f + p.energy / p.maxEnergy * 0.25f);
        aim = (int)(p.aim * pFactor * (1f - randoRange * 0.5f + Random.value * randoRange));
        dodge = (int)(p.dodge * pFactor * (1f - randoRange * 0.5f + Random.value * randoRange));
        tSkill = (int)(p.tSkill * pFactor * (1f - randoRange * 0.5f + Random.value * randoRange));
        ctSkill = (int)(p.ctSkill * pFactor * (1f - randoRange * 0.5f + Random.value * randoRange));
        intelligence = (int)(p.intelligence * pFactor * (1f - randoRange * 0.5f + Random.value * randoRange));
        mapAttack = (int)(p.mapAttack[map] * pFactor * (1f - randoRange * 0.5f + Random.value * randoRange));
        mapDefense = (int)(p.mapDefense[map] * pFactor * (1f - randoRange * 0.5f + Random.value * randoRange));
        pistolSkill = (int)(p.pistolSkill * pFactor * (1f - randoRange * 0.5f + Random.value * randoRange));
        smgSkill = (int)(p.smgSkill * pFactor * (1f - randoRange * 0.5f + Random.value * randoRange));
        rifleSkill = (int)(p.rifleSkill * pFactor * (1f - randoRange * 0.5f + Random.value * randoRange));
        sniperSkill = (int)(p.sniperSkill * pFactor * (1f - randoRange * 0.5f + Random.value * randoRange));

        kills = 0;
        deaths = 0;
        totalDamage = 0;
        hasKevlar = false;
        defending = true;

        reloadTimeLeft = 0f;
        timeTillNextShot = 0f;
    }
}


[System.Serializable]
public class TeamUnit
{
    public string teamName;
    public bool myTeam = false;
    public PlayerUnit[] members;
    public int money;
    public int roundsWon;
    public int lossBonus;

    public int attackingTactic; //0 - random, 1 - straight A, 2 - straight B, 3 - split A, 4 - split B, 5 - mid A, 6 mid B
    public List<int>[] defendingPlayers;

}
