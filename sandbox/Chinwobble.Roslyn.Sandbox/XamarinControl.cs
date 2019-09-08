using Chinwobble.Roslyn.PropertyChanged;
using Xamarin.Forms;

namespace Chinwobble.Roslyn.Sandbox
{
    [GenerateDependencyProperty]
    public partial class XamarinControl : ContentView
    {
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(
                returnType: typeof(int),
                propertyName: "Command",
                declaringType: typeof(XamarinControl),
                defaultValue: null);
        public XamarinControl()
        {

            var x = this.Command;
            Command = x;
            // var x = this.Command;
        }

    }
}
