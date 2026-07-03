using UnityEngine;
using System.Collections;

public class ItemAnimEvents : MonoBehaviour
{


    public Item item;
    public GameObject particals;

    public void SetAnimationDestroyingFinished()
    {
        if (item != null)
        {
            item.SetAnimationDestroyingFinished();
            item = null;
        }
    }
}
