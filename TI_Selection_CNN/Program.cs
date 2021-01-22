using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TI_Selection_CNN
{
    class Program
    {
        static void Main(string[] args)
        {
            //(Batch calculation) Firstly, the candidate training image is preprocessed, 
            //and then the CNN recognition program EXE of another project is called
            TI_Selection.Run();
        }
    }
}
