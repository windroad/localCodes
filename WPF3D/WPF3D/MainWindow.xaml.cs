using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;

namespace WPF3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GeometryModel3D mGeometry;
        private bool mDown;
        private Point mLastPos;

        public MainWindow()
        {
            InitializeComponent();

            BuildSolid();
        }

        private void BuildSolid()
        {
            // Define 3D mesh object
            MeshGeometry3D mesh = new MeshGeometry3D();
            double x = 0;
            double y = 0;

            mesh.Positions.Add(new Point3D(0, 0, 0));//顶点

            //x*x + y*y = z/2; 15度分
            for (float i = 0.1f; i < 2; i += 0.1f)//Z
            {
                for (double j = 0; j < 2 * Math.PI; j += (Math.PI / 12))
                {
                    x = Math.Sqrt(0.5 * i) * Math.Cos(j);
                    y = Math.Sqrt(0.5 * i) * Math.Sin(j);
                    mesh.Positions.Add(new Point3D(x, y, i));
                }

                //最前面的锥形。
                for (int k = 1; k < 25; k++)//15度，所以24个三角形
                {
                    //外表面/外表面
                    //mesh.TriangleIndices.Add(0);
                    //mesh.TriangleIndices.Add(k);
                    //mesh.TriangleIndices.Add(k + 1);

                    //内表面/外表面
                    mesh.TriangleIndices.Add(0);
                    mesh.TriangleIndices.Add(k + 1);
                    mesh.TriangleIndices.Add(k);
                }

                //后面的抛面柱体
                for (int l = 25; l < 475; l++)
                {
                    //外表面/内表面
                    mesh.TriangleIndices.Add(l + 1);
                    mesh.TriangleIndices.Add(l);
                    mesh.TriangleIndices.Add(l - 24);

                    //内表面/外表面
                    //mesh.TriangleIndices.Add(l + 1);
                    //mesh.TriangleIndices.Add(l - 24);
                    //mesh.TriangleIndices.Add(l);

                    //外表面/内表面.
                    mesh.TriangleIndices.Add(l);
                    mesh.TriangleIndices.Add(l - 25);
                    mesh.TriangleIndices.Add(l - 24);

                    //内表面/外表面.
                    //mesh.TriangleIndices.Add(l);
                    //mesh.TriangleIndices.Add(l - 24);
                    //mesh.TriangleIndices.Add(l - 25);

                }
            }

            // Geometry creation
            mGeometry = new GeometryModel3D(mesh, new DiffuseMaterial(Brushes.YellowGreen));
            mGeometry.BackMaterial = new DiffuseMaterial(Brushes.BlueViolet);            
            mGeometry.Transform = new Transform3DGroup();
            group.Children.Add(mGeometry);
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (mDown)
            {
                Point pos = Mouse.GetPosition(viewport);
                Point actualPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
                double dx = actualPos.X - mLastPos.X, dy = actualPos.Y - mLastPos.Y;

                double mouseAngle = 0;
                if (dx != 0 && dy != 0)
                {
                    mouseAngle = Math.Asin(Math.Abs(dy) / Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));
                    if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
                    else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
                    else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
                }
                else if (dx == 0 && dy != 0) mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
                else if (dx != 0 && dy == 0) mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;

                double axisAngle = mouseAngle + Math.PI / 2;

                Vector3D axis = new Vector3D(Math.Cos(axisAngle) * 4, Math.Sin(axisAngle) * 4, 0);

                double rotation = 0.01 * Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));

                Transform3DGroup group = mGeometry.Transform as Transform3DGroup;
                QuaternionRotation3D r = new QuaternionRotation3D(new Quaternion(axis, rotation * 180 / Math.PI));
                group.Children.Add(new RotateTransform3D(r));

                mLastPos = actualPos;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            mDown = true;
            Point pos = Mouse.GetPosition(viewport);
            mLastPos = new Point(pos.X - viewport.ActualWidth / 2, viewport.ActualHeight / 2 - pos.Y);
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mDown = false;
        }

        private void sldPosition_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (camera != null)
            {
                camera.Position = new Point3D(camera.Position.X, camera.Position.Y, e.NewValue);
            }
        }
    }
}
