using Chinwobble.Roslyn.PropertyChanged;
using System.Windows;

namespace Chinwobble.Roslyn.Sandbox
{
    [GenerateDependencyProperty]
    public partial class ChartControl : System.Windows.Controls.Control
    {
        public static readonly DependencyProperty YScalePrecisionProperty =
            DependencyProperty.Register(
                name: "YScalePrecision",
                propertyType: typeof(int),
                ownerType: typeof(ChartControl));

        public static readonly DependencyProperty XScalePrecisionProperty =
            DependencyProperty.Register(
                name: "XScalePrecision",
                propertyType: typeof(int),
                ownerType: typeof(ChartControl),
                typeMetadata: new PropertyMetadata(defaultValue: 2));

        public static readonly DependencyProperty ZScalePrecisionProperty =
            DependencyProperty.Register(
                name: "ZScalePrecision",
                propertyType: typeof(int),
                ownerType: typeof(ChartControl),
                typeMetadata: new PropertyMetadata(defaultValue: 2));
        public ChartControl()
        {
            this.ZScalePrecision = 0;
            this.YScalePrecision = 1;
            this.XScalePrecision = 2;
        }
    }
}
