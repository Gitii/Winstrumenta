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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Community.Archives.Rpm
{
    public class RpmVersion
    {
        public static Regex R_NONALNUMTILDE = new Regex(@"^([^a-zA-Z0-9~\^]*)(.*)$");
        public static Regex R_NUM = new Regex(@"^([\d]+)(.*)$");
        public static Regex R_ALPHA = new Regex(@"^([a-zA-Z]+)(.*)$");

        public static int compare(string first, string second)
        {
            bool isnum;
            while (!string.IsNullOrEmpty(first) || !string.IsNullOrEmpty(second))
            {
                var m1 = R_NONALNUMTILDE.Match(first);
                var m2 = R_NONALNUMTILDE.Match(second);
                var m1_head = m1.Groups[1].Value;
                first = m1.Groups[2].Value;
                var m2_head = m2.Groups[1].Value;
                second = m2.Groups[2].Value;
                if (!string.IsNullOrEmpty(m1_head) || !string.IsNullOrEmpty(m2_head))
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
                m1 = R_NUM.Match(first);
                if (m1.Success)
                {
                    m2 = R_NUM.Match(second);
                    if (!m2.Success)
                    {
                        // numeric segments are always newer than alpha segments
                        return 1;
                    }

                    isnum = true;
                }
                else
                {
                    m1 = R_ALPHA.Match(first);
                    m2 = R_ALPHA.Match(second);
                    if (!m2.Success)
                    {
                        return -1;
                    }

                    isnum = false;
                }

                m1_head = m1.Groups[1].Value;
                first = m1.Groups[2].Value;
                m2_head = m2.Groups[1].Value;
                second = m2.Groups[2].Value;
                if (isnum)
                {
                    var m1_num = Int32.Parse(m1_head);
                    var m2_num = Int32.Parse(m2_head);

                    var cmp = m1_num.CompareTo(m2_num);

                    if (cmp < 0)
                    {
                        return -1;
                    }

                    if (cmp > 0)
                    {
                        return 1;
                    }

                    // throw away any leading zeros - it's a number, right?
                    m1_head = m1_head.TrimStart('0');
                    m2_head = m2_head.TrimStart('0');
                    // whichever number has more digits wins
                    var m1hlen = m1_head.Length;
                    var m2hlen = m2_head.Length;
                    if (m1hlen < m2hlen)
                    {
                        return -1;
                    }

                    if (m1hlen > m2hlen)
                    {
                        return 1;
                    }
                }
                else
                {
                    var head_cmp = String.Compare(m1_head, m2_head, StringComparison.Ordinal);

                    if (head_cmp < 0)
                    {
                        return -1;
                    }

                    if (head_cmp > 0)
                    {
                        return 1;
                    }
                }

                // Same number of chars
                if (m1_head.Length < m2_head.Length)
                {
                    return -1;
                }

                if (m1_head.Length > m2_head.Length)
                {
                    return 1;
                }
                // Both segments equal
                continue;
            }

            var m1len = first.Length;
            var m2len = second.Length;
            if (m1len == 0 && m2len == 0)
            {
                return 0;
            }

            if (m1len != 0)
            {
                return 1;
            }

            return -1;
        }
    }
}
