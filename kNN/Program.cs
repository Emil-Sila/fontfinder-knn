using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Windows.Forms.DataVisualization.Charting;

namespace kNN
{

    public class kNNEntity
    {
        public int[] array;
        public string font;

        public kNNEntity(int[] array, string font)
        {
            this.array = array;
            this.font = font;
        }


    }

    public static class Program
    {
        static int k = 10;
        static string fontLocation = @"C:\Users\Emil\Desktop\Diplomski\Fonts";
        static Bitmap imageTest = new Bitmap(@"C:\Users\Emil\Desktop\Diplomski\Fonts\Inconsolata\Inconsolata_0BL.png");
        static Bitmap imageTest2 = new Bitmap(@"C:\Users\Emil\Desktop\Diplomski\ANN\example\ex4.jpg");

        static string[] glyphs = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
            "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G",
            "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "1",
            "2", "3", "4", "5", "6", "7", "8", "9", "0" };

        static Dictionary<string, List<kNNEntity>> kNNGroups = new Dictionary<string, List<kNNEntity>>();
        //static Dictionary<string, List<string>> GlyphGroupsAA = new Dictionary<string, List<string>>();
        //static Dictionary<string, List<string>> GlyphGroupsBL = new Dictionary<string, List<string>>();
        //static Dictionary<string, List<string>> GlyphGroupsNH = new Dictionary<string, List<string>>();

        static Dictionary<string, double> UltimateResult = new Dictionary<string, double>();


        static void Main(string[] args)
        {
            // Find all directories in the main path
            List<string> directoryEntries = Directory.GetDirectories(fontLocation).ToList();

            // Group up same glyphs of different font in GlyphGroups dictionary
            OrganisekNNGroups(directoryEntries, "_.png", ".png", kNNGroups);
            OrganisekNNGroups(directoryEntries, "_AA_.png", "AA.png", kNNGroups);
            OrganisekNNGroups(directoryEntries, "_BL_.png", "BL.png", kNNGroups);
            OrganisekNNGroups(directoryEntries, "_NH_.png", "NH.png", kNNGroups);

            var testList = kNNGroups["e"];
            testList.Shuffle();

            // List of Lists
            var split = SplitList(testList, 10);

            //FindOptimalK(split);

            ProcessImg(imageTest2);

            /*
            foreach (var glyph in glyphs)
            {
                foreach (var kNNElement in kNNGroups[glyph])
                {
                    
                }
            }
            */
            Console.ReadLine();
        }

        private static void FindkNN(int k, Dictionary<string, List<kNNEntity>> kNNGroups, Bitmap imageTest, string charValue)
        {
            var trainList = kNNGroups[charValue];
            var imageArray = BitmapToIntArray(imageTest);
            Dictionary<int, double> kDict = new Dictionary<int, double>();
            List<double> distanceList = new List<double>();

            foreach (var trainElement in trainList)
            {
                var distance = (double)kNNDistance(imageArray, trainElement.array);
                distanceList.Add(distance);
            }

            for (int n = 0; n < k; n++)
            {
                var min = distanceList.Min();
                var index = distanceList.IndexOf(min);
                //Console.WriteLine("min: " + min + " index: " + index);
                kDict.Add(index, min);
                distanceList[index] = double.MaxValue;
            }

            var sum = kDict.Sum(x => x.Value);
            //Console.WriteLine(testFontValue);
            //Console.WriteLine(sum);
            kDict = kDict.ToDictionary(x => x.Key, x => Math.Round(Math.Abs(x.Value - sum), 3));
            sum = kDict.Sum(x => x.Value);
            //Console.WriteLine(sum);
            if (sum != 0)
                kDict = kDict.ToDictionary(x => x.Key, x => Math.Round(x.Value / sum, 3));

            Dictionary<string, double> resultDict = new Dictionary<string, double>();

            foreach (var element in kDict)
            {
                var currentFont = trainList[element.Key].font;
                var currentValue = element.Value;

                if (resultDict.ContainsKey(currentFont))
                    resultDict[currentFont] += currentValue;
                else
                    resultDict.Add(currentFont, currentValue); 
            }

            
            foreach (var result in resultDict)
            {
                if (!UltimateResult.ContainsKey(result.Key))
                    UltimateResult.Add(result.Key, result.Value);
                else
                    UltimateResult[result.Key] += result.Value;
                //Console.WriteLine(result.Key + ": " + result.Value);
            }
            //Console.WriteLine("");
            //Console.ReadLine();
        }

