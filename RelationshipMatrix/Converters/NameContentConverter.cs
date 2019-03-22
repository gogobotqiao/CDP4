﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NameContentConverter.cs" company="RHEA System S.A.">
//   Copyright (c) 2015-2019 RHEA System S.A.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace CDP4RelationshipMatrix.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    using System.Windows.Data;
    using CDP4Common.CommonData;
    using DevExpress.Xpf.Grid;
    using ViewModels;

    /// <summary>
    /// The converter to retrieve the name to display for a row based on the <see cref="EditGridCellData"/>
    /// </summary>
    public class NameContentConverter : IValueConverter
    {
        /// <summary>
        /// The conversion method returns the object associated to the current fieldname
        /// </summary>
        /// <param name="value">
        /// The incoming value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter passed on to this conversion.
        /// </param>
        /// <param name="culture">
        /// The culture information.
        /// </param>
        /// <returns>
        /// The <see cref="object"/> containing the same objects as the input collection.
        /// </returns>
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var gridData = value as EditGridCellData;
            if (gridData == null)
            {
                return null;
            }

            var row = gridData.RowData.Row as ExpandoObject;
            if (row == null)
            {
                return null;
            }

            var dic = (IDictionary<string, object>)row;
            var definedThing = (dic[gridData.Column.FieldName] as MatrixCellViewModel)?.Source1 as DefinedThing;
            return definedThing != null ? definedThing.Name : "-";
        }

        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="value">
        /// The incoming collection.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter passed on to this conversion.
        /// </param>
        /// <param name="culture">
        /// The culture information.
        /// </param>
        /// <returns>
        /// The result 
        /// </returns>
        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}