namespace Api.Infrastructure
{
    using System.Linq.Expressions;
    using Api.Domain;
    using Humanizer;
    using Typesense;

    public static class SchemaHelper
    {
        public static SchemaBuilder<T> Create<T>() 
            where T : IIndexable
        {
            return new SchemaBuilder<T>();
        }

        public static Field BuildField<T>(Expression<Func<T, object>> expression, bool? facet = null, bool? optional = null, Func<(string name, FieldType type), Field>? ctor = null)
            where T : IIndexable
        {
            var fieldName = GetFieldName(expression);
            var fieldType = GetFieldType(expression);

            if(ctor == null)
            {
                return new Field(fieldName, fieldType, facet, optional);
            }

            return ctor((fieldName, fieldType));
        }

        public static string GetFieldName<T>(Expression<Func<T, object>> expression)
            where T : IIndexable
        {
            if (expression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name.Camelize();
            }

            if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
            {
                return operand.Member.Name.Camelize();
            }

            throw new ArgumentException("Invalid expression");
        }

        public static FieldType GetFieldType<T>(Expression<Func<T, object>> expression)
            where T : IIndexable
        {
            Type type;

            if (expression.Body is MemberExpression memberExpression)
            {
                type = GetMemberType(memberExpression);
            }
            else if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression operand)
            {
                type = GetMemberType(operand);
            }
            else
            {
                throw new ArgumentException("Invalid expression");
            }

            return GetFieldTypeFromType(type);
        }

        public static string GetCollectionName<T>()
            where T : IIndexable
        {
            return typeof(T).Name.Pluralize().ToLower();
        }

        private static Type GetMemberType(MemberExpression memberExpression)
        {
            return memberExpression.Member.MemberType switch
            {
                System.Reflection.MemberTypes.Field => ((System.Reflection.FieldInfo)memberExpression.Member).FieldType,
                System.Reflection.MemberTypes.Property => ((System.Reflection.PropertyInfo)memberExpression.Member).PropertyType,
                _ => throw new ArgumentException("Member is not a field or property")
            };
        }

        private static FieldType GetFieldTypeFromType(Type type)
        {
            if (type == typeof(string))
            {
                return FieldType.String;
            }
            else if (type == typeof(int))
            {
                return FieldType.Int32;
            }
            else if (type == typeof(long))
            {
                return FieldType.Int64;
            }
            else if (type == typeof(float))
            {
                return FieldType.Float;
            }
            else if (type == typeof(bool))
            {
                return FieldType.Bool;
            }
            else if (type == typeof(double))
            {
                return FieldType.Float;  // Assuming double should be mapped to float
            }
            else if (type == typeof(decimal))
            {
                return FieldType.Float;  // Assuming decimal should be mapped to float
            }
            else if (type.IsArray && type.GetElementType() == typeof(double) && type.GetArrayRank() == 1)
            {
                return FieldType.GeoPoint;  // Assuming a double array represents a geopoint
            }
            else
            {
                throw new NotSupportedException($"Type '{type.FullName}' is not supported.");
            }
        }
    }
}
