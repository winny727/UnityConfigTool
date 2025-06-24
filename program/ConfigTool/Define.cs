using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ConfigTool
{
    public static class Define
    {
        private class Config
        {
            public string SourceDirectory;
            public string JsonDirectory;
            public string CSharpDirectory;
            public string SourcePostfix;
            public string JsonPostfix;
            public string CSharpPostfix;
            public string SourceEncoding;
            public string JsonEncoding;
            public string CSharpEncoding;
            public string DoubleKeyIDPrefix;
            public Formatting JsonFormatting;
            public List<string> NameSpaceList;
        }

        private static Config CreateConfig()
        {
            Config config = new Config
            {
                SourceDirectory = "/config",
                JsonDirectory = "/Assets/Resources/Configs",
                CSharpDirectory = "/Assets/Scripts/Game/Config/GameConfig",
                SourcePostfix = ".xls",
                JsonPostfix = ".txt",
                CSharpPostfix = ".cs",
                SourceEncoding = "GB2312",
                JsonEncoding = "UTF8",
                CSharpEncoding = "UTF8",
                DoubleKeyIDPrefix = "^",
                JsonFormatting = Formatting.Indented,
                NameSpaceList = new List<string>
                {
                    "System", "System.Collections.Generic", "UnityEngine",
                },
            };

            return config;
        }

        private static Config mCfg;
        private static Config Cfg
        {
            get
            {
                string cfgPath = Directory.GetCurrentDirectory() + "/config.json";

                void RevertCfg()
                {
                    mCfg = CreateConfig();
                    string json = JsonConvert.SerializeObject(mCfg, Formatting.Indented);
                    File.WriteAllText(cfgPath, json);
                }

                if (!File.Exists(cfgPath))
                {
                    RevertCfg();
                    return mCfg;
                }

                try
                {
                    string json = File.ReadAllText(cfgPath);
                    mCfg = JsonConvert.DeserializeObject<Config>(json);
                }
                catch
                {
                    RevertCfg();
                }
                return mCfg;
            }
        }

        private static string mCurrentDirectory;
        public static string CurrentDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(mCurrentDirectory))
                {
//#if RELEASE
                    mCurrentDirectory = Directory.GetCurrentDirectory();
//#else
//                    mCurrentDirectory = Directory.GetCurrentDirectory() + "/../../../../..";
//#endif
                }
                return mCurrentDirectory;
            }
        }

        private static Encoding GetEncoding(string name)
        {
            switch (name)
            {
                case "ASCII":
                    return Encoding.ASCII;
                case "BigEndianUnicode":
                    return Encoding.BigEndianUnicode;
                case "Default":
                    return Encoding.Default;
                case "Unicode":
                    return Encoding.Unicode;
                case "UTF32":
                    return Encoding.UTF32;
                case "UTF7":
                    return Encoding.UTF7;
                case "UTF8":
                    return Encoding.UTF8;
                default:
                    break;
            }
            return Encoding.GetEncoding(name);
        }

        public static string SourceDirectory { get; private set; } = CurrentDirectory + Cfg.SourceDirectory;
        public static string JsonDirectory { get; private set; } = CurrentDirectory + Cfg.JsonDirectory;
        public static string CSharpDirectory { get; private set; } = CurrentDirectory + Cfg.CSharpDirectory;
        public static string SourcePostfix { get; private set; } = Cfg.SourcePostfix;
        public static string JsonPostfix { get; private set; } = Cfg.JsonPostfix;
        public static string CSharpPostfix { get; private set; } = Cfg.CSharpPostfix;
        public static Encoding SourceEncoding { get; private set; } = GetEncoding(Cfg.SourceEncoding);
        public static Encoding JsonEncoding { get; private set; } = GetEncoding(Cfg.JsonEncoding);
        public static Encoding CSharpEncoding { get; private set; } = GetEncoding(Cfg.CSharpEncoding);
        public static string DoubleKeyIDPrefix { get; private set; } = Cfg.DoubleKeyIDPrefix;
        public static Formatting JsonFormatting { get; private set; } = Cfg.JsonFormatting;
        public static List<string> NameSpaceList { get; private set; } = Cfg.NameSpaceList;
    }
}
