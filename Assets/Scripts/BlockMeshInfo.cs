using UnityEngine;

/// <summary>
/// Holds all the information for constructing a performant cube mesh
/// </summary>
public static class BlockMeshInfo
{
    public static readonly int BLOCK_TRI_LENGTH = 6;

    public static readonly Vector3Int[] blockVerts = new Vector3Int[8]
    {
        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(1, 1, 1),
        new Vector3Int(0, 1, 1)
    };

    public static readonly Vector3Int[] neighborFaceChecks = new Vector3Int[6]
    {
        // (Same order of blockTris)
        new Vector3Int(0, 0, -1), // Back
        new Vector3Int(0, 0, 1), // Front
        new Vector3Int(0, 1, 0), // Top
        new Vector3Int(0, -1, 0), // Bottom
        new Vector3Int(-1, 0, 0), // Left
        new Vector3Int(1, 0, 0), // Right
    };

    public static readonly int[,] blockTris = new int[6, 4]
    {
        // (Order matters)
        { 0, 3, 1, 2 }, // Back face
        { 5, 6, 4, 7 }, // Front face
        { 3, 7, 2, 6 }, // Top face
        { 1, 5, 0, 4 }, // Bottom Face
        { 4, 7, 0, 3 }, // Left face
        { 1, 2, 5, 6 }  // Right face
    };

    public static readonly Vector2Int[] blockUVs = new Vector2Int[4]
    {
        new Vector2Int(0, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(1, 1)
    };
}
