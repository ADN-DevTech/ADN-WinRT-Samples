/////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved 
// Written by Philippe Leefsma 2013 - ADN/Developer Technical Services
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace AdnCloudViewer
{
    /////////////////////////////////////////////////////////////////////////////
    // 
    //
    /////////////////////////////////////////////////////////////////////////////
    public class AdnPoint
    {
        public double X
        {
            get;
            set;
        }

        public double Y
        {
            get;
            set;
        }

        public double Z
        {
            get;
            set;
        }

        public AdnPoint()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public AdnPoint(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public AdnPoint(double[] xyz)
        {
            X = xyz[0];
            Y = xyz[1];
            Z = xyz[2];
        }

        public void Add(AdnPoint point)
        {
            X += point.X;
            Y += point.Y;
            Z += point.Z;
        }

        public void Scale(double factor)
        {
            X *= factor;
            Y *= factor;
            Z *= factor;
        }

        public AdnVector AsVector()
        {
            return new AdnVector(X, Y, Z);
        }

        public double SquaredDistanceTo(AdnPoint p)
        {
            return
                (X - p.X) * (X - p.X) +
                (Y - p.Y) * (Y - p.Y) +
                (Z - p.Z) * (Z - p.Z);
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    // 
    //
    /////////////////////////////////////////////////////////////////////////////
    public class AdnVector
    {
        static AdnVector _xAxis = new AdnVector(1, 0, 0);
        static AdnVector _yAxis = new AdnVector(0, 1, 0);
        static AdnVector _zAxis = new AdnVector(0, 0, 1);

        public static AdnVector XAxis
        {
            get
            {
                return _xAxis;  
            }
        }

        public static AdnVector YAxis
        {
            get
            {
                return _yAxis;
            }
        }

        public static AdnVector ZAxis
        {
            get
            {
                return _zAxis;
            }
        }

        public AdnVector()
        {
            X = 0.0;
            Y = 0.0;
            Z = 0.0;
        }

        public AdnVector(AdnVector v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public AdnVector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public AdnVector(double[] xyz)
        {
            X = xyz[0];
            Y = xyz[1];
            Z = xyz[2];
        }

        public AdnVector Add(AdnVector v)
        {
            return new AdnVector(v.X + X, v.Y + Y, v.Z + Z);
        }

        public AdnVector Substract(AdnVector v)
        {
            return new AdnVector(X - v.X, Y - v.Y, Z - v.Z);
        }

        public AdnVector Scale(double factor)
        {
            return new AdnVector(X * factor, Y * factor, Z * factor);
        }

        public double DotProduct (AdnVector v)
        {
            return ((X * v.X) + (Y * v.Y) + (Z * v.Z));
        }

        public AdnVector CrossProduct (AdnVector v)
        {
            return new AdnVector(
                (Y * v.Z) - (Z * v.Y),
                (Z * v.X) - (X * v.Z),
                (X * v.Y) - (Y * v.X));
        }

        public void Normalize()
        {
            double norm = Math.Sqrt(X * X + Y * Y + Z * Z);

            X /= norm;
            Y /= norm;
            Z /= norm;
        }

        public double[] ToArray()
        {
            return new double[] { X, Y, Z };
        }

        public double X, Y, Z;
    }

    /////////////////////////////////////////////////////////////////////////////
    // 
    //
    /////////////////////////////////////////////////////////////////////////////
    public class AdnRay
    {
        const double EPSILON = 0.000001;

        public AdnPoint Origin
        {
            get;
            private set;
        }

        public AdnVector Direction
        {
            get;
            private set;
        }

        public AdnRay(AdnPoint origin, AdnVector direction)
        {
            Origin = origin;

            Direction = direction;

            Direction.Normalize();
        }

        public bool Intersect(
            AdnPoint v1,
            AdnPoint v2,
            AdnPoint v3)
        {
            AdnPoint p = null;

            return Intersect(v1, v2, v3, out p);
        }

        public bool Intersect(
            AdnPoint v1,
            AdnPoint v2,
            AdnPoint v3,
            out AdnPoint p)
        {
            p = null;

            double t = 0.0;
            double u = 0.0;
            double v = 0.0;

            AdnVector vert0 = v1.AsVector();
            AdnVector vert1 = v2.AsVector();
            AdnVector vert2 = v3.AsVector();

            // Find vectors for two edges sharing vert0.
            AdnVector edge1 = vert1.Substract(vert0);
            AdnVector edge2 = vert2.Substract(vert0);

            // Begin calculating determinant - also used to calculate U parameter.
            AdnVector pvec = Direction.CrossProduct(edge2);

            // If determinant is near zero, ray lies in plane of triangle.
            double det = edge1.DotProduct(pvec);

            if (det > -EPSILON && det < EPSILON)
                return false;

            double inv_det = 1.0 / det;

            // Calculate distance from vert0 to ray origin.
            AdnVector tvec = Origin.AsVector().Substract(vert0);

            // Calculate U parameter and test bounds.
            u = tvec.DotProduct(pvec) * inv_det;

            if (u < 0.0 || u > 1.0)
                return false;

            // Prepare to test V parameter.
            AdnVector qvec = tvec.CrossProduct(edge1);

            // Calculate V parameter and test bounds.
            v = Direction.DotProduct(qvec) * inv_det;

            if (v < 0.0 || u + v > 1.0)
                return false;

            // Calculate t, ray intersects triangle.
            t = edge2.DotProduct(qvec) * inv_det;

            AdnVector pos = Origin.AsVector().Add(Direction.Scale(t));

            p = new AdnPoint(pos.X, pos.Y, pos.Z);

            return true;
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    // 
    //
    /////////////////////////////////////////////////////////////////////////////
    public class AdnQuaternion
    {
        double _x;
        double _y;
        double _z;
        double _w;

        public AdnQuaternion()
        {
            _x = _y = _z = 0.0f;
            _w = 1.0f;
        }

        public AdnQuaternion(AdnVector axis, double degrees)
        {
            // First we want to convert the degrees to radians 
            // since the angle is assumed to be in radians
            double angle = (degrees / 180.0) * Math.PI;

            // Here we calculate the sin( theta / 2) once for optimization
            double result = Math.Sin(angle * 0.5);
		
            // Calcualte the w value by cos( theta / 2 )
            _w = Math.Cos(angle * 0.5);

            // Calculate the x, y and z of the quaternion
            _x = axis.X * result;
            _y = axis.Y * result;
            _z = axis.Z * result;
        }

        public AdnQuaternion Clone()
        { 
            AdnQuaternion q = new AdnQuaternion();

            q._w = _w;
            q._x = _x;
            q._y = _y;
            q._z = _z;

            return q;
        }

        public double Angle
        {
            get
            { 
                return 2 * Math.Acos(_w);
            }
        }

        public double[] Axis
        {
            get
            { 
                double sin = Math.Sin(Angle * 0.5);

                return new double[]
                {
                    _x / sin, 
                    _y / sin, 
                    _z / sin
                };
            }
        }

        public void Normalise()
        {
            double norm = _w * _w + _x * _x + _y * _y + _z * _z;

            if (norm > 0.00001)
            {
                norm = Math.Sqrt(norm);

                _w /= norm;
                _x /= norm;
                _y /= norm;
                _z /= norm;
            }
            else
            {
                _w = 1.0; _x = 0.0; _y = 0.0; _z = 0.0;
            }
        }

        public AdnQuaternion Multiply(AdnQuaternion q)
        {
            AdnQuaternion r = new AdnQuaternion();

            r._w = _w * q._w - _x * q._x - _y * q._y - _z * q._z;
            r._x = _w * q._x + _x * q._w + _y * q._z - _z * q._y;
            r._y = _w * q._y + _y * q._w + _z * q._x - _x * q._z;
            r._z = _w * q._z + _z * q._w + _x * q._y - _y * q._x;
	
            return(r);
        }

        public AdnQuaternion Invert()
        {
            AdnQuaternion qinv = new AdnQuaternion();

            qinv._w = _w;
            qinv._x = -_x;
            qinv._y = -_y;
            qinv._z = -_z;

            return qinv;
        }
        
        public double[] ToMatrix()
        {
            double[] m = new double[16];

            // First row
            m[0] = 1.0 - 2.0 * ( _y * _y + _z * _z ); 
            m[1] = 2.0 * (_x * _y + _z * _w);
            m[2] = 2.0 * (_x * _z - _y * _w);
            m[3] = 0.0;  
            
            // Second row
            m[4] = 2.0 * ( _x * _y - _z * _w );  
            m[5] = 1.0 - 2.0 * ( _x * _x + _z * _z ); 
            m[6] = 2.0 * (_z * _y + _x * _w );  
            m[7] = 0.0;  
            
            // Third row
            m[8] = 2.0 * ( _x * _z + _y * _w );
            m[9] = 2.0 * ( _y * _z - _x * _w );
            m[10] = 1.0 - 2.0 * ( _x * _x + _y * _y );  
            m[11] = 0.0;  
            
            // Fourth row
            m[12] = 0;  
            m[13] = 0;  
            m[14] = 0;  
            m[15] = 1.0;

            return m;
        }

        public AdnPoint Transform(AdnPoint point)
        {
            double[] m = ToMatrix();

            return new AdnPoint(
                m[0] * point.X + m[1] * point.Y + m[2] * point.Z,
                m[4] * point.X + m[5] * point.Y + m[6] * point.Z,
                m[8] * point.X + m[9] * point.Y + m[10] * point.Z);
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    // 
    //
    /////////////////////////////////////////////////////////////////////////////
    public class AdnBoundingBox
    {
        private List<AdnPoint> _vertices;

        public AdnBoundingBox(List<AdnPoint> meshVertices)
        {
            _vertices = new List<AdnPoint>();

            AdnPoint min = new AdnPoint(
                double.PositiveInfinity,
                double.PositiveInfinity,
                double.PositiveInfinity);

            AdnPoint max =  new AdnPoint(
                double.NegativeInfinity,
                double.NegativeInfinity,
                double.NegativeInfinity);

            foreach (AdnPoint vertex in meshVertices)
            {
                if (vertex.X < min.X)
                    min.X = vertex.X;

                if (vertex.Y < min.Y)
                    min.Y = vertex.Y;

                if (vertex.Z < min.Z)
                    min.Z = vertex.Z;

                if (vertex.X > max.X)
                    max.X = vertex.X;

                if (vertex.Y > max.Y)
                    max.Y = vertex.Y;

                if (vertex.Z > max.Z)
                    max.Z = vertex.Z;
            }

            _vertices.Add(min);
            _vertices.Add(new AdnPoint(min.X, min.Y, max.Z));
            _vertices.Add(new AdnPoint(max.X, min.Y, max.Z));
            _vertices.Add(new AdnPoint(max.X, min.Y, min.Z));

            _vertices.Add(max);
            _vertices.Add(new AdnPoint(min.X, max.Y, max.Z));
            _vertices.Add(new AdnPoint(min.X, max.Y, min.Z));
            _vertices.Add(new AdnPoint(max.X, max.Y, min.Z));
        }

        public bool Intersects(AdnRay ray)
        { 
            if (ray.Intersect(_vertices[0], _vertices[1], _vertices[2]))
                return true;

            if (ray.Intersect(_vertices[2], _vertices[3], _vertices[0]))
                return true;

            if (ray.Intersect(_vertices[1], _vertices[5], _vertices[4]))
                return true;

            if (ray.Intersect(_vertices[4], _vertices[2], _vertices[1]))
                return true;

            if (ray.Intersect(_vertices[3], _vertices[2], _vertices[4]))
                return true;

            if (ray.Intersect(_vertices[4], _vertices[7], _vertices[3]))
                return true;

            if (ray.Intersect(_vertices[0], _vertices[7], _vertices[7]))
                return true;

            if (ray.Intersect(_vertices[7], _vertices[0], _vertices[6]))
                return true;

            if (ray.Intersect(_vertices[0], _vertices[1], _vertices[6]))
                return true;

            if (ray.Intersect(_vertices[1], _vertices[5], _vertices[6]))
                return true;

            if (ray.Intersect(_vertices[4], _vertices[7], _vertices[6]))
                return true;

            if (ray.Intersect(_vertices[6], _vertices[5], _vertices[4]))
                return true;

            return false;
        }
    }
}
