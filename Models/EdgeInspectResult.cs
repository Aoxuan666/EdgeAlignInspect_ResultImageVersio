using System;
using System.Collections.Generic;
using System.Drawing;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class EdgeInspectResult
	{
		public bool Success { get; set; }

		public string Message { get; set; } = "";

		public InspectionLanguage Language { get; set; } = InspectionLanguage.Chinese;

		public bool TemplateMatchEnabled { get; set; }

		public bool TemplateMatchOk { get; set; }

		public double TemplateMatchScore { get; set; }

		public double TemplateMatchRow { get; set; }

		public double TemplateMatchCol { get; set; }

		public double TemplateMatchAngle { get; set; }

		public PointF TemplateMatchCenter { get; set; }

		public RotRectF TemplateRoiCur { get; set; }

		public List<PointF> TemplateMatchContourPoints { get; } = new List<PointF>();

		public List<BaseRoiInspectResult> BaseResults { get; } = new List<BaseRoiInspectResult>();

		public List<CircleBaseRoiInspectResult> CircleBaseResults { get; } = new List<CircleBaseRoiInspectResult>();

		public List<CirclePointRoiInspectResult> CirclePointResults { get; } = new List<CirclePointRoiInspectResult>();

		public List<DetectRoiInspectResult> DetectResults { get; } = new List<DetectRoiInspectResult>();

		public List<string> FailedItems { get; } = new List<string>();

		public RotRectF BaseRoiCur { get; set; }

		public RotRectF DetectRoiCur { get; set; }

		public EdgeLineFit BaseLine { get; set; } = new EdgeLineFit();

		public List<EdgePointResult> Points { get; } = new List<EdgePointResult>();

		public NgReason NgReasons { get; set; } = NgReason.None;

		public string NgReasonText => NgReasonHelper.ToText(NgReasons, Language);

		public int BurrCount { get; set; }

		public int DentCount { get; set; }

		public int OverEdgeCount { get; set; }

		public int CopperLeakCount { get; set; }

		public double SignedMin { get; set; }

		public double SignedMax { get; set; }

		public double SignedMean { get; set; }

		public double DeltaMin { get; set; }

		public double DeltaMax { get; set; }

		public double DeltaMean { get; set; }

		public double Nominal { get; set; }

		public double BurrTolerance { get; set; }

		public double DentTolerance { get; set; }

		public DefectDetectMode DetectMode { get; set; } = DefectDetectMode.Both;

		public bool UseReferenceLine { get; set; }

		public bool HasReferenceLineItems { get; set; }

		public bool HasSelfFitLineItems { get; set; }

		public bool IsMixedJudgeMode => HasReferenceLineItems && HasSelfFitLineItems;

		public bool UseExternalBurrTolerance { get; set; }

		public double ExternalBurrTolerance { get; set; }

		public Bitmap ResultImage { get; set; }

		public double ExternalDentTolerance { get; set; }

		public double ExternalOverEdgeTolerance { get; set; }

		public double ExternalCopperLeakTolerance { get; set; }

		public double PixelResolutionX { get; set; } = 1.0;

		public double PixelResolutionY { get; set; } = 1.0;

		public double MaxPositiveDeltaPx { get; set; }

		public double MaxPositiveDeltaValue { get; set; }
	}
}
