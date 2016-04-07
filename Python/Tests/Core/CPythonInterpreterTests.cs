// Python Tools for Visual Studio
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the License); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY
// IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System;
using System.Linq;
using Microsoft.PythonTools.Interpreter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using TestUtilities;
using TestUtilities.Python;

namespace PythonToolsTests {
    [TestClass]
    public class CPythonInterpreterTests {
        internal static readonly CPythonInterpreterFactoryProvider InterpFactory = new CPythonInterpreterFactoryProvider();

        [ClassInitialize]
        public static void DoDeployment(TestContext context) {
            AssertListener.Initialize();
            PythonTestData.Deploy();
        }
        
        [TestMethod, Priority(0)]
        public void FactoryProvider() {
            var provider = InterpFactory;
            var factories = provider.GetInterpreterFactories().ToArray();

            foreach (var factory in factories) {
                if (factory.Configuration.Id.StartsWith("Global|")) {
                    Assert.IsTrue(factory.Configuration.Description.StartsWith("Python"), "non 'Python' interpreter");
                    Assert.IsTrue(factory.Configuration.Version.Major == 2 || factory.Configuration.Version.Major == 3, "unknown 32-bit version");
                    Assert.IsNotNull(factory.CreateInterpreter(), "32-bit failed to create interpreter");
                    if (factory.Configuration.Architecture == System.Reflection.ProcessorArchitecture.Amd64) {
                        Assert.IsTrue(factory.Configuration.FullDescription.StartsWith("Python 64-bit"), "non 'Python 64-bit' interpreter");
                    }
                } else {
                    Assert.Fail("Expected Global interpreter ID");
                }
            }
        }

        [TestMethod, Priority(1)]
        public void DiscoverRegistryRace() {
            // https://github.com/Microsoft/PTVS/issues/558

            using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Python\PythonCore")) {
                for (int changes = 0; changes < 1000; ++changes) {
                    // Doesn't matter about the name - we just want to trigger
                    // discovery and then remove the key during GetSubKeyNames.
                    key.CreateSubKey("NotARealInterpreter").Close();
                    key.DeleteSubKey("NotARealInterpreter", false);
                }
            }
        }
    }
}
