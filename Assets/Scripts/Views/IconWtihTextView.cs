using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class IconWtihTextView : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _text;

        public void SetText(string name)
        {
            _text.text = name;
        }

        public void SetImage(Sprite image)
        {
            _image.sprite = image;
        }
    }
}
