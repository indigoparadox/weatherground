using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeatherGround;

namespace WeatherThumb {
    public partial class WeatherThumb : Form {

        public const int SYSMENU_REFRESH_ID = 0x1;
        public const int SYSMENU_OPTIONS_ID = 0x2;
        public const int SYSMENU_WUNDERGROUND_ID = 0x3;

        protected WeatherGroundUnder checker = null;
        protected Bitmap pictureBitmap = null;
        protected Timer checkTimer = new Timer();
        protected Brush backgroundColor = Brushes.AliceBlue;
        protected uint thumbBoxSize = 128;

        public WeatherThumb() {
            InitializeComponent();

            int iTrue = 1;
            ThumbnailAPI.DwmSetWindowAttribute(
                this.Handle,
                ThumbnailAPI.DWMWINDOWATTRIBUTE.DWMWA_FORCE_ICONIC_REPRESENTATION,
                ref iTrue,
                sizeof( int )
            );
            ThumbnailAPI.DwmSetWindowAttribute(
                this.Handle,
                ThumbnailAPI.DWMWINDOWATTRIBUTE.DWMWA_HAS_ICONIC_BITMAP,
                ref iTrue,
                sizeof( int )
            );

            this.UpdateZipCode();
        }

        private void WeatherThumb_Load( object sender, EventArgs e ) {
            this.UpdateWeather();

            this.checkTimer.Tick += CheckTimer_Tick;
            this.checkTimer.Interval = 60000 * 5;
            this.checkTimer.Enabled = true;
            this.checkTimer.Start();

        }

        private void CheckTimer_Tick( object sender, EventArgs e ) {
            this.UpdateWeather();
        }

        public void UpdateZipCode() {
            this.checker = new WeatherGroundUnder( Properties.Settings.Default.ZipCode );
        }

        public void UpdateWeather() {
            WeatherResponse response = checker.GetWeather();

            Bitmap iconBitmap = GetWeatherBitmap( response.Conditions );
            iconBitmap.MakeTransparent();
            Bitmap iconComplete;
            using( Graphics g = Graphics.FromImage( iconBitmap ) ) {
                Font tempFont = new Font( "Arial", 64 );
                SizeF tempSize = g.MeasureString( response.Temperature, tempFont );
                iconComplete = new Bitmap( iconBitmap.Width + tempFont.Height, iconBitmap.Height + tempFont.Height, PixelFormat.Format32bppRgb );
                using( Graphics cg = Graphics.FromImage( iconComplete ) ) {
                    cg.FillRectangle(
                        this.backgroundColor,
                        new Rectangle(
                            0,
                            0,
                            iconComplete.Width,
                            iconComplete.Height
                        )
                    );
                    cg.DrawImageUnscaled(
                        iconBitmap,
                        new Point( (iconComplete.Width / 2) - (iconBitmap.Width / 2), 0 )
                    );
                    /*
                    cg.FillRectangle(
                        this.backgroundColor,
                        new Rectangle(
                            0,
                            iconComplete.Height - (int)tempSize.Height,
                            iconComplete.Width,
                            (int)tempSize.Height
                        )
                    );
                    */
                    cg.DrawString(
                        response.Temperature,
                        tempFont,
                        Brushes.Black,
                        new Point(
                            (iconComplete.Width / 2) - ((int)tempSize.Width / 2),
                            iconComplete.Height - (int)tempSize.Height
                        )
                    );
                }
            }

            this.pictureBitmap = new Bitmap( iconComplete );

            //iconComplete.MakeTransparent();
            IntPtr hBitmap = ThumbnailAPI.MakeIcon( iconComplete, 128, true );
            this.Icon = Icon.FromHandle( hBitmap );
            //ThumbnailAPI.DestroyIcon( hBitmap );

            this.Text = response.ConditionsRaw;

            //this.pictureBitmap = GetWeatherBitmap( response.Conditions );
            //this.pictureBitmap.MakeTransparent();

            ThumbnailAPI.DwmInvalidateIconicBitmaps( this.Handle );
            this.Invalidate();
            this.Update();
            this.Refresh();
            Application.DoEvents();
        }

        protected Bitmap GenerateScaledBitmap( Bitmap bitmap, uint width, uint height ) {
            float scale = Math.Min( (float)width / bitmap.Width, (float)height / bitmap.Height );
            int scaleWidth = (int)(bitmap.Width * scale);
            int scaleHeight = (int)(bitmap.Height * scale);
            Bitmap temp = new Bitmap( (int)width, (int)height );
            using( Graphics g = Graphics.FromImage( temp ) ) {
                g.FillRectangle( this.backgroundColor, 0, 0, width, height );
                g.DrawImage(
                    bitmap,
                    new Rectangle(
                        (int)((width / 2) - (scaleWidth / 2)),
                        (int)((height / 2) - (scaleHeight / 2)),
                        scaleWidth,
                        scaleHeight
                    )
                );
            }
            return temp;
        }

        protected override void OnPaintBackground( PaintEventArgs e ) {
            e.Graphics.FillRectangle( this.backgroundColor, new Rectangle( 0, 0, this.Width, this.Height ) );
            if( null != this.pictureBitmap ) {
                e.Graphics.DrawImage( pictureBitmap, new Rectangle( 12, 12, 128, 128 ), new Rectangle( 0, 0, this.pictureBitmap.Width, this.pictureBitmap.Height ), GraphicsUnit.Pixel );
            }
        }

