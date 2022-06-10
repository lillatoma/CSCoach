using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuSwapper : MonoBehaviour
{

    static public void ChangeLevel(string levelName)
    {
        if (SceneManager.GetActiveScene().name == levelName)
            return;
        SceneManager.LoadScene(levelName);
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
