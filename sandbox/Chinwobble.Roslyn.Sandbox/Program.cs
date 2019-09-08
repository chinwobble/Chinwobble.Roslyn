using Chinwobble.Roslyn.PropertyChanged;
using System;
using Xamarin.Forms;

namespace Chinwobble.Roslyn.Sandbox
{
    [GenerateNotifyPropertyChanged]
    public partial class Program : BindableObject
    {
        private string _test;
        private Program a, b;
        private string _d, c;
        public Program()
        {
            var result = this.Test;
            // this.Test
            // this.Id;
        }
        static void Main(string[] args)
        {
            
            Console.WriteLine("Hello World!");
        }
    }
}
