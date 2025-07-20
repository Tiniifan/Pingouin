using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Pingouin.Controls
{
    // CircularProgressBar logic inspired by Ali Tor - StackOverflow
    public partial class CircularProgressBar : ProgressBar
    {
        public CircularProgressBar()
        {
            this.ValueChanged += CircularProgressBar_ValueChanged;
        }

        void CircularProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CircularProgressBar bar = sender as CircularProgressBar;
            if (bar == null) return;

            double currentAngle = bar.Angle;
            double targetAngle = e.NewValue / bar.Maximum * 359.999; // Avoid rendering glitch at exactly 360°

            // Animate the transition between current and target angle
            DoubleAnimation anim = new DoubleAnimation(currentAngle, targetAngle, TimeSpan.FromMilliseconds(250));
            bar.BeginAnimation(CircularProgressBar.AngleProperty, anim, HandoffBehavior.SnapshotAndReplace);
        }

        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(CircularProgressBar), new PropertyMetadata(0.0));

        public double StrokeThickness
        {
            get { return (double)GetValue(StrokeThicknessProperty); }
            set { SetValue(StrokeThicknessProperty, value); }
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
            DependencyProperty.Register("StrokeThickness", typeof(double), typeof(CircularProgressBar), new PropertyMetadata(8.0));
    }
}