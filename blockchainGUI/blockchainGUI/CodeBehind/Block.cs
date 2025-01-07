using Newtonsoft.Json;
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
        public int Difficulty { get; set; }     //težavnost je prikazana kot število ničel, ki jih mora imeti hash na začetku, da je blok veljaven
        public int Nonce { get; set; }      //izraz za žeton za enkratno uporabo

        public string CalculateHash()
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var input = $"{Index}{Timestamp}{Data}{PreviousHash}{Difficulty}{Nonce}";
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(inputBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public void MineBlock()     //proof-of-work
        {
            Nonce = 0;
            string format = new string('0', Difficulty);
            while (true) 
            {
                Hash = CalculateHash();
                if (Hash.StartsWith(format))
                {
                    Console.WriteLine($"Block mined! Nonce: {Nonce}, Hash: {Hash}");    //če zgoščena vrednost ustreza bo prekinilo loop in šlo naprej na validacijo
                    break;
                }
                Nonce++;
            }
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
