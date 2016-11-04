using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace SandBox
{
    class Program
    {
        private static readonly List<Point> directions = new List<Point>
        {
            new Point(1, 0),
            new Point(0, 1),
            new Point(-1, 0),
            new Point(0, -1)
        };

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                HelpFile();
                return;
            }
            try
            {
                var config = new ColorGeneratorConfig
                {
                    XLength = int.Parse(args[0]),
                    YLength = int.Parse(args[1])
                };

                Console.WriteLine("Starting image generation with:");
                Console.WriteLine($"\tDimensions:\t\t{config.XLength} X {config.YLength}");
                Console.WriteLine($"\tSteps Per Channel:\t{config.NumOfSteps}");
                Console.WriteLine($"\tStep Size:\t\t{config.ColorStep}");
                Console.WriteLine($"\tSteps to Skip:\t\t{config.StepsToSkip}\n");

                var runner = new TaskRunner();
                var colors = runner.Run(() => GenerateColorList(config), "color selection");
                var pixels = runner.Run(() => BuildPixelArray(colors, config), "pixel array generation");
                runner.Run(() => OutputBitmap(pixels, config), "bitmap creation");
            }
            catch (Exception ex)
            {
               HelpFile("There was an issue in execution");
            }
            
            Console.ReadLine();
        }

        private static void HelpFile(string errorMessage = "")
        {
            const string Header = "Generates an image with every pixel having a unique color";
            Console.WriteLine(errorMessage == string.Empty ? Header : $"An error has occured: {errorMessage}\n Ensure the Arguments you have provided are valid");
            Console.WriteLine();
            Console.WriteLine($"{AppDomain.CurrentDomain.FriendlyName} X Y");
            Console.WriteLine();
            Console.WriteLine("X\t\tThe Length of the X dimension eg: 256");
            Console.WriteLine("Y\t\tThe Length of the Y dimension eg: 128");
        }

        public static List<Color> GenerateColorList(ColorGeneratorConfig config)
        {

            //Every iteration of our color generation loop will add the iterationfactor to this accumlator which is used to know when to skip
            decimal iterationAccumulator = 0;

            var colors = new List<Color>();
            for (var r = 0; r < config.NumOfSteps; r++)
                for (var g = 0; g < config.NumOfSteps; g++)
                    for (var b = 0; b < config.NumOfSteps; b++)
                    {
                        iterationAccumulator += config.IterationFactor;

                        //If our accumulator has reached 1, then subtract one and skip this iteration
                        if (iterationAccumulator > 1)
                        {
                            iterationAccumulator -= 1;
                            continue;
                        }

                        colors.Add(Color.FromArgb(r*config.ColorStep, g*config.ColorStep,b*config.ColorStep));
                    }
            return colors;
        }

        public static Color?[,] BuildPixelArray(List<Color> colors, ColorGeneratorConfig config)
        {
            //Get a random color to start with.
            var random = new Random(Guid.NewGuid().GetHashCode());
            var nextColor = colors[random.Next(colors.Count)];

            var pixels = new Color?[config.XLength, config.YLength];
            var currPixel = new Point(0, 0);

            var i = 0;

            //Since we've only generated exactly enough colors to fill our image we can remove them from the list as we add them to our image and stop when none are left.
            while (colors.Count > 0)
            {
                i++;

                //Set the current pixel and remove the color from the list.
                pixels[currPixel.X, currPixel.Y] = nextColor;
                colors.RemoveAt(colors.IndexOf(nextColor));

                //Our image generation works in an inward spiral generation GetNext point will retrieve the next pixel given the current top direction.
                var nextPixel = GetNextPoint(currPixel, directions.First());

                //If this next pixel were to be out of bounds (for first circle of spiral) or hit a previously generated pixel (for all other circles)
                //Then we need to cycle the direction and get a new next pixel
                if (nextPixel.X >= config.XLength || nextPixel.Y >= config.YLength || nextPixel.X < 0 || nextPixel.Y < 0 ||
                    pixels[nextPixel.X, nextPixel.Y] != null)
                {
                    var d = directions.First();
                    directions.RemoveAt(0);
                    directions.Add(d);
                    nextPixel = GetNextPoint(currPixel, directions.First());
                }

                //This code sets the pixel to pick a color for and also gets the next color
                //We do this at the end of the loop so that we can also support haveing the first pixel set outside of the loop
                currPixel = nextPixel;

                if (colors.Count == 0) continue;

                var neighbours = GetNeighbours(currPixel, pixels, config);
                nextColor = colors.AsParallel().Aggregate((item1, item2) => GetAvgColorDiff(item1, neighbours) <
                                                                            GetAvgColorDiff(item2, neighbours)
                    ? item1
                    : item2);
            }

            return pixels;
        }

        public static void OutputBitmap(Color?[,] pixels, ColorGeneratorConfig config)
        {
            //Now that we have generated our image in the color array we need to copy it into a bitmap and save it to file.
            var image = new Bitmap(config.XLength, config.YLength);

            for (var x = 0; x < config.XLength; x++)
                for (var y = 0; y < config.YLength; y++)
                    image.SetPixel(x, y, pixels[x, y].Value);

            using (var file = new FileStream($@".\{config.XLength}X{config.YLength}.png", FileMode.Create))
            {
                image.Save(file, ImageFormat.Png);
            }
        }

        static Point GetNextPoint(Point current, Point direction)
        {
            return new Point(current.X + direction.X, current.Y + direction.Y);
        }

        static List<Color> GetNeighbours(Point current, Color?[,] grid, ColorGeneratorConfig config)
        {
            var list = new List<Color>();
            foreach (var direction in directions)
            {
                var xCoord = current.X + direction.X;
                var yCoord = current.Y + direction.Y;
                if (xCoord < 0 || xCoord >= config.XLength|| yCoord < 0 || yCoord >= config.YLength)
                {
                    continue;
                }
                var cell = grid[xCoord, yCoord];
                if (cell.HasValue) list.Add(cell.Value);
            }
            return list;
        }

        static double GetAvgColorDiff(Color source, IList<Color> colors)
        {
            return colors.Average(color => GetColorDiff(source, color));
        }

        static int GetColorDiff(Color color1, Color color2)
        {
            var redDiff = Math.Abs(color1.R - color2.R);
            var greenDiff = Math.Abs(color1.G - color2.G);
            var blueDiff = Math.Abs(color1.B - color2.B);
            return redDiff + greenDiff + blueDiff;
        }
    }

    public class ColorGeneratorConfig
    {
        public int XLength { get; set; }
        public int YLength { get; set; }

        //Get the number of permutations for each color value base on the required number of pixels.
        public int NumOfSteps
            => (int)Math.Ceiling(Math.Pow((ulong)XLength * (ulong)YLength, 1.0 / ColorDimensions));

        //Calculate the increment for each step
        public int ColorStep
            => 255 / (NumOfSteps - 1);

        //Because NumOfSteps will either give the exact number of colors or more (never less) we will sometimes to to skip some
        //this calculation tells how many we need to skip
        public decimal StepsToSkip
            => Convert.ToDecimal(Math.Pow(NumOfSteps, ColorDimensions) - XLength * YLength);

        //This factor will be used to as evenly as possible spread out the colors to be skipped so there are no large gaps in the spectrum
        public decimal IterationFactor => StepsToSkip / Convert.ToDecimal(Math.Pow(NumOfSteps, ColorDimensions));

        private double ColorDimensions => 3.0;
    }

    public class TaskRunner
    {
        private Stopwatch _sw;
        public TaskRunner()
        {
            _sw = new Stopwatch();
        }

        public void Run(Action task, string taskName)
        {
            Console.WriteLine($"Starting {taskName}...");
            _sw.Start();
            task();
            _sw.Stop();
            Console.WriteLine($"Finished {taskName}. Elapsed(ms): {_sw.ElapsedMilliseconds}");
            Console.WriteLine();
            _sw.Reset();
        }

        public T Run<T>(Func<T> task, string taskName)
        {
            Console.WriteLine($"Starting {taskName}...");
            _sw.Start();
            var result = task();
            _sw.Stop();
            Console.WriteLine($"Finished {taskName}. Elapsed(ms): {_sw.ElapsedMilliseconds}");
            Console.WriteLine();
            _sw.Reset();
            return result;
        }
    }
}