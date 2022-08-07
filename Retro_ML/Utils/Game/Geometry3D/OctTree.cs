namespace Retro_ML.Utils.Game.Geometry3D;
public class OctTree : IRaytracable
{
    /// <summary>
    /// The minimum size this cube can have
    /// </summary>
    private float minSize;
    /// <summary>
    /// The maximum lifespan a branch can reach
    /// </summary>
    private int maxLifespan;
    /// <summary>
    /// The lifespan of newly created branches
    /// </summary>
    private int initialLifespan;
    /// <summary>
    /// The length of this cube's edges
    /// </summary>
    private float currSize;
    /// <summary>
    /// This branch's current lifespan
    /// </summary>
    private int currLifespan;
    /// <summary>
    /// Used to keep track of whether or not this branch should be kept
    /// </summary>
    private int currLife;
    /// <summary>
    /// Bounding box of this branch
    /// </summary>
    private AABB aabb;

    /// <summary>
    /// This branch's parent. Null if it is the root of the tree
    /// </summary>
    private OctTree? parent;
    /// <summary>
    /// This branch's objects
    /// </summary>
    private List<IRaytracable> objects;
    /// <summary>
    /// Objects to add
    /// </summary>
    private Queue<IRaytracable> pendingObjects;
    private OctTree?[] childNodes;

    private bool built;

    public OctTree(Vector position, float size, float minSize, int branchMaxLifespan, int initialLifespan)
    {
        this.aabb = new AABB(position, size);
        this.currSize = size;
        this.minSize = minSize;
        this.maxLifespan = branchMaxLifespan;
        this.initialLifespan = initialLifespan;
        this.currLifespan = initialLifespan;
        this.currLife = initialLifespan;

        this.objects = new();
        this.built = false;
        this.pendingObjects = new();
        this.childNodes = new OctTree?[8];
    }

    public AABB AABB => aabb;
    public float MinX => aabb.MinX;
    public float MaxX => aabb.MaxX;
    public float MinY => aabb.MinY;
    public float MaxY => aabb.MaxY;
    public float MinZ => aabb.MinZ;
    public float MaxZ => aabb.MaxZ;
    public bool Static => true;
    public int Count { get; private set; }

    private bool HasChildren => childNodes.Any(cn => cn != null) || objects.Any();

    public float GetRaytrace(Ray ray)
    {
        PrepareTree();

        if (!HasChildren) return float.NaN;

        float currMin = float.PositiveInfinity;
        foreach (var obj in objects)
        {
            float dist = obj.GetRaytrace(ray);
            if (float.IsNaN(dist)) continue;
            currMin = MathF.Min(currMin, dist);
        }

        for (int i = 0; i < 8; i++)
        {
            if (childNodes[i] == null) continue;
            var child = childNodes[i]!;

            float aabbCollisionDist = child.AABB.GetRaytrace(ray);
            if (float.IsNaN(aabbCollisionDist)) continue;

            float dist = child.GetRaytrace(ray);
            if (float.IsNaN(dist)) continue;
            currMin = MathF.Min(currMin, dist);
        }

        return float.IsFinite(currMin) ? currMin : float.NaN;
    }

    public bool Contains(Vector p)
    {
        PrepareTree();

        if (!HasChildren) return false;

        foreach (var obj in objects)
        {
            if (obj.Contains(p)) return true;
        }

        for (int i = 0; i < 8; i++)
        {
            if (childNodes[i] == null) continue;
            if (childNodes[i]!.Contains(p)) return true;
        }

        return false;
    }

    /// <summary>
    /// Add this object to the OctTree
    /// </summary>
    /// <param name="obj"></param>
    public void AddObject(IRaytracable obj)
    {
        pendingObjects.Enqueue(obj);
        Count++;
    }

    /// <summary>
    /// Add all the items to the OctTree
    /// </summary>
    /// <param name="objects"></param>
    public void AddObjects(params IRaytracable[] objects)
    {
        if (!pendingObjects.Any()) pendingObjects = new(objects);
        else foreach (var obj in objects) AddObject(obj);
        Count += objects.Length;
    }

    /// <summary>
    /// Updates this tree, by removing dynamic objects and updating all the active branches recursively
    /// </summary>
    public void Update()
    {
        PrepareTree();

        var removed = RemoveDynamic();
        Count -= removed;
        var currParent = parent;
        while (currParent != null)
        {
            currParent.Count -= removed;
            currParent = currParent.parent;
        }
        if (pendingObjects.Any()) ProcessPending();

        UpdateCurrentLife();

        EliminateDeadChildren();

        UpdateChildren();
    }

    private void UpdateCurrentLife()
    {
        if (HasChildren)
        {
            //increase the lifespan of this branch
            if (currLifespan < maxLifespan) currLifespan *= 2;
            currLife = currLifespan;
        }
        else
        {
            currLife--;
        }
    }

    private void EliminateDeadChildren()
    {
        for (int i = 0; i < 8; i++)
        {
            var childNode = childNodes[i];
            if (childNode == null) continue;


            if (childNode.currLife <= 0 && !childNode.HasChildren)
            {
                childNodes[i] = null;
            }
        }
    }

