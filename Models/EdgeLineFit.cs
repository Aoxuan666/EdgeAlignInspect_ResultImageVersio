using System;
using System.Collections.Generic;
using System.Drawing;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class EdgeLineFit
	{
		public bool Success { get; set; }

		public PointF P1 { get; set; }

		public PointF P2 { get; set; }

		public double AngleRad { get; set; }

		public string Message { get; set; } = "";

		public List<PointF> MeasurePoints { get; } = new List<PointF>();
	}
}
