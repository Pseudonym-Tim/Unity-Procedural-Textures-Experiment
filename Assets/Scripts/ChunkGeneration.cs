using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Perlin noise!
// TODO: Seperate chunk meshes...
// TODO: Chunk generation does NOT like generating using coroutines
// TODO: Setting the blockMap to air will fuck things up for some reason

    ////////////////////////////////////////////////////////////////////////////////////
   //                       Written By Pseudonym_Tim 2020                            //
  // Inspired by Notch's minecraft 4k javascript fiddle: http://jsfiddle.net/uzMPU/ //
 ////////////////////////////////////////////////////////////////////////////////////

/// <summary>
/// Generates a procedurally generated perlin noise chunk
/// of runtime created block meshes using textures created entirely through code
/// </summary>
public class ChunkGeneration : MonoBehaviour
{
    public static ChunkGeneration chunkGen { get; private set; }
    private ProceduralTexturer procTexture;

    private MeshRenderer chunkRenderer;
    private MeshFilter chunkFilter;

    public TMPro.TextMeshProUGUI debugText;
    private int blockCount;
    private float frameRate;
    private float frameTimer;
    private Coroutine chunkMeshInfoCO;

    [Header("World Gen Settings")]
    [Range(0, 1)] public float createBlockWaitTime = 0.01f;
    [Range(0, 1)] public float createAirChance = 0.9f;
    [Range(4, 16)] public int chunkWidth = 16;
    [Range(4, 16)] public int chunkHeight = 16;

    private int vertIndex = 0;
    private List<Vector3> verts = new List<Vector3>();
    private List<int> tris = new List<int>();
    private List<int> transTris = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();
    private Material[] materials = new Material[2];
    private Texture2D blocksAtlas;
    private Mesh chunkMesh;

    private byte[,,] blockMap;

    private void Awake()
    {
        chunkGen = this;

        blockMap = new byte[chunkGen.chunkWidth, chunkGen.chunkHeight, chunkGen.chunkWidth];

        // Setup our components...
        chunkFilter = gameObject.AddComponent<MeshFilter>();
        chunkRenderer = gameObject.AddComponent<MeshRenderer>();

        procTexture = GetComponent<ProceduralTexturer>();
    }

    private void Start()
    {
        transform.position = Vector3.zero; // Make sure this parent object is centered...

        // Make the empty chunk mesh...
        chunkMesh = new Mesh();
        chunkMesh.MarkDynamic(); // We're going to be updating this a lot...

        // Create our procedural block texture atlas and assign it...
        blocksAtlas = procTexture.CreateBlockTextureAtlas(); 

        // Setup atlas settings...
        blocksAtlas.wrapMode = TextureWrapMode.Clamp;
        blocksAtlas.filterMode = FilterMode.Point;
        blocksAtlas.requestedMipmapLevel = 2;

        // Setup our materials, auto grab unlits...
        materials[0] = new Material(Shader.Find("Unlit/Texture"));
        materials[1] = new Material(Shader.Find("Unlit/Transparent Cutout"));

        chunkRenderer.materials = materials; // Fill in materials...

        // (Set our materials texture to our created atlas)
        chunkRenderer.materials[0].mainTexture = blocksAtlas; 
        chunkRenderer.materials[1].mainTexture = blocksAtlas;

        SetBlockMap(); // Set our block map so we know what blocks we want where in the chunk...
        StartAddChunkMeshInfo();
    }

    private void Update()
    {
        DebugText();

        // Debug, randomize chunk during runtime
        if(Input.GetKeyDown(KeyCode.T)) { RandomizeChunk(); }
    }

    private void StartAddChunkMeshInfo()
    {
        if(chunkMeshInfoCO != null) { StopCoroutine(chunkMeshInfoCO); }

        chunkMeshInfoCO = StartCoroutine(AddChunkMeshInfo());
    }

    private void RandomizeChunk()
    {
        ClearChunkMesh();

        SetBlockMap(); // Set our block map so we know what blocks we want where in the chunk...
        StartAddChunkMeshInfo();
    }

    private IEnumerator AddChunkMeshInfo()
    {
        for(int x = 0; x < chunkWidth; x++)
        {
            for(int z = 0; z < chunkWidth; z++)
            {
                for(int y = 0; y < chunkHeight; y++)
                {
                    AddBlockMeshData(new Vector3Int(x, y, z));
                    MakeChunkMesh(); // Make our mesh...
                    yield return new WaitForSecondsRealtime(createBlockWaitTime);
                }

                blockCount++;
            }
        }
    }

    /*private void AddChunkMeshInfo()
    {
        for(int x = 0; x < chunkWidth; x++)
        {
            for(int z = 0; z < chunkWidth; z++)
            {
                for(int y = 0; y < chunkHeight; y++)
                {
                    AddBlockMeshData(new Vector3Int(x, y, z));
                }

                blockCount++;
            }
        }
    }*/

