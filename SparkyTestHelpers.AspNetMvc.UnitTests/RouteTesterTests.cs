﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SparkyTestHelpers.AspNetMvc.Routing;
using SparkyTestHelpers.AspNetMvc.UnitTests.Routing;
using System.Web.Mvc;

namespace SparkyTestHelpers.AspNetMvc.UnitTests
{
    /// <summary>
    /// <see cref="RouteTester"/> / <see cref="RoutingAsserter"/> unit tests, using "RegisterRoutes" constructor.
    /// </summary>
    [TestClass]
    public class RouteTesterTests : RouteTesterTestsBase
    {
        [TestInitialize]
        public void TestInitialize()
        {
            _routeTester = new RouteTester(RouteConfig.RegisterRoutes);
            //_routeTester = new RouteTester(new TestAreaRegistration());
        }

        //[TestMethod]
        //public void Test_AreaRegistration()
        //{
        //    var testAreaRegistration = new TestAreaRegistration();
        //    var routeTester = new RouteTester(testAreaRegistration);
        //}

        private class TestAreaRegistration : AreaRegistration
        {
            public override string AreaName => "TestArea";

            public override void RegisterArea(AreaRegistrationContext context)
            {
                context.Routes.Add("Legacy", new LegacyUrlRoute());

                context.MapRoute(
                    "TestArea_default",
                    "TestArea/{controller}/{action}/{id}",
                    new { action = "Index", id = UrlParameter.Optional }
                );
            }
        }
    }
}
