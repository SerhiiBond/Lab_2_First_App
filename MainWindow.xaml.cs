using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Math;



namespace Lab_2_First_App

{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static DispatcherTimer dT;
        static int _radius = 30;
        static int _pointCount = 5;
        static string _algoritm = "Жадібний";
        static Polygon _myPolygon = new Polygon();
        static List<Ellipse> _ellipseArray = new List<Ellipse>();
        static PointCollection _pC = new PointCollection();
        public int _count;
        static int[,] parents;


        public MainWindow()
        {
            InitMainWindow();
        }

        private void InitMainWindow()
        {
            dT = new DispatcherTimer();

            InitializeComponent();
            InitPoints();
            InitPolygon();

            dT = new DispatcherTimer();
            dT.Tick += new EventHandler(OneStep);
            dT.Interval = new TimeSpan(0, 0, 0, 0, 1000);
        }

        private void InitPoints()
        {
            Random rnd = new Random();
            _pC.Clear();
            _ellipseArray.Clear();

            for (int i = 0; i < _pointCount; i++)
            {
                Point p = new Point();

                p.X = rnd.Next(_radius, (int)(0.75 * MainWin.Width) - 3 * _radius);
                p.Y = rnd.Next(_radius, (int)(0.90 * MainWin.Height - 3 * _radius));
                _pC.Add(p);
            }

            for (int i = 0; i < _pointCount; i++)
            {
                Ellipse el = new Ellipse();

                el.StrokeThickness = 2;
                el.Height = el.Width = _radius;
                el.Stroke = Brushes.Black;

                if (i == 0) el.Fill = Brushes.Gold;
                else el.Fill = Brushes.LightBlue;
                _ellipseArray.Add(el);
            }
        }

        private void InitPolygon()
        {
            _myPolygon.Stroke = System.Windows.Media.Brushes.Black;
            _myPolygon.StrokeThickness = 2;
        }

        private void PlotPoints()
        {
            for (int i = 0; i < _pointCount; i++)
            {
                Canvas.SetLeft(_ellipseArray[i], _pC[i].X - _radius / 2);
                Canvas.SetTop(_ellipseArray[i], _pC[i].Y - _radius / 2);
                MyCanvas.Children.Add(_ellipseArray[i]);
            }
        }


        private void PlotWay(int[] BestWayIndex)
        {
            PointCollection Points = new PointCollection();

            for (int i = 0; i < BestWayIndex.Length; i++)
                Points.Add(_pC[BestWayIndex[i]]);

            _myPolygon.Points = Points;
            MyCanvas.Children.Add(_myPolygon);
        }

        private void VelCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox CB = (ComboBox)e.Source;
            ListBoxItem item = (ListBoxItem)CB.SelectedItem;

