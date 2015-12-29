using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ChinaTelecom.OperateBrace.Lib;

namespace ChinaTelecom.OperateBrace.Tests
{
    public class AddressAnalyserTests
    {
        [Theory]
        [InlineData("blablabla", "blablabla")]
        [InlineData("blablabla（）", "blablabla")]
        [InlineData("blablabla（（））", "blablabla")]
        [InlineData("blablabla（）（）", "blablabla")]
        [InlineData("blablabla（123）（321）", "blablabla")]
        [InlineData("哈尔滨市道里区经纬十一道街9号3单元负101室（D04）", "哈尔滨市道里区经纬十一道街9号3单元负101室")]
        public void filter_brackets_tests(string src, string expected)
        {
            // Act
            var result = AddressAnalyser.FilterBrackets(src);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("哈尔滨市道里区经纬十一道街9号3单元负101室（D04）", 4, "93101D04")]
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

        [Theory]
        [InlineData("哈尔滨市道里区经纬十一道街9号3单元负101室（D04）", -1)]
        [InlineData("哈尔滨市道里区恒祥城2期9栋1单元1001室", 10)]
        [InlineData("哈尔滨市道里区安心街114号3栋3单元301室", 3)]
        [InlineData("哈尔滨市香坊区睿城小区A5号楼3单元301室", 3)]
        [InlineData("哈尔滨市南岗区恒祥家园富华轩1单元20楼B", 20)]
        [InlineData("哈尔滨市道里区提拉米苏小区C栋3单元603室", 6)]
        [InlineData("哈尔滨道里区安静二胡同12-1号3单元301室", 3)]
        [InlineData("哈尔滨市道里区安丰街106号401室", 4)]
        [InlineData("哈尔滨南岗区恒祥家园中华轩2单元16A", 16)]
        [InlineData("", null)]
        public void get_layer_tests(string src, int? expected)
        {
            // Act
            var result = AddressAnalyser.GetLayer(src);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("哈尔滨市道里区工部街18号2单元701室", 1)]
        [InlineData("哈尔滨市道里区恒祥城2期9栋1单元1001室", 1)]
        [InlineData("哈尔滨市道里区安心街114号3栋3单元301室", 1)]
        [InlineData("哈尔滨市香坊区睿城小区A5号楼3单元301室", 1)]
        [InlineData("哈尔滨市南岗区恒祥家园富华轩1单元20楼B", 2)]
        [InlineData("哈尔滨市道里区提拉米苏小区C栋3单元603室", 3)]
        [InlineData("哈尔滨道里区安静二胡同12-1号3单元301室", 1)]
        [InlineData("哈尔滨市道里区安广街副4号701室", 1)]
        [InlineData("哈尔滨市道里区安丰街106号401室", 1)]
        [InlineData("哈尔滨南岗区恒祥家园中华轩2单元16A", 1)]
        [InlineData("", null)]
        public void get_door_tests(string src, int? expected)
        {
            // Act
            var result = AddressAnalyser.GetDoor(AddressAnalyser.GetNumbers(src));

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("哈尔滨市道里区工部街18号2单元701室", 2)]
        [InlineData("哈尔滨市道里区恒祥城2期9栋1单元1001室", 1)]
        [InlineData("哈尔滨市道里区安心街114号3栋3单元301室", 3)]
        [InlineData("哈尔滨市香坊区睿城小区A5号楼3单元301室", 3)]
        [InlineData("哈尔滨市南岗区恒祥家园富华轩1单元20楼B", 1)]
        [InlineData("哈尔滨市道里区提拉米苏小区C栋3单元603室", 3)]
        [InlineData("哈尔滨道里区安静二胡同12-1号3单元301室", 3)]
        [InlineData("哈尔滨市道里区安广街副4号701室", 1)]
        [InlineData("哈尔滨市道里区安丰街106号401室", 1)]
        [InlineData("哈尔滨南岗区恒祥家园中华轩2单元16A", 2)]
        [InlineData("", null)]
        public void get_unit_tests(string src, int? expected)
        {
            // Act
            var result = AddressAnalyser.GetUnit(src);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("哈尔滨市香坊区睿城小区A5号楼3单元301室", "A5")]
        [InlineData("哈尔滨市道里区工部街18号2单元701室", null)]
        [InlineData("哈尔滨市道里区恒祥城2期9栋1单元1001室", "9")]
        [InlineData("哈尔滨市道里区安心街114号3栋3单元301室", "3")]
        [InlineData("哈尔滨市南岗区恒祥家园富华轩1单元20楼B", null)]
        [InlineData("哈尔滨市道里区提拉米苏小区C栋3单元603室", "C")]
        [InlineData("哈尔滨道里区安静二胡同12-1号3单元301室", null)]
        [InlineData("哈尔滨市道里区安广街副4号701室", null)]
        [InlineData("哈尔滨市道里区安丰街106号401室", null)]
        [InlineData("哈尔滨南岗区恒祥家园中华轩2单元16A", null)]
        [InlineData("哈尔滨市道里区恒祥凯悦B栋26层2602室", "B")]
        [InlineData("", null)]
        public void get_building_number_tests(string src, string expected)
        {
            // Act
            var result = AddressAnalyser.GetBuildingNumber(src);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("哈尔滨市香坊区睿城小区A5号楼3单元301室", "哈尔滨市")]
        [InlineData("哈尔滨市道里区工部街18号2单元701室", "哈尔滨市")]
        [InlineData("哈尔滨市道里区恒祥城2期9栋1单元1001室", "哈尔滨市")]
        [InlineData("哈尔滨市道里区安心街114号3栋3单元301室", "哈尔滨市")]
        [InlineData("哈尔滨市南岗区恒祥家园富华轩1单元20楼B", "哈尔滨市")]
        [InlineData("哈尔滨市道里区提拉米苏小区C栋3单元603室", "哈尔滨市")]
        [InlineData("哈尔滨道里区安静二胡同12-1号3单元301室", null)]
        [InlineData("哈尔滨市道里区安广街副4号701室", "哈尔滨市")]
        [InlineData("哈尔滨市道里区安丰街106号401室", "哈尔滨市")]
        [InlineData("哈尔滨南岗区恒祥家园中华轩2单元16A", null)]
        [InlineData("哈尔滨市道里区恒祥凯悦B栋26层2602室", "哈尔滨市")]
        [InlineData("", null)]
        public void get_city_tests(string src, string expected)
        {
            // Act
            var result = AddressAnalyser.GetCity(src);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("哈尔滨市香坊区睿城小区A5号楼3单元301室", "香坊区")]
        [InlineData("哈尔滨市道里区工部街18号2单元701室", "道里区")]
        [InlineData("哈尔滨市道里区恒祥城2期9栋1单元1001室", "道里区")]
        [InlineData("哈尔滨市道里区安心街114号3栋3单元301室", "道里区")]
        [InlineData("哈尔滨市南岗区恒祥家园富华轩1单元20楼B", "南岗区")]
        [InlineData("哈尔滨市道里区提拉米苏小区C栋3单元603室", "道里区")]
        [InlineData("哈尔滨道里区安静二胡同12-1号3单元301室", "哈尔滨道里区")]
        [InlineData("哈尔滨市道里区安广街副4号701室", "道里区")]
        [InlineData("哈尔滨市道里区安丰街106号401室", "道里区")]
        [InlineData("哈尔滨南岗区恒祥家园中华轩2单元16A", "哈尔滨南岗区")]
        [InlineData("哈尔滨市道里区恒祥凯悦B栋26层2602室", "道里区")]
        [InlineData("", null)]
        public void get_district_tests(string src, string expected)
        {
            // Act
            var result = AddressAnalyser.GetDistrict(src);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("哈尔滨市香坊区睿城小区A5号楼3单元301室", "睿城小区")]
        [InlineData("哈尔滨市道里区工部街18号2单元701室", "工部街18号")]
        [InlineData("哈尔滨市道里区恒祥城2期9栋1单元1001室", "恒祥城")]
        [InlineData("哈尔滨市道里区安心街114号3栋3单元301室", "安心街114号")]
        [InlineData("哈尔滨市南岗区恒祥家园富华轩1单元20楼B", "恒祥家园富华轩")]
        [InlineData("哈尔滨市道里区提拉米苏小区C栋3单元603室", "提拉米苏小区")]
        [InlineData("哈尔滨道里区安静二胡同12-1号3单元301室", "安静二胡同12-1号")]
        [InlineData("哈尔滨市道里区安广街副4号701室", "安广街副4号")]
        [InlineData("哈尔滨市道里区安丰街106号401室", "安丰街106号")]
        [InlineData("哈尔滨南岗区恒祥家园中华轩2单元16A", "恒祥家园中华轩")]
        [InlineData("哈尔滨市道里区恒祥凯悦B栋26层2602室", "恒祥凯悦")]
        [InlineData("", null)]
        public void get_estate_tests(string src, string expected)
        {
            // Act
            var result = AddressAnalyser.GetEstate(src);

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
