namespace Wbtb.Extensions.Data.Postgres
{
    /// <summary>
    /// Carrier for query parameters because Npgsql is too ****** to expose their own carrier
    /// </summary>
    public class QueryParameter
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public QueryParameter()
        { 

        }

        public QueryParameter(string name, object value)
        { 
            this.Name = name;
            this.Value = value;
        }

    }
}
