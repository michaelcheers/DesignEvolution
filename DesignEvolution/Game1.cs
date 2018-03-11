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
        public Block[,] Blocks = new Block[640, 480];
        public List<Organism> Organisms = new List<Organism>();
        public event Action OnUpdate;

        public static readonly Point worldSize = new Point(640, 480);

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
                PreferredBackBufferWidth = 640,
                PreferredBackBufferHeight = 480
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        Texture2D rectangle;
        public static Random rnd = new Random();

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
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

            
                        Organisms.Add(Organism.Create(
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
                                        blockType = BlockType.Heart,
                                        direction = Direction.Right
                                    },

                                    new OrganismDesign.GrowPattern
                                    {
                                        blockNum = 1,
                                        blockType = BlockType.Heart,
                                        direction = Direction.Up
                                    },
                                    new OrganismDesign.GrowPattern
                                    {
                                        blockNum = 1,
                                        blockType = BlockType.Heart,
                                        direction = Direction.Down
                                    }
                                }
                            }
                            , new Point(100, 100), this));

            toDraw = new Texture2D(GraphicsDevice, 640, 480);
            UpdateFoodAndLight();

            base.Initialize();
        }
        DateTime nextUpdateOfTexture = DateTime.Now;
        Color[] lightPixels = new Color[640 * 480];

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

            if (input.MouseWheelDelta != 0)
            {
                Vector2 oldMousePos = (input.MousePos - offset)/zoom;
                zoom = Math.Max(0.5f, zoom + input.MouseWheelDelta / 100);
                Vector2 newMousePos = (input.MousePos - offset)/zoom;
                offset -= (oldMousePos - newMousePos)*zoom;
            }

            if (!paused)
            {
                UpdateFoodAndLight();
                foreach (var org in Organisms.ToArray())
                    org.Update(this);
                OnUpdate?.Invoke();
            }

            base.Update(gameTime);
        }

        void UpdateFoodAndLightTexture ()
        {
            for (int y = 0; y < 480; y++)
            {
                for (int x = 0; x < 640; x++)
                {
                    byte sun = Blocks[x, y].SunlightAmount;
                    byte food = Blocks[x, y].EnergyAmount;
                    lightPixels[x + y * 640] = new Color(sun/2, food, 128+sun/2);
                }
            }
            toDraw.SetData(lightPixels);
        }

        void UpdateFoodAndLight()
        {
            for (int x = 0; x < Blocks.GetLength(0); x++)
            {
                Blocks[x, 0].SunlightAmount = 255;
            }
            for (int y = 1; y < Blocks.GetLength(1); y++)
            {
                for (int x = 0; x < Blocks.GetLength(0); x++)
                {
                    byte energyAbove = Blocks[x, y - 1].EnergyAmount;
                    byte energyHere = Blocks[x, y].EnergyAmount;
                    if (energyAbove > 0 && energyHere < 255)
                    {
                        Blocks[x, y].EnergyAmount++;
                        energyHere++;
                        Blocks[x, y - 1].EnergyAmount--;
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

                    if (Blocks[x, y - 1].Type == BlockType.None)
                        Blocks[x, y].SunlightAmount = (byte)Math.Max(0, Blocks[x, y - 1].SunlightAmount - 1);
                    else
                        Blocks[x, y].SunlightAmount = (byte)Math.Max(0, Blocks[x, y - 1].SunlightAmount - 20);
                }
            }
            if (DateTime.Now >= nextUpdateOfTexture)
            {
                nextUpdateOfTexture = DateTime.Now + TimeSpan.FromSeconds(1);
                UpdateFoodAndLightTexture();
            }
        }

        static readonly Dictionary<BlockType, Color> colors = new Dictionary<BlockType, Color>
        {
            {BlockType.None, Color.Yellow },
            {BlockType.Leaf, Color.DarkGreen },
            {BlockType.Heart, Color.DarkRed },
            {BlockType.Grower, Color.Purple },
            {BlockType.Engine, Color.Gray },
            {BlockType.Buoyancy, Color.White },
            {BlockType.Bone, Color.Black }
        };

        float zoom = 1;
        Vector2 offset;

        public static int Clamp(int n, int min, int max)
        {
            if (n > max)
                return max;
            if (n < min)
                return min;
            return n;
        }

        Texture2D toDraw;

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            spriteBatch.Begin();
            int minX = Clamp((int)(-offset.X / zoom), 0, Blocks.GetLength(0));
            int maxX = Clamp((int)((GraphicsDevice.Viewport.Width - offset.X)/zoom) + 1, 0, Blocks.GetLength(0));
            int minY = Clamp((int)(-offset.Y / zoom), 0, Blocks.GetLength(1));
            int maxY = Clamp((int)((GraphicsDevice.Viewport.Height - offset.Y) / zoom) + 1, 0, Blocks.GetLength(1));
            spriteBatch.Draw(toDraw, new Vectangle(offset, new Vector2(Blocks.GetLength(0) * zoom, Blocks.GetLength(1) * zoom)), Color.White);
            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
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
