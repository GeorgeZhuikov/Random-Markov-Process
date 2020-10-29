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
        private GraphicObjects _graphicObjects = new GraphicObjects();

        private Font _fontAxis = new Font("Microsoft Sans Serif", 16);
        private Pen _penAxis = new Pen(Color.LightGray, 2);
        private Pen _penPolygonLines = new Pen(Color.Black, 1);
        private bool _sizeChanged;

        private Bitmap _bmp;
        private Graphics _g;
        private Size _size;
        private int _side;
        private double _vAngleCos;
        private double _axis;        
        private Point _initPoint;
        private PointXYZ _specPointXYZ;
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

        public double DefaultHorizontalAngleGrad { get; } = 60;
        public double DefaultVerticalAngleGrad { get; } = 340;
        public double DefaultZoom { get; } = 1.2;
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

        private PointXYZ FindPoint2DAndAddToArray(Point[] points, int index, int i, int j)
        {
            var p3D = GetPoint2DFromArray(i, j);
            points[index] = GraphicObject.GetPoint(p3D, _initPoint, _hAngle, _vAngleSin, _vAngleCos);

            return p3D;
        }

        private Color GetColor(PointXYZ[] pointsXYZ)
        {
            Color result = Color.Transparent;

            double z = int.MinValue;
            for (int index = 0; index < 4; index++)
            {
                var p = pointsXYZ[index];
                if (p.Z > z)
                    z = p.Z;
            }

            int r = (int)(z / _axis * 225);
            int rr = r + 30;
            result = Color.FromArgb(rr, rr / 2, 255 - r);

            return result;
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
                _g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            }

            _g.Clear(Color.White);
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

            var cX = Math.Abs(maxX - minX);
            var cY = Math.Abs(maxY - minY);
            _rangeZ = Math.Abs(maxZ - minZ);

            _cX = cX > 0 ? _axis / cX : 0;
            _cY = cY > 0 ? _axis / cY : 0;
            _cZ = _rangeZ > 0 ? _axis / _rangeZ : 0;
        }

        private void SetAxisCoefs()
        {
            _hAngle = GradToRad(HorizontalAngleGrad);
            _vAngle = GradToRad(VerticalAngleGrad);

            _vAngleSin = Math.Sin(_vAngle);
            _vAngleCos = Math.Cos(_vAngle);

            _axis = _side / 2 * _zoom;
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
            
            var p = GraphicObject.GetPoint(ip, _initPoint, _hAngle, _vAngleSin, _vAngleCos);
            _initPoint = new Point(p.X, p.Y);

            _hAngle = ha;
            _vAngle = va;

            ip = new PointXYZ(ip.X, ip.Y, ip.Z + _axis);
            _specPointXYZ = new PointXYZ(
                ip.X + _axis *  Math.Sin(_hAngle) * _vAngleCos,
                ip.Y + _axis * Math.Cos(_hAngle) * _vAngleCos, 
                ip.Z - _axis * Math.Sin(_vAngle));

            _graphicObjects.Add(new GraphicObject(GraphicObjectType.Line, _specPointXYZ, ip) { Pen = new Pen(Color.Red, 4) });
        }

        private void ProceedAxis()
        {
            var initialPoint = new PointXYZ(0, 0, 0);

            var gos = new List<GraphicObject>();

            gos.Add(new GraphicObject(GraphicObjectType.Line, initialPoint, new PointXYZ(_axis, 0, 0)) { Pen = _penAxis });
            gos.Add(new GraphicObject(GraphicObjectType.Line, initialPoint, new PointXYZ(0, _axis, 0)) { Pen = _penAxis });
            gos.Add(new GraphicObject(GraphicObjectType.Line, initialPoint, new PointXYZ(0, 0, _axis)) { Pen = _penAxis });

            gos.Add(new GraphicObject(GraphicObjectType.String, new PointXYZ(_axis, 0, 0)) { String = NameAxisX, Color = Color.Black, Font = _fontAxis });
            gos.Add(new GraphicObject(GraphicObjectType.String, new PointXYZ(0, _axis, 0)) { String = NameAxisY, Color = Color.Black, Font = _fontAxis });
            gos.Add(new GraphicObject(GraphicObjectType.String, new PointXYZ(0, 0, _axis)) { String = NameAxisZ, Color = Color.Black, Font = _fontAxis });

            _graphicObjects.Add(gos);
        }

        private void ProceedArrayPoints()
        {
            var gos = new List<GraphicObject>();

            var length = Points3D.Length - 1;
            for (int i = 0; i < length; i++)
                for (int j = 0; j < length; j++)
                {
                    var points = new Point[4];
                    var pointsXYZ = new PointXYZ[4];
                    pointsXYZ[0] = FindPoint2DAndAddToArray(points, 0, i, j);
                    pointsXYZ[1] = FindPoint2DAndAddToArray(points, 1, i + 1, j);
                    pointsXYZ[2] = FindPoint2DAndAddToArray(points, 2, i + 1, j + 1);
                    pointsXYZ[3] = FindPoint2DAndAddToArray(points, 3, i, j + 1);

                    gos.Add(new GraphicObject(GraphicObjectType.Polygon, pointsXYZ) { Color = GetColor(pointsXYZ), Pen = _penPolygonLines });
                }

            _graphicObjects.Add(gos);
        }

        #endregion


        public Bitmap DrawGraph()
        {
            _graphicObjects.Clear();

            SetSide(_size);
            SetBmp();
            SetAxisCoefs();
            SetAxisRange();
            FindInitPoint();
            ProceedAxis();
            ProceedArrayPoints();

            _graphicObjects.InitialPoint = _initPoint;
            _graphicObjects.HorizontalAngle = _hAngle;
            _graphicObjects.VerticalAngle = _vAngle;
            _graphicObjects.FindDistances(_specPointXYZ);
            _graphicObjects.Draw(_g);

            return _bmp;
        }
    }
}
