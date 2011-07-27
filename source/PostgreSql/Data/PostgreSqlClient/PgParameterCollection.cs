/*
 *  PgSqlClient - ADO.NET Data Provider for PostgreSQL 7.4+
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. 
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2003, 2006 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;

namespace PostgreSql.Data.PostgreSqlClient
{
    [ListBindable(false)]
    public sealed class PgParameterCollection 
        : DbParameterCollection
    {	
        #region · Fields ·

        private SynchronizedCollection<PgParameter> parameters;

        #endregion

        #region · Indexers ·

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new PgParameter this[string parameterName]
        {
            get { return (PgParameter)this[this.IndexOf(parameterName)]; }
            set { this[this.IndexOf(parameterName)] = (PgParameter)value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new PgParameter this[int index]
        {
            get { return (PgParameter)this.parameters[index]; }
            set { this.parameters[index] = (PgParameter)value; }
        }

        #endregion

        #region · DbParameterCollection Properties ·

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override int Count
        {
            get { return this.parameters.Count; }
        }

        public override bool IsFixedSize
        {
            get { return ((IList)this.parameters).IsFixedSize; }
        }

        public override bool IsReadOnly
        {
            get { return ((IList)this.parameters).IsReadOnly; }
        }

        public override bool IsSynchronized
        {
            get { return ((IList)this.parameters).IsSynchronized; }
        }

        public override object SyncRoot
        {
            get { return this.parameters.SyncRoot; }
        }

        #endregion

        #region · Constructors ·

        internal PgParameterCollection()
        {
            this.parameters = new SynchronizedCollection<PgParameter>();
        }

        #endregion

        #region · DbParameterCollection Protected methods ·

        protected override DbParameter GetParameter(string parameterName)
        {
            return this[parameterName];
        }

        protected override DbParameter GetParameter(int index)
        {
            return this[index];
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            this[index] = (PgParameter)value;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            this[parameterName] = (PgParameter)value;
        }

        #endregion

        #region · DbParameterCollection overriden methods ·

        public override void CopyTo(Array array, int index)
        {
            this.parameters
                .ToList()
                .CopyTo((PgParameter[])array, index);
        }

        public override void Clear()
        {
            this.parameters.Clear();
        }

        public override IEnumerator GetEnumerator()
        {
            return this.parameters.GetEnumerator();
        }

        public override void AddRange(Array values)
        {
            foreach (PgParameter parameter in values)
            {
                this.Add(parameter);
            }
        }

        public PgParameter Add(string parameterName, object value)
        {
            return this.Add(new PgParameter(parameterName, value));
        }

        public PgParameter Add(string parameterName, PgDbType providerType)
        {
            return this.Add(new PgParameter(parameterName, providerType));
        }

        public PgParameter Add(string parameterName, PgDbType providerType, int size)
        {
            return this.Add(new PgParameter(parameterName, providerType, size));
        }

        public PgParameter Add(string parameterName, PgDbType providerType, int size, string sourceColumn)
        {
            return this.Add(new PgParameter(parameterName, providerType, size, sourceColumn));
        }

        public PgParameter Add(PgParameter value)
        {
            lock (this.parameters.SyncRoot)
            {
                if (value == null)
                {
                    throw new ArgumentException("The value parameter is null.");
                }
                if (value.Parent != null)
                {
                    throw new ArgumentException("The PgParameter specified in the value parameter is already added to this or another FbParameterCollection.");
                }
                if (value.ParameterName == null || value.ParameterName.Length == 0)
                {
                    //value.ParameterName = this.GenerateParameterName();
                }
                else
                {
                    if (this.IndexOf(value) != -1)
                    {
                        throw new ArgumentException("PgParameterCollection already contains PgParameter with ParameterName '" + value.ParameterName + "'.");
                    }
                }

                this.parameters.Add(value);

                return value;
            }
        }

        public override int Add(object value)
        {
            if (!(value is PgParameter))
            {
                throw new InvalidCastException("The parameter passed was not a PgParameter.");
            }

            return this.IndexOf(this.Add(value as PgParameter));
        }

        public override bool Contains(object value)
        {
            return this.parameters.Contains(value);
        }

        public override bool Contains(string parameterName)
        {
            return (-1 != this.IndexOf(parameterName));
        }

        public override int IndexOf(object value)
        {
            return this.parameters.IndexOf((PgParameter)value);
        }

        public override int IndexOf(string parameterName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (((PgParameter)this[i]).ParameterName == parameterName)
                {
                    return i;
                }
            }

            return -1;
        }

        public override void Insert(int index, object value)
        {
            this.parameters.Insert(index, (PgParameter)value);
        }

        public override void Remove(object value)
        {
            if (!(value is PgParameter))
            {
                throw new InvalidCastException("The parameter passed was not a PgParameter.");
            }
            if (!this.Contains(value))
            {
                throw new SystemException("The parameter does not exist in the collection.");
            }

            this.parameters.Remove((PgParameter)value);

            ((PgParameter)value).Parent = null;
        }

        public override void RemoveAt(string parameterName)
        {
            this.RemoveAt(this.IndexOf(parameterName));
        }

        public override void RemoveAt(int index)
        {
            if (index < 0 || index > this.Count)
            {
                throw new IndexOutOfRangeException("The specified index does not exist.");
            }

            this[index].Parent = null;
            this.parameters.RemoveAt(index);
        }
        
        #endregion
    }
}