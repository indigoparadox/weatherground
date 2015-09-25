using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeatherThumb {
    public partial class WeatherOptions : Form {
        protected WeatherThumb MainForm = null;

        public WeatherOptions( WeatherThumb mainFormIn ) {
            InitializeComponent();

            this.MainForm = mainFormIn;
        }

        private void buttonOK_Click( object sender, EventArgs e ) {
            Properties.Settings.Default.ZipCode = textZipCode.Text;
            Properties.Settings.Default.Save();
            this.MainForm.UpdateZipCode();
            this.MainForm.UpdateWeather();
            this.Close();
        }

        private void buttonCancel_Click( object sender, EventArgs e ) {
            this.Close();
        }

        private void WeatherOptions_Load( object sender, EventArgs e ) {
            textZipCode.Text = Properties.Settings.Default.ZipCode;
        }
    }
}
