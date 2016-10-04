// Copyright 2016 OSIsoft, LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed
// on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;

/****************************************************************************************************************************************
 * Background Information:
 * 
 * Randy Esposito turned me onto a sortable binding list with some VB.NET code:
 *      http://www.tech.windowsapplication1.com/content/sortable-binding-list-custom-data-objects
 * 
 * Randy's VB code & links was probably a translation from C# originally.  So rather than convert a conversion, 
 * I looked up the original stuff.  A few years had passed since Randy's original conversion, there was some 
 * updated info about using a STABLE sort that I have added at the bottom of this file.
 * 
 *   Core classes:  http://www.timvw.be/2008/08/02/presenting-the-sortablebindinglistt-take-two/
 *
 *   Rick added  :  http://stackoverflow.com/questions/7342319/simplest-way-to-make-sortablebindinglist-use-a-stable-sort
 *---------------------------------------------------------------------------------------------------------------------------------------
 * While a binding list does not have to be associated to a Form, I have only used the SortableBindingList in association
 * with a BindingSource to a DataGridView on a WinForm.  Thus for organizational purposes, I am placing the class here.
 ***************************************************************************************************************************************/

namespace PIDevClub.Library.WinForm
{
    /// <summary>
    /// A sortable <see cref="BindingList{T}"/> with a stable sort.  May be useful with a <see cref="BindingSource"/> control
    /// on a <see cref="Windows.Form"/> that is bound to a <see cref="DataGridView"/> .
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SortableBindingList<T> : BindingList<T>
    {
        private readonly Dictionary<Type, PropertyComparer<T>> comparers;
        private bool isSorted;
        private ListSortDirection listSortDirection;
        private PropertyDescriptor propertyDescriptor;

        public SortableBindingList()
            : base(new List<T>())
        {
            AllowSorting = true;
            this.comparers = new Dictionary<Type, PropertyComparer<T>>();
        }

        public SortableBindingList(IEnumerable<T> enumeration)
            : base(new List<T>(enumeration))
        {
            AllowSorting = true;
            this.comparers = new Dictionary<Type, PropertyComparer<T>>();
        }

        public bool AllowSorting { get; set; }
        protected override bool SupportsSortingCore
        {
            get { return AllowSorting; }
        }

        protected override bool IsSortedCore
        {
            get { return this.isSorted; }
        }

        protected override PropertyDescriptor SortPropertyCore
        {
            get { return this.propertyDescriptor; }
        }

        protected override ListSortDirection SortDirectionCore
        {
            get { return this.listSortDirection; }
        }

        protected override bool SupportsSearchingCore
        {
            get { return true; }
        }

        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
        {
            List<T> itemsList = (List<T>)this.Items;

            Type propertyType = property.PropertyType;
            PropertyComparer<T> comparer;
            if (!this.comparers.TryGetValue(propertyType, out comparer))
            {
                comparer = new PropertyComparer<T>(property, direction);
                this.comparers.Add(propertyType, comparer);
            }

            comparer.SetPropertyAndDirection(property, direction);
            itemsList.StableSort(comparer);

            this.propertyDescriptor = property;
            this.listSortDirection = direction;
            this.isSorted = true;

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override void RemoveSortCore()
        {
            this.isSorted = false;
            this.propertyDescriptor = base.SortPropertyCore;
            this.listSortDirection = base.SortDirectionCore;

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override int FindCore(PropertyDescriptor property, object key)
        {
            int count = this.Count;
            for (int i = 0; i < count; ++i)
            {
                T element = this[i];
                if (property.GetValue(element).Equals(key))
                {
                    return i;
                }
            }

            return -1;
        }

    } //end SortableBindingList class


    //
    // PropertyComparer class
    //
    public class PropertyComparer<T> : IComparer<T>
    {
        private readonly IComparer comparer;
        private PropertyDescriptor propertyDescriptor;
        private int reverse;

        public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
        {
            this.propertyDescriptor = property;
            Type comparerForPropertyType = typeof(Comparer<>).MakeGenericType(property.PropertyType);
            this.comparer = (IComparer)comparerForPropertyType.InvokeMember("Default", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.Public, null, null, null);
            this.SetListSortDirection(direction);
        }

        #region IComparer<T> Members

        public int Compare(T x, T y)
        {
            return this.reverse * this.comparer.Compare(this.propertyDescriptor.GetValue(x), this.propertyDescriptor.GetValue(y));
        }

        #endregion

        private void SetPropertyDescriptor(PropertyDescriptor descriptor)
        {
            this.propertyDescriptor = descriptor;
        }

        private void SetListSortDirection(ListSortDirection direction)
        {
            this.reverse = direction == ListSortDirection.Ascending ? 1 : -1;
        }

        public void SetPropertyAndDirection(PropertyDescriptor descriptor, ListSortDirection direction)
        {
            this.SetPropertyDescriptor(descriptor);
            this.SetListSortDirection(direction);
        }

    } //end PropertyComparer class


    //
    // http://stackoverflow.com/questions/7342319/simplest-way-to-make-sortablebindinglist-use-a-stable-sort
    //
    public static class ListExtensions
    {
        public static void StableSort<T>(this List<T> list, IComparer<T> comparer)
        {
            var pairs = list.Select((value, index) => Tuple.Create(value, index)).ToList();
            pairs.Sort((x, y) =>
            {
                int result = comparer.Compare(x.Item1, y.Item1);
                return result != 0 ? result : x.Item2 - y.Item2;
            });
            list.Clear();
            list.AddRange(pairs.Select(key => key.Item1));
        }
    } // end ListExtensions class

}
