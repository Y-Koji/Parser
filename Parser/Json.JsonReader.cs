using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    partial class Json
    {
        private class JsonReader
        {
            const string MSG_OUT_OF_RANGE = "Json文字列サイズの境界範囲を超えてアクセスしようとしました。";

            public string Json { get; private set; } = string.Empty;
            public int Size { get; private set; } = 0;
            public int Position { get; private set; } = 0;

            public JsonReader(string json)
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new ArgumentNullException(nameof(json));
                }

                Json = json;
                Size = json.Length;
            }

            public char Read()
            {
                if (Position < Size)
                {
                    return Json[Position++];
                }
                else
                {
                    if (Position.Equals(Size))
                    {
                        return char.MinValue;
                    }

                    throw new IndexOutOfRangeException(MSG_OUT_OF_RANGE);
                }
            }

            public char Peek()
            {
                if (Position < Size)
                {
                    return Json[Position];
                }
                else
                {
                    if (Position.Equals(Size))
                    {
                        return char.MinValue;
                    }

                    throw new IndexOutOfRangeException(MSG_OUT_OF_RANGE);
                }
            }

            public int TrimStart()
            {
                int count = 0;

                while (char.IsWhiteSpace(Peek()))
                {
                    Read();

                    count++;
                }

                return count;
            }
        }
    }
}
