﻿using System;
using System.Linq.Expressions;
using ReactGraph.Construction;
using ReactGraph.Tests.TestObjects;
using Shouldly;
using Xunit;

namespace ReactGraph.Tests
{
    public class ExpressionStringBuilderTests
    {
        [Fact]
        public void VerifyProperties()
        {
            var viewModel = new SimpleWithNotification();
            var mortgateCalculatorViewModel = new MortgateCalculatorViewModel();
            VerifyExpression(() => Foo, "Foo");
            VerifyExpression(() => viewModel.Value, "viewModel.Value");
            VerifyExpression(() => mortgateCalculatorViewModel.PaymentSchedule.HasValidationError, "mortgateCalculatorViewModel.PaymentSchedule.HasValidationError");
        }

        [Fact]
        public void VerifyFormulas()
        {
            var notifies = new Totals
            {
                TaxPercentage = 20
            };
            var mortgateCalculatorViewModel = new MortgateCalculatorViewModel();
            VerifyExpression(() => (int)(notifies.SubTotal * (1m + (notifies.TaxPercentage / 100m))), "(notifies.SubTotal * (1 + (notifies.TaxPercentage / 100)))");
            VerifyExpression(() => InstanceMethodCall(), "InstanceMethodCall()");
            VerifyExpression(() => notifies.InstanceMethod(), "notifies.InstanceMethod()");
            VerifyExpression(() => mortgateCalculatorViewModel.RegeneratePaymentSchedule(true), "mortgateCalculatorViewModel.RegeneratePaymentSchedule(true)");
            VerifyExpression(() => StaticMethod(notifies.SubTotal), "StaticMethod(notifies.SubTotal)");
            VerifyExpression(() => StaticMethodLotsOfArguments(notifies.TaxPercentage, notifies.TaxPercentage, notifies.TaxPercentage, notifies.TaxPercentage), "StaticMethodLotsOfArguments(...)");
        }

        static void StaticMethod(int subTotal)
        {
        }

        static void StaticMethodLotsOfArguments(int a, int b, int c, int d)
        {
        }

        [Fact]
        public void VerifyAction()
        {
            Action action = () => { };
            VerifyExpression(() => action(), "action()");
        }

        object InstanceMethodCall()
        {
            return null;
        }

        void VerifyExpression<T>(Expression<Func<T>> func, string expression)
        {
            ExpressionStringBuilder.ToString(func).ShouldBe(expression);
        }

        void VerifyExpression(Expression<Action> func, string expression)
        {
            ExpressionStringBuilder.ToString(func).ShouldBe(expression);
        }

        public object Foo { get; set; }
    }
}