    private void DebugText()
    {
        string debugString = null;
        debugString += "\n";
        debugString += "<color=orange>FPS: " + frameRate;
        debugString += "\n";
        debugString += "<color=blue>Blocks Created: " + blockCount;
        debugString += "\n";
        debugString += "\n";
        debugString += "<color=white><u>Controls:</u>";
        debugString += "\n";
        debugString += "WASD - (Move)";
        debugString += "\n";
        debugString += "Hold Right Click - (Pan Camera)";
        debugString += "\n";
        debugString += "Space/Left Shift - (Go Up/Down)";
        debugString += "\n";
        debugString += "Scroll Wheel - (Change overall fly speed)";
        debugString += "\n";
        debugString += "T - (Randomize blocks)";

        if(frameTimer > 1f)
        {
            frameRate = (int)(1f / Time.unscaledDeltaTime);
            frameTimer = 0;
        }
        else
        {
            frameTimer += Time.deltaTime;
        }

        debugText.text = debugString;
    }

    private void SetBlockMap()
    {
        for(int x = 0; x < chunkWidth; x++)
        {
            for(int z = 0; z < chunkWidth; z++)
            {
                for(int y = 0; y < chunkHeight; y++)
                {
                    // Set to a random block...
                    blockMap[x, y, z] = (byte)Random.Range(0, System.Enum.GetNames(typeof(BlockTypes)).Length);

                    // Randomly set to air instead
                    //if(Random.value <= createAirChance) { blockMap[x, y, z] = (byte)BlockTypes.AIR; }
                }
            }
        }
    }

    // Should there be a block here from the block map we set?
    public bool BlockDesignated(Vector3Int pos) 
    {
        int x = pos.x;
        int y = pos.y;
        int z = pos.z;

        float cw = chunkWidth;
        float ch = chunkHeight;

        // Make sure we aren't out of bounds...
        if(x < 0 || x > cw - 1 || y < 0 || y > ch - 1 || z < 0 || z > cw - 1) { return true; }

        return IsTransparentBlock(blockMap[x, y, z]);
    }

    // NOTE: Transparent block type ID's go here...
    private bool IsTransparentBlock(byte blockID)
    {
        switch(blockID)
        {
            case 0: return true; // Air
            case 3: return true; // Leaves
        }

        return false;
    }

    private void ApplyBlockTexture(int blockIndex)
    {
        float y = blockIndex / ProceduralTexturer.textureAtlasSizeInBlocks;
        float x = blockIndex - (y * ProceduralTexturer.textureAtlasSizeInBlocks);

        x *= ProceduralTexturer.normalizedBlockTextureSize;
        y *= ProceduralTexturer.normalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + ProceduralTexturer.normalizedBlockTextureSize));
        uvs.Add(new Vector2(x + ProceduralTexturer.normalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + ProceduralTexturer.normalizedBlockTextureSize, y + ProceduralTexturer.normalizedBlockTextureSize));
    }

    private void AddBlockMeshData(Vector3Int pos)
    {
        byte blockID = blockMap[pos.x, pos.y, pos.z];

        for(int v = 0; v < BlockMeshInfo.BLOCK_TRI_LENGTH; v++)
        {
            if(BlockDesignated(pos + BlockMeshInfo.neighborFaceChecks[v]))
            {
                // Bit hacky bit it works, reduces vertcount by 2 on each face...
                verts.Add(pos + BlockMeshInfo.blockVerts[BlockMeshInfo.blockTris[v, 0]]);
                verts.Add(pos + BlockMeshInfo.blockVerts[BlockMeshInfo.blockTris[v, 1]]);
                verts.Add(pos + BlockMeshInfo.blockVerts[BlockMeshInfo.blockTris[v, 2]]);
                verts.Add(pos + BlockMeshInfo.blockVerts[BlockMeshInfo.blockTris[v, 3]]);

                ApplyBlockTexture(blockID); // Apply block texture...

                if(!IsTransparentBlock(blockID))
                {
                    tris.Add(vertIndex);
                    tris.Add(vertIndex + 1);
                    tris.Add(vertIndex + 2);
                    tris.Add(vertIndex + 2);
                    tris.Add(vertIndex + 1);
                    tris.Add(vertIndex + 3);
                }
                else
                {
                    transTris.Add(vertIndex);
                    transTris.Add(vertIndex + 1);
                    transTris.Add(vertIndex + 2);
                    transTris.Add(vertIndex + 2);
                    transTris.Add(vertIndex + 1);
                    transTris.Add(vertIndex + 3);
                }

                vertIndex += 4;
            }
        }
    }

    private void ClearChunkMesh()
    {
        chunkMesh = new Mesh(); // (Remake our chunk mesh)
        chunkMesh.MarkDynamic(); // We're going to be updating this a lot...

        // Reset our mesh info...
        vertIndex = 0;
        blockCount = 0;

        verts.Clear();
        uvs.Clear();
        tris.Clear();
        transTris.Clear();
    }

    private void MakeChunkMesh()
    {
        // Fill in mesh info...
        chunkMesh.vertices = verts.ToArray();

        chunkMesh.subMeshCount = 2;
        chunkMesh.SetTriangles(tris.ToArray(), 0);
        chunkMesh.SetTriangles(transTris.ToArray(), 1);

        chunkMesh.uv = uvs.ToArray();

        chunkMesh.RecalculateNormals();

        chunkFilter.mesh = chunkMesh;
    }
}
