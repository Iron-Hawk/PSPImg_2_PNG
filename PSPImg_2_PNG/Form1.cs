using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PSPImg_2_PNG
{
    public partial class Form1 : Form
    {
        OpenFileDialog ofd = new OpenFileDialog();
        OpenFileDialog ofd2 = new OpenFileDialog();
        SaveFileDialog sfd = new SaveFileDialog();
        SaveFileDialog sfd2 = new SaveFileDialog();
        SaveFileDialog sfd3 = new SaveFileDialog();
        public int FileLoaded = 0;
        public int FileLoaded2 = 0;
        public int has_colorpal_data = 0;
        public int colorpal_data_over_16 = 0;
        public int colorpal_data_over_128 = 0;
        public int has_swizzle = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (FileLoaded == 1)
            {
                TextConverted.Text = "Console Messages: Converting... Please wait...";
                int_doesfilehave_colorpal_check();
            }
            else
            {
                TextConverted.Text = "Console Messages: There is no .img file loaded";
            }
        }

        public void int_doesfilehave_colorpal_check()
        {
            long byte_total = new FileInfo(ofd.FileName).Length;
            int data_amount_without_header = unchecked((int)byte_total) - 32;
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(26, SeekOrigin.Begin);
            byte[] swizzle_check = reader.ReadBytes(2);
            string swizzle_check_s = BitConverter.ToString(swizzle_check);
            switch (swizzle_check_s)
            {
                case "00-00":
                    has_swizzle = 0;
                    break;
                default:
                    has_swizzle = 1;
                    break;
            }
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] img_w_byte = reader.ReadBytes(2);
            byte[] img_h_byte = reader.ReadBytes(2);
            int img_w = BitConverter.ToInt16(img_w_byte, 0);
            int img_h = BitConverter.ToInt16(img_h_byte, 0);
            int pixel_amount = img_w * img_h;
            int pixel_amount_4 = img_w * img_h * 4;
            int data_col_pal_length = data_amount_without_header - pixel_amount;
            Console.WriteLine(pixel_amount);
            Console.WriteLine(data_amount_without_header);
            if (pixel_amount_4 == data_amount_without_header)
            {
                has_colorpal_data = 0;
            }
            else
            {
                has_colorpal_data = 1;
                if (data_col_pal_length <= 64)
                {
                    colorpal_data_over_16 = 0;
                    colorpal_data_over_128 = 0;
                }
                else
                {
                    if (data_col_pal_length > 512)
                    {
                        colorpal_data_over_16 = 1;
                        colorpal_data_over_128 = 1;
                    }
                    else
                    {
                        colorpal_data_over_16 = 1;
                        colorpal_data_over_128 = 0;
                    }
                }
            }
            int_sizecheck();
        }

        public void int_sizecheck ()
        {
            if (has_colorpal_data == 0)
            {
                if (has_swizzle == 0)
                {
                    convert_file_no_colorpal();
                }
                else
                {
                    TextConverted.Text = "Console Messages: IMG files without color pal should not be swizzled?!?";
                }

            }
            else
            {
                if (colorpal_data_over_16 == 0)
                {
                    if (has_swizzle == 0)
                    {
                        convert_file_with_colorpal();
                    }
                    else
                    {
                        convert_file_with_colorpal_wswizzle();
                    }
                }
                else
                {
                    if (colorpal_data_over_128 == 1)
                    {
                        if (has_swizzle == 0)
                        {
                            convert_file_with_colorpal_256();
                        }
                        else
                        {
                            convert_file_with_colorpal_256_wswizzle();
                        }
                    }
                    else
                    {
                        if (has_swizzle == 0)
                        {
                            convert_file_with_colorpal_256Ver2();
                        }
                        else
                        {
                            convert_file_with_colorpal_256Ver2_wswizzle();
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ofd.Filter = "All files (*.*)|*.*";
            ofd.SupportMultiDottedExtensions = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                FileLoaded = 1;
                TextFileNameDir.Text = "Opened File Directory: " + ofd.FileName;
                TextFileName.Text = "Opened File Name: " + ofd.SafeFileName;
                TextConverted.Text = "Console Messages: N/A";
            }
            else
            {
                FileLoaded = 0;
                TextFileNameDir.Text = "Opened File Directory: N/A";
                TextFileName.Text = "Opened File Name: N/A";
                TextConverted.Text = "Console Messages: No File Was Selected";
            }
        }

        public void convert_file_with_colorpal()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] img_w_byte = reader.ReadBytes(2);
            byte[] img_h_byte = reader.ReadBytes(2);
            int img_w = BitConverter.ToInt16(img_w_byte, 0);
            int img_h = BitConverter.ToInt16(img_h_byte, 0);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int current_pixel_h = 0;
            int current_pixel_pal_w = 0;
            int current_pixel_w = 0;
            int p = 0;
            List<int[]> col_pal_list = new List<int[]>();
            int[] temp_col_pal = new int[4] { 128, 128, 128, 128 };
            int[] temp2_col_pal = new int[4] { 128, 128, 128, 128 };

            int pixel_amount = img_w * img_h / 2;
            long byte_total = new FileInfo(ofd.FileName).Length;
            int pixel_data_offset = unchecked((int)byte_total) - pixel_amount;
            int col_pal_length = (pixel_data_offset - 32) / 4;
            int col_pal_length_forwidth = col_pal_length * 10;
            var img_col_pal = new Bitmap(col_pal_length_forwidth, 10);
            var img_final = new Bitmap(img_w, img_h);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            while (p < col_pal_length)
            {
                byte r_v = reader.ReadBytes(1)[0];
                byte g_v = reader.ReadBytes(1)[0];
                byte b_v = reader.ReadBytes(1)[0];
                byte a_v = reader.ReadBytes(1)[0];
                if (AlphaOverride_CheckBox.Checked)
                {
                    a_v = 255;
                }
                Console.WriteLine(r_v);
                Console.WriteLine(g_v);
                Console.WriteLine(b_v);
                Console.WriteLine(a_v);
                using (var g = Graphics.FromImage(img_col_pal))
                {
                    int w = 0;
                    while (w < 10)
                    {
                        img_col_pal.SetPixel(current_pixel_pal_w, 0, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 1, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 2, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 3, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 4, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 5, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 6, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 7, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 8, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 9, Color.FromArgb(a_v, r_v, g_v, b_v));
                        current_pixel_pal_w++;
                        w++;
                    }
                    p++;
                    temp_col_pal = new int[4] { a_v, r_v, g_v, b_v };
                    col_pal_list.Add(temp_col_pal);
                }

            }
            if (ExportColorPal_CheckBox.Checked)
            {
            sfd.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd.SupportMultiDottedExtensions = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                img_col_pal.Save(sfd.FileName);
                TextConverted.Text = "Console Messages: File has been converted and saved";
            }
            else
            {
                    TextConverted.Text = "Console Messages: Saving color pal file was canceled, but we must continue on";
            }
            }

            int i = 0;
            int i_x = img_w * img_h;
            while (i < i_x)
            {
                using (var g = Graphics.FromImage(img_final))
                {
                    byte[] cur_pixels = reader.ReadBytes(1);
                    string cur_pixel_s = BitConverter.ToString(cur_pixels);
                    string last = cur_pixel_s.Substring(0, (int)(cur_pixel_s.Length / 2));
                    string first = cur_pixel_s.Substring((int)(cur_pixel_s.Length / 2), (int)(cur_pixel_s.Length / 2));
                    int p_f = 0;
                    int p_l = 0;
                    switch (first)
                    {
                        case "0":
                            p_f = 0;
                            break;
                        case "1":
                            p_f = 1;
                            break;
                        case "2":
                            p_f = 2;
                            break;
                        case "3":
                            p_f = 3;
                            break;
                        case "4":
                            p_f = 4;
                            break;
                        case "5":
                            p_f = 5;
                            break;
                        case "6":
                            p_f = 6;
                            break;
                        case "7":
                            p_f = 7;
                            break;
                        case "8":
                            p_f = 8;
                            break;
                        case "9":
                            p_f = 9;
                            break;
                        case "A":
                            p_f = 10;
                            break;
                        case "B":
                            p_f = 11;
                            break;
                        case "C":
                            p_f = 12;
                            break;
                        case "D":
                            p_f = 13;
                            break;
                        case "E":
                            p_f = 14;
                            break;
                        case "F":
                            p_f = 15;
                            break;
                        default:
                            p_f = 0;
                            break;
                    }
                    temp_col_pal = col_pal_list[p_f];
                    img_final.SetPixel(current_pixel_w, current_pixel_h, Color.FromArgb((int)temp_col_pal[0], (int)temp_col_pal[1], (int)temp_col_pal[2], (int)temp_col_pal[3]));
                    current_pixel_w++;
                    i++;
                    switch (last)
                    {
                        case "0":
                            p_l = 0;
                            break;
                        case "1":
                            p_l = 1;
                            break;
                        case "2":
                            p_l = 2;
                            break;
                        case "3":
                            p_l = 3;
                            break;
                        case "4":
                            p_l = 4;
                            break;
                        case "5":
                            p_l = 5;
                            break;
                        case "6":
                            p_l = 6;
                            break;
                        case "7":
                            p_l = 7;
                            break;
                        case "8":
                            p_l = 8;
                            break;
                        case "9":
                            p_l = 9;
                            break;
                        case "A":
                            p_l = 10;
                            break;
                        case "B":
                            p_l = 11;
                            break;
                        case "C":
                            p_l = 12;
                            break;
                        case "D":
                            p_l = 13;
                            break;
                        case "E":
                            p_l = 14;
                            break;
                        case "F":
                            p_l = 15;
                            break;
                        default:
                            p_l = 0;
                            break;
                    }
                    temp2_col_pal = col_pal_list[p_l];
                    img_final.SetPixel(current_pixel_w, current_pixel_h, Color.FromArgb((int)temp2_col_pal[0], (int)temp_col_pal[1], (int)temp_col_pal[2], (int)temp_col_pal[3]));
                    current_pixel_w++;
                    i++;
                    if (current_pixel_w == img_w)
                    {
                        current_pixel_h++;
                        current_pixel_w = 0;
                    }
                }
            }
            sfd2.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd2.SupportMultiDottedExtensions = true;

            if (sfd2.ShowDialog() == DialogResult.OK)
            {
                img_final.Save(sfd2.FileName);
                TextConverted.Text = "Console Messages: File has been converted and saved";
            }

        }

        public void convert_file_with_colorpal_wswizzle()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] img_w_byte = reader.ReadBytes(2);
            byte[] img_h_byte = reader.ReadBytes(2);
            int img_w = BitConverter.ToInt16(img_w_byte, 0);
            int img_h = BitConverter.ToInt16(img_h_byte, 0);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int current_pixel_h = 0;
            int current_pixel_pal_w = 0;
            int current_pixel_w = 0;
            int p = 0;
            List<int[]> col_pal_list = new List<int[]>();
            int[] temp_col_pal = new int[4] { 128, 128, 128, 128 };
            int[] temp2_col_pal = new int[4] { 128, 128, 128, 128 };

            int pixel_amount = img_w * img_h / 2;
            long byte_total = new FileInfo(ofd.FileName).Length;
            int pixel_data_offset = unchecked((int)byte_total) - pixel_amount;
            int col_pal_length = (pixel_data_offset - 32) / 4;
            int col_pal_length_forwidth = col_pal_length * 10;
            var img_col_pal = new Bitmap(col_pal_length_forwidth, 10);
            var img_final = new Bitmap(img_w, img_h);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            while (p < col_pal_length)
            {
                byte r_v = reader.ReadBytes(1)[0];
                byte g_v = reader.ReadBytes(1)[0];
                byte b_v = reader.ReadBytes(1)[0];
                byte a_v = reader.ReadBytes(1)[0];
                if (AlphaOverride_CheckBox.Checked)
                {
                    a_v = 255;
                }
                Console.WriteLine(r_v);
                Console.WriteLine(g_v);
                Console.WriteLine(b_v);
                Console.WriteLine(a_v);
                using (var g = Graphics.FromImage(img_col_pal))
                {
                    int w = 0;
                    while (w < 10)
                    {
                        img_col_pal.SetPixel(current_pixel_pal_w, 0, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 1, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 2, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 3, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 4, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 5, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 6, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 7, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 8, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 9, Color.FromArgb(a_v, r_v, g_v, b_v));
                        current_pixel_pal_w++;
                        w++;
                    }
                    p++;
                    temp_col_pal = new int[4] { a_v, r_v, g_v, b_v };
                    col_pal_list.Add(temp_col_pal);
                }

            }
            if (ExportColorPal_CheckBox.Checked)
            {
                sfd.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                sfd.SupportMultiDottedExtensions = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    img_col_pal.Save(sfd.FileName);
                    TextConverted.Text = "Console Messages: File has been converted and saved";
                }
                else
                {
                    TextConverted.Text = "Console Messages: Saving file was canceled, process has been ended";
                }
            }

            int i = 0;
            int s_i = 0;
            int f_i = 0;
            int chunk_i = 0;
            int chunk_o = 0;
            int i_x = img_w * img_h;
            while (i < i_x)
            {
                using (var g = Graphics.FromImage(img_final))
                {
                    switch (f_i)
                    {
                        case 0:
                            if (s_i == 0)
                            {
                                if (chunk_i > 0)
                                {
                                    reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                    reader.BaseStream.Seek(chunk_o, SeekOrigin.Current);
                                }
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 1:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                reader.BaseStream.Seek(16 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 2:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                reader.BaseStream.Seek(32 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 3:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                reader.BaseStream.Seek(48 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 4:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                reader.BaseStream.Seek(64 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 5:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                reader.BaseStream.Seek(80 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 6:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                reader.BaseStream.Seek(96 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 7:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                reader.BaseStream.Seek(112 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        default:
                            Console.WriteLine("Shit, f_i missing statment for this case");
                            break;
                    }
                    byte[] cur_pixels = reader.ReadBytes(1);
                    string cur_pixel_s = BitConverter.ToString(cur_pixels);
                    string last = cur_pixel_s.Substring(0, (int)(cur_pixel_s.Length / 2));
                    string first = cur_pixel_s.Substring((int)(cur_pixel_s.Length / 2), (int)(cur_pixel_s.Length / 2));
                    int p_f = 0;
                    int p_l = 0;
                    switch (first)
                    {
                        case "0":
                            p_f = 0;
                            break;
                        case "1":
                            p_f = 1;
                            break;
                        case "2":
                            p_f = 2;
                            break;
                        case "3":
                            p_f = 3;
                            break;
                        case "4":
                            p_f = 4;
                            break;
                        case "5":
                            p_f = 5;
                            break;
                        case "6":
                            p_f = 6;
                            break;
                        case "7":
                            p_f = 7;
                            break;
                        case "8":
                            p_f = 8;
                            break;
                        case "9":
                            p_f = 9;
                            break;
                        case "A":
                            p_f = 10;
                            break;
                        case "B":
                            p_f = 11;
                            break;
                        case "C":
                            p_f = 12;
                            break;
                        case "D":
                            p_f = 13;
                            break;
                        case "E":
                            p_f = 14;
                            break;
                        case "F":
                            p_f = 15;
                            break;
                        default:
                            p_f = 0;
                            break;
                    }
                    temp_col_pal = col_pal_list[p_f];
                    img_final.SetPixel(current_pixel_w, current_pixel_h, Color.FromArgb((int)temp_col_pal[0], (int)temp_col_pal[1], (int)temp_col_pal[2], (int)temp_col_pal[3]));
                    current_pixel_w++;
                    i++;
                    switch (last)
                    {
                        case "0":
                            p_l = 0;
                            break;
                        case "1":
                            p_l = 1;
                            break;
                        case "2":
                            p_l = 2;
                            break;
                        case "3":
                            p_l = 3;
                            break;
                        case "4":
                            p_l = 4;
                            break;
                        case "5":
                            p_l = 5;
                            break;
                        case "6":
                            p_l = 6;
                            break;
                        case "7":
                            p_l = 7;
                            break;
                        case "8":
                            p_l = 8;
                            break;
                        case "9":
                            p_l = 9;
                            break;
                        case "A":
                            p_l = 10;
                            break;
                        case "B":
                            p_l = 11;
                            break;
                        case "C":
                            p_l = 12;
                            break;
                        case "D":
                            p_l = 13;
                            break;
                        case "E":
                            p_l = 14;
                            break;
                        case "F":
                            p_l = 15;
                            break;
                        default:
                            p_l = 0;
                            break;
                    }
                    temp2_col_pal = col_pal_list[p_l];
                    img_final.SetPixel(current_pixel_w, current_pixel_h, Color.FromArgb((int)temp2_col_pal[0], (int)temp2_col_pal[1], (int)temp2_col_pal[2], (int)temp2_col_pal[3]));
                    current_pixel_w++;
                    i++;
                    s_i++;
                    if (current_pixel_w == img_w)
                    {
                        current_pixel_h++;
                        current_pixel_w = 0;
                        if (f_i == 7)
                        {
                            if (s_i == 16)
                            {
                                chunk_i++;
                                chunk_o += img_w * 4;
                                f_i = 0;
                                s_i = 0;
                            }
                        }
                        else
                        {
                            f_i++;
                            s_i = 0;
                        }
                    }
                }
            }
            sfd2.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd2.SupportMultiDottedExtensions = true;

            if (sfd2.ShowDialog() == DialogResult.OK)
            {
                img_final.Save(sfd2.FileName);
                TextConverted.Text = "Console Messages: File has been converted and saved";
            }
            else
            {
                TextConverted.Text = "Console Messages: Saving file was canceled, process has been ended";
            }
        }

        public void convert_file_with_colorpal_256()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] img_w_byte = reader.ReadBytes(2);
            byte[] img_h_byte = reader.ReadBytes(2);
            int img_w = BitConverter.ToInt16(img_w_byte, 0);
            int img_h = BitConverter.ToInt16(img_h_byte, 0);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int current_pixel_h = 0;
            int current_pixel_pal_w = 0;
            int current_pixel_w = 0;
            int p = 0;
            List<int[]> col_pal_list = new List<int[]>();
            int[] temp_col_pal = new int[4] { 128, 128, 128, 128 };
            int[] temp2_col_pal = new int[4] { 128, 128, 128, 128 };

            int pixel_amount = img_w * img_h;
            long byte_total = new FileInfo(ofd.FileName).Length;
            int pixel_data_offset = unchecked((int)byte_total) - pixel_amount;
            int col_pal_length = (pixel_data_offset - 32) / 4;
            int col_pal_length_forwidth = col_pal_length * 10;
            var img_col_pal = new Bitmap(col_pal_length_forwidth, 10);
            var img_final = new Bitmap(img_w, img_h);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            while (p < col_pal_length)
            {
                byte r_v = reader.ReadBytes(1)[0];
                byte g_v = reader.ReadBytes(1)[0];
                byte b_v = reader.ReadBytes(1)[0];
                byte a_v = reader.ReadBytes(1)[0];
                if (AlphaOverride_CheckBox.Checked)
                {
                    a_v = 255;
                }
                Console.WriteLine(r_v);
                Console.WriteLine(g_v);
                Console.WriteLine(b_v);
                Console.WriteLine(a_v);
                using (var g = Graphics.FromImage(img_col_pal))
                {
                    int w = 0;
                    while (w < 10)
                    {
                        img_col_pal.SetPixel(current_pixel_pal_w, 0, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 1, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 2, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 3, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 4, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 5, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 6, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 7, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 8, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 9, Color.FromArgb(a_v, r_v, g_v, b_v));
                        current_pixel_pal_w++;
                        w++;
                    }
                    p++;
                    temp_col_pal = new int[4] { a_v, r_v, g_v, b_v };
                    col_pal_list.Add(temp_col_pal);
                }

            }
            if (ExportColorPal_CheckBox.Checked)
            {
            sfd.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd.SupportMultiDottedExtensions = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                img_col_pal.Save(sfd.FileName);
                TextConverted.Text = "Console Messages: File has been converted and saved";
            }
            else
                {
                    TextConverted.Text = "Console Messages: Saving color pal file was canceled, but we must continue on";
                }
            }

            int i = 0;
            int i_x = img_w * img_h;
            while (i < i_x)
            {
                using (var g = Graphics.FromImage(img_final))
                {
                    byte cur_pixels = reader.ReadBytes(1)[0];
                    int p_f = (int)cur_pixels;
                    temp_col_pal = col_pal_list[p_f];
                    img_final.SetPixel(current_pixel_w, current_pixel_h, Color.FromArgb((int)temp_col_pal[0], (int)temp_col_pal[1], (int)temp_col_pal[2], (int)temp_col_pal[3]));
                    current_pixel_w++;
                    i++;
                    if (current_pixel_w == img_w)
                    {
                        current_pixel_h++;
                        current_pixel_w = 0;
                    }
                }
            }
            sfd2.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd2.SupportMultiDottedExtensions = true;

            if (sfd2.ShowDialog() == DialogResult.OK)
            {
                img_final.Save(sfd2.FileName);
                TextConverted.Text = "Console Messages: File has been converted and saved";
            }
            else
            {
                TextConverted.Text = "Console Messages: Saving file was canceled, process has been ended";
            }
        }

        public void convert_file_with_colorpal_256Ver2()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] img_w_byte = reader.ReadBytes(2);
            byte[] img_h_byte = reader.ReadBytes(2);
            int img_w = BitConverter.ToInt16(img_w_byte, 0);
            int img_h = BitConverter.ToInt16(img_h_byte, 0);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int current_pixel_h = 0;
            int current_pixel_pal_w = 0;
            int current_pixel_w = 0;
            int p = 0;
            List<int[]> col_pal_list = new List<int[]>();
            int[] temp_col_pal = new int[4] { 128, 128, 128, 128 };
            int[] temp2_col_pal = new int[4] { 128, 128, 128, 128 };

            int pixel_amount = img_w * img_h;
            long byte_total = new FileInfo(ofd.FileName).Length;
            int pixel_data_offset = unchecked((int)byte_total) - pixel_amount;
            int col_pal_length = (pixel_data_offset - 32) / 2;
            int col_pal_length_forwidth = col_pal_length * 10;
            var img_col_pal = new Bitmap(col_pal_length_forwidth, 10);
            var img_final = new Bitmap(img_w, img_h);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            while (p < col_pal_length)
            {
                byte[] byte_val1 = reader.ReadBytes(1);
                string byte_val1_s = BitConverter.ToString(byte_val1);
                string first = byte_val1_s.Substring(0, (int)(byte_val1_s.Length / 2));
                string last = byte_val1_s.Substring((int)(byte_val1_s.Length / 2), (int)(byte_val1_s.Length / 2));
                int r_v = 0;
                int r_v2 = 0;
                int g_v = 0;
                switch (first)
                {
                    case "0":
                        r_v = 0;
                        g_v = 0;
                        break;
                    case "1":
                        r_v = 133;
                        g_v = 0;
                        break;
                    case "2":
                        r_v = 0;
                        g_v = 8;
                        break;
                    case "3":
                        r_v = 133;
                        g_v = 8;
                        break;
                    case "4":
                        r_v = 0;
                        g_v = 16;
                        break;
                    case "5":
                        r_v = 133;
                        g_v = 16;
                        break;
                    case "6":
                        r_v = 0;
                        g_v = 24;
                        break;
                    case "7":
                        r_v = 133;
                        g_v = 24;
                        break;
                    case "8":
                        r_v = 0;
                        g_v = 33;
                        break;
                    case "9":
                        r_v = 133;
                        g_v = 33;
                        break;
                    case "A":
                        r_v = 0;
                        g_v = 41;
                        break;
                    case "B":
                        r_v = 133;
                        g_v = 41;
                        break;
                    case "C":
                        r_v = 0;
                        g_v = 49;
                        break;
                    case "D":
                        r_v = 133;
                        g_v = 49;
                        break;
                    case "E":
                        r_v = 0;
                        g_v = 57;
                        break;
                    case "F":
                        r_v = 133;
                        g_v = 57;
                        break;
                    default:
                        r_v = 0;
                        g_v = 0;
                        break;
                }
                switch (last)
                {
                    case "0":
                        r_v2 = 0;
                        break;
                    case "1":
                        r_v2 = 8;
                        break;
                    case "2":
                        r_v2 = 16;
                        break;
                    case "3":
                        r_v2 = 24;
                        break;
                    case "4":
                        r_v2 = 33;
                        break;
                    case "5":
                        r_v2 = 41;
                        break;
                    case "6":
                        r_v2 = 49;
                        break;
                    case "7":
                        r_v2 = 57;
                        break;
                    case "8":
                        r_v2 = 66;
                        break;
                    case "9":
                        r_v2 = 75;
                        break;
                    case "A":
                        r_v2 = 83;
                        break;
                    case "B":
                        r_v2 = 91;
                        break;
                    case "C":
                        r_v2 = 100;
                        break;
                    case "D":
                        r_v2 = 108;
                        break;
                    case "E":
                        r_v2 = 116;
                        break;
                    case "F":
                        r_v2 = 124;
                        break;
                    default:
                        r_v2 = 0;
                        break;
                }

                byte[] byte_val2 = reader.ReadBytes(1);
                string byte_val2_s = BitConverter.ToString(byte_val2);
                string first2 = byte_val2_s.Substring(0, (int)(byte_val2_s.Length / 2));
                string last2 = byte_val2_s.Substring((int)(byte_val2_s.Length / 2), (int)(byte_val2_s.Length / 2));
                int a_v = 0;
                int b_v = 0;
                int b_v2 = 0;
                int g_v2 = 0;
                switch (first2)
                {
                    case "0":
                        a_v = 0;
                        b_v = 0;
                        break;
                    case "1":
                        a_v = 0;
                        b_v = 33;
                        break;
                    case "2":
                        a_v = 0;
                        b_v = 66;
                        break;
                    case "3":
                        a_v = 0;
                        b_v = 100;
                        break;
                    case "4":
                        a_v = 0;
                        b_v = 133;
                        break;
                    case "5":
                        a_v = 0;
                        b_v = 166;
                        break;
                    case "6":
                        a_v = 0;
                        b_v = 200;
                        break;
                    case "7":
                        a_v = 0;
                        b_v = 233;
                        break;
                    case "8":
                        a_v = 255;
                        b_v = 0;
                        break;
                    case "9":
                        a_v = 255;
                        b_v = 33;
                        break;
                    case "A":
                        a_v = 255;
                        b_v = 66;
                        break;
                    case "B":
                        a_v = 255;
                        b_v = 100;
                        break;
                    case "C":
                        a_v = 255;
                        b_v = 133;
                        break;
                    case "D":
                        a_v = 255;
                        b_v = 166;
                        break;
                    case "E":
                        a_v = 255;
                        b_v = 200;
                        break;
                    case "F":
                        a_v = 255;
                        b_v = 233;
                        break;
                    default:
                        a_v = 0;
                        b_v = 0;
                        break;
                }
                switch (last2)
                {
                    case "0":
                        b_v2 = 0;
                        g_v2 = 0;
                        break;
                    case "1":
                        b_v2 = 0;
                        g_v2 = 66;
                        break;
                    case "2":
                        b_v2 = 0;
                        g_v2 = 133;
                        break;
                    case "3":
                        b_v2 = 0;
                        g_v2 = 200;
                        break;
                    case "4":
                        b_v2 = 8;
                        g_v2 = 0;
                        break;
                    case "5":
                        b_v2 = 8;
                        g_v2 = 66;
                        break;
                    case "6":
                        b_v2 = 8;
                        g_v2 = 133;
                        break;
                    case "7":
                        b_v2 = 8;
                        g_v2 = 200;
                        break;
                    case "8":
                        b_v2 = 16;
                        g_v2 = 0;
                        break;
                    case "9":
                        b_v2 = 16;
                        g_v2 = 66;
                        break;
                    case "A":
                        b_v2 = 16;
                        g_v2 = 133;
                        break;
                    case "B":
                        b_v2 = 16;
                        g_v2 = 200;
                        break;
                    case "C":
                        b_v2 = 24;
                        g_v2 = 0;
                        break;
                    case "D":
                        b_v2 = 24;
                        g_v2 = 66;
                        break;
                    case "E":
                        b_v2 = 24;
                        g_v2 = 133;
                        break;
                    case "F":
                        b_v2 = 24;
                        g_v2 = 200;
                        break;
                    default:
                        b_v2 = 0;
                        g_v2 = 0;
                        break;
                }
                if (AlphaOverride_CheckBox.Checked)
                {
                    a_v = 255;
                }
                int r_v_final = r_v + r_v2;
                if (r_v_final > 255)
                {
                    r_v_final = 255;
                }
                int g_v_final = g_v + g_v2;
                if (g_v_final > 255)
                {
                    g_v_final = 255;
                }
                int b_v_final = b_v + b_v2;
                if (b_v_final > 255)
                {
                    b_v_final = 255;
                }
                Console.WriteLine(r_v_final);
                Console.WriteLine(g_v_final);
                Console.WriteLine(b_v_final);
                Console.WriteLine(a_v);
                using (var g = Graphics.FromImage(img_col_pal))
                {
                    int w = 0;
                    while (w < 10)
                    {
                        img_col_pal.SetPixel(current_pixel_pal_w, 0, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 1, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 2, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 3, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 4, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 5, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 6, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 7, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 8, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 9, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        current_pixel_pal_w++;
                        w++;
                    }
                    p++;
                    temp_col_pal = new int[4] { a_v, r_v_final, g_v_final, b_v_final };
                    col_pal_list.Add(temp_col_pal);
                }

            }
            if (ExportColorPal_CheckBox.Checked)
            {
                sfd.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                sfd.SupportMultiDottedExtensions = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    img_col_pal.Save(sfd.FileName);
                    TextConverted.Text = "Console Messages: File has been converted and saved";
                }
                else
                {
                    TextConverted.Text = "Console Messages: Saving color pal file was canceled, but we must continue on";
                }
            }

            int i = 0;
            int i_x = img_w * img_h;
            while (i < i_x)
            {
                using (var g = Graphics.FromImage(img_final))
                {
                    byte cur_pixels = reader.ReadBytes(1)[0];
                    int p_f = (int)cur_pixels;
                    temp_col_pal = col_pal_list[p_f];
                    img_final.SetPixel(current_pixel_w, current_pixel_h, Color.FromArgb((int)temp_col_pal[0], (int)temp_col_pal[1], (int)temp_col_pal[2], (int)temp_col_pal[3]));
                    current_pixel_w++;
                    i++;
                    if (current_pixel_w == img_w)
                    {
                        current_pixel_h++;
                        current_pixel_w = 0;
                    }
                }
            }
            sfd2.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd2.SupportMultiDottedExtensions = true;

            if (sfd2.ShowDialog() == DialogResult.OK)
            {
                img_final.Save(sfd2.FileName);
                TextConverted.Text = "Console Messages: File has been converted and saved";
            }
            else
            {
                TextConverted.Text = "Console Messages: Saving file was canceled, process has been ended";
            }
        }

        public void convert_file_with_colorpal_256_wswizzle()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] img_w_byte = reader.ReadBytes(2);
            byte[] img_h_byte = reader.ReadBytes(2);
            int img_w = BitConverter.ToInt16(img_w_byte, 0);
            int img_h = BitConverter.ToInt16(img_h_byte, 0);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int current_pixel_h = 0;
            int current_pixel_pal_w = 0;
            int current_pixel_w = 0;
            int p = 0;
            List<int[]> col_pal_list = new List<int[]>();
            int[] temp_col_pal = new int[4] { 128, 128, 128, 128 };
            int[] temp2_col_pal = new int[4] { 128, 128, 128, 128 };

            int pixel_amount = img_w * img_h;
            long byte_total = new FileInfo(ofd.FileName).Length;
            int pixel_data_offset = unchecked((int)byte_total) - pixel_amount;
            int col_pal_length = (pixel_data_offset - 32) / 4;
            int col_pal_length_forwidth = col_pal_length * 10;
            var img_col_pal = new Bitmap(col_pal_length_forwidth, 10);
            var img_final = new Bitmap(img_w, img_h);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            while (p < col_pal_length)
            {
                byte r_v = reader.ReadBytes(1)[0];
                byte g_v = reader.ReadBytes(1)[0];
                byte b_v = reader.ReadBytes(1)[0];
                byte a_v = reader.ReadBytes(1)[0];
                if (AlphaOverride_CheckBox.Checked)
                {
                    a_v = 255;
                }
                Console.WriteLine(r_v);
                Console.WriteLine(g_v);
                Console.WriteLine(b_v);
                Console.WriteLine(a_v);
                using (var g = Graphics.FromImage(img_col_pal))
                {
                    int w = 0;
                    while (w < 10)
                    {
                        img_col_pal.SetPixel(current_pixel_pal_w, 0, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 1, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 2, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 3, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 4, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 5, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 6, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 7, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 8, Color.FromArgb(a_v, r_v, g_v, b_v));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 9, Color.FromArgb(a_v, r_v, g_v, b_v));
                        current_pixel_pal_w++;
                        w++;
                    }
                    p++;
                    temp_col_pal = new int[4] { a_v, r_v, g_v, b_v };
                    col_pal_list.Add(temp_col_pal);
                }

            }
            if (ExportColorPal_CheckBox.Checked)
            {
                sfd.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                sfd.SupportMultiDottedExtensions = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    img_col_pal.Save(sfd.FileName);
                    TextConverted.Text = "Console Messages: File has been converted and saved";
                }
                else
                {
                    TextConverted.Text = "Console Messages: Saving color pal file was canceled, but we must continue on";
                }
            }

            int i = 0;
            int s_i = 0;
            int f_i = 0;
            int chunk_i = 0;
            int chunk_o = 0;
            int i_x = img_w * img_h;
            while (i < i_x)
            {
                using (var g = Graphics.FromImage(img_final))
                {
                    switch (f_i)
                    {
                        case 0:
                            if (s_i == 0)
                            {
                                if (chunk_i > 0)
                                {
                                    reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                    reader.BaseStream.Seek(chunk_o, SeekOrigin.Current);
                                }
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 1:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                reader.BaseStream.Seek(16 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 2:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                reader.BaseStream.Seek(32 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 3:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                reader.BaseStream.Seek(48 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 4:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                reader.BaseStream.Seek(64 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 5:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                reader.BaseStream.Seek(80 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 6:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                reader.BaseStream.Seek(96 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 7:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                reader.BaseStream.Seek(112 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        default:
                            Console.WriteLine("Shit, f_i missing statment for this case");
                            break;
                    }
                    byte cur_pixels = reader.ReadBytes(1)[0];
                    int p_f = (int)cur_pixels;
                    temp_col_pal = col_pal_list[p_f];
                    img_final.SetPixel(current_pixel_w, current_pixel_h, Color.FromArgb((int)temp_col_pal[0], (int)temp_col_pal[1], (int)temp_col_pal[2], (int)temp_col_pal[3]));
                    current_pixel_w++;
                    i++;
                    s_i++;
                    if (current_pixel_w == img_w)
                    {
                        current_pixel_h++;
                        current_pixel_w = 0;
                        if (f_i == 7)
                        {
                            if (s_i == 16)
                            {
                                chunk_i++;
                                chunk_o += img_w * 8;
                                f_i = 0;
                                s_i = 0;
                            }
                        }
                        else
                        {
                            f_i++;
                            s_i = 0;
                        }
                    }
                }
            }
            sfd2.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd2.SupportMultiDottedExtensions = true;

            if (sfd2.ShowDialog() == DialogResult.OK)
            {
                img_final.Save(sfd2.FileName);
                TextConverted.Text = "Console Messages: File has been converted and saved";
            }
            else
            {
                TextConverted.Text = "Console Messages: Saving file was canceled, process has been ended";
            }
        }

        public void convert_file_with_colorpal_256Ver2_wswizzle()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] img_w_byte = reader.ReadBytes(2);
            byte[] img_h_byte = reader.ReadBytes(2);
            int img_w = BitConverter.ToInt16(img_w_byte, 0);
            int img_h = BitConverter.ToInt16(img_h_byte, 0);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int current_pixel_h = 0;
            int current_pixel_pal_w = 0;
            int current_pixel_w = 0;
            int p = 0;
            List<int[]> col_pal_list = new List<int[]>();
            int[] temp_col_pal = new int[4] { 128, 128, 128, 128 };
            int[] temp2_col_pal = new int[4] { 128, 128, 128, 128 };

            int pixel_amount = img_w * img_h;
            long byte_total = new FileInfo(ofd.FileName).Length;
            int pixel_data_offset = unchecked((int)byte_total) - pixel_amount;
            int col_pal_length = (pixel_data_offset - 32) / 2;
            int col_pal_length_forwidth = col_pal_length * 10;
            var img_col_pal = new Bitmap(col_pal_length_forwidth, 10);
            var img_final = new Bitmap(img_w, img_h);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            while (p < col_pal_length)
            {
                byte[] byte_val1 = reader.ReadBytes(1);
                string byte_val1_s = BitConverter.ToString(byte_val1);
                string first = byte_val1_s.Substring(0, (int)(byte_val1_s.Length / 2));
                string last = byte_val1_s.Substring((int)(byte_val1_s.Length / 2), (int)(byte_val1_s.Length / 2));
                int r_v = 0;
                int r_v2 = 0;
                int g_v = 0;
                switch (first)
                {
                    case "0":
                        r_v = 0;
                        g_v = 0;
                        break;
                    case "1":
                        r_v = 133;
                        g_v = 0;
                        break;
                    case "2":
                        r_v = 0;
                        g_v = 8;
                        break;
                    case "3":
                        r_v = 133;
                        g_v = 8;
                        break;
                    case "4":
                        r_v = 0;
                        g_v = 16;
                        break;
                    case "5":
                        r_v = 133;
                        g_v = 16;
                        break;
                    case "6":
                        r_v = 0;
                        g_v = 24;
                        break;
                    case "7":
                        r_v = 133;
                        g_v = 24;
                        break;
                    case "8":
                        r_v = 0;
                        g_v = 33;
                        break;
                    case "9":
                        r_v = 133;
                        g_v = 33;
                        break;
                    case "A":
                        r_v = 0;
                        g_v = 41;
                        break;
                    case "B":
                        r_v = 133;
                        g_v = 41;
                        break;
                    case "C":
                        r_v = 0;
                        g_v = 49;
                        break;
                    case "D":
                        r_v = 133;
                        g_v = 49;
                        break;
                    case "E":
                        r_v = 0;
                        g_v = 57;
                        break;
                    case "F":
                        r_v = 133;
                        g_v = 57;
                        break;
                    default:
                        r_v = 0;
                        g_v = 0;
                        break;
                }
                switch (last)
                {
                    case "0":
                        r_v2 = 0;
                        break;
                    case "1":
                        r_v2 = 8;
                        break;
                    case "2":
                        r_v2 = 16;
                        break;
                    case "3":
                        r_v2 = 24;
                        break;
                    case "4":
                        r_v2 = 33;
                        break;
                    case "5":
                        r_v2 = 41;
                        break;
                    case "6":
                        r_v2 = 49;
                        break;
                    case "7":
                        r_v2 = 57;
                        break;
                    case "8":
                        r_v2 = 66;
                        break;
                    case "9":
                        r_v2 = 75;
                        break;
                    case "A":
                        r_v2 = 83;
                        break;
                    case "B":
                        r_v2 = 91;
                        break;
                    case "C":
                        r_v2 = 100;
                        break;
                    case "D":
                        r_v2 = 108;
                        break;
                    case "E":
                        r_v2 = 116;
                        break;
                    case "F":
                        r_v2 = 124;
                        break;
                    default:
                        r_v2 = 0;
                        break;
                }

                byte[] byte_val2 = reader.ReadBytes(1);
                string byte_val2_s = BitConverter.ToString(byte_val2);
                string first2 = byte_val2_s.Substring(0, (int)(byte_val2_s.Length / 2));
                string last2 = byte_val2_s.Substring((int)(byte_val2_s.Length / 2), (int)(byte_val2_s.Length / 2));
                int a_v = 0;
                int b_v = 0;
                int b_v2 = 0;
                int g_v2 = 0;
                switch (first2)
                {
                    case "0":
                        a_v = 0;
                        b_v = 0;
                        break;
                    case "1":
                        a_v = 0;
                        b_v = 33;
                        break;
                    case "2":
                        a_v = 0;
                        b_v = 66;
                        break;
                    case "3":
                        a_v = 0;
                        b_v = 100;
                        break;
                    case "4":
                        a_v = 0;
                        b_v = 133;
                        break;
                    case "5":
                        a_v = 0;
                        b_v = 166;
                        break;
                    case "6":
                        a_v = 0;
                        b_v = 200;
                        break;
                    case "7":
                        a_v = 0;
                        b_v = 233;
                        break;
                    case "8":
                        a_v = 255;
                        b_v = 0;
                        break;
                    case "9":
                        a_v = 255;
                        b_v = 33;
                        break;
                    case "A":
                        a_v = 255;
                        b_v = 66;
                        break;
                    case "B":
                        a_v = 255;
                        b_v = 100;
                        break;
                    case "C":
                        a_v = 255;
                        b_v = 133;
                        break;
                    case "D":
                        a_v = 255;
                        b_v = 166;
                        break;
                    case "E":
                        a_v = 255;
                        b_v = 200;
                        break;
                    case "F":
                        a_v = 255;
                        b_v = 233;
                        break;
                    default:
                        a_v = 0;
                        b_v = 0;
                        break;
                }
                switch (last2)
                {
                    case "0":
                        b_v2 = 0;
                        g_v2 = 0;
                        break;
                    case "1":
                        b_v2 = 0;
                        g_v2 = 66;
                        break;
                    case "2":
                        b_v2 = 0;
                        g_v2 = 133;
                        break;
                    case "3":
                        b_v2 = 0;
                        g_v2 = 200;
                        break;
                    case "4":
                        b_v2 = 8;
                        g_v2 = 0;
                        break;
                    case "5":
                        b_v2 = 8;
                        g_v2 = 66;
                        break;
                    case "6":
                        b_v2 = 8;
                        g_v2 = 133;
                        break;
                    case "7":
                        b_v2 = 8;
                        g_v2 = 200;
                        break;
                    case "8":
                        b_v2 = 16;
                        g_v2 = 0;
                        break;
                    case "9":
                        b_v2 = 16;
                        g_v2 = 66;
                        break;
                    case "A":
                        b_v2 = 16;
                        g_v2 = 133;
                        break;
                    case "B":
                        b_v2 = 16;
                        g_v2 = 200;
                        break;
                    case "C":
                        b_v2 = 24;
                        g_v2 = 0;
                        break;
                    case "D":
                        b_v2 = 24;
                        g_v2 = 66;
                        break;
                    case "E":
                        b_v2 = 24;
                        g_v2 = 133;
                        break;
                    case "F":
                        b_v2 = 24;
                        g_v2 = 200;
                        break;
                    default:
                        b_v2 = 0;
                        g_v2 = 0;
                        break;
                }
                if (AlphaOverride_CheckBox.Checked)
                {
                    a_v = 255;
                }
                int r_v_final = r_v + r_v2;
                if (r_v_final > 255)
                {
                    r_v_final = 255;
                }
                int g_v_final = g_v + g_v2;
                if (g_v_final > 255)
                {
                    g_v_final = 255;
                }
                int b_v_final = b_v + b_v2;
                if (b_v_final > 255)
                {
                    b_v_final = 255;
                }
                Console.WriteLine(r_v_final);
                Console.WriteLine(g_v_final);
                Console.WriteLine(b_v_final);
                Console.WriteLine(a_v);
                using (var g = Graphics.FromImage(img_col_pal))
                {
                    int w = 0;
                    while (w < 10)
                    {
                        img_col_pal.SetPixel(current_pixel_pal_w, 0, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 1, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 2, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 3, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 4, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 5, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 6, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 7, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 8, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        img_col_pal.SetPixel(current_pixel_pal_w, 0 + 9, Color.FromArgb(a_v, r_v_final, g_v_final, b_v_final));
                        current_pixel_pal_w++;
                        w++;
                    }
                    p++;
                    temp_col_pal = new int[4] { a_v, r_v_final, g_v_final, b_v_final };
                    col_pal_list.Add(temp_col_pal);
                }

            }
            if (ExportColorPal_CheckBox.Checked)
            {
                sfd.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                sfd.SupportMultiDottedExtensions = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    img_col_pal.Save(sfd.FileName);
                    TextConverted.Text = "Console Messages: File has been converted and saved";
                }
                else
                {
                    TextConverted.Text = "Console Messages: Saving color pal file was canceled, but we must continue on";
                }
            }

            int i = 0;
            int s_i = 0;
            int f_i = 0;
            int chunk_i = 0;
            int chunk_o = 0;
            int i_x = img_w * img_h;
            while (i < i_x)
            {
                using (var g = Graphics.FromImage(img_final))
                {
                    switch (f_i)
                    {
                        case 0:
                            if (s_i == 0)
                            {
                                if (chunk_i > 0)
                                {
                                    reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                    reader.BaseStream.Seek(chunk_o, SeekOrigin.Current);
                                }
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 1:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                reader.BaseStream.Seek(16 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 2:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                reader.BaseStream.Seek(32 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 3:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                reader.BaseStream.Seek(48 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 4:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                reader.BaseStream.Seek(64 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 5:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                reader.BaseStream.Seek(80 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 6:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                reader.BaseStream.Seek(96 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        case 7:
                            if (s_i == 0)
                            {
                                reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                reader.BaseStream.Seek(112 + chunk_o, SeekOrigin.Current);
                            }
                            else if (s_i == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                s_i = 0;
                            }
                            break;
                        default:
                            Console.WriteLine("Shit, f_i missing statment for this case");
                            break;
                    }
                    byte cur_pixels = reader.ReadBytes(1)[0];
                    int p_f = (int)cur_pixels;
                    temp_col_pal = col_pal_list[p_f];
                    img_final.SetPixel(current_pixel_w, current_pixel_h, Color.FromArgb((int)temp_col_pal[0], (int)temp_col_pal[1], (int)temp_col_pal[2], (int)temp_col_pal[3]));
                    current_pixel_w++;
                    i++;
                    s_i++;
                    if (current_pixel_w == img_w)
                    {
                        current_pixel_h++;
                        current_pixel_w = 0;
                        if (f_i == 7)
                        {
                            if (s_i == 16)
                            {
                                chunk_i++;
                                chunk_o += img_w * 8;
                                f_i = 0;
                                s_i = 0;
                            }
                        }
                        else
                        {
                            f_i++;
                            s_i = 0;
                        }
                    }
                }
            }
            sfd2.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd2.SupportMultiDottedExtensions = true;

            if (sfd2.ShowDialog() == DialogResult.OK)
            {
                img_final.Save(sfd2.FileName);
                TextConverted.Text = "Console Messages: File has been converted and saved";
            }
            else
            {
                TextConverted.Text = "Console Messages: Saving file was canceled, process has been ended";
            }
        }

        public void convert_file_no_colorpal()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] img_w_byte = reader.ReadBytes(2);
            byte[] img_h_byte = reader.ReadBytes(2);
            int img_w = BitConverter.ToInt16(img_w_byte, 0);
            int img_h = BitConverter.ToInt16(img_h_byte, 0);
            var img_final2 = new Bitmap(img_w, img_h);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int current_pixel_h = 0;
            int current_pixel_w = 0;
                Console.WriteLine("File Isn't 8 Bit");
                reader.BaseStream.Seek(32, SeekOrigin.Begin);
                int i = 0;
                int i_x = img_w * img_h;
            while (i < i_x)
            {
                    byte r_v = reader.ReadBytes(1)[0];
                    byte g_v = reader.ReadBytes(1)[0];
                    byte b_v = reader.ReadBytes(1)[0];
                    byte a_v = reader.ReadBytes(1)[0];
                    if (AlphaOverride_CheckBox.Checked)
                    {
                        a_v = 255;
                    }
                    img_final2.SetPixel(current_pixel_w, current_pixel_h, Color.FromArgb(a_v, r_v, g_v, b_v));
                    current_pixel_w++;
                    i++;
                    Console.WriteLine(i);
                    if (current_pixel_w == img_w)
                    {
                        current_pixel_h++;
                        current_pixel_w = 0;
                    }
                }
                sfd2.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                sfd2.SupportMultiDottedExtensions = true;

                if (sfd2.ShowDialog() == DialogResult.OK)
                {
                    img_final2.Save(sfd2.FileName);
                    TextConverted.Text = "Console Messages: File has been converted and saved";
                }
                else
            {
                TextConverted.Text = "Console Messages: Saving file was canceled, process has been ended";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ofd2.Filter = "All files (*.*)|*.*";
            ofd2.SupportMultiDottedExtensions = true;
            if (ofd2.ShowDialog() == DialogResult.OK)
            {
                FileLoaded2 = 1;
                TextFileNameDir.Text = "Opened File Directory: " + ofd2.FileName;
                TextFileName.Text = "Opened File Name: " + ofd2.SafeFileName;
                TextConverted.Text = "Console Messages: N/A";
            }
            else
            {
                FileLoaded2 = 0;
                TextFileNameDir.Text = "Opened File Directory: N/A";
                TextFileName.Text = "Opened File Name: N/A";
                TextConverted.Text = "Console Messages: No File Was Selected";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (FileLoaded2 == 1)
            {
                TextConverted.Text = "Console Messages: Converting... Please wait...";
                sfd3.Filter = "All files (*.*)|*.*";
                sfd3.SupportMultiDottedExtensions = true;

                if (sfd3.ShowDialog() == DialogResult.OK)
                {
                    using (FileStream fileStream = new FileStream(sfd3.FileName, FileMode.Create))
                    {
                        if (UseColorPal_CheckBox.Checked)
                        {
                            HashSet<Color> colors = new HashSet<Color>();
                            List<int[]> col_pal_list = new List<int[]>();
                            int[] temp_col_pal = new int[4] { 128, 128, 128, 128 };
                            int[] temp_col_pal2 = new int[4] { 128, 128, 128, 128 };
                            var myBitmap = new Bitmap(ofd2.FileName);
                            int i = 0;
                            int i_x = myBitmap.Width * myBitmap.Height;
                            int current_pixel_w = 0;
                            int current_pixel_h = 0;
                            byte[] array = { 0x02, 0x00, 0x00, 0x00, 0x84, 0x02, 0x84, 0x02, 0x06, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                            byte[] array2 = BitConverter.GetBytes(myBitmap.Width);
                            byte[] array3 = BitConverter.GetBytes(myBitmap.Height);
                            fileStream.Write(array, 0, 0x1C);
                            fileStream.Write(array2, 0, 0x02);
                            fileStream.Write(array3, 0, 0x02);
                            int img_w = myBitmap.Width;
                                while (i < i_x)
                                {
                                    Color pixelColor = myBitmap.GetPixel(current_pixel_w, current_pixel_h);
                                    Console.WriteLine(pixelColor);
                                    byte r_v = pixelColor.R;
                                    byte g_v = pixelColor.G;
                                    byte b_v = pixelColor.B;
                                    byte a_v = pixelColor.A;
                                    temp_col_pal = new int[4] { a_v, r_v, g_v, b_v };
                                    if (colors.Contains(pixelColor))
                                    {
                                        Console.WriteLine("RGBA value already exists in list, skipping");
                                    }
                                    else
                                    {
                                        colors.Add(pixelColor);
                                        col_pal_list.Add(temp_col_pal);
                                        byte[] bytes = new byte[4];
                                        bytes[0] = pixelColor.R;
                                        bytes[1] = pixelColor.G;
                                        bytes[2] = pixelColor.B;
                                        bytes[3] = pixelColor.A;
                                        fileStream.Write(bytes, 0, 0x04);
                                    }
                                    current_pixel_w++;
                                    i++;
                                    if (current_pixel_w == img_w)
                                    {
                                        current_pixel_h++;
                                        current_pixel_w = 0;
                                    }
                                }
                                int col_pal_list_length = col_pal_list.Count;
                                while (col_pal_list_length < 256)
                                {
                                    byte[] bytes = new byte[4];
                                    bytes[0] = 0x00;
                                    bytes[1] = 0x00;
                                    bytes[2] = 0x00;
                                    bytes[3] = 0xFF;
                                    fileStream.Write(bytes, 0, 0x04);
                                    temp_col_pal = new int[4] { 255, 0, 0, 0 };
                                    col_pal_list.Add(temp_col_pal);
                                    col_pal_list_length = col_pal_list.Count;
                                }
                                if (col_pal_list_length > 256)
                                {
                                    TextConverted.Text = "Console Messages: .png has over 256 colors and cannot be converted";
                                }
                                else
                                {
                                    i = 0;
                                    current_pixel_h = 0;
                                    current_pixel_w = 0;
                                    while (i < i_x)
                                    {
                                        Color pixelColor = myBitmap.GetPixel(current_pixel_w, current_pixel_h);
                                        byte r_v = pixelColor.R;
                                        byte g_v = pixelColor.G;
                                        byte b_v = pixelColor.B;
                                        byte a_v = pixelColor.A;
                                        temp_col_pal = new int[4] { a_v, r_v, g_v, b_v };
                                        int col_pal_index = col_pal_list.FindIndex(l => Enumerable.SequenceEqual(temp_col_pal, l));
                                        Console.WriteLine(col_pal_list);
                                        Console.WriteLine(col_pal_index);
                                        fileStream.Write(BitConverter.GetBytes(col_pal_index), 0, 0x01);
                                        current_pixel_w++;
                                        i++;
                                        if (current_pixel_w == img_w)
                                        {
                                            current_pixel_h++;
                                            current_pixel_w = 0;
                                        }
                                    }
                                    TextConverted.Text = "Console Messages: File has been converted and saved";
                                }
                        }
                        else
                        {
                            var myBitmap = new Bitmap(ofd2.FileName);
                            int i = 0;
                            int i_x = myBitmap.Width * myBitmap.Height;
                            int current_pixel_w = 0;
                            int current_pixel_h = 0;
                            int img_w = myBitmap.Width;
                            byte[] array = { 0x02, 0x00, 0x00, 0x00, 0x84, 0x02, 0x84, 0x02, 0x07, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                            byte[] array2 = BitConverter.GetBytes(myBitmap.Width);
                            byte[] array3 = BitConverter.GetBytes(myBitmap.Height);
                            fileStream.Write(array, 0, 0x1C);
                            fileStream.Write(array2, 0, 0x02);
                            fileStream.Write(array3, 0, 0x02);
                            while (i < i_x)
                            {
                                Color pixelColor = myBitmap.GetPixel(current_pixel_w, current_pixel_h);
                                Console.WriteLine(pixelColor);
                                byte[] bytes = new byte[4];
                                bytes[0] = pixelColor.R;
                                bytes[1] = pixelColor.G;
                                bytes[2] = pixelColor.B;
                                bytes[3] = pixelColor.A;
                                fileStream.Write(bytes, 0, 0x04);
                                i++;
                                current_pixel_w++;
                                if (current_pixel_w == img_w)
                                {
                                    current_pixel_h++;
                                    current_pixel_w = 0;
                                }
                            }
                            TextConverted.Text = "Console Messages: File has been converted and saved";
                        }
                        fileStream.Close();
                    }
                }
                else
                {
                    TextConverted.Text = "Console Messages: Saving file was canceled, process has been ended";
                }
            }
            else
            {
                TextConverted.Text = "Console Messages: There is no .png file loaded";
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
