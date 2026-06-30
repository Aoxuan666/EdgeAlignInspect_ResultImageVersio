using System;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class TemplateMatchParameters
	{
		public bool Enabled { get; set; } = true;

		public int NumLevels { get; set; } = 5;

		public double AngleStart { get; set; } = -0.3;

		public double AngleExtent { get; set; } = 0.6;

		public double MinScore { get; set; } = 0.5;

		public bool UseOuterContourOnly { get; set; } = true;

		public double EdgeSigma { get; set; } = 1.2;

		public double EdgeLowThreshold { get; set; } = 15.0;

		public double EdgeHighThreshold { get; set; } = 35.0;

		public double FeatureMinDistancePx { get; set; } = 6.0;

		public int FeatureAngleBins { get; set; } = 360;

		public double EraseRadiusPx { get; set; } = 12.0;
	}
}
