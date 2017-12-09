using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser
{
    partial class Json
    {
        private readonly JsonReader _reader;
        private readonly StringBuilder _builder;
        private readonly dynamic _object;

        public dynamic Object => _object;

        private Exception GetInvalidTokenException()
        {
            string json = _reader.Json.Substring(_reader.Position, 0);
            string message =
                string.Format(
                    "予期しない文字`{0}`が見つかりました({1}番目、「{2}」)",
                    _reader.Peek(), _reader.Position, json);

            return new FormatException(message);
        }

        private Exception GetInvalidParsingException()
        {
            return new FormatException($"文字列解析中に不正な制御文字が見つかりました(\\{_reader.Peek()}");
        }

        public Json(string json)
        {
            _reader = new JsonReader(json);
            _builder = new StringBuilder();
            _object = Parse();
        }

        private dynamic Parse()
        {
            return ParseValue();
        }

        private string ParseString()
        {
            _reader.TrimStart();
            _builder.Clear();

            if ('"' == _reader.Peek())
            {
                _reader.Read();
            }
            else
            {
                throw GetInvalidTokenException();
            }

            while ('"' != _reader.Peek())
            {
                if ('\\' == _reader.Peek())
                {
                    _reader.Read();

                    switch (_reader.Peek())
                    {
                        case char peek when '"' == peek:
                            _reader.Read();
                            _builder.Append('\"');
                            break;

                        case char peek when '\\' == peek:
                            _reader.Read();
                            _builder.Append('\\');
                            break;

                        case char peek when '/' == peek:
                            _reader.Read();
                            _builder.Append('/');
                            break;

                        case char peek when 'b' == peek:
                            _reader.Read();
                            _builder.Append("\b");
                            break;

                        case char peek when 'f' == peek:
                            _reader.Read();
                            _builder.Append("\f");
                            break;

                        case char peek when 'n' == peek:
                            _reader.Read();
                            _builder.Append("\n");
                            break;

                        case char peek when 'r' == peek:
                            _reader.Read();
                            _builder.Append("\r");
                            break;

                        case char peek when 't' == peek:
                            _reader.Read();
                            _builder.Append("\t");
                            break;

                        case char peek when 'u' == peek:
                            char u = _reader.Read();
                            char hex0 = _reader.Read();
                            char hex1 = _reader.Read();
                            char hex2 = _reader.Read();
                            char hex3 = _reader.Read();

                            string escape = string.Concat("\\", u, hex0, hex1, hex2, hex3);
                            string unescape = Regex.Unescape(escape);

                            _builder.Append(unescape);
                            break;

                        default: throw GetInvalidParsingException();
                    }
                }
                else
                {
                    _builder.Append(_reader.Read());
                }
            }

            _reader.Read();

            return _builder.ToString();
        }

        private double ParseNumber()
        {
            _reader.TrimStart();
            _builder.Clear();

            if ('+' == _reader.Peek() || '-' == _reader.Peek())
            {
                _builder.Append(_reader.Read());
            }

            if (char.IsDigit(_reader.Peek()))
            {
                while (char.IsDigit(_reader.Peek()))
                {
                    _builder.Append(_reader.Read());
                }
            }
            else
            {
                throw GetInvalidTokenException();
            }

            if ('.' == _reader.Peek())
            {
                _builder.Append(_reader.Read());

                if (char.IsDigit(_reader.Peek()))
                {
                    while (char.IsDigit(_reader.Peek()))
                    {
                        _builder.Append(_reader.Read());
                    }
                }
                else
                {
                    throw GetInvalidTokenException();
                }
            }

            if ('e' == char.ToLower(_reader.Peek()))
            {
                _builder.Append(_reader.Read());

                if ('+' == _reader.Peek() || '-' == _reader.Peek())
                {
                    _builder.Append(_reader.Read());
                }

                if (char.IsDigit(_reader.Peek()))
                {
                    while (char.IsDigit(_reader.Peek()))
                    {
                        _builder.Append(_reader.Read());
                    }
                }
                else
                {
                    throw GetInvalidTokenException();
                }
            }

            string number = _builder.ToString();

            return double.Parse(number);
        }

        private bool ParseBoolean()
        {
            _reader.TrimStart();

            if ('t' == _reader.Peek())
            {
                ValidString("true");

                return true;
            }

            if ('f' == _reader.Peek())
            {
                ValidString("false");

                return false;
            }

            throw GetInvalidTokenException();
        }

        private object ParseNull()
        {
            _reader.TrimStart();

            ValidString("null");

            return null;
        }

        private DynamicList ParseArray()
        {
            _reader.TrimStart();

            List<object> list = new List<object>();

            if ('[' == _reader.Peek())
            {
                _reader.Read();
            }
            else
            {
                throw GetInvalidParsingException();
            }

            while (']' != _reader.Peek())
            {
                object value = ParseValue();

                list.Add(value);

                _reader.TrimStart();
                if (',' != _reader.Peek() && ']' != _reader.Peek())
                {
                    throw GetInvalidTokenException();
                }

                if (',' == _reader.Peek())
                {
                    _reader.Read();
                }
            }

            _reader.Read();

            return new DynamicList(list);
        }

        private DynamicDictionary ParseObject()
        {
            _reader.TrimStart();
            _builder.Clear();
            Dictionary<string, object> obj = new Dictionary<string, object>();

            if ('{' == _reader.Peek())
            {
                _reader.Read();
            }
            else
            {
                throw GetInvalidTokenException();
            }

            while ('}' != _reader.Peek())
            {
                if (IsStringStart())
                {
                    string key = ParseString();

                    _reader.TrimStart();
                    if (':' == _reader.Read())
                    {
                        object value = ParseValue();

                        obj.Add(key, value);
                    }
                    else
                    {
                        throw GetInvalidTokenException();
                    }

                    _reader.TrimStart();
                    if ('}' != _reader.Peek() && ',' != _reader.Peek())
                    {
                        throw GetInvalidTokenException();
                    }

                    if (',' == _reader.Peek())
                    {
                        _reader.Read();
                    }

                }
                else
                {
                    throw GetInvalidTokenException();
                }
            }

            _reader.Read();

            return new DynamicDictionary(obj);
        }

        private object ParseValue()
        {
            if (IsNumberStart())
            {
                return ParseNumber();
            }

            if (IsBooleanStart())
            {
                return ParseBoolean();
            }

            if (IsStringStart())
            {
                return ParseString();
            }

            if (IsArrayStart())
            {
                return ParseArray();
            }

            if (IsObjectStart())
            {
                return ParseObject();
            }

            if (IsNullStart())
            {
                return ParseNull();
            }

            throw GetInvalidTokenException();
        }

        private bool IsNumberStart()
        {
            _reader.TrimStart();

            char peek = _reader.Peek();
            if (char.IsDigit(peek) || '+' == peek || '-' == peek)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsBooleanStart()
        {
            _reader.TrimStart();

            char peek = _reader.Peek();
            if ('t' == peek || 'f' == peek)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsNullStart()
        {
            _reader.TrimStart();

            char peek = _reader.Peek();
            if ('n' == peek)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsStringStart()
        {
            _reader.TrimStart();

            char peek = _reader.Peek();
            if ('"' == peek)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsArrayStart()
        {
            _reader.TrimStart();

            char peek = _reader.Peek();
            if ('[' == peek)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsObjectStart()
        {
            _reader.TrimStart();

            char peek = _reader.Peek();
            if ('{' == peek)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ValidString(string keyword)
        {
            _reader.TrimStart();

            foreach (char @char in keyword)
            {
                char read = _reader.Read();

                if (char.ToLower(@char) != char.ToLower(read))
                {
                    throw GetInvalidParsingException();
                }
            }
        }

        public static dynamic Parse(string json)
        {
            return new Json(json).Parse();
        }

        public override string ToString()
        {
            return _reader.Json;
        }
    }
}