        protected static Bitmap GetWeatherBitmap( WeatherConditions conditionsIn ) {
            switch( conditionsIn ) {
                case WeatherConditions.Clear:
                    return new Bitmap( Properties.Resources.weather_clear );

                case WeatherConditions.Overcast:
                    return new Bitmap( Properties.Resources.weather_overcast );

                case WeatherConditions.PartlyCloudy:
                    return new Bitmap( Properties.Resources.weather_few_clouds );

                case WeatherConditions.Rain:
                    return new Bitmap( Properties.Resources.weather_showers );

                case WeatherConditions.Storm:
                    return new Bitmap( Properties.Resources.weather_storm );

                case WeatherConditions.Snow:
                    return new Bitmap( Properties.Resources.weather_snow );

                default:
                    return new Bitmap( Properties.Resources.weather_severe_alert );
            }
        }

        private void WeatherThumb_Resize( object sender, EventArgs e ) {
            if( FormWindowState.Minimized != this.WindowState ) {
                this.WindowState = FormWindowState.Minimized;
            }
        }

        /*
        private void ClickMenuOptions_Click( object sender, EventArgs e ) {
            new WeatherOptions( this ).ShowDialog();
        }

        private void ClickMenuWunderground_Click( object sender, EventArgs e ) {
            System.Diagnostics.Process.Start(
                String.Format( "http://www.wunderground.com/cgi-bin/findweather/getForecast?query={0}&MR=1", this.checker.ZipCode )
            );
        }
        */

        private void WeatherThumb_Click( object sender, EventArgs e ) {
            /*
                ContextMenuStrip clickMenu = new ContextMenuStrip();

                clickMenu.Items.Add( "&Options..." ).Click += ClickMenuOptions_Click;
                clickMenu.Items.Add( "Open &Wunderground..." ).Click += ClickMenuWunderground_Click;

                clickMenu.Show( ((MouseEventArgs)e).Location );
            */
        }

        protected override void OnHandleCreated( EventArgs e ) {
            base.OnHandleCreated( e );

            // Get a handle to a copy of this form's system (window) menu.
            IntPtr hSysMenu = ThumbnailAPI.GetSystemMenu( this.Handle, false );
            ThumbnailAPI.AppendMenu( hSysMenu, ThumbnailAPI.MF_SEPARATOR, 0, string.Empty );
            ThumbnailAPI.AppendMenu( hSysMenu, ThumbnailAPI.MF_STRING, SYSMENU_REFRESH_ID, "&Refresh" );
            ThumbnailAPI.AppendMenu( hSysMenu, ThumbnailAPI.MF_SEPARATOR, 0, string.Empty );
            ThumbnailAPI.AppendMenu( hSysMenu, ThumbnailAPI.MF_STRING, SYSMENU_OPTIONS_ID, "&Options…" );
            ThumbnailAPI.AppendMenu( hSysMenu, ThumbnailAPI.MF_STRING, SYSMENU_WUNDERGROUND_ID, "Open &Wunderground…" );
        }

        protected void OnThumbnail( uint width, uint height ) {
            this.thumbBoxSize = height;
            Bitmap temp = GenerateScaledBitmap( this.pictureBitmap, width, height );
            ThumbnailAPI.DwmSetIconicThumbnail( this.Handle, temp.GetHbitmap(), 0 );
        }

        protected void OnLivePreview( uint width, uint height ) {
            if( 0 >= width || 0 >= height ) {
                width = this.thumbBoxSize;
                height = this.thumbBoxSize;
            }
            Bitmap temp = GenerateScaledBitmap( this.pictureBitmap, width, height );
            ThumbnailAPI.DwmSetIconicLivePreviewBitmap( this.Handle, temp.GetHbitmap(), IntPtr.Zero, 0 );
        }

        protected void OnCommand( int wParam ) {
            switch( wParam ) {
                case SYSMENU_REFRESH_ID:
                    this.UpdateWeather();
                    break;

                case SYSMENU_OPTIONS_ID:
                    new WeatherOptions( this ).ShowDialog();
                    break;

                case SYSMENU_WUNDERGROUND_ID:
                    System.Diagnostics.Process.Start(
                        String.Format( "http://www.wunderground.com/cgi-bin/findweather/getForecast?query={0}&MR=1", this.checker.ZipCode )
                    );
                    break;

                case ThumbnailAPI.SC_RESTORE:
                    System.Diagnostics.Process.Start(
                        String.Format( "http://www.wunderground.com/cgi-bin/findweather/getForecast?query={0}&MR=1", this.checker.ZipCode )
                    );
                    return;
            }
        }

        protected override void WndProc( ref Message m ) {
            base.WndProc( ref m );

            switch( m.Msg ) {
                case ThumbnailAPI.WM_SYSCOMMAND:
                    this.OnCommand( (int)m.WParam );
                    break;

                case ThumbnailAPI.WM_DWMSENDICONICTHUMBNAIL:
                    this.OnThumbnail( ThumbnailAPI.HIWORD( (uint)m.LParam ), ThumbnailAPI.LOWORD( (uint)m.LParam ) );
                    break;

                case ThumbnailAPI.WM_DWMSENDICONICLIVEPREVIEWBITMAP:
                    this.OnLivePreview( ThumbnailAPI.HIWORD( (uint)m.LParam ), ThumbnailAPI.LOWORD( (uint)m.LParam ) );
                    break;
            }

            base.WndProc( ref m );
        }
    }
}
