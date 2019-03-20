namespace Bifoql.Lex
{
    using System.Collections;
    using System.Collections.Generic;

    public class LookaheadEnumerator<T> : IEnumerator<T>
    {
        private readonly IEnumerator<T> _enumerator;
        private readonly int _lookahead;

        private T[] _values;
        private int _currentIndex = 0;
        private T _defaultValue;
        private bool[] _moreLeft;

        private bool _first = true;

        public LookaheadEnumerator(IEnumerator<T> enumerator, int lookahead, T defaultValue = default(T))
        {
            _enumerator = enumerator;
            _lookahead = lookahead;
            _values = new T[lookahead+1];
            _moreLeft = new bool[lookahead+1];
            _defaultValue = defaultValue;

            for (int i = 0; i <= _lookahead; i++)
            {
                _values[i] = _defaultValue;
                _moreLeft[i] = true;
            }
        }

        public bool MoveNext()
        {
            if (_first)
            {
                _first = false;
                for (int i = 0; i <= _lookahead; i++)
                {
                    Advance();
                }
            }
            else
            {
                Advance();
            }

            return _moreLeft[_currentIndex];
        }

        public T Current => _values[_currentIndex];
        public IReadOnlyList<T> Values => _values;

        public T Next => _values[_currentIndex+1];

        object IEnumerator.Current => Current;

        public void Reset()
        {
            _enumerator.Reset();

            for (int i = 0; i <= _lookahead; i++)
            {
                _values[i] = _defaultValue;
                _moreLeft[i] = true;
            }

            _first = true;
        }

        public void Dispose()
        {
            _enumerator.Dispose();
        }

        private void Advance()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
            else
            {
                for (var i = 1; i <= _lookahead; i++)
                {
                    _values[i - 1] = _values[i];
                    _moreLeft[i - 1] = _moreLeft[i];
                }

                if (_moreLeft[_lookahead - 1])
                {
                    _moreLeft[_lookahead] = _enumerator.MoveNext();
                    if (_moreLeft[_lookahead])
                    {
                        _values[_lookahead] = _enumerator.Current;
                    }
                    else
                    {
                        _values[_lookahead] = _defaultValue;
                    }
                }
                else
                {
                    _moreLeft[_lookahead] = false;
                    _values[_lookahead] = _defaultValue;
                }
            }
        }

        public void Unwind()
        {
            _currentIndex++;
        }
    }
}