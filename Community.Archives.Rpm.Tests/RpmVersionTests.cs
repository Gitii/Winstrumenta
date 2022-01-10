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

using FluentAssertions;
using NUnit.Framework;

namespace Community.Archives.Rpm.Tests
{
    public class RpmVersionTests
    {
        [TestCase("1.0", "1.0", 0)]
        [TestCase("1.0", "2.0", -1)]
        [TestCase("2.0", "1.0", 1)]
        [TestCase("2.0.1", "2.0.1", 0)]
        [TestCase("2.0", "2.0.1", -1)]
        [TestCase("2.0.1", "2.0", 1)]
        [TestCase("2.0.1a", "2.0.1a", 0)]
        [TestCase("2.0.1a", "2.0.1", 1)]
        [TestCase("2.0.1", "2.0.1a", -1)]
        [TestCase("5.5p1", "5.5p1", 0)]
        [TestCase("5.5p1", "5.5p2", -1)]
        [TestCase("5.5p2", "5.5p1", 1)]
        [TestCase("5.5p10", "5.5p10", 0)]
        [TestCase("5.5p1", "5.5p10", -1)]
        [TestCase("5.5p10", "5.5p1", 1)]
        [TestCase("10xyz", "10.1xyz", -1)]
        [TestCase("10.1xyz", "10xyz", 1)]
        [TestCase("xyz10", "xyz10", 0)]
        [TestCase("xyz10", "xyz10.1", -1)]
        [TestCase("xyz10.1", "xyz10", 1)]
        [TestCase("xyz.4", "xyz.4", 0)]
        [TestCase("xyz.4", "8", -1)]
        [TestCase("8", "xyz.4", 1)]
        [TestCase("xyz.4", "2", -1)]
        [TestCase("2", "xyz.4", 1)]
        [TestCase("5.5p2", "5.6p1", -1)]
        [TestCase("5.6p1", "5.5p2", 1)]
        [TestCase("5.6p1", "6.5p1", -1)]
        [TestCase("6.5p1", "5.6p1", 1)]
        [TestCase("6.0.rc1", "6.0", 1)]
        [TestCase("6.0", "6.0.rc1", -1)]
        [TestCase("10b2", "10a1", 1)]
        [TestCase("10a2", "10b2", -1)]
        [TestCase("1.0aa", "1.0aa", 0)]
        [TestCase("1.0a", "1.0aa", -1)]
        [TestCase("1.0aa", "1.0a", 1)]
        [TestCase("10.0001", "10.0001", 0)]
        [TestCase("10.0001", "10.1", 0)]
        [TestCase("10.1", "10.0001", 0)]
        [TestCase("10.0001", "10.0039", -1)]
        [TestCase("10.0039", "10.0001", 1)]
        [TestCase("4.999.9", "5.0", -1)]
        [TestCase("5.0", "4.999.9", 1)]
        [TestCase("20101121", "20101121", 0)]
        [TestCase("20101121", "20101122", -1)]
        [TestCase("20101122", "20101121", 1)]
        [TestCase("2_0", "2_0", 0)]
        [TestCase("2.0", "2_0", 0)]
        [TestCase("2_0", "2.0", 0)]
        // RhBug:178798 case
        [TestCase("a", "a", 0)]
        [TestCase("a+", "a+", 0)]
        [TestCase("a+", "a_", 0)]
        [TestCase("a_", "a+", 0)]
        [TestCase("+a", "+a", 0)]
        [TestCase("+a", "_a", 0)]
        [TestCase("_a", "+a", 0)]
        [TestCase("+_", "+_", 0)]
        [TestCase("_+", "+_", 0)]
        [TestCase("_+", "_+", 0)]
        [TestCase("+", "_", 0)]
        [TestCase("_", "+", 0)]
        // Basic testcases for tilde sorting
        [TestCase("1.0~rc1", "1.0~rc1", 0)]
        [TestCase("1.0~rc1", "1.0", -1)]
        [TestCase("1.0", "1.0~rc1", 1)]
        [TestCase("1.0~rc1", "1.0~rc2", -1)]
        [TestCase("1.0~rc2", "1.0~rc1", 1)]
        [TestCase("1.0~rc1~git123", "1.0~rc1~git123", 0)]
        [TestCase("1.0~rc1~git123", "1.0~rc1", -1)]
        [TestCase("1.0~rc1", "1.0~rc1~git123", 1)]
        // Basic testcases for caret sorting
        [TestCase("1.0^", "1.0^", 0)]
        [TestCase("1.0^", "1.0", 1)]
        [TestCase("1.0", "1.0^", -1)]
        [TestCase("1.0^git1", "1.0^git1", 0)]
        [TestCase("1.0^git1", "1.0", 1)]
        [TestCase("1.0", "1.0^git1", -1)]
        [TestCase("1.0^git1", "1.0^git2", -1)]
        [TestCase("1.0^git2", "1.0^git1", 1)]
        [TestCase("1.0^git1", "1.01", -1)]
        [TestCase("1.01", "1.0^git1", 1)]
        [TestCase("1.0^20160101", "1.0^20160101", 0)]
        [TestCase("1.0^20160101", "1.0.1", -1)]
        [TestCase("1.0.1", "1.0^20160101", 1)]
        [TestCase("1.0^20160101^git1", "1.0^20160101^git1", 0)]
        [TestCase("1.0^20160102", "1.0^20160101^git1", 1)]
        [TestCase("1.0^20160101^git1", "1.0^20160102", -1)]
        // Basic testcases for tilde and caret sorting
        [TestCase("1.0~rc1^git1", "1.0~rc1^git1", 0)]
        [TestCase("1.0~rc1^git1", "1.0~rc1", 1)]
        [TestCase("1.0~rc1", "1.0~rc1^git1", -1)]
        [TestCase("1.0^git1~pre", "1.0^git1~pre", 0)]
        [TestCase("1.0^git1", "1.0^git1~pre", 1)]
        [TestCase("1.0^git1~pre", "1.0^git1", -1)]
        [Test]
        public void Test_CompareVersions(string first, string second, int expectedComparision)
        {
            RpmVersion.Compare(first, second).Should().Be(expectedComparision);
        }
    }
}
