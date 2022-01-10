namespace PackageInstaller.Core.Helpers;

using System;

public static class SpanExtensions
{
    public static SplittedSpanEntry Split(this ReadOnlySpan<char> span, char separator)
    {
        ReadOnlySpan<char> part1 = ReadOnlySpan<char>.Empty;
        ReadOnlySpan<char> part2 = ReadOnlySpan<char>.Empty;
        ReadOnlySpan<char> part3 = ReadOnlySpan<char>.Empty;
        ReadOnlySpan<char> part4 = ReadOnlySpan<char>.Empty;
        ReadOnlySpan<char> part5 = ReadOnlySpan<char>.Empty;
        ReadOnlySpan<char> part6 = ReadOnlySpan<char>.Empty;
        ReadOnlySpan<char> part7 = ReadOnlySpan<char>.Empty;
        ReadOnlySpan<char> part8 = ReadOnlySpan<char>.Empty;
        ReadOnlySpan<char> part9 = ReadOnlySpan<char>.Empty;

        var hitCounter = 0;
        var offset = 0;

        _ =
            FindSeparatorAndSplit(ref offset, ref hitCounter, span, out part1)
            && FindSeparatorAndSplit(ref offset, ref hitCounter, span, out part2)
            && FindSeparatorAndSplit(ref offset, ref hitCounter, span, out part3)
            && FindSeparatorAndSplit(ref offset, ref hitCounter, span, out part4)
            && FindSeparatorAndSplit(ref offset, ref hitCounter, span, out part5)
            && FindSeparatorAndSplit(ref offset, ref hitCounter, span, out part6)
            && FindSeparatorAndSplit(ref offset, ref hitCounter, span, out part7)
            && FindSeparatorAndSplit(ref offset, ref hitCounter, span, out part8)
            && FindSeparatorAndSplit(ref offset, ref hitCounter, span, out part9);

        return new SplittedSpanEntry(
            part1,
            part2,
            part3,
            part4,
            part5,
            part6,
            part7,
            part8,
            part9,
            hitCounter + 1 // +1 because n hits == (n+1) parts
        );

        bool FindSeparatorAndSplit(
            ref int offset,
            ref int hitCounter,
            ReadOnlySpan<char> completeSpan,
            out ReadOnlySpan<char> part
        )
        {
            if (offset >= completeSpan.Length)
            {
                part = ReadOnlySpan<char>.Empty;

                return false;
            }

            var searchedSpan = completeSpan.Slice(offset);
            var foundIndex = searchedSpan.IndexOf(separator);
            if (foundIndex >= 0)
            {
                part = searchedSpan.Slice(0, foundIndex);
                offset += foundIndex + 1; // +1 for separator
                hitCounter += 1;
                return true;
            }
            else
            {
                part = searchedSpan;
                return false;
            }
        }
    }

