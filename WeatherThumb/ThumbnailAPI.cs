using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WeatherThumb {
    public class ThumbnailAPI {

        [Flags]
        public enum DWMWINDOWATTRIBUTE : uint {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_LAST
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct Rect {
            internal Rect( int left, int top, int right, int bottom ) {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout( LayoutKind.Sequential )]
        public struct ThumbnailProperties {
            public int dwFlags;
            public Rect rcDestination;
            public Rect rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;
        }

        public const int WM_SYSCOMMAND = 0x112;
        public const int WM_DWMSENDICONICTHUMBNAIL = 0x0323;
        public const int WM_DWMSENDICONICLIVEPREVIEWBITMAP = 0x0326;
        public const int SC_RESTORE = 0xF120;
        public const int MF_STRING = 0x0;
        public const int MF_SEPARATOR = 0x800;

        [DllImport( "dwmapi.dll", PreserveSig = true )]
        public static extern int DwmSetWindowAttribute( IntPtr hwnd, DWMWINDOWATTRIBUTE attr, ref int attrValue, int attrSize );

        [DllImport( "user32.dll", CharSet = CharSet.Auto )]
        public static extern bool DestroyIcon( IntPtr handle );

        [DllImport( "dwmapi.dll", PreserveSig = false )]
        public static extern void DwmRegisterThumbnail( IntPtr destinationWindowHandle, IntPtr sourceWindowHandle, out IntPtr thumbnailHandle );

        [DllImport( "dwmapi.dll", PreserveSig = false )]
        public static extern void DwmUnregisterThumbnail( IntPtr thumbnailHandle );

        [DllImport( "dwmapi.dll", PreserveSig = false )]
        public static extern void DwmUpdateThumbnailProperties( IntPtr thumbnailHandle, ref ThumbnailProperties properties );

        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern IntPtr GetSystemMenu( IntPtr hWnd, bool bRevert );

        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern bool AppendMenu( IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem );

        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern bool InsertMenu( IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem );

        [DllImport( "dwmapi.dll" )]
        public static extern int DwmSetIconicLivePreviewBitmap( IntPtr hwnd, IntPtr hbitmap, IntPtr ptClient, uint flags );

        [DllImport( "dwmapi.dll" )]
        public static extern int DwmSetIconicThumbnail( IntPtr hwnd, IntPtr hbitmap, uint flags );

        [DllImport( "dwmapi.dll" )]
        public static extern int DwmInvalidateIconicBitmaps( IntPtr hwnd );

        public static uint HIWORD( UInt32 inp ) {
            return (inp >> 16);
        }

        public static uint LOWORD( UInt32 inp ) {
            return (inp & 0xffff);
        }

        public static IntPtr MakeIcon( Image img, int size, bool keepAspectRatio ) {
            using( Bitmap square = new Bitmap( size, size ) )// create new bitmap
            {
                using( Graphics g = Graphics.FromImage( square ) ) // allow drawing to it
                {
                    int x, y, w, h; // dimensions for new image

                    if( !keepAspectRatio || img.Height == img.Width ) {
                        // just fill the square
                        x = y = 0; // set x and y to 0
                        w = h = size; // set width and height to size
                    } else {
                        // work out the aspect ratio
                        float r = (float)img.Width / (float)img.Height;

                        // set dimensions accordingly to fit inside size^2 square
                        if( r > 1 ) { // w is bigger, so divide h by r
                            w = size;
                            h = (int)((float)size / r);
                            x = 0; y = (size - h) / 2; // center the image
                        } else { // h is bigger, so multiply w by r
                            w = (int)((float)size * r);
                            h = size;
                            y = 0; x = (size - w) / 2; // center the image
                        }
                    }

                    // make the image shrink nicely by using HighQualityBicubic mode
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage( img, x, y, w, h ); // draw image with specified dimensions
                    g.Flush(); // make sure all drawing operations complete before we get the icon

                    IntPtr hIcon = square.GetHicon();
                    return hIcon;
                }
            }
        }
    }
}
