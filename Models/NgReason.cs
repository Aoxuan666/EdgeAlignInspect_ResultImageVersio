using System;

namespace EdgeAlignInspect
{
	[Flags]
	public enum NgReason
	{
		None = 0,
		Burr = 1,
		Dent = 2,
		OverEdge = 4,
		CopperLeak = 8,
		TemplateMatchFailed = 0x10,
		BaseRoiFailed = 0x20,
		DetectRoiFailed = 0x40,
		ParameterInvalid = 0x80,
		AlgorithmException = 0x100
	}
}
