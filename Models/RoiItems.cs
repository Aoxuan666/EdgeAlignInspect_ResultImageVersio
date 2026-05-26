using System;

namespace EdgeAlignInspect
{
    [Serializable]
    public sealed class BaseRoiItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "基准1";
        public RotRectF Roi { get; set; }
        public CaliperParameters Caliper { get; set; } = EdgeInspectJob.CreateDefaultBaseCaliper();

        public BaseRoiItem DeepClone()
        {
            return new BaseRoiItem
            {
                Id = Id,
                Name = Name,
                Roi = Roi,
                Caliper = Caliper?.DeepClone() ?? EdgeInspectJob.CreateDefaultBaseCaliper()
            };
        }

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? "基准" : Name;
    }

    [Serializable]
    public sealed class CircleBaseRoiItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "圆基准1";
        public CircleRoiF Circle1 { get; set; }
        public CircleRoiF Circle2 { get; set; }
        public CaliperParameters Caliper { get; set; } = EdgeInspectJob.CreateDefaultCircleCaliper();

        public CircleBaseRoiItem DeepClone()
        {
            return new CircleBaseRoiItem
            {
                Id = Id,
                Name = Name,
                Circle1 = Circle1,
                Circle2 = Circle2,
                Caliper = Caliper?.DeepClone() ?? EdgeInspectJob.CreateDefaultCircleCaliper()
            };
        }

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? "圆基准" : Name;
    }

    [Serializable]
    public sealed class DetectRoiItem
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = "检测1";
        public RotRectF Roi { get; set; }
        public bool UseReferenceLine { get; set; } = true;
        public ReferenceBaseKind ReferenceBaseKind { get; set; } = ReferenceBaseKind.LineRoi;
        public int BaseRoiIndex { get; set; } = 0;
        public string BaseRoiId { get; set; } = "";
        public int CircleBaseRoiIndex { get; set; } = 0;
        public string CircleBaseRoiId { get; set; } = "";
        public DetectAngleReferenceMode AngleReference { get; set; } = DetectAngleReferenceMode.ParallelToBase;
        public double NominalDistancePx { get; set; } = 0.0;
        public double BurrTolerancePx { get; set; } = 2.0;
        public double DentTolerancePx { get; set; } = 2.0;
        public bool Enabled { get; set; } = true;
        public CaliperParameters Caliper { get; set; } = EdgeInspectJob.CreateDefaultDetectCaliper();

        public DetectRoiItem DeepClone()
        {
            return new DetectRoiItem
            {
                Id = Id,
                Name = Name,
                Roi = Roi,
                UseReferenceLine = UseReferenceLine,
                ReferenceBaseKind = ReferenceBaseKind,
                BaseRoiIndex = BaseRoiIndex,
                BaseRoiId = BaseRoiId,
                CircleBaseRoiIndex = CircleBaseRoiIndex,
                CircleBaseRoiId = CircleBaseRoiId,
                AngleReference = AngleReference,
                NominalDistancePx = NominalDistancePx,
                BurrTolerancePx = BurrTolerancePx,
                DentTolerancePx = DentTolerancePx,
                Enabled = Enabled,
                Caliper = Caliper?.DeepClone() ?? EdgeInspectJob.CreateDefaultDetectCaliper()
            };
        }

        public override string ToString() => string.IsNullOrWhiteSpace(Name) ? "检测" : Name;
    }
}
