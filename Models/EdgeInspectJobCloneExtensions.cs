namespace EdgeAlignInspect
{
	public static class EdgeInspectJobCloneExtensions
	{
		public static EdgeInspectJob DeepClone(this EdgeInspectJob src)
		{
			if (src == null)
			{
				return null;
			}
			EdgeInspectJob edgeInspectJob = new EdgeInspectJob
			{
				TemplateRoi = src.TemplateRoi,
				UseReferenceLine = src.UseReferenceLine,
				DetectMode = src.DetectMode,
				UseExternalBurrTolerance = src.UseExternalBurrTolerance,
				ExternalBurrTolerance = src.ExternalBurrTolerance,
				ExternalDentTolerance = src.ExternalDentTolerance,
				ExternalOverEdgeTolerance = src.ExternalOverEdgeTolerance,
				ExternalCopperLeakTolerance = src.ExternalCopperLeakTolerance,
				PixelResolutionX = src.PixelResolutionX,
				PixelResolutionY = src.PixelResolutionY,
				Match = new TemplateMatchParameters
				{
					Enabled = (src.Match?.Enabled ?? true),
					NumLevels = (src.Match?.NumLevels ?? 5),
					AngleStart = (src.Match?.AngleStart ?? (-0.3)),
					AngleExtent = (src.Match?.AngleExtent ?? 0.6),
					MinScore = (src.Match?.MinScore ?? 0.5)
				},
				BaseCaliper = (src.BaseCaliper?.DeepClone() ?? EdgeInspectJob.CreateDefaultBaseCaliper()),
				CircleCaliper = (src.CircleCaliper?.DeepClone() ?? EdgeInspectJob.CreateDefaultCircleCaliper()),
				DetectCaliper = (src.DetectCaliper?.DeepClone() ?? EdgeInspectJob.CreateDefaultDetectCaliper()),
				TeachData = (src.TeachData?.DeepClone() ?? new TemplateTeachData())
			};
			if (src.BaseRois != null)
			{
				foreach (BaseRoiItem baseRoi in src.BaseRois)
				{
					edgeInspectJob.BaseRois.Add(baseRoi?.DeepClone() ?? new BaseRoiItem
					{
						Caliper = edgeInspectJob.BaseCaliper.DeepClone()
					});
				}
			}
			if (src.CircleBaseRois != null)
			{
				foreach (CircleBaseRoiItem circleBaseRoi in src.CircleBaseRois)
				{
					edgeInspectJob.CircleBaseRois.Add(circleBaseRoi?.DeepClone() ?? new CircleBaseRoiItem
					{
						Caliper = edgeInspectJob.CircleCaliper.DeepClone()
					});
				}
			}
			if (src.CirclePointRois != null)
			{
				foreach (CirclePointRoiItem circlePointRoi in src.CirclePointRois)
				{
					edgeInspectJob.CirclePointRois.Add(circlePointRoi?.DeepClone() ?? new CirclePointRoiItem
					{
						Caliper = edgeInspectJob.CircleCaliper.DeepClone()
					});
				}
			}
			if (src.DetectItems != null)
			{
				foreach (DetectRoiItem detectItem in src.DetectItems)
				{
					edgeInspectJob.DetectItems.Add(detectItem?.DeepClone() ?? new DetectRoiItem
					{
						Caliper = edgeInspectJob.DetectCaliper.DeepClone()
					});
				}
			}
			edgeInspectJob.Normalize();
			return edgeInspectJob;
		}
	}
}
