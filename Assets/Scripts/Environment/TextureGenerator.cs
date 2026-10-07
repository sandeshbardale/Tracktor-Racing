using UnityEngine;

namespace BhootiyaRasta.Environment
{
    public static class TextureGenerator
    {
        public static Texture2D CreateDirtRoadTexture(int width = 512, int height = 512)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "Tex_KacchaRoad_Dirt";
            tex.wrapMode = TextureWrapMode.Repeat;

            Color baseSoil = new Color(0.24f, 0.16f, 0.10f); // Dark rich brown mud loam
            Color drySoil = new Color(0.36f, 0.25f, 0.16f);  // Lighter dry earthen patch
            Color wetMud = new Color(0.14f, 0.09f, 0.05f);   // Damp dark mud
            Color pebbleCol = new Color(0.45f, 0.42f, 0.38f); // Small stone

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;

                    // Perlin noise octaves for natural soil variation
                    float n1 = Mathf.PerlinNoise(u * 6f, v * 12f);
                    float n2 = Mathf.PerlinNoise(u * 20f + 10f, v * 35f + 10f) * 0.5f;
                    float n3 = Mathf.PerlinNoise(u * 60f + 40f, v * 60f + 40f) * 0.25f;
                    float soilNoise = (n1 + n2 + n3) / 1.75f;

                    // Tire track ruts pattern along U (0.28 to 0.38 and 0.62 to 0.72)
                    float rutL = Mathf.Clamp01(1f - Mathf.Abs(u - 0.33f) / 0.09f);
                    float rutR = Mathf.Clamp01(1f - Mathf.Abs(u - 0.67f) / 0.09f);
                    float inRut = Mathf.Max(rutL, rutR);

                    // Chevron tire tread imprint in the ruts
                    float treadChevron = Mathf.Sin((v * 48f) + Mathf.Abs(u - 0.33f) * 35f);
                    float treadEffect = inRut * (0.5f + 0.5f * treadChevron);

                    Color col = Color.Lerp(baseSoil, drySoil, soilNoise);
                    col = Color.Lerp(col, wetMud, inRut * 0.65f + treadEffect * 0.25f);

                    // Scatter small pebbles/gravel
                    float pebbleNoise = Mathf.PerlinNoise(u * 120f, v * 120f);
                    if (pebbleNoise > 0.82f)
                    {
                        col = Color.Lerp(col, pebbleCol, (pebbleNoise - 0.82f) * 5f);
                    }

                    pixels[y * width + x] = col;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static Texture2D CreateRoadNormalMap(int width = 512, int height = 512)
        {
            Texture2D normalTex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            normalTex.name = "Tex_KacchaRoad_Normal";
            normalTex.wrapMode = TextureWrapMode.Repeat;

            float[,] heights = new float[width, height];

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float n1 = Mathf.PerlinNoise(u * 8f, v * 16f);
                    float n2 = Mathf.PerlinNoise(u * 32f, v * 48f) * 0.4f;
                    float n3 = Mathf.PerlinNoise(u * 90f, v * 90f) * 0.15f;

                    // Wheel ruts depression
                    float rutL = Mathf.Clamp01(1f - Mathf.Abs(u - 0.33f) / 0.09f);
                    float rutR = Mathf.Clamp01(1f - Mathf.Abs(u - 0.67f) / 0.09f);
                    float ruts = -Mathf.Max(rutL, rutR) * 0.6f;

                    // Chevron lugs
                    float chevron = Mathf.Sin(v * 48f) * Mathf.Max(rutL, rutR) * 0.2f;

                    heights[x, y] = (n1 + n2 + n3 + ruts + chevron);
                }
            }

            Color[] pixels = new Color[width * height];
            float strength = 4.0f;

