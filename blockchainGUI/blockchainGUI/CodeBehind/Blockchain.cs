using Newtonsoft.Json;
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
        public const int GenerateInterval = 10;     //vsakih 10s lahko vstavimo nov blok
        public const int AdjustDiffInterval = 3;   //vsakih 10 blokov, se težavnost spremeni

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

        public Block AddBlock(string data)
        {
            var prevBlock = Chain[Chain.Count - 1];
            var newBlock = new Block
            {
                Index = prevBlock.Index + 1,
                Data = $"Lastnik bloka: {data}",
                Timestamp = DateTime.Now,
                PreviousHash = prevBlock.Hash,
                Difficulty = Diff,
            };
            newBlock.MineBlock();

            if (ValidateBlock(newBlock, prevBlock))
            {
                Chain.Add(newBlock);
                if (Chain.Count % AdjustDiffInterval == 0)
                    newBlock.Difficulty = AdjustDifficulty();
            }
            else
                throw new InvalidOperationException("Adding block failed.");

            return newBlock;
        }

        public bool ValidateBlock(Block currBlock, Block prevBlock)     //validacija bloka, preverjanje indeksa in (prev)hasha
        {
            if(currBlock.Index != prevBlock.Index + 1)
                return false;
            if(currBlock.PreviousHash != prevBlock.Hash)
                return false;
            if(currBlock.Hash != currBlock.CalculateHash()) 
                return false;
            return true;
        }

        public bool ValidateChain()                                 //loop cez celoten chain, da preverimo integirteto vseh blokov
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                if (!ValidateBlock(Chain[i], Chain[i - 1]))
                    return false;                               //izpis na GUI
            }
            return true;
        }

        private bool ValidateOtherChain(List<Block> otherChain)
        {
            for (int i = 1; i < otherChain.Count; i++)
            {
                if (!ValidateBlock(otherChain[i], otherChain[i - 1]))
                    return false;
            }
            return true;
        }

        public void ChooseCorrectChain(List<Block> otherChain)      //zaradi sočasnosti lahko dobim različna stanja verig, velja daljša
        {
            if (otherChain.Count > Chain.Count && ValidateOtherChain(otherChain))
                Chain = new List<Block>(otherChain);
        }

        public int AdjustDifficulty()
        {
            int timeExpected = GenerateInterval * AdjustDiffInterval;   //pričakovan čas generiranja bloka

            if (Chain.Count < AdjustDiffInterval)   //če veriga nima več blokov kot v const, ne spreminjamo težavnosti
                return -1;

            var latestBlock = Chain.Last();
            var previousAdjustmentBlock = Chain[Chain.Count - AdjustDiffInterval];

            double timeTaken = (latestBlock.Timestamp - previousAdjustmentBlock.Timestamp).TotalSeconds;

            if (timeTaken < timeExpected / 2)
                return previousAdjustmentBlock.Difficulty + 1;
            else if (timeTaken > (timeExpected * 2))
                return previousAdjustmentBlock.Difficulty - 1;
            else
                return previousAdjustmentBlock.Difficulty;
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Block FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Block>(json);
        }
    }
}
