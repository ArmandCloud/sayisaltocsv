using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SayisalLotoSonuclari
{
    class SeleniumManager
    {
        IWebDriver driver;
        FileOperations fileManager;
        public SeleniumManager()
        {
            InitializeSelenium();
        }
        void InitializeSelenium()
        {
            driver = new ChromeDriver();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            fileManager = new FileOperations();
            Thread.Sleep(1000);
            RetriveNumbers();
        }
        public void RetriveNumbers()
        {
         
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
            driver.Navigate().GoToUrl("http://www.mpi.gov.tr/sonuclar/_cs_sayisal.php");
            while (!driver.Title.Contains("Sayısal"))
            {
                Console.WriteLine("Site yükleniyor...");
            }
            Console.WriteLine("Site yüklendi...");

            // id si sayisal-tarihList olan elamanı seçtik
            IWebElement dates = driver.FindElement(By.Id("sayisal-tarihList"));
            // id si "sayisal-tarihList" olan elamanın içindeki "option" tag ı olan tüm elemanları seçtik
            IReadOnlyCollection<IWebElement> all_options = dates.FindElements(By.TagName("option"));
            IWebElement[] all_options_array = new IWebElement[all_options.Count];
            all_options_array = all_options.ToArray();

            string tempHaftaNo = driver.FindElement(By.Id("sayisal-hafta")).Text;
            int difference = CompareWeekFromFile(int.Parse(tempHaftaNo));
            if (difference == 0)
            {
                Console.WriteLine("File is up to date.");
                return;
            }
            else if (difference > 0)
            {
                Console.WriteLine("File is " + difference + " week old.");
                string data = "";
                
                for (int i = 0; i < difference; i++)
                {
                    if (i > 0)
                    {
                        string sitede_gorunen_hafta = driver.FindElement(By.Id("sayisal-hafta")).Text;
                        all_options_array[i].Click();
                        IsWeekChanged(sitede_gorunen_hafta);
                    }
                    data = GetCurrentWeekData(all_options_array[i].GetAttribute("value"));
                    string result = JsonConvert.SerializeObject(GetCurrentWeekDataJson(all_options_array[i].GetAttribute("value")));
                    fileManager.AddResultDataToJson(result);
                    Console.WriteLine(data);
                    fileManager.AddToCSV(data);
                }
                fileManager.SaveCSVLocal(false);
                //fileManager.SaveJsonLocal();
            }
            else
            {
                Console.WriteLine("There is no file.");
                string data = "";
                string result = "";
                Console.WriteLine(all_options_array.Length);
                for (int i = 0; i < all_options_array.Length; i++)
                {
                    if (i > 0)
                    {
                        string sitede_gorunen_hafta = driver.FindElement(By.Id("sayisal-hafta")).Text;
                        all_options_array[i].Click();
                        IsWeekChanged(sitede_gorunen_hafta);
                    }
                    data = GetCurrentWeekData(all_options_array[i].GetAttribute("value"));
                    result = JsonConvert.SerializeObject(GetCurrentWeekDataJson(all_options_array[i].GetAttribute("value")));
                    fileManager.AddResultDataToJson(result);
                    Console.WriteLine(data);
                    fileManager.AddToCSV(data);
                }
                fileManager.SaveCSVLocal(true);
                fileManager.SaveJsonLocal(result);
            }
    
        }
        private string GetLuckyNumbers ()
        {
            string luckyNumbers="";
            IWebElement numara_elementi = driver.FindElement(By.Id("sayisal-numaralar"));
            IReadOnlyCollection<IWebElement> all_numbers = numara_elementi.FindElements(By.TagName("li"));
            IWebElement[] all_numbers_array = new IWebElement[all_numbers.Count];
            all_numbers_array = all_numbers.ToArray();
            for (int i = 0; i < all_numbers_array.Length; i++)
            {
                luckyNumbers = String.Concat(luckyNumbers, ",", all_numbers_array[i].Text);
            }
            return luckyNumbers;

        }
        private int[] GetLuckyNumbersArray()
        {
            int[] luckyNumbers = new int[6];
            IWebElement numara_elementi = driver.FindElement(By.Id("sayisal-numaralar"));
            IReadOnlyCollection<IWebElement> all_numbers = numara_elementi.FindElements(By.TagName("li"));
            IWebElement[] all_numbers_array = new IWebElement[all_numbers.Count];
            all_numbers_array = all_numbers.ToArray();
            for (int i = 0; i < all_numbers_array.Length; i++)
            {
                luckyNumbers[i] = int.Parse(all_numbers_array[i].Text);
            }
            return luckyNumbers;
        }
        private string GetCurrentWeekData(string tarih)
        {
            string data = "";
            string haftaNo = driver.FindElement(By.Id("sayisal-hafta")).Text;
            string ililce = driver.FindElement(By.Id("sayisal-buyukIkramiyeKazananIl")).Text;
            string altiBilen = driver.FindElement(By.Id("sayisal-bilenkisisayisi-6_BILEN")).Text;
            string altiBilenIkramiye = driver.FindElement(By.Id("sayisal-bilenkisikisibasidusenikramiye-6_BILEN")).Text;
            string besBilen = driver.FindElement(By.Id("sayisal-bilenkisisayisi-5_BILEN")).Text;
            string besBilenIkramiye = driver.FindElement(By.Id("sayisal-bilenkisikisibasidusenikramiye-5_BILEN")).Text;
            string dortBilen = driver.FindElement(By.Id("sayisal-bilenkisisayisi-4_BILEN")).Text;
            string dortBilenIkramiye = driver.FindElement(By.Id("sayisal-bilenkisikisibasidusenikramiye-4_BILEN")).Text;
            string ucBilen = driver.FindElement(By.Id("sayisal-bilenkisisayisi-3_BILEN")).Text;
            string ucBilenIkramiye = driver.FindElement(By.Id("sayisal-bilenkisikisibasidusenikramiye-3_BILEN")).Text;
            // options tagı içindeki text
            string kazananSayılar = GetLuckyNumbers();
            data = String.Concat(tarih, ",", haftaNo,kazananSayılar, ",", ililce, ",", altiBilen, ",", besBilen,
                ",", dortBilen, ",", ucBilen, ",", altiBilenIkramiye, ",", besBilenIkramiye, ",", dortBilenIkramiye, ",", ucBilenIkramiye);
            return data;
        }
        private ResultData GetCurrentWeekDataJson(string date)
        {
            ResultData resultData = new ResultData();
            resultData.date = date;
            string haftaNo = driver.FindElement(By.Id("sayisal-hafta")).Text;
            resultData.week = int.Parse(haftaNo);
            string ililce = driver.FindElement(By.Id("sayisal-buyukIkramiyeKazananIl")).Text;
            resultData.location = ililce;
            string altiBilen = driver.FindElement(By.Id("sayisal-bilenkisisayisi-6_BILEN")).Text;
            resultData.prizeCounts[0] = altiBilen;
            string altiBilenIkramiye = driver.FindElement(By.Id("sayisal-bilenkisikisibasidusenikramiye-6_BILEN")).Text;
            resultData.prizes[0] = altiBilenIkramiye;
            string besBilen = driver.FindElement(By.Id("sayisal-bilenkisisayisi-5_BILEN")).Text;
            resultData.prizeCounts[1] = besBilen;
            string besBilenIkramiye = driver.FindElement(By.Id("sayisal-bilenkisikisibasidusenikramiye-5_BILEN")).Text;
            resultData.prizes[0] = besBilenIkramiye;
            string dortBilen = driver.FindElement(By.Id("sayisal-bilenkisisayisi-4_BILEN")).Text;
            resultData.prizeCounts[2] = dortBilen;
            string dortBilenIkramiye = driver.FindElement(By.Id("sayisal-bilenkisikisibasidusenikramiye-4_BILEN")).Text;
            resultData.prizes[0] = dortBilenIkramiye;
            string ucBilen = driver.FindElement(By.Id("sayisal-bilenkisisayisi-3_BILEN")).Text;
            resultData.prizeCounts[3] = ucBilen;
            string ucBilenIkramiye = driver.FindElement(By.Id("sayisal-bilenkisikisibasidusenikramiye-3_BILEN")).Text;
            resultData.prizes[0] = ucBilenIkramiye;
            // options tagı içindeki text
            resultData.numbers = GetLuckyNumbersArray();
            return resultData;
        }
        public int WeeksToAdd()
        {
            int weekCount=0;

            return weekCount;
        }
        public int CompareWeekFromFile(int currentWeek)
        {
            if (File.Exists(FileOperations.localFilePath))
            {
                StreamReader reader = new StreamReader(FileOperations.localFilePath);
                int tempWeek = 0;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    if (int.TryParse(values[1], out tempWeek))
                    {
                        reader.Close();
                        break;
                    }
                }
                return currentWeek - tempWeek;
            }
            else
                return -1;
            
        }
        void IsWeekChanged(string oldTitle)
        {
            while (true)
            {
                if (!driver.FindElement(By.Id("sayisal-hafta")).Text.Equals(oldTitle))
                    break;
                Thread.Sleep(10);
            }
        }


    }
}
