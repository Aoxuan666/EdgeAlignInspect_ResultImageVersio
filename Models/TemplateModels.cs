using System;

namespace EdgeAlignInspect
{
    [Serializable]
    public sealed class TemplateTeachData
    {
        public bool HasTemplate { get; set; }
        public byte[] ModelBytes { get; set; }
        public double RefRow { get; set; }
        public double RefCol { get; set; }
        public double RefAngle { get; set; }

        public void Clear()
        {
            HasTemplate = false;
            ModelBytes = null;
            RefRow = 0;
            RefCol = 0;
            RefAngle = 0;
        }

        public TemplateTeachData DeepClone()
        {
            return new TemplateTeachData
            {
                HasTemplate = HasTemplate,
                ModelBytes = ModelBytes == null ? null : (byte[])ModelBytes.Clone(),
                RefRow = RefRow,
                RefCol = RefCol,
                RefAngle = RefAngle
            };
        }
    }

    [Serializable]
    public sealed class TemplateMatchParameters
    {
        public bool Enabled { get; set; } = true;
        public int NumLevels { get; set; } = 5;
        public double AngleStart { get; set; } = -0.3;
        public double AngleExtent { get; set; } = 0.6;
        public double MinScore { get; set; } = 0.5;
    }
}
