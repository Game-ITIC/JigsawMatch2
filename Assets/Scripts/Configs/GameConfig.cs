using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = "FILENAME", menuName = "MENUNAME", order = 0)]
    public class GameConfig : ScriptableObject
    {
        public int MaxLifes = 5;
    }
}