using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ConfigTool
{
    public class ConfigInfo
    {
        public string FileName;
        public List<string> Names = new List<string>();
        public List<string> Types = new List<string>();
        public List<string> Comments = new List<string>();
        public string Key1Type;
        public string Key2Type;
    }

    public static class CSharpGenerator
    {
        public static void GenerateCSharpFiles(List<ConfigInfo> configInfos)
        {
            for (int i = 0; i < configInfos.Count; i++)
            {
                ConfigInfo configInfo = configInfos[i];
                string csharpFilePath = Utils.GetFilePath(Define.CSharpDirectory, "Cfg" + configInfo.FileName, Define.CSharpPostfix);
                string csharp = GetCSharpScript(configInfo);
                File.WriteAllText(csharpFilePath, csharp);
            }
            string cfgReader = GetCfgReaderFile(configInfos);
            File.WriteAllText(Define.CSharpDirectory + "/CfgReaderPartial" + Define.CSharpPostfix, cfgReader);
        }

        public static string GetCSharpScript(ConfigInfo configInfo)
        {
            StringBuilder sb = new StringBuilder();

            string fileName = configInfo.FileName;
            string key1Type = configInfo.Key1Type;
            string key2Type = configInfo.Key2Type;
            bool isDoubleKey = !string.IsNullOrEmpty(key2Type);
            var names = configInfo.Names;
            var types = configInfo.Types;
            var comments = configInfo.Comments;
            string name1 = names[0].ToLower();
            string name2 = "id2";
            if (isDoubleKey)
            {
                name2 = names[1].ToLower();
            }

            string configClassName = "Cfg" + fileName;
            string configDataTypeName = "Cfg" + fileName + "Data";

            sb.AppendLine("/*");
            sb.AppendLine("* 此类由ConfigTool自动生成");
            sb.AppendLine("*/");

            for (int i = 0; i < Define.NameSpaceList.Count; i++)
            {
                string nameSpace = Define.NameSpaceList[i];
                sb.AppendLine($"using {nameSpace};");
            }

            sb.AppendLine("");
            sb.AppendLine("public partial class CfgReader");
            sb.AppendLine("{");

            sb.AppendLine($"    public static {configClassName} {configClassName} {{ get; protected set; }}");
            sb.AppendLine("");

            if (!isDoubleKey)
            {
                sb.AppendLine($"    public static {configDataTypeName} Get{fileName}Data({key1Type} {name1})");
                sb.AppendLine("    {");
                sb.AppendLine($"        if ({configClassName}.Data.ContainsKey({name1}))");
                sb.AppendLine($"            return {configClassName}.Data[{name1}];");
            }
            else
            {
                sb.AppendLine($"    public static {configDataTypeName} Get{fileName}Data({key1Type} {name1}, {key2Type} {name2})");
                sb.AppendLine("    {");
                sb.AppendLine($"        if ({configClassName}.Data.ContainsKey({name1}) && {configClassName}.Data[{name1}].ContainsKey({name2}))");
                sb.AppendLine($"            return {configClassName}.Data[{name1}][{name2}];");
            }

            sb.AppendLine($"        else");
            sb.AppendLine($"            return null;");
            sb.AppendLine("    }");
            sb.AppendLine("");

            for (int i = 0; i < names.Count; i++)
            {
                string name = names[i];
                string type = i == 0 ? key1Type : Utils.ConvertDataType(types[i]);
                string comment = comments[i];

                sb.AppendLine($"    /// <summary> 获取{fileName} {comment} </summary>");
                if (!isDoubleKey)
                {
                    sb.AppendLine($"    public static {type} Get{fileName}_{name}({key1Type} {name1})");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        {configDataTypeName} data = Get{fileName}Data({name1});");
                }
                else
                {
                    sb.AppendLine($"    public static {type} Get{fileName}_{name}({key1Type} {name1}, {key2Type} {name2})");
                    sb.AppendLine("    {");
                    sb.AppendLine($"        {configDataTypeName} data = Get{fileName}Data({name1}, {name2});");
                }

                sb.AppendLine($"        if (data != null)");
                sb.AppendLine($"            return data.{name};");
                sb.AppendLine($"        else");
                sb.AppendLine($"            return default;");
                sb.AppendLine("    }");
                sb.AppendLine("");
            }

            sb.AppendLine("}");

            sb.AppendLine("");
            sb.AppendLine("/*");
            sb.AppendLine("* 此类由ConfigTool自动生成");
            sb.AppendLine("*/");
            sb.AppendLine("[CfgReaderConfig]");
            sb.AppendLine($"public class {configClassName}");
            sb.AppendLine("{");
            if (!isDoubleKey)
            {
                sb.AppendLine($"    public Dictionary<{key1Type}, {configDataTypeName}> Data;");
            }
            else
            {
                sb.AppendLine($"    public Dictionary<{key1Type}, Dictionary<{key2Type}, {configDataTypeName}>> Data;");
            }
            sb.AppendLine("}");
            sb.AppendLine("");

            sb.AppendLine("[CfgReaderConfig]");
            sb.AppendLine($"public class {configDataTypeName}");
            sb.AppendLine("{");
            for (int i = 0; i < names.Count; i++)
            {
                string name = names[i];
                string type = Utils.ConvertDataType(types[i]);
                string comment = comments[i];
                sb.AppendLine($"    /// <summary> {comment} </summary>");
                sb.AppendLine($"    public {type} {name};");
                sb.AppendLine("");
            }
            sb.AppendLine("}");

            return sb.ToString();
        }

        public static string GetCfgReaderFile(List<ConfigInfo> configInfos)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/*");
            sb.AppendLine("* 此类由ConfigTool自动生成");
            sb.AppendLine("*/");

            for (int i = 0; i < Define.NameSpaceList.Count; i++)
            {
                string nameSpace = Define.NameSpaceList[i];
                sb.AppendLine($"using {nameSpace};");
            }
            sb.AppendLine($"using Newtonsoft.Json;");

            sb.AppendLine("");
            sb.AppendLine("public partial class CfgReader");
            sb.AppendLine("{");
            sb.AppendLine("    private static void LoadAllJson()");
            sb.AppendLine("    {");

            string jsonResPath = Define.JsonDirectory;
            int index = Define.JsonDirectory.IndexOf("Resources");
            if (index >= 0)
            {
                jsonResPath = jsonResPath.Substring(index + "Resources".Length + 1);
            }

            for (int i = 0; i < configInfos.Count; i++)
            {
                ConfigInfo configInfo = configInfos[i];
                string fileName = configInfo.FileName;
                sb.AppendLine($"        TextAsset {fileName}TextAsset = AssetsMgr.Instance.LoadResource<TextAsset>(\"{jsonResPath}/{fileName}\");");
                sb.AppendLine($"        string {fileName}JsonStr = \"{{Data : \" + {fileName}TextAsset.text + \"}}\";");
                sb.AppendLine($"        Cfg{fileName} = JsonConvert.DeserializeObject<Cfg{fileName}>({fileName}JsonStr);");
                sb.AppendLine("");
            }
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}