using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Random_Markov_Process_Class_Library.ACF
{
    internal class GraphicObject
    {
        private GraphicObjectType _graphicObject;


        internal PointXYZ[] PointXYZs { get; }
        internal double Distance { get; private set; }
        internal string String { get; set; }
        internal Pen Pen { get; set; }
        internal Color Color { get; set; }
        internal Font Font { get; set; }


        internal GraphicObject(GraphicObjectType graphicObject, params PointXYZ[] pointXYZs)
        {
            _graphicObject = graphicObject;

            PointXYZs = pointXYZs;
        }


        #region Private methods

        private Point[] GetPointArray(Point initialPoint,
            double horizontalAngle,
            double verticalAngleSin,
            double verticalAngleCos)
        {
            var count = PointXYZs.Count();
            var result = new Point[count];

            for (int index = 0; index < count; index++)
                result[index] = GetPoint(PointXYZs[index], initialPoint, horizontalAngle, verticalAngleSin, verticalAngleCos);

            return result;
        }

        #endregion

        #region Draw private methods

        private void DrawLine(Graphics g,
            Point initialPoint,
            double horizontalAngle,
            double verticalAngleSin,
            double verticalAngleCos)
        {
            var points = GetPointArray(initialPoint, horizontalAngle, verticalAngleSin, verticalAngleCos);
            g.DrawLine(Pen, points[0], points[1]);

        }

        private void DrawString(Graphics g,
            Point initialPoint,
            double horizontalAngle,
            double verticalAngleSin,
            double verticalAngleCos)
        {
            var points = GetPointArray(initialPoint, horizontalAngle, verticalAngleSin, verticalAngleCos);
            g.DrawString(String, Font, new SolidBrush(Color), points[0]);
        }

        private void DrawPolygon(Graphics g,
            Point initialPoint,
            double horizontalAngle,
            double verticalAngleSin,
            double verticalAngleCos)
        {

            var points = GetPointArray(initialPoint, horizontalAngle, verticalAngleSin, verticalAngleCos);
            g.FillPolygon(new SolidBrush(Color), points);
            g.DrawLines(Pen, points);
        }

        #endregion


        #region Internal methods

        internal static Point GetPoint(PointXYZ pointXYZ,
            Point initialPoint,
            double horizontalAngle,
            double verticalAngleSin,
            double verticalAngleCos)
        {
            double d2X = initialPoint.X;
            double d2Y = initialPoint.Y;

            double rXY = Math.Sqrt(Math.Pow(pointXYZ.X, 2) + Math.Pow(pointXYZ.Y, 2));
            double fiXY = rXY == 0 ? 0 : Math.Sign(pointXYZ.Y) * Math.Acos(pointXYZ.X / rXY) + horizontalAngle;
            var cosFiXY = Math.Cos(fiXY);
            var sinFiXY = Math.Sin(fiXY);
            var dXYx = rXY * cosFiXY;
            var dXYy = rXY * sinFiXY;

            var dXZy = dXYy * verticalAngleSin + pointXYZ.Z * verticalAngleCos;

            d2X += dXYx;
            d2Y -= dXZy;

            return new Point(Convert.ToInt32(d2X), Convert.ToInt32(d2Y));
        }

        internal void Draw(Graphics g, 
            Point initialPoint, 
            double horizontalAngle, 
            double verticalAngleSin, 
            double verticalAngleCos)
        {
            switch (_graphicObject)
            {
                default: break;
                case GraphicObjectType.Line: DrawLine(g, initialPoint, horizontalAngle, verticalAngleSin, verticalAngleCos); break;
                case GraphicObjectType.String: DrawString(g, initialPoint, horizontalAngle, verticalAngleSin, verticalAngleCos); break;
                case GraphicObjectType.Polygon: DrawPolygon(g, initialPoint, horizontalAngle, verticalAngleSin, verticalAngleCos); break;
            }
        }

        internal void FindDistance(PointXYZ pointXYZ)
        {
            double distanceSum = 0;
            for (int index = 0; index < PointXYZs.Length; index++)
            {
                var point = PointXYZs[index];
                var dist = Math.Sqrt(Math.Pow(pointXYZ.X - point.X, 2) + Math.Pow(pointXYZ.Y - point.Y, 2) + Math.Pow(pointXYZ.Z - point.Z, 2));
                distanceSum += dist;
                if (dist > Distance) Distance = dist;
            }
        }

        #endregion
    }

    internal class GraphicObjects
    {
        private List<GraphicObject> _graphicObjects = new List<GraphicObject>();
        private Point _initialPoint;
        private double _horizontalAngle;
        private double _verticalAngleSin;
        private double _verticalAngleCos;

        internal Point InitialPoint { set { _initialPoint = value; } }
        internal double HorizontalAngle
        {
            set
            {
                _horizontalAngle = value;
            }
        }
        internal double VerticalAngle
        {
            get
            {
                return _horizontalAngle;
            }
            set
            {
                _verticalAngleSin = Math.Sin(value);
                _verticalAngleCos = Math.Cos(value);
            }
        }

        internal void Add(GraphicObject graphicObject)
        {
            _graphicObjects.Add(graphicObject);
        }

        internal void Add(List<GraphicObject> graphicObjects)
        {
            _graphicObjects.AddRange(graphicObjects);
        }

        internal void Clear()
        {
            _graphicObjects.Clear();
        }

        internal void FindDistances(PointXYZ pointXYZ)
        {
            for (int index = 0; index < _graphicObjects.Count; index++)
                _graphicObjects[index].FindDistance(pointXYZ);
        }

        internal void Draw(Graphics g)
        {
            var graphicObjects = _graphicObjects;
            while (graphicObjects.Count > 0)
            {
                double distance = 0;
                GraphicObject graphicObject = null;
                for (int index = 0; index < graphicObjects.Count; index++)
                {
                    var go = graphicObjects[index];
                    if (distance <= go.Distance)
                    {
                        distance = go.Distance;
                        graphicObject = go;
                    }
                }
                graphicObject.Draw(g, _initialPoint, _horizontalAngle, _verticalAngleSin, _verticalAngleCos);
                graphicObjects.Remove(graphicObject);
            }
        }
    }

    internal enum GraphicObjectType
    {
        Line,
        String,
        Polygon
    }

    [DebuggerDisplay("X = {X}, Y = {Y}, Z = {Z}")]
    internal struct PointXYZ
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

}
