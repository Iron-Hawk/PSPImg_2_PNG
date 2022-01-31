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
        public int fileLoadedIMG = 0;
        public int fileLoadedPNG = 0;
        public int hasColorPalData = 0;
        public int colorPalDataOver16 = 0;
        public int colorPalDataOver128 = 0;
        public int isSwizzled = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void openIMGFile(object sender, EventArgs e)
        {
            ofd.Filter = "All files (*.*)|*.*";
            ofd.SupportMultiDottedExtensions = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                fileLoadedIMG = 1;
                openedFileDirText.Text = "Opened File Directory: " + ofd.FileName;
                openedFileNameText.Text = "Opened File Name: " + ofd.SafeFileName;
                consoleText.Text = "Console Messages: N/A";
            }
            else
            {
                fileLoadedIMG = 0;
                openedFileDirText.Text = "Opened File Directory: N/A";
                openedFileNameText.Text = "Opened File Name: N/A";
                consoleText.Text = "Console Messages: No File Was Selected";
            }
        }

        private void convertIMGToPNGFile(object sender, EventArgs e)
        {
            if (fileLoadedIMG == 1)
            {
                consoleText.Text = "Console Messages: Converting... Please wait...";
                IntDoesFileHaveColorPalCheck();
            }
            else
            {
                consoleText.Text = "Console Messages: There is no .img file loaded";
            }
        }

        public void IntDoesFileHaveColorPalCheck()
        {
            long byteTotal = new FileInfo(ofd.FileName).Length;
            int dataAmountWithoutHeader = unchecked((int)byteTotal) - 32;
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(26, SeekOrigin.Begin);
            byte[] swizzleCheck = reader.ReadBytes(2);
            string swizzleCheckString = BitConverter.ToString(swizzleCheck);
            switch (swizzleCheckString)
            {
                case "00-00":
                    isSwizzled = 0;
                    break;
                default:
                    isSwizzled = 1;
                    break;
            }
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] imageWidthBytes = reader.ReadBytes(2);
            byte[] imageHeightBytes = reader.ReadBytes(2);
            int imageWidth = BitConverter.ToInt16(imageWidthBytes, 0);
            int imageHeight = BitConverter.ToInt16(imageHeightBytes, 0);
            int pixelAmount = imageWidth * imageHeight;
            int pixelAmountTimes4 = imageWidth * imageHeight * 4;
            int dataColPalLength = dataAmountWithoutHeader - pixelAmount;
            if (pixelAmountTimes4 == dataAmountWithoutHeader)
            {
                hasColorPalData = 0;
            }
            else
            {
                hasColorPalData = 1;
                if (dataColPalLength <= 64)
                {
                    colorPalDataOver16 = 0;
                    colorPalDataOver128 = 0;
                }
                else
                {
                    if (dataColPalLength > 512)
                    {
                        colorPalDataOver16 = 1;
                        colorPalDataOver128 = 1;
                    }
                    else
                    {
                        colorPalDataOver16 = 1;
                        colorPalDataOver128 = 0;
                    }
                }
            }
            IntSizeCheck();
        }

        public void IntSizeCheck ()
        {
            if (hasColorPalData == 0)
            {
                if (isSwizzled == 0)
                {
                    ConvertImgToPNGNoColorPal();
                }
                else
                {
                    consoleText.Text = "Console Messages: IMG files without color pal should not be swizzled?!?";
                }

            }
            else
            {
                if (colorPalDataOver16 == 0)
                {
                    if (isSwizzled == 0)
                    {
                        ConvertImgToPNGWithColorPal16();
                    }
                    else
                    {
                        ConvertImgToPNGWithColorPal16Swizzled();
                    }
                }
                else
                {
                    if (colorPalDataOver128 == 1)
                    {
                        if (isSwizzled == 0)
                        {
                            ConvertImgToPNGWithColorPal256();
                        }
                        else
                        {
                            ConvertImgToPNGWithColorPal256Swizzled();
                        }
                    }
                    else
                    {
                        if (isSwizzled == 0)
                        {
                            ConvertImgToPNGWithColorPal256Halfed();
                        }
                        else
                        {
                            ConvertImgToPNGWithColorPal256HalfedSwizzled();
                        }
                    }
                }
            }
        }

        public void ConvertImgToPNGWithColorPal16()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] imageWidthBytes = reader.ReadBytes(2);
            byte[] imageHeightBytes = reader.ReadBytes(2);
            int imageWidth = BitConverter.ToInt16(imageWidthBytes, 0);
            int imageHeight = BitConverter.ToInt16(imageHeightBytes, 0);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int currentPixelHeight = 0;
            int currentPixelPalWidth = 0;
            int currentPixelWidth = 0;
            int pIndex = 0;
            List<int[]> colorPalList = new List<int[]>();
            int[] tempColorPal = new int[4] { 128, 128, 128, 128 };
            int[] tempColorPal2 = new int[4] { 128, 128, 128, 128 };

            int pixelAmount = imageWidth * imageHeight / 2;
            long byteTotal = new FileInfo(ofd.FileName).Length;
            int pixelDataOffset = unchecked((int)byteTotal) - pixelAmount;
            int colorPalLength = (pixelDataOffset - 32) / 4;
            int colorPalLengthForWidth = colorPalLength * 10;
            var imageColorPal = new Bitmap(colorPalLengthForWidth, 10);
            var imageFinal = new Bitmap(imageWidth, imageHeight);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            while (pIndex < colorPalLength)
            {
                byte redValue = reader.ReadBytes(1)[0];
                byte greenValue = reader.ReadBytes(1)[0];
                byte blueValue = reader.ReadBytes(1)[0];
                byte alphaValue = reader.ReadBytes(1)[0];
                if (alphaOverrideCheckbox.Checked)
                {
                    alphaValue = 255;
                }
                using (var g = Graphics.FromImage(imageColorPal))
                {
                    int wIndex = 0;
                    while (wIndex < 10)
                    {
                        imageColorPal.SetPixel(currentPixelPalWidth, 0, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 1, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 2, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 3, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 4, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 5, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 6, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 7, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 8, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 9, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        currentPixelPalWidth++;
                        wIndex++;
                    }
                    pIndex++;
                    tempColorPal = new int[4] { alphaValue, redValue, greenValue, blueValue };
                    colorPalList.Add(tempColorPal);
                }

            }
            if (exportColorPalCheckbox.Checked)
            {
            sfd.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd.SupportMultiDottedExtensions = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                imageColorPal.Save(sfd.FileName);
                consoleText.Text = "Console Messages: File has been converted and saved";
            }
            else
            {
                    consoleText.Text = "Console Messages: Saving color pal file was canceled, but we must continue on";
            }
            }

            int iIndex = 0;
            int iX = imageWidth * imageHeight;
            while (iIndex < iX)
            {
                using (var g = Graphics.FromImage(imageFinal))
                {
                    byte[] currentPixels = reader.ReadBytes(1);
                    string currentPixelsString = BitConverter.ToString(currentPixels);
                    string secondByteHalf = currentPixelsString.Substring(0, (int)(currentPixelsString.Length / 2));
                    string firstByteHalf = currentPixelsString.Substring((int)(currentPixelsString.Length / 2), (int)(currentPixelsString.Length / 2));
                    int pixelFirstHalf = 0;
                    int pixelSecondHalf = 0;
                    switch (firstByteHalf)
                    {
                        case "0":
                            pixelFirstHalf = 0;
                            break;
                        case "1":
                            pixelFirstHalf = 1;
                            break;
                        case "2":
                            pixelFirstHalf = 2;
                            break;
                        case "3":
                            pixelFirstHalf = 3;
                            break;
                        case "4":
                            pixelFirstHalf = 4;
                            break;
                        case "5":
                            pixelFirstHalf = 5;
                            break;
                        case "6":
                            pixelFirstHalf = 6;
                            break;
                        case "7":
                            pixelFirstHalf = 7;
                            break;
                        case "8":
                            pixelFirstHalf = 8;
                            break;
                        case "9":
                            pixelFirstHalf = 9;
                            break;
                        case "A":
                            pixelFirstHalf = 10;
                            break;
                        case "B":
                            pixelFirstHalf = 11;
                            break;
                        case "C":
                            pixelFirstHalf = 12;
                            break;
                        case "D":
                            pixelFirstHalf = 13;
                            break;
                        case "E":
                            pixelFirstHalf = 14;
                            break;
                        case "F":
                            pixelFirstHalf = 15;
                            break;
                        default:
                            pixelFirstHalf = 0;
                            break;
                    }
                    tempColorPal = colorPalList[pixelFirstHalf];
                    imageFinal.SetPixel(currentPixelWidth, currentPixelHeight, Color.FromArgb((int)tempColorPal[0], (int)tempColorPal[1], (int)tempColorPal[2], (int)tempColorPal[3]));
                    currentPixelWidth++;
                    iIndex++;
                    switch (secondByteHalf)
                    {
                        case "0":
                            pixelSecondHalf = 0;
                            break;
                        case "1":
                            pixelSecondHalf = 1;
                            break;
                        case "2":
                            pixelSecondHalf = 2;
                            break;
                        case "3":
                            pixelSecondHalf = 3;
                            break;
                        case "4":
                            pixelSecondHalf = 4;
                            break;
                        case "5":
                            pixelSecondHalf = 5;
                            break;
                        case "6":
                            pixelSecondHalf = 6;
                            break;
                        case "7":
                            pixelSecondHalf = 7;
                            break;
                        case "8":
                            pixelSecondHalf = 8;
                            break;
                        case "9":
                            pixelSecondHalf = 9;
                            break;
                        case "A":
                            pixelSecondHalf = 10;
                            break;
                        case "B":
                            pixelSecondHalf = 11;
                            break;
                        case "C":
                            pixelSecondHalf = 12;
                            break;
                        case "D":
                            pixelSecondHalf = 13;
                            break;
                        case "E":
                            pixelSecondHalf = 14;
                            break;
                        case "F":
                            pixelSecondHalf = 15;
                            break;
                        default:
                            pixelSecondHalf = 0;
                            break;
                    }
                    tempColorPal2 = colorPalList[pixelSecondHalf];
                    imageFinal.SetPixel(currentPixelWidth, currentPixelHeight, Color.FromArgb((int)tempColorPal2[0], (int)tempColorPal[1], (int)tempColorPal[2], (int)tempColorPal[3]));
                    currentPixelWidth++;
                    iIndex++;
                    if (currentPixelWidth == imageWidth)
                    {
                        currentPixelHeight++;
                        currentPixelWidth = 0;
                    }
                }
            }
            sfd2.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd2.SupportMultiDottedExtensions = true;

            if (sfd2.ShowDialog() == DialogResult.OK)
            {
                imageFinal.Save(sfd2.FileName);
                consoleText.Text = "Console Messages: File has been converted and saved";
            }

        }

        public void ConvertImgToPNGWithColorPal16Swizzled()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] imageWidthBytes = reader.ReadBytes(2);
            byte[] imageHeightBytes = reader.ReadBytes(2);
            int imageWidth = BitConverter.ToInt16(imageWidthBytes, 0);
            int imageHeight = BitConverter.ToInt16(imageHeightBytes, 0);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int currentPixelHeight = 0;
            int currentPixelPalWidth = 0;
            int currentPixelWidth = 0;
            int pIndex = 0;
            List<int[]> colorPalList = new List<int[]>();
            int[] tempColorPal = new int[4] { 128, 128, 128, 128 };
            int[] tempColorPal2 = new int[4] { 128, 128, 128, 128 };
            int pixelAmount = imageWidth * imageHeight / 2;
            long byteTotal = new FileInfo(ofd.FileName).Length;
            int pixelDataOffset = unchecked((int)byteTotal) - pixelAmount;
            int colorPalLength = (pixelDataOffset - 32) / 4;
            int colorPalLengthForWidth = colorPalLength * 10;
            var imageColorPal = new Bitmap(colorPalLengthForWidth, 10);
            var imageFinal = new Bitmap(imageWidth, imageHeight);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            while (pIndex < colorPalLength)
            {
                byte redValue = reader.ReadBytes(1)[0];
                byte greenValue = reader.ReadBytes(1)[0];
                byte blueValue = reader.ReadBytes(1)[0];
                byte alphaValue = reader.ReadBytes(1)[0];
                if (alphaOverrideCheckbox.Checked)
                {
                    alphaValue = 255;
                }
                using (var g = Graphics.FromImage(imageColorPal))
                {
                    int wIndex = 0;
                    while (wIndex < 10)
                    {
                        imageColorPal.SetPixel(currentPixelPalWidth, 0, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 1, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 2, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 3, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 4, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 5, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 6, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 7, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 8, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 9, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        currentPixelPalWidth++;
                        wIndex++;
                    }
                    pIndex++;
                    tempColorPal = new int[4] { alphaValue, redValue, greenValue, blueValue };
                    colorPalList.Add(tempColorPal);
                }

            }
            if (exportColorPalCheckbox.Checked)
            {
                sfd.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                sfd.SupportMultiDottedExtensions = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    imageColorPal.Save(sfd.FileName);
                    consoleText.Text = "Console Messages: File has been converted and saved";
                }
                else
                {
                    consoleText.Text = "Console Messages: Saving file was canceled, process has been ended";
                }
            }

            int iIndex = 0;
            int horizontalPixelIndex = 0;
            int verticalPixelIndex = 0;
            int chunkNum = 0;
            int chunkOffset = 0;
            int iX = imageWidth * imageHeight;
            while (iIndex < iX)
            {
                using (var g = Graphics.FromImage(imageFinal))
                {
                    switch (verticalPixelIndex)
                    {
                        case 0:
                            if (horizontalPixelIndex == 0)
                            {
                                if (chunkNum > 0)
                                {
                                    reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                    reader.BaseStream.Seek(chunkOffset, SeekOrigin.Current);
                                }
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 1:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                reader.BaseStream.Seek(16 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 2:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                reader.BaseStream.Seek(32 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 3:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                reader.BaseStream.Seek(48 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 4:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                reader.BaseStream.Seek(64 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 5:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                reader.BaseStream.Seek(80 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 6:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                reader.BaseStream.Seek(96 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 7:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(96, SeekOrigin.Begin);
                                reader.BaseStream.Seek(112 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        default:
                            Console.WriteLine("verticalPixelIndex missing statment for this case");
                            break;
                    }
                    byte[] currentPixels = reader.ReadBytes(1);
                    string currentPixelsString = BitConverter.ToString(currentPixels);
                    string secondByteHalf = currentPixelsString.Substring(0, (int)(currentPixelsString.Length / 2));
                    string firstByteHalf = currentPixelsString.Substring((int)(currentPixelsString.Length / 2), (int)(currentPixelsString.Length / 2));
                    int pixelFirstHalf = 0;
                    int pixelSecondHalf = 0;
                    switch (firstByteHalf)
                    {
                        case "0":
                            pixelFirstHalf = 0;
                            break;
                        case "1":
                            pixelFirstHalf = 1;
                            break;
                        case "2":
                            pixelFirstHalf = 2;
                            break;
                        case "3":
                            pixelFirstHalf = 3;
                            break;
                        case "4":
                            pixelFirstHalf = 4;
                            break;
                        case "5":
                            pixelFirstHalf = 5;
                            break;
                        case "6":
                            pixelFirstHalf = 6;
                            break;
                        case "7":
                            pixelFirstHalf = 7;
                            break;
                        case "8":
                            pixelFirstHalf = 8;
                            break;
                        case "9":
                            pixelFirstHalf = 9;
                            break;
                        case "A":
                            pixelFirstHalf = 10;
                            break;
                        case "B":
                            pixelFirstHalf = 11;
                            break;
                        case "C":
                            pixelFirstHalf = 12;
                            break;
                        case "D":
                            pixelFirstHalf = 13;
                            break;
                        case "E":
                            pixelFirstHalf = 14;
                            break;
                        case "F":
                            pixelFirstHalf = 15;
                            break;
                        default:
                            pixelFirstHalf = 0;
                            break;
                    }
                    tempColorPal = colorPalList[pixelFirstHalf];
                    imageFinal.SetPixel(currentPixelWidth, currentPixelHeight, Color.FromArgb((int)tempColorPal[0], (int)tempColorPal[1], (int)tempColorPal[2], (int)tempColorPal[3]));
                    currentPixelWidth++;
                    iIndex++;
                    switch (secondByteHalf)
                    {
                        case "0":
                            pixelSecondHalf = 0;
                            break;
                        case "1":
                            pixelSecondHalf = 1;
                            break;
                        case "2":
                            pixelSecondHalf = 2;
                            break;
                        case "3":
                            pixelSecondHalf = 3;
                            break;
                        case "4":
                            pixelSecondHalf = 4;
                            break;
                        case "5":
                            pixelSecondHalf = 5;
                            break;
                        case "6":
                            pixelSecondHalf = 6;
                            break;
                        case "7":
                            pixelSecondHalf = 7;
                            break;
                        case "8":
                            pixelSecondHalf = 8;
                            break;
                        case "9":
                            pixelSecondHalf = 9;
                            break;
                        case "A":
                            pixelSecondHalf = 10;
                            break;
                        case "B":
                            pixelSecondHalf = 11;
                            break;
                        case "C":
                            pixelSecondHalf = 12;
                            break;
                        case "D":
                            pixelSecondHalf = 13;
                            break;
                        case "E":
                            pixelSecondHalf = 14;
                            break;
                        case "F":
                            pixelSecondHalf = 15;
                            break;
                        default:
                            pixelSecondHalf = 0;
                            break;
                    }
                    tempColorPal2 = colorPalList[pixelSecondHalf];
                    imageFinal.SetPixel(currentPixelWidth, currentPixelHeight, Color.FromArgb((int)tempColorPal2[0], (int)tempColorPal2[1], (int)tempColorPal2[2], (int)tempColorPal2[3]));
                    currentPixelWidth++;
                    iIndex++;
                    horizontalPixelIndex++;
                    if (currentPixelWidth == imageWidth)
                    {
                        currentPixelHeight++;
                        currentPixelWidth = 0;
                        if (verticalPixelIndex == 7)
                        {
                            if (horizontalPixelIndex == 16)
                            {
                                chunkNum++;
                                chunkOffset += imageWidth * 4;
                                verticalPixelIndex = 0;
                                horizontalPixelIndex = 0;
                            }
                        }
                        else
                        {
                            verticalPixelIndex++;
                            horizontalPixelIndex = 0;
                        }
                    }
                }
            }
            sfd2.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd2.SupportMultiDottedExtensions = true;

            if (sfd2.ShowDialog() == DialogResult.OK)
            {
                imageFinal.Save(sfd2.FileName);
                consoleText.Text = "Console Messages: File has been converted and saved";
            }
            else
            {
                consoleText.Text = "Console Messages: Saving file was canceled, process has been ended";
            }
        }

        public void ConvertImgToPNGWithColorPal256()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] imageWidthBytes = reader.ReadBytes(2);
            byte[] imageHeightBytes = reader.ReadBytes(2);
            int imageWidth = BitConverter.ToInt16(imageWidthBytes, 0);
            int imageHeight = BitConverter.ToInt16(imageHeightBytes, 0);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int currentPixelHeight = 0;
            int currentPixelPalWidth = 0;
            int currentPixelWidth = 0;
            int pIndex = 0;
            List<int[]> colorPalList = new List<int[]>();
            int[] tempColorPal = new int[4] { 128, 128, 128, 128 };
            int[] tempColorPal2 = new int[4] { 128, 128, 128, 128 };

            int pixelAmount = imageWidth * imageHeight;
            long byteTotal = new FileInfo(ofd.FileName).Length;
            int pixelDataOffset = unchecked((int)byteTotal) - pixelAmount;
            int colorPalLength = (pixelDataOffset - 32) / 4;
            int colorPalLengthForWidth = colorPalLength * 10;
            var imageColorPal = new Bitmap(colorPalLengthForWidth, 10);
            var imageFinal = new Bitmap(imageWidth, imageHeight);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            while (pIndex < colorPalLength)
            {
                byte redValue = reader.ReadBytes(1)[0];
                byte greenValue = reader.ReadBytes(1)[0];
                byte blueValue = reader.ReadBytes(1)[0];
                byte alphaValue = reader.ReadBytes(1)[0];
                if (alphaOverrideCheckbox.Checked)
                {
                    alphaValue = 255;
                }
                using (var g = Graphics.FromImage(imageColorPal))
                {
                    int wIndex = 0;
                    while (wIndex < 10)
                    {
                        imageColorPal.SetPixel(currentPixelPalWidth, 0, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 1, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 2, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 3, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 4, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 5, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 6, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 7, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 8, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 9, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        currentPixelPalWidth++;
                        wIndex++;
                    }
                    pIndex++;
                    tempColorPal = new int[4] { alphaValue, redValue, greenValue, blueValue };
                    colorPalList.Add(tempColorPal);
                }

            }
            if (exportColorPalCheckbox.Checked)
            {
            sfd.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd.SupportMultiDottedExtensions = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                imageColorPal.Save(sfd.FileName);
                consoleText.Text = "Console Messages: File has been converted and saved";
            }
            else
                {
                    consoleText.Text = "Console Messages: Saving color pal file was canceled, but we must continue on";
                }
            }

            int iIndex = 0;
            int iX = imageWidth * imageHeight;
            while (iIndex < iX)
            {
                using (var g = Graphics.FromImage(imageFinal))
                {
                    byte currentPixels = reader.ReadBytes(1)[0];
                    int pixelFirstHalf = (int)currentPixels;
                    tempColorPal = colorPalList[pixelFirstHalf];
                    imageFinal.SetPixel(currentPixelWidth, currentPixelHeight, Color.FromArgb((int)tempColorPal[0], (int)tempColorPal[1], (int)tempColorPal[2], (int)tempColorPal[3]));
                    currentPixelWidth++;
                    iIndex++;
                    if (currentPixelWidth == imageWidth)
                    {
                        currentPixelHeight++;
                        currentPixelWidth = 0;
                    }
                }
            }
            sfd2.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd2.SupportMultiDottedExtensions = true;

            if (sfd2.ShowDialog() == DialogResult.OK)
            {
                imageFinal.Save(sfd2.FileName);
                consoleText.Text = "Console Messages: File has been converted and saved";
            }
            else
            {
                consoleText.Text = "Console Messages: Saving file was canceled, process has been ended";
            }
        }

        public void ConvertImgToPNGWithColorPal256Halfed()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] imageWidthBytes = reader.ReadBytes(2);
            byte[] imageHeightBytes = reader.ReadBytes(2);
            int imageWidth = BitConverter.ToInt16(imageWidthBytes, 0);
            int imageHeight = BitConverter.ToInt16(imageHeightBytes, 0);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int currentPixelHeight = 0;
            int currentPixelPalWidth = 0;
            int currentPixelWidth = 0;
            int pIndex = 0;
            List<int[]> colorPalList = new List<int[]>();
            int[] tempColorPal = new int[4] { 128, 128, 128, 128 };
            int[] tempColorPal2 = new int[4] { 128, 128, 128, 128 };

            int pixelAmount = imageWidth * imageHeight;
            long byteTotal = new FileInfo(ofd.FileName).Length;
            int pixelDataOffset = unchecked((int)byteTotal) - pixelAmount;
            int colorPalLength = (pixelDataOffset - 32) / 2;
            int colorPalLengthForWidth = colorPalLength * 10;
            var imageColorPal = new Bitmap(colorPalLengthForWidth, 10);
            var imageFinal = new Bitmap(imageWidth, imageHeight);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            while (pIndex < colorPalLength)
            {
                byte[] byte_val1 = reader.ReadBytes(1);
                string byte_val1_s = BitConverter.ToString(byte_val1);
                string firstByteHalf = byte_val1_s.Substring(0, (int)(byte_val1_s.Length / 2));
                string secondByteHalf = byte_val1_s.Substring((int)(byte_val1_s.Length / 2), (int)(byte_val1_s.Length / 2));
                int redValue = 0;
                int redValue2 = 0;
                int greenValue = 0;
                switch (firstByteHalf)
                {
                    case "0":
                        redValue = 0;
                        greenValue = 0;
                        break;
                    case "1":
                        redValue = 133;
                        greenValue = 0;
                        break;
                    case "2":
                        redValue = 0;
                        greenValue = 8;
                        break;
                    case "3":
                        redValue = 133;
                        greenValue = 8;
                        break;
                    case "4":
                        redValue = 0;
                        greenValue = 16;
                        break;
                    case "5":
                        redValue = 133;
                        greenValue = 16;
                        break;
                    case "6":
                        redValue = 0;
                        greenValue = 24;
                        break;
                    case "7":
                        redValue = 133;
                        greenValue = 24;
                        break;
                    case "8":
                        redValue = 0;
                        greenValue = 33;
                        break;
                    case "9":
                        redValue = 133;
                        greenValue = 33;
                        break;
                    case "A":
                        redValue = 0;
                        greenValue = 41;
                        break;
                    case "B":
                        redValue = 133;
                        greenValue = 41;
                        break;
                    case "C":
                        redValue = 0;
                        greenValue = 49;
                        break;
                    case "D":
                        redValue = 133;
                        greenValue = 49;
                        break;
                    case "E":
                        redValue = 0;
                        greenValue = 57;
                        break;
                    case "F":
                        redValue = 133;
                        greenValue = 57;
                        break;
                    default:
                        redValue = 0;
                        greenValue = 0;
                        break;
                }
                switch (secondByteHalf)
                {
                    case "0":
                        redValue2 = 0;
                        break;
                    case "1":
                        redValue2 = 8;
                        break;
                    case "2":
                        redValue2 = 16;
                        break;
                    case "3":
                        redValue2 = 24;
                        break;
                    case "4":
                        redValue2 = 33;
                        break;
                    case "5":
                        redValue2 = 41;
                        break;
                    case "6":
                        redValue2 = 49;
                        break;
                    case "7":
                        redValue2 = 57;
                        break;
                    case "8":
                        redValue2 = 66;
                        break;
                    case "9":
                        redValue2 = 75;
                        break;
                    case "A":
                        redValue2 = 83;
                        break;
                    case "B":
                        redValue2 = 91;
                        break;
                    case "C":
                        redValue2 = 100;
                        break;
                    case "D":
                        redValue2 = 108;
                        break;
                    case "E":
                        redValue2 = 116;
                        break;
                    case "F":
                        redValue2 = 124;
                        break;
                    default:
                        redValue2 = 0;
                        break;
                }

                byte[] bytesValue2 = reader.ReadBytes(1);
                string bytesValue2String = BitConverter.ToString(bytesValue2);
                string firstByteHalf2 = bytesValue2String.Substring(0, (int)(bytesValue2String.Length / 2));
                string secondByteHalf2 = bytesValue2String.Substring((int)(bytesValue2String.Length / 2), (int)(bytesValue2String.Length / 2));
                int alphaValue = 0;
                int blueValue = 0;
                int blueValue2 = 0;
                int greenValue2 = 0;
                switch (firstByteHalf2)
                {
                    case "0":
                        alphaValue = 0;
                        blueValue = 0;
                        break;
                    case "1":
                        alphaValue = 0;
                        blueValue = 33;
                        break;
                    case "2":
                        alphaValue = 0;
                        blueValue = 66;
                        break;
                    case "3":
                        alphaValue = 0;
                        blueValue = 100;
                        break;
                    case "4":
                        alphaValue = 0;
                        blueValue = 133;
                        break;
                    case "5":
                        alphaValue = 0;
                        blueValue = 166;
                        break;
                    case "6":
                        alphaValue = 0;
                        blueValue = 200;
                        break;
                    case "7":
                        alphaValue = 0;
                        blueValue = 233;
                        break;
                    case "8":
                        alphaValue = 255;
                        blueValue = 0;
                        break;
                    case "9":
                        alphaValue = 255;
                        blueValue = 33;
                        break;
                    case "A":
                        alphaValue = 255;
                        blueValue = 66;
                        break;
                    case "B":
                        alphaValue = 255;
                        blueValue = 100;
                        break;
                    case "C":
                        alphaValue = 255;
                        blueValue = 133;
                        break;
                    case "D":
                        alphaValue = 255;
                        blueValue = 166;
                        break;
                    case "E":
                        alphaValue = 255;
                        blueValue = 200;
                        break;
                    case "F":
                        alphaValue = 255;
                        blueValue = 233;
                        break;
                    default:
                        alphaValue = 0;
                        blueValue = 0;
                        break;
                }
                switch (secondByteHalf2)
                {
                    case "0":
                        blueValue2 = 0;
                        greenValue2 = 0;
                        break;
                    case "1":
                        blueValue2 = 0;
                        greenValue2 = 66;
                        break;
                    case "2":
                        blueValue2 = 0;
                        greenValue2 = 133;
                        break;
                    case "3":
                        blueValue2 = 0;
                        greenValue2 = 200;
                        break;
                    case "4":
                        blueValue2 = 8;
                        greenValue2 = 0;
                        break;
                    case "5":
                        blueValue2 = 8;
                        greenValue2 = 66;
                        break;
                    case "6":
                        blueValue2 = 8;
                        greenValue2 = 133;
                        break;
                    case "7":
                        blueValue2 = 8;
                        greenValue2 = 200;
                        break;
                    case "8":
                        blueValue2 = 16;
                        greenValue2 = 0;
                        break;
                    case "9":
                        blueValue2 = 16;
                        greenValue2 = 66;
                        break;
                    case "A":
                        blueValue2 = 16;
                        greenValue2 = 133;
                        break;
                    case "B":
                        blueValue2 = 16;
                        greenValue2 = 200;
                        break;
                    case "C":
                        blueValue2 = 24;
                        greenValue2 = 0;
                        break;
                    case "D":
                        blueValue2 = 24;
                        greenValue2 = 66;
                        break;
                    case "E":
                        blueValue2 = 24;
                        greenValue2 = 133;
                        break;
                    case "F":
                        blueValue2 = 24;
                        greenValue2 = 200;
                        break;
                    default:
                        blueValue2 = 0;
                        greenValue2 = 0;
                        break;
                }
                if (alphaOverrideCheckbox.Checked)
                {
                    alphaValue = 255;
                }
                int redValueFinal = redValue + redValue2;
                if (redValueFinal > 255)
                {
                    redValueFinal = 255;
                }
                int greenValueFinal = greenValue + greenValue2;
                if (greenValueFinal > 255)
                {
                    greenValueFinal = 255;
                }
                int blueValueFinal = blueValue + blueValue2;
                if (blueValueFinal > 255)
                {
                    blueValueFinal = 255;
                }
                using (var g = Graphics.FromImage(imageColorPal))
                {
                    int wIndex = 0;
                    while (wIndex < 10)
                    {
                        imageColorPal.SetPixel(currentPixelPalWidth, 0, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 1, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 2, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 3, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 4, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 5, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 6, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 7, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 8, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 9, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        currentPixelPalWidth++;
                        wIndex++;
                    }
                    pIndex++;
                    tempColorPal = new int[4] { alphaValue, redValueFinal, greenValueFinal, blueValueFinal };
                    colorPalList.Add(tempColorPal);
                }

            }
            if (exportColorPalCheckbox.Checked)
            {
                sfd.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                sfd.SupportMultiDottedExtensions = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    imageColorPal.Save(sfd.FileName);
                    consoleText.Text = "Console Messages: File has been converted and saved";
                }
                else
                {
                    consoleText.Text = "Console Messages: Saving color pal file was canceled, but we must continue on";
                }
            }

            int iIndex = 0;
            int iX = imageWidth * imageHeight;
            while (iIndex < iX)
            {
                using (var g = Graphics.FromImage(imageFinal))
                {
                    byte currentPixels = reader.ReadBytes(1)[0];
                    int pixelFirstHalf = (int)currentPixels;
                    tempColorPal = colorPalList[pixelFirstHalf];
                    imageFinal.SetPixel(currentPixelWidth, currentPixelHeight, Color.FromArgb((int)tempColorPal[0], (int)tempColorPal[1], (int)tempColorPal[2], (int)tempColorPal[3]));
                    currentPixelWidth++;
                    iIndex++;
                    if (currentPixelWidth == imageWidth)
                    {
                        currentPixelHeight++;
                        currentPixelWidth = 0;
                    }
                }
            }
            sfd2.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd2.SupportMultiDottedExtensions = true;

            if (sfd2.ShowDialog() == DialogResult.OK)
            {
                imageFinal.Save(sfd2.FileName);
                consoleText.Text = "Console Messages: File has been converted and saved";
            }
            else
            {
                consoleText.Text = "Console Messages: Saving file was canceled, process has been ended";
            }
        }

        public void ConvertImgToPNGWithColorPal256Swizzled()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] imageWidthBytes = reader.ReadBytes(2);
            byte[] imageHeightBytes = reader.ReadBytes(2);
            int imageWidth = BitConverter.ToInt16(imageWidthBytes, 0);
            int imageHeight = BitConverter.ToInt16(imageHeightBytes, 0);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int currentPixelHeight = 0;
            int currentPixelPalWidth = 0;
            int currentPixelWidth = 0;
            int pIndex = 0;
            List<int[]> colorPalList = new List<int[]>();
            int[] tempColorPal = new int[4] { 128, 128, 128, 128 };
            int[] tempColorPal2 = new int[4] { 128, 128, 128, 128 };

            int pixelAmount = imageWidth * imageHeight;
            long byteTotal = new FileInfo(ofd.FileName).Length;
            int pixelDataOffset = unchecked((int)byteTotal) - pixelAmount;
            int colorPalLength = (pixelDataOffset - 32) / 4;
            int colorPalLengthForWidth = colorPalLength * 10;
            var imageColorPal = new Bitmap(colorPalLengthForWidth, 10);
            var imageFinal = new Bitmap(imageWidth, imageHeight);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            while (pIndex < colorPalLength)
            {
                byte redValue = reader.ReadBytes(1)[0];
                byte greenValue = reader.ReadBytes(1)[0];
                byte blueValue = reader.ReadBytes(1)[0];
                byte alphaValue = reader.ReadBytes(1)[0];
                if (alphaOverrideCheckbox.Checked)
                {
                    alphaValue = 255;
                }
                using (var g = Graphics.FromImage(imageColorPal))
                {
                    int wIndex = 0;
                    while (wIndex < 10)
                    {
                        imageColorPal.SetPixel(currentPixelPalWidth, 0, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 1, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 2, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 3, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 4, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 5, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 6, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 7, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 8, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 9, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                        currentPixelPalWidth++;
                        wIndex++;
                    }
                    pIndex++;
                    tempColorPal = new int[4] { alphaValue, redValue, greenValue, blueValue };
                    colorPalList.Add(tempColorPal);
                }

            }
            if (exportColorPalCheckbox.Checked)
            {
                sfd.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                sfd.SupportMultiDottedExtensions = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    imageColorPal.Save(sfd.FileName);
                    consoleText.Text = "Console Messages: File has been converted and saved";
                }
                else
                {
                    consoleText.Text = "Console Messages: Saving color pal file was canceled, but we must continue on";
                }
            }

            int iIndex = 0;
            int horizontalPixelIndex = 0;
            int verticalPixelIndex = 0;
            int chunkNum = 0;
            int chunkOffset = 0;
            int iX = imageWidth * imageHeight;
            while (iIndex < iX)
            {
                using (var g = Graphics.FromImage(imageFinal))
                {
                    switch (verticalPixelIndex)
                    {
                        case 0:
                            if (horizontalPixelIndex == 0)
                            {
                                if (chunkNum > 0)
                                {
                                    reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                    reader.BaseStream.Seek(chunkOffset, SeekOrigin.Current);
                                }
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 1:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                reader.BaseStream.Seek(16 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 2:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                reader.BaseStream.Seek(32 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 3:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                reader.BaseStream.Seek(48 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 4:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                reader.BaseStream.Seek(64 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 5:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                reader.BaseStream.Seek(80 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 6:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                reader.BaseStream.Seek(96 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 7:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(1056, SeekOrigin.Begin);
                                reader.BaseStream.Seek(112 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        default:
                            Console.WriteLine("verticalPixelIndex missing statment for this case");
                            break;
                    }
                    byte currentPixels = reader.ReadBytes(1)[0];
                    int pixelFirstHalf = (int)currentPixels;
                    tempColorPal = colorPalList[pixelFirstHalf];
                    imageFinal.SetPixel(currentPixelWidth, currentPixelHeight, Color.FromArgb((int)tempColorPal[0], (int)tempColorPal[1], (int)tempColorPal[2], (int)tempColorPal[3]));
                    currentPixelWidth++;
                    iIndex++;
                    horizontalPixelIndex++;
                    if (currentPixelWidth == imageWidth)
                    {
                        currentPixelHeight++;
                        currentPixelWidth = 0;
                        if (verticalPixelIndex == 7)
                        {
                            if (horizontalPixelIndex == 16)
                            {
                                chunkNum++;
                                chunkOffset += imageWidth * 8;
                                verticalPixelIndex = 0;
                                horizontalPixelIndex = 0;
                            }
                        }
                        else
                        {
                            verticalPixelIndex++;
                            horizontalPixelIndex = 0;
                        }
                    }
                }
            }
            sfd2.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd2.SupportMultiDottedExtensions = true;

            if (sfd2.ShowDialog() == DialogResult.OK)
            {
                imageFinal.Save(sfd2.FileName);
                consoleText.Text = "Console Messages: File has been converted and saved";
            }
            else
            {
                consoleText.Text = "Console Messages: Saving file was canceled, process has been ended";
            }
        }

        public void ConvertImgToPNGWithColorPal256HalfedSwizzled()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] imageWidthBytes = reader.ReadBytes(2);
            byte[] imageHeightBytes = reader.ReadBytes(2);
            int imageWidth = BitConverter.ToInt16(imageWidthBytes, 0);
            int imageHeight = BitConverter.ToInt16(imageHeightBytes, 0);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int currentPixelHeight = 0;
            int currentPixelPalWidth = 0;
            int currentPixelWidth = 0;
            int pIndex = 0;
            List<int[]> colorPalList = new List<int[]>();
            int[] tempColorPal = new int[4] { 128, 128, 128, 128 };
            int[] tempColorPal2 = new int[4] { 128, 128, 128, 128 };
            int pixelAmount = imageWidth * imageHeight;
            long byteTotal = new FileInfo(ofd.FileName).Length;
            int pixelDataOffset = unchecked((int)byteTotal) - pixelAmount;
            int colorPalLength = (pixelDataOffset - 32) / 2;
            int colorPalLengthForWidth = colorPalLength * 10;
            var imageColorPal = new Bitmap(colorPalLengthForWidth, 10);
            var imageFinal = new Bitmap(imageWidth, imageHeight);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            while (pIndex < colorPalLength)
            {
                byte[] byte_val1 = reader.ReadBytes(1);
                string byte_val1_s = BitConverter.ToString(byte_val1);
                string firstByteHalf = byte_val1_s.Substring(0, (int)(byte_val1_s.Length / 2));
                string secondByteHalf = byte_val1_s.Substring((int)(byte_val1_s.Length / 2), (int)(byte_val1_s.Length / 2));
                int redValue = 0;
                int redValue2 = 0;
                int greenValue = 0;
                switch (firstByteHalf)
                {
                    case "0":
                        redValue = 0;
                        greenValue = 0;
                        break;
                    case "1":
                        redValue = 133;
                        greenValue = 0;
                        break;
                    case "2":
                        redValue = 0;
                        greenValue = 8;
                        break;
                    case "3":
                        redValue = 133;
                        greenValue = 8;
                        break;
                    case "4":
                        redValue = 0;
                        greenValue = 16;
                        break;
                    case "5":
                        redValue = 133;
                        greenValue = 16;
                        break;
                    case "6":
                        redValue = 0;
                        greenValue = 24;
                        break;
                    case "7":
                        redValue = 133;
                        greenValue = 24;
                        break;
                    case "8":
                        redValue = 0;
                        greenValue = 33;
                        break;
                    case "9":
                        redValue = 133;
                        greenValue = 33;
                        break;
                    case "A":
                        redValue = 0;
                        greenValue = 41;
                        break;
                    case "B":
                        redValue = 133;
                        greenValue = 41;
                        break;
                    case "C":
                        redValue = 0;
                        greenValue = 49;
                        break;
                    case "D":
                        redValue = 133;
                        greenValue = 49;
                        break;
                    case "E":
                        redValue = 0;
                        greenValue = 57;
                        break;
                    case "F":
                        redValue = 133;
                        greenValue = 57;
                        break;
                    default:
                        redValue = 0;
                        greenValue = 0;
                        break;
                }
                switch (secondByteHalf)
                {
                    case "0":
                        redValue2 = 0;
                        break;
                    case "1":
                        redValue2 = 8;
                        break;
                    case "2":
                        redValue2 = 16;
                        break;
                    case "3":
                        redValue2 = 24;
                        break;
                    case "4":
                        redValue2 = 33;
                        break;
                    case "5":
                        redValue2 = 41;
                        break;
                    case "6":
                        redValue2 = 49;
                        break;
                    case "7":
                        redValue2 = 57;
                        break;
                    case "8":
                        redValue2 = 66;
                        break;
                    case "9":
                        redValue2 = 75;
                        break;
                    case "A":
                        redValue2 = 83;
                        break;
                    case "B":
                        redValue2 = 91;
                        break;
                    case "C":
                        redValue2 = 100;
                        break;
                    case "D":
                        redValue2 = 108;
                        break;
                    case "E":
                        redValue2 = 116;
                        break;
                    case "F":
                        redValue2 = 124;
                        break;
                    default:
                        redValue2 = 0;
                        break;
                }

                byte[] bytesValue2 = reader.ReadBytes(1);
                string bytesValue2String = BitConverter.ToString(bytesValue2);
                string firstByteHalf2 = bytesValue2String.Substring(0, (int)(bytesValue2String.Length / 2));
                string secondByteHalf2 = bytesValue2String.Substring((int)(bytesValue2String.Length / 2), (int)(bytesValue2String.Length / 2));
                int alphaValue = 0;
                int blueValue = 0;
                int blueValue2 = 0;
                int greenValue2 = 0;
                switch (firstByteHalf2)
                {
                    case "0":
                        alphaValue = 0;
                        blueValue = 0;
                        break;
                    case "1":
                        alphaValue = 0;
                        blueValue = 33;
                        break;
                    case "2":
                        alphaValue = 0;
                        blueValue = 66;
                        break;
                    case "3":
                        alphaValue = 0;
                        blueValue = 100;
                        break;
                    case "4":
                        alphaValue = 0;
                        blueValue = 133;
                        break;
                    case "5":
                        alphaValue = 0;
                        blueValue = 166;
                        break;
                    case "6":
                        alphaValue = 0;
                        blueValue = 200;
                        break;
                    case "7":
                        alphaValue = 0;
                        blueValue = 233;
                        break;
                    case "8":
                        alphaValue = 255;
                        blueValue = 0;
                        break;
                    case "9":
                        alphaValue = 255;
                        blueValue = 33;
                        break;
                    case "A":
                        alphaValue = 255;
                        blueValue = 66;
                        break;
                    case "B":
                        alphaValue = 255;
                        blueValue = 100;
                        break;
                    case "C":
                        alphaValue = 255;
                        blueValue = 133;
                        break;
                    case "D":
                        alphaValue = 255;
                        blueValue = 166;
                        break;
                    case "E":
                        alphaValue = 255;
                        blueValue = 200;
                        break;
                    case "F":
                        alphaValue = 255;
                        blueValue = 233;
                        break;
                    default:
                        alphaValue = 0;
                        blueValue = 0;
                        break;
                }
                switch (secondByteHalf2)
                {
                    case "0":
                        blueValue2 = 0;
                        greenValue2 = 0;
                        break;
                    case "1":
                        blueValue2 = 0;
                        greenValue2 = 66;
                        break;
                    case "2":
                        blueValue2 = 0;
                        greenValue2 = 133;
                        break;
                    case "3":
                        blueValue2 = 0;
                        greenValue2 = 200;
                        break;
                    case "4":
                        blueValue2 = 8;
                        greenValue2 = 0;
                        break;
                    case "5":
                        blueValue2 = 8;
                        greenValue2 = 66;
                        break;
                    case "6":
                        blueValue2 = 8;
                        greenValue2 = 133;
                        break;
                    case "7":
                        blueValue2 = 8;
                        greenValue2 = 200;
                        break;
                    case "8":
                        blueValue2 = 16;
                        greenValue2 = 0;
                        break;
                    case "9":
                        blueValue2 = 16;
                        greenValue2 = 66;
                        break;
                    case "A":
                        blueValue2 = 16;
                        greenValue2 = 133;
                        break;
                    case "B":
                        blueValue2 = 16;
                        greenValue2 = 200;
                        break;
                    case "C":
                        blueValue2 = 24;
                        greenValue2 = 0;
                        break;
                    case "D":
                        blueValue2 = 24;
                        greenValue2 = 66;
                        break;
                    case "E":
                        blueValue2 = 24;
                        greenValue2 = 133;
                        break;
                    case "F":
                        blueValue2 = 24;
                        greenValue2 = 200;
                        break;
                    default:
                        blueValue2 = 0;
                        greenValue2 = 0;
                        break;
                }
                if (alphaOverrideCheckbox.Checked)
                {
                    alphaValue = 255;
                }
                int redValueFinal = redValue + redValue2;
                if (redValueFinal > 255)
                {
                    redValueFinal = 255;
                }
                int greenValueFinal = greenValue + greenValue2;
                if (greenValueFinal > 255)
                {
                    greenValueFinal = 255;
                }
                int blueValueFinal = blueValue + blueValue2;
                if (blueValueFinal > 255)
                {
                    blueValueFinal = 255;
                }
                using (var g = Graphics.FromImage(imageColorPal))
                {
                    int wIndex = 0;
                    while (wIndex < 10)
                    {
                        imageColorPal.SetPixel(currentPixelPalWidth, 0, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 1, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 2, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 3, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 4, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 5, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 6, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 7, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 8, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        imageColorPal.SetPixel(currentPixelPalWidth, 0 + 9, Color.FromArgb(alphaValue, redValueFinal, greenValueFinal, blueValueFinal));
                        currentPixelPalWidth++;
                        wIndex++;
                    }
                    pIndex++;
                    tempColorPal = new int[4] { alphaValue, redValueFinal, greenValueFinal, blueValueFinal };
                    colorPalList.Add(tempColorPal);
                }

            }
            if (exportColorPalCheckbox.Checked)
            {
                sfd.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                sfd.SupportMultiDottedExtensions = true;

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    imageColorPal.Save(sfd.FileName);
                    consoleText.Text = "Console Messages: File has been converted and saved";
                }
                else
                {
                    consoleText.Text = "Console Messages: Saving color pal file was canceled, but we must continue on";
                }
            }

            int iIndex = 0;
            int horizontalPixelIndex = 0;
            int verticalPixelIndex = 0;
            int chunkNum = 0;
            int chunkOffset = 0;
            int iX = imageWidth * imageHeight;
            while (iIndex < iX)
            {
                using (var g = Graphics.FromImage(imageFinal))
                {
                    switch (verticalPixelIndex)
                    {
                        case 0:
                            if (horizontalPixelIndex == 0)
                            {
                                if (chunkNum > 0)
                                {
                                    reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                    reader.BaseStream.Seek(chunkOffset, SeekOrigin.Current);
                                }
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 1:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                reader.BaseStream.Seek(16 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 2:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                reader.BaseStream.Seek(32 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 3:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                reader.BaseStream.Seek(48 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 4:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                reader.BaseStream.Seek(64 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 5:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                reader.BaseStream.Seek(80 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 6:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                reader.BaseStream.Seek(96 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        case 7:
                            if (horizontalPixelIndex == 0)
                            {
                                reader.BaseStream.Seek(544, SeekOrigin.Begin);
                                reader.BaseStream.Seek(112 + chunkOffset, SeekOrigin.Current);
                            }
                            else if (horizontalPixelIndex == 16)
                            {
                                reader.BaseStream.Seek(112, SeekOrigin.Current);
                                horizontalPixelIndex = 0;
                            }
                            break;
                        default:
                            Console.WriteLine("verticalPixelIndex missing statment for this case");
                            break;
                    }
                    byte currentPixels = reader.ReadBytes(1)[0];
                    int pixelFirstHalf = (int)currentPixels;
                    tempColorPal = colorPalList[pixelFirstHalf];
                    imageFinal.SetPixel(currentPixelWidth, currentPixelHeight, Color.FromArgb((int)tempColorPal[0], (int)tempColorPal[1], (int)tempColorPal[2], (int)tempColorPal[3]));
                    currentPixelWidth++;
                    iIndex++;
                    horizontalPixelIndex++;
                    if (currentPixelWidth == imageWidth)
                    {
                        currentPixelHeight++;
                        currentPixelWidth = 0;
                        if (verticalPixelIndex == 7)
                        {
                            if (horizontalPixelIndex == 16)
                            {
                                chunkNum++;
                                chunkOffset += imageWidth * 8;
                                verticalPixelIndex = 0;
                                horizontalPixelIndex = 0;
                            }
                        }
                        else
                        {
                            verticalPixelIndex++;
                            horizontalPixelIndex = 0;
                        }
                    }
                }
            }
            sfd2.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
            sfd2.SupportMultiDottedExtensions = true;

            if (sfd2.ShowDialog() == DialogResult.OK)
            {
                imageFinal.Save(sfd2.FileName);
                consoleText.Text = "Console Messages: File has been converted and saved";
            }
            else
            {
                consoleText.Text = "Console Messages: Saving file was canceled, process has been ended";
            }
        }

        public void ConvertImgToPNGNoColorPal()
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(ofd.FileName));
            reader.BaseStream.Seek(28, SeekOrigin.Begin);
            byte[] imageWidthBytes = reader.ReadBytes(2);
            byte[] imageHeightBytes = reader.ReadBytes(2);
            int imageWidth = BitConverter.ToInt16(imageWidthBytes, 0);
            int imageHeight = BitConverter.ToInt16(imageHeightBytes, 0);
            var imageFinal2 = new Bitmap(imageWidth, imageHeight);
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int currentPixelHeight = 0;
            int currentPixelWidth = 0;
            reader.BaseStream.Seek(32, SeekOrigin.Begin);
            int iIndex = 0;
            int iX = imageWidth * imageHeight;
            while (iIndex < iX)
            {
                    byte redValue = reader.ReadBytes(1)[0];
                    byte greenValue = reader.ReadBytes(1)[0];
                    byte blueValue = reader.ReadBytes(1)[0];
                    byte alphaValue = reader.ReadBytes(1)[0];
                    if (alphaOverrideCheckbox.Checked)
                    {
                        alphaValue = 255;
                    }
                    imageFinal2.SetPixel(currentPixelWidth, currentPixelHeight, Color.FromArgb(alphaValue, redValue, greenValue, blueValue));
                    currentPixelWidth++;
                    iIndex++;
                    if (currentPixelWidth == imageWidth)
                    {
                        currentPixelHeight++;
                        currentPixelWidth = 0;
                    }
                }
                sfd2.Filter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
                sfd2.SupportMultiDottedExtensions = true;

                if (sfd2.ShowDialog() == DialogResult.OK)
                {
                    imageFinal2.Save(sfd2.FileName);
                    consoleText.Text = "Console Messages: File has been converted and saved";
                }
                else
            {
                consoleText.Text = "Console Messages: Saving file was canceled, process has been ended";
            }
        }

        private void openPNGFile(object sender, EventArgs e)
        {
            ofd2.Filter = "All files (*.*)|*.*";
            ofd2.SupportMultiDottedExtensions = true;
            if (ofd2.ShowDialog() == DialogResult.OK)
            {
                fileLoadedPNG = 1;
                openedFileDirText.Text = "Opened File Directory: " + ofd2.FileName;
                openedFileNameText.Text = "Opened File Name: " + ofd2.SafeFileName;
                consoleText.Text = "Console Messages: N/A";
            }
            else
            {
                fileLoadedPNG = 0;
                openedFileDirText.Text = "Opened File Directory: N/A";
                openedFileNameText.Text = "Opened File Name: N/A";
                consoleText.Text = "Console Messages: No File Was Selected";
            }
        }

        private void convertPNGToIMGFile(object sender, EventArgs e)
        {
            if (fileLoadedPNG == 1)
            {
                consoleText.Text = "Console Messages: Converting... Please wait...";
                sfd3.Filter = "All files (*.*)|*.*";
                sfd3.SupportMultiDottedExtensions = true;

                if (sfd3.ShowDialog() == DialogResult.OK)
                {
                    using (FileStream fileStream = new FileStream(sfd3.FileName, FileMode.Create))
                    {
                        if (useColorPalCheckbox.Checked)
                        {
                            HashSet<Color> colors = new HashSet<Color>();
                            List<int[]> colorPalList = new List<int[]>();
                            int[] tempColorPal = new int[4] { 128, 128, 128, 128 };
                            int[] temp_col_pal2 = new int[4] { 128, 128, 128, 128 };
                            var myBitmap = new Bitmap(ofd2.FileName);
                            int iIndex = 0;
                            int iX = myBitmap.Width * myBitmap.Height;
                            int currentPixelWidth = 0;
                            int currentPixelHeight = 0;
                            byte[] arrayHeader = { 0x02, 0x00, 0x00, 0x00, 0x84, 0x02, 0x84, 0x02, 0x06, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                            byte[] arrayWidth = BitConverter.GetBytes(myBitmap.Width);
                            byte[] arrayHeight = BitConverter.GetBytes(myBitmap.Height);
                            fileStream.Write(arrayHeader, 0, 0x1C);
                            fileStream.Write(arrayWidth, 0, 0x02);
                            fileStream.Write(arrayHeight, 0, 0x02);
                            int imageWidth = myBitmap.Width;
                                while (iIndex < iX)
                                {
                                    Color pixelColor = myBitmap.GetPixel(currentPixelWidth, currentPixelHeight);
                                    byte redValue = pixelColor.R;
                                    byte greenValue = pixelColor.G;
                                    byte blueValue = pixelColor.B;
                                    byte alphaValue = pixelColor.A;
                                    tempColorPal = new int[4] { alphaValue, redValue, greenValue, blueValue };
                                    if (colors.Contains(pixelColor))
                                    {
                                        //Console.WriteLine("RGBA value already exists in list, skipping");
                                    }
                                    else
                                    {
                                        colors.Add(pixelColor);
                                        colorPalList.Add(tempColorPal);
                                        byte[] bytes = new byte[4];
                                        bytes[0] = pixelColor.R;
                                        bytes[1] = pixelColor.G;
                                        bytes[2] = pixelColor.B;
                                        bytes[3] = pixelColor.A;
                                        fileStream.Write(bytes, 0, 0x04);
                                    }
                                    currentPixelWidth++;
                                    iIndex++;
                                    if (currentPixelWidth == imageWidth)
                                    {
                                        currentPixelHeight++;
                                        currentPixelWidth = 0;
                                    }
                                }
                                int colorPalListLength = colorPalList.Count;
                                while (colorPalListLength < 256)
                                {
                                    byte[] bytes = new byte[4];
                                    bytes[0] = 0x00;
                                    bytes[1] = 0x00;
                                    bytes[2] = 0x00;
                                    bytes[3] = 0xFF;
                                    fileStream.Write(bytes, 0, 0x04);
                                    tempColorPal = new int[4] { 255, 0, 0, 0 };
                                    colorPalList.Add(tempColorPal);
                                    colorPalListLength = colorPalList.Count;
                                }
                                if (colorPalListLength > 256)
                                {
                                    consoleText.Text = "Console Messages: .png has over 256 colors and cannot be converted";
                                }
                                else
                                {
                                    iIndex = 0;
                                    currentPixelHeight = 0;
                                    currentPixelWidth = 0;
                                    while (iIndex < iX)
                                    {
                                        Color pixelColor = myBitmap.GetPixel(currentPixelWidth, currentPixelHeight);
                                        byte redValue = pixelColor.R;
                                        byte greenValue = pixelColor.G;
                                        byte blueValue = pixelColor.B;
                                        byte alphaValue = pixelColor.A;
                                        tempColorPal = new int[4] { alphaValue, redValue, greenValue, blueValue };
                                        int col_pal_index = colorPalList.FindIndex(l => Enumerable.SequenceEqual(tempColorPal, l));
                                        fileStream.Write(BitConverter.GetBytes(col_pal_index), 0, 0x01);
                                        currentPixelWidth++;
                                        iIndex++;
                                        if (currentPixelWidth == imageWidth)
                                        {
                                            currentPixelHeight++;
                                            currentPixelWidth = 0;
                                        }
                                    }
                                    consoleText.Text = "Console Messages: File has been converted and saved";
                                }
                        }
                        else
                        {
                            var myBitmap = new Bitmap(ofd2.FileName);
                            int iIndex = 0;
                            int iX = myBitmap.Width * myBitmap.Height;
                            int currentPixelWidth = 0;
                            int currentPixelHeight = 0;
                            int imageWidth = myBitmap.Width;
                            byte[] arrayHeader = { 0x02, 0x00, 0x00, 0x00, 0x84, 0x02, 0x84, 0x02, 0x07, 0x00, 0x00, 0x00, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                            byte[] arrayWidth = BitConverter.GetBytes(myBitmap.Width);
                            byte[] arrayHeight = BitConverter.GetBytes(myBitmap.Height);
                            fileStream.Write(arrayHeader, 0, 0x1C);
                            fileStream.Write(arrayWidth, 0, 0x02);
                            fileStream.Write(arrayHeight, 0, 0x02);
                            while (iIndex < iX)
                            {
                                Color pixelColor = myBitmap.GetPixel(currentPixelWidth, currentPixelHeight);
                                byte[] bytes = new byte[4];
                                bytes[0] = pixelColor.R;
                                bytes[1] = pixelColor.G;
                                bytes[2] = pixelColor.B;
                                bytes[3] = pixelColor.A;
                                fileStream.Write(bytes, 0, 0x04);
                                iIndex++;
                                currentPixelWidth++;
                                if (currentPixelWidth == imageWidth)
                                {
                                    currentPixelHeight++;
                                    currentPixelWidth = 0;
                                }
                            }
                            consoleText.Text = "Console Messages: File has been converted and saved";
                        }
                        fileStream.Close();
                    }
                }
                else
                {
                    consoleText.Text = "Console Messages: Saving file was canceled, process has been ended";
                }
            }
            else
            {
                consoleText.Text = "Console Messages: There is no .png file loaded";
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
