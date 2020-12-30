using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    private const string bestScoreKey = "BeseScore";
    private const string currentScoreKey = "CurrentScore";
    private const string DataFileName = "StoredData";
    private static DataManager instance;

    private int bestScore = -1;
    private int currentScore = -1;
    private List<CellData> storedCellList;

    public static DataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new DataManager();
            }
            return instance;
        }
    }

    public int CurrentScore
    {
        get
        {
            if (currentScore < 0)
            {
                currentScore = PlayerPrefs.GetInt(currentScoreKey);
            }
            return currentScore;
        }
        set
        {
            currentScore = value;
            PlayerPrefs.SetInt(currentScoreKey, currentScore);
        }
    }

    public int BestScore
    {
        get
        {
            if (bestScore < 0)
            {
                bestScore = PlayerPrefs.GetInt(bestScoreKey);
            }
            return bestScore;
        }
        set
        {
            if (value > BestScore)
            {
                bestScore = value;
                PlayerPrefs.SetInt(bestScoreKey, bestScore);
            }
        }
    }

    public List<CellData> StoredCellList
    {
        get
        {
            if (storedCellList == null)
            {
                storedCellList = JsonManager.LoadFile<List<CellData>>(DataFileName) ?? new List<CellData>();
            }
            return storedCellList;
        }
        set
        {
            storedCellList = value;
            JsonManager.SaveFile(DataFileName, storedCellList);
        }
    }

    public void ClearCellData()
    {
        storedCellList.Clear();
        JsonManager.SaveFile(DataFileName, storedCellList);
    }

    [JsonConverter(typeof(CellDataConvert))]
    public struct CellData
    {
        public int CoordX { get; set; }
        public int CoordY { get; set; }
        public int Score { get; set; }

        public CellData(Vector2 coord, int score) : this((int)coord.x, (int)coord.y, score)
        {
        }

        public CellData(int coordX, int coordY, int score)
        {
            CoordX = coordX;
            CoordY = coordY;
            Score = score;
        }

        public override string ToString()
        {
            return $"{CoordX} {CoordY} {Score}";
        }
    }

    public class CellDataConvert : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CellData);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.String) return null;
            var array = reader.Value.ToString().Split(' ');
            return new CellData(Convert.ToInt32(array[0]), Convert.ToInt32(array[1]), Convert.ToInt32(array[2]));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }
}