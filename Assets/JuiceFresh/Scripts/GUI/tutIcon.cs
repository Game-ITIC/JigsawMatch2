using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tutIcon : MonoBehaviour
{
    public GameObject tutIcona;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerPrefs.GetInt("isParkVisited") != 1 && StarModel.instance._totalStars >= 3)
        {
            tutIcona.SetActive(true);
        }
        else
        {
            tutIcona.SetActive(false);
        }
    }
}
