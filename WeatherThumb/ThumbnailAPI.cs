using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WeatherThumb {
    public class ThumbnailAPI {

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
        public const int MF_STRING = 0x0;
        public const int MF_SEPARATOR = 0x800;

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
