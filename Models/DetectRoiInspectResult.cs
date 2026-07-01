using System;
using System.Collections.Generic;
using System.Drawing;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class DetectRoiInspectResult
	{
		public int Index { get; set; }

		public string Name { get; set; }

		public bool Enabled { get; set; }

		public RotRectF RoiCur { get; set; }

		public bool UseReferenceLine { get; set; }

		public ReferenceBaseKind ReferenceBaseKind { get; set; } = ReferenceBaseKind.LineRoi;

		public int BaseRoiIndex { get; set; } = -1;

		public int CircleBaseRoiIndex { get; set; } = -1;

		public int CirclePointRoiIndex { get; set; } = -1;

		public DetectAngleReferenceMode AngleReference { get; set; } = DetectAngleReferenceMode.ParallelToBase;

		public EdgeLineFit FittedLine { get; set; } = new EdgeLineFit();

		public EdgeLineFit JudgeLine { get; set; } = new EdgeLineFit();

		public CaliperParameters CaliperUsed { get; set; } = EdgeInspectJob.CreateDefaultDetectCaliper();

		public double RefAngleRad { get; set; }

		public double FittedAngleRad { get; set; }

		public double AngleDeltaRad { get; set; }

		public double AngleDeltaDeg { get; set; }

		public string AngleReferenceText { get; set; } = "";

		public double NominalDistancePx { get; set; }

		public PointF ReferencePointCur { get; set; }

		public PointF ReferenceFootPoint { get; set; }

		public bool HasOverallDistance { get; set; }

		public bool HasPointToLineDistance { get; set; }

		public PointF OverallMeasurePoint { get; set; }

		public PointF OverallFootPoint { get; set; }

		public EdgeLineFit OverallReferenceLine { get; set; } = new EdgeLineFit();

		public double OverallDistancePx { get; set; }

		public double OverallDistanceValue { get; set; }

		public double OverallDeltaPx { get; set; }

		public double OverallDeltaValue { get; set; }

		public bool IsOverEdge { get; set; }

		public bool IsCopperLeak { get; set; }

		public double PointToLineDistancePx
		{
			get
			{
				return OverallDistancePx;
			}
			set
			{
				OverallDistancePx = value;
			}
		}

		public double PointToLineDistanceValue
		{
			get
			{
				return OverallDistanceValue;
			}
			set
			{
				OverallDistanceValue = value;
			}
		}

		public double PointToLineDeltaPx
		{
			get
			{
				return OverallDeltaPx;
			}
			set
			{
				OverallDeltaPx = value;
			}
		}

		public double PointToLineDeltaValue
		{
			get
			{
				return OverallDeltaValue;
			}
			set
			{
				OverallDeltaValue = value;
			}
		}

		public bool IsPointToLineTooMuch
		{
			get
			{
				return IsOverEdge;
			}
			set
			{
				IsOverEdge = value;
			}
		}

		public bool IsPointToLineTooLittle
		{
			get
			{
				return IsCopperLeak;
			}
			set
			{
				IsCopperLeak = value;
			}
		}

		public double BurrTolerancePx { get; set; }

		public double DentTolerancePx { get; set; }

		public bool UseExternalBurrTolerance { get; set; }

		public double ExternalBurrTolerance { get; set; }

		public double ExternalDentTolerance { get; set; }

		public double ExternalOverEdgeTolerance { get; set; }

		public double ExternalCopperLeakTolerance { get; set; }

		public double PixelResolutionX { get; set; } = 1.0;

		public double PixelResolutionY { get; set; } = 1.0;

		public DefectDetectMode DetectMode { get; set; } = DefectDetectMode.Both;

		public List<EdgePointResult> Points { get; } = new List<EdgePointResult>();

		public NgReason NgReasons { get; set; } = NgReason.None;

		public InspectionLanguage Language { get; set; } = InspectionLanguage.Chinese;

		public string NgReasonText => NgReasonHelper.ToText(NgReasons, Language);

		public int BurrCount { get; set; }

		public int DentCount { get; set; }

		public double SignedMin { get; set; }

		public double SignedMax { get; set; }

		public double SignedMean { get; set; }

		public double DeltaMin { get; set; }

		public double DeltaMax { get; set; }

		public double DeltaMean { get; set; }

		public double MaxPositiveDeltaPx { get; set; }

		public double MaxPositiveDeltaValue { get; set; }

		public bool Success { get; set; }

		public string Message { get; set; } = "";
	}
}
