﻿namespace SparkyTestHelpers.Moq.Fluent.UnitTests
{
    public interface IMockable
    {
        string TestProperty { get; set; }

        void TestMethod();

        bool TestFunction(string parm1);
    }
}
