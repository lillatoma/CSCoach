using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MatchUIManager : MonoBehaviour
{
    public ScoreboardTeam teamA;
    public ScoreboardTeam teamB;
    public TMP_Text speedButtonText;


    private Match match;
    private WeaponList weapons;
    private MapList maps;
    private MatchConsole matchConsole;

    public float matchSpeed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        match = GetComponent<Match>();
        weapons = FindObjectOfType<WeaponList>();
        maps = FindObjectOfType<MapList>();
        matchConsole = FindObjectOfType<MatchConsole>();
    }

    void UpdateTeamInfoA()
    {
        TeamUnit tm;

        if (match.ctTeam.myTeam)
            tm = match.ctTeam;
        else if (match.tTeam.myTeam)
            tm = match.tTeam;
        else tm = match.ctTeam;

        teamA.nameText.text = tm.teamName;
        teamA.moneyText.text = "$" + tm.money;
        teamA.roundText.text = tm.roundsWon.ToString() + " Won";

        List<int> order = new List<int>();

        for(int i = 0; i < 5; i++)
        {
            order.Add(i);
        }

        for(int i = 0; i < 5; i++)
        {
            int ind = order[0];
            for (int j = 1; j < order.Count; j++)
                if (tm.members[order[j]].totalDamage > tm.members[ind].totalDamage)
                    ind = order[j];
            order.Remove(ind);

            teamA.players[i].nameText.text = tm.members[ind].playerName;
            teamA.players[i].killText.text = tm.members[ind].kills.ToString() + "K";
            teamA.players[i].deathText.text = tm.members[ind].deaths.ToString() + "D";
            int roundCount = match.ctTeam.roundsWon + match.tTeam.roundsWon;
            if (roundCount == 0)
                roundCount = 1;
            int adr = tm.members[ind].totalDamage / roundCount;
            teamA.players[i].adrText.text = adr.ToString() + "ADR";

            int totalNades = tm.members[ind].flashCount + tm.members[ind].hegCount + tm.members[ind].molotovCount;
            string weaponText = weapons.weapons[tm.members[ind].weapon].name;
            if (totalNades > 0)
                weaponText += " + " + totalNades + "GRN";
            teamA.players[i].weaponText.text = weaponText;

            teamA.players[i].kevlarText.text = (tm.members[ind].hasKevlar) ? "X" : "";
            teamA.players[i].positionText.text = maps.maps[match.mapID].positions[tm.members[ind].position].name;

            teamA.players[i].SetHP(tm.members[ind].hp);

            float percent = (tm.members[ind].nextPositionTotalTime - tm.members[ind].timeTillNextPosition) / tm.members[ind].nextPositionTotalTime;
            if (tm.members[ind].nextPositionTotalTime == 0)
                percent = 0f;
            else if (percent > 1f)
                percent = 1f;
            teamA.players[i].SetPosition(percent);

        }

    }

    void UpdateTeamInfoB()
    {
        TeamUnit tm;

        if (match.ctTeam.myTeam)
            tm = match.tTeam;
        else if (match.tTeam.myTeam)
            tm = match.ctTeam;
        else tm = match.tTeam;

        teamB.nameText.text = tm.teamName;
        teamB.moneyText.text = "$" + tm.money;
        teamB.roundText.text = tm.roundsWon.ToString() + " Won";

        List<int> order = new List<int>();

        for (int i = 0; i < 5; i++)
        {
            order.Add(i);
        }

        for (int i = 0; i < 5; i++)
        {
            int ind = order[0];
            for (int j = 1; j < order.Count; j++)
                if (tm.members[order[j]].totalDamage > tm.members[ind].totalDamage)
                    ind = order[j];
            order.Remove(ind);

            teamB.players[i].nameText.text = tm.members[ind].playerName;
            teamB.players[i].killText.text = tm.members[ind].kills.ToString() + "K";
            teamB.players[i].deathText.text = tm.members[ind].deaths.ToString() + "D";
            int roundCount = match.ctTeam.roundsWon + match.tTeam.roundsWon;
            if (roundCount == 0)
                roundCount = 1;
            int adr = tm.members[ind].totalDamage / roundCount;
            teamB.players[i].adrText.text = adr.ToString() + "ADR";


            int totalNades = tm.members[ind].flashCount + tm.members[ind].hegCount + tm.members[ind].molotovCount;
            string weaponText = weapons.weapons[tm.members[ind].weapon].name;
            if (totalNades > 0)
                weaponText += " + " + totalNades + "GRN";
            teamB.players[i].weaponText.text = weaponText;

            teamB.players[i].kevlarText.text = (tm.members[ind].hasKevlar) ? "X" : "";
            teamB.players[i].positionText.text = maps.maps[match.mapID].positions[tm.members[ind].position].name;

            teamB.players[i].SetHP(tm.members[ind].hp);
            float percent = (tm.members[ind].nextPositionTotalTime - tm.members[ind].timeTillNextPosition) / tm.members[ind].nextPositionTotalTime;
            if (tm.members[ind].nextPositionTotalTime == 0)
                percent = 0f;
            else if (percent > 1f)
                percent = 1f;
            teamB.players[i].SetPosition(percent);

        }

    }

    void UpdateConsoleTexts()
    {
        for(int i = 0; i < 13; i++)
        {
            if(match.messages.Count <= i)
            {
                float alpha = 0.4f;
                if (i % 2 == 1)
                    alpha -= 0.1f;
                matchConsole.printedMessageBGs[i].color = new Color(1, 1, 1, alpha);
                matchConsole.printedMessages[i].text = "";
            }
            else
            {
                matchConsole.printedMessages[i].text = match.messages[i];
                float alpha = 0.4f;
                if (i % 2 == 1)
                    alpha -= 0.1f;
                matchConsole.printedMessageBGs[i].color = 
                    new Color(match.messageColors[i].r, 
                    match.messageColors[i].g, 
                    match.messageColors[i].b, alpha);
            }
        }
    }


    public void StartRound()
    {
        match.StartRound();
    }

    public void SimulateRound()
    {
        match.MarkRoundForQuickEnd();
    }

    public void SimulateMatch()
    {
        match.MarkMatchForQuickEnd();
    }

    public void SpeedButton()
    {
        if (matchSpeed == 1f)
            matchSpeed = 3f;
        else if (matchSpeed == 3f)
            matchSpeed = 10f;
        else if (matchSpeed == 10f)
            matchSpeed = 1f;

        match.matchSpeed = matchSpeed;

        int sp = (int)matchSpeed;
        speedButtonText.text = sp.ToString() + "X";

    }

    public void EcoButton()
    {
        match.UserEcoCall();
    }

    public void ForceButton()
    {
        match.UserForceCall();
    }
    public void ForceNadesButton()
    {
        match.UserForceCallNades();
    }
    public void FullBuyButton()
    {
        match.UserFullBuy();
    }

    public void FullBuyNadesButton()
    {
        match.UserFullBuyNades();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTeamInfoA();
        UpdateTeamInfoB();
        UpdateConsoleTexts();
    }
}
