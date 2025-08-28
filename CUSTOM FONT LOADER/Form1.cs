using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CUSTOM_FONT_LOADER
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private EmbeddedFontLoader _fontLoader;
        private void Form1_Load(object sender, EventArgs e)
        {
            _fontLoader = new EmbeddedFontLoader();

            _fontLoader.LoadFontsFromResources(
                "CUSTOM_FONT_LOADER.Nevan RUS.ttf"

            );

            label1.Font = new Font(_fontLoader.GetFontFamilyByName("Nevan RUS"), 18f, FontStyle.Regular);

        }
    }
}
