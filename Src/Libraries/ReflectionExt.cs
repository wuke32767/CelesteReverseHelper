using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Helpers;

namespace Celeste.Mod.ReverseHelper.Libraries
{
    internal static class ReflectionExt
    {
        public static FieldInfo fieldof<T, U>(Expression<Func<T, U>> expr)
        {
            return TryGet_Yaml(expr) switch
            {
                { } a => a,
                _ => throw new InvalidCastException("expression detail: " + expr.ToString()),
            };
            FieldInfo? TryGet_Yaml(Expression<Func<T, U>> expr)
            {
                Expression expression = expr.Body;
                if (expression is UnaryExpression unaryExpression)
                {
                    if (unaryExpression.NodeType != ExpressionType.Convert)
                    {
                        return null;
                    }

                    expression = unaryExpression.Operand;
                }

                if (expression is MemberExpression memberExpression)
                {
                    if (memberExpression.Expression != expr.Parameters[0])
                    {
                        return null;
                    }

                    return memberExpression.Member as FieldInfo;
                }

                return null;
            }
        }
        public static PropertyInfo propertyof<T, U>(Expression<Func<T, U>> expr)
        {
            return expr.AsProperty();
        }
        public static MethodInfo methodof<T>(Expression<Func<T, Delegate>> expr)
        {
            return impl(expr);
        }
        public static MethodInfo methodof<T, U>(Expression<Func<T, U>> expr)
        {
            return impl(expr);
        }
        public static T Any<T>()
        {
            throw new InvalidOperationException("unreachable");
        }
        public static MethodInfo methodof<U>(Expression<Func<U>> expr)
        {
            return impl(expr);
        }
        public static MethodInfo methodof<U>(Expression<Action<U>> expr)
        {
            return impl(expr);
        }
        public static MethodInfo methodof<U>(Expression<Action> expr)
        {
            return impl(expr);
        }

        static MethodInfo impl(Expression expr)
        {
            static MethodInfo rimpl(Expression expr)
            {
                switch (expr)
                {
                    case LambdaExpression lambdaExpression:
                        return rimpl(lambdaExpression.Body);
                    case UnaryExpression unaryExpression when expr.NodeType == ExpressionType.Convert:
                        return rimpl(unaryExpression.Operand);
                    case MethodCallExpression methodCallExpression:
                        if (methodCallExpression.Method.Name.Contains("CreateDelegate")
                            && (methodCallExpression.Method.DeclaringType?.IsAssignableTo(typeof(MethodBase)) ?? false))
                        {
                            return rimpl(methodCallExpression.Object!);
                        }
                        return methodCallExpression.Method;
                    case ConstantExpression constantExpression:
                        return (MethodInfo)constantExpression.Value!;
                    default:
                        throw new InvalidCastException("current expression detail: " + expr.ToString());
                }
            }
            try
            {
                return rimpl(expr);
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException("expression detail: " + expr.ToString(), ex);
            }
        }
        public static MethodInfo methodof(Expression<Func<Delegate>> expr)
        {
            return impl(expr);
        }
        public static Hook On(this MethodInfo info, Delegate @delegate)
        {
            return new Hook(info, @delegate);
        }
        public static ILHook ILState(this MethodInfo info, params (int at, Action<ILCursor> @delegate)[] at)
        {
            return new ILHook(info, il =>
            {
                ILCursor ic = new(il);
                ILLabel[] lb = null!;
                ic.GotoNext(i => i.MatchSwitch(out lb!));
                foreach (var (i, @delegate) in at)
                {
                    ILCursor get = new(il) { Next = lb[i].Target };
                    @delegate(get);
                }
            });
        }
        public static ILHook IL(this MethodInfo info, ILContext.Manipulator @delegate)
        {
            return new ILHook(info, @delegate);
        }
    }
}
