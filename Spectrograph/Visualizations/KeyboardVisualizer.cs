using OrionSparkLedTestBed;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Spectrograph.Visualizations
{
    public class KeyboardVisualizer
    {
        private bool _enabled = true;

        public KeyboardVisualizer()
        {
            LogitechGsdk.LogiLedInit();
            bool result = LogitechGsdk.LogiLedSetTargetDevice(4); // 1 == mouse, 4 == keyboard

            if (!result)
            {
                _enabled = false;
                return;
            }

            LogitechGsdk.LogiLedSaveCurrentLighting();
            LogitechGsdk.LogiLedSetLighting(0, 0, 0);
        }

        public void SetKeyboardLighting(Bitmap bitmap)
        {
            if (!_enabled)
            {
                return;
            }

            bitmap = new Bitmap(bitmap, new Size(21, 6));

            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var length = bitmapData.Stride * bitmapData.Height;

            byte[] bytes = new byte[length];

            Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
            bitmap.UnlockBits(bitmapData);
            
            LogitechGsdk.LogiLedSetLightingFromBitmap(bytes);
        }

        public void Dispose()
        {
            if (_enabled)
            {
                LogitechGsdk.LogiLedRestoreLighting();
            }

            LogitechGsdk.LogiLedShutdown();
        }
    }
}
