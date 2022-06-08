using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct Pair
{
    public int left;
    public int right;

    public Pair(int l, int r)
    {
        left = l;
        right = r;
    }
}


[System.Serializable]
public class MatchSettings
{
    public int teamStartMoney = 4000;
    public int teamMaxMoney = 80000;
    public int halfTimeLength = 15;
    public bool overtimePossible = false;
    public int overtimeHalfTime = 3;
    public int overtimeStartMoney = 50000;
    public int terrorWinReward = 3250;
    public int ctWinReward = 3250;
    public int terrorLoseReward = 1400;
    public int ctLossReward = 1400;
    public int killReward = 300;
    public int bombPlantReward = 800;
    public int defaultLossBonus = 1;
    public int maxLossBonus = 4;
    public int lossBonusReward = 500;
    public int lossBonusAddition = 1;
    public int lossBonusLoss = 2;
}

public class Match : MonoBehaviour
{
    public MatchSettings matchSettings;

    bool roundStarted = false;
    bool markedForEnd = false;
    bool markedForFullSimulate = false;
    bool halftimeHappened = false;

    public float[] hitChances;
    public float matchSpeed = 1f;
    public float nonkevlarInaccuracyMod = 0.8f;
    public Color terrorColor;
    public Color ctColor;
    public Color msgColor;

    public TeamUnit tTeam;
    public TeamUnit ctTeam;
    private WeaponList weapons;
    private MapList maps;
    public int mapID;

    public float timeAliveXPRate; //Chance for XP / sec
    public float timeInFightXPRate; //Chance for XP / sec
    public int xpForKill;
    public float xpDeathHigherSkillRate;



    bool hasUserBought = false;
    int bombPlanted = -1;

    int clinchPoint = 16;
    int nextTeamSwap = 15;

    List<Pair> activeFights = new List<Pair>();
    List<float> activeFightsActivationLeft = new List<float>();
    List<int> activeFightsDistances = new List<int>();
    public List<string> messages = new List<string>();
    public List<Color> messageColors = new List<Color>();


    // Start is called before the first frame update
    void Start()
    {
        tTeam = new TeamUnit();
        ctTeam = new TeamUnit();
        tTeam.members = new PlayerUnit[5];
        ctTeam.members = new PlayerUnit[5];
        for(int i = 0; i < 5; i++)
        {
            tTeam.members[i] = new PlayerUnit();
            ctTeam.members[i] = new PlayerUnit();
        }
        weapons = FindObjectOfType<WeaponList>();
        maps = FindObjectOfType<MapList>();
        SetupMyTeam(Player.Default(8000), Player.Default(), Player.Default(), Player.Default(), Player.Default());
        SetupEnemyTeam(Player.Default(6000), Player.Default(6000), Player.Default(6000), Player.Default(6000), Player.Default(6000));

        BeginMatch();

    }

    void AddMessage(string msg, Color clr)
    {
        if(messages.Count < 13)
        {
            messages.Add(msg);
            messageColors.Add(clr);
        }
        else
        {
            messages.Add(msg);
            messageColors.Add(clr);
            messages.RemoveAt(0);
            messageColors.RemoveAt(0);
        }
    }


    void SetupMyTeam(Player p1, Player p2, Player p3, Player p4, Player p5)
    {
        ctTeam.members[0].Setup(p1, mapID);
        ctTeam.members[1].Setup(p2, mapID);
        ctTeam.members[2].Setup(p3, mapID);
        ctTeam.members[3].Setup(p4, mapID);
        ctTeam.members[4].Setup(p5, mapID);


        ctTeam.myTeam = true;

        int playersUsed = 0 ;

        ctTeam.defendingPlayers = new List<int>[maps.maps[mapID].positions.Length];
        for (int i = 0; i < maps.maps[mapID].positions.Length; i++)
        {
            ctTeam.defendingPlayers[i] = new List<int>();
            for (int j = 0; j < maps.maps[mapID].positions[i].usualCTCount; j++)
            {
                ctTeam.defendingPlayers[i].Add(playersUsed);
                playersUsed++;
            }
        }
    }

    void SetupEnemyTeam(Player p1, Player p2, Player p3, Player p4, Player p5)
    {
        tTeam.members[0].Setup(p1, mapID);
        tTeam.members[1].Setup(p2, mapID);
        tTeam.members[2].Setup(p3, mapID);
        tTeam.members[3].Setup(p4, mapID);
        tTeam.members[4].Setup(p5, mapID);



        int playersUsed = 0;
        tTeam.defendingPlayers = new List<int>[maps.maps[mapID].positions.Length];
        for (int i = 0; i < maps.maps[mapID].positions.Length; i++)
        {
            tTeam.defendingPlayers[i] = new List<int>();
            for (int j = 0; j < maps.maps[mapID].positions[i].usualCTCount; j++)
            {
                tTeam.defendingPlayers[i].Add(playersUsed);
                playersUsed++;
            }
        }
    }

    void SetupEnemyTeamRandom()
    {

    }


    void NoticeFights()
    {
        List<int> ctr = new List<int>();


        for(int i = 0; i < 5; i++)
        {
            ctr.Add(i);
        }

        for(int i = 0; i < 5; i++)
        {
            int r1 = Random.Range(0, ctr.Count);
            int ct = ctr[r1];
            ctr.RemoveAt(r1);
            if (ctTeam.members[ct].hp == 0)
                continue;

            List<int> tr = new List<int>();
            for (int j = 0; j < 5; j++)
                tr.Add(j);
            for (int j = 0; j < 5; j++)
            {
                int r2 = Random.Range(0, tr.Count);
                int t = tr[r2];
                tr.RemoveAt(r2);
                if (tTeam.members[t].hp == 0)
                    continue;
                if(ctTeam.members[ct].position == tTeam.members[t].position)
                {
                    if (!activeFights.Contains(new Pair(ct, t+5)))
                    {
                        activeFights.Add(new Pair(ct, t+5));
                        if(ctTeam.members[ct].defending)
                            activeFightsActivationLeft.Add(Random.Range(3f, 4f));
                        else
                            activeFightsActivationLeft.Add(Random.Range(3.5f, 4.5f));
                        activeFights.Add(new Pair(t+5, ct));
                        if(tTeam.members[t].defending)
                            activeFightsActivationLeft.Add(Random.Range(3f, 4f));
                        else
                            activeFightsActivationLeft.Add(Random.Range(3.5f, 4.5f));

                        float r = Random.value;
                        MapPosition position = maps.maps[mapID].positions[tTeam.members[t].position];

                        if( r < position.fightChances[0])
                        {
                            activeFightsDistances.Add(0);
                            activeFightsDistances.Add(0);
                        }
                        else if (r < position.fightChances[0] + position.fightChances[1])
                        {
                            activeFightsDistances.Add(1);
                            activeFightsDistances.Add(1);
                        }
                        else
                        {
                            activeFightsDistances.Add(2);
                            activeFightsDistances.Add(2);
                        }

                    }
                }

            }
        }
    }

    int min(int a, int b)
    {
        return (a < b) ? a : b;
    }

    int max(int a, int b)
    {
        return (a > b) ? a : b;
    }

    float min(float a, float b)
    {
        return (a < b) ? a : b;
    }

    float max(float a, float b)
    {
        return (a > b) ? a : b;
    }

    void CheckFights(float ingameTime)
    {
        for(int j = activeFights.Count - 1; j >= 0; j--)
        {
            int ct = min(activeFights[j].left, activeFights[j].right);
            int t = max(activeFights[j].left, activeFights[j].right) - 5;

            if (ctTeam.members[ct].hp == 0 || tTeam.members[t].hp == 0 || ctTeam.members[ct].position != tTeam.members[t].position)
            {
                activeFights.RemoveAt(j);
                activeFightsActivationLeft.RemoveAt(j);
                activeFightsDistances.RemoveAt(j);
            }
            else
            {
                activeFightsActivationLeft[j] -= ingameTime;
            }

        }
    }


