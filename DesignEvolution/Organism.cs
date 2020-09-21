using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignEvolution
{
    public struct Cache4<T>
    {
        public static T defaultValue;
        public T first, second, third, fourth;

        public T this[int idx]
        {
            get
            {
                switch (idx)
                {
                    case 0: return first;
                    case 1: return second;
                    case 2: return third;
                    case 3: return fourth;
                    default:
                        throw new IndexOutOfRangeException("Get - Index " + idx + " is not supported by Cache4");
                }
            }

            set
            {
                switch (idx)
                {
                    case 0: first = value; break;
                    case 1: second = value; break;
                    case 2: third = value; break;
                    case 3: fourth = value; break;
                    default:
                        throw new IndexOutOfRangeException("Set - Index " + idx + " is not supported by Cache4");
                }
            }
        }

        public void FillFrom(List<T> array, int first)
        {
            if (first < array.Count)
                this.first = array[first];
            else
                this.first = defaultValue;

            if (first + 1 < array.Count)
                second = array[first + 1];
            else
                this.first = defaultValue;

            if (first + 2 < array.Count)
                third = array[first + 2];
            else
                this.first = defaultValue;

            if (first + 3 < array.Count)
                fourth = array[first + 3];
            else
                this.first = defaultValue;
        }

        public void ShiftDown()
        {
            first = second;
            second = third;
            third = fourth;
            fourth = defaultValue;
        }
    }

    public struct Organism
    {
        public OrganismDesign Design;
        Cache4<OrganismDesign.GrowPattern> designCache;
        public List<OrganismPiece> Pieces;

        public int organismIndex;
        public float Energy;
        public Point Position;
        public Vector2 FractionalPosition;
        public Vector2 MovePressure;
        public short nextGrow;
        public short growFail;
        public short Age;

        enum OrganismFlags
        {
            Dead = 1,
            Baby = 2, // spawned this frame
            HasBones = 4
        }
        byte _flags;
        public bool dead
        {
            get { return (_flags & (int)OrganismFlags.Dead) != 0; }
            set
            {
                if (value) { _flags |= (byte)OrganismFlags.Dead; }
                else if(dead) { _flags -= (byte)OrganismFlags.Dead; }
            }
        }
        public bool baby
        {
            get { return (_flags & (int) OrganismFlags.Baby) != 0; }
            set
            {
                if (value) { _flags |= (byte) OrganismFlags.Baby; }
                else if(baby) { _flags -= (byte) OrganismFlags.Baby; }
            }
        }
        public bool hasBones
        {
            get { return (_flags & (int)OrganismFlags.HasBones) != 0; }
            set
            {
                if (value) { _flags |= (byte)OrganismFlags.HasBones; }
                else if (hasBones) { _flags -= (byte)OrganismFlags.HasBones; }
            }
        }

        byte _minY;
        byte _maxY;
        public int minY { get { return -_minY; } set { _minY = (byte)(-value); } }
        public int maxY { get { return _maxY; } set { _maxY = (byte)value; } }

        public Point WorldPos (Point point)
        {
            return new Point((Position.X + point.X + Game1.worldSize.X) % Game1.worldSize.X, Position.Y + point.Y); 
        }

        public Point LocalPos (Point point)
        {
            Point @new = point - Position;
            if (@new.X > 300)
                @new.X -= Game1.worldSize.X;
            else if (@new.X < -300)
                @new.X += Game1.worldSize.X;
            return @new;
        }

        public struct OrganismPiece
        {
            byte PosX;
            byte PosY;
            public Point Position {
                get {
                    return new Point(PosX - 128, PosY - 128);
                }
                set {
                    PosX = (byte)(value.X+128);
                    PosY = (byte)(value.Y+128);
                }
            }
            byte _blockTypeByte;
            public BlockType BlockType
            {
                get { return (BlockType)_blockTypeByte; }
                set { _blockTypeByte = (byte)value; }
            }
        }

        public bool CanMove(Point offsetPosition, Game1 game)
        {
            Point minYPos = WorldPos(offsetPosition + new Point(0, minY));
            Point maxYPos = WorldPos(offsetPosition + new Point(0, maxY));

            if (minYPos.Y < 0 || maxYPos.Y >= game.Blocks.GetLength(1))
                return false;

            Point heartPos = WorldPos(offsetPosition + new Point(0, 0));
            if(heartPos.Y < 0)
            {
                int breakhere = 1;
                breakhere++;
            }

            if (!hasBones)
                return true;

            foreach (OrganismPiece piece in Pieces)
            {
                Point toCheck = WorldPos(offsetPosition + piece.Position);
                if (toCheck.Y < 0 || toCheck.Y >= game.Blocks.GetLength(1))
                    return false;

                if (piece.BlockType != BlockType.Bone)
                    continue;
                Block block = game.Blocks[toCheck.X, toCheck.Y];
                if (block.Type == BlockType.Bone && block.ControllerIdx != organismIndex && block.ControllerIdx != -1)
                    return false;
            }
            return true;
        }

    
        public static OrganismDesign Mutate(OrganismDesign design)
        {
            List<OrganismDesign.GrowPattern> copyPattern;
            switch (Game1.rnd.Next(80))
            {
                case 0: // change block type
                    {
                        copyPattern = design.GrowPatterns.ToList();
                        int index = Game1.rnd.Next(copyPattern.Count);
                        OrganismDesign.GrowPattern p = copyPattern[index];
                        p.blockType = (BlockType)(Game1.rnd.Next(7) + 1);
                        copyPattern[index] = p;
                    }
                    break;
                case 1: // change direction
                    {
                        copyPattern = design.GrowPatterns.ToList();
                        int index = Game1.rnd.Next(copyPattern.Count);
                        OrganismDesign.GrowPattern p = copyPattern[index];
                        p.direction = (Direction)(Game1.rnd.Next(4));
                        copyPattern[index] = p;
                    }
                    break;
                case 2: // change blockNum
                    {
                        copyPattern = design.GrowPatterns.ToList();
                        int index = Game1.rnd.Next(copyPattern.Count);
                        OrganismDesign.GrowPattern p = copyPattern[index];
                        p.blockNum = (byte)Game1.rnd.Next(index);
                        copyPattern[index] = p;
                    }
                    break;
                case 3: // add a block
                    {
                        if (design.GrowPatterns.Count < 16)
                        {
                            copyPattern = design.GrowPatterns.ToList();
                            OrganismDesign.GrowPattern p = new OrganismDesign.GrowPattern();
                            p.blockType = (BlockType)(Game1.rnd.Next(8) + 1);
                            p.direction = (Direction)(Game1.rnd.Next(4));
                            p.blockNum = (byte)Game1.rnd.Next(copyPattern.Count);
                            copyPattern.Add(p);
                        }
                        else
                        {
                            return design;
                        }
                    }
                    break;
                case 4: // remove a block
                    {
                        copyPattern = design.GrowPatterns.ToList();
                        int index = Game1.rnd.Next(copyPattern.Count);
                        copyPattern.RemoveAt(index);
                        for (int Idx = index; Idx < copyPattern.Count; ++Idx)
                        {
                            OrganismDesign.GrowPattern p = copyPattern[Idx];
                            if (p.blockNum > index)
                                p.blockNum--;
                            copyPattern[Idx] = p;
                        }
                    }
                    break;
                default:
                    return design;
            }

            OrganismDesign copyDesign = new OrganismDesign
            {
                GrowPatterns = copyPattern
            };
            return copyDesign;
        }

        int GetGrowCost(BlockType type)
        {
            switch(type)
            {
                case BlockType.Heart: return 20;
                case BlockType.Bone: return 10;
                default: return 1;
            }
        }

        /*
        static readonly EnumArray<BlockType, byte> growCosts = new EnumArray<BlockType, byte>
        {
            {BlockType.Leaf, 1 },
            {BlockType.Heart, 20 },
           // {BlockType.Grower, 1 },
            {BlockType.Engine, 1 },
            {BlockType.Buoyancy, 1 },
            {BlockType.Bone, 10 },
            {BlockType.Sinker, 1 },
        };*/

        public bool Grow(Point positionRelativeToHeart, BlockType blockType, Game1 game, out bool blocked, bool reproduce = true, int pieceIdx = 0)
        {
            blocked = false;

            if (Pieces.Count > 15)
                return false;

            int growCost = GetGrowCost(blockType);
            if (Energy < growCost)
                return false;

            Point newPos = WorldPos(positionRelativeToHeart);
            if (newPos.Y < 0 || newPos.Y >= Game1.worldHeight)
            {
                blocked = true;
                return false; // growth blocked by edge of world
            }

            Block targetBlock = game.Blocks[newPos.X, newPos.Y];
            int replaceIdx = targetBlock.ControllerIdx;
            bool replaceSelf = replaceIdx == organismIndex && targetBlock.Type != BlockType.Heart;
            if (replaceIdx != -1 && !replaceSelf)
            {
                blocked = true;
                return false; // growth blocked by obstacle
            }

            Energy -= growCost;

            if (replaceSelf)
            {
                DestroyBlock(newPos, game, true);
            }

            if (blockType == BlockType.Heart && reproduce)
            {
                Create(Mutate(Design), newPos, game);
                return true;
            }

            if (blockType == BlockType.Buoyancy)
                MovePressure.Y++;
            else if (blockType == BlockType.Sinker)
                MovePressure.Y--;
            else if (blockType == BlockType.Engine)
            {
                if (positionRelativeToHeart.X >= 0)
                    MovePressure.X++;
                else
                    MovePressure.X--;
            }

            OrganismPiece newPiece = new OrganismPiece
            {
                Position = positionRelativeToHeart,
                BlockType = blockType
            };

            /*
            if(Pieces.FindIndex(p=> p.Position == positionRelativeToHeart) != -1)
            {
                int breakhere = 1;
            }*/

            if (pieceIdx < Pieces.Count)
            {
                Pieces.Add(Pieces[pieceIdx]);
                Pieces[pieceIdx] = newPiece;
            }
            else
            {
                Pieces.Add(newPiece);
            }

            if (blockType == BlockType.Bone)
                hasBones = true;

            minY = Math.Min(minY, positionRelativeToHeart.Y);
            maxY = Math.Max(maxY, positionRelativeToHeart.Y);

            Block oldBlock = game.Blocks[newPos.X, newPos.Y];
            oldBlock.ControllerIdx = organismIndex;
            oldBlock.Type = blockType;
            //Energy += oldBlock.EnergyAmount;
            //oldBlock.EnergyAmount = 0;
            game.Blocks[newPos.X, newPos.Y] = oldBlock;
            return true;
        }

        public bool Move(Vector2 fractionalMove, Game1 game)
        {
            FractionalPosition += fractionalMove;
            Point offsetMovePosition = FractionalPosition.ToPoint();
            if (offsetMovePosition == Point.Zero)
                return true;
            if (!CanMove(offsetMovePosition, game))
            {
                FractionalPosition = Vector2.Zero;
                return false;
            }
            FractionalPosition -= offsetMovePosition.ToVector2();
            foreach (OrganismPiece piece in Pieces)
            {
                Point toCheck = WorldPos(piece.Position);
                game.Blocks[toCheck.X, toCheck.Y].ControllerIdx = -1;
                game.Blocks[toCheck.X, toCheck.Y].Type = BlockType.None;
            }

            Position = WorldPos(offsetMovePosition);

            OrganismPiece[] piecesCopy = new OrganismPiece[Pieces.Count];
            Pieces.CopyTo(piecesCopy);
            foreach (OrganismPiece piece in piecesCopy)
            {
                Point toCheck = WorldPos(piece.Position);
                Block oldBlock = game.Blocks[toCheck.X, toCheck.Y];
                if (oldBlock.Type != BlockType.None)
                {
                    bool oldBone = oldBlock.Type == BlockType.Bone;
                    bool newBone = piece.BlockType == BlockType.Bone;
                    if ((oldBone && !newBone) || (oldBone == newBone && Game1.rnd.Next(10) == 0))
                    {
                        // remove this piece from me
                        DestroyBlock(toCheck, game);
                        if (dead)
                            return false;
                        continue;
                    }
                    else if (oldBlock.ControllerIdx != -1)
                    {
                        // remove this piece from the other organism
                        game.Organisms[oldBlock.ControllerIdx].DestroyBlock(toCheck, game);
                    }
                }
                game.Blocks[toCheck.X, toCheck.Y].ControllerIdx = organismIndex;
                game.Blocks[toCheck.X, toCheck.Y].Type = piece.BlockType;
                //Energy += game.Blocks[toCheck.X, toCheck.Y].EnergyAmount;
                //game.Blocks[toCheck.X, toCheck.Y].EnergyAmount = 0;
            }
            return true;
        }

        public void DestroyBlock (Point absolutePosition, Game1 game, bool eatingSelf = false)
        {
            if (dead)
                return;

            if (game.Blocks[absolutePosition.X, absolutePosition.Y].ControllerIdx == this.organismIndex)
            {
                game.Blocks[absolutePosition.X, absolutePosition.Y].ControllerIdx = -1;
                game.Blocks[absolutePosition.X, absolutePosition.Y].Type = BlockType.None;
            }
            if(!eatingSelf)
                game.Blocks[absolutePosition.X, absolutePosition.Y].EnergyAmount += 1;

            Point posToDestroy = LocalPos(absolutePosition);
            int newMinY = 0;
            int newMaxY = 0;
            bool hasOtherBone = false;
            for(int pieceIdx = 0; pieceIdx < Pieces.Count; ++pieceIdx)
            {
                OrganismPiece piece = Pieces[pieceIdx];

                if (piece.Position != posToDestroy)
                {
                    newMinY = Math.Min(newMinY, piece.Position.Y);
                    newMaxY = Math.Max(newMaxY, piece.Position.Y);
                    if (piece.BlockType == BlockType.Bone)
                        hasOtherBone = true;
                    continue;
                }

                switch(piece.BlockType)
                {
                    case BlockType.Heart:
                        Die(game);
                        return;
                    case BlockType.Buoyancy:
                        MovePressure.Y--;
                        break;
                    case BlockType.Sinker:
                        MovePressure.Y++;
                        break;
                    case BlockType.Engine:
                        if (posToDestroy.X >= 0)
                            MovePressure.X--;
                        else
                            MovePressure.X++;
                        break;
                    case BlockType.Bone:
                        break;
                }
                Pieces.RemoveAt(pieceIdx);
                pieceIdx--;
            }

            minY = newMinY;
            maxY = newMaxY;
            hasBones = hasOtherBone;
        }

        public static void Create (OrganismDesign design, Point position, Game1 game)
        {
            Organism newOrganism = new Organism
            {
                Position = position,
                FractionalPosition = Vector2.Zero,
                Design = design,
                Energy = 38,
                Pieces = new List<OrganismPiece>(16),
                baby = true,
                nextGrow = (short)(design.GrowPatterns.Count - 1),
                growFail = -1
            };

            newOrganism.designCache.FillFrom(design.GrowPatterns, 0);

            int organismIndex = game.AllocOrganism();
            newOrganism.organismIndex = organismIndex;
            bool blocked;
            newOrganism.Grow(Point.Zero, BlockType.Heart, game, out blocked, reproduce: false);
            game.Organisms[organismIndex] = newOrganism;
        }

        public void Die (Game1 game)
        {
            dead = true;
            game.RemoveOrganism(organismIndex);
            game.Blocks[Position.X, Position.Y].EnergyAmount += (byte)(Energy/2);
            foreach (var piece in Pieces)
            {
                Point pos = WorldPos(piece.Position);
                var block = game.Blocks[pos.X, pos.Y];
                if (block.ControllerIdx == this.organismIndex) // this function might be called while the organism is halfway through moving
                {
                    block.ControllerIdx = -1;
                    //if(block.Type != BlockType.Heart)
                        //block.EnergyAmount += (byte)GetGrowCost(piece.BlockType);
                    block.Type = BlockType.None;
                    game.Blocks[pos.X, pos.Y] = block;
                }
            }
        }
        
        public Organism Update (Game1 game)
        {
            if(baby)
            {
                baby = false;
                return this;
            }

            Move(-MovePressure*0.5f, game);
            Age++;
            if (Age > 1000 || Energy <= 0)
            {
                Die(game);
            }
            else if(!dead)
            {
                if(nextGrow > 0)
                {
                    OrganismDesign.GrowPattern pattern = designCache.first;
                    if (pattern.blockType == BlockType.None)
                    {
                        int patternIdx = Design.GrowPatterns.Count - 1 - nextGrow;
                        pattern = Design.GrowPatterns[patternIdx];
                        designCache.FillFrom(Design.GrowPatterns, patternIdx+1);
                    }
                    else
                    {
                        designCache.ShiftDown();
                    }

                    if (pattern.blockType == BlockType.NextGrow)
                    {
                        nextGrow = (short)(pattern.blockNum % Design.GrowPatterns.Count);
                    }
                    else if(pattern.blockType == BlockType.GrowFail)
                    {
                        growFail = (short)(pattern.blockNum % Design.GrowPatterns.Count);
                    }
                    else
                    {
                        int pieceIdx = Math.Min(pattern.blockNum, Pieces.Count - 1);
                        Point pos = Pieces.Count == 0 ? Point.Zero : Pieces[pieceIdx].Position + Game1.ToPoint(pattern.direction);
                        bool blocked = false;
                        if (Grow(pos, pattern.blockType, game, out blocked, pieceIdx:pieceIdx))
                        {
                            nextGrow--;
                        }

                        if (blocked && growFail >= 0)
                        {
                            nextGrow = growFail;
                        }
                    }
                }

                /*
                bool firstBone = true;
                foreach (var piece in Pieces)
                {
                    Point pos = WorldPos(piece.Position);
                    Block block = game.Blocks[pos.X, pos.Y];
                    if (block.EnergyAmount > 0)
                    {
                        Energy++;
                        game.Blocks[pos.X, pos.Y].EnergyAmount--;
                    }
                    switch (piece.BlockType)
                    {
                        case BlockType.Leaf:
                            Energy += block.SunlightAmount / 1000f;
                            break;
                        case BlockType.Buoyancy:
                            Energy -= 0.15f;
                            break;
                        case BlockType.Sinker:
                            Energy -= 0.15f;
                            break;
                        case BlockType.Engine:
                            Energy -= 0.4f;
                            break;
                        case BlockType.Heart:
                            Energy -= 0.1f;
                            break;
                    }
                   /* case BlockType.Grower:
                        RunGrower(piece.Position, game);
                        break;
                    * /
                }*/
            }

            return this;
        }

        void RunGrower(Point growerPos, Game1 game)
        {
            if (Pieces.Count >= 20)
                return;
            Point growDir;
            if (Math.Abs(growerPos.X) >= Math.Abs(growerPos.Y))
            {
                growDir = new Point(Math.Sign(growerPos.X), 0);
            }
            else
            {
                growDir = new Point(0, Math.Sign(growerPos.Y));
            }
            if (growDir == Point.Zero)
                return;

            Point copyPos = WorldPos(growerPos - growDir);
            BlockType copyType = game.Blocks[copyPos.X, copyPos.Y].Type;

            if (copyType == BlockType.None)
                return;

            if (Energy < GetGrowCost(copyType))
                return;

            Point localCopyToPos = growerPos;

            while (true)
            {
                localCopyToPos += growDir;
                Point copyToPos = WorldPos(localCopyToPos);
                if (copyToPos.Y < 0 || copyToPos.Y >= Game1.worldHeight)
                {
                    return;
                }

                int copyToIdx = game.Blocks[copyToPos.X, copyToPos.Y].ControllerIdx;
                if (copyToIdx == -1)
                {
                    bool blocked;
                    Grow(localCopyToPos, copyType, game, out blocked);
                    return;
                }
                else if (copyToIdx != organismIndex)
                {
                    return;
                }
            }
        }
    }
}
