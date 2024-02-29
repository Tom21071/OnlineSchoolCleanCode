using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineSchool.Application.EncryptionServiceInterface
{
    public interface IEncryptionService
    {
        public byte[] EncryptMessage(string message);
        public string DecryptMessage(byte[] ciphertext);
    }
}
