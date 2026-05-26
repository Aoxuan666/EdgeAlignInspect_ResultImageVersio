using System;
using System.IO;
using System.Windows.Forms;

namespace EdgeAlignInspect
{
	internal static class Program
	{
		[STAThread]
		private static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(defaultValue: false);
			if (args != null && args.Length != 0 && string.Equals(args[0], "--sdk-selftest", StringComparison.OrdinalIgnoreCase))
			{
				Environment.ExitCode = SdkResultImageSelfTest.Run(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SelfTestOutput"));
				return;
			}
			Application.Run(new TestHostForm());
		}
	}
}
