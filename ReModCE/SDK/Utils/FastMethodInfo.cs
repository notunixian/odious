using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using MelonLoader;

namespace ReModCE.SDK.Utils
{
	// stolen from old munchen source
	[PatchShield]
    internal class FastMethodInfo
    {
        private delegate object ReturnValueDelegate(object instance, object[] arguments);

        private delegate void VoidDelegate(object instance, object[] arguments);

        private readonly MethodInfo methodInfoBackup;

        private ReturnValueDelegate Delegate { get; }

        internal FastMethodInfo(MethodInfo methodInfo)
        {
            methodInfoBackup = methodInfo;
            ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "instance");
            ParameterExpression parameterExpression2 = Expression.Parameter(typeof(object[]), "arguments");
            List<Expression> list = new List<Expression>();
            ParameterInfo[] parameters = methodInfo.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameterInfo = parameters[i];
                list.Add(Expression.Convert(Expression.ArrayIndex(parameterExpression2, Expression.Constant(i)), parameterInfo.ParameterType));
            }
            MethodCallExpression methodCallExpression = Expression.Call((!methodInfo.IsStatic) ? Expression.Convert(parameterExpression, methodInfo.ReflectedType) : null, methodInfo, list);
            if (methodCallExpression.Type == typeof(void))
            {
                VoidDelegate voidDelegate = Expression.Lambda<VoidDelegate>(methodCallExpression, new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
                Delegate = delegate (object instance, object[] arguments)
                {
                    voidDelegate(instance, arguments);
                    return null;
                };
            }
            else
            {
                Delegate = Expression.Lambda<ReturnValueDelegate>(Expression.Convert(methodCallExpression, typeof(object)), new ParameterExpression[2] { parameterExpression, parameterExpression2 }).Compile();
            }
        }

        internal MethodInfo GetOriginalMethod()
        {
            return methodInfoBackup;
        }

        internal object Invoke(object instance, params object[] arguments)
        {
            return Delegate(instance, arguments);
        }
    }
}
