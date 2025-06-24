using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConfigTool
{
    public static class Utils
    {
        public static object ConvertData(string dataValue, string dataType)
        {
            //清一下Excel转义符"
            if (dataValue.StartsWith("\"") && dataValue.EndsWith("\""))
            {
                dataValue = dataValue.Substring(1, dataValue.Length - 2);
            }
            dataValue = dataValue.Replace("\"\"", "\"");

            switch (dataType.ToLower())
            {
                case "b":
                case "bool":
                    if (!bool.TryParse(dataValue, out var bRet))
                    {
                        bRet = false;
                    }
                    return bRet;
                case "n":
                case "i":
                case "int":
                    if (!int.TryParse(dataValue, out var nRet))
                    {
                        nRet = 0;
                    }
                    return nRet;
                case "f":
                case "float":
                    if (!float.TryParse(dataValue, out var fRet))
                    {
                        fRet = 0;
                    }
                    return fRet;
                case "d":
                case "double":
                    if (!double.TryParse(dataValue, out var dRet))
                    {
                        dRet = 0;
                    }
                    return dRet;
                case "s":
                case "string":
                    return dataValue ?? "";
                default:
                    break;
            }

            if (dataType.Contains(":"))
            {
                string[] typeInfo = dataType.Split(':');
                string listType = typeInfo[0].ToLower();
                string type = typeInfo[1];
                switch (listType)
                {
                    case "l":
                    case "list":
                        List<object> list = new List<object>();
                        string[] datas = dataValue.Split('|');
                        for (int i = 0; i < datas.Length; i++)
                        {
                            list.Add(ConvertData(datas[i], type));
                        }
                        return list;
                    case "l2":
                    case "list2":
                        List<List<object>> list1 = new List<List<object>>();
                        string[] datas1 = dataValue.Split('|');
                        for (int i = 0; i < datas1.Length; i++)
                        {
                            List<object> list2 = new List<object>();
                            string[] datas2 = datas1[i].Split(';');
                            for (int j = 0; j < datas2.Length; j++)
                            {
                                list2.Add(ConvertData(datas2[j], type));
                            }
                            list1.Add(list2);
                        }
                        return list1;
                    default:
                        break;
                }
            }

            return dataValue;
        }

        public static string ConvertDataType(string dataType)
        {
            switch (dataType)
            {
                case "b":
                case "bool":
                    return "bool";
                case "n":
                case "i":
                case "int":
                    return "int";
                case "f":
                case "float":
                    return "float";
                case "d":
                case "double":
                    return "double";
                case "s":
                case "string":
                    return "string";
                default:
                    break;
            }

            if (dataType.Contains(":"))
            {
                string[] typeInfo = dataType.Split(':');
                string listType = typeInfo[0].ToLower();
                string type = ConvertDataType(typeInfo[1]);
                switch (listType)
                {
                    case "l":
                    case "list":
                        return $"List<{type}>";
                    case "l2":
                    case "list2":
                        return $"List<List<{type}>>";
                    default:
                        break;
                }
            }

            return dataType;
        }

        public static string GetFilePath(string directory, string fileName, string postfix)
        {
            return Path.Combine(directory, fileName + postfix); ;
        }

        public static List<string> GetAllFilePaths(string directory, string pattern)
        {
            var filePaths = new List<string>();

            foreach (var filePath in Directory.GetFiles(directory))
            {
                if (Path.GetExtension(filePath).Equals(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    filePaths.Add(filePath);
                }
            }

            foreach (var subDirectory in Directory.GetDirectories(directory))
            {
                filePaths.AddRange(GetAllFilePaths(subDirectory, pattern));
            }

            return filePaths;
        }

        public static void DeleteAllFiles(string path, string pattern = "*.*")
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            foreach (FileInfo file in directoryInfo.GetFiles(pattern))
            {
                file.Delete();
            }
        }

        public static List<string> ReadAllLines(string filePath)
        {
            // 读取数据文件
            //var lines = File.ReadAllLines(filePath, Define.SourceEncoding);

            //Excel会占用文件导致File.ReadAllLines读取不了，还是用FileStream
            List<string> lines = new List<string>();
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) //FileShare.ReadWrite参数表示可以与其他进程共享读写权限
            {
                using (StreamReader reader = new StreamReader(fileStream, Define.SourceEncoding))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                }
            }

            return lines;
        }

        public static ConfigInfo GetConfigInfo(string filePath)
        {
            ConfigInfo configInfo = new ConfigInfo();
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) //FileShare.ReadWrite参数表示可以与其他进程共享读写权限
            {
                using (StreamReader reader = new StreamReader(fileStream, Define.SourceEncoding))
                {
                    string line0 = reader.ReadLine();
                    string line1 = reader.ReadLine();
                    string line2 = reader.ReadLine();
                    string[] comments = line0.Split('\t');
                    string[] dataTypes = line1.Split('\t');
                    string[] fieldNames = line2.Split('\t');

                    configInfo.FileName = Path.GetFileNameWithoutExtension(filePath);
                    configInfo.Key1Type = ConvertDataType(dataTypes[0]);

                    if (comments[0].StartsWith(Define.DoubleKeyIDPrefix))
                    {
                        comments[0] = comments[0].Substring(Define.DoubleKeyIDPrefix.Length);
                        configInfo.Key2Type = ConvertDataType(dataTypes[1]);
                    }

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

                    for (int i = 0; i < maxCol; i++)
                    {
                        configInfo.Names.Add(fieldNames[i]);
                        configInfo.Types.Add(dataTypes[i]);
                        configInfo.Comments.Add(comments[i]);
                    }
                }
            }

            return configInfo;
        }
    }
}
