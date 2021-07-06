using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CEtrainer_extract
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Critical("CEForm-extract by VollRagm");

            if (args.Length <= 0) Log.Fatal("Please provide the extract CETrainer XML!");

            FormHandler.DecompressForms(args[0]);
            ScriptHandler.HandleScripts(args[0]);

            Console.ReadLine();
        }
    }
}
