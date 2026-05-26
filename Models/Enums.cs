using System;

namespace EdgeAlignInspect
{
    [Serializable]
    public enum DefectDetectMode
    {
        Both = 0,
        BurrOnly = 1,
        DentOnly = 2
    }

    [Serializable]
    public enum DetectAngleReferenceMode
    {
        ParallelToBase = 0,
        Horizontal = 1,
        Vertical = 2
    }

    [Serializable]
    public enum ReferenceBaseKind
    {
        LineRoi = 0,
        CirclePair = 1
    }
}
