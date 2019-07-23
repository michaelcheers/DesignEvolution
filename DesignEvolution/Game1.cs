using LRCEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DesignEvolution
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public const int worldWidth = 1920 / 3;
        public const int worldHeight = 1080 / 3;
        public Block[,] Blocks = new Block[worldWidth, worldHeight];
        Queue<int> freeOrganismSlots = new Queue<int>();
        public List<Organism> Organisms = new List<Organism>();
        public event Action OnUpdate;

        public static readonly Point worldSize = new Point(worldWidth, worldHeight);

        public static Point ToPoint (Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return new Point(0, -1);
                case Direction.Right:
                    return new Point(1, 0);
                case Direction.Down:
                    return new Point(0, 1);
                case Direction.Left:
                    return new Point(-1, 0);
                default:
                    throw new Exception();
            }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = worldWidth*3,
                PreferredBackBufferHeight = worldHeight*3,
                //IsFullScreen = true,
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        public int AllocOrganism()
        {
            if (freeOrganismSlots.Count > 0)
            {
                return freeOrganismSlots.Dequeue();
            }
            else
            {
                Organisms.Add(new Organism() { });
                return Organisms.Count - 1;
            }
        }

        public void RemoveOrganism(int index)
        {
            Organisms[index] = new Organism() { dead = true };
            freeOrganismSlots.Enqueue(index);
        }

        Texture2D rectangle;
        public static Random rnd;

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            int seed = new Random().Next();
            seed = 1140187476;
            System.IO.File.WriteAllText("seed.txt", seed.ToString());

            rnd = new Random(seed);

            /*Organisms.Add(Organism.Create(
                new OrganismDesign
                {
                    GrowPatterns = new List<OrganismDesign.GrowPattern>
                    {
                        new OrganismDesign.GrowPattern
                        {
                            blockNum = 0,
                            blockType = BlockType.Leaf,
                            direction = Direction.Right
                        },
                        new OrganismDesign.GrowPattern
                        {
                            blockNum = 1,
                            blockType = BlockType.Grower,
                            direction = Direction.Right
                        },
                    }
                }, new Point(100, 100), this
            ));*/

            /*
            Organisms.Add(Organism.Create(
                new OrganismDesign
                {
                    GrowPatterns = new List<OrganismDesign.GrowPattern>
                    {
                                    new OrganismDesign.GrowPattern
                                    {
                                        blockNum = 0,
                                        blockType = BlockType.Bone,
                                        direction = Direction.Down
                                    },
                    }
                }, new Point(100, 476), this
            ));*/
            for (int x = 0; x < worldWidth; x++)
                for (int y = 0; y < worldHeight; y++)
                    Blocks[x, y].ControllerIdx = -1;

            Organism.Create(
                            new OrganismDesign
                            {
                                GrowPatterns = new List<OrganismDesign.GrowPattern>
                                {
                                    new OrganismDesign.GrowPattern
                                    {
                                        blockNum = 0,
                                        blockType = BlockType.Leaf,
                                        direction = Direction.Right
                                    },
                                    new OrganismDesign.GrowPattern
                                    {
                                        blockNum = 0,
                                        blockType = BlockType.Heart,
                                        direction = Direction.Right
                                    },

                                    new OrganismDesign.GrowPattern
                                    {
                                        blockNum = 0,
                                        blockType = BlockType.Heart,
                                        direction = Direction.Up
                                    },
                                    new OrganismDesign.GrowPattern
                                    {
                                        blockNum = 0,
                                        blockType = BlockType.Heart,
                                        direction = Direction.Down
                                    },
                                    /*new OrganismDesign.GrowPattern
                                    {
                                        blockNum = 0,
                                        blockType = BlockType.Bone,
                                        direction = Direction.Down
                                    }*/
                                }
                            }
                            , new Point(100, 100), this);

            lightTexture = new Texture2D(GraphicsDevice, worldWidth, worldHeight);
            UpdateFoodAndLight();

            base.Initialize();
        }
        DateTime nextUpdateOfTexture = DateTime.Now;
        Color[] lightPixels = new Color[worldWidth * worldHeight];

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            rectangle = Content.Load<Texture2D>("white");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        InputState input = new InputState();
        bool paused = false;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            input.Update();
            if(input.mouseLeft.isDown)
                offset += input.MouseDelta;

            if(input.WasKeyJustPressed(Keys.Space))
            {
                paused = !paused;

                if(paused)
                {
                    UpdateFoodAndLightTexture();
                }
            }
            else if (input.WasKeyJustPressed(Keys.Right))
            {
                Step();
                UpdateFoodAndLightTexture();
                paused = true;
            }

            if (input.MouseWheelDelta != 0)
            {
                Vector2 oldMousePos = (input.MousePos - offset)/zoom;
                zoom = Math.Max(0.5f, zoom + input.MouseWheelDelta / 100);
                Vector2 newMousePos = (input.MousePos - offset)/zoom;
                offset -= (oldMousePos - newMousePos)*zoom;
            }

            if (!paused)
            {
                Step();
            }

            base.Update(gameTime);
        }

        void Step()
        {
            UpdateFoodAndLight();
            for(int Idx = 0; Idx < Organisms.Count; ++Idx)
            {
                Organism localOrg = Organisms[Idx];
                if (!localOrg.dead)
                    Organisms[Idx] = localOrg.Update(this);
            }
            OnUpdate?.Invoke();
        }

        void UpdateFoodAndLightTexture ()
        {
            for (int y = 0; y < worldHeight; y++)
            {
                for (int x = 0; x < worldWidth; x++)
                {
                    Block b = Blocks[x, y];
                    byte sun = b.SunlightAmount;
                    byte food = b.EnergyAmount;
                    byte foodVis = food == 0 ? (byte)0 : Math.Min((byte)(25 + food), (byte)255);
                    lightPixels[x + y * worldWidth] = new Color(sun, sun/2+ foodVis / 2, 128-sun/4+ foodVis / 2);
                }
            }
            lightTexture.SetData(lightPixels);
        }

        void UpdateFoodAndLight()
        {
            int gridHeight = Blocks.GetLength(1);
            int gridWidth = Blocks.GetLength(0);
            for (int y = gridHeight - 1; y > 0; y--)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    byte energyAbove = Blocks[x, y - 1].EnergyAmount;
                    byte energyHere = Blocks[x, y].EnergyAmount;
                    if (energyAbove > 0 && energyHere < 255)
                    {
                        byte energyDelta = Math.Min((byte)(energyAbove-energyAbove / 2), (byte)(255-energyHere));
                        Blocks[x, y].EnergyAmount += energyDelta;
                        Blocks[x, y - 1].EnergyAmount -= energyDelta;
                    }
                }
            }

            /*
            if (x > 0)
            {
                byte energyAdjacent = Blocks[x - 1, y].EnergyAmount;
                int delta = energyAdjacent - energyHere;
                if(delta > 1)
                {
                    Blocks[x, y].EnergyAmount++;
                    Blocks[x - 1, y].EnergyAmount--;
                }
                else if (delta < -1)
                {
                    Blocks[x, y].EnergyAmount--;
                    Blocks[x - 1, y].EnergyAmount++;
                }
            }
            */

            int width = Blocks.GetLength(0);
            int height = Blocks.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Block block = Blocks[x, y];
                    if (y <= 0)
                    {
                        block.SunlightAmount = 255;
                    }
                    else
                    {
                        Block blockAbove = Blocks[x, y - 1];
                        block.SunlightAmount = (byte)Math.Max(0,
                            blockAbove.SunlightAmount - (((y % 2 == 0) ? 1 : 0) + (blockAbove.Type == BlockType.None? 0: 1))
                        );
                    }

                    if (block.ControllerIdx >= 0)
                    {
                        Organism organism = Organisms[block.ControllerIdx];
                        if (block.EnergyAmount > 0)
                        {
                            organism.Energy++;
                            block.EnergyAmount--;
                        }
                        switch (block.Type)
                        {
                            case BlockType.Leaf:
                                organism.Energy += block.SunlightAmount / 1000f;
                                break;
                            case BlockType.Buoyancy:
                                organism.Energy -= 0.15f;
                                break;
                            case BlockType.Sinker:
                                organism.Energy -= 0.15f;
                                break;
                            case BlockType.Engine:
                                organism.Energy -= 0.4f;
                                break;
                            case BlockType.Heart:
                                organism.Energy -= 0.1f;
                                break;
                        }

                        Organisms[block.ControllerIdx] = organism;
                    }

                    Blocks[x, y] = block;
                }
            }
        }

        static readonly Dictionary<BlockType, Color> colors = new Dictionary<BlockType, Color>
        {
            {BlockType.None, Color.Yellow },
            {BlockType.Leaf, Color.DarkGreen },
            {BlockType.Heart, Color.DarkRed },
           // {BlockType.Grower, Color.Purple },
            {BlockType.Engine, Color.Gray },
            {BlockType.Buoyancy, Color.White },
            {BlockType.Bone, Color.Magenta },
            {BlockType.Sinker, Color.Black }
        };

        float zoom = 3;
        Vector2 offset;

        public static int Clamp(int n, int min, int max)
        {
            if (n > max)
                return max;
            if (n < min)
                return min;
            return n;
        }

        Texture2D lightTexture;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (DateTime.Now >= nextUpdateOfTexture)
            {
                nextUpdateOfTexture = DateTime.Now + TimeSpan.FromSeconds(0.08f);
                UpdateFoodAndLightTexture();
            }

            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();
            int minX = Clamp((int)(-offset.X / zoom), 0, Blocks.GetLength(0));
            int maxX = Clamp((int)((GraphicsDevice.Viewport.Width - offset.X)/zoom) + 1, 0, Blocks.GetLength(0));
            int minY = Clamp((int)(-offset.Y / zoom), 0, Blocks.GetLength(1));
            int maxY = Clamp((int)((GraphicsDevice.Viewport.Height - offset.Y) / zoom) + 1, 0, Blocks.GetLength(1));
            spriteBatch.Draw(lightTexture, new Vectangle(offset, new Vector2(Blocks.GetLength(0) * zoom, Blocks.GetLength(1) * zoom)), Color.White);

            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    BlockType typeXY = Blocks[x, y].Type;
                    if (typeXY == BlockType.None)
                        continue;
                    if (typeXY == BlockType.Bone)
                        ;
                    Vector2 pos = new Vector2(x * zoom + offset.X, y * zoom + offset.Y);
                    spriteBatch.Draw(rectangle, new Vectangle(pos, new Vector2(zoom)), colors[typeXY]);
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
