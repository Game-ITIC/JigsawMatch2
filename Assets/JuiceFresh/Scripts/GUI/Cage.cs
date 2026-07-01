using UnityEngine;
using System.Collections;
using JuiceFresh;
using TMPro;

public class Cage : MonoBehaviour
{
    public TMP_Text HP;
    private Square square;

    // Use this for initialization
    void Start()
    {
        square = transform.parent.GetComponent<Square>();
    }

    // Update is called once per frame
    void Update()
    {
        HP.text = "" + square.cageHPPreview;
    }
}
