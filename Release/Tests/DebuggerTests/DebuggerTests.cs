﻿/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using EnvDTE90;
using Microsoft.PythonTools.Debugger;
using Microsoft.PythonTools.Parsing;
using Microsoft.TC.TestHostAdapters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using TestUtilities;

namespace DebuggerTests {
    [TestClass]
    public class DebuggerTests : BaseDebuggerTests {
        [TestMethod, Priority(0)]
        public void TestThreads() {
            // TODO: Thread creation tests w/ both thread.start_new_thread and threading module.
        }

        #region Enum Children Tests

        [TestMethod, Priority(0)]
        public void EnumChildrenTest() {
            const int lastLine = 40;

            if (Version.Version.Is3x()) {
                ChildTest(EnumChildrenTestName, lastLine, "s", AppendCount(new ChildInfo("[0]", "frozenset({2, 3, 4})")));
            } else {
                ChildTest(EnumChildrenTestName, lastLine, "s", AppendCount(new ChildInfo("[0]", "frozenset([2, 3, 4])")));
            }
            if (GetType() != typeof(DebuggerTestsIpy) && Version.Version.Is2x()) {
                // IronPython unicode repr differs
                // 3.x: http://pytools.codeplex.com/workitem/76
                ChildTest(EnumChildrenTestName, lastLine, "cinst", new ChildInfo("abc", "42", "0x2a"), new ChildInfo("uc", "u\'привет мир\'"));
            }
            ChildTest(EnumChildrenTestName, lastLine, "c2inst", new ChildInfo("abc", "42", "0x2a"), new ChildInfo("bar", "100", "0x64"), new ChildInfo("self", "myrepr", "myhex"));
            ChildTest(EnumChildrenTestName, lastLine, "c3inst", new ChildInfo("_contents", "[1, 2]"), new ChildInfo("abc", "42", "0x2a"), new ChildInfo("[0]", "1"), new ChildInfo("[1]", "2"));
            ChildTest(EnumChildrenTestName, lastLine, "l", AppendCountAndItem(new ChildInfo("[0]", "1"), new ChildInfo("[1]", "2")));
            ChildTest(EnumChildrenTestName, lastLine, "d1", AppendCountItemKeysAndValues(new ChildInfo("[42]", "100", "0x64")));
            ChildTest(EnumChildrenTestName, lastLine, "d2", AppendCountItemKeysAndValues(new ChildInfo("['abc']", "'foo'")));
            ChildTest(EnumChildrenTestName, lastLine, "i", null);
            ChildTest(EnumChildrenTestName, lastLine, "u1", null);
        }

        private ChildInfo[] AppendCount(ChildInfo existing) {
            if (this is DebuggerTestsIpy) {
                return new ChildInfo[] { new ChildInfo("Count"), existing };
            }
            return new[] { existing };
        }

        private ChildInfo[] AppendCountAndItem(params ChildInfo[] existing) {
            if (this is DebuggerTestsIpy) {
                List<ChildInfo> res = new List<ChildInfo>(existing);
                res.Add(new ChildInfo("Count"));
                res.Add(new ChildInfo("Item"));
                return res.ToArray();
            }
            return existing;
        }

        private ChildInfo[] AppendCountItemKeysAndValues(params ChildInfo[] existing) {
            if (this is DebuggerTestsIpy) {
                List<ChildInfo> res = new List<ChildInfo>(existing);
                res.Add(new ChildInfo("Count"));
                res.Add(new ChildInfo("Item"));
                res.Add(new ChildInfo("Keys"));
                res.Add(new ChildInfo("Values"));
                return res.ToArray();
            }
            return existing;
        }

