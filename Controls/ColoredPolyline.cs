using System.Collections.Generic;
using System.Drawing;

namespace EdgeAlignInspect
{
	public sealed class ColoredPolyline
	{
		public List<PointF> Points = new List<PointF>();

		public Color Color = Color.Cyan;

		public float Width = 2f;

		public bool Arrow = false;
	}
}
