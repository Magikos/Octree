using UnityEngine;

public class Main : MonoBehaviour
{
    public int Width = 10;
    public int Height = 10;
    public int Depth = 10;

    private Octree _octTree;

    void Start()
    {
        var areaCenter = transform.position;
        var boundary = new Octree.BoundingBox(areaCenter.x, areaCenter.y, areaCenter.z, Width / 2, Height / 2, Depth / 2);
        _octTree = new Octree(boundary, 4);
    }

    void Update()
    {
        // show current octree
        DrawDebug(_octTree);

        // press mouse to insert random point
        if (Input.GetMouseButtonDown(0))
        {
            var b = _octTree.Boundary;
            var pos = new Vector3(Random.Range(b.MinX, b.MaxX), Random.Range(b.MinY, b.MaxY), Random.Range(b.MinZ, b.MaxZ));
            var p = new Octree.OctreePoint(pos.x, pos.y, pos.z);
            _octTree.Insert(p);
        }
    }


    public void DrawDebug(Octree octree)
    {
        var bottomLeftBack = new Vector3(octree.Boundary.MinX, octree.Boundary.MinY, octree.Boundary.MaxZ);
        var bottomLeftFront = new Vector3(octree.Boundary.MinX, octree.Boundary.MinY, octree.Boundary.MinZ);
        var bottomRightBack = new Vector3(octree.Boundary.MaxX, octree.Boundary.MinY, octree.Boundary.MaxZ);
        var bottomRightFront = new Vector3(octree.Boundary.MaxX, octree.Boundary.MinY, octree.Boundary.MinZ);

        var topLeftBack = new Vector3(octree.Boundary.MinX, octree.Boundary.MaxY, octree.Boundary.MaxZ);
        var topLeftFront = new Vector3(octree.Boundary.MinX, octree.Boundary.MaxY, octree.Boundary.MinZ);
        var topRightBack = new Vector3(octree.Boundary.MaxX, octree.Boundary.MaxY, octree.Boundary.MaxZ);
        var topRightFront = new Vector3(octree.Boundary.MaxX, octree.Boundary.MaxY, octree.Boundary.MinZ);

        Debug.DrawLine(bottomLeftBack, bottomLeftFront, Color.red);
        Debug.DrawLine(bottomRightBack, bottomRightFront, Color.green);
        Debug.DrawLine(bottomLeftBack, bottomRightBack, Color.magenta);
        Debug.DrawLine(bottomLeftFront, bottomRightFront, Color.gray);

        Debug.DrawLine(topLeftBack, topLeftFront, Color.yellow);
        Debug.DrawLine(topRightBack, topRightFront, Color.blue);
        Debug.DrawLine(topLeftBack, topRightBack, Color.cyan);
        Debug.DrawLine(topLeftFront, topRightFront, Color.white);

        Debug.DrawLine(bottomLeftBack, topLeftBack, Color.red);
        Debug.DrawLine(bottomLeftFront, topLeftFront, Color.green);
        Debug.DrawLine(bottomRightBack, topRightBack, Color.magenta);
        Debug.DrawLine(bottomRightFront, topRightFront, Color.gray);

        // draw actual points
        foreach (var point in octree.Points)
        {
            Debug.DrawRay(new Vector3(point.X, point.Y, point.Z), Vector3.up * 0.1f, Color.white);
            Debug.DrawRay(new Vector3(point.X, point.Y, point.Z), -Vector3.up * 0.1f, Color.white);

            Debug.DrawRay(new Vector3(point.X, point.Y, point.Z), Vector3.forward * 0.1f, Color.white);
            Debug.DrawRay(new Vector3(point.X, point.Y, point.Z), -Vector3.forward * 0.1f, Color.white);

            Debug.DrawRay(new Vector3(point.X, point.Y, point.Z), Vector3.right * 0.1f, Color.white);
            Debug.DrawRay(new Vector3(point.X, point.Y, point.Z), -Vector3.right * 0.1f, Color.white);
        }

        // recursively show children
        if (!octree.IsDivided) {return;}
        DrawDebug(octree.NorthEastBack);
        DrawDebug(octree.NorthWestBack);
        DrawDebug(octree.SouthEastBack);
        DrawDebug(octree.SouthWestBack);
        DrawDebug(octree.NorthEastFront);
        DrawDebug(octree.NorthWestFront);
        DrawDebug(octree.SouthEastFront);
        DrawDebug(octree.SouthWestFront);
    }
}
