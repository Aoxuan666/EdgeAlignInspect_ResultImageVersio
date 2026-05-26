using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace EdgeAlignInspect
{
	internal static class SdkResultImageSelfTest
	{
		public static int Run(string outputDirectory)
		{
			Directory.CreateDirectory(outputDirectory);
			string imagePath = Path.Combine(outputDirectory, "sdk_selftest_input.png");
			string resultPath = Path.Combine(outputDirectory, "sdk_selftest_result.png");
			string logPath = Path.Combine(outputDirectory, "sdk_selftest_log.txt");

			try
			{
				using (Bitmap image = CreateTestImage())
				{
					image.Save(imagePath, ImageFormat.Png);
					EdgeInspectJob job = CreateTestJob(image.Width, image.Height);
					TemplateEdgeInspectSdk sdk = new TemplateEdgeInspectSdk();
					EdgeInspectResult result = sdk.RunInspection(image, job, 2.0, 1.0, 1.0, true);
					if (result.ResultImage == null)
					{
						File.WriteAllText(logPath, "FAIL: ResultImage is null.\r\n" + result.Message);
						return 2;
					}
					using (Bitmap resultImage = new Bitmap(result.ResultImage))
					{
						resultImage.Save(resultPath, ImageFormat.Png);
						File.WriteAllText(logPath,
							"OK\r\n" +
							"Success=" + result.Success + "\r\n" +
							"ResultImage=" + resultImage.Width + "x" + resultImage.Height + "\r\n" +
							"Message=" + result.Message + "\r\n" +
							"Input=" + imagePath + "\r\n" +
							"Output=" + resultPath);
					}
					return 0;
				}
			}
			catch (Exception ex)
			{
				File.WriteAllText(logPath, "FAIL: " + ex);
				return 1;
			}
		}

		private static Bitmap CreateTestImage()
		{
			Bitmap bitmap = new Bitmap(900, 620);
			using (Graphics g = Graphics.FromImage(bitmap))
			{
				g.Clear(Color.FromArgb(24, 24, 24));
				using (Brush board = new SolidBrush(Color.FromArgb(215, 215, 215)))
				using (Brush copper = new SolidBrush(Color.FromArgb(75, 75, 75)))
				using (Pen edgePen = new Pen(Color.White, 3f))
				{
					g.FillRectangle(board, 80, 80, 700, 390);
					g.FillRectangle(copper, 360, 120, 110, 310);
					g.FillRectangle(copper, 540, 120, 95, 310);
					g.DrawLine(edgePen, 260, 120, 260, 430);
					g.DrawLine(edgePen, 520, 120, 520, 430);
				}
			}
			return bitmap;
		}

		private static EdgeInspectJob CreateTestJob(int width, int height)
		{
			EdgeInspectJob job = new EdgeInspectJob
			{
				UseReferenceLine = false,
				DetectMode = DefectDetectMode.Both
			};
			job.Match.Enabled = false;
			job.DetectItems.Add(new DetectRoiItem
			{
				Name = "自测检测ROI",
				Enabled = true,
				UseReferenceLine = false,
				AngleReference = DetectAngleReferenceMode.Vertical,
				NominalDistancePx = 0.0,
				BurrTolerancePx = 2.0,
				DentTolerancePx = 2.0,
				Roi = RotRectF.FromAxisAligned(new RectangleF(width * 0.23f, height * 0.20f, width * 0.14f, height * 0.50f)),
				Caliper = new CaliperParameters
				{
					NumMeasures = 34,
					MeasureLength = 36.0,
					MeasureWidth = 6.0,
					Sigma = 1.0,
					Threshold = 12.0,
					MeasureInterpolation = "bicubic",
					MeasureSelect = "first",
					Transition = "positive"
				}
			});
			job.Normalize();
			return job;
		}
	}
}
