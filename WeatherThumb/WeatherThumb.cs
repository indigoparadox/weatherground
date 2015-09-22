using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeatherGround;

namespace WeatherThumb {
    public partial class WeatherThumb : Form {

        public static readonly int SYSMENU_REFRESH_ID = 0x1;
        public static readonly int SYSMENU_OPTIONS_ID = 0x2;
        public static readonly int SYSMENU_WUNDERGROUND_ID = 0x3;

        protected WeatherGroundUnder checker = null;
        protected Bitmap pictureBitmap = null;
        protected Timer checkTimer = new Timer();

        public WeatherThumb() {
            InitializeComponent();

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
                iconComplete = new Bitmap( iconBitmap.Width + tempFont.Height, iconBitmap.Height + tempFont.Height );
                using( Graphics cg = Graphics.FromImage( iconComplete ) ) {
                    cg.DrawImageUnscaled(
                        iconBitmap,
                        new Point( (iconComplete.Width / 2) - (iconBitmap.Width / 2), 0 )
                    );
                    cg.FillRectangle(
                        Brushes.White,
                        new Rectangle(
                            0,
                            iconComplete.Height - (int)tempSize.Height,
                            iconComplete.Width,
                            (int)tempSize.Height
                        )
                    );
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

            this.Invalidate();
            this.Update();
            this.Refresh();
            Application.DoEvents();
        }

        protected override void OnPaintBackground( PaintEventArgs e ) {
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

        protected override void WndProc( ref Message m ) {
            base.WndProc( ref m );

            // Test if the About item was selected from the system menu
            if( (ThumbnailAPI.WM_SYSCOMMAND == m.Msg) && (SYSMENU_REFRESH_ID == (int)m.WParam) ) {
                this.UpdateWeather();
            } else if( (ThumbnailAPI.WM_SYSCOMMAND == m.Msg) && (SYSMENU_OPTIONS_ID == (int)m.WParam) ) {
                new WeatherOptions( this ).ShowDialog();
            } else if( (ThumbnailAPI.WM_SYSCOMMAND == m.Msg) && (SYSMENU_WUNDERGROUND_ID == (int)m.WParam) ) {
                System.Diagnostics.Process.Start(
                    String.Format( "http://www.wunderground.com/cgi-bin/findweather/getForecast?query={0}&MR=1", this.checker.ZipCode )
                );
            }
        }
    }
}
