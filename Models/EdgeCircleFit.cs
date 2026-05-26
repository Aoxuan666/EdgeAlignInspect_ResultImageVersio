using System;
using System.Collections.Generic;
using System.Drawing;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 边缘点拟合出的圆结果。
	/// </summary>
	public sealed class EdgeCircleFit
	{
		/// <summary>圆拟合是否成功。</summary>
		public bool Success { get; set; }

		/// <summary>拟合圆心图像坐标。</summary>
		public PointF Center { get; set; }

		/// <summary>拟合半径，单位为像素。</summary>
		public double Radius { get; set; }

		/// <summary>拟合说明或失败原因。</summary>
		public string Message { get; set; } = "";

		/// <summary>参与拟合或显示的边缘测量点。</summary>
		public List<PointF> MeasurePoints { get; } = new List<PointF>();
	}
}
