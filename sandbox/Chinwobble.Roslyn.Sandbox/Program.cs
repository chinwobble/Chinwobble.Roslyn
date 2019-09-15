using Chinwobble.Roslyn.PropertyChanged;
using System;
using Xamarin.Forms;

namespace Chinwobble.Roslyn.Sandbox
{
    [GenerateNotifyPropertyChanged]
    public partial class Program : BindableObject
    {
        private string _title;
        private string _name;
        
        public Program()
        {
            var result = this.Name;
        }
        static void Main(string[] args)
        {
            
            Console.WriteLine("Hello World!");
        }
    }
}
