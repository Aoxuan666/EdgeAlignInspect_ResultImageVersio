using System;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class CaliperParameters
	{
		public int NumMeasures { get; set; } = 30;

		public double MeasureLength { get; set; } = 30.0;

		public double MeasureWidth { get; set; } = 5.0;

		public double Sigma { get; set; } = 1.0;

		public double Threshold { get; set; } = 20.0;

		public double SearchOutward { get; set; } = 6.0;

		public string MeasureInterpolation { get; set; } = "bicubic";

		public string MeasureSelect { get; set; } = "first";

		public string Transition { get; set; } = "negative";

		public CaliperParameters DeepClone()
		{
			return new CaliperParameters
			{
				NumMeasures = NumMeasures,
				MeasureLength = MeasureLength,
				MeasureWidth = MeasureWidth,
				Sigma = Sigma,
				Threshold = Threshold,
				SearchOutward = SearchOutward,
				MeasureInterpolation = MeasureInterpolation,
				MeasureSelect = MeasureSelect,
				Transition = Transition
			};
		}
	}
}
