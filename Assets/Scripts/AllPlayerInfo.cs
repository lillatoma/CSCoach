using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllPlayerInfo : MonoBehaviour
{
    public SmallPlayerInfo[] playerInfos;
    public DetailedPlayerInfo detailedInfo;

    private Team team;
    // Start is called before the first frame update
    void Start()
    {
        team = FindObjectOfType<Team>();
        for(int i = 0; i < playerInfos.Length; i++)
        {
            if (team.players.Count >= i)
                playerInfos[i].gameObject.SetActive(false);
            else
                playerInfos[i].LinkPlayer(team.players[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
