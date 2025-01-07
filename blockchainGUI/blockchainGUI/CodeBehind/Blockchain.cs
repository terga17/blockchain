using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blockchainGUI.CodeBehind
{
    public class Blockchain
    {
        public List<Block> Chain {  get; set; } = new List<Block>();
        public int Diff { get; set; } = 4;

        public Blockchain() 
        {
            Chain.Add(GenFirstBlock());
        }

        private Block GenFirstBlock()
        {
            var firstBlock = new Block
            {
                Index = 0,
                Data = "Prvi blok",
                Timestamp = DateTime.Now,
                PreviousHash = "0",
                Difficulty = Diff,
            };
            firstBlock.Hash = firstBlock.CalculateHash();
            return firstBlock;
        }

        public void AddBlock(string data)
        {
            var prevBlock = Chain[Chain.Count - 1];
            var newBlock = new Block
            {
                Index = prevBlock.Index + 1,
                Data = data,
                Timestamp = DateTime.Now,
                PreviousHash = prevBlock.Hash,
                Difficulty = Diff,
            };
            newBlock.MineBlock();
            if (ValidateBlock(newBlock, prevBlock))
                Chain.Add(newBlock);
            else
                throw new InvalidOperationException("Adding block failed.");
        }

        public bool ValidateBlock(Block currBlock, Block prevBlock)
        {
            if(currBlock.Index != prevBlock.Index + 1)
                return false;
            if(currBlock.PreviousHash != prevBlock.Hash)
                return false;
            if(currBlock.Hash != currBlock.CalculateHash()) 
                return false;
            return true;
        }
    }
}
