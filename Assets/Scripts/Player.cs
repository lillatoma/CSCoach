using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
