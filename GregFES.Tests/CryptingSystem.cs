using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using NSubstitute;

namespace GregFES.Tests
{
    [TestClass]
    public class CryptingSystemTest
    {
        [TestMethod]
        public void CalculateSHA256NotEqual()
        {
            CalculateSHA256(new byte[][] { CryptingSystem.GenerateRandomSalt(), CryptingSystem.GenerateRandomSalt() }, false);
        }

        [TestMethod]
        public void GenerateRandomSaltTestNotEqual()
        {
            Assert.AreNotEqual(CryptingSystem.GenerateRandomSalt(), CryptingSystem.GenerateRandomSalt());
        }

        [TestMethod]
        public void CalculateSHA256Equal()
        {
            CalculateSHA256(new byte[][] { new byte[] { 0 }, new byte[] { 0 } }, true);
        }

        public void CalculateSHA256(byte[][] salt, bool resultExpected)
        {
            FileStream[] fs = { new FileStream(System.IO.Directory.GetCurrentDirectory() + "\\test0.gregtest", FileMode.Create), new FileStream(System.IO.Directory.GetCurrentDirectory() + "\\test1.gregtest", FileMode.Create) };
            string[] SHA256 = new string[2];
            for (int i = 0; i < 2; i++)
            {
                fs[i].Write(salt[i], 0, salt[i].Length);
                fs[i].Close();
            }

            for (int i = 0; i < 2; i++)
            {
                SHA256[i] = CryptingSystem.CalculateSHA256(System.IO.Directory.GetCurrentDirectory() + $"\\test{i}.gregtest");
                File.Delete(System.IO.Directory.GetCurrentDirectory() + $"\\test{i}.gregtest");
            }

            if (resultExpected)
            {
                Assert.AreEqual(SHA256[0], SHA256[1]);
            }
            else
            {
                Assert.AreNotEqual(SHA256[0], SHA256[1]);
            }
        }
    }
}
