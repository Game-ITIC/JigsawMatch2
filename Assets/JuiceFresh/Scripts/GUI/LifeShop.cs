using UnityEngine;
using System.Collections;
using TMPro;

public class LifeShop : MonoBehaviour
{
	public int CostIfRefill = 5;
	// Use this for initialization
	void OnEnable ()
	{
		transform.Find ("Image/BuyLife/Price").GetComponent<TMP_Text> ().text = "" + CostIfRefill;
		//if (!LevelManager.THIS.enableInApps)
			//transform.Find ("Image/BuyLife").gameObject.SetActive (false);
		
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
