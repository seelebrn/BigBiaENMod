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
using AssetsTools.NET;
using AssetsTools.NET.Extra;

using System.Dynamic;
using System.Runtime.Versioning;

namespace BigBiaENMod
{
    internal class Dump
    {

        public static void DumpUIKV()
        {
            var missingtx = new List<string>();

            var di = new DirectoryInfo(Path.Combine(Application.dataPath)).GetFiles();
            if (File.Exists(Path.Combine(Main.dumppath, "UIKV.txt")))
            {
                File.Delete(Path.Combine(Main.dumppath, "UIKV.txt"));
            }
            foreach (var f in di)
            {
                if (!f.FullName.Contains("resS") && !f.FullName.Contains(".resource") && !f.FullName.Contains(".info") && !f.FullName.Contains("boot.config") && !f.FullName.Contains(".json") && !f.FullName.Contains("sharedassets4"))
                {


                    Main.log.LogInfo("Initiating dumping process...");
                    var manager = new AssetsManager();

                    Main.log.LogInfo("Now processing file... : " + f.FullName);


                    manager.MonoTempGenerator = new MonoCecilTempGenerator(Path.Combine(Application.dataPath, "Managed"));
                    manager.LoadClassPackage(Path.Combine(BepInEx.Paths.PluginPath, "classdata.tpk"));

                    var afileInst = manager.LoadAssetsFile(f.FullName, true);
                    var afile = afileInst.file;
                    manager.LoadClassDatabaseFromPackage(afile.Metadata.UnityVersion);
                    foreach (var texInfo in afile.GetAssetsOfType(AssetClassID.MonoBehaviour))
                    {
                        try
                        {

                            var texBase = manager.GetBaseField(afileInst, texInfo);
                            var text = texBase["m_Text"].AsString;

                            if (!Main.translationDict.ContainsKey(text.Replace("\n", "\\n").Replace("\r", "\\r")))
                            {
                                missingtx.Add(text.Replace("\n", "\\n").Replace("\r", "\\r"));
                            }
                        }
                        catch
                        {

                        }
                    }



                    foreach (var s in missingtx.Distinct())
                    {
                        Main.log.LogInfo("MissingTx Content : " + s);
                    }

                }
            }
            using (StreamWriter sw = new StreamWriter(Path.Combine(Main.dumppath, "UIKV.txt")))
            {

                foreach (var s in missingtx.Distinct())
                {
                    if (s != null && s != "")
                    {
                        if(Helpers.IsChinese(s))
                        { 
                        sw.WriteLine(s);
                        }
                    }
                }
            }
        }
    }
}
