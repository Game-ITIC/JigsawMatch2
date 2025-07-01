using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using Models;
using R3;
using UnityEngine.UI;

public class BoostIcon : MonoBehaviour
{
    public Text boostCount;
    public BoostType type;
    bool check;
    public Text price;
    public Button button;

    private BoosterModel _boosterModel;

    private CompositeDisposable _disposable = new();

    public BoosterModel BoosterModel
    {
        get { return _boosterModel; }
        set
        {
            _boosterModel = value;
            boostCount.text = BoosterModel.Count.Value.ToString();

            _boosterModel.Count.Subscribe(OnCountChanged).AddTo(_disposable);
        }
    }

    private void OnCountChanged(int newValue)
    {
        boostCount.text = newValue.ToString();
    }

    void OnEnable()
    {
        if (name != "Main Camera")
        {
            if (LevelManager.THIS != null)
            {
                if (LevelManager.THIS.gameStatus == GameState.Map)
                    transform.Find("Indicator/Count/Check").gameObject.SetActive(false);
                // if (!LevelManager.THIS.enableInApps)//1.4.9
                //     gameObject.SetActive(false);
            }
        }
    }

    public void ActivateBoost()
    {
        if (LevelManager.THIS.ActivatedBoost == this)
        {
            UnCheckBoost();
            return;
        }
        else if (BoostCount() == 0)
        {
            OpenBoostShop(type);
        }
        else
        {
            LevelManager.THIS.ActivatedBoost = this;
            return;
        }


        if (BoostCount() > 0)
        {
            //if (type != BoostType.Colorful_bomb && type != BoostType.Stripes && !LevelManager.THIS.DragBlocked)
            //    LevelManager.THIS.ActivatedBoost = this;
            //if (type == BoostType.Colorful_bomb)
            //{
            //    LevelManager.THIS.BoostColorfullBomb = 1;
            //    Check();
            //}
            //if (type == BoostType.Stripes)
            //{
            //    LevelManager.THIS.BoostStriped = 2;
            //    Check();
            //}
        }
    }


    void UnCheckBoost()
    {
        LevelManager.THIS.activatedBoost = null;
        LevelManager.THIS.UnLockBoosts();
    }

    public void InitBoost()
    {
        transform.Find("Indicator/Count/Check").gameObject.SetActive(false);
        transform.Find("Indicator/Count/Count").gameObject.SetActive(true);
        LevelManager.THIS.BoostColorfullBomb = 0;
        LevelManager.THIS.BoostPackage = 0;
        LevelManager.THIS.BoostStriped = 0;
        check = false;
    }

    void Check()
    {
        check = true;
        transform.Find("Indicator/Count/Check").gameObject.SetActive(true);
        transform.Find("Indicator/Count/Count").gameObject.SetActive(false);
        //InitScript.Instance.SpendBoost(type);
    }

    public void LockBoost()
    {
        Color c = GetComponent<Image>().color;
        GetComponent<Image>().color = new Color(c.a, c.g, c.b, 0.5f);
        //transform.Find("Lock").gameObject.SetActive(true);
        transform.Find("Indicator").gameObject.SetActive(false);
    }

    public void UnLockBoost()
    {
        Color c = GetComponent<Image>().color;
        GetComponent<Image>().color = new Color(1, 1, 1, 1);

        //transform.Find("Lock").gameObject.SetActive(false);
        transform.Find("Indicator").gameObject.SetActive(true);
    }

    bool IsLocked()
    {
        return false;
    }

    int BoostCount()
    {
        return _boosterModel.Count.Value;
    }

    public void OpenBoostShop(BoostType boosType)
    {
        SoundBase.Instance.PlaySound(SoundBase.Instance.click);
        GameObject.Find("CanvasGlobal").transform.Find("BoostShop").gameObject.GetComponent<BoostShop>()
            .SetBoost(boosType);
    }

    void ShowPlus(bool show)
    {
        transform.Find("Indicator/Plus").gameObject.SetActive(show);
        transform.Find("Indicator/Count").gameObject.SetActive(!show);
    }


    void Update()
    {
        if (boostCount != null)
        {
            //boostCount.text = "" + PlayerPrefs.GetInt("" + type);
            if (!check)
            {
                if (BoostCount() > 0)
                    ShowPlus(false);
                else
                    ShowPlus(true);
            }
        }
    }

    private void OnDestroy()
    {
        _disposable.Dispose();
    }
}