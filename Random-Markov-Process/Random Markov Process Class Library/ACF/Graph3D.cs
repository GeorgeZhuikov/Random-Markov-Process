using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Random_Markov_Process_Class_Library.ACF
{
    public class Graph3D
    {
        [DebuggerDisplay("X = {X}, Y = {Y}, Z = {Z}")]
        private struct PointXYZ
        {
            public double X { get; }
            public double Y { get; }
            public double Z { get; }

            public PointXYZ(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        private Font _fontAxis = new Font("Microsoft Sans Serif", 16);
        private Pen _penAxis = new Pen(Color.LightGray, 2);
        private int _pointSize = 4;
        private bool _sizeChanged;

        private Bitmap _bmp;
        private Graphics _g;
        private Size _size;
        private int _side;
        private double _vAngleCos;
        private double _axis;        
        private Point _initPoint;
        private double _rangeZ;
        private double _cX, _cY, _cZ;
        private double _minX;
        private double _minY;
        private double _minZ;
        private double _hAngle;
        private double _vAngle;
        private double _vAngleSin;

        private double _horizontalAngleGrad { get; set; }
        private double _verticalAngleGrad { get; set; }
        private double _zoom { get; set; }

        public double DefaultHorizontalAngleGrad { get; } = 210;
        public double DefaultVerticalAngleGrad { get; } = 15;
        public double DefaultZoom { get; } = .6;
        public double HorizontalAngleGrad
        {
            get
            {
                return _horizontalAngleGrad;
            }
            set
            {
                _horizontalAngleGrad = CheckAngle(value);
            }
        }
        public double VerticalAngleGrad
        {
            get
            {
                return _verticalAngleGrad;
            }
            set
            {
                _verticalAngleGrad = CheckAngle(value);
            }
        }
        public double Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                _zoom = value > 0 ? _zoom = value : .1;
            }
        }

        public Size Size { set { _sizeChanged = true; _size = value; } }
        internal Points3D Points3D { get; set; }
        internal string NameAxisX { get; set; }
        internal string NameAxisY{ get; set; }
        internal string NameAxisZ { get; set; }


        internal Graph3D()
        {
            Size = new Size(500, 500);

            HorizontalAngleGrad = DefaultHorizontalAngleGrad;
            VerticalAngleGrad = DefaultVerticalAngleGrad;
            Zoom = DefaultZoom;

            NameAxisX = "x";
            NameAxisY = "y";
            NameAxisZ = "z";
        }


        #region Private help methods

        private double CheckAngle(double angle)
        {
            if (angle < 0) angle = 360;
            if (angle > 360) angle = 0;

            return angle;
        }

        private double GradToRad(double grad)
        {
            return (double)grad / 180 * Math.PI;
        }

        private Point GetPoint2D(PointXYZ pointXYZ)
        {
            double d2X = _initPoint.X;
            double d2Y = _initPoint.Y;

            double rXY = Math.Sqrt(Math.Pow(pointXYZ.X, 2) + Math.Pow(pointXYZ.Y, 2));
            double fiXY = rXY == 0 ? 0 : Math.Acos(pointXYZ.X / rXY) + _hAngle;
            var cosFiXY = Math.Cos(fiXY);
            var sinFiXY = Math.Sin(fiXY);
            var dXYx = rXY * cosFiXY;
            var dXYy = rXY * sinFiXY;

            var dXZy = dXYy * _vAngleSin + pointXYZ.Z * _vAngleCos;

            d2X += dXYx;
            d2Y -= dXZy;

            return new Point(Convert.ToInt32(d2X), Convert.ToInt32(d2Y));
        }

        private PointXYZ GetPoint2DFromArray(int i, int j)
        {
            var x = Points3D.X[i];
            var y = Points3D.Y[j];
            var z = Points3D.Z[i, j];

            var valX = (x - _minX) * _cX;
            var valY = (y - _minY) * _cY;
            var valZ = (z - _minZ) * _cZ;

            return new PointXYZ(valX, valY, valZ);
        }

        private void DrawPolygon(Point[] points, Color color)
        {
            int ps = _pointSize / 2;
            _g.FillPolygon(new SolidBrush(color), points);
        }

        private PointXYZ FindPoint2DAndAddToArray(Point[] points, int index, int i, int j)
        {
            var p3D = GetPoint2DFromArray(i, j);
            points[index] = GetPoint2D(p3D);

            return p3D;
        }

        #endregion


        #region Private methods

        private void SetSide(Size size)
        {
            _size = size;
            _side = size.Width > size.Height ? size.Height : size.Width;
        }

        private void SetBmp()
        {
            if (_sizeChanged)
            {
                _bmp?.Dispose();
                _g?.Dispose();

                _bmp = new Bitmap(_size.Width, _size.Height);
                _g = Graphics.FromImage(_bmp);
            }

            _g.Clear(Color.White);
        }

        private void SetAxisCoefs()
        {
            _hAngle = GradToRad(HorizontalAngleGrad);
            _vAngle = GradToRad(VerticalAngleGrad);

            _vAngleSin = Math.Sin(_vAngle);
            _vAngleCos = Math.Cos(_vAngle);

            _axis = _side * _zoom;
        }

        private void FindInitPoint()
        {
            var s = _side / 2;
            _initPoint = new Point(s, s);

            var ha = _hAngle;
            _hAngle = Math.PI + ha;
            var va = _vAngle;
            _vAngle = Math.PI + va;

            var a = _axis / 2;
            var ip = new PointXYZ(a, a, -a);

            var p = GetPoint2D(ip);

            _initPoint = new Point(p.X, p.Y);

            _hAngle = ha;
            _vAngle = va;
        }

        private void DrawAxis()
        {
            SolidBrush sb = new SolidBrush(Color.Black);

            var xPoint = GetPoint2D(new PointXYZ(_axis, 0, 0));
            var yPoint = GetPoint2D(new PointXYZ(0, _axis, 0));
            var zPoint = GetPoint2D(new PointXYZ(0, 0, _axis));

            _g.DrawLine(_penAxis, _initPoint, xPoint);
            _g.DrawLine(_penAxis, _initPoint, yPoint);
            _g.DrawLine(_penAxis, _initPoint, zPoint);

            _g.DrawString(NameAxisX, _fontAxis, sb, new Point(xPoint.X - 12, xPoint.Y - 37));
            _g.DrawString(NameAxisY, _fontAxis, sb, new Point(yPoint.X - 30, yPoint.Y - 48));
            _g.DrawString(NameAxisZ, _fontAxis, sb, new Point(zPoint.X + 7, zPoint.Y + 4));
        }

        private void SetAxisRange()
        {
            double minX = double.MaxValue, minY = minX, minZ = minX;
            double maxX = double.MinValue, maxY = maxX, maxZ = maxX;

            var length = Points3D.Length;
            for (int index = 0; index < length; index++)
            {
                var val = Points3D.X[index];
                if (val > maxX) maxX = val;
                if (val < minX) minX = val;
            }

            for (int index = 0; index < length; index++)
            {
                var val = Points3D.Y[index];
                if (val > maxY) maxY = val;
                if (val < minY) minY = val;
            }

            for (int i = 0; i < length; i++)
                for (int j = 0; j < length; j++)
                {
                    var val = Points3D.Z[i, j];
                    if (val > maxZ) maxZ = val;
                    if (val < minZ) minZ = val;
                }

            _minX = minX;
            _minY = minY;
            _minZ = minZ;

            var xC = Math.Abs(maxX - minX);
            var yC = Math.Abs(maxY - minY);
            _rangeZ = Math.Abs(maxZ - minZ);

            _cX = xC > 0 ? _axis / xC : 0;
            _cY = yC > 0 ? _axis / yC : 0;
            _cZ = _rangeZ > 0 ? _axis / _rangeZ : 0;
        }

        private void DrawPoints()
        {
            var length = Points3D.Length - 1;
            for (int i = 0; i < length; i++)
                for (int j = 0; j < length; j++)
                {
                    var points = new Point[4];
                    var p3D = FindPoint2DAndAddToArray(points, 0, i, j);
                    FindPoint2DAndAddToArray(points, 1, i + 1, j);
                    FindPoint2DAndAddToArray(points, 2, i + 1, j + 1);
                    FindPoint2DAndAddToArray(points, 3, i, j + 1);

                    int r = (int)(p3D.Z / _axis * 255);
                    Color color = Color.FromArgb(225, r, r / 2, 255 - r);

                    DrawPolygon(points, color);
                }
        }

        #endregion


        public Bitmap DrawGraph()
        {
            SetSide(_size);
            SetBmp();
            SetAxisCoefs();
            FindInitPoint();
            DrawAxis();
            SetAxisRange();
            DrawPoints();

            return _bmp;
        }
    }
}
