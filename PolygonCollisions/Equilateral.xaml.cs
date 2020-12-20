using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PolygonCollisions
{
    /// <summary>
    /// Equilateral Polygon
    /// </summary>
    public partial class Equilateral : UserControl
    {
        /// <summary>
        /// Contains all the lines used to draw the polygon
        /// </summary>
        private Line[] Lines { get; }
        /// <summary>
        /// Contains the base Vertexes of the non-rotated polygon
        /// </summary>
        public Point[] Vertexes { get; }
        /// <summary>
        /// Contains the center (world space) of the polygon
        /// </summary>
        public Point Center { get; set; } = new Point(0, 0);

        /// <summary>
        /// Handle the rotation of the polygon
        /// </summary>
        double rotation = 0;
        public double Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
                CalculatePoints();
                UpdateVertexes();
            }
        }

        /// <summary>
        /// Flags is the polygon is currently in collision with another polygon
        /// </summary>
        bool inCollision = false;
        public bool InCollision
        {
            get { return inCollision; }
            set
            {
                inCollision = value;
                var c = inCollision ? Brushes.Red : Brushes.Black;
                foreach (var l in Lines)
                    l.Stroke = c;
            }
        }

        /// <summary>
        /// Contains the rotated vertexes
        /// </summary>
        public Point[] CalculatedVertexes { get; }

        /// <summary>
        /// Contains the radius if the polygon
        /// </summary>
        public double Radius { get; }

        /// <summary>
        /// Creates an equilateral polygon of nbSegments of the given radius and at the initialPosition as center
        /// </summary>
        /// <param name="nbSegments"></param>
        /// <param name="radius"></param>
        /// <param name="initialPosition"></param>
        public Equilateral(int nbSegments, int radius, Point initialPosition)
        {
            InitializeComponent();

            var a = Math.PI * 2 / nbSegments;
            Radius = radius;

            Center = initialPosition;

            Vertexes = Enumerable.Range(0, nbSegments)
                .Select(r => new Point
                {
                    X = Math.Cos(a * r) * radius,
                    Y = Math.Sin(a * r) * radius
                }).ToArray();
            CalculatedVertexes = new Point[nbSegments];
            Lines = new Line[nbSegments + 1];
            CalculatePoints();
            PlaceVertexes();
            this.Width = radius * 2;
            this.Height = radius * 2;
            Place();
        }

        /// <summary>
        /// Rotates the points given the current Rotation
        /// </summary>
        private void CalculatePoints()
        {
            var cos = Math.Cos(Rotation);
            var sin = Math.Sin(Rotation);
            for (var i = 0; i < Vertexes.Length; i++)
            {
                CalculatedVertexes[i].X = Vertexes[i].X * cos - Vertexes[i].Y * sin + Radius;
                CalculatedVertexes[i].Y = Vertexes[i].X * sin + Vertexes[i].Y * cos + Radius;
            }
        }

        /// <summary>
        /// Creates the lines to draw the polygon and place them
        /// </summary>
        private void PlaceVertexes()
        {
            Line l;
            for (var i = 0; i < CalculatedVertexes.Length; i++)
            {
                var p1 = CalculatedVertexes[i];
                var p2 = CalculatedVertexes[(i + 1) % CalculatedVertexes.Length];
                l = new Line { X1 = p1.X, Y1 = p1.Y, X2 = p2.X, Y2 = p2.Y, Stroke = Brushes.Black, StrokeThickness = 2 };
                Lines[i] = l;
                ShapeCanvas.Children.Add(l);
            }

            // Add a pointing line
            l = new Line { X1 = Radius, Y1 = Radius, X2 = CalculatedVertexes[0].X, Y2 = CalculatedVertexes[0].Y, Stroke = Brushes.Black, StrokeThickness = 1 };
            Lines[CalculatedVertexes.Length] = l;
            ShapeCanvas.Children.Add(l);
        }

        /// <summary>
        /// Updates the lines to reflect the new rotated vertexes
        /// </summary>
        private void UpdateVertexes()
        {
            for (var i = 0; i < CalculatedVertexes.Length; i++)
            {
                var p1 = CalculatedVertexes[i];
                var p2 = CalculatedVertexes[(i + 1) % CalculatedVertexes.Length];
                Lines[i].X1 = p1.X;
                Lines[i].Y1 = p1.Y;
                Lines[i].X2 = p2.X;
                Lines[i].Y2 = p2.Y;
            }

            // Add a pointing line
            Lines[CalculatedVertexes.Length].X2 = CalculatedVertexes[0].X;
            Lines[CalculatedVertexes.Length].Y2 = CalculatedVertexes[0].Y;
        }

        /// <summary>
        /// Place the polygon into the correct parrent coordinates
        /// </summary>
        private void Place()
        {
            this.Margin = new Thickness
            {
                Left = Center.X - Radius,
                Top = Center.Y - Radius,
                Bottom = 0,
                Right = 0
            };
        }

        /// <summary>
        /// Moves the polygon a given ammount
        /// </summary>
        /// <param name="ammount"></param>
        public void Move(double ammount)
        {
            var sin = Math.Sin(Rotation);
            var cos = Math.Cos(Rotation);

            Center = new Point(Center.X + cos * ammount, Center.Y + sin * ammount);
            CalculatePoints();
            UpdateVertexes();
            Place();
        }

        private Point[] TranslatedPoints
        {
            get
            {
                return CalculatedVertexes.Select(p => new Point(p.X + Center.X - Radius, p.Y + Center.Y - Radius)).ToArray();
            }
        }

        /// <summary>
        /// Collision Detection Using Separating Axis Theorem
        /// </summary>
        /// <param name="other"></param>
        /// <param name="recurse"></param>
        /// <returns></returns>
        public bool CheckCollision(Equilateral other, bool recurse = true)
        {
            var thisTranslated = TranslatedPoints;
            var otherTranslated = other.TranslatedPoints;

            // Small optimization
            // Check the boundry boxes, to avoid to check the whole object, if the broundry boxes don't collide we are sure the objects don't collides either
            if (recurse)
            {
                var sThisMin = thisTranslated.Min(p => p.X);
                var sThisMax = thisTranslated.Max(p => p.X);
                var sOtherMin = otherTranslated.Min(p => p.X);
                var sOtherMax = otherTranslated.Max(p => p.X);
                if (sThisMin > sOtherMax || sOtherMin > sOtherMax)
                    return false;

                sThisMin = thisTranslated.Min(p => p.Y);
                sThisMax = thisTranslated.Max(p => p.Y);
                sOtherMin = otherTranslated.Min(p => p.Y);
                sOtherMax = otherTranslated.Max(p => p.Y);
                if (sThisMin > sOtherMax || sOtherMin > sOtherMax)
                    return false;
            }

            for (var a = 0; a < thisTranslated.Length; a++)
            {
                var b = (a + 1) % thisTranslated.Length;
                var v = new Vector(-(thisTranslated[b].Y - thisTranslated[a].Y), thisTranslated[b].X - thisTranslated[a].X);
                var l = v.Length;
                v = new Vector(v.X / l, v.Y / l);

                var proj = thisTranslated.Select(r => (r.X * v.X + r.Y * v.Y)).ToList();
                var minThis = proj.Min();
                var maxThis = proj.Max();

                proj = otherTranslated.Select(r => (r.X * v.X + r.Y * v.Y)).ToList();
                var minOther = proj.Min();
                var maxOther = proj.Max();

                if (!(maxOther >= minThis && maxThis >= minOther))
                    return false;
            }

            if (recurse && other.CheckCollision(this, false) == false)
                return false;

            return true;
        }
    }
}
