using System.Linq.Expressions;
using System.Text;

namespace Dapper.AutoQuery.Lib
{
    public class CustomQueryTranslator : ExpressionVisitor
    {
        private StringBuilder _sb;
        private string _orderBy = string.Empty;
        private int? _skip = null;
        private int? _take = null;
        private string _whereClause = string.Empty;

        public int? Skip => _skip;
        public int? Take => _take;
        public string OrderBy => _orderBy;
        public string WhereClause => _whereClause;

        public ParameterExpression MainParameter { get; set; }
        public ParameterExpression[] OtherParameters { get; set; }

        IOperationMap _operationMap = new SqlServerOperationMap();

        public CustomQueryTranslator()
        {
        }

        public string Translate(Expression expression)
        {
            if (expression is not LambdaExpression lambda || lambda.Parameters.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Invalid expression type or parameters missing - type: {expression.GetType().Name}");
            }

            MainParameter = lambda.Parameters.First();
            OtherParameters = lambda.Parameters.Skip(1).ToArray();

            _sb = new StringBuilder();

            Visit(expression);
            _whereClause = _sb.ToString();
            return _whereClause;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                Visit(m.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);
                return m;
            }
            else if (m.Method.Name == "Take")
            {
                if (ParseTakeExpression(m))
                {
                    Expression nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "Skip")
            {
                if (ParseSkipExpression(m))
                {
                    Expression nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OrderBy")
            {
                if (ParseOrderByExpression(m, "ASC"))
                {
                    Expression nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OrderByDescending")
            {
                if (ParseOrderByExpression(m, "DESC"))
                {
                    Expression nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "Contains")
            {
                switch (ParseContainsExpression(m))
                {
                    case EContainsType.String:
                        {
                            HandleStringContainsMethod(m);
                            return m;
                        }
                    case EContainsType.Collection:
                        {
                            Visit(m.Object);
                            Visit(MethodCallExpression.Call(m.Object, m.Method));
                            Visit(m.Arguments.First());
                            return m;
                        }
                    default:
                        break;
                }
            }
            else if (m.Method.Name == "Equals")
            {
                Visit(m.Object);
                VisitMethod(m);
                return Visit(m.Arguments.First());
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        private Expression HandleLINQContainsMethod(MethodCallExpression m)
        {
            Visit(m.Object);
            VisitMethod(m);
            Visit(m.Arguments.First());
            return m;
        }
        private Expression HandleStringContainsMethod(MethodCallExpression m)
        {
            Visit(m.Object);

            VisitMethod(m);

            Visit(Expression.Constant("%"));
            Visit(Expression.Constant('+'));

            Visit(m.Arguments.First());

            Visit(Expression.Constant('+'));
            return Visit(Expression.Constant("%"));
        }

        protected Expression VisitMethod(MethodCallExpression m)
        {
            if (_operationMap.TryGetMapForOperator(m.Method.Name, out var op))
            {
                var method = op.ForType(m.Method.DeclaringType!);
                _sb.Append(" " + method.SqlName + " ");
                
                return m;
            }

            throw new Exception($"Operation map for method {m.Method.Name} is not defined!");
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    _sb.Append(" NOT ");
                    Visit(u.Operand);
                    break;
                case ExpressionType.Convert:
                    Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            _sb.Append("(");
            Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.And:
                    _sb.Append(DAQDefaults.Configuration.FieldLayoutPolicy.Prefix + " AND ");
                    break;

                case ExpressionType.AndAlso:
                    _sb.Append(DAQDefaults.Configuration.FieldLayoutPolicy.Prefix + " AND ");
                    break;

                case ExpressionType.Or:
                    _sb.Append(DAQDefaults.Configuration.FieldLayoutPolicy.Prefix + " OR ");
                    break;

                case ExpressionType.OrElse:
                    _sb.Append(DAQDefaults.Configuration.FieldLayoutPolicy.Prefix + " OR ");
                    break;

                case ExpressionType.Equal:
                    if (IsNullConstant(b.Right))
                    {
                        _sb.Append(" IS ");
                    }
                    else
                    {
                        _sb.Append(" = ");
                    }
                    break;

                case ExpressionType.NotEqual:
                    if (IsNullConstant(b.Right))
                    {
                        _sb.Append(" IS NOT ");
                    }
                    else
                    {
                        _sb.Append(" <> ");
                    }
                    break;

                case ExpressionType.LessThan:
                    _sb.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    _sb.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    _sb.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    _sb.Append(" >= ");
                    break;


                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));

            }

            Visit(b.Right);
            _sb.Append(")");
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;

            if (q == null && c.Value == null)
            {
                _sb.Append("NULL");
            }
            else if (q == null)
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        _sb.Append(((bool)c.Value) ? 1 : 0);
                        break;

                    case TypeCode.String:
                        _sb.Append('\'');
                        _sb.Append(c.Value);
                        _sb.Append('\'');
                        break;

                    case TypeCode.DateTime:
                        _sb.Append('\'');
                        _sb.Append(c.Value);
                        _sb.Append('\'');
                        break;

                    case TypeCode.Object:
                        throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", c.Value));

                    default:
                        _sb.Append(c.Value);
                        break;
                }
            }

            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                if (m.Expression.ToString() == MainParameter.Name)
                {
                    _sb.Append(m.Member.Name);
                    return m;
                }

                _sb.Append(DAQDefaults.Configuration.VarPrefix + m.Member.Name);
                return m;
            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }
        protected bool IsNullConstant(Expression exp)
        {
            return (exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null);
        }



        private bool ParseOrderByExpression(MethodCallExpression expression, string order)
        {
            UnaryExpression unary = (UnaryExpression)expression.Arguments[1];
            LambdaExpression lambdaExpression = (LambdaExpression)unary.Operand;

            lambdaExpression = (LambdaExpression)Evaluator.PartialEval(lambdaExpression);

            MemberExpression body = lambdaExpression.Body as MemberExpression;
            if (body != null)
            {
                if (string.IsNullOrEmpty(_orderBy))
                {
                    _orderBy = string.Format("{0} {1}", body.Member.Name, order);
                }
                else
                {
                    _orderBy = string.Format("{0}, {1} {2}", _orderBy, body.Member.Name, order);
                }

                return true;
            }

            return false;
        }

        private bool ParseTakeExpression(MethodCallExpression expression)
        {
            ConstantExpression sizeExpression = (ConstantExpression)expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size))
            {
                _take = size;
                return true;
            }

            return false;
        }
        private EContainsType ParseContainsExpression(MethodCallExpression expression)
        {
            if (expression is null || expression.NodeType != ExpressionType.Call)
            {
                return EContainsType.None;
            }


            return expression.Arguments.First().Type == typeof(String)
                ? EContainsType.String
                : EContainsType.Collection;
        }

        private bool ParseSkipExpression(MethodCallExpression expression)
        {
            ConstantExpression sizeExpression = (ConstantExpression)expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size))
            {
                _skip = size;
                return true;
            }

            return false;
        }
    }
}