using System;
using System.Text.RegularExpressions;

namespace Preloader
{
	// Token: 0x02000003 RID: 3
	public static class Helpers
	{
		// Token: 0x06000005 RID: 5 RVA: 0x000026F8 File Offset: 0x000008F8
		public static bool IsChinese(string s)
		{
			return Helpers.cjkCharRegex.IsMatch(s);
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002718 File Offset: 0x00000918
		public static string CustomEscape(string s)
		{
			return s.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002758 File Offset: 0x00000958
		public static string CustomUnescape(string s)
		{
			return s.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");
		}

		// Token: 0x0400000D RID: 13
		public static readonly Regex cjkCharRegex = new Regex("\\p{IsCJKUnifiedIdeographs}");
	}
}
