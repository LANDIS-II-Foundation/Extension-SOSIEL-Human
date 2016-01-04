using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ChartSurfaceControl
{
    [TemplatePart(Name = AreaCanvas, Type = typeof (Canvas))]
    [TemplatePart(Name = AreaBorder, Type = typeof (Border))]
    public class ChartSurfaceControl : Control
    {
        #region constructors

        public ChartSurfaceControl()
        {
            DefaultStyleKey = typeof (ChartSurfaceControl);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            OnValueChanged(this);
            base.OnApplyTemplate();
        }

        #region constants

        private const int _range = 10;
        public const string AreaCanvas = "AreaCanvas";
        public const string AreaBorder = "AreaBorder";

        #endregion

        #region Dependency Properties

        public ObservableCollection<LineModel> LineData
        {
            get { return (ObservableCollection<LineModel>) GetValue(LineDataProperty); }
            set { SetValue(LineDataProperty, value); }
        }

        public static readonly DependencyProperty LineDataProperty =
            DependencyProperty.Register("LineData", typeof (ObservableCollection<LineModel>),
                typeof (ChartSurfaceControl), new PropertyMetadata(OnValueChanged));

        public double LineThickness
        {
            get { return (double) GetValue(LineThicknessProperty); }
            set { SetValue(LineThicknessProperty, value); }
        }

        public static readonly DependencyProperty LineThicknessProperty =
            DependencyProperty.Register("LineThickness", typeof (double), typeof (ChartSurfaceControl),
                new PropertyMetadata(1d, OnValueChanged));

        public Brush LineBrush
        {
            get { return (Brush) GetValue(LineBrushProperty); }
            set { SetValue(LineBrushProperty, value); }
        }

        public static readonly DependencyProperty LineBrushProperty =
            DependencyProperty.Register("LineBrush", typeof (Brush), typeof (ChartSurfaceControl),
                new PropertyMetadata(Brushes.Black, OnValueChanged));

        public string FontFamily
        {
            get { return (string) GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public static readonly DependencyProperty FontFamilyProperty =
            DependencyProperty.Register("FontFamily", typeof (string), typeof (ChartSurfaceControl),
                new PropertyMetadata("Arial", OnValueChanged));

        public double FontSize
        {
            get { return (double) GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register("FontSize", typeof (double), typeof (ChartSurfaceControl),
                new PropertyMetadata(10d, OnValueChanged));

        public string XAxisTitle
        {
            get { return (string) GetValue(XAxisTitleProperty); }
            set { SetValue(XAxisTitleProperty, value); }
        }

        public static readonly DependencyProperty XAxisTitleProperty =
            DependencyProperty.Register("XAxisTitle", typeof (string), typeof (ChartSurfaceControl),
                new PropertyMetadata("XAxis", OnValueChanged));

        public string YAxisTitle
        {
            get { return (string) GetValue(YAxisTitleProperty); }
            set { SetValue(YAxisTitleProperty, value); }
        }

        public static readonly DependencyProperty YAxisTitleProperty =
            DependencyProperty.Register("YAxisTitle", typeof (string), typeof (ChartSurfaceControl),
                new PropertyMetadata("YAxis", OnValueChanged));

        public ChartType ChartType
        {
            get { return (ChartType) GetValue(ChartTypeProperty); }
            set { SetValue(ChartTypeProperty, value); }
        }

        public static readonly DependencyProperty ChartTypeProperty =
            DependencyProperty.Register("ChartType", typeof (ChartType), typeof (ChartSurfaceControl),
                new PropertyMetadata(ChartType.Linear, OnValueChanged));

        public bool ShowGridLines
        {
            get { return (bool) GetValue(ShowGridLinesProperty); }
            set { SetValue(ShowGridLinesProperty, value); }
        }

        public static readonly DependencyProperty ShowGridLinesProperty =
            DependencyProperty.Register("ShowGridLines", typeof (bool), typeof (ChartSurfaceControl),
                new PropertyMetadata(true, OnValueChanged));

        public Brush GridLineBrush
        {
            get { return (Brush) GetValue(GridLineBrushProperty); }
            set { SetValue(GridLineBrushProperty, value); }
        }

        public static readonly DependencyProperty GridLineBrushProperty =
            DependencyProperty.Register("GridLineBrush", typeof (Brush), typeof (ChartSurfaceControl),
                new PropertyMetadata(Brushes.LightGray, OnValueChanged));

        #endregion

        #region OnValueChanged

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == LineDataProperty
                || e.Property == LineBrushProperty
                || e.Property == ShowGridLinesProperty
                || e.Property == LineThicknessProperty
                || e.Property == ChartTypeProperty)
            {
                OnValueChanged(d);
            }
        }

        private static void OnValueChanged(DependencyObject d)
        {
            var c = (ChartSurfaceControl) d;
            var areaCanvas = c.GetTemplateChild(AreaCanvas) as Canvas;
            var areaBorder = c.GetTemplateChild(AreaBorder) as Border;
            if (areaCanvas != null && areaBorder != null)
            {
                areaCanvas.Children.Clear();
                if (c.ShowGridLines)
                {
                    for (var u = 0; u < 3; u++)
                    {
                        var hLine = new Line
                        {
                            Visibility = Visibility.Visible,
                            StrokeThickness = 1,
                            Stroke = c.GridLineBrush,
                            X1 = (u + 1)*(areaBorder.Width/4),
                            X2 = (u + 1)*(areaBorder.Width/4),
                            Y1 = 0,
                            Y2 = areaBorder.Height
                        };
                        var vLine = new Line
                        {
                            Visibility = Visibility.Visible,
                            StrokeThickness = 1,
                            Stroke = c.GridLineBrush,
                            Y1 = (u + 1)*(areaBorder.Height/4),
                            Y2 = (u + 1)*(areaBorder.Height/4),
                            X1 = 0,
                            X2 = areaBorder.Width
                        };
                        areaCanvas.Children.Add(hLine);
                        areaCanvas.Children.Add(vLine);
                    }
                }

                switch (c.ChartType)
                {
                    case ChartType.Linear:
                        if (c.LineData != null)
                            foreach (var lineModel in c.LineData)
                            {
                                for (var u = 0; u < lineModel.Contributions.Count-1; u++)
                                {
                                    var line = new Line
                                    {
                                        Visibility = Visibility.Visible,
                                        StrokeThickness = c.LineThickness,
                                        Stroke = c.LineBrush,
                                        X1 = u*(areaBorder.Width/c.LineData.Count),
                                        X2 = (u + 1)*(areaBorder.Width/c.LineData.Count),
                                        Y1 = areaBorder.Height - lineModel.Contributions[u]*(areaBorder.Height/_range),
                                        Y2 = areaBorder.Height -
                                             lineModel.Contributions[u + 1]*(areaBorder.Height/_range)
                                    };

                                    areaCanvas.Children.Add(line);
                                }
                            }

                        break;

                    //case ChartType.Squared:

                    //    for (var u = 0; u < c.LineData.Count - 1; u++)
                    //    {
                    //        var rect = new Rectangle();
                    //        rect.Visibility = Visibility.Visible;
                    //        rect.StrokeThickness = c.LineThickness;
                    //        rect.Fill = c.LineBrush;
                    //        rect.Width = (areaBorder.Width/c.LineData.Count);
                    //        rect.Height = c.LineData[u]*(areaBorder.Height/_range);
                    //        Canvas.SetLeft(rect, u*(areaBorder.Width/c.LineData.Count));
                    //        Canvas.SetTop(rect, areaBorder.Height - c.LineData[u]*(areaBorder.Height/_range));
                    //        areaCanvas.Children.Add(rect);
                    //    }
                    //    break;
                }
            }
        }

        #endregion
    }

    public class LineModel
    {
        public string Name { get; set; }
        public List<double> Contributions { get; set; }
    }
}