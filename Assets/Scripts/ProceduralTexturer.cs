using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Idea/extra, make it parse texture pixel info from a string box so you can make textures in the editor during runtime
// TODO: Just make a custom cull off transparent cutout shader...
// NOTE: When regenerating during runtime it's better to just do SetPixels lol
/// IDEA: use GL or something to set resolution to 212x120 pixels

    ////////////////////////////////////////////////////////////////////////////////////
   //                       Written By Pseudonym_Tim 2020                            //
  // Inspired by Notch's minecraft 4k javascript fiddle: http://jsfiddle.net/uzMPU/ //
 ////////////////////////////////////////////////////////////////////////////////////

/// <summary>
/// Holds information for creating procedurally generated textures
/// </summary>
public class ProceduralTexturer : MonoBehaviour
{
    public static readonly int TEXTURE_SIZE = 16;

    public static readonly int textureAtlasSizeInBlocks = 2; // (We can just make it according to our block types count I think)
    public static float normalizedBlockTextureSize { get { return 1f / (float)textureAtlasSizeInBlocks; } }

    private void Update()
    {

    }

    public Texture2D CreateBlockTextureAtlas()
    {
        List<Texture2D> blockTextures = new List<Texture2D>();

        // Loop through all our block types, paint their textures and add them...
        for(int i = 0; i < System.Enum.GetNames(typeof(BlockTypes)).Length; i++)
        {
            // Create and add this blocks texture to our array...
            Texture2D blockTexture = new Texture2D(TEXTURE_SIZE, TEXTURE_SIZE, TextureFormat.ARGB32, false);

            PaintTexture(i, blockTexture); // Paint the texture for this block and apply it...

            blockTextures.Add(blockTexture); // Add this block to our textures list
        }

        // Stitch together an atlas out of all the block textures...
        Texture2D blocksAtlas = new Texture2D(256, 256);
        blocksAtlas.PackTextures(blockTextures.ToArray(), 0, 256);
        
        return blocksAtlas;
    }

    public void PaintTexture(int blockIndex, Texture2D blockTexture)
    {
        switch(blockIndex)
        {
            case 0:
                // (Air, transparent)
                MakeAirTexture(blockTexture);
                break;
            case 1:
                MakeBrickTexture(blockTexture);
                break;
            case 2:
                MakeDirtTexture(blockTexture);
                break;
            case 3:
                MakeLeavesTexture(blockTexture);
                break;
        }

        if(blockIndex != 0) { PaintNoise(blockTexture); } // Noise pass on the new texture...
        blockTexture.Apply();
    }

    private void MakeBrickTexture(Texture2D blockTexture)
    {
        Color color = Color.clear;

        for(int y = 0; y < TEXTURE_SIZE; y++)
        {
            for(int x = 0; x < TEXTURE_SIZE; x++)
            {
                color = new Color32(181, 58, 21, 1);

                if((x + (y >> 2) * 4) % 8 == 0 || y % 4 == 0)
                {
                    color = new Color32(188, 175, 165, 1);
                }

                blockTexture.SetPixel(x, y, color);
            }
        }
    }

    private void MakeAirTexture(Texture2D blockTexture)
    {
        Color color = Color.clear;

        for(int y = 0; y < TEXTURE_SIZE; y++)
        {
            for(int x = 0; x < TEXTURE_SIZE; x++)
            {
                blockTexture.SetPixel(x, y, color);
            }
        }
    }

    private void MakeDirtTexture(Texture2D blockTexture)
    {
        Color color = Color.clear;

        for(int y = 0; y < TEXTURE_SIZE; y++)
        {
            for(int x = 0; x < TEXTURE_SIZE; x++)
            {
                color = new Color32(150, 108, 74, 1);

                if(Random.value > 0.98f) { color = new Color32(127, 127, 127, 1); }

                blockTexture.SetPixel(x, y, color);
            }
        }
    }

    private void MakeLeavesTexture(Texture2D blockTexture)
    {
        Color color = Color.clear;
        Color green = new Color32(29, 166, 4, 255);

        for(int y = 0; y < TEXTURE_SIZE; y++)
        {
            for(int x = 0; x < TEXTURE_SIZE; x++)
            {
                color = Random.value >= 0.6f ? Color.clear : green;
                blockTexture.SetPixel(x, y, color);
            }
        }
    }

    private void MakeOakWoodTexture(Texture2D blockTexture)
    {
        Color color = Color.clear;

        for(int y = 0; y < TEXTURE_SIZE; y++)
        {
            for(int x = 0; x < TEXTURE_SIZE; x++)
            {
                //color = new Color32(103, 82, 49, 1);
                color = new Color32(188, 152, 98, 1);

                if(x % 4 == 0)
                {
                    //color = new Color32(188, 152, 98, 1);
                    color = new Color32(103, 82, 49, 1);
                }

                blockTexture.SetPixel(x, y, color);
            }
        }
    }

    // TODO: Actual perlin noise or something lol
    /// <summary>
    /// Darken random pixels for some noise to fake some detail...
    /// </summary>
    public void PaintNoise(Texture2D blockTexture)
    {
        for(int y = 0; y < TEXTURE_SIZE; y++)
        {
            for(int x = 0; x < TEXTURE_SIZE; x++)
            {
                // Get this pixel's color
                Color _color = blockTexture.GetPixel(x, y);

                float noiseAmount = Random.Range(0.01f, 0.05f);

                if(Random.value >= 0.5f)
                {
                    _color = new Color(_color.r - noiseAmount, _color.g - noiseAmount, _color.b - noiseAmount, _color.a);
                }
                else
                {
                    _color = new Color(_color.r + noiseAmount, _color.g + noiseAmount, _color.b + noiseAmount, _color.a);
                }

                blockTexture.SetPixel(x, y, _color);
            }
        }
    }

    private void MakeTestTexture(Texture2D blockTexture)
    {
        Color color = Color.clear;

        for(int y = 0; y < TEXTURE_SIZE; y++)
        {
            for(int x = 0; x < TEXTURE_SIZE; x++)
            {
                color = ((x & y) != 1 ? Color.black : Color.red);
                blockTexture.SetPixel(x, y, color);
            }
        }
    }
}

public enum BlockTypes
{
    // (Block order)
    AIR,
    BRICK,
    DIRT,
    LEAVES
}