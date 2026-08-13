using UnityEngine;
using UnityEngine.UI;

public class TaskPanel : MonoBehaviour
{
    [SerializeField] private Button[] _closeButtons;
    [SerializeField] private TaskView _taskViewPrefab;
    [SerializeField] private Transform _taskViewParent;
}
