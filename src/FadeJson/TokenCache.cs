﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace FadeJson
{
    internal class TokenCache : ICommonCache<JsonValue, Tokenizer>
    {
        private readonly JsonValue[] buffer;

        private readonly int bufferMax;
        private readonly Tokenizer tokenizer;
        private int pos;

        public TokenCache(Tokenizer tokenizer, int size = 4) {
            this.tokenizer = tokenizer;
            bufferMax = size;
            buffer = new JsonValue[2*size];
            FlushBuffer(0, bufferMax);
            FlushBuffer(bufferMax, 2*bufferMax);
        }

        public bool Check(IEnumerable<JsonValue> list) {
            var p = 0;
            return list.All(i => i.Equals(Lookahead(p++)));
        }

        public JsonValue Lookahead(int index = 0) {
            if (index < 0 || index > bufferMax) {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (pos >= 2*bufferMax) {
                pos = 0;
                FlushBuffer(bufferMax, 2*bufferMax);
            }
            return buffer[pos + index];
        }

        public JsonValue Next() {
            if (pos == bufferMax) {
                FlushBuffer(0, bufferMax);
                return buffer[pos++];
            }
            if (pos >= 2*bufferMax) {
                pos = 0;
                FlushBuffer(bufferMax, 2*bufferMax);
                return buffer[pos++];
            }
            return buffer[pos++];
        }

        public void Next(int count) {
            for (var i = 0; i < count; i++) {
                Next();
            }
        }

        private void FlushBuffer(int from, int to) {
            for (var i = from; i < to; i++) {
                buffer[i] = tokenizer.GetNextToken();
                if (buffer[i].Type == JsonType.Null) {
                    break;
                }
            }
        }
    }
}