        [TestMethod, Priority(0)]
        public void EnumChildrenTestPrevFrame() {
            const int breakLine = 2;

            if (Version.Version.Is3x()) {
                ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "s", 1, AppendCount(new ChildInfo("[0]", "frozenset({2, 3, 4})")));
            } else {
                ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "s", 1, AppendCount(new ChildInfo("[0]", "frozenset([2, 3, 4])")));
            }
            if (GetType() != typeof(DebuggerTestsIpy) && Version.Version.Is2x()) {
                // IronPython unicode repr differs
                // 3.x: http://pytools.codeplex.com/workitem/76
                ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "cinst", 1, new ChildInfo("abc", "42", "0x2a"), new ChildInfo("uc", "u\'привет мир\'"));
            }
            ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "c2inst", 1, new ChildInfo("abc", "42", "0x2a"), new ChildInfo("bar", "100", "0x64"), new ChildInfo("self", "myrepr", "myhex"));
            ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "l", 1, AppendCountAndItem(new ChildInfo("[0]", "1"), new ChildInfo("[1]", "2")));
            ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "d1", 1, AppendCountItemKeysAndValues(new ChildInfo("[42]", "100", "0x64")));
            ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "d2", 1, AppendCountItemKeysAndValues(new ChildInfo("['abc']", "'foo'")));
            ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "i", 1, null);
            ChildTest("PrevFrame" + EnumChildrenTestName, breakLine, "u1", 1, null);
        }

        [TestMethod, Priority(0)]
        public void GeneratorChildrenTest() {
            if (Version.Version <= PythonLanguageVersion.V25) {
                // gi_code new in 2.6
                ChildTest("GeneratorTest.py", 6, "a", 0,
                    new ChildInfo("gi_frame"),
                    new ChildInfo("gi_running")
                );
            } else {
                ChildTest("GeneratorTest.py", 6, "a", 0,
                    new ChildInfo("gi_code"),
                    new ChildInfo("gi_frame"),
                    new ChildInfo("gi_running")
                );
            }
        }

        public virtual string EnumChildrenTestName {
            get {
                return "EnumChildTest.py";
            }
        }

        private void ChildTest(string filename, int lineNo, string text, params ChildInfo[] children) {
            ChildTest(filename, lineNo, text, 0, children);
        }

        private void ChildTest(string filename, int lineNo, string text, int frame, params ChildInfo[] children) {
            var debugger = new PythonDebugger();
            PythonThread thread = null;
            var process = DebugProcess(debugger, DebuggerTestPath + filename, (newproc, newthread) => {
                var breakPoint = newproc.AddBreakPoint(filename, lineNo);
                breakPoint.Add();
                thread = newthread;
            });

            AutoResetEvent brkHit = new AutoResetEvent(false);
            process.BreakpointHit += (sender, args) => {
                brkHit.Set();
            };

            process.Start();

            AssertWaited(brkHit);

            var frames = thread.Frames;

            AutoResetEvent evalComplete = new AutoResetEvent(false);
            PythonEvaluationResult evalRes = null;
            frames[frame].ExecuteText(text, (completion) => {
                evalRes = completion;
                evalComplete.Set();
            });

            AssertWaited(evalComplete);
            Assert.IsTrue(evalRes != null, "didn't get evaluation result");


            if (children == null) {
                Assert.IsTrue(!evalRes.IsExpandable);
                Assert.IsTrue(evalRes.GetChildren(Int32.MaxValue) == null);
            } else {
                Assert.IsTrue(evalRes.IsExpandable);
                var childrenReceived = new List<PythonEvaluationResult>(evalRes.GetChildren(Int32.MaxValue));

                Assert.AreEqual(children.Length, childrenReceived.Count, String.Format("received incorrect number of children: {0} expected, received {1}", children.Length, childrenReceived.Count));
                for (int i = 0; i < children.Length; i++) {
                    var curChild = children[i];
                    bool foundChild = false;
                    for (int j = 0; j < childrenReceived.Count; j++) {
                        var curReceived = childrenReceived[j];
                        if (ChildrenMatch(curChild, curReceived)) {
                            foundChild = true;

                            if (children[i].ChildText.StartsWith("[")) {
                                Assert.AreEqual(childrenReceived[j].Expression, text + children[i].ChildText);
                            } else {
                                Assert.AreEqual(childrenReceived[j].Expression, text + "." + children[i].ChildText);
                            }

                            Assert.AreEqual(childrenReceived[j].Frame, frames[frame]);
                            childrenReceived.RemoveAt(j);
                            break;
                        }
                    }
                    Assert.IsTrue(foundChild, "failed to find " + children[i].ChildText + " found " + String.Join(", ", childrenReceived.Select(x => x.Expression)));
                }
                Assert.IsTrue(childrenReceived.Count == 0, "there's still some children left over which we didn't find");
            }

            process.Continue();

            process.WaitForExit();
        }

        private bool ChildrenMatch(ChildInfo curChild, PythonEvaluationResult curReceived) {
            return curReceived.ChildText == curChild.ChildText && 
                (curReceived.StringRepr == curChild.Repr || curChild.Repr == null) &&
                (Version.Version.Is3x() || (curChild.HexRepr == null || curChild.HexRepr == curReceived.HexRepr));// __hex__ no longer used in 3.x, http://mail.python.org/pipermail/python-list/2009-September/1218287.html
        }

        class ChildInfo {
            public readonly string ChildText;
            public readonly string Repr;
            public readonly string HexRepr;

            public ChildInfo(string key, string value = null, string hexRepr = null) {
                ChildText = key;
                Repr = value;
                HexRepr = hexRepr;
            }
        }

        #endregion

        #region Set Next Line Tests

        [TestMethod, Priority(0)]
        public void SetNextLineTest() {
            if (GetType() == typeof(DebuggerTestsIpy)) {
                //http://ironpython.codeplex.com/workitem/30129
                return;
            }

            var debugger = new PythonDebugger();
            PythonThread thread = null;
            var process = DebugProcess(debugger, DebuggerTestPath + @"SetNextLine.py", (newproc, newthread) => {
                var breakPoint = newproc.AddBreakPoint("SetNextLine.py", 1);
                breakPoint.Add();
                thread = newthread;
            });

            AutoResetEvent brkHit = new AutoResetEvent(false);
            AutoResetEvent stepDone = new AutoResetEvent(false);
            process.BreakpointHit += (sender, args) => {
                brkHit.Set();
            };
            process.StepComplete += (sender, args) => {
                stepDone.Set();
            };

            process.Start();

            AssertWaited(brkHit);

            var moduleFrame = thread.Frames[0];
            Assert.AreEqual(moduleFrame.StartLine, 1);
            if (GetType() != typeof(DebuggerTestsIpy)) {
                Assert.AreEqual(moduleFrame.EndLine, 13);
            }

            // skip over def f()
            Assert.IsTrue(moduleFrame.SetLineNumber(6));

            // set break point in g, run until we hit it.
            var newBp = process.AddBreakPoint("SetNextLine.py", 7);
            newBp.Add();

            process.Resume();
            AssertWaited(brkHit);

            thread.StepOver(); // step over x = 42
            AssertWaited(stepDone);

            // skip y = 100
            Assert.IsTrue(moduleFrame.SetLineNumber(9));

            thread.StepOver(); // step over z = 200
            AssertWaited(stepDone);

            // z shouldn't be defined
            var frames = thread.Frames;
            new HashSet<string>(new[] { "x", "z" }).ContainsExactly(frames[0].Locals.Select(x => x.Expression));

            // set break point in module, run until we hit it.
            newBp = process.AddBreakPoint("SetNextLine.py", 13);
            newBp.Add();
            thread.Resume();
            AssertWaited(brkHit);

            // f shouldn't be defined.
            frames = thread.Frames;
            new HashSet<string>(new[] { "sys", "g" }).ContainsExactly(frames[0].Locals.Select(x => x.Expression));

            process.Continue();

            process.WaitForExit();
        }

        #endregion

        #region BreakAll Tests

        
        [TestMethod, Priority(0)]
        public void TestBreakAll() {
            var debugger = new PythonDebugger();

            PythonThread thread = null;
            AutoResetEvent loaded = new AutoResetEvent(false);
            var process = DebugProcess(debugger, DebuggerTestPath + "BreakAllTest.py", (newproc, newthread) => {
                loaded.Set();
                thread = newthread;
            });

            process.Start();
            AssertWaited(loaded);

            // let loop run
            Thread.Sleep(500);
            AutoResetEvent breakComplete = new AutoResetEvent(false);
            PythonThread breakThread = null;
            process.AsyncBreakComplete += (sender, args) => {
                breakThread = args.Thread;
                breakComplete.Set();
            };

            process.Break();
            AssertWaited(breakComplete);

            Assert.AreEqual(breakThread, thread);

            process.Resume();

            process.Terminate();
        }

        [TestMethod, Priority(0)]
        public void TestBreakAllThreads() {
            var debugger = new PythonDebugger();

            PythonThread thread = null;
            AutoResetEvent loaded = new AutoResetEvent(false);
            var process = DebugProcess(debugger, DebuggerTestPath + "InfiniteThreads.py", (newproc, newthread) => {
                loaded.Set();
                thread = newthread;
            });

            process.Start();
            AssertWaited(loaded);

            AutoResetEvent breakComplete = new AutoResetEvent(false);
            process.AsyncBreakComplete += (sender, args) => {
                breakComplete.Set();
            };

            // let loop run
            for (int i = 0; i < 100; i++) {
                Thread.Sleep(50);

                Debug.WriteLine(String.Format("Breaking {0}", i));
                process.Break();
                if (!breakComplete.WaitOne(10000)) {
                    Console.WriteLine("Failed to break");
                }
                process.Resume();
                Debug.WriteLine(String.Format("Resumed {0}", i));
            }

            process.Terminate();
        }

        #endregion

        #region Eval Tests

        [TestMethod, Priority(0)]
        public void EvalTest() {
            EvalTest("LocalsTest4.py", 2, "g", 1, EvalResult.Value("baz", "int", "42"));
            EvalTest("LocalsTest3.py", 2, "f", 0, EvalResult.Value("x", "int", "42"));
            EvalTest("LocalsTest3.py", 2, "f", 0, EvalResult.Exception("not_defined", "name 'not_defined' is not defined"));
            EvalTest("LocalsTest3.py", 2, "f", 0, EvalResult.ErrorExpression("/2", "unexpected token '/'\r\ninvalid syntax\r\n"));
        }

        class EvalResult {
            private readonly string _typeName, _repr;
            public readonly string ExceptionText, Expression;
            public readonly bool IsError;

            public static EvalResult Exception(string expression, string exceptionText) {
                return new EvalResult(expression, exceptionText, false);
            }

            public static EvalResult Value(string expression, string typeName, string repr) {
                return new EvalResult(expression, typeName, repr);
            }

            public static EvalResult ErrorExpression(string expression, string error) {
                return new EvalResult(expression, error, true);
            }

            EvalResult(string expression, string exceptionText, bool isError) {
                Expression = expression;
                ExceptionText = exceptionText;
                IsError = isError;
            }

            EvalResult(string expression, string typeName, string repr) {
                Expression = expression;
                _typeName = typeName;
                _repr = repr;
            }

            public void Validate(PythonEvaluationResult result) {
                if (ExceptionText != null) {
                    Assert.AreEqual(result.ExceptionText, ExceptionText);
                } else {
                    Assert.AreEqual(result.TypeName, _typeName);
                    Assert.AreEqual(result.StringRepr, _repr);
                }
            }
        }

        private void EvalTest(string filename, int lineNo, string frameName, int frameIndex, EvalResult eval) {
            var debugger = new PythonDebugger();
            PythonThread thread = null;
            var process = DebugProcess(debugger, DebuggerTestPath + filename, (newproc, newthread) => {
                var breakPoint = newproc.AddBreakPoint(filename, lineNo);
                breakPoint.Add();
                thread = newthread;
            });

            AutoResetEvent brkHit = new AutoResetEvent(false);
            process.BreakpointHit += (sender, args) => {
                brkHit.Set();
            };

            process.Start();

            AssertWaited(brkHit);

            var frames = thread.Frames;

            PythonEvaluationResult obj = null;
            string errorMsg;
            if (eval.IsError) {
                Assert.IsTrue(!frames[frameIndex].TryParseText(eval.Expression, out errorMsg));
                Assert.AreEqual(errorMsg, eval.ExceptionText);
            } else {
                Assert.IsTrue(frames[frameIndex].TryParseText(eval.Expression, out errorMsg));
                Assert.AreEqual(errorMsg, null);

                AutoResetEvent textExecuted = new AutoResetEvent(false);
                Assert.AreEqual(frameName, frames[frameIndex].FunctionName);
                frames[frameIndex].ExecuteText(eval.Expression, (completion) => {
                    obj = completion;
                    textExecuted.Set();
                }
                );
                AssertWaited(textExecuted);
                eval.Validate(obj);
            }

            process.Continue();

            process.WaitForExit();
        }


        #endregion

        #region Local Tests

        /// <summary>
        /// Verify it takes more than just an items() method for us to treat something like a dictionary.
        /// </summary>
        [TestMethod, Priority(0)]
        public void CloseToDictExpansionBug484() {
            PythonThread thread = RunAndBreak("LocalsTestBug484.py", 7);
            
            var frames = thread.Frames;

            var obj = frames[0].Locals.First(x => x.Expression == "x");
            var children = obj.GetChildren(2000);
            int extraCount = 0;
            if (this is DebuggerTestsIpy) {
                extraCount += 2;
            }
            Assert.AreEqual(children.Length, extraCount + 3);
            Assert.AreEqual(children[0 + extraCount].StringRepr, "2");
            Assert.AreEqual(children[1 + extraCount].StringRepr, "3");
            Assert.AreEqual(children[2 + extraCount].StringRepr, "4");

            thread.Process.Continue();

            thread.Process.WaitForExit();
        }

        [TestMethod, Priority(0)]
        public void LocalsTest() {
            LocalsTest("LocalsTest.py", 3, new string[] { }, new string[] { "x" });

            LocalsTest("LocalsTest2.py", 2, new string[] { "x" }, new string[] { });

            LocalsTest("LocalsTest3.py", 2, new string[] { "x" }, new string[] { "y" });
        }

        [TestMethod, Priority(0)]
        public void GlobalsTest() {
            if (Version.Version >= PythonLanguageVersion.V32) {
                LocalsTest("GlobalsTest.py", 4, new string[] { }, new[] { "x", "y", "__file__", "__name__", "__package__", "__builtins__", "__doc__", "__cached__" });
            } else if (Version.Version >= PythonLanguageVersion.V26 && GetType() != typeof(DebuggerTestsIpy)) { // IronPython doesn't set __package__
                LocalsTest("GlobalsTest.py", 4, new string[] { }, new[] { "x", "y", "__file__", "__name__", "__package__", "__builtins__", "__doc__" });
            } else {
                LocalsTest("GlobalsTest.py", 4, new string[] { }, new[] { "x", "y", "__file__", "__name__", "__builtins__", "__doc__" });
            }
        }

        #endregion

        #region Stepping Tests

        [TestMethod, Priority(0)]
        public void StepTest() {
            // Bug 507: http://pytools.codeplex.com/workitem/507
            StepTest(DebuggerTestPath + @"SteppingTestBug507.py",
                    new ExpectedStep(StepKind.Over, 1),     // step over def add_two_numbers(x, y):
                    new ExpectedStep(StepKind.Over, 4),     // step over class Z(object):
                    new ExpectedStep(StepKind.Over, 9),     // step over p = Z()
                    new ExpectedStep(StepKind.Into, 10),     // step into print add_two_numbers(p.foo, 3)
                    new ExpectedStep(StepKind.Out, 7),     // step out return 7
                    new ExpectedStep(StepKind.Into, 10),     // step into add_two_numbers(p.foo, 3)
                    new ExpectedStep(StepKind.Resume, 2)     // wait for exit after return x + y
                );

            // Bug 508: http://pytools.codeplex.com/workitem/508
            StepTest(DebuggerTestPath + @"SteppingTestBug508.py",
                    new ExpectedStep(StepKind.Into, 1),     // step print (should step over)
                    new ExpectedStep(StepKind.Resume, 2)     // step print (should step over)
                );

            // Bug 509: http://pytools.codeplex.com/workitem/509
            StepTest(DebuggerTestPath + @"SteppingTestBug509.py",
                    new ExpectedStep(StepKind.Over, 1),     // step over def triangular_number
                    new ExpectedStep(StepKind.Into, 3),     // step into triangular_number
                    new ExpectedStep(StepKind.Into, 1),     // step over triangular_number
                    new ExpectedStep(StepKind.Into, 1),     // step into triangular_number
                    new ExpectedStep(StepKind.Into, 1),     // step into triangular_number
                    new ExpectedStep(StepKind.Resume, 1)    // let program exit
                );

            // Bug 503: http://pytools.codeplex.com/workitem/503
            StepTest(DebuggerTestPath + @"SteppingTestBug503.py",
                new []  { 6, 12 },
                new Action<PythonProcess>[] { 
                    (x) => {},
                    (x) => {},
                },
                new ExpectedStep(StepKind.Resume, 1),     // continue from def x1(y):
                new ExpectedStep(StepKind.Out, 6),     // step out after hitting breakpoint at return y
                new ExpectedStep(StepKind.Out, 3),     // step out z += 1
                new ExpectedStep(StepKind.Out, 3),     // step out z += 1
                new ExpectedStep(StepKind.Out, 3),     // step out z += 1
                new ExpectedStep(StepKind.Out, 3),     // step out z += 1
                new ExpectedStep(StepKind.Out, 3),     // step out z += 1

                new ExpectedStep(StepKind.Out, 14),     // step out after stepping out to x2(5)
                new ExpectedStep(StepKind.Out, 12),     // step out after hitting breakpoint at return y
                new ExpectedStep(StepKind.Out, 10),     // step out return z + 3
                new ExpectedStep(StepKind.Out, 10),     // step out return z + 3
                new ExpectedStep(StepKind.Out, 10),     // step out return z + 3
                new ExpectedStep(StepKind.Out, 10),     // step out return z + 3
                new ExpectedStep(StepKind.Out, 10),     // step out return z + 3
                
                new ExpectedStep(StepKind.Resume, 15)     // let the program exit
            );

            if (Version.Version < PythonLanguageVersion.V30) {  // step into print on 3.x runs more Python code
                StepTest(DebuggerTestPath + @"SteppingTest7.py",
                    new ExpectedStep(StepKind.Over, 1),     // step over def f():
                    new ExpectedStep(StepKind.Over, 6),     // step over def g():
                    new ExpectedStep(StepKind.Over, 10),     // step over def h()
                    new ExpectedStep(StepKind.Into, 13),     // step into f() call
                    new ExpectedStep(StepKind.Into, 2),     // step into print 'abc'
                    new ExpectedStep(StepKind.Into, 3),     // step into print 'def'
                    new ExpectedStep(StepKind.Into, 4),     // step into print 'baz'
                    new ExpectedStep(StepKind.Into, 13),     // step into g()
                    new ExpectedStep(StepKind.Into, 14),     // step into g()
                    new ExpectedStep(StepKind.Into, 7),     // step into dict assign
                    new ExpectedStep(StepKind.Into, 8),     // step into print 'hello'
                    new ExpectedStep(StepKind.Into, 14),     // step into h()
                    new ExpectedStep(StepKind.Into, 15),     // step into h()
                    new ExpectedStep(StepKind.Into, 11),    // step into h() return
                    new ExpectedStep(StepKind.Resume, 15)    // step into h() return
                );
            }

            StepTest(DebuggerTestPath + @"SteppingTest6.py",
                new ExpectedStep(StepKind.Over, 1),     // step over print 'hello world'
                new ExpectedStep(StepKind.Over, 2),     // step over a = set([i for i in range(256)])
                new ExpectedStep(StepKind.Over, 3),     // step over print a
                new ExpectedStep(StepKind.Resume, 4)    // let the program exit
            );

            StepTest(DebuggerTestPath + @"SteppingTest5.py",
                new ExpectedStep(StepKind.Over, 1),     // step over def g():...
                new ExpectedStep(StepKind.Over, 4),     // step over def f():...
                new ExpectedStep(StepKind.Into, 8),     // step into f()
                new ExpectedStep(StepKind.Out, 5),      // step out of f() on line "g()"
                new ExpectedStep(StepKind.Resume, 8)    // let the program exit
            );

            StepTest(DebuggerTestPath + @"SteppingTest4.py",
                new ExpectedStep(StepKind.Over, 1),     // step over def f():...
                new ExpectedStep(StepKind.Into, 5),     // step into f()
                new ExpectedStep(StepKind.Over, 2),     // step over for i in xrange(10):
                new ExpectedStep(StepKind.Over, 3),     // step over for print i
                new ExpectedStep(StepKind.Over, 2),     // step over for i in xrange(10):
                new ExpectedStep(StepKind.Over, 3),     // step over for print i
                new ExpectedStep(StepKind.Over, 2),     // step over for i in xrange(10):
                new ExpectedStep(StepKind.Over, 3),     // step over for print i
                new ExpectedStep(StepKind.Over, 2),     // step over for i in xrange(10):
                new ExpectedStep(StepKind.Resume, 5)    // let the program exit
            );

            StepTest(DebuggerTestPath + @"SteppingTest3.py",
                new ExpectedStep(StepKind.Over, 1),     // step over def f():...
                new ExpectedStep(StepKind.Over, 5),     // step over f()
                new ExpectedStep(StepKind.Resume, 6)    // let the program exit
            );

            StepTest(DebuggerTestPath + @"SteppingTest3.py",
                new ExpectedStep(StepKind.Over, 1),     // step over def f():...
                new ExpectedStep(StepKind.Into, 5),     // step into f()
                new ExpectedStep(StepKind.Out, 2),     // step out of f()
                new ExpectedStep(StepKind.Resume, 5)    // let the program exit
            );

            StepTest(DebuggerTestPath + @"SteppingTest2.py",
                new ExpectedStep(StepKind.Over, 1),     // step over def f():...
                new ExpectedStep(StepKind.Into, 4),     // step into f()
                new ExpectedStep(StepKind.Over, 2),     // step over print 'hi'
                new ExpectedStep(StepKind.Resume, 4)    // let the program exit
            );

            StepTest(DebuggerTestPath + @"SteppingTest2.py",
                new ExpectedStep(StepKind.Over, 1),     // step over def f():...
                new ExpectedStep(StepKind.Into, 4),     // step into f()
                new ExpectedStep(StepKind.Over, 2),     // step over print 'hi'
                new ExpectedStep(StepKind.Resume, 4)      // let the program exit
            );

            StepTest(DebuggerTestPath + @"SteppingTest.py",
                new ExpectedStep(StepKind.Over, 1),     // step over print "hello"
                new ExpectedStep(StepKind.Over, 2),     // step over print "goodbye"
                new ExpectedStep(StepKind.Resume, 3)   // let the program exit
            );

        }

        [TestMethod, Priority(0)]
        public void StepStdLib() {
            // http://pytools.codeplex.com/workitem/504 - test option for stepping into std lib.
            var debugger = new PythonDebugger();

            string fullPath = Path.GetFullPath(DebuggerTestPath + "StepStdLib.py");
            string dir = Path.GetDirectoryName(DebuggerTestPath + "StepStdLib.py");
            foreach (var steppingStdLib in new[] { false, true}) {
                var process = debugger.CreateProcess(
                    Version.Version,
                    Version.Path,
                    "\"" + fullPath + "\"",
                    dir,
                    "",
                    debugOptions: steppingStdLib ? PythonDebugOptions.DebugStdLib : PythonDebugOptions.None);

                PythonThread thread = null;
                process.ThreadCreated += (sender, args) => {
                    thread = args.Thread;
                };

                AutoResetEvent processEvent = new AutoResetEvent(false);

                bool processLoad = false, stepComplete = false;
                PythonBreakpoint bp = null;
                process.ProcessLoaded += (sender, args) => {
                    bp = process.AddBreakPoint(fullPath, 2);
                    bp.Add();

                    processLoad = true;
                    processEvent.Set();
                };

                process.StepComplete += (sender, args) => {
                    stepComplete = true;
                    processEvent.Set();
                };

                bool breakHit = false;
                process.BreakpointHit += (sender, args) => {
                    breakHit = true;
                    bp.Disable();
                    processEvent.Set();
                };

                process.Start();
                AssertWaited(processEvent);
                Assert.IsTrue(processLoad);
                Assert.IsFalse(stepComplete);
                process.Resume();

                AssertWaited(processEvent);
                Assert.IsTrue(breakHit);

                thread.StepInto();
                AssertWaited(processEvent);
                Assert.IsTrue(stepComplete);

                Debug.WriteLine(thread.Frames[thread.Frames.Count - 1].FileName);

                if (steppingStdLib) {
                    Assert.IsTrue(thread.Frames[0].FileName.EndsWith("\\os.py"));
                } else {
                    Assert.IsTrue(thread.Frames[0].FileName.EndsWith("\\StepStdLib.py"));
                }

                process.Resume();
            }
        }


        #endregion

        #region Breakpoint Tests

        /// <summary>
        /// Sets 2 breakpoints on one line after another, hits the 1st one, then steps onto
        /// the next one.  Makes sure we only break in once.
        /// </summary>
        [TestMethod, Priority(0)]
        public void BreakStepStep() {
            // http://pytools.codeplex.com/workitem/815

            string cwd = Path.Combine(Environment.CurrentDirectory, DebuggerTestPath);
            var debugger = new PythonDebugger();
            string fn = Path.Combine(cwd, "StepBreakBreak.py");
            var process = DebugProcess(debugger, fn, (newproc, newthread) => {
                PythonBreakpoint breakPoint = AddBreakPoint(newproc, 2, fn);
                breakPoint.Add();

                breakPoint = AddBreakPoint(newproc, 3, fn);
                breakPoint.Add();
            }, cwd: cwd);

            bool hitBp = false;
            process.BreakpointHit += (sender, args) => {
                Assert.IsTrue(!hitBp);
                hitBp = true;
                args.Thread.StepOver();
            };
            bool sentStep = false;
            process.StepComplete += (sender, args) => {
                if (sentStep) {
                    process.Continue();
                } else {
                    args.Thread.StepOver();
                    sentStep = true;
                }
            };
            process.Start();
            process.WaitForExit();
        }

        [TestMethod, Priority(0)]
        public void BreakpointNonMainFileRemoved() {
            // http://pytools.codeplex.com/workitem/638

            string cwd = Path.Combine(Environment.CurrentDirectory, DebuggerTestPath);
            BreakpointTest(
                Path.Combine(cwd, "BreakpointNonMainFileRemoved.py"),
                new[] { 2 },
                new[] { -2 },
                cwd: cwd,
                breakFilename: Path.Combine(cwd, "BreakpointNonMainFileRemovedImported.py"),
                checkBound: false,
                checkThread: false);
        }


        [TestMethod, Priority(0)]
        public void BreakpointNonMainThreadMainThreadExited() {
            // http://pytools.codeplex.com/workitem/638

            string cwd = Path.Combine(Environment.CurrentDirectory, DebuggerTestPath);
            BreakpointTest(
                Path.Combine(cwd, "BreakpointMainThreadExited.py"),
                new[] { 8 },
                new[] { 8, 8, 8, 8, 8 },
                cwd: cwd,
                breakFilename: Path.Combine(cwd, "BreakpointMainThreadExited.py"),
                checkBound: false,
                checkThread: false);
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointsFilenameColide() {
            // http://pytools.codeplex.com/workitem/565

            string cwd = Path.Combine(Environment.CurrentDirectory, DebuggerTestPath);
            BreakpointTest(
                Path.Combine(Environment.CurrentDirectory, DebuggerTestPath, "BreakpointFilenames.py"),
                new [] { 4 },
                new int [0],
                cwd: cwd,
                breakFilename: Path.Combine(Environment.CurrentDirectory, DebuggerTestPath, "B", "module1.py"),
                checkBound: false);
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointsSimpleFilename() {
            // http://pytools.codeplex.com/workitem/522

            string cwd = Path.GetFullPath(Path.Combine(Path.GetDirectoryName("SimpleFilenameBreakpoint.py"), ".."));
            BreakpointTest(
                Path.Combine(Environment.CurrentDirectory, DebuggerTestPath, "SimpleFilenameBreakpoint.py"), 
                new [] { 4, 10 }, 
                new[] { 4, 10 }, 
                cwd: cwd, 
                breakFilename: Path.Combine(Environment.CurrentDirectory, DebuggerTestPath, "CompiledCodeFile.py"),
                checkBound: false);
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointHitOtherThreadStackTrace() {
            // http://pytools.codeplex.com/workitem/483

            var debugger = new PythonDebugger();
            string filename = Path.Combine(Environment.CurrentDirectory, DebuggerTestPath, "ThreadJoin.py");
            PythonThread thread = null;
            var process = DebugProcess(debugger, filename, (newproc, newthread) => {
                    thread = newthread;
                    var bp = newproc.AddBreakPoint(filename, 5);
                    bp.Add();
                },
                debugOptions: PythonDebugOptions.WaitOnAbnormalExit | PythonDebugOptions.WaitOnNormalExit
            );

            AutoResetEvent bpHit = new AutoResetEvent(false);
            
            process.BreakpointHit += (sender, args) => {
                Assert.AreNotEqual(args.Thread, thread, "breakpoint shouldn't be on main thread");

                Assert.IsTrue(thread.Frames.Count > 1);
                foreach (var frame in thread.Frames) {
                    Console.WriteLine(frame.FileName);
                    Console.WriteLine(frame.LineNo);
                }
                process.Continue();
                bpHit.Set();
            };

            process.Start();

            if (!bpHit.WaitOne(10000)) {
                Assert.Fail("Failed to hit breakpoint");
            }

            process.Terminate();
        }

        [TestMethod, Priority(0)]
        public void TestBreakpoints() {
            BreakpointTest("BreakpointTest.py", new[] { 1 }, new[] { 1 });
        }

        [TestMethod, Priority(0)]
        public void TestBreakpoints2() {
            BreakpointTest("BreakpointTest2.py", new[] { 3 }, new[] { 3, 3, 3 });
        }

        [TestMethod, Priority(0)]
        public void TestBreakpoints3() {
            BreakpointTest("BreakpointTest3.py", new[] { 1 }, new[] { 1 });
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointsConditional() {
            BreakpointTest("BreakpointTest2.py", new[] { 3 }, new[] { 3 }, new[] { "i == 1" });
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointsConditionalOnChange() {
            BreakpointTest("BreakpointTest5.py", new[] { 4 }, new[] { 4, 4, 4, 4, 4 }, new[] { "j" }, new[] { true });
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointRemove() {
            BreakpointTest("BreakpointTest2.py", new[] { 3 }, new[] { -3 });
        }

        [TestMethod, Priority(0)]
        public void TestBreakpointFailed() {
            var debugger = new PythonDebugger();

            PythonThread thread = null;
            PythonBreakpoint breakPoint = null;
            var process = DebugProcess(debugger, DebuggerTestPath + "BreakpointTest.py", (newproc, newthread) => {
                breakPoint = newproc.AddBreakPoint("doesnotexist.py", 1);
                breakPoint.Add();
                thread = newthread;
            });

            bool bindFailed = false;
            process.BreakpointBindFailed += (sender, args) => {
                bindFailed = true;
                Assert.AreEqual(args.Breakpoint, breakPoint);
            };

            process.Start();

            process.WaitForExit();

            Assert.AreEqual(bindFailed, true);
        }

        #endregion

        #region Exception Tests

        class ExceptionInfo {
            public readonly string TypeName;
            public readonly int LineNumber;

            public ExceptionInfo(string typeName, int lineNumber) {
                TypeName = typeName;
                LineNumber = lineNumber;
            }
        }

        class ExceptionHandlerInfo {
            public readonly int FirstLine;
            public readonly int LastLine;
            public readonly HashSet<string> Expressions;

            public ExceptionHandlerInfo(int firstLine, int lastLine, params string[] expressions) {
                FirstLine = firstLine;
                LastLine = lastLine;
                Expressions = new HashSet<string>(expressions);
            }
        }

        public string ExceptionModule {
            get {
                if (Version.Version.Is3x()) {
                    return "builtins";
                }
                return "exceptions";
            }
        }

        public string PickleModule {
            get {
                if (Version.Version.Is3x()) {
                    return "_pickle";
                }
                return "cPickle";
            }
        }

        public virtual string ComplexExceptions {
            get {
                return "ComplexExceptions.py";
            }
        }

        [TestMethod, Priority(0)]
        public void TestExceptions() {
            var debugger = new PythonDebugger();
            for (int i = 0; i < 2; i++) {
                TestException(debugger, DebuggerTestPath + @"SimpleException.py", i == 0, 1, new KeyValuePair<string, int>[0],
                    new ExceptionInfo(ExceptionModule + ".Exception", 3));

                TestException(debugger, DebuggerTestPath + ComplexExceptions, i == 0, 1, new KeyValuePair<string, int>[0],
                    new ExceptionInfo(PickleModule + ".PickleError", 6),
                    new ExceptionInfo(ExceptionModule + ".StopIteration", 13),
                    new ExceptionInfo(ExceptionModule + ".NameError", 15),
                    new ExceptionInfo(ExceptionModule + ".StopIteration", 21),
                    new ExceptionInfo(ExceptionModule + ".NameError", 23),
                    new ExceptionInfo(ExceptionModule + ".Exception", 29),
                    new ExceptionInfo(ExceptionModule + ".Exception", 32)
                );

                TestException(debugger, DebuggerTestPath + ComplexExceptions, i == 0, 32, new KeyValuePair<string, int>[] {
                    new KeyValuePair<string, int>(PickleModule + ".PickleError", 0)
                });
                TestException(debugger, DebuggerTestPath + ComplexExceptions, i == 0, 0, new KeyValuePair<string, int>[] {
                    new KeyValuePair<string, int>(PickleModule + ".PickleError", 1),
                    new KeyValuePair<string, int>(ExceptionModule + ".StopIteration", 32),
                    new KeyValuePair<string, int>(ExceptionModule + ".NameError", 0),
                    new KeyValuePair<string, int>(ExceptionModule + ".Exception", 33),
                },
                    new ExceptionInfo(PickleModule + ".PickleError", 6),
                    new ExceptionInfo(ExceptionModule + ".Exception", 29),
                    new ExceptionInfo(ExceptionModule + ".Exception", 32)
                );

                if (Version.Version.Is2x()) {
                    TestException(debugger, DebuggerTestPath + @"UnicodeException.py", i == 0, 1, new KeyValuePair<string, int>[0],
                        new ExceptionInfo(ExceptionModule + ".Exception", 3));
                }

                // Only the last exception in each file should be noticed.
                if (Version.Version <= PythonLanguageVersion.V25) {
                    TestException(debugger, DebuggerTestPath + @"UnhandledException1_v25.py", i == 0, 32, new KeyValuePair<string, int>[0],
                        new ExceptionInfo(ExceptionModule + ".Exception", 57)
                    );
                } else if(Version.Version.Is3x()) {
                    TestException(debugger, DebuggerTestPath + @"UnhandledException1_v3x.py", i == 0, 32, new KeyValuePair<string, int>[0],
                        new ExceptionInfo(ExceptionModule + ".Exception", 56)
                    );
                } else {
                    TestException(debugger, DebuggerTestPath + @"UnhandledException1.py", i == 0, 32, new KeyValuePair<string, int>[0],
                        new ExceptionInfo(ExceptionModule + ".Exception", 81)
                    );
                }

                TestException(debugger, DebuggerTestPath + @"UnhandledException2.py", i == 0, 32, new KeyValuePair<string, int>[0],
                    new ExceptionInfo(ExceptionModule + ".Exception", 16)
                );
                TestException(debugger, DebuggerTestPath + @"UnhandledException3.py", i == 0, 32, new KeyValuePair<string, int>[0],
                    new ExceptionInfo(ExceptionModule + ".ValueError", 12)
                );
                TestException(debugger, DebuggerTestPath + @"UnhandledException4.py", i == 0, 32, new KeyValuePair<string, int>[0],
                    new ExceptionInfo(ExceptionModule + ".OSError", 17)
                );
                TestException(debugger, DebuggerTestPath + @"UnhandledException5.py", i == 0, 32, new KeyValuePair<string, int>[0],
                    new ExceptionInfo(ExceptionModule + ".ValueError", 4)
                );
                TestException(debugger, DebuggerTestPath + @"UnhandledException6.py", i == 0, 32, new KeyValuePair<string, int>[0],
                    new ExceptionInfo(ExceptionModule + ".OSError", 12)
                );
            }
        }

        private void TestException(PythonDebugger debugger, string filename, bool resumeProcess,
            int defaultExceptionMode, ICollection<KeyValuePair<string, int>> exceptionModes, params ExceptionInfo[] exceptions) {
                TestException(debugger, filename, resumeProcess, defaultExceptionMode, exceptionModes, PythonDebugOptions.None, exceptions);
        }

        private void TestException(PythonDebugger debugger, string filename, bool resumeProcess, 
            int defaultExceptionMode, ICollection<KeyValuePair<string, int>> exceptionModes, PythonDebugOptions debugOptions, params ExceptionInfo[] exceptions) {
            bool loaded = false;
            var process = DebugProcess(debugger, filename, (processObj, threadObj) => {
                loaded = true;
                processObj.SetExceptionInfo(defaultExceptionMode, exceptionModes);
            }, debugOptions: debugOptions);

            int curException = 0;
            process.ExceptionRaised += (sender, args) => {
                // V30 raises an exception as the process shuts down.
                if (loaded && ((Version.Version == PythonLanguageVersion.V30 && curException < exceptions.Length) || Version.Version != PythonLanguageVersion.V30)) {
                    if (GetType() != typeof(DebuggerTestsIpy) || curException < exceptions.Length) {    // Ipy over reports
                        Assert.AreEqual(args.Exception.TypeName, exceptions[curException].TypeName);
                    }

                    if (GetType() != typeof(DebuggerTestsIpy) || curException < exceptions.Length) {    // Ipy over reports
                        curException++;
                    }
                    if (resumeProcess) {
                        process.Resume();
                    } else {
                        args.Thread.Resume();
                    }
                } else {
                    args.Thread.Resume();
                }
            };

            process.Start();
            process.WaitForExit();

            Assert.AreEqual(exceptions.Length, curException);
        }

        /// <summary>
        /// Test cases for http://pytools.codeplex.com/workitem/367
        /// </summary>
        [TestMethod, Priority(0)]
        public void TestExceptionsSysExitZero() {
            var debugger = new PythonDebugger();

            TestException(debugger, 
                DebuggerTestPath + @"SysExitZeroRaise.py", 
                true, 32, 
                new KeyValuePair<string, int>[0],
                PythonDebugOptions.BreakOnSystemExitZero,
                new ExceptionInfo(ExceptionModule + ".SystemExit", 1)
            );

            TestException(debugger,
                DebuggerTestPath + @"SysExitZero.py",
                true, 32,
                new KeyValuePair<string, int>[0],
                PythonDebugOptions.BreakOnSystemExitZero,
                new ExceptionInfo(ExceptionModule + ".SystemExit", 2)
            );

            TestException(debugger,
                DebuggerTestPath + @"SysExitZeroRaise.py",
                true, 32,
                new KeyValuePair<string, int>[0],
                PythonDebugOptions.None
            );

            TestException(debugger,
                DebuggerTestPath + @"SysExitZero.py",
                true, 32,
                new KeyValuePair<string, int>[0],
                PythonDebugOptions.None
            );
        }

        [TestMethod, Priority(0)]
        public void TestExceptionHandlers() {
            var debugger = new PythonDebugger();

            TestGetHandledExceptionRanges(debugger, DebuggerTestPath + @"ExceptionHandlers.py",
                new ExceptionHandlerInfo(1, 3, "*"),
                new ExceptionHandlerInfo(6, 7, "*"),
                new ExceptionHandlerInfo(9, 13, "*"),
                
                new ExceptionHandlerInfo(18, 19, "ArithmeticError", "AssertionError", "AttributeError", "BaseException", "BufferError", "BytesWarning", "DeprecationWarning", "EOFError", "EnvironmentError", "Exception", "FloatingPointError", "FutureWarning", "GeneratorExit", "IOError", "ImportError", "ImportWarning", "IndentationError", "IndexError", "KeyError", "KeyboardInterrupt", "LookupError", "MemoryError", "NameError", "NotImplementedError", "OSError", "OverflowError", "PendingDeprecationWarning", "ReferenceError", "RuntimeError", "RuntimeWarning", "StandardError", "StopIteration", "SyntaxError", "SyntaxWarning", "SystemError", "SystemExit", "TabError", "TypeError", "UnboundLocalError", "UnicodeDecodeError", "UnicodeEncodeError", "UnicodeError", "UnicodeTranslateError", "UnicodeWarning", "UserWarning", "ValueError", "Warning", "WindowsError", "ZeroDivisionError"),
                new ExceptionHandlerInfo(69, 70, "ArithmeticError", "AssertionError", "AttributeError", "BaseException", "BufferError", "BytesWarning", "DeprecationWarning", "EOFError", "EnvironmentError", "Exception", "FloatingPointError", "FutureWarning", "GeneratorExit", "IOError", "ImportError", "ImportWarning", "IndentationError", "IndexError", "KeyError", "KeyboardInterrupt", "LookupError", "MemoryError", "NameError", "NotImplementedError", "OSError", "OverflowError", "PendingDeprecationWarning", "ReferenceError", "RuntimeError", "RuntimeWarning", "StandardError", "StopIteration", "SyntaxError", "SyntaxWarning", "SystemError", "SystemExit", "TabError", "TypeError", "UnboundLocalError", "UnicodeDecodeError", "UnicodeEncodeError", "UnicodeError", "UnicodeTranslateError", "UnicodeWarning", "UserWarning", "ValueError", "Warning", "WindowsError", "ZeroDivisionError"),
                new ExceptionHandlerInfo(72, 73, "*"),
                
                new ExceptionHandlerInfo(125, 126, "struct.error", "socket.error", "os.error"),
                new ExceptionHandlerInfo(130, 131, "struct.error", "socket.error", "os.error"),
                
                new ExceptionHandlerInfo(133, 143, "ValueError"),
                new ExceptionHandlerInfo(135, 141, "TypeError"),
                new ExceptionHandlerInfo(137, 139, "ValueError"),

                new ExceptionHandlerInfo(146, 148, "ValueError"),
                new ExceptionHandlerInfo(150, 156, "TypeError"),
                new ExceptionHandlerInfo(152, 154, "ValueError"),

                new ExceptionHandlerInfo(159, 160, "Exception"),
                new ExceptionHandlerInfo(162, 163, "Exception"),
                new ExceptionHandlerInfo(165, 166, "ValueError", "TypeError"),
                new ExceptionHandlerInfo(168, 169, "ValueError", "TypeError"),

                new ExceptionHandlerInfo(171, 172, "is_included", "also.included", "this.one.too.despite.having.lots.of.dots")
            );
        }

        private void TestGetHandledExceptionRanges(PythonDebugger debugger, string filename, params ExceptionHandlerInfo[] expected) {
            var process = DebugProcess(debugger, filename, (processObj, threadObj) => { });

            var actual = process.GetHandledExceptionRanges(filename);
            Assert.AreEqual(expected.Length, actual.Count);

            Assert.IsTrue(actual.All(a => 
                expected.SingleOrDefault(e => e.FirstLine == a.Item1 && e.LastLine == a.Item2 && e.Expressions.ContainsExactly(a.Item3)) != null
            ));
        }

        #endregion

        #region Module Load Tests

        [TestMethod, Priority(0)]
        public void TestModuleLoad() {
            var debugger = new PythonDebugger();

            // main file is reported
            TestModuleLoad(debugger, TestData.GetPath(@"TestData\HelloWorld\Program.py"), "Program.py");

            // imports are reported
            TestModuleLoad(debugger, DebuggerTestPath + @"imports_other.py", "imports_other.py", "is_imported.py");
        }

        private void TestModuleLoad(PythonDebugger debugger, string filename, params string[] expectedModulesLoaded) {
            var process = DebugProcess(debugger, filename);

            List<string> receivedFilenames = new List<string>();
            process.ModuleLoaded += (sender, args) => {
                receivedFilenames.Add(args.Module.Filename);
            };

            process.Start();
            process.WaitForExit();

            Assert.IsTrue(receivedFilenames.Count >= expectedModulesLoaded.Length);
            var set = new HashSet<string>();
            foreach (var received in receivedFilenames) {
                set.Add(Path.GetFileName(received));
            }

            foreach (var file in expectedModulesLoaded) {
                Assert.IsTrue(set.Contains(file));
            }
        }

        #endregion

        #region Exit Code Tests

        [TestMethod, Priority(0)]
        public void TestStartup() {
            var debugger = new PythonDebugger();

            // hello world
            TestExitCode(debugger, TestData.GetPath(@"TestData\HelloWorld\Program.py"), 0);

            // test which calls sys.exit(23)
            TestExitCode(debugger, DebuggerTestPath + @"SysExit.py", 23);

            // test which calls raise Exception()
            TestExitCode(debugger, DebuggerTestPath + @"ExceptionalExit.py", 1);
        }

        [TestMethod, Priority(0)]
        public void TestWindowsStartup() {
            var debugger = new PythonDebugger();

            string pythonwExe = Path.Combine(Path.GetDirectoryName(Version.Path), "pythonw.exe");
            if (!File.Exists(pythonwExe)) {
                pythonwExe = Path.Combine(Path.GetDirectoryName(Version.Path), "ipyw.exe");
            }

            if (File.Exists(pythonwExe)) {
                // hello world
                TestExitCode(debugger, TestData.GetPath(@"TestData\HelloWorld\Program.py"), 0, pythonExe: pythonwExe);

                // test which calls sys.exit(23)
                TestExitCode(debugger, DebuggerTestPath + @"SysExit.py", 23, pythonExe: pythonwExe);

                // test which calls raise Exception()
                TestExitCode(debugger, DebuggerTestPath + @"ExceptionalExit.py", 1, pythonExe: pythonwExe);
            }
        }

        private void TestExitCode(PythonDebugger debugger, string filename, int expectedExitCode, string interpreterOptions = null, string pythonExe = null) {
            var process = DebugProcess(debugger, filename, interpreterOptions: interpreterOptions, pythonExe: pythonExe);

            bool created = false, exited = false;
            process.ThreadCreated += (sender, args) => {
                created = true;
            };
            process.ThreadExited += (sender, args) => {
                exited = true;
            };
            process.ProcessExited += (sender, args) => {
                Assert.AreEqual(args.ExitCode, expectedExitCode);
            };
            process.ExceptionRaised += (sender, args) => {
                process.Resume();
            };

            process.Start();
            process.WaitForExit();

            Assert.IsTrue(created, "Never got notification of thread creation");
            Assert.IsTrue(exited, "Process failed to exit");
        }

        private new PythonProcess DebugProcess(PythonDebugger debugger, string filename, Action<PythonProcess, PythonThread> onLoaded = null, string interpreterOptions = null, PythonDebugOptions debugOptions = PythonDebugOptions.None, string cwd = null, string pythonExe = null) {
            string fullPath = Path.GetFullPath(filename);
            string dir = cwd ?? Path.GetFullPath(Path.GetDirectoryName(filename));
            var process = debugger.CreateProcess(Version.Version, pythonExe ?? Version.Path, "\"" + fullPath + "\"", dir, "", interpreterOptions, debugOptions);
            process.ProcessLoaded += (sender, args) => {
                if (onLoaded != null) {
                    onLoaded(process, args.Thread);
                }
                process.Resume();
            };

            return process;
        }

        #endregion

        #region Argument Tests

        [TestMethod, Priority(0)]
        public void TestInterpreterArguments() {
            var debugger = new PythonDebugger();

            // test which verifies we have no doc string when running w/ -OO
            TestExitCode(debugger, DebuggerTestPath + @"DocString.py", 0, interpreterOptions:"-OO");
        }

        #endregion

        #region Attach Tests

        /// <summary>
        /// threading module imports thread.start_new_thread, verifies that we patch threading's method
        /// in addition to patching the thread method so that breakpoints on threads created after
        /// attach via the threading module can be hit.
        /// </summary>
        [TestMethod, Priority(0)]
        public void AttachThreadingStartNewThread() {
            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython doesn't support attach
                // http://pytools.codeplex.com/workitem/638
                // http://pytools.codeplex.com/discussions/285741#post724014
                var psi = new ProcessStartInfo(Version.Path, "\"" + TestData.GetPath(@"TestData\DebuggerProject\ThreadingStartNewThread.py") + "\"");
                psi.WorkingDirectory = TestData.GetPath(@"TestData\DebuggerProject");
                Process p = Process.Start(psi);
                System.Threading.Thread.Sleep(1000);

                AutoResetEvent attached = new AutoResetEvent(false);
                AutoResetEvent breakpointHit = new AutoResetEvent(false);

                PythonProcess proc;
                ConnErrorMessages errReason;
                if ((errReason = PythonProcess.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
                    Assert.Fail("Failed to attach {0}", errReason);
                }

                proc.ProcessLoaded += (sender, args) => {
                    attached.Set();
                    var bp = proc.AddBreakPoint("ThreadingStartNewThread.py", 9);
                    bp.Add();

                    bp = proc.AddBreakPoint("ThreadingStartNewThread.py", 5);
                    bp.Add();

                    proc.Resume();
                };
                PythonThread mainThread = null;
                PythonThread bpThread = null;
                bool wrongLine = false;
                proc.BreakpointHit += (sender, args) => {
                    if (args.Breakpoint.LineNo == 9) {
                        // stop running the infinite loop
                        Debug.WriteLine(String.Format("First BP hit {0}", args.Thread.Id));
                        args.Thread.Frames[0].ExecuteText("x = False", (x) => {});
                        mainThread = args.Thread;
                    } else if (args.Breakpoint.LineNo == 5) {
                        // we hit the breakpoint on the new thread
                        Debug.WriteLine(String.Format("Second BP hit {0}", args.Thread.Id));
                        breakpointHit.Set();
                        bpThread = args.Thread;
                    } else {
                        Debug.WriteLine(String.Format("Hit breakpoint on wrong line number: {0}", args.Breakpoint.LineNo));
                        wrongLine = true;
                        attached.Set();
                        breakpointHit.Set();
                    }
                    proc.Continue();
                };
                proc.StartListening();

                Assert.IsTrue(attached.WaitOne(10000));
                Assert.IsTrue(breakpointHit.WaitOne(10000));
                Assert.IsFalse(wrongLine);

                Assert.AreNotEqual(mainThread, bpThread);
                proc.Detach();

                p.Kill();
            }
        }


        [TestMethod, Priority(0)]
        public void AttachReattach() {
            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython doesn't support attach
                Process p = Process.Start(Version.Path, "\"" + TestData.GetPath(@"TestData\DebuggerProject\InfiniteRun.py") + "\"");
                System.Threading.Thread.Sleep(1000);

                AutoResetEvent attached = new AutoResetEvent(false);
                AutoResetEvent detached = new AutoResetEvent(false);
                for (int i = 0; i < 10; i++) {
                    Console.WriteLine(i);

                    PythonProcess proc;
                    ConnErrorMessages errReason;
                    if ((errReason = PythonProcess.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
                        Assert.Fail("Failed to attach {0}", errReason);
                    }

                    proc.ProcessLoaded += (sender, args) => {
                        attached.Set();
                    };
                    proc.ProcessExited += (sender, args) => {
                        detached.Set();
                    };
                    proc.StartListening();

                    Assert.IsTrue(attached.WaitOne(10000));
                    proc.Detach();
                    Assert.IsTrue(detached.WaitOne(10000));
                }

                p.Kill();
            }
        }

        /// <summary>
        /// When we do the attach one thread is blocked in native code.  We attach, resume execution, and that
        /// thread should eventually wake up.  
        /// 
        /// The bug was two issues, when doing a resume all:
        ///		1) we don't clear the stepping if it's STEPPING_ATTACH_BREAK
        ///		2) We don't clear the stepping if we haven't yet blocked the thread
        ///		
        /// Because the thread is blocked in native code, and we don't clear the stepping, when the user
        /// hits resume the thread will eventually return back to Python code, and then we'll block it
        /// because we haven't cleared the stepping bit.
        /// </summary>
        [TestMethod, Priority(0)]
        public void AttachMultithreadedSleeper() {
            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython doesn't support attach
                // http://pytools.codeplex.com/discussions/285741 1/12/2012 6:20 PM
                Process p = Process.Start(Version.Path, "\"" + TestData.GetPath(@"TestData\DebuggerProject\AttachMultithreadedSleeper.py") + "\"");
                System.Threading.Thread.Sleep(1000);

                AutoResetEvent attached = new AutoResetEvent(false);

                PythonProcess proc;
                ConnErrorMessages errReason;
                if ((errReason = PythonProcess.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
                    Assert.Fail("Failed to attach {0}", errReason);
                }

                proc.ProcessLoaded += (sender, args) => {
                    attached.Set();
                };
                proc.StartListening();

                Assert.IsTrue(attached.WaitOne(10000));
                proc.Resume();
                Debug.WriteLine("Waiting for exit");
                Assert.IsTrue(proc.WaitForExit(20000));
            }
        }

        /*
        [TestMethod, Priority(0)]
        public void AttachReattach64() {
            Process p = Process.Start("C:\\Python27_x64\\python.exe", "\"" + TestData.GetPath(@"TestData\DebuggerProject\InfiniteRun.py") + "\"");
            System.Threading.Thread.Sleep(1000);

            for (int i = 0; i < 10; i++) {
                Console.WriteLine(i);

                PythonProcess proc;
                ConnErrorMessages errReason;
                if ((errReason = PythonProcess.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
                    Assert.Fail("Failed to attach {0}", errReason);
                }

                proc.Detach();
            }

            p.Kill();
        }*/

        [TestMethod, Priority(0)]
        public void AttachReattachThreadingInited() {
            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython shouldn't support attach
                Process p = Process.Start(Version.Path, "\"" + TestData.GetPath(@"TestData\DebuggerProject\InfiniteRunThreadingInited.py") + "\"");
                System.Threading.Thread.Sleep(1000);

                AutoResetEvent attached = new AutoResetEvent(false);
                AutoResetEvent detached = new AutoResetEvent(false);
                for (int i = 0; i < 10; i++) {
                    Console.WriteLine(i);

                    PythonProcess proc;
                    ConnErrorMessages errReason;
                    if ((errReason = PythonProcess.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
                        Assert.Fail("Failed to attach {0}", errReason);
                    }

                    proc.ProcessLoaded += (sender, args) => {
                        attached.Set();
                    };
                    proc.ProcessExited += (sender, args) => {
                        detached.Set();
                    };
                    proc.StartListening();

                    Assert.IsTrue(attached.WaitOne(10000));
                    proc.Detach();
                    Assert.IsTrue(detached.WaitOne(10000));
                }

                p.Kill();
            }
        }

        [TestMethod, Priority(0)]
        public void AttachReattachInfiniteThreads() {
            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython shouldn't support attach
                Process p = Process.Start(Version.Path, "\"" + TestData.GetPath(@"TestData\DebuggerProject\InfiniteThreads.py") + "\"");
                System.Threading.Thread.Sleep(1000);

                AutoResetEvent attached = new AutoResetEvent(false);
                AutoResetEvent detached = new AutoResetEvent(false);
                for (int i = 0; i < 10; i++) {
                    Console.WriteLine(i);

                    PythonProcess proc;
                    ConnErrorMessages errReason;
                    if ((errReason = PythonProcess.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
                        Assert.Fail("Failed to attach {0}", errReason);
                    }

                    proc.ProcessLoaded += (sender, args) => {
                        attached.Set();
                    };
                    proc.ProcessExited += (sender, args) => {
                        detached.Set();
                    };
                    proc.StartListening();

                    Assert.IsTrue(attached.WaitOne(20000));
                    proc.Detach();
                    Assert.IsTrue(detached.WaitOne(20000));

                }

                p.Kill();
            }
        }

        [TestMethod, Priority(0)]
        public void AttachTimeout() {
            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython doesn't support attach

                string cast = "(PyCodeObject*)";
                if (Version.Version >= PythonLanguageVersion.V32) {
                    // 3.2 changed the API here...
                    cast = "";
                }

                var hostCode = @"#include <python.h>
#include <windows.h>
#include <stdio.h>

int main(int argc, char* argv[]) {
    Py_Initialize();
    auto event = OpenEventA(EVENT_ALL_ACCESS, FALSE, argv[1]);
    if(!event) {
        printf(""Failed to open event\r\n"");
    }
    printf(""Waiting for event\r\n"");
    if(WaitForSingleObject(event, INFINITE)) {
        printf(""Wait failed\r\n"");
    }

    auto loc = PyDict_New ();
    auto glb = PyDict_New ();

    auto src = " + cast + @"Py_CompileString (""while 1:\n    pass"", ""<stdin>"", Py_file_input);

    if(src == nullptr) {
        printf(""Failed to compile code\r\n"");
    }
    printf(""Executing\r\n"");
    PyEval_EvalCode(src, glb, loc);
}";
                AttachTest(hostCode);
            }
        }

        /// <summary>
        /// Attempts to attach w/ code only running on new threads which are initialized using PyGILState_Ensure
        /// </summary>
        [TestMethod, Priority(0)]
        public void AttachNewThread_PyGILState_Ensure() {
            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython doesn't support attach


                File.WriteAllText("gilstate_attach.py", @"def test():
    for i in range(10):
        print(i)

    return 0");

                var hostCode = @"#include <Windows.h>
#include <process.h>
#undef _DEBUG
#include <Python.h>

PyObject *g_pFunc;

void Thread(void*)
{
    printf(""Worker thread started %x\r\n"", GetCurrentThreadId());
    while (true)
    {
        PyGILState_STATE state = PyGILState_Ensure();
        PyObject *pValue;

        pValue = PyObject_CallObject(g_pFunc, 0);
        if (pValue != NULL) {
            //printf(""Result of call: %ld\n"", PyInt_AsLong(pValue));
            Py_DECREF(pValue);
        }
        else {
            PyErr_Print();
            return;
        }
        PyGILState_Release(state);

        Sleep(1000);
    }
}

void main()
{
    PyObject *pName, *pModule;

    Py_Initialize();
    PyEval_InitThreads();
    pName = CREATE_STRING(""gilstate_attach"");

    pModule = PyImport_Import(pName);
    Py_DECREF(pName);

    if (pModule != NULL) {
        g_pFunc = PyObject_GetAttrString(pModule, ""test"");

        if (g_pFunc && PyCallable_Check(g_pFunc))
        {
            DWORD threadID;
            threadID = _beginthread(&Thread, 1024*1024, 0);
            threadID = _beginthread(&Thread, 1024*1024, 0);

            PyEval_ReleaseLock();
            while (true);
        }
        else
        {
            if (PyErr_Occurred())
                PyErr_Print();
        }
        Py_XDECREF(g_pFunc);
        Py_DECREF(pModule);
    }
    else
    {
        PyErr_Print();
        return;
    }
    Py_Finalize();
    return;
}".Replace("CREATE_STRING", CreateString);
                CompileCode(hostCode);

                // start the test process w/ our handle
                Process p = Process.Start("test.exe");

                System.Threading.Thread.Sleep(1500);

                AutoResetEvent attached = new AutoResetEvent(false);
                AutoResetEvent bpHit = new AutoResetEvent(false);
                PythonProcess proc;
                ConnErrorMessages errReason;
                if ((errReason = PythonProcess.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
                    Assert.Fail("Failed to attach {0}", errReason);
                } else {
                    Console.WriteLine("Attached");
                }

                proc.ProcessLoaded += (sender, args) => {
                    Console.WriteLine("Process loaded");
                    attached.Set();
                };
                proc.StartListening();

                Assert.IsTrue(attached.WaitOne(20000));

                proc.BreakpointHit += (sender, args) => {
                    Console.WriteLine("Breakpoint hit");
                    bpHit.Set();
                };

                var bp = proc.AddBreakPoint("gilstate_attach.py", 3);
                bp.Add();

                Assert.IsTrue(bpHit.WaitOne(20000));
                proc.Detach();

                p.Kill();
            }
        }

        /// <summary>
        /// Attempts to attach w/ code only running on new threads which are initialized using PyThreadState_New
        /// </summary>
        [TestMethod, Priority(0)]
        public void AttachNewThread_PyThreadState_New() {

            if (GetType() != typeof(DebuggerTestsIpy) &&    // IronPython doesn't support attach
                Version.Version <= PythonLanguageVersion.V31) {    // PyEval_AcquireLock deprecated in 3.2
                File.WriteAllText("gilstate_attach.py", @"def test():
    for i in range(10):
        print(i)

    return 0");

                var hostCode = @"#include <Windows.h>
#include <process.h>
#undef _DEBUG
#include <Python.h>

PyObject *g_pFunc;

void Thread(void*)
{
    printf(""Worker thread started %x\r\n"", GetCurrentThreadId());
    while (true)
    {
        PyEval_AcquireLock();
        PyInterpreterState* pMainInterpreterState = PyInterpreterState_Head();
        auto pThisThreadState = PyThreadState_New(pMainInterpreterState);
        PyThreadState_Swap(pThisThreadState);

        PyObject *pValue;

        pValue = PyObject_CallObject(g_pFunc, 0);
        if (pValue != NULL) {
            //printf(""Result of call: %ld\n"", PyInt_AsLong(pValue));
            Py_DECREF(pValue);
        }
        else {
            PyErr_Print();
            return;
        }

        PyThreadState_Swap(NULL);
        PyThreadState_Clear(pThisThreadState);
        PyThreadState_Delete(pThisThreadState);
        PyEval_ReleaseLock();

        Sleep(1000);
    }
}

void main()
{
    PyObject *pName, *pModule;

    Py_Initialize();
    PyEval_InitThreads();
    pName = CREATE_STRING(""gilstate_attach"");

    pModule = PyImport_Import(pName);
    Py_DECREF(pName);

    if (pModule != NULL) {
        g_pFunc = PyObject_GetAttrString(pModule, ""test"");

        if (g_pFunc && PyCallable_Check(g_pFunc))
        {
            DWORD threadID;
            threadID = _beginthread(&Thread, 1024*1024, 0);
            threadID = _beginthread(&Thread, 1024*1024, 0);
            PyEval_ReleaseLock();

            while (true);
        }
        else
        {
            if (PyErr_Occurred())
                PyErr_Print();
        }
        Py_XDECREF(g_pFunc);
        Py_DECREF(pModule);
    }
    else
    {
        PyErr_Print();
        return;
    }
    Py_Finalize();
    return;
}".Replace("CREATE_STRING", CreateString);
                CompileCode(hostCode);

                // start the test process w/ our handle
                Process p = Process.Start("test.exe");

                System.Threading.Thread.Sleep(1500);

                AutoResetEvent attached = new AutoResetEvent(false);
                AutoResetEvent bpHit = new AutoResetEvent(false);
                PythonProcess proc;
                ConnErrorMessages errReason;
                if ((errReason = PythonProcess.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
                    Assert.Fail("Failed to attach {0}", errReason);
                } else {
                    Console.WriteLine("Attached");
                }

                proc.ProcessLoaded += (sender, args) => {
                    Console.WriteLine("Process loaded");
                    attached.Set();
                };
                proc.StartListening();

                Assert.IsTrue(attached.WaitOne(20000));

                proc.BreakpointHit += (sender, args) => {
                    Console.WriteLine("Breakpoint hit");
                    bpHit.Set();
                };

                var bp = proc.AddBreakPoint("gilstate_attach.py", 3);
                bp.Add();

                Assert.IsTrue(bpHit.WaitOne(20000));
                proc.Detach();

                p.Kill();
            }
        }

        public virtual string CreateString {
            get {
                return "PyString_FromString";
            }
        }

        [TestMethod, Priority(0)]
        public void AttachTimeoutThreadsInitialized() {
            if (GetType() != typeof(DebuggerTestsIpy)) {    // IronPython doesn't support attach

                string cast = "(PyCodeObject*)";
                if (Version.Version >= PythonLanguageVersion.V32) {
                    // 3.2 changed the API here...
                    cast = "";
                }


                var hostCode = @"#include <python.h>
#include <windows.h>

int main(int argc, char* argv[]) {
    Py_Initialize();
    PyEval_InitThreads();

    auto event = OpenEventA(EVENT_ALL_ACCESS, FALSE, argv[1]);
    WaitForSingleObject(event, INFINITE);

    auto loc = PyDict_New ();
    auto glb = PyDict_New ();

    auto src = " + cast + @"Py_CompileString (""while 1:\n    pass"", ""<stdin>"", Py_file_input);

    if(src == nullptr) {
        printf(""Failed to compile code\r\n"");
    }
    printf(""Executing\r\n"");
    PyEval_EvalCode(src, glb, loc);
}";
                AttachTest(hostCode);

            }
        }

        private void AttachTest(string hostCode) {
            CompileCode(hostCode);

            // start the test process w/ our handle
            var eventName = Guid.NewGuid().ToString();
            EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.AutoReset, eventName);
            ProcessStartInfo psi = new ProcessStartInfo("test.exe", eventName);
            psi.UseShellExecute = false;
            psi.RedirectStandardError = psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;

            Process p = Process.Start(psi);
            var outRecv = new OutputReceiver();
            p.OutputDataReceived += outRecv.OutputDataReceived;
            p.ErrorDataReceived += outRecv.OutputDataReceived;
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();

            try {
                // start the attach with the GIL held
                AutoResetEvent attached = new AutoResetEvent(false);
                PythonProcess proc;
                ConnErrorMessages errReason;
                if ((errReason = PythonProcess.TryAttach(p.Id, out proc)) != ConnErrorMessages.None) {
                    Assert.Fail("Failed to attach {0}", errReason);
                }

                bool isAttached = false;
                proc.ProcessLoaded += (sender, args) => {
                    attached.Set();
                    isAttached = false;
                };
                proc.StartListening();

                Assert.AreEqual(false, isAttached); // we shouldn't have attached yet, we should be blocked
                handle.Set();   // let the code start running

                Assert.IsTrue(attached.WaitOne(20000));
                proc.Detach();

                p.Kill();
            } finally {
                Debug.WriteLine(String.Format("Process output: {0}", outRecv.Output.ToString()));
            }
        }

        private void CompileCode(string hostCode) {
            File.WriteAllText("test.cpp", hostCode);

            // compile our host code...
            var startInfo = new ProcessStartInfo(
                Path.Combine(GetVCInstallDir(), "bin", "cl.exe"),
                String.Format("/I{0}\\Include test.cpp /link /libpath:{0}\\libs", Path.GetDirectoryName(Version.Path))
            );
            
            startInfo.EnvironmentVariables["PATH"] = Environment.GetEnvironmentVariable("PATH") + ";" + GetVSIDEInstallDir();
            startInfo.EnvironmentVariables["INCLUDE"] = Path.Combine(GetVCInstallDir(), "INCLUDE") + ";" + Path.Combine(GetWindowsSDKDir(), "Include");
            startInfo.EnvironmentVariables["LIB"] = Path.Combine(GetVCInstallDir(), "LIB") + ";" + Path.Combine(GetWindowsSDKDir(), "Lib");
            Console.WriteLine(startInfo.EnvironmentVariables["LIB"]);

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            var compileProcess = Process.Start(startInfo);

            var outputReceiver = new OutputReceiver();
            compileProcess.OutputDataReceived += outputReceiver.OutputDataReceived; // for debugging if you change the code...
            compileProcess.ErrorDataReceived += outputReceiver.OutputDataReceived;
            compileProcess.BeginErrorReadLine();
            compileProcess.BeginOutputReadLine();
            compileProcess.WaitForExit();

            Assert.AreEqual(0, compileProcess.ExitCode, 
                "Incorrect exit code: " + compileProcess.ExitCode + Environment.NewLine +
                outputReceiver.Output.ToString()
            );
        }

        private static string GetVCInstallDir() {
            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\VisualStudio\\" + VSUtility.Version + "\\Setup\\VC")) {
                return key.GetValue("ProductDir").ToString();
            }
        }

        private static string GetWindowsSDKDir() {
            string[] sdkVersions = new[] { "v7.0A", "v8.0A", "v7.0" };
            object regValue = null;
            foreach (var sdkVersion in sdkVersions) {
                regValue = Registry.GetValue(
                    "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Microsoft SDKs\\Windows\\" + sdkVersion,
                    "InstallationFolder",
                    null);

                if (regValue != null && Directory.Exists(Path.Combine(regValue.ToString(), "Include"))) {
                    break;
                }
            }
            
            if (regValue == null) {
                Assert.IsTrue(Directory.Exists("C:\\Program Files\\Microsoft SDKs\\Windows\\v7.0\\Include"), "Windows SDK is not installed");
                return "C:\\Program Files\\Microsoft SDKs\\Windows\\v7.0";
            }

            return regValue.ToString();
        }

        private static string GetVSIDEInstallDir() {
            using (var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\VisualStudio\\" + VSUtility.Version + "\\Setup\\VS")) {
                return key.GetValue("EnvironmentDirectory").ToString();
            }
        }

        #endregion

        #region Output Tests

        [TestMethod, Priority(0)]
        public void Test3xStdoutBuffer() {
            if (Version.Version.Is3x()) {
                var debugger = new PythonDebugger();

                bool gotOutput = false;
                var process = DebugProcess(debugger, DebuggerTestPath + @"StdoutBuffer3x.py", (processObj, threadObj) => {
                    processObj.DebuggerOutput += (sender, args) => {
                        Assert.IsTrue(!gotOutput, "got output more than once");
                        gotOutput = true;
                        Assert.AreEqual(args.Output, "foo");
                    };
                }, debugOptions: PythonDebugOptions.RedirectOutput);

                process.Start();
                process.WaitForExit();

                Assert.IsTrue(gotOutput, "failed to get output");
            }
        }

        #endregion

        internal override PythonVersion Version {
            get {
                return PythonPaths.Python26;
            }
        }
    }

    public abstract class DebuggerTests3x : DebuggerTests {
        public override string EnumChildrenTestName {
            get {
                return "EnumChildTestV3.py";
            }
        }
        public override string ComplexExceptions {
            get {
                return "ComplexExceptionsV3.py";
            }
        }
        
        public override string CreateString {
            get {
                return "PyUnicodeUCS2_FromString";
            }
        }
    }

    [TestClass]
    public class DebuggerTests30 : DebuggerTests3x {
        internal override PythonVersion Version {
            get {
                return PythonPaths.Python30;
            }
        }
    }

    [TestClass]
    public class DebuggerTests31 : DebuggerTests3x {
        internal override PythonVersion Version {
            get {
                return PythonPaths.Python31;
            }
        }
    }

    [TestClass]
    public class DebuggerTests32 : DebuggerTests3x {
        internal override PythonVersion Version {
            get {
                return PythonPaths.Python32;
            }
        }
    }

    [TestClass]
    public class DebuggerTests27 : DebuggerTests {
        internal override PythonVersion Version {
            get {
                return PythonPaths.Python27;
            }
        }
    }

    [TestClass]
    public class DebuggerTests25 : DebuggerTests {
        internal override PythonVersion Version {
            get {
                return PythonPaths.Python25;
            }
        }
    }

    [TestClass]
    public class DebuggerTestsIpy : DebuggerTests {
        internal override PythonVersion Version {
            get {
                return PythonPaths.IronPython27;
            }
        }
    }
}