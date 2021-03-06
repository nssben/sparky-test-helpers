﻿using System;
using System.Linq.Expressions;

namespace SparkyTestHelpers.Mapping
{
    /// <summary>
    /// This class is for testing that a property was successfully "mapped" from one type to another.
    /// </summary>
    /// <typeparam name="TSource">The"map from" type.</typeparam>
    /// <typeparam name="TDestination">The "map to" type.</typeparam>
    public class MapMemberTester<TSource, TDestination>
    {
        private MapTester<TSource, TDestination> _mapTester;

        /// <summary>
        /// Should the property be ignored (Don't test the mapping)?
        /// </summary>
        internal bool ShouldBeIgnored { get; set; }

        /// <summary>
        /// Function to get the actual "mapped to" value.
        /// </summary>
        internal Func<TDestination, object> GetActualValue { get; private set; }

        /// <summary>
        /// Function to get the expected "mapped to" value.
        /// </summary>
        internal Func<TSource, object> GetExpectedValue { get; private set; }

        /// <summary>
        /// Action that examines the source and destination instances and performs a
        /// custom test that a property was mapped correctly.
        /// </summary>
        internal Action<TSource, TDestination> CustomTest { get; set; }

        internal string TestDescription { get; set; }

        /// <summary>
        /// Creates a new <see cref="MapMemberTester{TSource, TDestination}"/> instance.
        /// </summary>
        /// <param name="mapTester">"Parent" <see cref="MapMemberTester{TSource, TDestination}"/>.</param>
        /// <param name="getActualValue">Function to get the actual "mapped to" value.</param>
        /// <param name="getExpectedValue">Function to get the expected "mapped to" value.</param>
        public MapMemberTester(
            MapTester<TSource, TDestination> mapTester,
            Func<TDestination, object> getActualValue = null,
            Func<TSource, object> getExpectedValue = null)
        {
            _mapTester = mapTester;
            GetActualValue = getActualValue;
            GetExpectedValue = getExpectedValue;
        }

        /// <summary>
        /// Specify <typeparamref name="TSource"/> property that should match the <typeparamref name="TDestination"/>
        /// property for which mapping is being tested.
        /// </summary>
        /// <param name="sourceExpression">Expression to get source property name.</param>
        /// <returns>"Parent" <see cref="MapTester{TSource, TDestination}"/>.</returns>
        /// <example>
        /// <code><![CDATA[
        ///     MapTester
        ///         .ForMap<Foo, Bar>()
        ///         .WhereMember(dest => dest.Status).ShouldEqual(src => src.StatusCode)
        ///         .AssertMappedValues(foo, bar);
        /// ]]>
        /// </code>
        /// </example>
        public MapTester<TSource, TDestination> ShouldEqual(Expression<Func<TSource, object>> sourceExpression)
        {
            string expressionString = sourceExpression.ToString();
            TestDescription = $"ShouldEqual({expressionString})";
            GetExpectedValue = sourceExpression.Compile();
            return _mapTester;
        }

        /// <summary>
        /// Specify expected value that should match the <typeparamref name="TDestination"/>
        /// property for which mapping is being tested.
        /// </summary>
        /// <param name="value">The expected value.</param>
        /// <returns>"Parent" <see cref="MapTester{TSource, TDestination}"/>.</returns>
        /// <example>
        /// <code><![CDATA[
        ///     MapTester
        ///         .ForMap<Foo, Bar>()
        ///         .WhereMember(dest => dest.IsValid).ShouldEqual(true)
        ///         .AssertMappedValues(foo, bar);
        /// ]]>
        /// </code>
        /// </example>
        public MapTester<TSource, TDestination> ShouldEqualValue(object value)
        {
            string stringValue = (value == null)
                ? "null" 
                : (value is string)
                    ? "\"" + value + "\""
                    : value.ToString();

            TestDescription = $"ShouldEqualValue({stringValue})";

            GetExpectedValue = _ => value;
            return _mapTester;
        }

        /// <summary>
        /// Specify custom test for property mapping.
        /// </summary>
        /// <param name="customTest">Action that examines the source and destination instances 
        /// and asserts property map success.</param>
        /// <returns>"Parent" <see cref="MapTester{TSource, TDestination}"/>.</returns>
        /// <example>
        /// <code><![CDATA[
        ///     MapTester
        ///         .ForMap<Foo, Bar>()
        ///         .WhereMember(dest => dest.IsValid)
        ///             .IsTestedBy((src, dest) => Assert.AreEqual(src.Num + 1, dest.Num))
        ///         .AssertMappedValues(foo, bar);
        /// ]]>
        /// </code>
        /// </example>
        public MapTester<TSource, TDestination> IsTestedBy(Action<TSource, TDestination> customTest)
        {
            TestDescription = "IsTestedBy((src, dest) => { /* custom test */ })";
            CustomTest = customTest;
            return _mapTester;
        }

        /// <summary>
        /// Specify custom test for property mapping.
        /// </summary>
        /// <param name="customTest">Action that examines the destination instances
        /// and asserts property map success.</param>
        /// <returns>"Parent" <see cref="MapTester{TSource, TDestination}"/>.</returns>
        /// <example>
        /// <code><![CDATA[
        ///     MapTester
        ///         .ForMap<Foo, Bar>()
        ///         .WhereMember(dest => dest.IsValid).IsTestedBy((dest) => Assert.IsTrue(dest.IsValid))
        ///         .AssertMappedValues(foo, bar);
        /// ]]>
        /// </code>
        /// </example>
        public MapTester<TSource, TDestination> IsTestedBy(Action<TDestination> customTest)
        {
            TestDescription = "IsTestedBy(dest => { /* custom test */ })";
            CustomTest = (_, dest) => customTest(dest);
            return _mapTester;
        }

        /// <summary>
        /// Specifies that destination property .ToString() should match source property .ToString().
        /// </summary>
        /// <param name="sourceExpression">Expression to get source property name.</param>
        /// <returns>"Parent" <see cref="MapTester{TSource, TDestination}"/>.</returns>
        public MapTester<TSource, TDestination> ShouldBeStringMatchFor(Expression<Func<TSource, object>> sourceExpression)
        {
            TestDescription = $"IsTestedBy({sourceExpression})";

            Func<TSource, object> getSourceObjectValue = sourceExpression.Compile();
            GetExpectedValue = src => getSourceObjectValue(src).ToString();

            Func<TDestination, object> getDestObjectValue = GetActualValue;

            GetActualValue = dest => getDestObjectValue(dest).ToString();

            return _mapTester;
        }
    }
}
