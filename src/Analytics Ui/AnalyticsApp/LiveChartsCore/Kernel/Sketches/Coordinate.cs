//using LiveChartsCore;
//using LiveChartsCore.Defaults;
//using LiveChartsCore.SkiaSharpView;
//using LiveChartsCore.SkiaSharpView.Painting;
//using SkiaSharp;
//using System.Collections.ObjectModel;

namespace LiveChartsCore.Kernel.Sketches
{
    internal class Coordinate
    {
        private double v;
        private double? value;

        public Coordinate(double v, double? value)
        {
            this.v = v;
            this.value = value;
        }
    }
}
