using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class Octree
{
    public bool IsDivided { get; private set; }
    public BoundingBox Boundary { get { return _boundary; } }
    public IEnumerable<IOctreePoint> Points { get { return new ReadOnlyCollection<IOctreePoint>(_points);} }

    private readonly int _capacity;
    private readonly BoundingBox _boundary;
    private readonly List<IOctreePoint> _points = new List<IOctreePoint>();

    public Octree NorthEastBack;
    public Octree NorthWestBack;
    public Octree SouthEastBack;
    public Octree SouthWestBack;
    public Octree NorthEastFront;
    public Octree NorthWestFront;
    public Octree SouthEastFront;
    public Octree SouthWestFront;

    public Octree(BoundingBox boundary, int capacity)
    {
        _boundary = boundary;
        _capacity = capacity;
    }

    public bool Insert(IOctreePoint octreePoint)
    {
        if (!_boundary.Contains(octreePoint)) { return false; }
        if (_points.Count < _capacity)
        {
            _points.Add(octreePoint);
            return true;
        }

        if (!IsDivided) { Subdivide(); }
        return NorthEastBack.Insert(octreePoint) ||
               NorthWestBack.Insert(octreePoint) ||
               SouthEastBack.Insert(octreePoint) ||
               SouthWestBack.Insert(octreePoint) ||
               NorthEastFront.Insert(octreePoint) ||
               NorthWestFront.Insert(octreePoint) ||
               SouthEastFront.Insert(octreePoint) ||
               SouthWestFront.Insert(octreePoint);
    }

    public IEnumerable<IOctreePoint> Query(IOctreeBoundingVolume volume)
    {
        if (!volume.Intersects(_boundary)) { yield break; }
        foreach (var point in _points) { if (volume.Contains(point)) { yield return point; } }

        if (!IsDivided) { yield break; }
        foreach (var point in NorthEastBack.Query(volume)) { yield return point; }
        foreach (var point in NorthWestBack.Query(volume)) { yield return point; }
        foreach (var point in SouthEastBack.Query(volume)) { yield return point; }
        foreach (var point in SouthWestBack.Query(volume)) { yield return point; }
        foreach (var point in NorthEastFront.Query(volume)) { yield return point; }
        foreach (var point in NorthWestFront.Query(volume)) { yield return point; }
        foreach (var point in SouthEastFront.Query(volume)) { yield return point; }
        foreach (var point in SouthWestFront.Query(volume)) { yield return point; }
    }

    private void Subdivide()
    {
        var x = _boundary.X;
        var y = _boundary.Y;
        var z = _boundary.Z;
        var w = _boundary.Width;
        var h = _boundary.Height;
        var d = _boundary.Depth;

        var neb = new BoundingBox(x + w / 2, y + h / 2, z + d / 2, w / 2, h / 2, d / 2);
        NorthEastBack = new Octree(neb, _capacity);
        var nwb = new BoundingBox(x - w / 2, y + h / 2, z + d / 2, w / 2, h / 2, d / 2);
        NorthWestBack = new Octree(nwb, _capacity);
        var seb = new BoundingBox(x + w / 2, y - h / 2, z + d / 2, w / 2, h / 2, d / 2);
        SouthEastBack = new Octree(seb, _capacity);
        var swb = new BoundingBox(x - w / 2, y - h / 2, z + d / 2, w / 2, h / 2, d / 2);
        SouthWestBack = new Octree(swb, _capacity);

        var nef = new BoundingBox(x + w / 2, y + h / 2, z - d / 2, w / 2, h / 2, d / 2);
        NorthEastFront = new Octree(nef, _capacity);
        var nwf = new BoundingBox(x - w / 2, y + h / 2, z - d / 2, w / 2, h / 2, d / 2);
        NorthWestFront = new Octree(nwf, _capacity);
        var sef = new BoundingBox(x + w / 2, y - h / 2, z - d / 2, w / 2, h / 2, d / 2);
        SouthEastFront = new Octree(sef, _capacity);
        var swf = new BoundingBox(x - w / 2, y - h / 2, z - d / 2, w / 2, h / 2, d / 2);
        SouthWestFront = new Octree(swf, _capacity);

        IsDivided = true;
    }

    #region Point
    public interface IOctreePoint
    {
        float X { get; }
        float Y { get; }
        float Z { get; }
    }

    public class OctreePoint : IOctreePoint
    {
        private readonly float _x;
        private readonly float _y;
        private readonly float _z;

        public float X { get { return _x; } }
        public float Y { get { return _y; } }
        public float Z { get { return _z; } }

        public OctreePoint(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public override string ToString()
        {
            return X + "," + Y + "," + Z;
        }
    }
    #endregion

    #region Volume
    public interface IOctreeBoundingVolume
    {
        bool Contains(IOctreePoint point);
        bool Intersects(IOctreeBoundingVolume volume);
    }

    public class BoundingBox : IOctreeBoundingVolume
    {
        public float X;
        public float Y;
        public float Z;
        public float Width;
        public float Height;
        public float Depth;

        public float MinX { get { return X - Width; } }
        public float MaxX { get { return X + Width; } }
        public float MinY { get { return Y - Height; } }
        public float MaxY { get { return Y + Height; } }
        public float MinZ { get { return Z - Depth; } }
        public float MaxZ { get { return Z + Depth; } }

        public BoundingBox(float x, float y, float z, float width, float height, float depth)
        {
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
            Depth = depth;
        }

        public virtual bool Contains(IOctreePoint point) { return Contains(point.X, point.Y, point.Z); }
        public virtual bool Contains(float x, float y, float z)
        {
            return x >= MinX &&
                   x <= MaxX &&
                   y >= MinY &&
                   y <= MaxY &&
                   z >= MinZ &&
                   z <= MaxZ;
        }

        public virtual bool Intersects(IOctreeBoundingVolume volume)
        {
            var box = volume as BoundingBox;
            return box != null && Intersects(box);
        }

        public virtual bool Intersects(BoundingBox volume)
        {
            return MinX <= volume.MaxX && MaxX >= volume.MinX &&
                   MinY <= volume.MaxY && MaxY >= volume.MinY &&
                   MinZ <= volume.MaxZ && MaxZ >= volume.MinZ;
        }
    }

    public class BoundingSphere : IOctreeBoundingVolume
    {
        public float X;
        public float Y;
        public float Z;
        public float Radius;

        public BoundingSphere(float x, float y, float z, float radius)
        {
            X = x;
            Y = y;
            Z = z;
            Radius = radius;
        }

        public virtual bool Contains(IOctreePoint point) { return Contains(point.X, point.Y, point.Z); }
        public virtual bool Contains(float x, float y, float z)
        {
            var distance = Math.Sqrt((x - X) * (x - X) +
                                     (x - Y) * (y - Y) +
                                     (x - Z) * (z - Z));

            return distance < Radius;
        }

        public virtual bool Intersects(IOctreeBoundingVolume volume)
        {
            var box = volume as BoundingBox;
            if (box != null) { return Intersects(box); }

            var sphere = volume as BoundingSphere;
            return sphere != null && Intersects(sphere);
        }

        public virtual bool Intersects(BoundingBox volume)
        {
            var x = Math.Max(volume.MinX, Math.Min(X, volume.MaxX));
            var y = Math.Max(volume.MinY, Math.Min(Y, volume.MaxY));
            var z = Math.Max(volume.MinZ, Math.Min(Z, volume.MaxZ));

            return Contains(x, y, z);
        }

        public virtual bool Intersects(BoundingSphere volume)
        {
            var distance = Math.Sqrt((X - volume.X) * (X - volume.X) +
                                     (Y - volume.Y) * (Y - volume.Y) +
                                     (Z - volume.Z) * (Z - volume.Z));

            return distance < (Radius + volume.Radius);
        }
    }
    #endregion

}
