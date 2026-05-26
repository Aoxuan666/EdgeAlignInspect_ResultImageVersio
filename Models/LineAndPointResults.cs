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

    [Serializable]
    public sealed class EdgeCircleFit
    {
        public bool Success { get; set; }
        public PointF Center { get; set; }
        public double Radius { get; set; }
        public string Message { get; set; } = "";
        public List<PointF> MeasurePoints { get; } = new List<PointF>();
    }

    [Serializable]
    public sealed class EdgePointResult
    {
        public int DetectIndex { get; set; }
        public string DetectName { get; set; }
        public int Index { get; set; }
        public PointF Point { get; set; }
        public double SignedDistanceRawPx { get; set; }
        public double SignedDistancePx { get; set; }
        public double DeltaPx { get; set; }
        public double SignedDistanceValue { get; set; }
        public double DeltaValue { get; set; }
        public bool IsBurr { get; set; }
        public bool IsDent { get; set; }
    }
}