    private void UpdateChildren()
    {
        for (int i = 0; i < 8; i++)
        {
            childNodes[i]?.Update();
        }
    }

    /// <summary>
    /// prepares this tree for raycasting
    /// </summary>
    private void PrepareTree()
    {
        if (built)
        {
            ProcessPending();
            return;
        }

        while (pendingObjects.Count != 0)
        {
            objects.Add(pendingObjects.Dequeue());
        }
        BuildBranch();
    }

    private void BuildBranch()
    {
        if (objects.Count <= 1) return;
        if (currSize < minSize) return;

        float halfSize = 0.5f * currSize;

        List<IRaytracable>[] objectLists = new List<IRaytracable>[8];
        for (int i = 0; i < 8; i++) objectLists[i] = new();
        AABB[] boundingBoxes = GetChildrenAABBs(halfSize);

        List<IRaytracable> addedToChildren = new();
        foreach (var obj in objects)
        {
            for (int i = 0; i < 8; i++)
            {
                if (boundingBoxes[i].FullyContains(obj.AABB))
                {
                    objectLists[i].Add(obj);
                    addedToChildren.Add(obj);
                    break;
                }
            }
        }

        foreach (var obj in addedToChildren) _ = objects.Remove(obj);

        for (int i = 0; i < 8; i++)
        {
            childNodes[i] = CreateNode(boundingBoxes[i].Position, halfSize, objectLists[i]);
        }

        built = true;
    }

    private void Insert(IRaytracable obj)
    {
        //If there are no objects, or we're at the minimum size
        if (!objects.Any() || currSize <= minSize)
        {
            objects.Add(obj);
            return;
        }
        if (!AABB.FullyContains(obj.AABB))
        {
            //if we have a parent, try inserting the object into it.
            //otherwise, insert into this octtree anyway
            if (this.parent != null) parent.Insert(obj);
            else objects.Add(obj);
            return;
        }
        float halfSize = 0.5f * currSize;
        var boundingBoxes = GetChildrenAABBs(halfSize);

        bool added = false;
        for (int i = 0; i < 8; i++)
        {
            if (boundingBoxes[i].FullyContains(obj.AABB))
            {
                if (childNodes[i] == null)
                {
                    childNodes[i] = CreateNode(boundingBoxes[i].Position, halfSize, new List<IRaytracable>(1) { obj });
                }
                else
                {
                    childNodes[i]!.AddObject(obj);
                }
                added = true;
                break;
            }
        }

        if (!added)
        {
            objects.Add(obj);
        }
    }

    private AABB[] GetChildrenAABBs(float halfSize)
    {
        Vector halfDim = new(halfSize);
        Vector quarterDim = new(0.5f * halfSize);
        AABB[] boundingBoxes = new AABB[8];
        boundingBoxes[0] = childNodes[0] == null ? new AABB(aabb.Position + new Vector(+quarterDim.X, +quarterDim.Y, +quarterDim.Z), halfDim) : childNodes[0]!.AABB;
        boundingBoxes[1] = childNodes[1] == null ? new AABB(aabb.Position + new Vector(+quarterDim.X, +quarterDim.Y, -quarterDim.Z), halfDim) : childNodes[1]!.AABB;
        boundingBoxes[2] = childNodes[2] == null ? new AABB(aabb.Position + new Vector(+quarterDim.X, -quarterDim.Y, +quarterDim.Z), halfDim) : childNodes[2]!.AABB;
        boundingBoxes[3] = childNodes[3] == null ? new AABB(aabb.Position + new Vector(+quarterDim.X, -quarterDim.Y, -quarterDim.Z), halfDim) : childNodes[3]!.AABB;
        boundingBoxes[4] = childNodes[4] == null ? new AABB(aabb.Position + new Vector(-quarterDim.X, +quarterDim.Y, +quarterDim.Z), halfDim) : childNodes[4]!.AABB;
        boundingBoxes[5] = childNodes[5] == null ? new AABB(aabb.Position + new Vector(-quarterDim.X, +quarterDim.Y, -quarterDim.Z), halfDim) : childNodes[5]!.AABB;
        boundingBoxes[6] = childNodes[6] == null ? new AABB(aabb.Position + new Vector(-quarterDim.X, -quarterDim.Y, +quarterDim.Z), halfDim) : childNodes[6]!.AABB;
        boundingBoxes[7] = childNodes[7] == null ? new AABB(aabb.Position + new Vector(-quarterDim.X, -quarterDim.Y, -quarterDim.Z), halfDim) : childNodes[7]!.AABB;

        return boundingBoxes;
    }

    private void ProcessPending()
    {
        while (pendingObjects.Count != 0)
        {
            Insert(pendingObjects.Dequeue());
        }
    }

    private int RemoveDynamic() => objects.RemoveAll(o => !o.Static);

    /// <summary>
    /// Creates an <see cref="OctTree"/> node if the given parameters are valid, null otherwise
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="size"></param>
    /// <param name="objects"></param>
    /// <returns></returns>
    private OctTree? CreateNode(Vector pos, float size, List<IRaytracable> objects)
    {
        if (!objects.Any()) return null;

        OctTree res = new(pos, size, minSize, maxLifespan, initialLifespan)
        {
            parent = this,
            objects = objects
        };

        return res;
    }
}
