using System;
using System.Drawing;
using System.Windows.Forms;

namespace EdgeAlignInspect
{
	[Serializable]
	public sealed class EdgeInspectionToleranceOptions
	{
		public double BurrToleranceMm { get; set; }

		public double DentToleranceMm { get; set; }

		public double OverEdgeToleranceMm { get; set; }

		public double CopperLeakToleranceMm { get; set; }

		public double PixelResolutionX { get; set; }

		public double PixelResolutionY { get; set; }
	}

	/// <summary>
	/// 面向上位机的边缘对位检测 SDK 入口。
	/// </summary>
	/// <remarks>
	/// 配置窗口属于 WinForms 对话框，应在 UI 线程或 STA 线程调用；运行检测接口会复制输入图像和 Job，避免修改调用方对象。
	/// </remarks>
	public sealed class TemplateEdgeInspectSdk
	{
		/// <summary>
		/// 打开参数配置窗口，让用户基于当前图像编辑并示教检测任务。
		/// </summary>
		/// <param name="image">用于配置和示教的源图像。方法内部会复制该图像。</param>
		/// <param name="currentJob">已有任务配置；传入 <c>null</c> 时创建默认配置。</param>
		/// <returns>用户确认后的任务配置；用户取消或图像为空时返回 <c>null</c>。</returns>
		public EdgeInspectJob OpenSetupDialog(Bitmap image, EdgeInspectJob currentJob)
		{
			if (image == null)
			{
				return null;
			}
			using (Form1 form = new Form1())
			{
				using (Bitmap image2 = new Bitmap(image))
				{
					form.SetImage(image2);
					EdgeInspectJob job = currentJob?.DeepClone() ?? new EdgeInspectJob();
					form.SetJob(job);
					DialogResult dialogResult = form.ShowDialog();
					if (dialogResult != DialogResult.OK)
					{
						return null;
					}
					return form.ReturnedJob?.DeepClone();
				}
			}
		}

		/// <summary>
		/// 使用上位机传入的图像、任务配置和判定参数执行一次检测。
		/// </summary>
		/// <param name="image">待检测图像。方法内部会复制该图像。</param>
		/// <param name="job">已完成配置和示教的检测任务。</param>
		/// <param name="acceptedTolerance">上位机传入的毛刺判定公差，单位与像素分辨率换算后的物理单位一致。</param>
		/// <param name="pixelResolutionX">X 方向单像素代表的物理尺寸，必须大于 0。</param>
		/// <param name="pixelResolutionY">Y 方向单像素代表的物理尺寸，必须大于 0。</param>
		/// <returns>检测结果。参数错误或算法异常时返回 <see cref="EdgeInspectResult.Success"/> 为 <c>false</c> 的结果对象。</returns>
		public EdgeInspectResult RunInspection(Bitmap image, EdgeInspectJob job, double acceptedTolerance, double pixelResolutionX, double pixelResolutionY)
		{
			return RunInspection(image, job, acceptedTolerance, pixelResolutionX, pixelResolutionY, false);
		}

		public EdgeInspectResult RunInspection(Bitmap image, EdgeInspectJob job, double acceptedTolerance, double pixelResolutionX, double pixelResolutionY, bool returnResultImage)
		{
			return RunInspection(image, job, new EdgeInspectionToleranceOptions
			{
				BurrToleranceMm = acceptedTolerance,
				DentToleranceMm = acceptedTolerance,
				OverEdgeToleranceMm = acceptedTolerance,
				CopperLeakToleranceMm = acceptedTolerance,
				PixelResolutionX = pixelResolutionX,
				PixelResolutionY = pixelResolutionY
			}, returnResultImage);
		}

		public EdgeInspectResult RunInspection(Bitmap image, EdgeInspectJob job, EdgeInspectionToleranceOptions options)
		{
			return RunInspection(image, job, options, false);
		}

		public EdgeInspectResult RunInspection(Bitmap image, EdgeInspectJob job, EdgeInspectionToleranceOptions options, bool returnResultImage)
		{
			if (image == null)
			{
				return new EdgeInspectResult
				{
					Success = false,
					NgReasons = NgReason.ParameterInvalid,
					Message = "Input image is null."
				};
			}
			if (job == null)
			{
				return new EdgeInspectResult
				{
					Success = false,
					NgReasons = NgReason.ParameterInvalid,
					Message = "Job parameter is null."
				};
			}
			if (options == null)
			{
				return new EdgeInspectResult
				{
					Success = false,
					NgReasons = NgReason.ParameterInvalid,
					Message = "Tolerance options parameter is null."
				};
			}
			if (options.BurrToleranceMm < 0.0 || options.DentToleranceMm < 0.0 || options.OverEdgeToleranceMm < 0.0 || options.CopperLeakToleranceMm < 0.0)
			{
				return new EdgeInspectResult
				{
					Success = false,
					NgReasons = NgReason.ParameterInvalid,
					Message = "All tolerances must be >= 0."
				};
			}
			if (options.PixelResolutionX <= 0.0)
			{
				return new EdgeInspectResult
				{
					Success = false,
					NgReasons = NgReason.ParameterInvalid,
					Message = "Pixel resolution X must be > 0."
				};
			}
			if (options.PixelResolutionY <= 0.0)
			{
				return new EdgeInspectResult
				{
					Success = false,
					NgReasons = NgReason.ParameterInvalid,
					Message = "Pixel resolution Y must be > 0."
				};
			}
			try
			{
				using (Bitmap curBmp = new Bitmap(image))
				{
					if (returnResultImage)
					{
						using (Form1 form = new Form1())
						{
							form.ShowInTaskbar = false;
							form.StartPosition = FormStartPosition.Manual;
							form.Location = new Point(-32000, -32000);
							form.ClientSize = new Size(1320, 780);
							form.Show();
							Application.DoEvents();
							form.SetImage(curBmp);
							EdgeInspectJob jobForForm = job.DeepClone() ?? new EdgeInspectJob();
							form.SetJob(jobForForm);
							Application.DoEvents();
							EdgeInspectResult result = form.RunInspectionForSdk(jobForForm, options.BurrToleranceMm, options.PixelResolutionX, options.PixelResolutionY, true);
							Application.DoEvents();
							result.ResultImage = form.LastResultImage == null ? null : new Bitmap(form.LastResultImage);
							return result;
						}
					}
					using (TemplateEdgeInspectProcessor templateEdgeInspectProcessor = new TemplateEdgeInspectProcessor())
					{
						EdgeInspectJob edgeInspectJob = job.DeepClone() ?? new EdgeInspectJob();
						edgeInspectJob.UseExternalBurrTolerance = true;
						edgeInspectJob.ExternalBurrTolerance = options.BurrToleranceMm;
						edgeInspectJob.ExternalDentTolerance = options.DentToleranceMm;
						edgeInspectJob.ExternalOverEdgeTolerance = options.OverEdgeToleranceMm;
						edgeInspectJob.ExternalCopperLeakTolerance = options.CopperLeakToleranceMm;
						edgeInspectJob.PixelResolutionX = options.PixelResolutionX;
						edgeInspectJob.PixelResolutionY = options.PixelResolutionY;
						return templateEdgeInspectProcessor.Inspect(curBmp, edgeInspectJob);
					}
				}
			}
			catch (Exception ex)
			{
				return new EdgeInspectResult
				{
					Success = false,
					NgReasons = NgReason.AlgorithmException,
					Message = "Algorithm exception: " + ex.Message
				};
			}
		}
	}
}