        private static void ProcessImg(Bitmap image)
        {
            tessnet2.Tesseract ocr = new tessnet2.Tesseract();

            ocr.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");

            ocr.Init(@"C:\Users\Emil\Documents\Visual Studio 2015\Projects\NeuralNetwork\NeuralNetwork\bin\Debug\tessdata", "eng", false);

            List<tessnet2.Word> result = ocr.DoOCR(image, Rectangle.Empty);
            string resultString = "";

            Dictionary<string, List<Rectangle>> CharLocations = new Dictionary<string, List<Rectangle>>();
            int charCount = 0;

            foreach (tessnet2.Word word in result)
            {
                //Console.WriteLine(word.ToString());
                resultString += word.Text + " ";

                foreach (tessnet2.Character character in word.CharList)
                {
                    charCount++;
                    Rectangle charPosition = FindCharLocation(character.Left, character.Right, character.Top, character.Bottom);

                    //Console.WriteLine("{0} : {1}", character.Value.ToString(), charPosition.ToString());

                    List<Rectangle> allCharBounds;
                    if (!CharLocations.TryGetValue(character.Value.ToString(), out allCharBounds))
                    {
                        allCharBounds = new List<Rectangle>();
                        CharLocations.Add(character.Value.ToString(), allCharBounds);
                    }
                    allCharBounds.Add(charPosition);

                    //G.DrawRectangle(Pens.Blue, charPosition);
                }
            }

            foreach (var charPositions in CharLocations)
            {
                foreach (var charLocation in charPositions.Value)
                {
                    using (Bitmap croppedImage = ScaleImage(image.Clone(charLocation, image.PixelFormat), 30, 30))
                    {
                        //Console.WriteLine("Char: " + charPositions.Key);
                        FindkNN(k, kNNGroups, croppedImage, charPositions.Key);
                    }
                }
            }

            foreach (var res in UltimateResult)
            {
                Console.WriteLine(res.Key + ": " + res.Value);
            }
        }

        private static void FindOptimalK(List<List<kNNEntity>> split)
        {
            List<List<double>> finalResult = new List<List<double>>();

            foreach (var tst in split)
            {
                List<kNNEntity> test = new List<kNNEntity>(tst);
                // Select parts that are used for training and join them in a single list
                var result = split.Where((v, i) => i != split.IndexOf(tst)).ToList();
                List<kNNEntity> train = new List<kNNEntity>(result.SelectMany(x => x).ToList());

                foreach (var testElement in test)
                {
                    List<double> finalK = new List<double>();
                    List<double> distanceList = new List<double>();

                    foreach (var trainElement in train)
                    {
                        if (testElement != trainElement)
                        {
                            var distance = (double)kNNDistance(testElement.array, trainElement.array);
                            distanceList.Add(distance);
                        }
                    }
               
                    /*
                    foreach (var element in distanceList)
                    {
                        Console.Write( element + ", ");
                    }
                    Console.WriteLine("Count: " + distanceList.Count());

                    Console.ReadLine();
                    */

                    for (int k = 1; k <= train.Count(); k++)
                    {
                        List<double> distanceListCopy = new List<double>(distanceList);
                        Dictionary<int, double> kDict = new Dictionary<int, double>();

                        for (int n = 0; n < k; n++)
                        {
                            var min = distanceListCopy.Min();
                            var index = distanceListCopy.IndexOf(min);
                            //Console.WriteLine("min: " + min + " index: " + index);
                            kDict.Add(index, min);
                            distanceListCopy[index] = double.MaxValue;
                        }

                        var testFontValue = testElement.font;
                        var sum = kDict.Sum(x => x.Value);
                        //Console.WriteLine(testFontValue);
                        //Console.WriteLine(sum);
                        kDict = kDict.ToDictionary(x => x.Key, x => Math.Round(Math.Abs(x.Value - sum), 3) );
                        sum = kDict.Sum(x => x.Value);
                        //Console.WriteLine(sum);
                        if (sum != 0)
                            kDict = kDict.ToDictionary(x => x.Key, x => Math.Round(x.Value / sum, 3));
                        double correct = 0;

                        foreach (var element in kDict)
                        {
                            if (train[element.Key].font == testFontValue)
                            {
                                correct += 1;//element.Value;
                            }
                        }
                        correct = Math.Round(correct / k, 3); 
                        //Console.WriteLine(correct);
                        finalK.Add(correct);
                    }
                    //Console.ReadLine();
                    finalResult.Add(finalK);
                }
            }

            double[] totalSum = new double[30];

            foreach (var element in finalResult)
            {
                //Console.WriteLine("--------- new subject ----------");
                for (int i = 0; i<element.Count(); i++)
                {
                    totalSum[i] += element[i];
                    //Console.Write(i + 1 + ": " + element[i] + " | ");
                }
                //Console.WriteLine("");
            }

            for (int i = 0; i < totalSum.Count(); i++)
            {
                totalSum[i] = totalSum[i] / finalResult.Count();
                //Console.Write(i + 1 + ": " + element[i] + " | ");
            }
            MakeAGraph(totalSum);
            //Console.ReadLine();
        }

