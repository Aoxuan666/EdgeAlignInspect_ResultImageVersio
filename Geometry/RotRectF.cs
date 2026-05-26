using System;
using System.Drawing;

namespace EdgeAlignInspect
{
	[Serializable]
	public struct RotRectF
	{
		public PointF Center;

		public float AngleRad;

		public float HalfLen1;

		public float HalfLen2;

		public bool IsEmpty => HalfLen1 <= 0.5f || HalfLen2 <= 0.5f;

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

		public float Phi
		{
			get
			{
				return AngleRad;
			}
			set
			{
				AngleRad = NormalizeAngle(value);
			}
		}

		public RotRectF(PointF center, float angleRad, float halfLen1, float halfLen2)
		{
			Center = center;
			AngleRad = NormalizeAngle(angleRad);
			HalfLen1 = halfLen1;
			HalfLen2 = halfLen2;
		}

		public static RotRectF FromAxisAligned(RectangleF r)
		{
			PointF center = new PointF(r.Left + r.Width * 0.5f, r.Top + r.Height * 0.5f);
			return new RotRectF(center, 0f, Math.Max(1f, r.Width * 0.5f), Math.Max(1f, r.Height * 0.5f));
		}

		public PointF GetAxisU()
		{
			float x = (float)Math.Cos(AngleRad);
			float y = (float)Math.Sin(AngleRad);
			return new PointF(x, y);
		}

		public PointF GetAxisV()
		{
			float y = (float)Math.Cos(AngleRad);
			float num = (float)Math.Sin(AngleRad);
			return new PointF(0f - num, y);
		}

		public PointF[] GetCorners()
		{
			PointF axisU = GetAxisU();
			PointF axisV = GetAxisV();
			PointF center = Center;
			return new PointF[4]
			{
				new PointF(center.X + axisU.X * HalfLen1 + axisV.X * HalfLen2, center.Y + axisU.Y * HalfLen1 + axisV.Y * HalfLen2),
				new PointF(center.X + axisU.X * HalfLen1 - axisV.X * HalfLen2, center.Y + axisU.Y * HalfLen1 - axisV.Y * HalfLen2),
				new PointF(center.X - axisU.X * HalfLen1 - axisV.X * HalfLen2, center.Y - axisU.Y * HalfLen1 - axisV.Y * HalfLen2),
				new PointF(center.X - axisU.X * HalfLen1 + axisV.X * HalfLen2, center.Y - axisU.Y * HalfLen1 + axisV.Y * HalfLen2)
			};
		}

		public PointF GetEdgeMidLongPos()
		{
			PointF axisU = GetAxisU();
			return new PointF(Center.X + axisU.X * HalfLen1, Center.Y + axisU.Y * HalfLen1);
		}

		public PointF GetEdgeMidLongNeg()
		{
			PointF axisU = GetAxisU();
			return new PointF(Center.X - axisU.X * HalfLen1, Center.Y - axisU.Y * HalfLen1);
		}

		public PointF GetEdgeMidShortPos()
		{
			PointF axisV = GetAxisV();
			return new PointF(Center.X + axisV.X * HalfLen2, Center.Y + axisV.Y * HalfLen2);
		}

		public PointF GetEdgeMidShortNeg()
		{
			PointF axisV = GetAxisV();
			return new PointF(Center.X - axisV.X * HalfLen2, Center.Y - axisV.Y * HalfLen2);
		}

		public PointF GetRotateCorner()
		{
			PointF[] corners = GetCorners();
			return (corners != null && corners.Length == 4) ? corners[0] : Center;
		}

		public PointF GetResizeCorner()
		{
			return GetRotateCorner();
		}

		public PointF GetRotateHandle(float outDist)
		{
			return GetRotateCorner();
		}

		public RectangleF BoundsAABB()
		{
			PointF[] corners = GetCorners();
			float num = corners[0].X;
			float num2 = corners[0].X;
			float num3 = corners[0].Y;
			float num4 = corners[0].Y;
			for (int i = 1; i < 4; i++)
			{
				num = Math.Min(num, corners[i].X);
				num2 = Math.Max(num2, corners[i].X);
				num3 = Math.Min(num3, corners[i].Y);
				num4 = Math.Max(num4, corners[i].Y);
			}
			return RectangleF.FromLTRB(num, num3, num2, num4);
		}

		public RectangleF BoundingBox()
		{
			return BoundsAABB();
		}

		public PointF GetArrowEnd(float len)
		{
			PointF axisV = GetAxisV();
			return new PointF(Center.X + axisV.X * len, Center.Y + axisV.Y * len);
		}

		public static float NormalizeAngle(float a)
		{
			float num = (float)Math.PI;
			float num2 = 2f * num;
			while (a > num)
			{
				a -= num2;
			}
			while (a < 0f - num)
			{
				a += num2;
			}
			return a;
		}
	}
}
