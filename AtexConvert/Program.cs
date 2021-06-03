using System;

namespace AtexConvert
{
    class Program
    {
        // Other code adapted from https://github.com/0ceal0t/Dalamud-VFXEditor
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: atexconvert <input>.dds <output>.atex");
                Environment.Exit(1);
            }

            var imported = TextureManager.ImportTexture(args[0], args[1]);
            if (!imported)
            {
                Console.Error.WriteLine("Failed to convert");
                Environment.Exit(2);
            }
        }
    }
}