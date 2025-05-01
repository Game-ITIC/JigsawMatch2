using System.Collections.Generic;
using Models;
using Newtonsoft.Json;
using UnityEngine;
using Utils.Save;

namespace Providers
{
    public class BoostersProvider
    {
        public List<BoosterModel> BoostersModels { get; private set; } = new();

        public BoostersProvider()
        {
            Load();
        }

        public void Load()
        {
            var value = PlayerPrefs.GetString(PlayerPrefsKeys.Boosters, "empty");

            if (value == "empty")
            {
                BoostersModels = new List<BoosterModel>
                {
                    new(BoostType.Shovel),
                    new(BoostType.Energy),
                    new(BoostType.Bomb),
                    new(BoostType.ExtraMoves),
                };
            }
            else
            {
                BoostersModels = JsonConvert.DeserializeObject<List<BoosterModel>>(value);
            }
        }

        public void Save()
        {
            var value = JsonConvert.SerializeObject(BoostersModels);

            PlayerPrefs.SetString(PlayerPrefsKeys.Boosters, value);
            PlayerPrefs.Save();
        }
    }
}