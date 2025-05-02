using Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Configs
{
    [CreateAssetMenu(fileName = nameof(CountryConfig), menuName = nameof(Configs) + "/" + nameof(CountryConfig))]
    public class CountryConfig : ScriptableObject
    {
        [Title("Country Config")]
        [InfoBox("Id and Name needs to Save working correctly"), LabelText("Country Id")]
        public CountryEnum countryId;
        [LabelText("Country name")]
        public string countryName;

        public string GetCountryKeyToSave()
        {
            return $"{countryName}_{countryId}";
        }
    }
}