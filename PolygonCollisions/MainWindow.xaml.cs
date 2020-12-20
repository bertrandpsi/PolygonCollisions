/*
 * Use WASD to move the pentagone
 * From https://github.com/OneLoneCoder/olcPixelGameEngine/blob/master/Videos/OneLoneCoder_PGE_PolygonCollisions1.cpp as basis
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;

namespace PolygonCollisions
{
    /// <summary>
    /// Collision checking code (CONVEX ONLY!)
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Equilateral> shapes = new List<Equilateral>();
        Dictionary<Key, bool> activeKeys = new Dictionary<Key, bool>();
        const double rotationSpeed = Math.PI / 40;
        const double movementSpeed = 5;

        /// <summary>
        /// Creates 3 shapes and place them on the world.
        /// Initialize also the game loop.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            var triangle = new Equilateral(3, 50, new Point(100, 100));
            shapes.Add(triangle);
            DrawArea.Children.Add(triangle);


            var s2 = new Equilateral(5, 80, new Point(200, 200));
            shapes.Add(s2);
            DrawArea.Children.Add(s2);

            var s3 = new Equilateral(4, 70, new Point(100, 300));
            shapes.Add(s3);
            DrawArea.Children.Add(s3);

            // Game loop creation
            var gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
        }

        class CoupleChecked
        {
            public Equilateral A { get; set; }
            public Equilateral B { get; set; }

            public bool IsSame(CoupleChecked other)
            {
                return ((this.A == other.A && this.B == other.B) || (this.A == other.B && this.B == other.A));
            }
        }

        /// <summary>
        /// Runs every 20 miliseconds and handles the keyboard interaction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop(object sender, EventArgs e)
        {
            if (IsKeyDown(Key.A))
                shapes[1].Rotation -= rotationSpeed;
            else if (IsKeyDown(Key.D))
                shapes[1].Rotation += rotationSpeed;
            if (IsKeyDown(Key.W))
                shapes[1].Move(movementSpeed);


            shapes.ForEach(s => s.InCollision = false);
            foreach (var s1 in shapes)
            {
                foreach (var s2 in shapes)
                {
                    if (s1 == s2)
                        continue;

                    var isColliding = s1.CheckCollision(s2);
                    if (isColliding)
                    {
                        s1.InCollision = true;
                        s2.InCollision = true;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a key is down
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool IsKeyDown(Key key)
        {
            lock (activeKeys)
            {
                return (activeKeys.ContainsKey(key) && activeKeys[key]);
            }
        }

        /// <summary>
        /// Event handled KeyDown => stores the key down in the activeKey dictionary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            lock (activeKeys)
            {
                if (!activeKeys.ContainsKey(e.Key))
                    activeKeys.Add(e.Key, false);
                activeKeys[e.Key] = true;
            }
        }

        /// <summary>
        /// Event handled KeyUp => stores the key up in the activeKey dictionary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            lock (activeKeys)
            {
                if (!activeKeys.ContainsKey(e.Key))
                    activeKeys.Add(e.Key, false);
                activeKeys[e.Key] = false;
            }
        }
    }
}
