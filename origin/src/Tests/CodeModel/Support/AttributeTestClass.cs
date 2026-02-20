namespace Typewriter.Tests.CodeModel.Support
{
    public class AttributeTestClass
    {
        [AttributeInfo]
        public string NoParameters { get; set; }

        [AttributeInfo("parameter")]
        public string StringParameter { get; set; }

        [AttributeInfo(1)]
        public string IntParameter { get; set; }

        [AttributeInfo(2, Parameter = "parameter")]
        public string IntAndNamedParameter { get; set; }

        [AttributeInfo("parameter1", "parameter2")]
        public string ParamsParameter { get; set; }

        [AttributeInfo(1, "parameter")]
        public string IntAndParamsParameter { get; set; }

        [AttributeInfo(typeof(AttributeTestClass))]
        public string Type { get; set; }
    }
}