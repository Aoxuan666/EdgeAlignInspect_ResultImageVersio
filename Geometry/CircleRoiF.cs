using System;
using System.Drawing;

namespace EdgeAlignInspect
{
	[Serializable]
	public struct CircleRoiF
	{
		public PointF Center;

		public float Radius;

		public bool IsEmpty => Radius <= 0.5f;

		public float Cx
		{
			get
			{
				return Center.X;
			}
			set
			{
				Center = new PointF(value, Center.Y);
			}
		}

		public float Cy
		{
			get
			{
				return Center.Y;
			}
			set
			{
				Center = new PointF(Center.X, value);
			}
		}

		public CircleRoiF(PointF center, float radius)
		{
			Center = center;
			Radius = radius;
		}

		public RectangleF BoundsAABB()
		{
			return RectangleF.FromLTRB(Center.X - Radius, Center.Y - Radius, Center.X + Radius, Center.Y + Radius);
		}
	}
}
