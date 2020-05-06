namespace CDP4Composition.Reporting
{
    using System;
    using System.Collections.Generic;

    public interface IReportViewModel<T> where T : IReportViewModel<T>
    {
        T Parent { get; set; }

        List<T> Children { get; set; }

        string ReportOrder { get; set; }

        T GetEmptyOrderedRow();
    }
}
