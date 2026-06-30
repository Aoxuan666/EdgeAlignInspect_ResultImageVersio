using System;
using System.Drawing;
using System.Windows.Forms;

namespace EdgeAlignInspect
{
	/// <summary>
	/// 模板边缘检测 SDK，提供参数设置和检测执行入口。
	/// </summary>
	public sealed class TemplateEdgeInspectSdk
	{
		/// <summary>
		/// 打开参数设置窗口，用于编辑边缘检测任务。
		/// </summary>
		/// <param name="image">用于设置的源图像。</param>
		/// <param name="currentJob">当前任务参数；传入 null 时创建新任务。</param>
		/// <param name="options">检测公差和像素分辨率参数。</param>
		/// <returns>用户确认后返回编辑后的任务；取消时返回 null。</returns>
		public EdgeInspectJob OpenSetupDialog(Bitmap image, EdgeInspectJob currentJob, EdgeInspectionToleranceOptions options)
		{
			if (image == null)
			{
				return null;
			}
			ValidateToleranceOptions(options);
			using (Form1 form = new Form1())
			{
				using (Bitmap image2 = new Bitmap(image))
				{
					form.SetImage(image2);
					EdgeInspectJob job = currentJob?.DeepClone() ?? new EdgeInspectJob();
					ApplyToleranceOptions(job, options);
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
		/// 使用统一公差执行检测。
		/// </summary>
		/// <param name="image">待检测图像。</param>
		/// <param name="job">检测任务参数。</param>
		/// <param name="acceptedTolerance">所有缺陷类型共用的公差，单位毫米。</param>
		/// <param name="pixelResolutionX">X 方向像素分辨率。</param>
		/// <param name="pixelResolutionY">Y 方向像素分辨率。</param>
		/// <returns>检测结果。</returns>
		public EdgeInspectResult RunInspection(Bitmap image, EdgeInspectJob job, double acceptedTolerance, double pixelResolutionX, double pixelResolutionY)
		{
			return RunInspection(image, job, acceptedTolerance, pixelResolutionX, pixelResolutionY, false);
		}

		/// <summary>
		/// 使用统一公差执行检测，并可选择返回渲染后的结果图。
		/// </summary>
		/// <param name="image">待检测图像。</param>
		/// <param name="job">检测任务参数。</param>
		/// <param name="acceptedTolerance">所有缺陷类型共用的公差，单位毫米。</param>
		/// <param name="pixelResolutionX">X 方向像素分辨率。</param>
		/// <param name="pixelResolutionY">Y 方向像素分辨率。</param>
		/// <param name="returnResultImage">是否在 EdgeInspectResult.ResultImage 中返回渲染后的结果图。</param>
		/// <returns>检测结果。</returns>
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

		/// <summary>
		/// 使用独立公差和像素分辨率参数执行检测。
		/// </summary>
		/// <param name="image">待检测图像。</param>
		/// <param name="job">检测任务参数。</param>
		/// <param name="options">检测公差和像素分辨率参数。</param>
		/// <returns>检测结果。</returns>
		public EdgeInspectResult RunInspection(Bitmap image, EdgeInspectJob job, EdgeInspectionToleranceOptions options)
		{
			return RunInspection(image, job, options, false);
		}

		/// <summary>
		/// 使用独立公差和像素分辨率参数执行检测，并可选择返回渲染后的结果图。
		/// </summary>
		/// <param name="image">待检测图像。</param>
		/// <param name="job">检测任务参数。</param>
		/// <param name="options">检测公差和像素分辨率参数。</param>
		/// <param name="returnResultImage">是否在 EdgeInspectResult.ResultImage 中返回渲染后的结果图。</param>
		/// <returns>检测结果。</returns>
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
			string validationMessage;
			if (!TryValidateToleranceOptions(options, out validationMessage))
			{
				return new EdgeInspectResult
				{
					Success = false,
					NgReasons = NgReason.ParameterInvalid,
					Message = validationMessage
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
							EdgeInspectResult result = form.RunInspectionForSdk(jobForForm, options, true);
							Application.DoEvents();
							result.ResultImage = form.LastResultImage == null ? null : new Bitmap(form.LastResultImage);
							return result;
						}
					}
					using (TemplateEdgeInspectProcessor templateEdgeInspectProcessor = new TemplateEdgeInspectProcessor())
					{
						EdgeInspectJob edgeInspectJob = job.DeepClone() ?? new EdgeInspectJob();
						ApplyToleranceOptions(edgeInspectJob, options);
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

		private static void ApplyToleranceOptions(EdgeInspectJob job, EdgeInspectionToleranceOptions options)
		{
			if (job == null)
			{
				throw new ArgumentNullException("job");
			}
			ValidateToleranceOptions(options);
			job.UseExternalBurrTolerance = true;
			job.ExternalBurrTolerance = options.BurrToleranceMm;
			job.ExternalDentTolerance = options.DentToleranceMm;
			job.ExternalOverEdgeTolerance = options.OverEdgeToleranceMm;
			job.ExternalCopperLeakTolerance = options.CopperLeakToleranceMm;
			job.PixelResolutionX = options.PixelResolutionX;
			job.PixelResolutionY = options.PixelResolutionY;
			job.Normalize();
		}

		private static void ValidateToleranceOptions(EdgeInspectionToleranceOptions options)
		{
			string message;
			if (!TryValidateToleranceOptions(options, out message))
			{
				throw new ArgumentException(message, "options");
			}
		}

		private static bool TryValidateToleranceOptions(EdgeInspectionToleranceOptions options, out string message)
		{
			if (options == null)
			{
				message = "Tolerance options parameter is null.";
				return false;
			}
			if (options.BurrToleranceMm < 0.0 || options.DentToleranceMm < 0.0 || options.OverEdgeToleranceMm < 0.0 || options.CopperLeakToleranceMm < 0.0)
			{
				message = "All tolerances must be >= 0.";
				return false;
			}
			if (options.PixelResolutionX <= 0.0)
			{
				message = "Pixel resolution X must be > 0.";
				return false;
			}
			if (options.PixelResolutionY <= 0.0)
			{
				message = "Pixel resolution Y must be > 0.";
				return false;
			}
			message = "";
			return true;
		}
	}
}