    public readonly ref struct SplittedSpanEntry
    {
        public readonly int NumberOfParts;

        public readonly ReadOnlySpan<char> Part1;
        public readonly ReadOnlySpan<char> Part2;
        public readonly ReadOnlySpan<char> Part3;
        public readonly ReadOnlySpan<char> Part4;
        public readonly ReadOnlySpan<char> Part5;
        public readonly ReadOnlySpan<char> Part6;
        public readonly ReadOnlySpan<char> Part7;
        public readonly ReadOnlySpan<char> Part8;
        public readonly ReadOnlySpan<char> Part9;

        public SplittedSpanEntry(
            ReadOnlySpan<char> part1,
            ReadOnlySpan<char> part2,
            ReadOnlySpan<char> part3,
            ReadOnlySpan<char> part4,
            ReadOnlySpan<char> part5,
            ReadOnlySpan<char> part6,
            ReadOnlySpan<char> part7,
            ReadOnlySpan<char> part8,
            ReadOnlySpan<char> part9,
            int numberOfParts
        )
        {
            Part1 = part1;
            Part2 = part2;
            Part3 = part3;
            Part4 = part4;
            Part5 = part5;
            Part6 = part6;
            Part7 = part7;
            Part8 = part8;
            Part9 = part9;
            NumberOfParts = numberOfParts;
        }

        public void AssertEnoughHits(int minHits)
        {
            if (NumberOfParts < minHits)
            {
                throw new ArgumentException($"Found only {minHits} parts!");
            }
        }

        public void Deconstruct(out ReadOnlySpan<char> part1)
        {
            AssertEnoughHits(1);

            part1 = Part1;
        }

        public void Deconstruct(out ReadOnlySpan<char> part1, out ReadOnlySpan<char> part2)
        {
            AssertEnoughHits(2);
            part1 = Part1;
            part2 = Part2;
        }

        public void Deconstruct(
            out ReadOnlySpan<char> part1,
            out ReadOnlySpan<char> part2,
            out ReadOnlySpan<char> part3
        )
        {
            AssertEnoughHits(3);
            part1 = Part1;
            part2 = Part2;
            part3 = Part3;
        }

        public void Deconstruct(
            out ReadOnlySpan<char> part1,
            out ReadOnlySpan<char> part2,
            out ReadOnlySpan<char> part3,
            out ReadOnlySpan<char> part4
        )
        {
            AssertEnoughHits(4);

            part1 = Part1;
            part2 = Part2;
            part3 = Part3;
            part4 = Part4;
        }

        public void Deconstruct(
            out ReadOnlySpan<char> part1,
            out ReadOnlySpan<char> part2,
            out ReadOnlySpan<char> part3,
            out ReadOnlySpan<char> part4,
            out ReadOnlySpan<char> part5
        )
        {
            AssertEnoughHits(5);

            part1 = Part1;
            part2 = Part2;
            part3 = Part3;
            part4 = Part4;
            part5 = Part5;
        }

        public void Deconstruct(
            out ReadOnlySpan<char> part1,
            out ReadOnlySpan<char> part2,
            out ReadOnlySpan<char> part3,
            out ReadOnlySpan<char> part4,
            out ReadOnlySpan<char> part5,
            out ReadOnlySpan<char> part6
        )
        {
            AssertEnoughHits(6);

            part1 = Part1;
            part2 = Part2;
            part3 = Part3;
            part4 = Part4;
            part5 = Part5;
            part6 = Part6;
        }

        public void Deconstruct(
            out ReadOnlySpan<char> part1,
            out ReadOnlySpan<char> part2,
            out ReadOnlySpan<char> part3,
            out ReadOnlySpan<char> part4,
            out ReadOnlySpan<char> part5,
            out ReadOnlySpan<char> part6,
            out ReadOnlySpan<char> part7
        )
        {
            AssertEnoughHits(7);

            part1 = Part1;
            part2 = Part2;
            part3 = Part3;
            part4 = Part4;
            part5 = Part5;
            part6 = Part6;
            part7 = Part7;
        }

        public void Deconstruct(
            out ReadOnlySpan<char> part1,
            out ReadOnlySpan<char> part2,
            out ReadOnlySpan<char> part3,
            out ReadOnlySpan<char> part4,
            out ReadOnlySpan<char> part5,
            out ReadOnlySpan<char> part6,
            out ReadOnlySpan<char> part7,
            out ReadOnlySpan<char> part8
        )
        {
            AssertEnoughHits(8);

            part1 = Part1;
            part2 = Part2;
            part3 = Part3;
            part4 = Part4;
            part5 = Part5;
            part6 = Part6;
            part7 = Part7;
            part8 = Part8;
        }

        public void Deconstruct(
            out ReadOnlySpan<char> part1,
            out ReadOnlySpan<char> part2,
            out ReadOnlySpan<char> part3,
            out ReadOnlySpan<char> part4,
            out ReadOnlySpan<char> part5,
            out ReadOnlySpan<char> part6,
            out ReadOnlySpan<char> part7,
            out ReadOnlySpan<char> part8,
            out ReadOnlySpan<char> part9
        )
        {
            AssertEnoughHits(9);

            part1 = Part1;
            part2 = Part2;
            part3 = Part3;
            part4 = Part4;
            part5 = Part5;
            part6 = Part6;
            part7 = Part7;
            part8 = Part8;
            part9 = Part9;
        }
    }

    public static LineSplittingEnumerator SplitLines(this ReadOnlySpan<char> span)
    {
        // LineSplitEnumerator is a struct so there is no allocation here
        return new LineSplittingEnumerator(span);
    }

    // Must be a ref struct as it contains a ReadOnlySpan<char>
    public ref struct LineSplittingEnumerator
    {
        private ReadOnlySpan<char> _str;

        public LineSplittingEnumerator(ReadOnlySpan<char> str)
        {
            _str = str;
            Current = default;
        }

        // Needed to be compatible with the foreach operator
        public LineSplittingEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            var span = _str;
            if (span.Length == 0) // Reach the end of the string
                return false;

            var index = span.IndexOfAny('\r', '\n');
            if (index == -1) // The string is composed of only one line
            {
                _str = ReadOnlySpan<char>.Empty; // The remaining string is an empty string
                Current = new LineEntry(span, ReadOnlySpan<char>.Empty);
                return true;
            }

            if (index < span.Length - 1 && span[index] == '\r')
            {
                // Try to consume the '\n' associated to the '\r'
                var next = span[index + 1];
                if (next == '\n')
                {
                    Current = new LineEntry(span.Slice(0, index), span.Slice(index, 2));
                    _str = span.Slice(index + 2);
                    return true;
                }
            }

            Current = new LineEntry(span.Slice(0, index), span.Slice(index, 1));
            _str = span.Slice(index + 1);
            return true;
        }

        public LineEntry Current { get; private set; }
    }

    public readonly ref struct LineEntry
    {
        public LineEntry(ReadOnlySpan<char> line, ReadOnlySpan<char> separator)
        {
            Line = line;
            Separator = separator;
        }

        public ReadOnlySpan<char> Line { get; }
        public ReadOnlySpan<char> Separator { get; }

        // This method allow to deconstruct the type, so you can write any of the following code
        // foreach (var entry in str.SplitLines()) { _ = entry.Line; }
        // foreach (var (line, endOfLine) in str.SplitLines()) { _ = line; }
        // https://docs.microsoft.com/en-us/dotnet/csharp/deconstruct?WT.mc_id=DT-MVP-5003978#deconstructing-user-defined-types
        public void Deconstruct(out ReadOnlySpan<char> line, out ReadOnlySpan<char> separator)
        {
            line = Line;
            separator = Separator;
        }

        // This method allow to implicitly cast the type into a ReadOnlySpan<char>, so you can write the following code
        // foreach (ReadOnlySpan<char> entry in str.SplitLines())
        public static implicit operator ReadOnlySpan<char>(LineEntry entry) => entry.Line;
    }
}
