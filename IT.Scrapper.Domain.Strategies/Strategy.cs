using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace IT.Scrapper.Domain.Strategies
{
    [Serializable]
    public class Strategy
    {
        public int Order { get; set; }
        public StrategyTypeEnum Type { get; set; }
        public string Value { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum StrategyTypeEnum
    {
        [EnumMember(Value="none")]
        None,
        [EnumMember(Value="apply")]
        Apply,
        [EnumMember(Value="xpath")]
        Xpath,
        [EnumMember(Value="regex")]
        Regex,
        [EnumMember(Value="id")]
        Id,
        [EnumMember(Value="css")]
        Css
    }
}