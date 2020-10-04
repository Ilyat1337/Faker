using GeneratorInterface;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace FakerLib
{
    public class FakerConfig
    {
        private static readonly string WRONG_FIELD_GET_EXPR = "Wrong member access expression format.";
        private static readonly string RETURN_TYPE_MISMATCH = "Expression and generator return type mismatch.";

        private List<(Type, string, Type, IGenerator)> userGenerators;

        public FakerConfig()
        {
            userGenerators = new List<(Type, string, Type, IGenerator)>();
        }

        public void Add<ClassType, ResultType>(Expression<Func<ClassType, ResultType>> fieldGetExpr, IGenerator generator)
        {
            MemberInfo classField = ParseFieldGetExpression(fieldGetExpr);
            if (!generator.Generate().GetType().Equals(typeof(ResultType)))
                throw new FakerConfigException(RETURN_TYPE_MISMATCH);
            userGenerators.Add((typeof(ClassType), classField.Name, typeof(ResultType), generator));
        }

        private MemberInfo ParseFieldGetExpression<ClassType, ResultType>(Expression<Func<ClassType, ResultType>> fieldGetExpr)
        {
            if (!(fieldGetExpr.Body.NodeType == ExpressionType.MemberAccess))
                throw new FakerConfigException(WRONG_FIELD_GET_EXPR);
            MemberExpression memberExpression = fieldGetExpr.Body as MemberExpression;
            return memberExpression.Member;
        }

        internal List<(Type, string, Type, IGenerator)> GetUserGenerators()
        {
            return userGenerators;
        }
    }
}
