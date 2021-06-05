using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Utils;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Rendering;
using Avalonia.Input;
using System.Reactive.Linq;
using System.Reactive;
using Avalonia.Reactive;
using System;

namespace AvaloniaDiverseAndConnectedTiles
{
    public class Reveal : Control
    {
        LinearGradientBrush _brush = new LinearGradientBrush();

        static GradientStops REVEAL_STOPS = new GradientStops()
        {
            new GradientStop(Colors.Black, 0.5),
            new GradientStop(Colors.Transparent, 1)
        };


        RelativePoint _revealOffscreenCenter = new RelativePoint(-99999999, -99999999, RelativeUnit.Absolute);

        public static readonly AttachedProperty<GradientStops> RevealStopsProperty =
                AvaloniaProperty.RegisterAttached<Reveal, Control, GradientStops>(nameof(RevealStops));
        
        public GradientStops RevealStops
        {
            get => GetValue(RevealStopsProperty);
            set => SetValue(RevealStopsProperty, value);
        }

        public static readonly StyledProperty<double> RadiusXProperty =
                AvaloniaProperty.Register<Reveal, double>(nameof(RadiusX));
        
        public double RadiusX
        {
            get => GetValue(RadiusXProperty);
            set => SetValue(RadiusXProperty, value);
        }

        public static readonly StyledProperty<double> RadiusYProperty =
                AvaloniaProperty.Register<Reveal, double>(nameof(RadiusY));
        
        public double RadiusY
        {
            get => GetValue(RadiusYProperty);
            set => SetValue(RadiusYProperty, value);
        }

        public static readonly StyledProperty<double> RevealStrokeThicknessProperty =
                AvaloniaProperty.Register<Reveal, double>(nameof(RevealStrokeThickness));
        
        public double RevealStrokeThickness
        {
            get => GetValue(RevealStrokeThicknessProperty);
            set => SetValue(RevealStrokeThicknessProperty, value);
        }

        public override void Render(DrawingContext context)
        {
            if (_root != null)
            {
                context.DrawRectangle(null, new Pen(_brush, RevealStrokeThickness), Bounds.WithX(0).WithY(0), RadiusX, RadiusY);
            }
        }

        static Reveal()
        {
            AffectsRender<Reveal>(OpacityMaskProperty, RevealStopsProperty);
        }

        public Reveal() : base()
        {
            OpacityMask = new RadialGradientBrush()
            {
                GradientStops = REVEAL_STOPS,
                Center = _revealOffscreenCenter,
                [!RadialGradientBrush.RadiusProperty] = this.GetObservable(BoundsProperty).Select(x => (224 / Math.Max(x.Width, x.Height)) / 2).ToBinding()
            };
            (OpacityMask as RadialGradientBrush)[!RadialGradientBrush.GradientOriginProperty] = (OpacityMask as RadialGradientBrush).GetObservable(RadialGradientBrush.CenterProperty).ToBinding();

            _brush[!LinearGradientBrush.GradientStopsProperty] = this.GetObservable(RevealStopsProperty).ToBinding();
        }

        IControl _root = null;
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            if (e.Root is IControl rootCtrl)
            {
                _root = rootCtrl;
                _root.PointerEnter += RootVisual_PointerMoved;
                _root.PointerMoved += RootVisual_PointerMoved;
                _root.PointerLeave += RootVisual_PointerLeave;
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            if (e.Root is IControl rootCtrl)
            {
                rootCtrl.PointerEnter -= RootVisual_PointerMoved;
                rootCtrl.PointerMoved -= RootVisual_PointerMoved;
            }
            _root = null;
        }

        void RootVisual_PointerMoved(object sender, PointerEventArgs e)
        {
            var curPos = e.GetPosition(this);
            var pos = new RelativePoint(curPos, RelativeUnit.Absolute);
            (OpacityMask as RadialGradientBrush).Center = pos;

            double halfWidth = Bounds.Width / 2;
            double halfHeight = Bounds.Height / 2;
            
            double curX = curPos.X / 5;
            double curY = curPos.Y / 5;
            
            _brush.StartPoint = new RelativePoint(curX, curY, RelativeUnit.Absolute);
            _brush.EndPoint = new RelativePoint(Bounds.Width + curX, Bounds.Height + curY, RelativeUnit.Absolute);
        }

        void RootVisual_PointerLeave(object sender, PointerEventArgs e)
        {
            (OpacityMask as RadialGradientBrush).Center = _revealOffscreenCenter;
        }
    }
}