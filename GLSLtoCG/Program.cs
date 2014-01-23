using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GLSLtoCG
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            /*
            string file_in = "test.vp";
            string file_out = "test.cgvp";
            ShaderType type = ShaderType.VERTEX_SHADER;
            

            string file_in = "tex.fp";
            string file_out = "tex.cgfp";
            ShaderType type = ShaderType.FRAGMENT_SHADER;

            Translator translator = new Translator();
            translator.LoadFile(file_in, file_out, type);*/
        }
    }
}