        private static void MakeAGraph(double[] listElements)
        {
            List<int> xValues = new List<int>();
            for (int x = 0; x<listElements.Count(); x++)
            {
                xValues.Add(x + 1);
            }

            // create the chart
            var chart = new Chart();
            chart.Size = new Size(1000, 400);

            var chartArea = new ChartArea();
            chartArea.AxisX.Interval = 1;
            chartArea.AxisY.Interval = 0.05;
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            chartArea.AxisX.LabelStyle.Font = new Font("Consolas", 8);
            chartArea.AxisY.LabelStyle.Font = new Font("Consolas", 8);
            chart.ChartAreas.Add(chartArea);

            var series = new Series();
            series.Name = "Series1";
            series.ChartType = SeriesChartType.FastLine;
            series.XValueType = ChartValueType.Double;
            chart.Series.Add(series);

            // bind the datapoints
            chart.Series["Series1"].Points.DataBindXY(xValues, listElements);

            // draw!
            chart.Invalidate();

            // write out a file
            chart.SaveImage(@"D:\chart.png", ChartImageFormat.Png);
        }

        public static List<List<kNNEntity>> SplitList(List<kNNEntity> locations, int nSize = 10)
        {
            var list = new List<List<kNNEntity>>();

            for (int i = 0; i < locations.Count; i += nSize)
            {
                list.Add(locations.GetRange(i, Math.Min(nSize, locations.Count - i)));
            }

            return list;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        private static int kNNDistance(int[] testArr, int[] refArr)
        {
            var resultArr = testArr.Select((x, index) => 
                Math.Abs(x - refArr[index]))
                .ToArray();

            return resultArr.Sum();
        }
  
        private static int[] BitmapToIntArray(Bitmap img)
        {
            int counter = 0;
            int[] array = new int[img.Width*img.Height];

            for (int x = 0; x < img.Width; x++)
            {
                for (int y = 0; y < img.Height; y++)
                {
                    Color originalColor = img.GetPixel(x, y);
                    int grayScale = (int)((originalColor.R * 0.3) + (originalColor.G * 0.59) + (originalColor.B * 0.11));
                    array[counter] = grayScale;
                    counter++;
                }
            }

            //Console.Write(String.Join(", ", array));
            //Console.WriteLine("Count: " + array.Count());
            //Console.ReadLine();

            return array;
        }

        /*
        private static Bitmap ConvertToGrayscale(Bitmap img)
        {
            int width = img.Width;
            int heigth = img.Height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < heigth; y++)
                {
                    Color originalColor = img.GetPixel(x, y);
                    int grayScale = (int)((originalColor.R * 0.3) + (originalColor.G * 0.59) + (originalColor.B * 0.11));
                    Color newColor = Color.FromArgb(grayScale, 0, 0);
                    img.SetPixel(x, y, newColor);
                }
            }

            return img;
        }
        */

        private static void OrganisekNNGroups(List<string> fontDirectories, String pathUpper, String pathLower, Dictionary<string, List<kNNEntity>> dict)
        {
            foreach (var directoryEntry in fontDirectories)
            {
                foreach (string glyph in glyphs)
                {
                    string glyphImage;

                    if (char.IsUpper(glyph[0]))
                    {
                        glyphImage = Directory.GetFiles(directoryEntry, "*" + glyph + pathUpper)[0];
                    }
                    else
                    {
                        glyphImage = Directory.GetFiles(directoryEntry, "*" + glyph + pathLower)[0];
                    }

                    FileInfo fInfo = new FileInfo(glyphImage);
                    string fontName = fInfo.Directory.Name;

                    Bitmap image = new Bitmap(glyphImage);
                    int[] array = BitmapToIntArray(image);

                    // Put all of the same glyphs of different fonts into GlyphGroups dictionary
                    List<kNNEntity> sameLetterGlyphs;
                    if (!dict.TryGetValue(glyph, out sameLetterGlyphs))
                    {
                        sameLetterGlyphs = new List<kNNEntity>();
                        dict.Add(glyph, sameLetterGlyphs);
                    }
                    sameLetterGlyphs.Add(new kNNEntity(array, fontName));
                }
            }
            /*
            Console.WriteLine("Number of key - value pairs: " + dict.Count());
            foreach (var pair in dict)
            {
                Console.WriteLine("Letter: " + pair.Key + " has number of entries: " + pair.Value.Count());
            }
            Console.ReadLine();
            */
        }

        public static Bitmap ScaleImage(Bitmap image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = image.Width * ratio;
            var newHeight = image.Height * ratio;

            newWidth = newWidth + newWidth * 0.1;
            newHeight = newHeight + newHeight * 0.1;

            var newImage = new Bitmap(maxWidth, maxHeight);

            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.Clear(Color.White);
                graphics.DrawImage(image, -1, -3, (int)newWidth, (int)newHeight);
            }

            return newImage;
        }

        static Rectangle FindCharLocation(int left, int right, int top, int bottom)
        {
            int xSize = right - left;
            int ySize = bottom - top;
            return new Rectangle(left, top, xSize, ySize);
        }

    }
}
