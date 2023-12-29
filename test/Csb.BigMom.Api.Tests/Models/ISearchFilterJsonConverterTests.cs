using Csb.BigMom.Api.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Csb.BigMom.Api.Tests.Models
{
    public class ISearchFilterJsonConverterTests
    {
        [Fact]
        public void Deserialize_Test()
        {
            // Setup
            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new ISearchFilterJsonConverter()
                },
                PropertyNameCaseInsensitive = true
            };
            var json = "{\"filters\":{\"array_filter_1\":[\"value1\",\"value2\"],\"array_filter_2\":[0,1,2],\"array_filter_3\":[true,false],\"range_filter_1\":{\"start\":\"2021-08-01T00:00\",\"end\":\"2021-08-31T23:59:59\"},\"range_filter_2\":{\"start\":0,\"end\":10},\"range_filter_3\":\"\",\"range_filter_4\":0}}";

            // Act
            var request = JsonSerializer.Deserialize<SearchRequest>(json, options);

            // Assert
            request.Filters.Should().BeEquivalentTo(new Dictionary<string, ISearchFilter>
            {
                { "array_filter_1", new ArraySearchFilter<string>{ Values = new[]{ "value1", "value2" } } },
                { "array_filter_2", new ArraySearchFilter<double>{ Values = new[]{ 0d, 1d, 2d } } },
                { "array_filter_3", null },
                { "range_filter_1", new RangeSearchFilter<DateTimeOffset> { Start = DateTimeOffset.Parse("2021-08-01T00:00"), End = DateTimeOffset.Parse("2021-08-31T23:59:59") } },
                { "range_filter_2", new RangeSearchFilter<double> { Start = 0, End = 10 } },
                { "range_filter_3", null },
                { "range_filter_4", null }
            });
        }
    }
}
