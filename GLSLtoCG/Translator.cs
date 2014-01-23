using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace GLSLtoCG
{
    enum ShaderType
    {
        VERTEX_SHADER = 0,
        FRAGMENT_SHADER = 1
    };

    struct ShaderUniform
    {
        public string name;
        public string data_type; // float4x4, float4
    };

    struct ShaderVarying
    {
        public string name;
        public string data_type; // float4x4, float4
    };

    struct ShaderAttribute
    {
        public string name;
        public string data_type; // float4x4, float4
    };

    struct ShaderFunction
    {
        public string func_name;
        public string func_return_type;
        public string func_params;
        public string func_definition;
    }

    class Translator
    {
        private string m_filename;
        private Dictionary<string, string> m_map_replacements = new Dictionary<string,string>();

        private List<ShaderVarying> m_varyings = new List<ShaderVarying>();
        private List<ShaderUniform> m_uniforms = new List<ShaderUniform>();
        private List<ShaderAttribute> m_attribs = new List<ShaderAttribute>();
        private List<ShaderFunction> m_funcs = new List<ShaderFunction>();

        private ShaderType m_shader_type;

        public Translator()
        {
            InitDictionary();
        }

        private void InitDictionary()
        {
            m_map_replacements["vec2"] = "float2";
            m_map_replacements["vec3"] = "float3";
            m_map_replacements["vec4"] = "float4";
            m_map_replacements["mat4"] = "float4x4";
            m_map_replacements["mat3"] = "float3x3";
            m_map_replacements["lowp"] = "";
            m_map_replacements["mediump"] = "";
            m_map_replacements["highp"] = "";
            m_map_replacements["mix"] = "lerp";
            m_map_replacements["gl_Position"] = "v_position";
            m_map_replacements["gl_PointSize"] = "float psize";

            m_map_replacements["texture2D"] = "tex2D";
        }

        public static string RemoveComments(string a_text)
        {
            // remove comments
            var blockComments = @"/\*(.*?)\*/";
            var versionComments = @"#(.*?)\r?\n";
            var lineComments = @"//(.*?)\r?\n";
            var strings = @"""((\\[^\n]|[^""\n])*)""";
            var verbatimStrings = @"@(""[^""]*"")+";

            string results_no_comments = Regex.Replace(a_text,
                blockComments + "|" + lineComments + "|" + versionComments + "|" + strings + "|" + verbatimStrings,
                me =>
                {
                    if (me.Value.StartsWith("/*") || me.Value.StartsWith("//") || me.Value.StartsWith("#"))
                        return me.Value.StartsWith("//") ? Environment.NewLine : "";
                    // Keep the literal strings
                    return me.Value;
                },
                RegexOptions.Singleline);

            return results_no_comments;
        }

        private string MassageContents(string a_contents)
        {
            string result = a_contents;

            // replace matching pairs
            foreach ( KeyValuePair<string, string> entry in m_map_replacements )
            {
                result = result.Replace(entry.Key, entry.Value);
            }

            string results_no_comments = RemoveComments(result);

            // remove newlines and tabs
            results_no_comments = results_no_comments.Replace("\r\n", " ");
            results_no_comments = results_no_comments.Replace("\t", "  ");

            // collapses multiple spaces into one
            Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
            results_no_comments = regex.Replace(results_no_comments, @" ");

            return results_no_comments;
        }

        private String GetToken(ref string a_contents)
        {
            string content = a_contents;
            content = a_contents.TrimStart();

            if (content == "") return "";

            int last_index = content.IndexOfAny(new char[] { ' ', '*', '<', '>', '-', '+', '/', '%', '(', ')', '{', '}', ',', ';', '[', ']' });
            
            // if zero character, remove one
            last_index = Math.Max(last_index, 1);

            // extract token
            string token = content.Substring(0, last_index);

            // move the cursor forward
            a_contents = content.Remove(0, last_index);

            return token;
        }

        private string GetFuncParams(ref string a_contents)
        {
            a_contents = a_contents.TrimStart();

            int last_index = a_contents.IndexOf(')');

            string params_inside = a_contents.Substring(0, last_index);

            // consume contents
            a_contents = a_contents.Remove(0, last_index+1);
            a_contents = a_contents.TrimStart();

            return params_inside;
        }

        private string GetFuncDef(ref string a_contents)
        {
            string content = a_contents;
            int last_index = 0;
            int open_bracket_count = 1;

            while (open_bracket_count > 0)
            {
                int curr_index_open = content.IndexOf('{', last_index);
                int curr_index_closed = content.IndexOf('}', last_index);

                if (curr_index_closed < curr_index_open || curr_index_open <= 0)
                {
                    last_index = content.IndexOf('}', last_index);
                    --open_bracket_count;

                    if (open_bracket_count > 0)
                    {
                        ++last_index;
                    }
                }

                else
                {
                    last_index = curr_index_open + 1;
                    ++open_bracket_count;
                }
            }

            string func_def = content.Substring(0, last_index);
            func_def = func_def.Trim();

            a_contents = content.Remove(0, last_index+1);

            return func_def;
        }

        private void Parse(string a_contents)
        {
            string str_top = a_contents;

            while (str_top.Length > 0)
            {
                // get the token
                string token = GetToken(ref str_top);

                // attribute
                if (token == "attribute")
                {
                    string data_type = GetToken(ref str_top);
                    string name = GetToken(ref str_top);
                    string semi_colon = GetToken(ref str_top);

                    Debug.Assert(semi_colon == ";");

                    ShaderAttribute entry;
                    entry.data_type = data_type;
                    entry.name = name;
                    m_attribs.Add(entry);
                }

                // uniforms
                else if (token == "uniform")
                {
                    string data_type = GetToken(ref str_top);
                    string name = GetToken(ref str_top);
                    string semi_colon = GetToken(ref str_top);

                    Debug.Assert(semi_colon == ";");

                    ShaderUniform entry;
                    entry.data_type = data_type;
                    entry.name = name;
                    m_uniforms.Add(entry);
                }

                // varying
                else if (token == "varying")
                {
                    string data_type = GetToken(ref str_top);
                    string name = GetToken(ref str_top);
                    string semi_colon = GetToken(ref str_top);

                    Debug.Assert(semi_colon == ";");

                    ShaderVarying entry;
                    entry.data_type = data_type;
                    entry.name = name;
                    m_varyings.Add(entry);
                }

                else if (token == "")
                {
                    break;
                }

                // function
                else
                {
                    string func_return_type = token;
                    string func_name = GetToken(ref str_top);

                    // open paren - start of func params
                    string char_open_paren = GetToken(ref str_top);
                    Debug.Assert(char_open_paren == "(");

                    string func_params = GetFuncParams(ref str_top);

                    // open bracket - start of function def
                    string char_open_bracket = GetToken(ref str_top);
                    Debug.Assert(char_open_bracket == "{");

                    string func_definition = GetFuncDef(ref str_top);

                    // make entry
                    ShaderFunction entry;
                    entry.func_name = func_name;
                    entry.func_return_type = func_return_type;
                    entry.func_params = func_params;
                    entry.func_definition = func_definition;

                    m_funcs.Add(entry);
                }
            }
        }

        private ShaderFunction GetFunction( string a_func_name )
        {
            Debug.Assert(m_funcs.Count > 0);

            foreach( ShaderFunction func in m_funcs )
            {
                if ( func.func_name == a_func_name )
                {
                    return func;
                }
            }

            return m_funcs[0];
        }

        private string GetAttribsParamsStr()
        {
            string output = "";

            foreach (ShaderAttribute param in m_attribs)
            {
                output += String.Format("\t{0} {1},", param.data_type, param.name);
                output += System.Environment.NewLine;
            }

            return output;
        }

        private string GetUniformsParamsStr()
        {
            int sampler_count = 0;

            string output = "";

            foreach (ShaderUniform param in m_uniforms)
            {
                if (param.data_type == "sampler2D")
                {
                    output += String.Format("\t{0} {1} {2} : TEXUNIT{3},", "uniform", param.data_type, param.name, sampler_count);
                    output += System.Environment.NewLine;

                    ++sampler_count;
                }
                else
                {
                    output += String.Format("\t{0} {1} {2},", "uniform", param.data_type, param.name);
                    output += System.Environment.NewLine;
                }
            }

            return output;
        }

        private string GetVaryingsParamsStr()
        {
            string output = "";

            string varying_type = m_shader_type == ShaderType.VERTEX_SHADER ? "out" : "in";

            if (m_shader_type == ShaderType.VERTEX_SHADER)
            {
                output += String.Format("\t{0} {1} {2} : {3},", "float4", varying_type, "v_position", "POSITION");
                output += System.Environment.NewLine;
            }

            int texcoord_count = 0;
            foreach (ShaderVarying param in m_varyings)
            {
                string tex_coord_str = "TEXCOORD" + texcoord_count.ToString();
                ++texcoord_count;

                output += String.Format("\t{0} {1} {2} : {3},", param.data_type, varying_type, param.name, tex_coord_str);
                output += System.Environment.NewLine;
            }

            return output;
        }

        private string GetMainFuncParams()
        {
            // main func params
            string main_func_params = "";

            main_func_params += GetAttribsParamsStr();
            main_func_params += GetUniformsParamsStr();
            main_func_params += GetVaryingsParamsStr();

            // remove last comma
            int last_comma_index = main_func_params.LastIndexOf(',');
            if (last_comma_index > 0)
            {
                main_func_params = main_func_params.Substring(0, last_comma_index);
            }

            main_func_params += System.Environment.NewLine;
            return main_func_params;
        }

        private string GetNonMainFunctionStr()
        {
            string output = "";

            foreach (ShaderFunction func in m_funcs)
            {
                if (func.func_name == "main") 
                    continue;

                output += String.Format("{0} {1} ({2})", func.func_return_type, func.func_name, func.func_params ) + System.Environment.NewLine;
                output += "{" + System.Environment.NewLine;
                output += GetFunctionDefinitionFormated( func.func_definition ) + System.Environment.NewLine;
                output += "}" + System.Environment.NewLine;
                output += System.Environment.NewLine;
            }

            return output;
        }

        private string GetFunctionDefinitionFormated(string a_func_definition)
        {
            // replace output of fragment shader
            Regex regex_spacing = new Regex(@"gl_FragColor[ ]{0,}=[ ]{0,}", RegexOptions.None);
            string results = regex_spacing.Replace(a_func_definition, @"return ");

            // replace output of fragment shader
            Regex regex_semicolons = new Regex(@";[ ]{0,}", RegexOptions.None);
            results = regex_semicolons.Replace(results, @";" + System.Environment.NewLine + '\t');

            //
            results = Regex.Replace(results, @"(u_mvp)[ ]{0,}[*][ ]{0,}(.*?);",
             m => 
                string.Format(
                "mul( {1}, {0} );",
                m.Groups[1].Value,
                m.Groups[2].Value));


            return "\t" + results;
        }

        private string OutputCgVertexShader()
        {
            ShaderFunction main_func = GetFunction("main");

            string output = "";

            output += "// Auto-generated from GLSL vertex shader" + System.Environment.NewLine;
            output += GetNonMainFunctionStr();
            output += "void main(" + System.Environment.NewLine;
            output += GetMainFuncParams();
            output += "\t)" + System.Environment.NewLine;
            output += "{" + System.Environment.NewLine;

            output += GetFunctionDefinitionFormated(main_func.func_definition) + System.Environment.NewLine;
            output += "}" + System.Environment.NewLine;

            return output;
        }

        private string OutputCgFragmentShader()
        {
            ShaderFunction main_func = GetFunction("main");

            string output = "";

            output += "// Auto-generated from GLSL fragment shader" + System.Environment.NewLine;
            output += GetNonMainFunctionStr();
            output += "float4 main(" + System.Environment.NewLine;
            output += GetMainFuncParams();
            output += "\t)" + System.Environment.NewLine;
            output += "{" + System.Environment.NewLine;
            output += GetFunctionDefinitionFormated(main_func.func_definition) + System.Environment.NewLine;
            output += "}" + System.Environment.NewLine;

            return output;
        }

        private string BuildCgShader()
        {
            if ( m_shader_type == ShaderType.VERTEX_SHADER )
            {
                 return OutputCgVertexShader();
            }
            else if ( m_shader_type == ShaderType.FRAGMENT_SHADER )
            {
                return OutputCgFragmentShader();
            }

            Debug.Fail("Undefined shader type");
            return "ERROR";
        }

        public void LoadFile(string a_file_in, string a_file_out, ShaderType a_shader_type)
        {
            m_shader_type = a_shader_type;

            Console.WriteLine(String.Format("Loading file: {0}", a_file_in));
            m_filename = a_file_in;

            StreamReader streamReader = new StreamReader(a_file_in, Encoding.UTF8);
            string file_contents = streamReader.ReadToEnd();
            streamReader.Close();

            // convert strings and remove comments
            string contents_massaged = MassageContents(file_contents);

            /*Console.WriteLine("---- OLD FILE ---");
            Console.WriteLine(file_contents);
            Console.WriteLine("---- MASSAGED FILE ---");
            Console.WriteLine(contents_massaged);*/

            Parse(contents_massaged);

            string cg_output = BuildCgShader();

            //Console.WriteLine("---- OUTPUT FILE ---");
            //Console.WriteLine(cg_output);

            System.IO.File.WriteAllText(a_file_out, cg_output);

            string shader_type = m_shader_type == ShaderType.VERTEX_SHADER ? "vertex" : "fragment";
            Console.WriteLine("Translated {0} shader to file: {1}", shader_type, a_file_out);
        }
    }
}
