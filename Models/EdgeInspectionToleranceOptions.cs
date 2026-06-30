using System;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class EdgeInspectionToleranceOptions
	{
		public double BurrToleranceMm { get; set; }

		public double DentToleranceMm { get; set; }

		public double OverEdgeToleranceMm { get; set; }

		public double CopperLeakToleranceMm { get; set; }

		public double PixelResolutionX { get; set; }

		public double PixelResolutionY { get; set; }
	}
}
