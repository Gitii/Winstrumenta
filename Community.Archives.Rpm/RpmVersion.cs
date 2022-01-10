// Copyright (c) SAS Institute Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System.Text.RegularExpressions;

namespace Community.Archives.Rpm
{
    public static class RpmVersion
    {
        private static Regex FindNonAlphaNumericPrefix = new Regex(@"^([^a-zA-Z0-9~\^]*)(.*)$");
        private static Regex IsNumeric = new Regex(@"^([\d]+)(.*)$");
        private static Regex IsAlpha = new Regex(@"^([a-zA-Z]+)(.*)$");

        public static int Compare(string first, string second)
        {
            bool isnum;
            while (!string.IsNullOrEmpty(first) || !string.IsNullOrEmpty(second))
            {
                var m1 = FindNonAlphaNumericPrefix.Match(first);
                var m2 = FindNonAlphaNumericPrefix.Match(second);
                var m1Head = m1.Groups[1].Value;
                first = m1.Groups[2].Value; // tail
                var m2Head = m2.Groups[1].Value;
                second = m2.Groups[2].Value; // tail
                if (!string.IsNullOrEmpty(m1Head) || !string.IsNullOrEmpty(m2Head))
                {
                    // Ignore junk at the beginning
                    continue;
                }

                // handle the tilde separator, it sorts before everything else
                if (first.StartsWith('~'))
                {
                    if (!second.StartsWith('~'))
                    {
                        return -1;
                    }

                    first = first.Substring(1);
                    second = second.Substring(1);
                    continue;
                }

                if (second.StartsWith('~'))
                {
                    return 1;
                }

                // Now look at the caret, which is like the tilde but pointier.
                if (first.StartsWith('^'))
                {
                    // first has a caret but second has ended
                    if (string.IsNullOrEmpty(second))
                    {
                        return 1; // first > second
                    }

                    // first has a caret but second continues on
                    if (!second.StartsWith('^'))
                    {
                        return -1; // first < second
                    }

                    // strip the ^ and start again
                    first = first.Substring(1);
                    second = second.Substring(1);
                    continue;
                }

                // Caret means the version is less... Unless the other version
                // has ended, then do the exact opposite.
                if (second.StartsWith('^'))
                {
                    if (string.IsNullOrEmpty(first))
                    {
                        return -1;
                    }

                    return 1;
                }

                // We've run out of characters to compare.
                // Note: we have to do this after we compare the ~ and ^ madness
                // because ~'s and ^'s take precedance.
                // If we ran to the end of either, we are finished with the loop
                if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second))
                {
                    break;
                }

                // grab first completely alpha or completely numeric segment
                m1 = IsNumeric.Match(first);
                if (m1.Success)
                {
                    m2 = IsNumeric.Match(second);
                    if (!m2.Success)
                    {
                        // numeric segments are always newer than alpha segments
                        return 1;
                    }

                    isnum = true;
                }
                else
                {
                    m1 = IsAlpha.Match(first);
                    m2 = IsAlpha.Match(second);
                    if (!m2.Success)
                    {
                        return -1;
                    }

                    isnum = false;
                }

                m1Head = m1.Groups[1].Value;
                first = m1.Groups[2].Value; // tail
                m2Head = m2.Groups[1].Value;
                second = m2.Groups[2].Value; // tail
                if (isnum)
                {
                    var m1Num = Int32.Parse(m1Head);
                    var m2Num = Int32.Parse(m2Head);

                    var cmp = m1Num.CompareTo(m2Num);

                    if (cmp < 0)
                    {
                        return -1;
                    }

                    if (cmp > 0)
                    {
                        return 1;
                    }

                    // throw away any leading zeros - it's a number, right?
                    m1Head = m1Head.TrimStart('0');
                    m2Head = m2Head.TrimStart('0');

                    // whichever number has more digits wins
                    if (m1Head.Length < m2Head.Length)
                    {
                        return -1;
                    }

                    if (m1Head.Length > m2Head.Length)
                    {
                        return 1;
                    }
                }
                else
                {
                    var headCmp = String.Compare(m1Head, m2Head, StringComparison.Ordinal);

                    if (headCmp < 0)
                    {
                        return -1;
                    }

                    if (headCmp > 0)
                    {
                        return 1;
                    }
                }

                // Same number of chars
                if (m1Head.Length < m2Head.Length)
                {
                    return -1;
                }

                if (m1Head.Length > m2Head.Length)
                {
                    return 1;
                }

                // Both segments equal
                // continue with next segment
            }

            if (first.Length == 0 && second.Length == 0)
            {
                return 0;
            }

            if (first.Length != 0)
            {
                return 1;
            }

            return -1;
        }
    }
}