    void SimulateGrenadesForTimePassed(float ingameTime)
    {
        bool killMade = false;

        bool[] ctAvailable = new bool[5] { true, true, true, true, true };
        bool[] tAvailable = new bool[5] { true, true, true, true, true };

        float flashReferenceTime = 4f;
        float hegReferenceTime = 2f;
        float molotovReferenceTime = 3f;

        for (int j = 0; j < activeFights.Count; j++)
        {
            float timeLeft = ingameTime;
            if (activeFightsActivationLeft[j] <= 1.5f)
            {
                int ct = min(activeFights[j].left, activeFights[j].right);
                int t = max(activeFights[j].left, activeFights[j].right) - 5;
                bool ctShoots = ct == activeFights[j].left;

                if (ctShoots)
                {
                    if (ctTeam.members[ct].hp == 0)
                        continue;
                    if (!ctAvailable[ct])
                        continue;
                    ctAvailable[ct] = false;

                    if (ctTeam.members[ct].flashCount > 0 && Random.value < ingameTime / flashReferenceTime)
                    {
                        ctTeam.members[ct].flashCount--;
                        if (ctTeam.members[ct].defending)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                if (tTeam.members[i].hp > 0 && tTeam.members[i].position == ctTeam.members[ct].position)
                                {
                                    if (Random.value * (ctTeam.members[ct].intelligence + tTeam.members[i].intelligence) < ctTeam.members[ct].intelligence)
                                        tTeam.members[i].timeTillNextShot += Random.Range(1f, 3f);
                                }
                            }
                        }
                        else
                        {
                            if (Random.value * (ctTeam.members[ct].intelligence + tTeam.members[t].intelligence) < ctTeam.members[ct].intelligence)
                                tTeam.members[t].timeTillNextShot += Random.Range(1f, 3f);
                        }
                    }

                    if (ctTeam.members[ct].hegCount > 0 && Random.value < ingameTime / hegReferenceTime)
                    {
                        ctTeam.members[ct].hegCount--;
                        if (ctTeam.members[ct].defending)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                if (tTeam.members[i].hp > 0 && tTeam.members[i].position == ctTeam.members[ct].position)
                                {
                                    if (Random.value * (ctTeam.members[ct].intelligence + tTeam.members[i].intelligence) < ctTeam.members[ct].intelligence)
                                    {
                                        int damage = Random.Range(0, 30) + Random.Range(0, 30) + Random.Range(0, 30) + Random.Range(0, 30) + Random.Range(0, 30);
                                        if (tTeam.members[i].hasKevlar)
                                            damage = (int)(damage * 0.4);

                                        ctTeam.members[ct].totalDamage += damage;
                                        tTeam.members[i].hp -= damage;
                                        if (tTeam.members[i].hp <= 0)
                                        {
                                            ctTeam.members[ct].totalDamage += tTeam.members[i].hp;
                                            tTeam.members[i].hp = 0;
                                            HandoutKillXp(ct, true);
                                            HandoutDeathXP(ct, true, i);
                                            //WeaponSwap Part
                                            if (weapons.weapons[tTeam.members[i].weapon].DPSMid() 
                                                > weapons.weapons[ctTeam.members[ct].weapon].DPSMid())
                                            {
                                                ctTeam.members[ct].weapon = tTeam.members[i].weapon;
                                                ctTeam.members[ct].timeTillNextShot = 1f / weapons.weapons[ctTeam.members[ct].weapon].fireRate;
                                                ctTeam.members[ct].bulletsLeft = tTeam.members[i].bulletsLeft;
                                            }
                                            if(ctTeam.members[ct].TotalGrenades() < 4) //Dead drops one grenade
                                            {
                                                if (tTeam.members[i].molotovCount > 0)
                                                    ctTeam.members[ct].molotovCount++;
                                                else if (tTeam.members[i].hegCount > 0)
                                                    ctTeam.members[ct].hegCount++;
                                                else if (tTeam.members[i].flashCount > 0)
                                                    ctTeam.members[ct].flashCount++;
                                            }

                                            tTeam.members[i].weapon = 0;
                                            tTeam.members[i].deaths++;
                                            ctTeam.members[ct].kills++;
                                            ctTeam.money += matchSettings.killReward;
                                            killMade = true;
                                            string msg = ctTeam.members[ct].playerName + " killed " + tTeam.members[i].playerName + " with a HE Grenade";
                                            AddMessage(msg, ctColor);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Random.value * (ctTeam.members[ct].intelligence + tTeam.members[t].intelligence) < ctTeam.members[ct].intelligence)
                            {
                                int damage = Random.Range(0, 30) + Random.Range(0, 30) + Random.Range(0, 30) + Random.Range(0, 30) + Random.Range(0, 30);
                                if (tTeam.members[t].hasKevlar)
                                    damage = (int)(damage * 0.4);

                                ctTeam.members[ct].totalDamage += damage;
                                tTeam.members[t].hp -= damage;
                                if (tTeam.members[t].hp <= 0)
                                {
                                    ctTeam.members[ct].totalDamage += tTeam.members[t].hp;
                                    tTeam.members[t].hp = 0;
                                    HandoutKillXp(ct, true);
                                    HandoutDeathXP(ct, true, t);
                                    //WeaponSwap Part

                                    if (weapons.weapons[tTeam.members[t].weapon].DPSMid()
                                        > weapons.weapons[ctTeam.members[ct].weapon].DPSMid())
                                    {
                                        ctTeam.members[ct].weapon = tTeam.members[t].weapon;
                                        ctTeam.members[ct].timeTillNextShot = 1f / weapons.weapons[ctTeam.members[ct].weapon].fireRate;
                                        ctTeam.members[ct].bulletsLeft = tTeam.members[t].bulletsLeft;
                                    }
                                    if (ctTeam.members[ct].TotalGrenades() < 4) //Dead drops one grenade
                                    {
                                        if (tTeam.members[t].molotovCount > 0)
                                            ctTeam.members[ct].molotovCount++;
                                        else if (tTeam.members[t].hegCount > 0)
                                            ctTeam.members[ct].hegCount++;
                                        else if (tTeam.members[t].flashCount > 0)
                                            ctTeam.members[ct].flashCount++;
                                    }
                                    tTeam.members[t].weapon = 0;
                                    tTeam.members[t].deaths++;
                                    ctTeam.members[ct].kills++;
                                    ctTeam.money += matchSettings.killReward;
                                    killMade = true;
                                    string msg = ctTeam.members[ct].playerName + " killed " + tTeam.members[t].playerName + " with a HE Grenade";
                                    AddMessage(msg, ctColor);
                                }
                            }
                        }
                    }

                    if (ctTeam.members[ct].molotovCount > 0 && Random.value < ingameTime / molotovReferenceTime)
                    {
                        ctTeam.members[ct].molotovCount--;
                        if (ctTeam.members[ct].defending)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                if (tTeam.members[i].hp > 0 && tTeam.members[i].position == ctTeam.members[ct].position)
                                {
                                    if (Random.value * (ctTeam.members[ct].intelligence + tTeam.members[i].intelligence) < ctTeam.members[ct].intelligence)
                                    {
                                        int ticks = 0;
                                        int damage = 0;
                                        for (int k = 0; k < 20; k++)
                                        {
                                            if (Random.value * (ctTeam.members[ct].intelligence + tTeam.members[i].intelligence) < ctTeam.members[ct].intelligence)
                                            {
                                                int pDamage = 1 + ticks * 1;
                                                if (pDamage > 7)
                                                    pDamage = 7;
                                                ticks++;
                                                damage += pDamage;
                                            }
                                        }

                                        ctTeam.members[ct].totalDamage += damage;
                                        tTeam.members[i].hp -= damage;
                                        if (tTeam.members[i].hp <= 0)
                                        {
                                            ctTeam.members[ct].totalDamage += tTeam.members[i].hp;
                                            tTeam.members[i].hp = 0;
                                            HandoutKillXp(ct, true);
                                            HandoutDeathXP(ct, true, i);
                                            //WeaponSwap Part
                                            if (weapons.weapons[tTeam.members[i].weapon].DPSMid()
                                                > weapons.weapons[ctTeam.members[ct].weapon].DPSMid())
                                            {
                                                ctTeam.members[ct].weapon = tTeam.members[i].weapon;
                                                ctTeam.members[ct].timeTillNextShot = 1f / weapons.weapons[ctTeam.members[ct].weapon].fireRate;
                                                ctTeam.members[ct].bulletsLeft = tTeam.members[i].bulletsLeft;
                                            }
                                            if (ctTeam.members[ct].TotalGrenades() < 4) //Dead drops one grenade
                                            {
                                                if (tTeam.members[i].molotovCount > 0)
                                                    ctTeam.members[ct].molotovCount++;
                                                else if (tTeam.members[i].hegCount > 0)
                                                    ctTeam.members[ct].hegCount++;
                                                else if (tTeam.members[i].flashCount > 0)
                                                    ctTeam.members[ct].flashCount++;
                                            }
                                            tTeam.members[i].weapon = 0;
                                            tTeam.members[i].deaths++;
                                            ctTeam.members[ct].kills++;
                                            ctTeam.money += matchSettings.killReward;
                                            killMade = true;
                                            string msg = ctTeam.members[ct].playerName + " killed " + tTeam.members[i].playerName + " with a HE Grenade";
                                            AddMessage(msg, ctColor);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Random.value * (ctTeam.members[ct].intelligence + tTeam.members[t].intelligence) < ctTeam.members[ct].intelligence)
                            {
                                int ticks = 0;
                                int damage = 0;
                                for (int k = 0; k < 20; k++)
                                {
                                    if (Random.value * (ctTeam.members[ct].intelligence + tTeam.members[t].intelligence) < ctTeam.members[ct].intelligence)
                                    {
                                        int pDamage = 1 + ticks * 1;
                                        if (pDamage > 7)
                                            pDamage = 7;
                                        ticks++;
                                        damage += pDamage;
                                    }
                                }


                                ctTeam.members[ct].totalDamage += damage;
                                tTeam.members[t].hp -= damage;
                                if (tTeam.members[t].hp <= 0)
                                {
                                    ctTeam.members[ct].totalDamage += tTeam.members[t].hp;
                                    tTeam.members[t].hp = 0;
                                    HandoutKillXp(ct, true);
                                    HandoutDeathXP(ct, true, t);
                                    //WeaponSwap Part
                                    if (weapons.weapons[tTeam.members[t].weapon].DPSMid()
                                        > weapons.weapons[ctTeam.members[ct].weapon].DPSMid())
                                    {
                                        ctTeam.members[ct].weapon = tTeam.members[t].weapon;
                                        ctTeam.members[ct].timeTillNextShot = 1f / weapons.weapons[ctTeam.members[ct].weapon].fireRate;
                                        ctTeam.members[ct].bulletsLeft = tTeam.members[t].bulletsLeft;
                                    }
                                    if (ctTeam.members[ct].TotalGrenades() < 4) //Dead drops one grenade
                                    {
                                        if (tTeam.members[t].molotovCount > 0)
                                            ctTeam.members[ct].molotovCount++;
                                        else if (tTeam.members[t].hegCount > 0)
                                            ctTeam.members[ct].hegCount++;
                                        else if (tTeam.members[t].flashCount > 0)
                                            ctTeam.members[ct].flashCount++;
                                    }
                                    tTeam.members[t].weapon = 0;
                                    tTeam.members[t].deaths++;
                                    ctTeam.members[ct].kills++;
                                    ctTeam.money += matchSettings.killReward;
                                    killMade = true;
                                    string msg = ctTeam.members[ct].playerName + " killed " + tTeam.members[t].playerName + " with a HE Grenade";
                                    AddMessage(msg, ctColor);
                                }
                            }
                        }
                    }

                }



                else //WHEN T SHOOTS
                {
                    if (tTeam.members[t].hp == 0)
                        continue;
                    if (!tAvailable[t])
                        continue;
                    tAvailable[t] = false;

                    if (tTeam.members[t].flashCount > 0 && Random.value < ingameTime / flashReferenceTime)
                    {
                        tTeam.members[t].flashCount--;
                        if (tTeam.members[t].defending)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                if (ctTeam.members[i].hp > 0 && ctTeam.members[i].position == tTeam.members[t].position)
                                {
                                    if (Random.value * (tTeam.members[t].intelligence + ctTeam.members[i].intelligence) < tTeam.members[t].intelligence)
                                        ctTeam.members[i].timeTillNextShot += Random.Range(1f, 3f);
                                }
                            }
                        }
                        else
                        {
                            if (Random.value * (tTeam.members[t].intelligence + ctTeam.members[ct].intelligence) < tTeam.members[t].intelligence)
                                ctTeam.members[ct].timeTillNextShot += Random.Range(1f, 3f);
                        }
                    }

                    if (tTeam.members[t].hegCount > 0 && Random.value < ingameTime / hegReferenceTime)
                    {
                        tTeam.members[t].hegCount--;
                        if (tTeam.members[t].defending)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                if (ctTeam.members[i].hp > 0 && ctTeam.members[i].position == tTeam.members[t].position)
                                {
                                    if (Random.value * (tTeam.members[t].intelligence + ctTeam.members[i].intelligence) < tTeam.members[t].intelligence)
                                    {
                                        int damage = Random.Range(0, 30) + Random.Range(0, 30) + Random.Range(0, 30) + Random.Range(0, 30) + Random.Range(0, 30);
                                        if (ctTeam.members[i].hasKevlar)
                                            damage = (int)(damage * 0.4);

                                        tTeam.members[t].totalDamage += damage;
                                        ctTeam.members[i].hp -= damage;
                                        if (tTeam.members[i].hp <= 0)
                                        {
                                            tTeam.members[t].totalDamage += ctTeam.members[i].hp;
                                            ctTeam.members[i].hp = 0;
                                            HandoutKillXp(t, false);
                                            HandoutDeathXP(t, false, i);
                                            //WeaponSwap Part
                                            if (weapons.weapons[ctTeam.members[i].weapon].DPSMid()
                                                > weapons.weapons[tTeam.members[t].weapon].DPSMid())
                                            {
                                                tTeam.members[t].weapon = tTeam.members[i].weapon;
                                                tTeam.members[t].timeTillNextShot = 1f / weapons.weapons[tTeam.members[t].weapon].fireRate;
                                                tTeam.members[t].bulletsLeft = ctTeam.members[i].bulletsLeft;
                                            }
                                            if (tTeam.members[t].TotalGrenades() < 4) //Dead drops one grenade
                                            {
                                                if (ctTeam.members[i].molotovCount > 0)
                                                    tTeam.members[t].molotovCount++;
                                                else if (ctTeam.members[i].hegCount > 0)
                                                    tTeam.members[t].hegCount++;
                                                else if (ctTeam.members[i].flashCount > 0)
                                                    tTeam.members[t].flashCount++;
                                            }
                                            ctTeam.members[i].weapon = 0;
                                            ctTeam.members[i].deaths++;
                                            tTeam.members[t].kills++;
                                            tTeam.money += matchSettings.killReward;
                                            killMade = true;
                                            string msg = tTeam.members[t].playerName + " killed " + ctTeam.members[i].playerName + " with a HE Grenade";
                                            AddMessage(msg, terrorColor);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Random.value * (tTeam.members[t].intelligence + ctTeam.members[ct].intelligence) < tTeam.members[t].intelligence)
                            {
                                int damage = Random.Range(0, 30) + Random.Range(0, 30) + Random.Range(0, 30) + Random.Range(0, 30) + Random.Range(0, 30);
                                if (ctTeam.members[ct].hasKevlar)
                                    damage = (int)(damage * 0.4);

                                tTeam.members[t].totalDamage += damage;
                                ctTeam.members[ct].hp -= damage;
                                if (ctTeam.members[ct].hp <= 0)
                                {
                                    tTeam.members[t].totalDamage += ctTeam.members[ct].hp;
                                    ctTeam.members[ct].hp = 0;
                                    HandoutKillXp(t, false);
                                    HandoutDeathXP(t, false, ct);
                                    //WeaponSwap Part
                                    if (weapons.weapons[ctTeam.members[ct].weapon].DPSMid()
                                        > weapons.weapons[tTeam.members[t].weapon].DPSMid())
                                    {
                                        tTeam.members[t].weapon = tTeam.members[ct].weapon;
                                        tTeam.members[t].timeTillNextShot = 1f / weapons.weapons[tTeam.members[t].weapon].fireRate;
                                        tTeam.members[t].bulletsLeft = ctTeam.members[ct].bulletsLeft;
                                    }
                                    if (tTeam.members[t].TotalGrenades() < 4) //Dead drops one grenade
                                    {
                                        if (ctTeam.members[ct].molotovCount > 0)
                                            tTeam.members[t].molotovCount++;
                                        else if (ctTeam.members[ct].hegCount > 0)
                                            tTeam.members[t].hegCount++;
                                        else if (ctTeam.members[ct].flashCount > 0)
                                            tTeam.members[t].flashCount++;
                                    }
                                    ctTeam.members[ct].weapon = 0;
                                    ctTeam.members[ct].deaths++;
                                    tTeam.members[t].kills++;
                                    tTeam.money += matchSettings.killReward;
                                    killMade = true;
                                    string msg = tTeam.members[t].playerName + " killed " + ctTeam.members[ct].playerName + " with a HE Grenade";
                                    AddMessage(msg, terrorColor);
                                }
                            }
                        }
                    }

                    if (tTeam.members[t].molotovCount > 0 && Random.value < ingameTime / molotovReferenceTime)
                    {
                        tTeam.members[t].molotovCount--;
                        if (tTeam.members[t].defending)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                if (ctTeam.members[i].hp > 0 && ctTeam.members[i].position == tTeam.members[t].position)
                                {
                                    if (Random.value * (tTeam.members[t].intelligence + ctTeam.members[i].intelligence) < tTeam.members[t].intelligence)
                                    {
                                        int ticks = 0;
                                        int damage = 0;
                                        for (int k = 0; k < 20; k++)
                                        {
                                            if (Random.value * (tTeam.members[t].intelligence + ctTeam.members[i].intelligence) < tTeam.members[t].intelligence)
                                            {
                                                int pDamage = 1 + ticks * 1;
                                                if (pDamage > 7)
                                                    pDamage = 7;
                                                ticks++;
                                                damage += pDamage;
                                            }
                                        }

                                        tTeam.members[t].totalDamage += damage;
                                        ctTeam.members[i].hp -= damage;
                                        if (ctTeam.members[i].hp <= 0)
                                        {
                                            tTeam.members[t].totalDamage += ctTeam.members[i].hp;
                                            ctTeam.members[i].hp = 0;
                                            HandoutKillXp(t, false);
                                            HandoutDeathXP(t, false, i);
                                            //WeaponSwap Part
                                            if (weapons.weapons[ctTeam.members[i].weapon].DPSMid()
                                                > weapons.weapons[tTeam.members[t].weapon].DPSMid())
                                            {
                                                tTeam.members[t].weapon = ctTeam.members[i].weapon;
                                                tTeam.members[t].timeTillNextShot = 1f / weapons.weapons[tTeam.members[t].weapon].fireRate;
                                                tTeam.members[t].bulletsLeft = ctTeam.members[i].bulletsLeft;
                                            }
                                            if (tTeam.members[t].TotalGrenades() < 4) //Dead drops one grenade
                                            {
                                                if (ctTeam.members[i].molotovCount > 0)
                                                    tTeam.members[t].molotovCount++;
                                                else if (ctTeam.members[i].hegCount > 0)
                                                    tTeam.members[t].hegCount++;
                                                else if (ctTeam.members[i].flashCount > 0)
                                                    tTeam.members[t].flashCount++;
                                            }
                                            ctTeam.members[i].weapon = 0;
                                            ctTeam.members[i].deaths++;
                                            tTeam.members[t].kills++;
                                            tTeam.money += matchSettings.killReward;
                                            killMade = true;
                                            string msg = tTeam.members[t].playerName + " killed " + ctTeam.members[i].playerName + " with a Molotov";
                                            AddMessage(msg, terrorColor);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (Random.value * (tTeam.members[t].intelligence + ctTeam.members[ct].intelligence) < tTeam.members[t].intelligence)
                            {
                                int ticks = 0;
                                int damage = 0;
                                for (int k = 0; k < 20; k++)
                                {
                                    if (Random.value * (tTeam.members[t].intelligence + ctTeam.members[ct].intelligence) < tTeam.members[t].intelligence)
                                    {
                                        int pDamage = 1 + ticks * 1;
                                        if (pDamage > 7)
                                            pDamage = 7;
                                        ticks++;
                                        damage += pDamage;
                                    }
                                }


                                tTeam.members[t].totalDamage += damage;
                                ctTeam.members[ct].hp -= damage;
                                if (ctTeam.members[ct].hp <= 0)
                                {
                                    tTeam.members[t].totalDamage += ctTeam.members[ct].hp;
                                    ctTeam.members[ct].hp = 0;
                                    HandoutKillXp(t, false);
                                    HandoutDeathXP(t, false, ct);
                                    //WeaponSwap Part
                                    if (weapons.weapons[ctTeam.members[ct].weapon].DPSMid()
                                        > weapons.weapons[tTeam.members[t].weapon].DPSMid())
                                    {
                                        tTeam.members[t].weapon = ctTeam.members[ct].weapon;
                                        tTeam.members[t].timeTillNextShot = 1f / weapons.weapons[tTeam.members[t].weapon].fireRate;
                                        tTeam.members[t].bulletsLeft = ctTeam.members[ct].bulletsLeft;
                                    }
                                    if (tTeam.members[t].TotalGrenades() < 4) //Dead drops one grenade
                                    {
                                        if (ctTeam.members[ct].molotovCount > 0)
                                            tTeam.members[t].molotovCount++;
                                        else if (ctTeam.members[ct].hegCount > 0)
                                            tTeam.members[t].hegCount++;
                                        else if (ctTeam.members[ct].flashCount > 0)
                                            tTeam.members[t].flashCount++;
                                    }
                                    ctTeam.members[ct].weapon = 0;
                                    ctTeam.members[ct].deaths++;
                                    tTeam.members[t].kills++;
                                    tTeam.money += matchSettings.killReward;
                                    killMade = true;
                                    string msg = tTeam.members[t].playerName + " killed " + ctTeam.members[ct].playerName + " with a Molotov";
                                    AddMessage(msg, terrorColor);
                                }
                            }
                        }
                    }

                }
            }
        }
        if (killMade)
            CheckFights(ingameTime);
    }
    void SimulateFightsForTimePassed(float ingameTime, float largestStep = 0.1f)
    {
        while (ingameTime > largestStep)
        {
            SimulateFightsForTimePassed(largestStep,largestStep);
            ingameTime -= largestStep;
        }

        bool killMade = false;

        bool[] ctAvailable = new bool[5] { true, true, true, true, true };
        bool[] tAvailable = new bool[5] { true, true, true, true, true };


        for (int j = 0; j < activeFights.Count; j++)
        {
            float timeLeft = ingameTime;
            if (activeFightsActivationLeft[j] <= 0)
            {
                int ct = min(activeFights[j].left, activeFights[j].right);
                int t = max(activeFights[j].left, activeFights[j].right) - 5;
                bool ctShoots = ct == activeFights[j].left;

                if (ctShoots)
                {
                    if (ctTeam.members[ct].hp == 0)
                        continue;
                    if (!ctAvailable[ct])
                        continue;
                    ctAvailable[ct] = false;
                    
                    if (ctTeam.members[ct].reloadTimeLeft > 0 && ctTeam.members[ct].reloadTimeLeft > timeLeft)
                    {
                        ctTeam.members[ct].reloadTimeLeft -= timeLeft;
                        continue;
                    }
                    else if (ctTeam.members[ct].reloadTimeLeft > 0)
                    {
                        timeLeft -= ctTeam.members[ct].reloadTimeLeft;
                        ctTeam.members[ct].reloadTimeLeft = 0;
                        ctTeam.members[ct].timeTillNextShot = 0f;
                        ctTeam.members[ct].bulletsLeft = weapons.weapons[ctTeam.members[ct].weapon].ammoSize;
                    }
                    while (timeLeft > 0)
                    {
                        if (ctTeam.members[ct].bulletsLeft == 0)
                        {
                            ctTeam.members[ct].reloadTimeLeft = weapons.weapons[ctTeam.members[ct].weapon].reloadTime;
                            if (timeLeft < ctTeam.members[ct].reloadTimeLeft)
                            {
                                ctTeam.members[ct].reloadTimeLeft -= timeLeft;
                                break;
                            }
                            else
                            {
                                timeLeft -= ctTeam.members[ct].reloadTimeLeft;
                                ctTeam.members[ct].reloadTimeLeft = 0;
                            }
                        }

                        if (ctTeam.members[ct].timeTillNextShot > 0f && timeLeft < ctTeam.members[ct].timeTillNextShot)
                        {
                            ctTeam.members[ct].timeTillNextShot -= timeLeft;
                            timeLeft = 0;
                            break;
                        }
                        else if (ctTeam.members[ct].timeTillNextShot > 0f)
                        {
                            timeLeft -= ctTeam.members[ct].timeTillNextShot;
                            ctTeam.members[ct].timeTillNextShot = 0f;
                        }

                        if (tTeam.members[t].hp == 0)
                            break;


                        if (ctTeam.members[ct].timeTillNextShot <= 0f)
                        {
                            float r = Random.value;
                            ctTeam.members[ct].timeTillNextShot += 1f / weapons.weapons[ctTeam.members[ct].weapon].fireRate;
                            ctTeam.members[ct].bulletsLeft--;
                            if ((activeFightsDistances[j] == 0 && r < weapons.weapons[ctTeam.members[ct].weapon].closeAccuracy)
                                || (activeFightsDistances[j] == 1 && r < weapons.weapons[ctTeam.members[ct].weapon].midAccuracy)
                                || (activeFightsDistances[j] == 2 && r < weapons.weapons[ctTeam.members[ct].weapon].longAccuracy))
                            {
                                float ctPower = ctTeam.members[ct].aim + ctTeam.members[ct].ctSkill;
                                if (weapons.weapons[ctTeam.members[ct].weapon].isPistol)
                                    ctPower += ctTeam.members[ct].pistolSkill;
                                if (weapons.weapons[ctTeam.members[ct].weapon].isSMG)
                                    ctPower += ctTeam.members[ct].smgSkill;
                                if (weapons.weapons[ctTeam.members[ct].weapon].isRifle)
                                    ctPower += ctTeam.members[ct].rifleSkill;
                                if (weapons.weapons[ctTeam.members[ct].weapon].isSniper)
                                    ctPower += ctTeam.members[ct].sniperSkill;
                                float tPower = tTeam.members[t].dodge * 2 + tTeam.members[t].tSkill;
                                if (ctTeam.members[ct].defending)
                                {
                                    ctPower += ctTeam.members[ct].mapDefense;
                                    tPower += tTeam.members[t].mapAttack;
                                    ctPower *= maps.maps[mapID].positions[ctTeam.members[ct].position].defensiveAdvantage[activeFightsDistances[j]];
                                }
                                else
                                {
                                    tPower += tTeam.members[t].mapDefense;
                                    ctPower += ctTeam.members[ct].mapAttack;
                                    tPower *= maps.maps[mapID].positions[ctTeam.members[t].position].defensiveAdvantage[activeFightsDistances[j]];
                                }
                                if (!ctTeam.members[ct].hasKevlar)
                                    ctPower *= nonkevlarInaccuracyMod;
                                ctPower *= maps.maps[mapID].ctAdvantage;
                                if (Random.value * (ctPower + tPower) < ctPower)
                                {
                                    int damage = 0;
                                    r = Random.value;
                                    if (r < hitChances[0])
                                    {
                                        if (activeFightsDistances[j] == 0)
                                            damage = weapons.weapons[ctTeam.members[ct].weapon].closeDmg[0];
                                        else if (activeFightsDistances[j] == 1)
                                            damage = weapons.weapons[ctTeam.members[ct].weapon].midDmg[0];
                                        else
                                            damage = weapons.weapons[ctTeam.members[ct].weapon].longDmg[0];
                                        if (tTeam.members[t].hasKevlar)
                                            damage = (int)(damage * weapons.weapons[ctTeam.members[ct].weapon].armorPenetration);
                                    }
                                    else if (r < hitChances[0] + hitChances[1])
                                    {
                                        if (activeFightsDistances[j] == 0)
                                            damage = weapons.weapons[ctTeam.members[ct].weapon].closeDmg[1];
                                        else if (activeFightsDistances[j] == 1)
                                            damage = weapons.weapons[ctTeam.members[ct].weapon].midDmg[1];
                                        else
                                            damage = weapons.weapons[ctTeam.members[ct].weapon].longDmg[1];
                                        if (tTeam.members[t].hasKevlar)
                                            damage = (int)(damage * weapons.weapons[ctTeam.members[ct].weapon].armorPenetration);
                                    }
                                    else if (r < hitChances[0] + hitChances[1] + hitChances[2])
                                    {
                                        if (activeFightsDistances[j] == 0)
                                            damage = weapons.weapons[ctTeam.members[ct].weapon].closeDmg[2];
                                        else if (activeFightsDistances[j] == 1)
                                            damage = weapons.weapons[ctTeam.members[ct].weapon].midDmg[2];
                                        else
                                            damage = weapons.weapons[ctTeam.members[ct].weapon].longDmg[2];
                                        if (tTeam.members[t].hasKevlar)
                                            damage = (int)(damage * weapons.weapons[ctTeam.members[ct].weapon].armorPenetration);
                                    }
                                    else
                                    {
                                        if (activeFightsDistances[j] == 0)
                                            damage = weapons.weapons[ctTeam.members[ct].weapon].closeDmg[3];
                                        else if (activeFightsDistances[j] == 1)
                                            damage = weapons.weapons[ctTeam.members[ct].weapon].midDmg[3];
                                        else
                                            damage = weapons.weapons[ctTeam.members[ct].weapon].longDmg[3];
                                    }
                                    tTeam.members[t].hp -= damage;
                                    ctTeam.members[ct].totalDamage += damage;
                                    if (tTeam.members[t].hp <= 0)
                                    {
                                        ctTeam.members[ct].totalDamage += tTeam.members[t].hp;
                                        tTeam.members[t].hp = 0;
                                        HandoutKillXp(ct, true);
                                        HandoutDeathXP(ct, false, t);
                                        int ctWeapon = ctTeam.members[ct].weapon;
                                        if (weapons.weapons[tTeam.members[t].weapon].DPSMid()
                                                > weapons.weapons[ctTeam.members[ct].weapon].DPSMid())
                                        {
                                            ctTeam.members[ct].weapon = tTeam.members[t].weapon;
                                            ctTeam.members[ct].timeTillNextShot = 1f / weapons.weapons[ctTeam.members[ct].weapon].fireRate;
                                            ctTeam.members[ct].bulletsLeft = tTeam.members[t].bulletsLeft;
                                        }
                                        if (ctTeam.members[ct].TotalGrenades() < 4) //Dead drops one grenade
                                        {
                                            if (tTeam.members[t].molotovCount > 0)
                                                ctTeam.members[ct].molotovCount++;
                                            else if (tTeam.members[t].hegCount > 0)
                                                ctTeam.members[ct].hegCount++;
                                            else if (tTeam.members[t].flashCount > 0)
                                                ctTeam.members[ct].flashCount++;
                                        }
                                        tTeam.members[t].weapon = 0;
                                        tTeam.members[t].deaths++;
                                        ctTeam.members[ct].kills++;
                                        ctTeam.money += matchSettings.killReward;
                                        killMade = true;
                                        string msg = ctTeam.members[ct].playerName + " killed " + tTeam.members[t].playerName + " with a " + weapons.weapons[ctWeapon].name;
                                        AddMessage(msg, ctColor);
                                    }
                                }
                            }
                        }

                        timeLeft -= 1f / weapons.weapons[ctTeam.members[ct].weapon].fireRate;
                    }
                }



                else //WHEN T SHOOTS
                {
                    if (tTeam.members[t].hp == 0)
                        continue;
                    if (!tAvailable[t])
                        continue;
                    tAvailable[t] = false;
                    if (tTeam.members[t].reloadTimeLeft > 0 && tTeam.members[t].reloadTimeLeft > timeLeft)
                    {
                        tTeam.members[t].reloadTimeLeft -= timeLeft;
                        continue;
                    }
                    else if (tTeam.members[t].reloadTimeLeft > 0)
                    {
                        timeLeft -= tTeam.members[t].reloadTimeLeft;
                        tTeam.members[t].reloadTimeLeft = 0;
                        tTeam.members[t].timeTillNextShot = 0f;
                        tTeam.members[t].bulletsLeft = weapons.weapons[tTeam.members[t].weapon].ammoSize;
                    }
                    while (timeLeft > 0)
                    {
                        if (tTeam.members[t].bulletsLeft == 0)
                        {
                            tTeam.members[t].reloadTimeLeft = weapons.weapons[tTeam.members[t].weapon].reloadTime;
                            if (timeLeft < tTeam.members[t].reloadTimeLeft)
                            {
                                tTeam.members[t].reloadTimeLeft -= timeLeft;
                                break;
                            }
                            else
                            {
                                timeLeft -= tTeam.members[t].reloadTimeLeft;
                                tTeam.members[t].reloadTimeLeft = 0;
                            }
                        }

                        if (tTeam.members[t].timeTillNextShot > 0f && timeLeft < tTeam.members[t].timeTillNextShot)
                        {
                            tTeam.members[t].timeTillNextShot -= timeLeft;
                            timeLeft = 0;
                            break;
                        }
                        else if (tTeam.members[t].timeTillNextShot > 0f)
                        {
                            timeLeft -= tTeam.members[t].timeTillNextShot;
                            tTeam.members[t].timeTillNextShot = 0f;
                        }
                        if (ctTeam.members[ct].hp == 0)
                            break;


                        if (tTeam.members[t].timeTillNextShot <= 0f)
                        {
                            float r = Random.value;
                            tTeam.members[t].timeTillNextShot += 1f / weapons.weapons[tTeam.members[t].weapon].fireRate;
                            tTeam.members[t].bulletsLeft--;
                            if ((activeFightsDistances[j] == 0 && r < weapons.weapons[tTeam.members[t].weapon].closeAccuracy)
                                || (activeFightsDistances[j] == 1 && r < weapons.weapons[tTeam.members[t].weapon].midAccuracy)
                                || (activeFightsDistances[j] == 2 && r < weapons.weapons[tTeam.members[t].weapon].longAccuracy))
                            {
                                float ctPower = ctTeam.members[ct].dodge * 2 + ctTeam.members[ct].ctSkill;
                                float tPower = tTeam.members[t].aim + tTeam.members[t].tSkill;
                                if (weapons.weapons[tTeam.members[t].weapon].isPistol)
                                    tPower += tTeam.members[t].pistolSkill;
                                if (weapons.weapons[tTeam.members[t].weapon].isSMG)
                                    tPower += tTeam.members[t].smgSkill;
                                if (weapons.weapons[tTeam.members[t].weapon].isRifle)
                                    tPower += tTeam.members[t].rifleSkill;
                                if (weapons.weapons[tTeam.members[t].weapon].isSniper)
                                    tPower += tTeam.members[t].sniperSkill;
                                if (ctTeam.members[ct].defending)
                                {
                                    ctPower += ctTeam.members[ct].mapDefense;
                                    tPower += tTeam.members[t].mapAttack;
                                    ctPower *= maps.maps[mapID].positions[ctTeam.members[ct].position].defensiveAdvantage[activeFightsDistances[j]];
                                }
                                else
                                {
                                    tPower += tTeam.members[t].mapDefense;
                                    ctPower += ctTeam.members[ct].mapAttack;
                                    tPower *= maps.maps[mapID].positions[ctTeam.members[t].position].defensiveAdvantage[activeFightsDistances[j]];
                                }
                                if (!tTeam.members[t].hasKevlar)
                                    tPower *= nonkevlarInaccuracyMod;
                                ctPower *= maps.maps[mapID].ctAdvantage;
                                if (Random.value * (ctPower + tPower) < tPower)
                                {
                                    int damage = 0;
                                    r = Random.value;
                                    if (r < hitChances[0])
                                    {
                                        if (activeFightsDistances[j] == 0)
                                            damage = weapons.weapons[tTeam.members[t].weapon].closeDmg[0];
                                        else if (activeFightsDistances[j] == 1)
                                            damage = weapons.weapons[tTeam.members[t].weapon].midDmg[0];
                                        else
                                            damage = weapons.weapons[tTeam.members[t].weapon].longDmg[0];
                                        if (ctTeam.members[ct].hasKevlar)
                                            damage = (int)(damage * weapons.weapons[tTeam.members[t].weapon].armorPenetration);
                                    }
                                    else if (r < hitChances[0] + hitChances[1])
                                    {
                                        if (activeFightsDistances[j] == 0)
                                            damage = weapons.weapons[tTeam.members[t].weapon].closeDmg[1];
                                        else if (activeFightsDistances[j] == 1)
                                            damage = weapons.weapons[tTeam.members[t].weapon].midDmg[1];
                                        else
                                            damage = weapons.weapons[tTeam.members[t].weapon].longDmg[1];
                                        if (ctTeam.members[ct].hasKevlar)
                                            damage = (int)(damage * weapons.weapons[tTeam.members[t].weapon].armorPenetration);
                                    }
                                    else if (r < hitChances[0] + hitChances[1] + hitChances[2])
                                    {
                                        if (activeFightsDistances[j] == 0)
                                            damage = weapons.weapons[tTeam.members[t].weapon].closeDmg[2];
                                        else if (activeFightsDistances[j] == 1)
                                            damage = weapons.weapons[tTeam.members[t].weapon].midDmg[2];
                                        else
                                            damage = weapons.weapons[tTeam.members[t].weapon].longDmg[2];
                                        if (ctTeam.members[ct].hasKevlar)
                                            damage = (int)(damage * weapons.weapons[tTeam.members[t].weapon].armorPenetration);
                                    }
                                    else
                                    {
                                        if (activeFightsDistances[j] == 0)
                                            damage = weapons.weapons[tTeam.members[t].weapon].closeDmg[3];
                                        else if (activeFightsDistances[j] == 1)
                                            damage = weapons.weapons[tTeam.members[t].weapon].midDmg[3];
                                        else
                                            damage = weapons.weapons[tTeam.members[t].weapon].longDmg[3];
                                    }
                                    ctTeam.members[ct].hp -= damage;
                                    tTeam.members[t].totalDamage += damage;
                                    if (ctTeam.members[ct].hp <= 0)
                                    {
                                        tTeam.members[t].totalDamage += ctTeam.members[ct].hp;
                                        ctTeam.members[ct].hp = 0;
                                        HandoutKillXp(t, false);
                                        HandoutDeathXP(t, false, ct);
                                        int tWeapon = tTeam.members[t].weapon;
                                        //WeaponSwap Part
                                        if (weapons.weapons[ctTeam.members[ct].weapon].DPSMid()
                                            > weapons.weapons[tTeam.members[t].weapon].DPSMid())
                                        {
                                            tTeam.members[t].weapon = ctTeam.members[ct].weapon;
                                            tTeam.members[t].timeTillNextShot = 1f / weapons.weapons[tTeam.members[t].weapon].fireRate;
                                            tTeam.members[t].bulletsLeft = ctTeam.members[ct].bulletsLeft;
                                        }
                                        if (tTeam.members[t].TotalGrenades() < 4) //Dead drops one grenade
                                        {
                                            if (ctTeam.members[ct].molotovCount > 0)
                                                tTeam.members[t].molotovCount++;
                                            else if (ctTeam.members[ct].hegCount > 0)
                                                tTeam.members[t].hegCount++;
                                            else if (ctTeam.members[ct].flashCount > 0)
                                                tTeam.members[t].flashCount++;
                                        }
                                        ctTeam.members[ct].deaths++;
                                        ctTeam.members[ct].weapon = 1;
                                        tTeam.members[t].kills++;
                                        tTeam.money += matchSettings.killReward;
                                        killMade = true;
                                        string msg = tTeam.members[t].playerName + " killed " + ctTeam.members[ct].playerName + " with a " + weapons.weapons[tWeapon].name;
                                        AddMessage(msg, terrorColor);
                                    }
                                }
                            }
                        }

                        timeLeft -= 1f / weapons.weapons[tTeam.members[t].weapon].fireRate;
                    }
                }
            }
        }

        if (killMade)
            CheckFights(ingameTime);
        SimulateGrenadesForTimePassed(ingameTime);
        TeamThinkMidRound();
    }

    void SimulateRotationsAndDefending(float ingameTime)
    {
        for(int i = 0; i < 5; i++)
        {
            if(ctTeam.members[i].hp == 0)
            {
                ctTeam.members[i].timeTillNextPosition = ctTeam.members[i].nextPositionTotalTime;
            }
            else if(ctTeam.members[i].timeTillNextPosition > 0f && ctTeam.members[i].position != ctTeam.members[i].nextPosition)
            {
                ctTeam.members[i].timeTillNextPosition -= ingameTime;
                if (ctTeam.members[i].timeTillNextPosition <= 0f)
                {
                    ctTeam.members[i].timeTillNextPosition = ctTeam.members[i].nextPositionTotalTime;
                    ctTeam.members[i].position = ctTeam.members[i].nextPosition;
                    ctTeam.members[i].defending = false;
                }
            }
            if (tTeam.members[i].hp == 0)
            {
                tTeam.members[i].timeTillNextPosition = tTeam.members[i].nextPositionTotalTime;
            }
            else if (tTeam.members[i].timeTillNextPosition > 0f && tTeam.members[i].position != tTeam.members[i].nextPosition)
            {
                tTeam.members[i].timeTillNextPosition -= ingameTime;
                if (tTeam.members[i].timeTillNextPosition <= 0f)
                {
                    tTeam.members[i].timeTillNextPosition = tTeam.members[i].nextPositionTotalTime;
                    tTeam.members[i].position = tTeam.members[i].nextPosition;
                    tTeam.members[i].defending = false;
                }
            }
        }

        for(int i = 0; i < 5; i++)
        {
            if(ctTeam.members[i].hp > 0 && !ctTeam.members[i].defending)
            {
                ctTeam.members[i].defending = true;
                for (int j = 0; j < 5; j++)
                    if (tTeam.members[j].hp > 0 && tTeam.members[j].position == ctTeam.members[i].position)
                        ctTeam.members[i].defending = false;
            }
            if (tTeam.members[i].hp > 0 && !tTeam.members[i].defending)
            {
                tTeam.members[i].defending = true;
                for (int j = 0; j < 5; j++)
                    if (ctTeam.members[j].hp > 0 && ctTeam.members[j].position == tTeam.members[i].position)
                        tTeam.members[i].defending = false;
            }
        }

    }

    void CheckForRoundEnd()
    {
        bool tAlive = false;
        bool ctAlive = false;
        for(int i = 0; i < 5; i++)
        {
            if (tTeam.members[i].hp > 0)
                tAlive = true;
            if (ctTeam.members[i].hp > 0)
                ctAlive = true;
        }

        if (ctAlive && !tAlive)
        {
            DistributeMoney(false);
            ctTeam.roundsWon++;

            ctTeam.lossBonus -= matchSettings.lossBonusLoss;
            if (ctTeam.lossBonus < 0)
                ctTeam.lossBonus = 0;
            tTeam.lossBonus += matchSettings.lossBonusAddition;
            if (tTeam.lossBonus > matchSettings.maxLossBonus)
                tTeam.lossBonus = matchSettings.maxLossBonus;
            
            string msg = "Team " + ctTeam.teamName + " has won the round";
            AddMessage(msg, ctColor);
        }
        else if (!ctAlive && tAlive)
        {
            DistributeMoney(true);
            tTeam.roundsWon++;

            tTeam.lossBonus -= matchSettings.lossBonusLoss;
            if (tTeam.lossBonus < 0)
                tTeam.lossBonus = 0;
            ctTeam.lossBonus += matchSettings.lossBonusAddition;
            if (ctTeam.lossBonus > matchSettings.maxLossBonus)
                ctTeam.lossBonus = matchSettings.maxLossBonus;

            string msg = "Team " + tTeam.teamName + " has won the round";
            AddMessage(msg, terrorColor);
        }
        else if (!ctAlive && !tAlive)
        {
            if (Random.value < 0.5f)
            {
                DistributeMoney(false);
                ctTeam.roundsWon++;

                ctTeam.lossBonus -= matchSettings.lossBonusLoss;
                if (ctTeam.lossBonus < 0)
                    ctTeam.lossBonus = 0;
                tTeam.lossBonus += matchSettings.lossBonusAddition;
                if (tTeam.lossBonus > matchSettings.maxLossBonus)
                    tTeam.lossBonus = matchSettings.maxLossBonus;
                string msg = "Team " + ctTeam.teamName + " has won the round";
                AddMessage(msg, ctColor);
            }
            else
            {
                DistributeMoney(true);
                tTeam.roundsWon++;

                tTeam.lossBonus -= matchSettings.lossBonusLoss;
                if (tTeam.lossBonus < 0)
                    tTeam.lossBonus = 0;
                ctTeam.lossBonus += matchSettings.lossBonusAddition;
                if (ctTeam.lossBonus > matchSettings.maxLossBonus)
                    ctTeam.lossBonus = matchSettings.maxLossBonus;
                string msg = "Team " + tTeam.teamName + " has won the round";
                AddMessage(msg, terrorColor);
            }
        }
        else return;

        string msg2 = "";
        if (ctTeam.myTeam)
            msg2 += ctTeam.teamName + " " + ctTeam.roundsWon + ":" + tTeam.roundsWon + " " + tTeam.teamName;
        else if (tTeam.myTeam)
            msg2 += tTeam.teamName + " " + tTeam.roundsWon + ":" + ctTeam.roundsWon + " " + ctTeam.teamName;
        else msg2 += ctTeam.teamName + " " + ctTeam.roundsWon + ":" + tTeam.roundsWon + " " + tTeam.teamName;

        AddMessage(msg2, msgColor);

        roundStarted = false;
        markedForEnd = false;
        hasUserBought = false;
        CheckForSwap();
        SetupRound();
        bombPlanted = -1;
    }

    void CheckForBombPlant()
    {
        if (bombPlanted != -1)
            return;

        for(int i = 0; i < 5; i++)
        {
            if(maps.maps[mapID].positions[tTeam.members[i].position].terrorTarget)
            {
                bombPlanted = tTeam.members[i].position;
                for (int j = 0; j < 5; j++)
                {
                    if (ctTeam.members[j].hp > 0 && ctTeam.members[j].position == tTeam.members[i].position)
                        bombPlanted = -1;
                }
                if (bombPlanted != -1)
                {
                    AddMessage("Bomb has been planted!", msgColor);
                    break;
                }
            }
        }
    }

    void CheckForSwap()
    {
        Debug.Log("T: " + tTeam.roundsWon + " | CT: " + ctTeam.roundsWon + " | Next: " + nextTeamSwap);
        if(ctTeam.roundsWon + tTeam.roundsWon == nextTeamSwap)
        {
            if(!halftimeHappened)
            {
                SwapTeamsHalftime();
                halftimeHappened = true;
                nextTeamSwap += matchSettings.halfTimeLength;
            }
            else
            {
                if (matchSettings.overtimePossible && ctTeam.roundsWon == clinchPoint - 1 && tTeam.roundsWon == clinchPoint - 1)
                {
                    clinchPoint += matchSettings.overtimeHalfTime;
                }
                SwapTeamsOT();
                nextTeamSwap += matchSettings.overtimeHalfTime;
            }
        }

    }

    void SimulateBigStep()
    {
        for (int i = 0; i < 10; i++)
        {
            float time = 1f;
            SimulateRotationsAndDefending(time);
            NoticeFights();
            CheckFights(time);
            HandoutTimedXP(time);
            CheckForBombPlant();
            SimulateFightsForTimePassed(time);
        }
    }



    void SimulateUltraBigStep()
    {
        for (int i = 0; i < 10; i++)
        {
            float time = 1f;
            SimulateRotationsAndDefending(time);
            NoticeFights();
            CheckFights(time);
            HandoutTimedXP(time);
            CheckForBombPlant();
            SimulateFightsForTimePassed(time,0.5f);
        }
    }

    void SimulateRoundStep()
    {
        float time = Time.deltaTime * matchSpeed;
        SimulateRotationsAndDefending(time);
        NoticeFights();
        CheckFights(time);
        HandoutTimedXP(time);
        CheckForBombPlant();
        SimulateFightsForTimePassed(time);
    }

    void ReviveAllPlayers()
    {
        for(int i = 0; i < 5; i++)
        {
            if(tTeam.members[i].hp == 0)
            {
                tTeam.members[i].position = maps.maps[mapID].positions.Length - 1;
                tTeam.members[i].weapon = 0;
                tTeam.members[i].hasKevlar = false;
                tTeam.members[i].flashCount = 0;
                tTeam.members[i].hegCount = 0;
                tTeam.members[i].molotovCount = 0;
            }
            if (ctTeam.members[i].hp == 0)
            {
                ctTeam.members[i].position = 0;
                ctTeam.members[i].weapon = 1;
                ctTeam.members[i].hasKevlar = false;
                ctTeam.members[i].flashCount = 0;
                ctTeam.members[i].hegCount = 0;
                ctTeam.members[i].molotovCount = 0;
            }
            tTeam.members[i].hp = 100;
            ctTeam.members[i].hp = 100;
        }
    }

    void RepositionPlayers()
    {
        for (int i = 0; i < 5; i++)
        {
            tTeam.members[i].position = maps.maps[mapID].positions.Length - 1;
            ctTeam.members[i].position = 0;
        }
    }

    void SetupRound()
    {
        ReviveAllPlayers();
        RepositionPlayers();
    }

    void ReinitializeTeams()
    {
        ctTeam.money = matchSettings.teamStartMoney;
        tTeam.money = matchSettings.teamStartMoney;
        for(int i = 0; i < 5; i++)
        {
            ctTeam.members[i].hasKevlar = false;
            tTeam.members[i].hasKevlar = false;

            ctTeam.members[i].flashCount = 0;
            tTeam.members[i].flashCount = 0;
            ctTeam.members[i].hegCount = 0;
            tTeam.members[i].hegCount = 0;
            ctTeam.members[i].molotovCount = 0;
            tTeam.members[i].molotovCount = 0;
        }
        SetupRound();
    }

    void ReinitializeTeamsOT()
    {
        ctTeam.money = matchSettings.overtimeStartMoney;
        tTeam.money = matchSettings.overtimeStartMoney;
        for (int i = 0; i < 5; i++)
        {
            ctTeam.members[i].hasKevlar = false;
            tTeam.members[i].hasKevlar = false;

            ctTeam.members[i].flashCount = 0;
            tTeam.members[i].flashCount = 0;
            ctTeam.members[i].hegCount = 0;
            tTeam.members[i].hegCount = 0;
            ctTeam.members[i].molotovCount = 0;
            tTeam.members[i].molotovCount = 0;
        }
        SetupRound();
    }

    public void MarkRoundForQuickEnd()
    {
        markedForEnd = true;
    }

    public void MarkMatchForQuickEnd()
    {
        markedForFullSimulate = true;
    }

    public void StartRound()
    {
        if (roundStarted || MatchFinished())
            return;

        TeamSimulateBuying(ctTeam, true);
        TeamSimulateBuying(tTeam, false);

        roundStarted = true;
        tTeam.attackingTactic = Random.Range(0, 7);


        if(tTeam.attackingTactic == 0) //Random
        {
            for (int i = 0; i < 5; i++)
            {
                StartRotatePlayer(false, i, Random.Range(0,4));
            }
        }
        else if(tTeam.attackingTactic == 1) //Straight A
        {
            for (int i = 0; i < 5; i++)
            {
                StartRotatePlayer(false, i, 1);
            }
        }
        else if (tTeam.attackingTactic == 2) //Straight B
        {
            for (int i = 0; i < 5; i++)
            {
                StartRotatePlayer(false, i, 3);
            }
        }
        else if (tTeam.attackingTactic == 3) //Split A
        {
            int straightA = Random.Range(2, 5);
            for (int i = 0; i < 5; i++)
            {
                tTeam.members[i].finalDestination = 1;
                if (i < straightA)
                    StartRotatePlayer(false, i, 1);
                else
                    StartRotatePlayer(false, i, 2, false);
            }
        }
        else if (tTeam.attackingTactic == 4) //Split B
        {
            int straightB = Random.Range(2, 5);
            for (int i = 0; i < 5; i++)
            {
                tTeam.members[i].finalDestination = 3;
                if (i < straightB)
                    StartRotatePlayer(false, i, 3);
                else
                    StartRotatePlayer(false, i, 3, false);
            }
        }
        else if (tTeam.attackingTactic == 5) //Mid A
        {
            for (int i = 0; i < 5; i++)
            {
                tTeam.members[i].finalDestination = 1;
                StartRotatePlayer(false, i, 2, false);
            }
        }
        else if (tTeam.attackingTactic == 6) //Mid A
        {
            for (int i = 0; i < 5; i++)
            {
                tTeam.members[i].finalDestination = 3;
                StartRotatePlayer(false, i, 2, false);
            }
        }

        Debug.Log("Attacking tactic handled");


        for (int i = 0; i < ctTeam.defendingPlayers.Length; i++)
        {
            var defPlayers = ctTeam.defendingPlayers[i];
            for (int j = 0; j < defPlayers.Count; j++)
            {
                StartRotatePlayer(true, defPlayers[j], i);
            }
        }


        for(int i = 0; i < 5; i++)
        {
            ctTeam.members[i].bulletsLeft = weapons.weapons[ctTeam.members[i].weapon].ammoSize;
            tTeam.members[i].bulletsLeft = weapons.weapons[tTeam.members[i].weapon].ammoSize;
        }
    }

    void TeamThinkMidRound()
    {
        if (bombPlanted != -1)
        {
            for (int i = 0; i < 5; i++)
            {
                if (tTeam.members[i].hp > 0)
                {
                    if (tTeam.members[i].finalDestination != bombPlanted)
                        StartRotatePlayer(false, i, bombPlanted);
                    if (tTeam.members[i].position == bombPlanted)
                    {
                        tTeam.members[i].nextPosition = bombPlanted;
                        tTeam.members[i].timeTillNextPosition = 0f;
                    }
                }
                if (ctTeam.members[i].hp > 0 && ctTeam.members[i].finalDestination != bombPlanted)
                    StartRotatePlayer(true, i, bombPlanted);
            }
        }


        for(int i = 0; i < 5; i++)
        {
            if (tTeam.members[i].hp == 0)
                continue;

            if(tTeam.members[i].position == tTeam.members[i].finalDestination)
            {
                bool noEnemy = true;
                for(int j = 0; j < 5; j++)
                {
                    if (ctTeam.members[j].hp > 0 && ctTeam.members[j].position == tTeam.members[i].position)
                        noEnemy = false;
                }
                if (noEnemy)
                    tTeam.members[i].finalDestination = Random.Range(0, maps.maps[mapID].positions.Length);
            }
            else if (tTeam.members[i].position == tTeam.members[i].nextPosition)
            {
                StartRotatePlayer(false, i, tTeam.members[i].finalDestination, false);
            }
        }

        for(int i = 0; i < 5; i++)
        {
            if (ctTeam.members[i].hp == 0)
                continue;
            if (ctTeam.members[i].position == ctTeam.members[i].nextPosition)
            {
                StartRotatePlayer(true, i, ctTeam.members[i].finalDestination, false);
            }
        }

    }

    void StartRotatePlayer(bool ct, int player, int finalDestination, bool changeFinal = true)
    {
        if (ct)
        {
            if (ctTeam.members[player].position == finalDestination)
                return;
            if (ctTeam.members[player].hp == 0)
                return;
            if (maps.maps[mapID].positions.Length <= finalDestination)
                return;
            int nextpos = maps.NextRoutePointBetween(mapID, ctTeam.members[player].position, finalDestination);
            int rotation = maps.IsThereRotationBetween(mapID, ctTeam.members[player].position, nextpos);
            ctTeam.members[player].timeTillNextPosition = maps.maps[mapID].rotations[rotation].rotateTime;
            ctTeam.members[player].nextPositionTotalTime = maps.maps[mapID].rotations[rotation].rotateTime;
            ctTeam.members[player].nextPosition = nextpos;
            if (changeFinal)
                ctTeam.members[player].finalDestination = finalDestination;


        }
        else
        {
            if (tTeam.members[player].position == finalDestination)
                return;
            if (tTeam.members[player].hp == 0)
                return;
            if (maps.maps[mapID].positions.Length <= finalDestination)
                return;
            int nextpos = maps.NextRoutePointBetween(mapID, tTeam.members[player].position, finalDestination);
            int rotation = maps.IsThereRotationBetween(mapID, tTeam.members[player].position, nextpos);
            tTeam.members[player].timeTillNextPosition = maps.maps[mapID].rotations[rotation].rotateTime;
            tTeam.members[player].nextPositionTotalTime = maps.maps[mapID].rotations[rotation].rotateTime;
            tTeam.members[player].nextPosition = nextpos;
            if (changeFinal)
                tTeam.members[player].finalDestination = finalDestination;

        }
    }

    void SwapTeams()
    {
        TeamUnit foo = ctTeam;
        ctTeam = tTeam;
        tTeam = foo;
        ReinitializeTeams();
        for (int i = 0; i < 5; i++)
        {
            ctTeam.members[i].weapon = 1;
            tTeam.members[i].weapon = 0;
        }
    }

    void SwapTeamsOT()
    {
        TeamUnit foo = ctTeam;
        ctTeam = tTeam;
        tTeam = foo;
        ReinitializeTeamsOT();
        for (int i = 0; i < 5; i++)
        {
            ctTeam.members[i].weapon = 1;
            tTeam.members[i].weapon = 0;
        }
    }

    void SwapTeamsHalftime()
    {
        SwapTeams();
        AddMessage("Half time", msgColor);
    }

    void SwapTeamsOvertime()
    {
        SwapTeamsOT();
        AddMessage("Overtime Team Swap", msgColor);
    }

    void BeginMatch()
    {
        if (Random.value < 0.5f)
        {
            SwapTeams();
        }
        ReinitializeTeams();
        nextTeamSwap = matchSettings.halfTimeLength;
    }


    void DistributeMoney(bool terrorWin)
    {
        if(terrorWin)
        {
            tTeam.money += matchSettings.terrorWinReward * 5;
            ctTeam.money += (matchSettings.ctLossReward + matchSettings.lossBonusReward * ctTeam.lossBonus) * 5;
        }
        else
        {
            ctTeam.money += matchSettings.ctWinReward * 5;
            tTeam.money += (matchSettings.terrorLoseReward + matchSettings.lossBonusReward * tTeam.lossBonus) * 5;
            if (bombPlanted != -1)
                tTeam.money += 5 * matchSettings.bombPlantReward;
        }

        if (tTeam.money > matchSettings.teamMaxMoney)
            tTeam.money = matchSettings.teamMaxMoney;


        if (ctTeam.money > matchSettings.teamMaxMoney)
            ctTeam.money = matchSettings.teamMaxMoney;
    }

    void TeamSimulateBuying(TeamUnit team, bool ct)
    {
        if (team.myTeam && hasUserBought)
            return;
        int nrMoneyGainIfLost = 5 * (matchSettings.terrorLoseReward + matchSettings.lossBonusReward * team.lossBonus);
        if (ct)
            nrMoneyGainIfLost = 5 * (matchSettings.ctLossReward + matchSettings.lossBonusReward * team.lossBonus);


        if (team.money < 7000) //Pistol, pistol force
        {
            if (Random.value > 0.9f && ctTeam.roundsWon + tTeam.roundsWon == 0) //Eco later
                return;

            int playerPortion = team.money / 4;
            for (int i = 0; i < 5; i++)
            {
                int avMoney = playerPortion;
                if (i >= 3)
                    avMoney = team.money / (5 - i);
                bool armored = Random.value < 0.85f;
                if (armored && !team.members[i].hasKevlar)
                {
                    if (team.money > 650)
                    {
                        team.money -= 650;
                        avMoney -= 650;
                        team.members[i].hasKevlar = true;
                    }
                }
                else if (Random.value < 0.5f)
                {
                    for(int j = 0; j < 2; j++)
                    {
                        if (team.members[i].TotalGrenades() >= 4)
                            continue;
                        if (avMoney < 300)
                            continue;
                        if(Random.value < 0.5f)
                        {
                            team.members[i].flashCount++;
                            team.money -= 200;
                            avMoney -= 200;
                        }
                        else if (Random.value < 0.5f)
                        {
                            team.members[i].hegCount++;
                            team.money -= 300;
                            avMoney -= 300;
                        }
                        else 
                        {
                            team.members[i].molotovCount++;
                            team.money -= 400;
                            avMoney -= 400;
                        }

                    }
                }
                List<int> avWep = new List<int>();
                for (int j = 0; j < weapons.weapons.Length; j++)
                {
                    if (weapons.weapons[j].cost < avMoney && weapons.weapons[j].cost > weapons.weapons[team.members[i].weapon].cost
                        && weapons.weapons[j].DPSClose() > weapons.weapons[team.members[i].weapon].DPSClose())
                    {
                        if ((ct && weapons.weapons[j].CTBuyable)
                            || (!ct && weapons.weapons[j].TBuyable))
                            avWep.Add(j);
                    }
                }
                if (avWep.Count > 0)
                {
                    int buyIndex = 0;
                    for (int j = 0; j < avWep.Count; j++)
                        if (weapons.weapons[avWep[j]].DPSClose() > weapons.weapons[buyIndex].DPSClose())
                            buyIndex = avWep[j];
                    team.money -= weapons.weapons[buyIndex].cost;
                    team.members[i].weapon = buyIndex;
                    team.members[i].bulletsLeft = weapons.weapons[buyIndex].ammoSize;
                }
            }
        }
        else if (team.money < 20000 && team.money + nrMoneyGainIfLost > 20000) //Eco for full buy
        {
            int playerPortion = 700;
            for (int i = 0; i < 5; i++)
            {
                int avMoney = playerPortion;
                bool armored = Random.value < 0.5f;
                if (armored && !team.members[i].hasKevlar)
                {
                    if (team.money > 650)
                    {
                        team.money -= 650;
                        avMoney -= 650;
                        team.members[i].hasKevlar = true;
                    }
                }
                else if (Random.value < 0.6f)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (team.members[i].TotalGrenades() >= 4)
                            continue;
                        if (avMoney < 300)
                            continue;
                        if (Random.value < 0.5f)
                        {
                            team.members[i].flashCount++;
                            team.money -= 200;
                            avMoney -= 200;
                        }
                        else if (Random.value < 0.5f)
                        {
                            team.members[i].hegCount++;
                            team.money -= 300;
                            avMoney -= 300;
                        }
                        else
                        {
                            team.members[i].molotovCount++;
                            team.money -= 400;
                            avMoney -= 400;
                        }

                    }
                }
                List<int> avWep = new List<int>();
                for (int j = 0; j < weapons.weapons.Length; j++)
                {
                    if (weapons.weapons[j].cost < avMoney && weapons.weapons[j].cost > weapons.weapons[team.members[i].weapon].cost
                    && weapons.weapons[j].DPSClose() > weapons.weapons[team.members[i].weapon].DPSClose())
                    {
                        if ((ct && weapons.weapons[j].CTBuyable)
                            || (!ct && weapons.weapons[j].TBuyable))
                            avWep.Add(j);
                    }
                }
                if (avWep.Count > 0)
                {
                    int buyIndex = 0;
                    for (int j = 0; j < avWep.Count; j++)
                        if (weapons.weapons[avWep[j]].DPSClose() > weapons.weapons[buyIndex].DPSClose())
                            buyIndex = avWep[j];
                    team.money -= weapons.weapons[buyIndex].cost;
                    team.members[i].weapon = buyIndex;
                    team.members[i].bulletsLeft = weapons.weapons[buyIndex].ammoSize;
                }
            }
        }
        else if (team.money < 20000) //Pistol, pistol force
        {
            if (Random.value > 0.8f) //Eco later
                return;

            int playerPortion = team.money / 4;
            for (int i = 0; i < 5; i++)
            {
                int avMoney = playerPortion;
                if (i >= 3)
                    avMoney = team.money / (5 - i);
                bool armored = Random.value < 0.95f;
                if (armored && !team.members[i].hasKevlar)
                {
                    if (team.money > 650)
                    {
                        team.money -= 650;
                        avMoney -= 650;
                        team.members[i].hasKevlar = true;
                    }
                }
                List<int> avWep = new List<int>();
                for (int j = 0; j < weapons.weapons.Length; j++)
                {
                    if (weapons.weapons[j].cost < avMoney && weapons.weapons[j].cost > weapons.weapons[team.members[i].weapon].cost
                    && weapons.weapons[j].DPSMid() > weapons.weapons[team.members[i].weapon].DPSMid())
                    {
                        if ((ct && weapons.weapons[j].CTBuyable)
                            || (!ct && weapons.weapons[j].TBuyable))
                            avWep.Add(j);
                    }
                }
                if (avWep.Count > 0)
                {
                    int buyIndex = 0;
                    for (int j = 0; j < avWep.Count; j++)
                        if (weapons.weapons[avWep[j]].DPSMid() > weapons.weapons[buyIndex].DPSMid())
                            buyIndex = avWep[j];
                    team.money -= weapons.weapons[buyIndex].cost;
                    team.members[i].weapon = buyIndex;
                    team.members[i].bulletsLeft = weapons.weapons[buyIndex].ammoSize;
                }
                {
                    int upLimit = Random.Range(1, 5);
                    for (int j = 0; j < upLimit; j++)
                    {
                        if (team.members[i].TotalGrenades() >= 4)
                            continue;
                        if (avMoney < 300)
                            continue;
                        if (Random.value < 0.5f)
                        {
                            team.members[i].flashCount++;
                            team.money -= 200;
                            avMoney -= 200;
                        }
                        else if (Random.value < 0.5f)
                        {
                            team.members[i].hegCount++;
                            team.money -= 300;
                            avMoney -= 300;
                        }
                        else
                        {
                            team.members[i].molotovCount++;
                            team.money -= 400;
                            avMoney -= 400;
                        }

                    }
                }
            }
        }
        else
        {
            int playerPortion = team.money / 4;
            for (int i = 0; i < 5; i++)
            {
                int avMoney = playerPortion;
                if (i >= 3)
                    avMoney = team.money / (5 - i);
                bool armored = Random.value < 1f;
                if (armored && !team.members[i].hasKevlar)
                {
                    if (team.money > 650)
                    {
                        team.money -= 650;
                        avMoney -= 650;
                        team.members[i].hasKevlar = true;
                    }
                }
                List<int> avWep = new List<int>();
                for (int j = 0; j < weapons.weapons.Length; j++)
                {
                    if (weapons.weapons[j].cost < avMoney && weapons.weapons[j].cost > weapons.weapons[team.members[i].weapon].cost
                    && weapons.weapons[j].DPSMid() > weapons.weapons[team.members[i].weapon].DPSMid())
                    {
                        if ((ct && weapons.weapons[j].CTBuyable)
                            || (!ct && weapons.weapons[j].TBuyable))
                            avWep.Add(j);
                    }
                }
                if (avWep.Count > 0)
                {
                    int buyIndex = 0;
                    for (int j = 0; j < avWep.Count; j++)
                        if (weapons.weapons[avWep[j]].DPSMid() > weapons.weapons[buyIndex].DPSMid())
                            buyIndex = avWep[j];
                    team.money -= weapons.weapons[buyIndex].cost;
                    team.members[i].weapon = buyIndex;
                    team.members[i].bulletsLeft = weapons.weapons[buyIndex].ammoSize;
                }
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (team.members[i].TotalGrenades() >= 4)
                            continue;
                        if (avMoney < 300)
                            continue;
                        if (Random.value < 0.5f)
                        {
                            team.members[i].flashCount++;
                            team.money -= 200;
                            avMoney -= 200;
                        }
                        else if (Random.value < 0.5f)
                        {
                            team.members[i].hegCount++;
                            team.money -= 300;
                            avMoney -= 300;
                        }
                        else
                        {
                            team.members[i].molotovCount++;
                            team.money -= 400;
                            avMoney -= 400;
                        }

                    }
                }

            }
        }
    }

    public void UserEcoCall()
    {
        hasUserBought = true;

    }

    public void UserForceCall()
    {
        hasUserBought = true;
        bool ct = true;
        TeamUnit team;
        if (ctTeam.myTeam)
            team = ctTeam;
        else if (tTeam.myTeam)
        {
            ct = false;
            team = tTeam;
        }
        else return;

        {

            int playerPortion = team.money / 8;
            for (int i = 0; i < 5; i++)
            {
                int avMoney = playerPortion;
                if (i >= 3)
                    avMoney = team.money / (8 - i);
                bool armored = Random.value < 0.85f;
                if (armored && !team.members[i].hasKevlar)
                {
                    if (team.money > 650)
                    {
                        team.money -= 650;
                        avMoney -= 650;
                        team.members[i].hasKevlar = true;
                    }
                }
                else if (Random.value < 0.5f)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        if (team.members[i].TotalGrenades() >= 4)
                            continue;
                        float r = Random.value;
                        if (r < 0.5)
                        {
                            team.members[i].flashCount++;
                            team.money -= 200;
                            avMoney -= 200;
                        }
                        else if (r < 0.75)
                        {
                            team.members[i].hegCount++;
                            team.money -= 300;
                            avMoney -= 300;
                        }
                        else
                        {
                            team.members[i].molotovCount++;
                            team.money -= 300;
                            avMoney -= 300;
                        }
                    }
                }

                List<int> avWep = new List<int>();
                for (int j = 0; j < weapons.weapons.Length; j++)
                {
                    if (weapons.weapons[j].cost < avMoney && weapons.weapons[j].cost > weapons.weapons[team.members[i].weapon].cost
                        && weapons.weapons[j].DPSClose() > weapons.weapons[team.members[i].weapon].DPSClose())
                    {
                        if ((ct && weapons.weapons[j].CTBuyable)
                            || (!ct && weapons.weapons[j].TBuyable))
                            avWep.Add(j);
                    }
                }
                if (avWep.Count > 0)
                {
                    int buyIndex = 0;
                    for (int j = 0; j < avWep.Count; j++)
                        if (weapons.weapons[avWep[j]].DPSClose() > weapons.weapons[buyIndex].DPSClose())
                            buyIndex = avWep[j];
                    team.money -= weapons.weapons[buyIndex].cost;
                    team.members[i].weapon = buyIndex;
                    team.members[i].bulletsLeft = weapons.weapons[buyIndex].ammoSize;
                }
            }
        }
    }

    public void UserForceCallNades()
    {
        hasUserBought = true;
        bool ct = true;
        TeamUnit team;
        if (ctTeam.myTeam)
            team = ctTeam;
        else if (tTeam.myTeam)
        {
            ct = false;
            team = tTeam;
        }
        else return;

        {

            int playerPortion = team.money / 8;
            for (int i = 0; i < 5; i++)
            {
                int avMoney = playerPortion;
                if (i >= 3)
                    avMoney = team.money / (8 - i);
                bool armored = Random.value < 0.85f;
                float grenades = Random.Range(0f, 4f);
                if (armored && !team.members[i].hasKevlar)
                {
                    if (team.money > 650)
                    {
                        team.money -= 650;
                        avMoney -= 650;
                        team.members[i].hasKevlar = true;
                    }
                }
                else grenades += 1.5f;

                if (grenades > 4)
                    grenades = 4;

                for(int j = 0; j < grenades; j++)
                {
                    if (team.members[i].TotalGrenades() >= 4)
                        continue;
                    if (avMoney < 400)
                        continue;
                    float r = Random.value;
                    if(r < 0.5)
                    {
                        team.members[i].flashCount++;
                        team.money -= 200;
                        avMoney -= 200;
                    }
                    else  if (r < 0.75)
                    {
                        team.members[i].hegCount++;
                        team.money -= 300;
                        avMoney -= 300;
                    }
                    else
                    {
                        team.members[i].molotovCount++;
                        team.money -= 300;
                        avMoney -= 300;
                    }
                }

                List<int> avWep = new List<int>();
                for (int j = 0; j < weapons.weapons.Length; j++)
                {
                    if (weapons.weapons[j].cost < avMoney && weapons.weapons[j].cost > weapons.weapons[team.members[i].weapon].cost
                        && weapons.weapons[j].DPSClose() > weapons.weapons[team.members[i].weapon].DPSClose())
                    {
                        if ((ct && weapons.weapons[j].CTBuyable)
                            || (!ct && weapons.weapons[j].TBuyable))
                            avWep.Add(j);
                    }
                }
                if (avWep.Count > 0)
                {
                    int buyIndex = 0;
                    for (int j = 0; j < avWep.Count; j++)
                        if (weapons.weapons[avWep[j]].DPSClose() > weapons.weapons[buyIndex].DPSClose())
                            buyIndex = avWep[j];
                    team.money -= weapons.weapons[buyIndex].cost;
                    team.members[i].weapon = buyIndex;
                    team.members[i].bulletsLeft = weapons.weapons[buyIndex].ammoSize;
                }
            }
        }
    }

    public void UserFullBuyNades()
    {
        hasUserBought = true;
        bool ct = true;
        TeamUnit team;
        if (ctTeam.myTeam)
            team = ctTeam;
        else if (tTeam.myTeam)
        {
            ct = false;
            team = tTeam;
        }
        else return;

        {

            int playerPortion = team.money / 4;
            for (int i = 0; i < 5; i++)
            {
                int avMoney = playerPortion;
                if (i >= 3)
                    avMoney = team.money / (5 - i);
                bool armored = Random.value < 0.85f;

                if (armored && !team.members[i].hasKevlar)
                {
                    if (team.money > 650)
                    {
                        team.money -= 650;
                        avMoney -= 650;
                        team.members[i].hasKevlar = true;
                    }
                }
                

                List<int> avWep = new List<int>();
                for (int j = 0; j < weapons.weapons.Length; j++)
                {
                    if (weapons.weapons[j].cost < avMoney && weapons.weapons[j].cost > weapons.weapons[team.members[i].weapon].cost
                        && weapons.weapons[j].DPSMid() > weapons.weapons[team.members[i].weapon].DPSMid())
                    {
                        if ((ct && weapons.weapons[j].CTBuyable)
                            || (!ct && weapons.weapons[j].TBuyable))
                            avWep.Add(j);
                    }
                }
                if (avWep.Count > 0)
                {
                    int buyIndex = 0;
                    for (int j = 0; j < avWep.Count; j++)
                        if (weapons.weapons[avWep[j]].DPSMid() > weapons.weapons[buyIndex].DPSMid())
                            buyIndex = avWep[j];
                    team.money -= weapons.weapons[buyIndex].cost;
                    team.members[i].weapon = buyIndex;
                    team.members[i].bulletsLeft = weapons.weapons[buyIndex].ammoSize;
                }

                for (int j = 0; j < 4; j++)
                {
                    if (team.members[i].TotalGrenades() >= 4)
                        continue;
                    if (avMoney < 400)
                        continue;
                    float r = Random.value;
                    if (r < 0.5)
                    {
                        team.members[i].flashCount++;
                        team.money -= 200;
                        avMoney -= 200;
                    }
                    else if (r < 0.75)
                    {
                        team.members[i].hegCount++;
                        team.money -= 300;
                        avMoney -= 300;
                    }
                    else
                    {
                        team.members[i].molotovCount++;
                        team.money -= 300;
                        avMoney -= 300;
                    }
                }
            }
        }
    }
    public void UserFullBuy()
    {
        hasUserBought = true;
        bool ct = true;
        TeamUnit team;
        if (ctTeam.myTeam)
            team = ctTeam;
        else if (tTeam.myTeam)
        {
            ct = false;
            team = tTeam;
        }
        else return;

        {

            int playerPortion = team.money / 4;
            for (int i = 0; i < 5; i++)
            {
                int avMoney = playerPortion;
                if (i >= 3)
                    avMoney = team.money / (5 - i);
                bool armored = Random.value < 0.85f;
                if (armored && !team.members[i].hasKevlar)
                {
                    if (team.money > 650)
                    {
                        team.money -= 650;
                        avMoney -= 650;
                        team.members[i].hasKevlar = true;
                    }
                }
                List<int> avWep = new List<int>();
                for (int j = 0; j < weapons.weapons.Length; j++)
                {
                    if (weapons.weapons[j].cost < avMoney && weapons.weapons[j].cost > weapons.weapons[team.members[i].weapon].cost
                        && weapons.weapons[j].DPSMid() > weapons.weapons[team.members[i].weapon].DPSMid())
                    {
                        if ((ct && weapons.weapons[j].CTBuyable)
                            || (!ct && weapons.weapons[j].TBuyable))
                            avWep.Add(j);
                    }
                }
                if (avWep.Count > 0)
                {
                    int buyIndex = 0;
                    for (int j = 0; j < avWep.Count; j++)
                        if (weapons.weapons[avWep[j]].DPSMid() > weapons.weapons[buyIndex].DPSMid())
                            buyIndex = avWep[j];
                    team.money -= weapons.weapons[buyIndex].cost;
                    team.members[i].weapon = buyIndex;
                    team.members[i].bulletsLeft = weapons.weapons[buyIndex].ammoSize;
                }
            }
        }

    }

    bool MatchFinished()
    {
        if(!matchSettings.overtimePossible)
        {
            if (tTeam.roundsWon == matchSettings.halfTimeLength + 1
                || ctTeam.roundsWon == matchSettings.halfTimeLength + 1 ||
                tTeam.roundsWon + ctTeam.roundsWon == 2 * matchSettings.halfTimeLength)
                return true;
            else
                return false;
        }
        else
        {
            if (tTeam.roundsWon == clinchPoint ||
                ctTeam.roundsWon == clinchPoint)
                return true;
        }
        return false;
    }

    void HandoutKillXp(int i, bool ct)
    {
        if (ct)
            ctTeam.members[i].xpCollected += xpForKill;
        else
            tTeam.members[i].xpCollected += xpForKill;
    }

    void HandoutDeathXP(int a, bool ct, int b)
    {
        if (ct)
        {
            if (ctTeam.members[a].aim > tTeam.members[b].aim
                && Random.value < xpDeathHigherSkillRate)
                tTeam.members[b].xpCollected++;
            if (ctTeam.members[a].dodge > tTeam.members[b].dodge
                && Random.value < xpDeathHigherSkillRate)
                tTeam.members[b].xpCollected++;
            if (ctTeam.members[a].intelligence > tTeam.members[b].intelligence
                && Random.value < xpDeathHigherSkillRate)
                tTeam.members[b].xpCollected++;
            if (ctTeam.members[a].ctSkill > tTeam.members[b].tSkill
                && Random.value < xpDeathHigherSkillRate)
                tTeam.members[b].xpCollected++;
        }
        else
        {
            if (tTeam.members[a].aim > ctTeam.members[b].aim
                && Random.value < xpDeathHigherSkillRate)
                ctTeam.members[b].xpCollected++;
            if (tTeam.members[a].dodge > ctTeam.members[b].dodge
                && Random.value < xpDeathHigherSkillRate)
                ctTeam.members[b].xpCollected++;
            if (tTeam.members[a].intelligence > ctTeam.members[b].intelligence
                && Random.value < xpDeathHigherSkillRate)
                ctTeam.members[b].xpCollected++;
            if (tTeam.members[a].ctSkill > ctTeam.members[b].tSkill
                && Random.value < xpDeathHigherSkillRate)
                ctTeam.members[b].xpCollected++;
        }
    }

    public void HandoutTimedXP(float time)
    {
        for(int i = 0; i < 5; i++)
        {
            if (tTeam.members[i].hp > 0 && Random.value < time * timeAliveXPRate)
                tTeam.members[i].xpCollected++;
            if (ctTeam.members[i].hp > 0 && Random.value < time * timeAliveXPRate)
                ctTeam.members[i].xpCollected++;
        }
        for(int i = 0; i < activeFights.Count; i++)
        {
            if(activeFights[i].left < 5)
            {
                if (Random.value < time * timeInFightXPRate)
                    ctTeam.members[activeFights[i].left].xpCollected++;
            }
            else
            {
                if (Random.value < time * timeInFightXPRate)
                    tTeam.members[activeFights[i].left - 5].xpCollected++;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (roundStarted)
        {
            
            if(markedForFullSimulate)
            {
                while(!MatchFinished())
                {
                    StartRound();
                    SimulateUltraBigStep();
                    CheckForRoundEnd();
                }
            }
            if (markedForEnd)
            {
                SimulateBigStep();
                CheckForRoundEnd();
            }
            else
            {
                SimulateRoundStep();
                CheckForRoundEnd();
            }
        }
    }
}
