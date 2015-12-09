using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ChinaTelecom.Grid.Lib;

namespace ChinaTelecom.Grid.Tests
{
    public class AddressAnalyserTests
    {
        [Theory]
        [InlineData("blablabla", "blablabla")]
        [InlineData("blablabla（）", "blablabla")]
        [InlineData("blablabla（（））", "blablabla")]
        [InlineData("blablabla（）（）", "blablabla")]
        [InlineData("blablabla（123）（321）", "blablabla")]
        public void filter_brackets_tests(string src, string expected)
        {
            // Act
            var result = AddressAnalyser.FilterBrackets(src);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("哈尔滨市道里区恒祥城2期9栋1单元1001室", 4, "2911001")]
        [InlineData("哈尔滨市道里区安心街114号3栋3单元301室", 4, "11433301")]
        [InlineData("哈尔滨市香坊区睿城小区A5号楼3单元301室", 3, "A53301")]
        [InlineData("哈尔滨市南岗区恒祥家园富华轩1单元20楼B", 3, "120B")]
        [InlineData("哈尔滨市道里区提拉米苏小区C栋3单元603室", 3, "C3603")]
        [InlineData("哈尔滨道里区安静二胡同12-1号3单元301室", 3, "12-13301")]
        [InlineData("", 0, "")]
        public void get_numbers_tests(string src, int count, string expected)
        {
            // Act 1
            var result = AddressAnalyser.GetNumbers(src);

            // Assert 1
            Assert.Equal(count, result.Count);

            // Act 2
            var concat = "";
            foreach (var x in result)
                concat += x;

            // Assert 2
            Assert.Equal(expected, concat);
        }
    }
}