            for (int y = 0; y < height; y++)
            {
                int yPrev = (y - 1 + height) % height;
                int yNext = (y + 1) % height;

                for (int x = 0; x < width; x++)
                {
                    int xPrev = (x - 1 + width) % width;
                    int xNext = (x + 1) % width;

                    float dX = (heights[xNext, y] - heights[xPrev, y]) * strength;
                    float dY = (heights[x, yNext] - heights[x, yPrev]) * strength;

                    Vector3 n = new Vector3(-dX, -dY, 1f).normalized;
                    pixels[y * width + x] = new Color(n.x * 0.5f + 0.5f, n.y * 0.5f + 0.5f, n.z * 0.5f + 0.5f, 1f);
                }
            }

            normalTex.SetPixels(pixels);
            normalTex.Apply();
            return normalTex;
        }

        public static Texture2D CreateFarmlandSoilTexture(int width = 256, int height = 256)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "Tex_Farmland_Soil";
            tex.wrapMode = TextureWrapMode.Repeat;

            Color darkLoam = new Color(0.12f, 0.11f, 0.08f);
            Color dryEarth = new Color(0.22f, 0.19f, 0.13f);
            Color dryStubble = new Color(0.28f, 0.24f, 0.15f);

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                // Ploughed furrows along V
                float furrow = 0.5f + 0.5f * Mathf.Sin(v * 40f);

                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float n = Mathf.PerlinNoise(u * 15f, v * 15f);
                    Color c = Color.Lerp(darkLoam, dryEarth, n * 0.7f + furrow * 0.3f);

                    // Dry agricultural crop stubble speckles
                    if (Mathf.PerlinNoise(u * 60f + 2f, v * 60f + 3f) > 0.78f)
                    {
                        c = dryStubble;
                    }

                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static Texture2D CreateTractorPaintTexture(int width = 256, int height = 256)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "Tex_Mahindra_RedPaint";

            Color freshRed = new Color(0.72f, 0.11f, 0.09f);
            Color weatheredRed = new Color(0.55f, 0.12f, 0.10f);
            Color darkMudGrime = new Color(0.18f, 0.12f, 0.08f);
            Color wornMetalEdge = new Color(0.32f, 0.33f, 0.35f);

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float n = Mathf.PerlinNoise(u * 12f, v * 12f);
                    Color c = Color.Lerp(freshRed, weatheredRed, n);

                    // Mud splatter gradient towards the lower parts
                    float mudFactor = Mathf.Clamp01((1f - v) * 0.9f + (Mathf.PerlinNoise(u * 20f, v * 20f) - 0.5f) * 0.5f);
                    c = Color.Lerp(c, darkMudGrime, mudFactor * 0.85f);

                    // Paint chip wear
                    if (Mathf.PerlinNoise(u * 50f + 5f, v * 50f + 5f) > 0.86f)
                    {
                        c = wornMetalEdge;
                    }

                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static Texture2D CreateTireRubberTexture(int width = 256, int height = 256)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "Tex_TractorTire_Rubber";

            Color blackRubber = new Color(0.09f, 0.09f, 0.09f);
            Color dustyRubber = new Color(0.17f, 0.15f, 0.13f);
            Color cakedDriedMud = new Color(0.28f, 0.20f, 0.13f);

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float n = Mathf.PerlinNoise(u * 16f, v * 16f);
                    Color c = Color.Lerp(blackRubber, dustyRubber, n);

                    // Dried mud in tread grooves
                    float mudCling = Mathf.PerlinNoise(u * 35f + 12f, v * 35f + 12f);
                    if (mudCling > 0.65f)
                    {
                        c = Color.Lerp(c, cakedDriedMud, (mudCling - 0.65f) * 2.8f);
                    }

                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static Texture2D CreateClayMudWallTexture(int width = 256, int height = 256)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "Tex_Rural_ClayWall";

            Color clayBase = new Color(0.48f, 0.38f, 0.28f);
            Color clayDark = new Color(0.38f, 0.28f, 0.20f);
            Color plasterPale = new Color(0.58f, 0.48f, 0.37f);

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float n1 = Mathf.PerlinNoise(u * 8f, v * 8f);
                    float n2 = Mathf.PerlinNoise(u * 28f, v * 28f) * 0.4f;
                    float blend = (n1 + n2) / 1.4f;

                    Color c = Color.Lerp(clayDark, clayBase, blend);
                    c = Color.Lerp(c, plasterPale, Mathf.Pow(blend, 2f) * 0.45f);

                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static Texture2D CreateThatchStrawTexture(int width = 256, int height = 256)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "Tex_Rural_ThatchRoof";
            tex.wrapMode = TextureWrapMode.Repeat;

            Color dryGoldenStraw = new Color(0.46f, 0.37f, 0.21f);
            Color darkStrawShadow = new Color(0.24f, 0.18f, 0.10f);
            Color weatheredGrey = new Color(0.35f, 0.32f, 0.26f);

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                // Layered rows of reeds/straw
                float row = 0.5f + 0.5f * Mathf.Sin(v * 32f);

                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    // Vertical strands of straw
                    float strand = Mathf.Sin(u * 120f + Mathf.PerlinNoise(u * 10f, v * 20f) * 8f);
                    float n = Mathf.PerlinNoise(u * 12f, v * 8f);

                    Color c = Color.Lerp(darkStrawShadow, dryGoldenStraw, (strand * 0.5f + 0.5f) * 0.7f + row * 0.3f);
                    c = Color.Lerp(c, weatheredGrey, n * 0.35f);

                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static Texture2D CreateWeatheredWoodTexture(int width = 256, int height = 256)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "Tex_Weathered_Wood";
            tex.wrapMode = TextureWrapMode.Repeat;

            Color woodLight = new Color(0.38f, 0.29f, 0.20f);
            Color woodDark = new Color(0.22f, 0.16f, 0.11f);
            Color woodGrainCrack = new Color(0.12f, 0.08f, 0.05f);

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    // Wood grain streaks along Y
                    float grain = Mathf.Sin(u * 60f + Mathf.PerlinNoise(u * 5f, v * 35f) * 12f);
                    float n = Mathf.PerlinNoise(u * 12f, v * 3f);

                    Color c = Color.Lerp(woodDark, woodLight, grain * 0.5f + 0.5f);
                    c = Color.Lerp(c, woodGrainCrack, n > 0.75f ? (n - 0.75f) * 4f : 0f);

                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static Texture2D CreateAncientStoneTexture(int width = 256, int height = 256)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "Tex_Ancient_Stone";

            Color stoneGrey = new Color(0.34f, 0.33f, 0.30f);
            Color stoneDark = new Color(0.20f, 0.20f, 0.18f);
            Color mossGreen = new Color(0.18f, 0.24f, 0.14f);

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float n1 = Mathf.PerlinNoise(u * 14f, v * 14f);
                    float n2 = Mathf.PerlinNoise(u * 40f, v * 40f) * 0.3f;
                    Color c = Color.Lerp(stoneDark, stoneGrey, n1 + n2);

                    // Eerie damp moss in stone crevices
                    float mossNoise = Mathf.PerlinNoise(u * 18f + 3f, v * 18f + 3f);
                    if (mossNoise > 0.65f)
                    {
                        c = Color.Lerp(c, mossGreen, (mossNoise - 0.65f) * 2.5f);
                    }

                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        public static Texture2D CreateGhostClothTexture(int width = 256, int height = 256)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, true);
            tex.name = "Tex_Ghost_TatteredSaree";

            Color clothWhite = new Color(0.88f, 0.86f, 0.82f, 0.95f);
            Color dirtyStain = new Color(0.35f, 0.32f, 0.28f, 0.9f);
            Color bloodTear = new Color(0.32f, 0.08f, 0.08f, 0.85f);

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float n = Mathf.PerlinNoise(u * 15f, v * 15f);
                    Color c = Color.Lerp(clothWhite, dirtyStain, n * 0.6f);

                    // Tattered dirty hemline towards bottom (v < 0.25)
                    if (v < 0.25f)
                    {
                        float tearNoise = Mathf.PerlinNoise(u * 40f, v * 20f);
                        c.a = Mathf.Clamp01((v / 0.25f) * tearNoise * 1.5f);
                        if (n > 0.72f) c = Color.Lerp(c, bloodTear, (n - 0.72f) * 3f);
                    }

                    pixels[y * width + x] = c;
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
    }
}
