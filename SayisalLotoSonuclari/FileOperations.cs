using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;

namespace SayisalLotoSonuclari
{
    public class FileOperations
    {
        private StringBuilder stringBuilder = new StringBuilder();
        private StringBuilder jsonStringBuilder = new StringBuilder();
        public static string localFilePath,jsonFilePath;
        readonly string startupPath = AppDomain.CurrentDomain.BaseDirectory;
        static string fileName = "sonuclar.csv";
        static string jsonFileName = "results.json";
        string saveFile = "save.csv";
        string saveFilePath = "";

        public FileOperations()
        {
            Console.WriteLine(startupPath);
            localFilePath = Path.Combine(startupPath, fileName);
            localFilePath = Directory.GetParent(localFilePath).FullName;
            localFilePath = Directory.GetParent(localFilePath).FullName;
            localFilePath = Directory.GetParent(localFilePath).FullName;
            saveFilePath = Path.Combine(localFilePath, saveFile);
            localFilePath = Path.Combine(localFilePath, fileName);
            jsonFilePath= Path.Combine(startupPath, jsonFileName);
            jsonFilePath = Directory.GetParent(jsonFilePath).FullName;
            jsonFilePath = Directory.GetParent(jsonFilePath).FullName;
            jsonFilePath = Directory.GetParent(jsonFilePath).FullName;
            jsonFilePath = Path.Combine(jsonFilePath, jsonFileName);
            Console.WriteLine(localFilePath);
        }
        public void AddResultDataToJson(string resultData)
        {
            jsonStringBuilder.AppendLine(resultData);
        }
        public void SaveJsonLocal(string json)
        {
            string currentContent = String.Empty;
            if (File.Exists(jsonFilePath))
            {
                currentContent = File.ReadAllText(jsonFilePath);
            }

            try
            {
                File.WriteAllText(jsonFilePath,json);
            }
            catch (Exception)
            {
                throw;
            }

            Thread.Sleep(1000);
            Console.WriteLine("Local Json File Saved");
        }

        public void AddToCSV(string title)
        {
            stringBuilder.AppendLine(title);
        }
        public void SaveCSVLocal(bool newFile)
        {
            string currentContent = String.Empty;
            if (File.Exists(localFilePath))
            {
                currentContent = File.ReadAllText(localFilePath);
            }

            try
            {
                File.WriteAllText(localFilePath, stringBuilder.ToString() + currentContent);
            }
            catch (Exception)
            {
                throw;
            }

            Thread.Sleep(1000);
            Console.WriteLine("Local File Saved");
        }
        public void ConvertCsvToJason()
        {
            //354.817,23 TL,943,51 TL,8,42 TL,,84 TL
            List<ResultData> results = new List<ResultData>();
            using (TextFieldParser parser = new TextFieldParser(localFilePath))
            {
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    //Processing row
                    string[] fields = parser.ReadFields();
                    ResultData result = new ResultData();
                    result.date = fields[0];
                    result.week = int.Parse(fields[1]);
                    result.location = fields[8];
                    for(int i =0;i<result.numbers.Length;i++)
                    {
                        result.numbers[i] = int.Parse(fields[2 + i]);
                    }
                    for (int i = 0; i < result.prizeCounts.Length; i++)
                    {
                        result.prizeCounts[i]= fields[9+i];
                    }
                    for (int i = 0; i < result.prizes.Length; i++)
                    {
                        result.prizes[i] = SolvePrizeFormatting(fields[13 + i * 2], fields[13 + i * 2 + 1]);
                    }
                    results.Add(result);
                    //AddResultDataToJson(JsonConvert.SerializeObject(result,Formatting.Indented));
                }
            }
            string json = JsonConvert.SerializeObject(results, Formatting.Indented);
            SaveJsonLocal(json);
        }
        public static String SolvePrizeFormatting(string firstString, string secondString)
        {
            if (!String.IsNullOrEmpty(firstString))
            {
                return firstString + "," + secondString;
            }
            else
            {
                return firstString + "0," + secondString;
            }
        }
    }
    
}
