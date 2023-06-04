using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using UnityEngine.Assertions;
using XLua;
using XLua.LuaDLL;
using XLua.CSObjectWrap;
using System.Diagnostics;
using System.Collections;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using TMPro;

namespace BigBiaENMod
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin
    {

        public static ManualLogSource log = new ManualLogSource("EN"); 
        public const string pluginGuid = "Cadenza.BigBia.EnMod";
        public const string pluginName = "BigBia ENMOD";
        public const string pluginVersion = "0.5";
        public static bool enabled;
        public static bool enabledDebugLogging = false;
        public static Dictionary<string, string> translationDict;
        public static string sourceDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString();
        public static string parentDir = Directory.GetParent(Main.sourceDir).ToString();
        public static string configDir = Path.Combine(parentDir, "config");
        public static List<string> TAUN = new List<string>();
        public static List<string> UIUN = new List<string>();
        


        public static List<string> missingta = new List<string>();
        public static List<string> missingtx = new List<string>();
        public static string dumppath = Path.Combine(BepInEx.Paths.PluginPath, "Dump");





        public static Dictionary<string, string> FileToDictionary(string dir)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            IEnumerable<string> lines = File.ReadLines(Path.Combine(sourceDir, "Translations", dir));

            foreach (string line in lines)
            {

                var arr = line.Split('¤');
                if (arr[0] != arr[1])
                {
                    var pair = new KeyValuePair<string, string>(Regex.Replace(arr[0], @"\t|\n|\r", ""), arr[1]);

                    if (!dict.ContainsKey(pair.Key))
                        dict.Add(pair.Key, pair.Value);
                    else
                    {

                    }
                        Main.log.LogInfo($"Found a duplicated line while parsing {dir}: {pair.Key}");


                }

              

            }
            return dict;
        }



        public void Awake()
        {
            log = Logger;
          
            Main.log.LogInfo("Logger Online !");

            translationDict = FileToDictionary("TAKV.txt");
            translationDict = translationDict.MergeLeft(FileToDictionary("UIKV.txt"));

            //Dump.DumpUIKV();





            var harmony = new Harmony("Cadenza.IWOL.EnMod");
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
           




        }


       private void Update()
        {
         


            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (File.Exists(Path.Combine(dumppath, "TAKV.txt")))
                {
                    File.Delete(Path.Combine(dumppath, "TAKV.txt"));
                }
                
               
               
                using (StreamWriter sw = new StreamWriter(Path.Combine(dumppath, "TAKV.txt")))
                {
                    foreach (var s in TAUN.Distinct())
                    {
                        sw.WriteLine(s);
                    }
                }
            }
            if(Input.GetKeyDown(KeyCode.F2))
            {
                if (File.Exists(Path.Combine(dumppath, "UIKV.txt")))
                {
                    File.Delete(Path.Combine(dumppath, "UIKV.txt"));
                }

                Dump.DumpUIKV();

                /*var UITextList = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Text>().ToList();
                var TMPList = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>().ToList();
                Main.log.LogInfo("Count : " + UITextList.Count + " // " + TMPList.Count);
                foreach(var uit in UITextList)
                {
                    var text = uit.text;
                    if(!Main.translationDict.ContainsKey(text))
                    {
                        Main.UIUN.Add(text.Replace("\n", "\\n").Replace("\r", "\\r"));
                    }
                }
                foreach (var uit in TMPList)
                {
                    var text = uit.text;
                    if (!Main.translationDict.ContainsKey(text))
                    {
                        Main.UIUN.Add(text.Replace("\n", "\\n").Replace("\r", "\\r"));
                    }
                }

            }
                using (StreamWriter sw = new StreamWriter(Path.Combine(dumppath, "UIKV.txt")))
            {
                foreach (var s in UIUN.Distinct())
                {
                    if (Helpers.IsChinese(s))
                    {
                        sw.WriteLine(s);
                    }
                }*/
            }
           
        }



    }



    [HarmonyPatch(typeof(LocalizationMgr), "GetLocalizationDic")]
    static class Test_Patch
    {

        static void Prefix(LocalizationMgr __instance, LanguageType type)
        {
            __instance.CurLangDic = new Dictionary<string, string>();
            string[] array = Resources.Load<TextAsset>("Localization/" + type.ToString().ToLower()).text.Split(new char[] { '\n' });
            for (int i = 1; i < array.Length - 1; i++)
            {
                string[] array2 = array[i].Split(new char[] { ',' });
                if (array2.Length > 2)
                {
                    string text = "";
                    for (int j = 1; j < array2.Length; j++)
                    {
                        text = text + array2[j] + ",";
                    }
                    text = text.Substring(0, text.Length - 2);
                    Main.log.LogInfo("text : " + text);
                    text = text.Replace("\\n", "\n");
                    if(Main.translationDict.ContainsKey(text))
                    {
                    text = Main.translationDict[text];
                    }
                    else
                    {
                        if(text != null && text != "")
                        {
                            if(Helpers.IsChinese(text))
                            {
                                Main.log.LogInfo("Adding to untranslated TA... : " + text);

                                Main.TAUN.Add(text);
                            }

                        }
                    }
                    __instance.CurLangDic.Add(array2[0], text);


                }
                else
                {
                    string text2 = array2[1].Substring(0, array2[1].Length - 1);
                    Main.log.LogInfo("text : " + text2);
                    text2 = text2.Replace("\\n", "\n");
                    if (Main.translationDict.ContainsKey(text2))
                    {
                        text2 = Main.translationDict[text2];
                    }
                    else
                    {
                        if (text2 != null && text2 != "")
                        {
                            if (Helpers.IsChinese(text2))
                            {
                                Main.log.LogInfo("Adding to untranslated TA... : " + text2);
                                Main.TAUN.Add(text2);
                            }

                        }
                    }
                    __instance.CurLangDic.Add(array2[0], text2);
                }
            }
        }
    }

    [HarmonyPatch(typeof(LocalizationMgr), "GetLocalizationWord")]
    static class Test_Patch2
    {
        static void Prefix(LocalizationMgr __instance, ref string key)
        {
            if (!__instance.CurLangDic.ContainsKey(key))
            {
                Main.log.LogInfo("text2 : " + key);

            }

        }
    }
    
    [HarmonyPatch(typeof(TMP_Text), "text", MethodType.Getter)]
    static class TMP_Patch
    {
        static void Postfix(ref string __result)
        {
            if(Main.translationDict.ContainsKey(__result))
            {
                __result = Main.translationDict[__result];
            }
            if (Main.translationDict.ContainsKey(__result.Replace("\n", "\\n").Replace("\r", "\\r")))
            {
                __result = Main.translationDict[__result.Replace("\n", "\\n").Replace("\r", "\\r")].Replace("\\n", "\n").Replace("\\r", "\r");
            }
        }
    }

    [HarmonyPatch(typeof(UnityEngine.UI.Text), "text", MethodType.Getter)]
    static class UIText_Patch
    {
        static void Postfix(ref string __result)
        {
            if (Main.translationDict.ContainsKey(__result))
            {
                __result = Main.translationDict[__result];
            }
            if (Main.translationDict.ContainsKey(__result.Replace("\n", "\\n").Replace("\r", "\\r")))
            {
                __result = Main.translationDict[__result.Replace("\n", "\\n").Replace("\r", "\\r")].Replace("\\n", "\n").Replace("\\r", "\r");
            }
        }
    }
    
   [HarmonyPatch]
    static class Resources_Patch
    {
        static MethodBase TargetMethod()
        {
            return typeof(Resources).GetMethods().Where(x => x.IsGenericMethod && x.Name == "Load").First().MakeGenericMethod(typeof(UnityEngine.Object));
        }
        static void Postfix(ref string path, UnityEngine.Object __result)
        {
            Main.log.LogInfo("Type : " + __result.GetType());
            Main.log.LogInfo("Name : " + __result.name);

        }
    }
    public static class DictionaryExtensions
    {
        // Works in C#3/VS2008:
        // Returns a new dictionary of this ... others merged leftward.
        // Keeps the type of 'this', which must be default-instantiable.
        // Example: 
        //   result = map.MergeLeft(other1, other2, ...)
        public static T MergeLeft<T, K, V>(this T me, params IDictionary<K, V>[] others)
            where T : IDictionary<K, V>, new()
        {
            T newMap = new T();
            foreach (IDictionary<K, V> src in
                (new List<IDictionary<K, V>> { me }).Concat(others))
            {
                // ^-- echk. Not quite there type-system.
                foreach (KeyValuePair<K, V> p in src)
                {
                    newMap[p.Key] = p.Value;
                }
            }
            return newMap;
        }

        public static Dictionary<TKey, TValue>
        Merge<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> dictionaries)
        {
            var result = new Dictionary<TKey, TValue>(dictionaries.First().Comparer);
            foreach (var dict in dictionaries)
                foreach (var x in dict)
                    result[x.Key] = x.Value;
            return result;
        }

    }


    public static class Helpers
    {
        public static readonly Regex cjkCharRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}");
        public static bool IsChinese(string s)
        {
            return cjkCharRegex.IsMatch(s);
        }
        public static string CustomEscape(string s)
        {
            return s.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
        }
        public static string CustomUnescape(string s)
        {
            return s.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");
        }
    }
}


