namespace CDP4Composition.Reporting
{
    using CDP4Common.EngineeringModelData;

    public interface ICDP4ObjectDataSource
    {
        object CreateDataSource(Iteration iteration);
    }
}
