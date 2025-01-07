using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace blockchainGUI.CodeBehind
{
    public class Block
    {
        public int Index { get; set; }
        public string Data { get; set; }
        public DateTime Timestamp { get; set; }
        public string Hash { get; set; }
        public string PreviousHash { get; set; }
        public int Difficulty { get; set; }
        public int Nonce { get; set; }

        public string CalculateHash()
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var input = $"{Index}{Timestamp}{Data}{PreviousHash}";
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(inputBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public void MineBlock()
        {
            string format = new string('0', Difficulty);
            while (true) 
            {
                Hash = CalculateHash();
                if (Hash.StartsWith(format))
                    break;
                Nonce++;
            }
        }
    }
}
