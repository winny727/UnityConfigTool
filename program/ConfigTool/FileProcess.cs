using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ConfigTool
{
    public static class FileProcess
    {
        public static void Process(List<string> filePaths, List<string> allFilePath, bool exportCSharp = true)
        {
            if (!Directory.Exists(Define.JsonDirectory))
            {
                Directory.CreateDirectory(Define.JsonDirectory);
            }

            if (filePaths.Count == allFilePath.Count)
            {
                if (exportCSharp && !Directory.Exists(Define.CSharpDirectory))
                {
                    Directory.CreateDirectory(Define.CSharpDirectory);
                }
            }

            Utils.DeleteAllFiles(Define.JsonDirectory, "*" + Define.JsonPostfix);
            if (exportCSharp)
            {
                Utils.DeleteAllFiles(Define.CSharpDirectory, "*" + Define.CSharpPostfix);
            }

            int count = 0;
            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string jsonFilePath = Utils.GetFilePath(Define.JsonDirectory, fileName, Define.JsonPostfix);

                try
                {
                    List<string> lines = Utils.ReadAllLines(filePath);

                    string[] comments = lines[0].Split('\t'); //备注
                    string[] dataTypes = lines[1].Split('\t'); //数据类型
                    string[] fieldNames = lines[2].Split('\t'); //字段名

                    //截掉第一行为空白的列之后的列，后面可以写备注
                    int maxCol = fieldNames.Length;
                    for (int i = 0; i < fieldNames.Length; i++)
                    {
                        if (string.IsNullOrEmpty(fieldNames[i]))
                        {
                            maxCol = i;
                            break;
                        }
                    }

                    if (!comments[0].StartsWith(Define.DoubleKeyIDPrefix))
                    {
                        //单key表
                        var data = new Dictionary<string, Dictionary<string, object>>();
                        // 遍历数据行
                        for (int i = 3; i < lines.Count; i++)
                        {
                            string[] dataValues = lines[i].Split('\t');
                            string key = dataValues[0];

                            if (string.IsNullOrEmpty(key))
                            {
                                continue;
                            }

                            var dataDict = new Dictionary<string, object>();
                            for (int j = 0; j < maxCol; j++)
                            {
                                string fieldName = fieldNames[j];
                                string dataType = dataTypes[j];
                                //string comment = comments[j];
                                string dataValue = j < dataValues.Length ? dataValues[j] : default;

                                var convertedValue = Utils.ConvertData(dataValue, dataType);
                                dataDict.Add(fieldName, convertedValue);
                            }
                            data.Add(key, dataDict);
                        }

                        //json
                        string json = JsonConvert.SerializeObject(data, Define.JsonFormatting);
                        File.WriteAllText(jsonFilePath, json, Define.JsonEncoding);
                    }
                    else
                    {
                        //双key表
                        var data = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>();
                        var dataDict = new Dictionary<string, Dictionary<string, object>>();
                        // 遍历数据行
                        string key1 = null;
                        for (int i = 3; i < lines.Count; i++)
                        {
                            string[] dataValues = lines[i].Split('\t');
                            if (!string.IsNullOrEmpty(dataValues[0]))
                            {
                                dataDict = new Dictionary<string, Dictionary<string, object>>();
                                key1 = dataValues[0];
                            }
                            string key2 = dataValues[1];

                            if (string.IsNullOrEmpty(key1) && string.IsNullOrEmpty(key2))
                            {
                                continue;
                            }

                            if (!dataDict.ContainsKey(key2))
                            {
                                dataDict.Add(key2, new Dictionary<string, object>());
                            }

                            for (int j = 0; j < maxCol; j++)
                            {
                                string fieldName = fieldNames[j];
                                string dataType = dataTypes[j];
                                //string comment = comments[j];
                                string dataValue;
                                if (j == 0)
                                {
                                    dataValue = key1;
                                }
                                else if (j < dataValues.Length)
                                {
                                    dataValue = dataValues[j];
                                }
                                else
                                {
                                    dataValue = default;
                                }

                                var convertedValue = Utils.ConvertData(dataValue, dataType);
                                dataDict[key2].Add(fieldName, convertedValue);
                            }
                            if (!data.ContainsKey(key1))
                            {
                                data.Add(key1, dataDict);
                            }
                        }

                        //json
                        string json = JsonConvert.SerializeObject(data, Define.JsonFormatting);
                        File.WriteAllText(jsonFilePath, json, Define.JsonEncoding);
                    }

                    count++;
                    Log("Process Success: " + Path.GetFileName(filePath));
                }
                catch (Exception ex)
                {
                    LogError($"Process Error: {fileName}, Exception: \n{ex}");
                    OnComplete();
                    return;
                }
            }

            if (exportCSharp)
            {
                try
                {
                    var configInfos = new List<ConfigInfo>();
                    foreach (var filePath in allFilePath)
                    {
                        //string csharpFilePath = Utils.GetFilePath(Define.CSharpDirectory, fileName, Define.CSharpPostfix);
                        var configInfo = Utils.GetConfigInfo(filePath);
                        configInfos.Add(configInfo);
                    }

                    //C#
                    CSharpGenerator.GenerateCSharpFiles(configInfos);
                }
                catch (Exception ex)
                {
                    LogError($"Process Error: CSharp File Generate Error, Exception: \n{ex}");
                    OnComplete();
                    return;
                }
            }

            OnComplete();
        }

        public static void Log(object content)
        {
            Program.Form1.Invoke(new Action(() =>
            {
                Program.Form1.Log(content);
            }));
        }

        public static void LogError(object content)
        {
            Program.Form1.Invoke(new Action(() =>
            {
                Program.Form1.LogError(content);
            }));
        }

        public static void OnComplete()
        {
            Program.Form1.Invoke(new Action(() =>
            {
                Program.Form1.OnComplete();
            }));
        }
    }
}
