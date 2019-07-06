using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignEvolution
{
    public class Organism
    {
        public int organismIndex;
        public OrganismDesign Design;
        public float Energy;
        public List<OrganismPiece> Pieces = new List<OrganismPiece>();
        public Point Position;
        public Vector2 FractionalPosition;
        public Vector2 MovePressure;
        public int nextGrow = 0;

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
            public Point Position;
            public BlockType BlockType;
        }

        public bool CanMove(Point offsetPosition, Game1 game)
        {
            foreach (OrganismPiece piece in Pieces)
            {
                Point toCheck = WorldPos(offsetPosition + piece.Position);
                if (toCheck.Y < 0 || toCheck.Y >= game.Blocks.GetLength(1))
                    return false;

                if (piece.BlockType != BlockType.Bone)
                    continue;
                Block block = game.Blocks[toCheck.X, toCheck.Y];
                if (block.Type == BlockType.Bone && block.Controller != this && block.Controller != null)
                    return false;
            }
            return true;
        }

        public static OrganismDesign Mutate(OrganismDesign design)
        {
            List<OrganismDesign.GrowPattern> growPattern = design.GrowPatterns.ToList();
            OrganismDesign copy = new OrganismDesign
            {
                GrowPatterns = growPattern
            };
            switch (Game1.rnd.Next(80))
            {
                case 0: // change block type
                    {
                        int index = Game1.rnd.Next(copy.GrowPatterns.Count);
                        OrganismDesign.GrowPattern p = growPattern[index];
                        p.blockType = (BlockType)(Game1.rnd.Next(6) + 1);
                        growPattern[index] = p;
                    }
                    break;
                case 1: // change direction
                    {
                        int index = Game1.rnd.Next(copy.GrowPatterns.Count);
                        OrganismDesign.GrowPattern p = growPattern[index];
                        p.direction = (Direction)(Game1.rnd.Next(4));
                        growPattern[index] = p;
                    }
                    break;
                case 2: // change blockNum
                    {
                        int index = Game1.rnd.Next(copy.GrowPatterns.Count);
                        OrganismDesign.GrowPattern p = growPattern[index];
                        p.blockNum = Game1.rnd.Next(index);
                        growPattern[index] = p;
                    }
                    break;
                case 3: // add a block
                    {
                        OrganismDesign.GrowPattern p = new OrganismDesign.GrowPattern();
                        p.blockType = (BlockType)(Game1.rnd.Next(7) + 1);
                        p.direction = (Direction)(Game1.rnd.Next(4));
                        p.blockNum = Game1.rnd.Next(growPattern.Count);
                        growPattern.Add(p);
                    }
                    break;
                case 4: // remove a block
                    {
                        int index = Game1.rnd.Next(copy.GrowPatterns.Count);
                        growPattern.RemoveAt(index);
                        for (int Idx = index; Idx < growPattern.Count; ++Idx)
                        {
                            OrganismDesign.GrowPattern p = growPattern[Idx];
                            if (p.blockNum > index)
                                p.blockNum--;
                            growPattern[Idx] = p;
                        }
                    }
                    break;
            }
            return copy;
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

        public bool Grow(Point positionRelativeToHeart, BlockType blockType, Game1 game, bool reproduce = true, int pieceIdx = 0)
        {
            if (Pieces.Count > 15)
                return false;

            int growCost = GetGrowCost(blockType);
            if (Energy < growCost)
                return false;

            Point newPos = WorldPos(positionRelativeToHeart);
            if (newPos.Y < 0 || newPos.Y >= Game1.worldHeight)
                return true;

            if ((game.Blocks[newPos.X, newPos.Y].Controller != null &&
                blockType != BlockType.None))
                return true;

            Energy -= growCost;

            if (blockType == BlockType.Heart && reproduce)
            {
                game.AddOrganism(Create(Mutate(Design), newPos, game));
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

            game.Blocks[newPos.X, newPos.Y].Controller = this;
            game.Blocks[newPos.X, newPos.Y].Type = blockType;
            game.Blocks[newPos.X, newPos.Y].EnergyAmount = 0;
            Energy += game.Blocks[newPos.X, newPos.Y].EnergyAmount;
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
                game.Blocks[toCheck.X, toCheck.Y].Controller = null;
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
                    else if (oldBlock.Controller != null)
                    {
                        // remove this piece from the other organism
                        oldBlock.Controller.DestroyBlock(toCheck, game);
                    }
                }
                game.Blocks[toCheck.X, toCheck.Y].Controller = this;
                game.Blocks[toCheck.X, toCheck.Y].Type = piece.BlockType;
                game.Blocks[toCheck.X, toCheck.Y].EnergyAmount = 0;
                Energy += game.Blocks[toCheck.X, toCheck.Y].EnergyAmount;
            }
            return true;
        }

        public void DestroyBlock (Point absolutePosition, Game1 game)
        {
            if (game.Blocks[absolutePosition.X, absolutePosition.Y].Controller == this)
            {
                game.Blocks[absolutePosition.X, absolutePosition.Y].Controller = null;
                game.Blocks[absolutePosition.X, absolutePosition.Y].Type = BlockType.None;
            }
            game.Blocks[absolutePosition.X, absolutePosition.Y].EnergyAmount += 5;
            Point posToDestroy = LocalPos(absolutePosition);
            for(int pieceIdx = 0; pieceIdx < Pieces.Count; ++pieceIdx)
            {
                OrganismPiece piece = Pieces[pieceIdx];
                if (piece.Position != posToDestroy)
                    continue;

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
                return;
            }
        }

        public int Age = 0;

        public static Organism Create (OrganismDesign design, Point position, Game1 game)
        {
            Organism resulting = new Organism
            {
                Position = position,
                FractionalPosition = Vector2.Zero,
                Design = design,
                Energy = 38
            };
            resulting.CreateOrganism(design, game);
            return resulting;
        }

        public void CreateOrganism (OrganismDesign design, Game1 game)
        {
            Grow(Point.Zero, BlockType.Heart, game, false);
            //TaskCompletionSource<bool> task = null;
            //void OnUpdate() { if (!task.Task.IsCompleted) task.SetResult(true); }
            //game.OnUpdate += OnUpdate;
            /*foreach (var growPattern in design.GrowPatterns)
            {
                Point pos;
                do
                {
                    await (task = new TaskCompletionSource<bool>()).Task;
                    if (dead)
                        return;
                    pos = Game1.ToPoint(growPattern.direction);
                }
                while (!Grow(Pieces.Count == 0 ? Point.Zero : Pieces[Math.Min(growPattern.blockNum, Pieces.Count - 1)].Position + pos, growPattern.blockType, game));
            }
            game.OnUpdate -= OnUpdate;*/
        }

        public void Die (Game1 game)
        {
            dead = true;
            game.RemoveOrganism(this);
            game.Blocks[Position.X, Position.Y].EnergyAmount += (byte)Energy;
            foreach (var piece in Pieces)
            {
                Point pos = WorldPos(piece.Position);
                var block = game.Blocks[pos.X, pos.Y];
                if (block.Controller == this) // this function might be called while the organism is halfway through moving
                {
                    block.Controller = null;
                    block.EnergyAmount += (byte)GetGrowCost(piece.BlockType);
                    block.Type = BlockType.None;
                    game.Blocks[pos.X, pos.Y] = block;
                }
            }
        }

        public bool dead;
        
        public void Update (Game1 game)
        {
            Move(-MovePressure*0.5f, game);
            Age++;
            if (Age > 1000)
            {
                Die(game);
            }
            else if(!dead)
            {
                if(nextGrow < Design.GrowPatterns.Count)
                {
                    OrganismDesign.GrowPattern pattern = Design.GrowPatterns[nextGrow];
                    if (pattern.blockType == BlockType.NextGrow)
                    {
                        nextGrow = pattern.blockNum % Design.GrowPatterns.Count;
                    }
                    else
                    {
                        int pieceIdx = Math.Min(pattern.blockNum, Pieces.Count - 1);
                        Point pos = Pieces.Count == 0 ? Point.Zero : Pieces[pieceIdx].Position + Game1.ToPoint(pattern.direction);
                        if (Grow(pos, pattern.blockType, game, pieceIdx:pieceIdx))
                        {
                            nextGrow++;
                        }
                    }
                }

                foreach (var piece in Pieces)
                {
                    Point pos = WorldPos(piece.Position);
                    if (game.Blocks[pos.X, pos.Y].EnergyAmount > 0)
                    {
                        Energy++;
                        game.Blocks[pos.X, pos.Y].EnergyAmount--;
                    }
                    if (piece.BlockType == BlockType.Leaf)
                        Energy += game.Blocks[pos.X, pos.Y].SunlightAmount / 1000f;
                    else if (piece.BlockType == BlockType.Bone)
                        Energy -= 0.1f;
                    else if (piece.BlockType == BlockType.Engine)
                        Energy -= 0.0f;
                   /* else if (piece.BlockType == BlockType.Grower)
                    {
                        RunGrower(piece.Position, game);
                    }*/
                }
            }
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
                Organism otherBlockOrganism = game.Blocks[copyToPos.X, copyToPos.Y].Controller;
                if (otherBlockOrganism == null)
                {
                    Grow(localCopyToPos, copyType, game);
                    return;
                }
                else if (otherBlockOrganism != this)
                {
                    return;
                }
            }
        }

        public Organism ()
        {

        }
    }
}
