using System;
using System.Collections.Generic;
using JuiceFresh;
using UnityEngine;

namespace JuiceFresh.Scripts
{
    public static class LevelParserService
    {
        public static LevelData Parse(string mapText, IReadOnlyList<CollectedIngredients> collectedIngredients)
        {
            var data = new LevelData();
            string[] lines = mapText.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            int mapLine = 0;

            foreach (string line in lines)
            {
                if (line.StartsWith("MODE"))
                {
                    string modeString = line.Replace("MODE", string.Empty).Trim();
                    data.Target = (Target)int.Parse(modeString);
                }
                else if (line.StartsWith("SIZE "))
                {
                    string blocksString = line.Replace("SIZE", string.Empty).Trim();
                    string[] sizes = blocksString.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    data.MaxCols = int.Parse(sizes[0]);
                    data.MaxRows = int.Parse(sizes[1]);
                    data.LevelSquares = new SquareBlocks[data.MaxRows * data.MaxCols];

                    for (int i = 0; i < data.LevelSquares.Length; i++)
                    {
                        data.LevelSquares[i] = new SquareBlocks
                        {
                            block = SquareTypes.EMPTY,
                            obstacle = SquareTypes.NONE
                        };
                    }
                }
                else if (line.StartsWith("LIMIT"))
                {
                    string blocksString = line.Replace("LIMIT", string.Empty).Trim();
                    string[] sizes = blocksString.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    data.LimitType = (LIMIT)int.Parse(sizes[0]);
                    data.Limit = int.Parse(sizes[1]);
                }
                else if (line.StartsWith("COLOR LIMIT "))
                {
                    string blocksString = line.Replace("COLOR LIMIT", string.Empty).Trim();
                    data.ColorLimit = int.Parse(blocksString);
                }
                else if (line.StartsWith("STARS"))
                {
                    string blocksString = line.Replace("STARS", string.Empty).Trim();
                    string[] blocksNumbers = blocksString.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    data.Star1 = int.Parse(blocksNumbers[0]);
                    data.Star2 = int.Parse(blocksNumbers[1]);
                    data.Star3 = int.Parse(blocksNumbers[2]);
                }
                else if (line.StartsWith("COLLECT COUNT "))
                {
                    string blocksString = line.Replace("COLLECT COUNT", string.Empty).Trim();
                    string[] blocksNumbers = blocksString.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < blocksNumbers.Length; i++)
                    {
                        if (collectedIngredients.Count <= i && data.Target == Target.COLLECT)
                            break;

                        if (data.Target == Target.COLLECT)
                            data.IngrTarget.Add(collectedIngredients[i]);
                        else
                            data.IngrTarget.Add(new CollectedIngredients());

                        data.IngrTarget[data.IngrTarget.Count - 1].count = int.Parse(blocksNumbers[i]);
                    }
                }
                else if (line.StartsWith("COLLECT ITEMS "))
                {
                    string blocksString = line.Replace("COLLECT ITEMS", string.Empty).Trim();
                    string[] blocksNumbers = blocksString.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < blocksNumbers.Length; i++)
                    {
                        if (data.Target == Target.COLLECT)
                        {
                            if (data.IngrTarget.Count > i)
                            {
                                CollectedIngredients ingFromList =
                                    collectedIngredients[int.Parse(blocksNumbers[i])];
                                data.IngrTarget[i].check = true;
                                data.IngrTarget[i].name = ingFromList.name;
                                data.IngrTarget[i].sprite = ingFromList.sprite;
                            }
                        }
                        else if (data.Target == Target.ITEMS)
                        {
                            data.CollectItems[i] = (CollectItems)int.Parse(blocksNumbers[i]) + 1;
                        }
                    }
                }
                else if (line.StartsWith("CAGE "))
                {
                    string blocksString = line.Replace("CAGE ", string.Empty).Trim();
                    data.CageHp = int.Parse(blocksString);
                }
                else if (line.StartsWith("BOMBS "))
                {
                    Debug.Log("load bomb");
                    string blocksString = line.Replace("BOMBS ", string.Empty).Trim();
                    string[] blocksNumbers = blocksString.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    data.BombsCollect = int.Parse(blocksNumbers[0]);
                    data.BombTimer = int.Parse(blocksNumbers[1]);
                }
                else if (line.StartsWith("GETSTARS "))
                {
                    string blocksString = line.Replace("GETSTARS ", string.Empty).Trim();
                    data.StarsTargetCount = (CollectStars)int.Parse(blocksString);
                }
                else
                {
                    string[] st = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < st.Length; i++)
                    {
                        data.LevelSquares[mapLine * data.MaxCols + i].block =
                            (SquareTypes)int.Parse(st[i][0].ToString());
                        data.LevelSquares[mapLine * data.MaxCols + i].obstacle =
                            (SquareTypes)int.Parse(st[i][1].ToString());
                    }

                    mapLine++;
                }
            }

            data.TargetBlocks = CountTargetBlocks(data);
            data.TargetCages = CountTargetCages(data);
            return data;
        }

        static int CountTargetBlocks(LevelData data)
        {
            int count = 0;

            for (int row = 0; row < data.MaxRows; row++)
            {
                for (int col = 0; col < data.MaxCols; col++)
                {
                    SquareTypes block = data.LevelSquares[row * data.MaxCols + col].block;
                    if (block == SquareTypes.BLOCK)
                        count++;
                    else if (block == SquareTypes.DOUBLEBLOCK)
                        count += 2;
                }
            }

            return count;
        }

        static int CountTargetCages(LevelData data)
        {
            int count = 0;

            for (int row = 0; row < data.MaxRows; row++)
            {
                for (int col = 0; col < data.MaxCols; col++)
                {
                    if (data.LevelSquares[row * data.MaxCols + col].obstacle == SquareTypes.WIREBLOCK)
                        count++;
                }
            }

            return count;
        }
    }
}
