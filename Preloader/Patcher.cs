using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using HarmonyLib;

namespace Preloader
{
	public static class Patcher
	{
		public static string[] forbidden = new string[] { "英雄之家", "地下城生成器5", "英雄之家", "图鉴", "英雄之家", "祝福小屋", "祝福小屋退出" , "扭蛋机" , "冒险碑", "黑名单", "挂壁人", "玩具箱", "建造场景" , "建造场景", "建筑商店", "建筑商店", "肉鸽徒弟", "忠臣", "仇人", "胆小鬼", "巨人", "天才", "智者", "壮汉", "流浪狗", "人鱼" , "出售", "地下城营地" , "营地",
                    "入场选择" , "火把" };
        public static Dictionary<string, string> FileToDictionary(string dir)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			IEnumerable<string> enumerable = File.ReadLines(Path.Combine(Patcher.sourceDir, "Translations", dir));
			foreach (string text in enumerable)
			{
				string[] array = text.Split(new char[] { '¤' });
				bool flag = array[0] != array[1];
				if (flag)
				{
					KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>(Regex.Replace(array[0], "\\t|\\n|\\r", ""), array[1]);
					bool flag2 = !dictionary.ContainsKey(keyValuePair.Key);
					if (flag2)
					{
						dictionary.Add(keyValuePair.Key, keyValuePair.Value);
					}
					else
					{
						Console.Write("Found a duplicated line while parsing " + dir + ": " + keyValuePair.Key);
					}
				}
			}
			return dictionary;
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000002 RID: 2 RVA: 0x00002154 File Offset: 0x00000354
		public static IEnumerable<string> TargetDLLs { get; } = new string[] { "Assembly-CSharp.dll" };

