using System;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// HALCON 卡尺测量参数。
	/// </summary>
	/// <remarks>
	/// 线基准、圆基准和检测 ROI 均使用该参数对象；圆基准额外使用 <see cref="SearchOutward"/> 控制径向外扩搜索。
	/// </remarks>
	public sealed class CaliperParameters
	{
		/// <summary>沿 ROI 方向布置的测量卡尺数量。</summary>
		public int NumMeasures { get; set; } = 30;

		/// <summary>单个卡尺沿搜索方向的长度，单位为像素。</summary>
		public double MeasureLength { get; set; } = 30.0;

		/// <summary>单个卡尺垂直搜索方向的宽度，单位为像素。</summary>
		public double MeasureWidth { get; set; } = 5.0;

		/// <summary>边缘平滑参数，传递给 HALCON 测量算子。</summary>
		public double Sigma { get; set; } = 1.0;

		/// <summary>边缘强度阈值，值越大越倾向于忽略弱边缘。</summary>
		public double Threshold { get; set; } = 20.0;

		/// <summary>圆基准径向搜索时从理论圆向外扩展的距离，单位为像素。</summary>
		public double SearchOutward { get; set; } = 6.0;

		/// <summary>HALCON 测量插值方式，例如 <c>bicubic</c>。</summary>
		public string MeasureInterpolation { get; set; } = "bicubic";

		/// <summary>HALCON 边缘选择方式，例如 <c>first</c>、<c>last</c> 或 <c>all</c>。</summary>
		public string MeasureSelect { get; set; } = "first";

		/// <summary>边缘极性，例如 <c>positive</c>、<c>negative</c> 或 <c>all</c>。</summary>
		public string Transition { get; set; } = "negative";

		/// <summary>
		/// 创建当前卡尺参数的深拷贝。
		/// </summary>
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
