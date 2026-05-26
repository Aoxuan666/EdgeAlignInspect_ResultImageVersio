using System;
using System.Collections.Generic;

namespace EdgeAlignInspect
{
    [Serializable]
    public sealed class BaseRoiInspectResult
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public RotRectF RoiCur { get; set; }
        public EdgeLineFit Line { get; set; } = new EdgeLineFit();
        public CaliperParameters CaliperUsed { get; set; } = EdgeInspectJob.CreateDefaultBaseCaliper();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

    [Serializable]
    public sealed class CircleBaseRoiInspectResult
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public CircleRoiF Circle1RoiCur { get; set; }
        public CircleRoiF Circle2RoiCur { get; set; }
        public EdgeCircleFit Circle1 { get; set; } = new EdgeCircleFit();
        public EdgeCircleFit Circle2 { get; set; } = new EdgeCircleFit();
        public EdgeLineFit Line { get; set; } = new EdgeLineFit();
        public CaliperParameters CaliperUsed { get; set; } = EdgeInspectJob.CreateDefaultCircleCaliper();
        public bool Success { get; set; }
        public string Message { get; set; } = "";
    }

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
        public double BurrTolerancePx { get; set; }
        public double DentTolerancePx { get; set; }
        public bool UseExternalBurrTolerance { get; set; }
        public double ExternalBurrTolerance { get; set; }
        public double PixelResolutionX { get; set; } = 1.0;
        public double PixelResolutionY { get; set; } = 1.0;
        public DefectDetectMode DetectMode { get; set; } = DefectDetectMode.Both;
        public List<EdgePointResult> Points { get; } = new List<EdgePointResult>();
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
