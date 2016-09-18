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
        static string fontLocation = @"C:\Users\Emil\Desktop\Diplomski\Fonts";
        static Bitmap imageTest = new Bitmap(@"C:\Users\Emil\Desktop\Diplomski\Fonts\Inconsolata\Inconsolata_0BL.png");
        static Bitmap imageTest2 = new Bitmap(@"C:\Users\Emil\Desktop\Diplomski\Fonts\Inconsolata\Inconsolata_0.png");

        static string[] glyphs = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
            "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G",
            "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "1",
            "2", "3", "4", "5", "6", "7", "8", "9", "0" };

        static Dictionary<string, List<kNNEntity>> kNNGroups = new Dictionary<string, List<kNNEntity>>();
        //static Dictionary<string, List<string>> GlyphGroupsAA = new Dictionary<string, List<string>>();
        //static Dictionary<string, List<string>> GlyphGroupsBL = new Dictionary<string, List<string>>();
        //static Dictionary<string, List<string>> GlyphGroupsNH = new Dictionary<string, List<string>>();


        static void Main(string[] args)
        {
            // Find all directories in the main path
            List<string> directoryEntries = Directory.GetDirectories(fontLocation).ToList();

            // Group up same glyphs of different font in GlyphGroups dictionary
            OrganisekNNGroups(directoryEntries, "_.png", ".png", kNNGroups);
            OrganisekNNGroups(directoryEntries, "_AA_.png", "AA.png", kNNGroups);
            OrganisekNNGroups(directoryEntries, "_BL_.png", "BL.png", kNNGroups);
            OrganisekNNGroups(directoryEntries, "_NH_.png", "NH.png", kNNGroups);

            var testList = kNNGroups["a"];
            testList.Shuffle();

            // List of Lists
            var split = SplitList(testList, 10);

            FindOptimalK(split);

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
                            Console.WriteLine("min: " + min + " index: " + index);
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
                        //Console.WriteLine(correct);
                        finalK.Add(correct);
                    }
                    //Console.ReadLine();
                    finalResult.Add(finalK);
                }
            }

            foreach (var element in finalResult)
            {
                Console.WriteLine("--------- new subject ----------");
                for (int i = 0; i<element.Count(); i++)
                {
                    Console.Write(i + 1 + ": " + element[i] + " | ");
                }
                Console.WriteLine("");
            }
            Console.ReadLine();
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

    }
}
