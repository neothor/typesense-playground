namespace Api.Infrastructure
{
    using Api.Domain;
    using System.Linq.Expressions;
    using Typesense;

    public class SchemaBuilder<T>
        where T : IIndexable
    {
        private string _defaultSortingField;
        private readonly IList<Field> _fields;

        public SchemaBuilder()
        {
            _fields = new List<Field>();
        }

        public SchemaBuilder<T> AddField(Expression<Func<T, object>> expression, bool? facet = null, bool? optional = null, Func<(string name, FieldType type), Field>? ctor = null)
        {
            _fields.Add(TypesenseHelper.BuildField(expression, facet, optional, ctor));
            return this;
        }

        public SchemaBuilder<T> SetDefaultSortingField(Expression<Func<T, object>> expression)
        {
            _defaultSortingField = TypesenseHelper.GetFieldName(expression);
            return this;
        }

        public Schema Build()
        {
            if(string.IsNullOrEmpty(_defaultSortingField))
            {
                return new Schema(TypesenseHelper.GetCollectionName<T>(), _fields);
            }

            return new Schema(TypesenseHelper.GetCollectionName<T>(), _fields, _defaultSortingField);
        }
    }
}
