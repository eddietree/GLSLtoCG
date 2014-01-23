using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace GLSLtoCG
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            btn_drop_files.DragDrop += new DragEventHandler(this.btn_drop_files_DragDrop);
            btn_drop_files.DragEnter += new DragEventHandler(this.btn_drop_files_DragEnter);
            btn_drop_files.DragLeave += new EventHandler(this.btn_drop_files_DragLeave);
            btn_drop_files.BackColor = Color.LightGray;

            Console.SetOut(new Log(textbox_log));
        }

        private void btn_drop_files_Click(object sender, EventArgs e)
        {

        }

        private void btn_drop_files_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                btn_drop_files.BackColor = Color.LightGreen;
            }
        }

        private void btn_drop_files_DragLeave(object sender, EventArgs e)
        {
            btn_drop_files.BackColor = Color.LightGray;
        }

        private void btn_drop_files_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            Console.WriteLine(String.Format("Dropped in {0} files", files.Count().ToString()));

            foreach (string file in files)
            {
                Console.WriteLine(String.Format("Processing file: {0}", file));

                string file_in = file;
                string file_ext_in = Path.GetExtension(file);
                string file_ext_out ="";

                ShaderType shader_type = ShaderType.FRAGMENT_SHADER;

                if (file_ext_in == ".vp")
                {
                    shader_type = ShaderType.VERTEX_SHADER;
                    file_ext_out = ".cgvp";
                }
                else if (file_ext_in == ".fp")
                {
                    shader_type = ShaderType.FRAGMENT_SHADER;
                    file_ext_out = ".cgfp";
                }
                else
                {
                    Debug.Fail( String.Format("Unrecognized file type: {0}", file_ext_in ) );
                }

                string file_out = file_in.Replace(file_ext_in, file_ext_out);

                Translator translator = new Translator();
                translator.LoadFile(file_in, file_out, shader_type);

                /*string file_out = "tex.cgfp";
                ShaderType type = ShaderType.FRAGMENT_SHADER;

                Translator translator = new Translator();
                translator.LoadFile(file_in, file_out, type);*/

            }

            btn_drop_files.BackColor = Color.LightGray;
        }

    }
}