		// Token: 0x06000003 RID: 3 RVA: 0x0000215C File Offset: 0x0000035C
		public static void Patch(AssemblyDefinition assembly)
		{
			Patcher.DLLDict = Patcher.FileToDictionary("DLLKV.txt");
			foreach (TypeDefinition typeDefinition in assembly.MainModule.Types)
			{

				//Console.Write("Type found : " + typeDefinition.FullName);
				foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
				{
					bool hasBody = methodDefinition.HasBody;
                    if (hasBody && !methodDefinition.Name.Contains("Init"))

                    {
                        for (int i = 0; i < methodDefinition.Body.Instructions.Count; i++)
						{
							bool flag = methodDefinition.Body.Instructions[i] != null && methodDefinition.Body.Instructions[i].OpCode == OpCodes.Ldstr;
							if (flag)
							{
								object operand = methodDefinition.Body.Instructions[i].Operand;
								string text = methodDefinition.Body.Instructions[i].Operand.ToString();

								bool flag2 = Patcher.DLLDict.ContainsKey(Helpers.CustomEscape(operand.ToString()));
								if (flag2)
								{
									methodDefinition.Body.Instructions[i].Operand = Helpers.CustomUnescape(Patcher.DLLDict[Helpers.CustomEscape(operand.ToString())]);
                                 
                                    bool flag3 = !Patcher.DLLNEW.ContainsKey(text);
									if (flag3)
									{
                                        if (!Patcher.DLLNEW.ContainsKey(text))
										{ 
                                            Patcher.DLLNEW.Add(text, methodDefinition.Body.Instructions[i].Operand.ToString());
                                        }
                                    }
								}
								else
								{


                                    if (methodDefinition.Body.Instructions[i + 1].ToString().Contains("Localization"))
                                    {
                                        Patcher.DLLUN.Add(methodDefinition.Body.Instructions[i].Operand.ToString());
                                    }
                                    else
                                    {
                                        if (!methodDefinition.Body.Instructions[i].Operand.ToString().Contains("_") && !methodDefinition.Body.Instructions[i].Operand.ToString().Contains("/"))
                                        {

                                            if (!methodDefinition.Body.Instructions[i + 1].ToString().Contains("ID") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("MOD") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("equal") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("get_")
                                                                                            && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("Equal") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("Mod"))
                                            {

                                                forbidden.AddItem(methodDefinition.Body.Instructions[i + 1].ToString());

                                            }
                                            if (!methodDefinition.Body.Instructions[i + 1].ToString().Contains("ID") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("MOD") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("equal") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("get_")
                                                       && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("Equal") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("Mod")
                                                       && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("CardData") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("RotateCardAnimate") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("CardSlot"))
                                            {
                                                if (!forbidden.Contains(methodDefinition.Body.Instructions[i + 1].ToString()))
                                                {
                                                    foreach (var f in forbidden)
                                                    {
                                                        if (!methodDefinition.Body.Instructions[i + 1].ToString().Contains(f))
                                                        {
                                                            if (Helpers.IsChinese(methodDefinition.Body.Instructions[i].Operand.ToString()))
                                                            {
                                                                Console.WriteLine("i+1 : " + methodDefinition.Body.Instructions[i + 1].ToString());
                                                                Console.Write("Operand : " + methodDefinition.Body.Instructions[i].Operand.ToString());
                                                                Patcher.DLLUN.Add(methodDefinition.Body.Instructions[i].Operand.ToString());
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
							}
						}
					}

                    //One line makes everything crash, and it's 修仙武侠大战僵尸 // Similar lines may create issues in the future. Check relevant methods. Filters don't really work b
                }
                if (typeDefinition.HasNestedTypes)
				{
					foreach (var nestedType in typeDefinition.NestedTypes)
					{
						foreach (MethodDefinition methodDefinition in nestedType.Methods)
						{
							bool hasBody = methodDefinition.HasBody;
							if (hasBody && !methodDefinition.Name.Contains("Init"))
							{
								for (int i = 0; i < methodDefinition.Body.Instructions.Count; i++)
								{
									bool flag = methodDefinition.Body.Instructions[i] != null && methodDefinition.Body.Instructions[i].OpCode == OpCodes.Ldstr;
									if (flag)
									{
										object operand = methodDefinition.Body.Instructions[i].Operand;
										string text = methodDefinition.Body.Instructions[i].Operand.ToString();

										bool flag2 = Patcher.DLLDict.ContainsKey(Helpers.CustomEscape(operand.ToString()));
										if (flag2)
										{
											methodDefinition.Body.Instructions[i].Operand = Helpers.CustomUnescape(Patcher.DLLDict[Helpers.CustomEscape(operand.ToString())]);
										
											bool flag3 = !Patcher.DLLNEW.ContainsKey(text);
											if (flag3)
											{
												if(!Patcher.DLLNEW.ContainsKey(text))
												{ 
												Patcher.DLLNEW.Add(text, methodDefinition.Body.Instructions[i].Operand.ToString());
                                                }
                                            }
										}
                                        else
                                        {

                                            if (methodDefinition.Body.Instructions[i + 1].ToString().Contains("Localization"))
                                            {

                                                Patcher.DLLUN.Add(methodDefinition.Body.Instructions[i].Operand.ToString());
                                            }
                                            else
                                            {
                                                if (!methodDefinition.Body.Instructions[i].Operand.ToString().Contains("_") && !methodDefinition.Body.Instructions[i].Operand.ToString().Contains("/"))
                                                {

                                                    if (!methodDefinition.Body.Instructions[i + 1].ToString().Contains("ID") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("MOD") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("equal") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("get_")
                                                                                                    && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("Equal") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("Mod"))
                                                    {

                                                        forbidden.AddItem(methodDefinition.Body.Instructions[i + 1].ToString());

                                                    }
                                                    if (!methodDefinition.Body.Instructions[i + 1].ToString().Contains("ID") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("MOD") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("equal") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("get_")
                                                        && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("Equal") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("Mod")
                                                        && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("CardData") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("RotateCardAnimate") && !methodDefinition.Body.Instructions[i + 1].ToString().Contains("CardSlot"))
                                                    {
                                                        if (!forbidden.Contains(methodDefinition.Body.Instructions[i + 1].ToString()))
                                                        {
                                                            foreach (var f in forbidden)
                                                            {
                                                                if (!methodDefinition.Body.Instructions[i + 1].ToString().Contains(f))
                                                                {
																	if(Helpers.IsChinese(methodDefinition.Body.Instructions[i].Operand.ToString()))
																	{ 
                                                                    Console.WriteLine("i+1 : " + methodDefinition.Body.Instructions[i + 1].ToString());
                                                                    Console.Write("Operand : " + methodDefinition.Body.Instructions[i].Operand.ToString());
                                                                    Patcher.DLLUN.Add(methodDefinition.Body.Instructions[i].Operand.ToString());
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                    }
								}
							}
						}
					}
				}
			}
			bool flag4 = File.Exists(Patcher.DLLUNfile);
			if (flag4)
			{
				File.Delete(Patcher.DLLUNfile);
			}
			using (StreamWriter streamWriter = File.AppendText(Patcher.DLLUNfile))
			{

				foreach (string text2 in Patcher.DLLUN.Distinct())
				{

					//Console.Write("TEXT2 : " + text2);
					bool flag5 = Helpers.IsChinese(text2) && text2 != null && text2 != "" && !text2.Contains("_");
					if (flag5)
					{
						if(!forbidden.Contains(text2))
						{ 
                        //Console.Write("TEXT3 : " + Helpers.CustomEscape(text2) + Environment.NewLine);

                        streamWriter.Write(Helpers.CustomEscape(text2) + Environment.NewLine);
                        }
                    }
				}
			}
			bool flag6 = File.Exists(Patcher.NewDLLKVfile);
			if (flag6)
			{
				File.Delete(Patcher.NewDLLKVfile);
			}
			using (StreamWriter streamWriter2 = File.AppendText(Patcher.NewDLLKVfile))
			{

				foreach (KeyValuePair<string, string> keyValuePair in Enumerable.Distinct<KeyValuePair<string, string>>(Patcher.DLLNEW))
				{
					bool flag7 = Helpers.IsChinese(keyValuePair.Key) && keyValuePair.Value != null;
					if (flag7)
					{
						streamWriter2.Write(Helpers.CustomEscape(keyValuePair.Key) + "¤" + Helpers.CustomEscape(keyValuePair.Value) + Environment.NewLine);
					}
				}
			}
           
        }

		// Token: 0x04000001 RID: 1
		public static ManualLogSource Log;

		// Token: 0x04000002 RID: 2
		public static Dictionary<string, string> DLLDict = new Dictionary<string, string>();

		// Token: 0x04000003 RID: 3
		public static string DLLUNfile = Path.Combine(BepInEx.Paths.PluginPath, "Dump", "DLLUN.txt");

		// Token: 0x04000004 RID: 4
		public static string NewDLLKVfile = Path.Combine(Paths.PluginPath, "Dump", "DLLNewKV.txt");

		// Token: 0x04000005 RID: 5
		public static string TAUNfile = Path.Combine(Paths.PluginPath, "Dump", "TAUN.txt");

		// Token: 0x04000006 RID: 6
		public static string NewTAKVfile = Path.Combine(Paths.PluginPath, "Dump", "TANewKV.txt");

		// Token: 0x04000007 RID: 7
		public static List<string> DLLUN = new List<string>();

		// Token: 0x04000008 RID: 8
		public static Dictionary<string, string> DLLNEW = new Dictionary<string, string>();

		// Token: 0x04000009 RID: 9
		public static List<string> TAUN = new List<string>();

		// Token: 0x0400000A RID: 10
		public static Dictionary<string, string> TANEW = new Dictionary<string, string>();

		// Token: 0x0400000B RID: 11
		public static string sourceDir = Paths.PluginPath;
	}
}
