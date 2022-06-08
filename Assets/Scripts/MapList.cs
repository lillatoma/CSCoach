using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MapPosition
{
    public string name;
    public int usualCTCount;
    public float[] fightChances;
    public float[] defensiveAdvantage;
    public bool TSpawn;
    public bool CTSpawn;
    public bool terrorTarget;
 
}

[System.Serializable]
public struct MapRotate
{
    public int a;
    public int b;
    public float rotateTime;
}

[System.Serializable]
public struct MapInformation
{
    public string name;
    public float ctAdvantage;
    public MapPosition[] positions;
    public MapRotate[] rotations;
}

public class MapList : MonoBehaviour
{
    public int a;
    public int b;
    public MapInformation[] maps;

    public int IsThereRotationBetween(int map, int a, int b)
    {
        for(int i = 0; i < maps[map].rotations.Length; i++)
        {
            var rot = maps[map].rotations[i];
            if (rot.a == a && rot.b == b)
                return i;
            if (rot.b == a && rot.a == b)
                return i;
        }
        return -1;
    }

    public int NextRoutePointBetween(int map, int a, int b)
    {
        int rotation = IsThereRotationBetween(map, a, b);
        if (rotation != -1)
        {
            return b;
        }
        else
        {
            List<int> availablePoints = new List<int>();
            List<int> availablePointsNext = new List<int>();
            List<int> usedPoints = new List<int>();
            usedPoints.Add(b);
            availablePoints.Add(b);
            while (usedPoints.Count != maps[map].positions.Length)
            {
                for (int i = 0; i < maps[map].rotations.Length; i++)
                {
                    for (int j = 0; j < availablePoints.Count; j++)
                    {
                        if (maps[map].rotations[i].a == availablePoints[j])
                        {
                            if (!usedPoints.Contains(maps[map].rotations[i].b))
                            {
                                if (a == maps[map].rotations[i].b)
                                    return availablePoints[j];
                                usedPoints.Add(maps[map].rotations[i].b);
                                availablePointsNext.Add(maps[map].rotations[i].b);
                            }
                        }
                        else if (maps[map].rotations[i].b == availablePoints[j])
                        {
                            if (!usedPoints.Contains(maps[map].rotations[i].a))
                            {
                                if (a == maps[map].rotations[i].a)
                                    return availablePoints[j];
                                usedPoints.Add(maps[map].rotations[i].a);
                                availablePointsNext.Add(maps[map].rotations[i].a);
                            }
                        }
                    }


                }
                //availablePoints = availablePointsNext;
                availablePoints.Clear();
                foreach (var x in availablePointsNext)
                    availablePoints.Add(x);
                availablePointsNext.Clear();


            }
            return -1;
        }
    }

    [ContextMenu("Between")]
    public void RouteCheck()
    {
        string ConsoleText = a.ToString();
        if (a == b)
            ConsoleText += " -> " + b.ToString();
        else
        {
            int currentpos = a;
            while(currentpos != b)
            {
                currentpos = NextRoutePointBetween(0, currentpos, b);
                ConsoleText += " -> " + currentpos.ToString();
            }
        }
        Debug.Log(ConsoleText);
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
