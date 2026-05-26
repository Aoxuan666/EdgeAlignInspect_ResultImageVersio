using System;
using System.Collections.Generic;
using System.Drawing;

namespace EdgeAlignInspect
{
	[Serializable]
	/// <summary>
	/// 边缘点拟合出的直线结果。
	/// </summary>
	public sealed class EdgeLineFit
	{
		/// <summary>直线拟合是否成功。</summary>
		public bool Success { get; set; }

		/// <summary>直线在当前图像内的起点。</summary>
		public PointF P1 { get; set; }

		/// <summary>直线在当前图像内的终点。</summary>
		public PointF P2 { get; set; }

		/// <summary>直线角度，单位为弧度。</summary>
		public double AngleRad { get; set; }

		/// <summary>拟合说明或失败原因。</summary>
		public string Message { get; set; } = "";

		/// <summary>参与拟合或显示的边缘测量点。</summary>
		public List<PointF> MeasurePoints { get; } = new List<PointF>();
	}
}
