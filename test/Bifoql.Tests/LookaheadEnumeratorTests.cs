using System;
using System.Linq;
using Xunit;
using Bifoql.Lex;

namespace Bifoql.Tests
{
    public class LookaheadEnumeratorTests
    {
        [Fact]
        public void Empty()
        {
            var enumerator = new LookaheadEnumerator<char>("".GetEnumerator(), 1);
            Assert.False(enumerator.MoveNext());
            Assert.Equal((char)0, enumerator.Current);
            Assert.Equal((char)0, enumerator.Values[1]);
        }

        [Fact]
        public void OneChar()
        {
            var enumerator = new LookaheadEnumerator<char>("x".GetEnumerator(), 1);
            Assert.True(enumerator.MoveNext());
            Assert.Equal('x', enumerator.Current);
            Assert.Equal((char)0, enumerator.Values[1]);
            Assert.False(enumerator.MoveNext());
            Assert.Equal((char)0, enumerator.Current);
            Assert.Equal((char)0, enumerator.Values[1]);
        }


        [Fact]
        public void Unwind()
        {
            var enumerator = new LookaheadEnumerator<char>("x".GetEnumerator(), 1);
            Assert.True(enumerator.MoveNext());
            Assert.Equal('x', enumerator.Current);
            Assert.Equal((char)0, enumerator.Values[1]);
            enumerator.Unwind();

            Assert.True(enumerator.MoveNext());
            Assert.Equal('x', enumerator.Current);
            Assert.Equal((char)0, enumerator.Values[1]);
        }

        [Fact]
        public void TwoChar()
        {
            var enumerator = new LookaheadEnumerator<char>("xy".GetEnumerator(), 1);
            Assert.True(enumerator.MoveNext());
            Assert.Equal('x', enumerator.Current);
            Assert.Equal('y', enumerator.Values[1]);
            Assert.True(enumerator.MoveNext());
            Assert.Equal('y', enumerator.Current);
            Assert.Equal((char)0, enumerator.Values[1]);
            Assert.False(enumerator.MoveNext());
            Assert.Equal((char)0, enumerator.Current);
            Assert.Equal((char)0, enumerator.Values[1]);
        }

        [Fact]
        public void ThreeChar()
        {
            var enumerator = new LookaheadEnumerator<char>("xyz".GetEnumerator(), 1);
            Assert.True(enumerator.MoveNext());
            Assert.Equal('x', enumerator.Current);
            Assert.Equal('y', enumerator.Values[1]);
            Assert.True(enumerator.MoveNext());
            Assert.Equal('y', enumerator.Current);
            Assert.Equal('z', enumerator.Values[1]);
            Assert.True(enumerator.MoveNext());
            Assert.Equal('z', enumerator.Current);
            Assert.Equal((char)0, enumerator.Values[1]);
            Assert.False(enumerator.MoveNext());
            Assert.Equal((char)0, enumerator.Current);
            Assert.Equal((char)0, enumerator.Values[1]);
        }

        [Fact]
        public void Reset()
        {
            var enumerator = new LookaheadEnumerator<char>("xy".GetEnumerator(), 1);
            Assert.True(enumerator.MoveNext());
            Assert.Equal('x', enumerator.Current);
            Assert.Equal('y', enumerator.Values[1]);
            Assert.True(enumerator.MoveNext());
            Assert.Equal('y', enumerator.Current);
            Assert.Equal((char)0, enumerator.Values[1]);
            Assert.False(enumerator.MoveNext());
            Assert.Equal((char)0, enumerator.Current);
            Assert.Equal((char)0, enumerator.Values[1]);
            enumerator.Reset();
            Assert.True(enumerator.MoveNext());
            Assert.Equal('x', enumerator.Current);
            Assert.Equal('y', enumerator.Values[1]);
            Assert.True(enumerator.MoveNext());
            Assert.Equal('y', enumerator.Current);
            Assert.Equal((char)0, enumerator.Values[1]);
            Assert.False(enumerator.MoveNext());
            Assert.Equal((char)0, enumerator.Current);
            Assert.Equal((char)0, enumerator.Values[1]);
        }
    }
}
