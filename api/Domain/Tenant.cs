namespace Api.Domain
{
    public static class Tenant
    {
        public static string GetTenantKey(char tenant)
        {
            return $"tenant-{tenant}";
        }
    }
}