            dT.Interval = new TimeSpan(0, 0, 0, 0, Convert.ToInt16(item.Content));
        }

        private void StopStart_Click(object sender, RoutedEventArgs e)
        {
            if (dT.IsEnabled)
            {
                dT.Stop();
                NumElemCB.IsEnabled = true;
                InitMainWindow();
            }
            else
            {
                NumElemCB.IsEnabled = false;
                dT.Start();
            }
        }

        private void NumElemCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox CB = (ComboBox)e.Source;
            ListBoxItem item = (ListBoxItem)CB.SelectedItem;

            _pointCount = Convert.ToInt32(item.Content);
            InitPoints();
            InitPolygon();
        }
        private void OneStep(object sender, EventArgs e)
        {
            MyCanvas.Children.Clear();
            //InitPoints();
            PlotPoints();
            if (_algoritm == "Жадібний")
            {
                PlotWay(GetBestWay1());
            }
            else
            {

                CountTB.Text = Convert.ToString(_count);
                PlotWay(GetBestWay2());
                
            } 
               

        }

        /*private int[] GetBestWay()
        {
            Random rnd = new Random();
            int[] way = new int[PointCount];

            for (int i = 0; i < PointCount; i++)
                way[i] = i;

            for (int s = 0; s < 2 * PointCount; s++)
            {
                int i1, i2, tmp;

                i1 = rnd.Next(PointCount);
                i2 = rnd.Next(PointCount);
                tmp = way[i1];
                way[i1] = way[i2];
                way[i2] = tmp;
            }

            return way;
        }*/
        private int[] GetBestWay1()
        {
            int[] way = new int[_pointCount];
            double minWay = 0;
            double distance = 0;
            int swapPosition = 0;
            int swapTmp;

            for (int i = 0; i < _pointCount; i++) way[i] = i;

            for (int i = 0; i < _pointCount - 1; i++)
            {
                for (int j = i + 1; j < _pointCount; j++)
                {
                    distance = CalculateDistance(way[i], way[j]);
                    if (j == i + 1)
                    {
                        minWay = distance;
                        swapPosition = j;
                    }
                    else
                    {
                        if (minWay > distance)
                        {
                            minWay = distance;
                            swapPosition = j;
                        }
                    }

                }
                swapTmp = way[i + 1];
                way[i + 1] = way[swapPosition];
                way[swapPosition] = swapTmp;
            }
            return way;
        }
        private int[] GetBestWay2()
        {
            int[] way = new int[_pointCount];
            if (_count == 0)
            {
                int numberOfParents = 10;
                parents = new int[numberOfParents, _pointCount];
                generateParents(parents);
            }

            _count = _count+1;
           //PrintArr(parents);
            //sortArr(parents);
            generateChild(parents);
            sortArr(parents);
           // PrintArr(parents);
            for (int i = 0; i < _pointCount; i++)
            {
                way[i] = parents[0, i];
            }

            for (int i = 0; i < parents.GetLength(0)/2; i++)
            {
                for (int j = 0; j < parents.GetLength(1); j++)
                {
                    parents[i + parents.GetLength(0) / 2, j] = parents[i, j];
                }
            }

            return way;
        }

        private double CalculateDistance(int p1, int p2)
        {
            double distance = 0;
            distance = Sqrt(Pow((_pC[p1].X - _pC[p2].X), 2.0) + Pow((_pC[p1].Y - _pC[p2].Y), 2.0));
            return distance;
        }

        private void VelAlgoritm_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cAlg = (ComboBox)e.Source;
            ListBoxItem item = (ListBoxItem)cAlg.SelectedItem;
            _algoritm = Convert.ToString(item.Content);
        }

        private int[,] generateParents(int[,] parents)
        {
            Random rnd = new Random();
            List<int> rndColl = new List<int>();
            int position;
            for (int i = 0; i < parents.GetLength(0); i++)
            {
                for (int j = 0; j < parents.GetLength(1); j++) rndColl.Add(j);
                for (int j = 0; j < parents.GetLength(1); j++)
                {
                    position = rnd.Next(rndColl.Count);
                    parents[i, j] = rndColl[position];
                    rndColl.RemoveAt(position);
                }
            }
            return parents;
        }

        public void PrintArr(int[,] arr)
        {
            string str = "";
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    str = str + Convert.ToString(arr[i, j]) + "  ";
                }

                str = str + "\n";
            }
            MessageBox.Show(str);
        }

        private int[,] sortArr(int[,] parents)
        {
            double distance = 0;
            double minDistance = 0;
            int min_i = 0;
            int[] tmpArr = new int[parents.GetLength(1)];
            //           for (int i = 0; i < arr.GetLength(0); i++) tmpArr[i] = i;
            for (int l = 0; l < parents.GetLength(0); l++)
            {
                for (int i = l; i < parents.GetLength(0); i++)
                {
                    distance = 0;
                    for (int j = 0; j < parents.GetLength(1); j++)
                    {
                        if (j == parents.GetLength(1) - 1)
                        {
                            distance = distance + CalculateDistance(parents[i, j], parents[i, 0]);
                        }
                        else
                        {
                            distance = distance + CalculateDistance(parents[i, j], parents[i, j + 1]);
                        }
                    }

                    if (i == l)
                    {
                        minDistance = distance;
                        min_i = i;
                    }
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        min_i = i;
                    }
                }

                for (int i = 0; i < parents.GetLength(1); i++)
                {
                    tmpArr[i] = parents[min_i, i];
                    parents[min_i, i] = parents[l, i];
                    parents[l, i] = tmpArr[i];
                }
            }

           /* string s = "";
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                distance = 0;
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    if (j == arr.GetLength(1) - 1)
                    {
                        distance = distance + CalculateDistance(arr[i, j], arr[i, 0]);
                    }
                    else
                    {
                        distance = distance + CalculateDistance(arr[i, j], arr[i, j + 1]);
                    }
                }
                s = s + Convert.ToString(distance) + "\n";
            }
            MessageBox.Show(s);*/

            return parents;
        }

        private int[,] generateChild(int[,] parents)
        {
            Random rnd = new Random();
            int[,] child = new int[parents.GetLength(0) / 2, parents.GetLength(1)];
            List<int> tmp1 = new List<int>();
            List<int> tmp2 = new List<int>();
            List<int> ch1 = new List<int>();
            List<int> ch2 = new List<int>();
            int crossPoint;
            int mut1=0, mut2=0;
            int tmp;
            int parents1, parents2;
            
            for (int i = 0; i < parents.GetLength(0) / 2; i++)
            {
                crossPoint = rnd.Next(1, parents.GetLength(1) - 2);
                do
                {
                    parents1 = rnd.Next(parents.GetLength(0) / 2);
                    parents2 = rnd.Next(parents.GetLength(0) / 2);
                } while (parents1 == parents2);

                for (int j = 0; j < crossPoint; j++) tmp1.Add(parents[parents1, j]);
                for (int j = crossPoint; j < parents.GetLength(1); j++) tmp1.Add(parents[parents2, j]);
                for (int j = 0; j < crossPoint; j++) tmp2.Add(parents[parents2, j]);
                for (int j = crossPoint; j < parents.GetLength(1); j++) tmp2.Add(parents[parents1, j]);
                int moneta = rnd.Next(2);
                if (moneta == 0)
                {
                    foreach (var iterator in tmp1)
                    {
                        if(ch1.IndexOf(iterator)==-1) ch1.Add(iterator);
                    }
                    foreach (var iterator in tmp2)
                    {
                        if (ch1.IndexOf(iterator) == -1) ch1.Add(iterator);
                    }
                }
                else
                {
                    foreach (var iterator in tmp2)
                    {
                        if (ch1.IndexOf(iterator) == -1) ch1.Add(iterator);
                    }
                    foreach (var iterator in tmp1)
                    {
                        if (ch1.IndexOf(iterator) == -1) ch1.Add(iterator);
                    }
                }
                moneta = rnd.Next(100);
                if (moneta < 70)
                {
                    mut1 = rnd.Next(parents.GetLength(1));
                    mut2 = rnd.Next(parents.GetLength(1));
                    if (mut1 < mut2)
                    {
                        for (int a = mut1; a == mut2; a++)
                        {
                            tmp = ch1[a];
                            ch1[a] = ch1[mut2];
                            ch1[mut2] = tmp;
                            mut2--;
                        }
                    }
                    else
                    {
                        for (int a = mut2; a == mut1; a++)
                        {
                            tmp = ch1[a];
                            ch1[a] = ch1[mut2];
                            ch1[mut2] = tmp;
                            mut1--;
                        }
                    }
                }
                for (int j = 0; j < child.GetLength(1); j++)
                {
                    child[i, j] = ch1[j];
                }
                ch1.Clear();
                tmp1.Clear();
                tmp2.Clear();
            }

            for (int i = parents.GetLength(0) / 2; i < parents.GetLength(0); i++)
            {
                for (int j = 0; j < parents.GetLength(1); j++)
                {
                    parents[i, j] = child[i - parents.GetLength(0) / 2, j];
                }
            }
            return parents;
        }
    }
}