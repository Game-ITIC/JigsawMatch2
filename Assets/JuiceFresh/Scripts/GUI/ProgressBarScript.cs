using UI;
using UnityEngine;

[RequireComponent(typeof(UIFillBar))]
public class ProgressBarScript : MonoBehaviour
{
    public static ProgressBarScript Instance;

    public GameObject[] stars;
    public Vector3 pivot;
    public Vector3 angles;

    UIFillBar _fillBar;
    RectTransform _barTrack;

    void OnEnable()
    {
        Instance = this;
        _fillBar = GetComponent<UIFillBar>();
        if (_fillBar == null)
        {
            _fillBar = gameObject.AddComponent<UIFillBar>();
        }

        _barTrack = transform.parent as RectTransform;
        ResetBar();
        PrepareStars();
    }

    public void SetProgress(float currentValue, float maxValue)
    {
        _fillBar.SetFill(currentValue, maxValue);
    }

    public void UpdateDisplay(float normalizedProgress)
    {
        _fillBar.SetNormalizedFill(normalizedProgress);
    }

    public void AddValue(float delta)
    {
        _fillBar.SetNormalizedFill(_fillBar.NormalizedFill + delta);
    }

    public bool IsFull()
    {
        if (_fillBar.NormalizedFill >= 1f)
        {
            ResetBar();
            return true;
        }

        return false;
    }

    public void ResetBar()
    {
        _fillBar.SetNormalizedFill(0f, animated: false);
    }

    void PrepareStars()
    {
        if (LevelManager.THIS == null || stars == null || stars.Length < 3)
        {
            return;
        }

        float width = _barTrack != null ? _barTrack.rect.width : 0f;
        int maxScore = LevelManager.Instance.star3;
        if (width <= 0f || maxScore <= 0)
        {
            return;
        }

        float halfWidth = width * 0.5f;
        PositionStar(stars[0], LevelManager.Instance.star1, width, halfWidth, maxScore);
        PositionStar(stars[1], LevelManager.Instance.star2, width, halfWidth, maxScore);

        if (stars[2] != null && stars[2].transform.childCount > 0)
        {
            stars[2].transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    static void PositionStar(GameObject star, int starScore, float width, float halfWidth, int maxScore)
    {
        if (star == null)
        {
            return;
        }

        float x = starScore * width / maxScore - halfWidth;
        var position = star.transform.localPosition;
        star.transform.localPosition = new Vector3(x, position.y, position.z);

        if (star.transform.childCount > 0)
        {
            star.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivotPoint, Vector3 rotationAngles)
    {
        Vector3 dir = point - pivotPoint;
        dir = Quaternion.Euler(rotationAngles) * dir;
        return dir + pivotPoint;
    }
}
