using System;
using System.IO;
using Xunit;

namespace ClientTests
{
    public class Tests
    {
        private readonly Client _client = new Client();
        
        [Fact]
        public void DataInputTest()
        {
            Console.SetIn(new StringReader("fakeInput"));
            var result = Client.DataInput();
            Assert.Equal("fakeInput", result);
        }
        
        [Fact]
        public void DataInputTestNotCorrect()
        {
            Console.SetIn(new StringReader("asd"));
            Console.SetIn(new StringReader("asdf"));
            Console.SetIn(new StringReader("asdfg"));
            Console.SetIn(new StringReader("asdfgh"));
            Console.SetIn(new StringReader("asdfghj"));
            Console.SetIn(new StringReader("asdfghjk"));
            var result = Client.DataInput();
            Assert.Equal("asdfghjk", result);
        }
        
        [Fact]
        public void IdInputTest()
        {
            Console.SetIn(new StringReader("fakeInput"));
            var result = Client.IdInput();
            Assert.Equal("fakeInput", result);
        }
        
        [Fact]
        public void IdInputTestNotCorrect()
        {
            Console.SetIn(new StringReader("asd"));
            Console.SetIn(new StringReader("asdf"));
            Console.SetIn(new StringReader("asdfg"));
            var result = Client.IdInput();
            Assert.Equal("asdfg", result);
        }
    }
}